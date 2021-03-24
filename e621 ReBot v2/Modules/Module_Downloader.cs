using e621_ReBot.Modules;
using e621_ReBot_v2.CustomControls;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Modules
{
    public static class Module_Downloader
    {
        static Module_Downloader()
        {
            timer_Download = new Timer
            {
                Interval = Properties.Settings.Default.DelayDownload
            };
            timer_Download.Tick += DownloadTimer_Tick;
            Download_BGW = new BackgroundWorker();
            Download_BGW.DoWork += DownloadBGW_Start;
            Download_BGW.RunWorkerCompleted += DownloadBGW_Done;
            e6APIDL_BGW = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            e6APIDL_BGW.RunWorkerCompleted += E6APIDL_BGW_Done;
        }

        public static void DownloadEnabler(string WebAdress)
        {
            if (WebAdress.StartsWith("https://e621.net/posts", StringComparison.OrdinalIgnoreCase)
            || WebAdress.StartsWith("https://e621.net/posts/", StringComparison.OrdinalIgnoreCase)
            || WebAdress.StartsWith("https://e621.net/pools/", StringComparison.OrdinalIgnoreCase)
            || WebAdress.StartsWith("https://e621.net/favorites", StringComparison.OrdinalIgnoreCase)
            || WebAdress.Equals("https://e621.net/explore/posts/popular"))
            {
                Form_Loader._FormReference.BB_Download.Visible = true;
            }
        }



        public static Dictionary<string, string> IEDownload_Cache = new Dictionary<string, string>();
        public static void Load_IECache()
        {
            //IEDownload_Cache.Clear();
            Thread ThreadTemp = new Thread(Load_IECache_BGW);
            ThreadTemp.Start();
        }

        private static void Load_IECache_BGW()
        {
            DirectoryInfo CacheFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\IE");
            string[] Search4Extensions = { "*.jpg", "*.png", "*.gif" };
            string FileFoundNameFix;

            foreach (string Search4Extension in Search4Extensions)
            {
                foreach (FileInfo FileFound in CacheFolder.GetFiles(Search4Extension, SearchOption.AllDirectories))
                {
                    FileFoundNameFix = FileFound.Name.Replace("[1].", ".");
                    if (!IEDownload_Cache.ContainsKey(FileFoundNameFix))
                    {
                        IEDownload_Cache.Add(FileFoundNameFix, FileFound.FullName);
                    }
                }
            }
        }



        public static string GetMediasFileNameOnly(string FullNamePath)
        {
            if (FullNamePath.Contains("?token="))
            {
                FullNamePath = FullNamePath.Substring(0, FullNamePath.IndexOf("?token="));
            }
            if (FullNamePath.Contains("sofurryfiles.com"))
            {
                FullNamePath = FullNamePath.Substring(FullNamePath.LastIndexOf("&") + 1);
            }
            if (FullNamePath.Contains("pbs.twimg.com"))
            {
                FullNamePath = FullNamePath.Replace(":orig", "");
            }

            FullNamePath = FullNamePath.Substring(FullNamePath.LastIndexOf("/") + 1);

            return FullNamePath;
        }

        public static string RenameMediaFileName(string FileName, DataRow DataRowPass)
        {
            string NewFileName = (string)DataRowPass["Artist"] + "_";
            string FileFormat = FileName.Substring(FileName.LastIndexOf(".") + 1);

            switch (Properties.Settings.Default.Naming_web)
            {
                case 0: //Original
                    {
                        NewFileName = FileName;
                        break;
                    }

                case 1: //Artist_Title
                    {
                        if (((string)DataRowPass["Grab_Title"]).Contains("Created by")) goto case 2;
                        if (((string)DataRowPass["Grab_Title"]).Contains("Plurk by")) goto case 2;
                        string TitleSubstring = (string)DataRowPass["Grab_Title"];
                        TitleSubstring = TitleSubstring.Substring(0, TitleSubstring.IndexOf(" ⮘ by ")).Substring(2);
                        NewFileName += string.Format("{0}_{1}.{2}", TitleSubstring, FileName.Substring(0, 4), FileFormat);
                        NewFileName = string.Join("", NewFileName.Split(Path.GetInvalidFileNameChars()));
                        break;
                    }

                case 2: //Artist_Original
                    {
                        NewFileName += FileName;
                        NewFileName = string.Join("", NewFileName.Split(Path.GetInvalidFileNameChars()));
                        break;
                    }
            }

            return NewFileName;
        }

        public static bool ReSaveMedia(DataRow DataRowRef)
        {
            Uri DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString) + @"\";
            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, (string)DataRowRef["Artist"]).ToString();
            Directory.CreateDirectory(FolderPath);

            string ImageURL = (string)DataRowRef["Grab_MediaURL"];
            string ImageName = GetMediasFileNameOnly(ImageURL);
            string ImageRename = RenameMediaFileName(ImageName, DataRowRef);
            string FilePath = Path.Combine(FolderPath, ImageRename).ToString();

            if (DataRowRef.ItemArray.Length > 9)
            {
                DataRowRef["DL_FilePath"] = FilePath;
            }
            else
            {
                DataRow DataRow4Grid = DataRowRef["DataRowRef"] != DBNull.Value ? (DataRow)DataRowRef["DataRowRef"] : null;
                if (DataRow4Grid != null && DataRow4Grid.RowState != DataRowState.Detached)
                {
                    ((DataRow)DataRowRef["DataRowRef"])["DL_FilePath"] = FilePath;
                }
            }

            if (!Download_AlreadyDownloaded.Contains(ImageURL)) Download_AlreadyDownloaded.Add(ImageURL);

            if (!File.Exists(FilePath) && IEDownload_Cache.ContainsKey(ImageName))
            {
                File.Copy(IEDownload_Cache[ImageName], FilePath, true);
                return true;
            }
            return false;
        }



        public static void UpdateTreeViewText()
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() => { Form_Loader._FormReference.cCheckGroupBox_Download.Text = "Download Queue" + (Module_TableHolder.Download_Table.Rows.Count > 0 ? string.Format(" ({0})", Module_TableHolder.Download_Table.Rows.Count) : null); }));
        }

        public static int DownloadNodeMax = 0;
        public static int TreeViewPage = 0;
        public static void UpdateTreeViewNodes()
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.cTreeView_DownloadQueue.BeginUpdate();
                Form_Loader._FormReference.cTreeView_DownloadQueue.Nodes.Clear();
                if (Module_TableHolder.Download_Table.Rows.Count > 0)
                {
                    int EndIndex = (TreeViewPage + 1) * DownloadNodeMax;
                    if (EndIndex >= Module_TableHolder.Download_Table.Rows.Count)
                    {
                        TreeViewPage = (int)Math.Floor(Module_TableHolder.Download_Table.Rows.Count / (double)DownloadNodeMax);
                    }

                    for (int i = EndIndex - DownloadNodeMax; i < Math.Min(EndIndex, Module_TableHolder.Download_Table.Rows.Count); i++)
                    {
                        TreeNode TreeNodeTemp = new TreeNode
                        {
                            Text = Module_TableHolder.Download_Table.Rows[i][2].ToString(),
                            ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_Download
                        };
                        Form_Loader._FormReference.cTreeView_DownloadQueue.Nodes.Add(TreeNodeTemp);
                    }
                    Form_Loader._FormReference.bU_DownloadPageUp.Enabled = TreeViewPage != 0;
                    Form_Loader._FormReference.bU_DownloadPageDown.Enabled = EndIndex < Module_TableHolder.Download_Table.Rows.Count;
                }
                else
                {
                    TreeViewPage = 0;
                    Form_Loader._FormReference.bU_DownloadPageUp.Enabled = false;
                    Form_Loader._FormReference.bU_DownloadPageDown.Enabled = false;
                }
                Form_Loader._FormReference.cTreeView_DownloadQueue.EndUpdate();
            }));
        }



        public static List<string> Download_AlreadyDownloaded = new List<string>();
        public static void AddDownloadQueueItem(DataRow DataRowRef, string URL, string Media_URL, string Thumbnail_URL, string Artist = null, string Grab_Title = null, string e6_PostID = null, string e6_PoolName = null, string e6_PoolPostIndex = null)
        {
            DataRow DataRowTemp = Module_TableHolder.Download_Table.NewRow();
            if (DataRowRef != null) DataRowTemp["DataRowRef"] = DataRowRef;
            DataRowTemp["Grab_URL"] = URL;
            DataRowTemp["Grab_MediaURL"] = Media_URL;
            DataRowTemp["Grab_ThumbnailURL"] = Thumbnail_URL;
            if (Artist != null) DataRowTemp["Artist"] = Artist;
            if (Grab_Title != null) DataRowTemp["Grab_Title"] = Grab_Title;
            if (e6_PostID != null) DataRowTemp["e6_PostID"] = e6_PostID;
            if (e6_PoolName != null) DataRowTemp["e6_PoolName"] = e6_PoolName;
            if (e6_PoolPostIndex != null) DataRowTemp["e6_PoolPostIndex"] = e6_PoolPostIndex;
            lock (Module_TableHolder.Download_Table)
            {
                Module_TableHolder.Download_Table.Rows.Add(DataRowTemp);
            }
            UpdateTreeViewText();
        }




        public static void DownloadThumb(ref e6_DownloadItem e6_DownloadItemRef)
        {
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;
            string SiteReferer = "https://" + new Uri((string)DataRowTemp["Grab_URL"]).Host; ;
            using (WebClient ThumbClient = new WebClient())
            {
                ThumbClient.Headers.Add(HttpRequestHeader.Referer, SiteReferer);
                ThumbClient.DownloadDataCompleted += DownloadThumbFinished;
                ThumbClient.DownloadDataAsync(new Uri((string)DataRowTemp["Grab_ThumbnailURL"]), e6_DownloadItemRef);
            }
        }

        public static void DownloadThumbFinished(object sender, DownloadDataCompletedEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.UserState;

            Image DownloadedImage;
            using (var TempStream = new MemoryStream(e.Result))
            {
                DownloadedImage = Image.FromStream(TempStream);
            }
            Bitmap ResizedImage;
            int LargerSize = Math.Max(DownloadedImage.Width, DownloadedImage.Height);

            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;
            DataRow DataRow4Grid = DataRowTemp["DataRowRef"] != DBNull.Value ? (DataRow)DataRowTemp["DataRowRef"] : null;
            if (DataRow4Grid != null && DataRow4Grid.RowState != DataRowState.Detached)
            {
                if (LargerSize > 200)
                {
                    float scale_factor = 200f / LargerSize;
                    ResizedImage = new Bitmap(DownloadedImage, (int)(DownloadedImage.Width * scale_factor), (int)(DownloadedImage.Height * scale_factor)); // , Imaging.PixelFormat.Format32bppArgb)
                }
                else
                {
                    ResizedImage = new Bitmap(DownloadedImage, DownloadedImage.Width, DownloadedImage.Height);
                }
                if (DataRow4Grid["Thumbnail_Image"] == DBNull.Value)
                {
                    string Text2Draw = DownloadedImage.GetFrameCount(new FrameDimension(DownloadedImage.FrameDimensionsList[0])) > 1 ? "Animated" : null;
                    if (Text2Draw == null)
                    {
                        Text2Draw = ((string)DataRow4Grid["Grab_MediaURL"]).Contains("ugoira") ? "ugoira" : null;
                    }
                    if (Text2Draw != null)
                    {
                        Bitmap NewBitmapTemp = new Bitmap(200, 200);
                        using (Graphics gTemp = Graphics.FromImage(NewBitmapTemp))
                        {
                            gTemp.SmoothingMode = SmoothingMode.HighQuality;
                            gTemp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                            Point CenterPoint = new Point(100 - (ResizedImage.Width / 2), 100 - (ResizedImage.Height / 2));
                            gTemp.DrawImage(ResizedImage, CenterPoint);
                            using (Font fontTemp = new Font("Arial Black", 12, FontStyle.Bold, GraphicsUnit.Pixel))
                            {
                                using (GraphicsPath gpTemp = new GraphicsPath())
                                {
                                    using (var sfTemp = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                    {
                                        gpTemp.AddString(Text2Draw, fontTemp.FontFamily, (int)fontTemp.Style, fontTemp.Size, new Rectangle(new Point(60, 20), new Size(80, 20)), sfTemp);
                                    }
                                    using (Pen penTemp = new Pen(Color.Black, 2))
                                    {
                                        gTemp.DrawPath(penTemp, gpTemp);
                                    }
                                    using (SolidBrush brushTemp = new SolidBrush(Color.LightSteelBlue))
                                    {
                                        gTemp.FillPath(brushTemp, gpTemp);
                                    }
                                }
                            }
                        }
                        ResizedImage.Dispose();
                        ResizedImage = NewBitmapTemp;
                    }
                    DataRow4Grid["Thumbnail_Image"] = ResizedImage;

                    e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref DataRow4Grid);
                    if (e6_GridItemTemp != null)
                    {
                        e6_GridItemTemp.LoadImage();
                    }
                }
            }
            else
            {
                if (LargerSize > 158)
                {
                    float scale_factor = 158f / LargerSize;
                    ResizedImage = new Bitmap(DownloadedImage, (int)(DownloadedImage.Width * scale_factor), (int)(DownloadedImage.Height * scale_factor)); // , Imaging.PixelFormat.Format32bppArgb)
                }
                else
                {
                    ResizedImage = new Bitmap(DownloadedImage, DownloadedImage.Width, DownloadedImage.Height);
                }
            }
            e6_DownloadItemRef.picBox_ImageHolder.BackgroundImage = ResizedImage;

            DownloadedImage.Dispose();
            ((WebClient)sender).Dispose();
        }

        public static void DownloadFile(ref e6_DownloadItem e6_DownloadItemRef)
        {
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;
            string SiteReferer = "https://" + new Uri((string)DataRowTemp["Grab_URL"]).Host;
            e6_DownloadItemRef.SetTooltip((string)DataRowTemp["Grab_MediaURL"]);
            using (WebClient FileClient = new WebClient())
            {
                FileClient.Headers.Add(HttpRequestHeader.Referer, SiteReferer);
                FileClient.DownloadProgressChanged += Download_ProgressReport;
                FileClient.DownloadFileCompleted += DownloadFileFinished;
                FileClient.DownloadFileAsync(new Uri((string)DataRowTemp["Grab_MediaURL"]), e6_DownloadItemRef.DL_FolderIcon.Tag.ToString(), e6_DownloadItemRef);
            }
        }

        private static void Download_ProgressReport(object sender, DownloadProgressChangedEventArgs e)
        {
            ((e6_DownloadItem)e.UserState).DL_ProgressBar.Value = e.ProgressPercentage;
        }

        public static void DownloadFileFinished(object sender, AsyncCompletedEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.UserState;
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;

            lock (Download_AlreadyDownloaded)
            {
                Download_AlreadyDownloaded.Add((string)DataRowTemp["Grab_MediaURL"]);
            }
            Image ImageHolder = e6_DownloadItemRef.picBox_ImageHolder.Tag == null ? e6_DownloadItemRef.picBox_ImageHolder.BackgroundImage : null;
            AddPic2FLP((string)DataRowTemp["Grab_MediaURL"], e6_DownloadItemRef.DL_FolderIcon.Tag.ToString(), ImageHolder);

            ((e6_DownloadItem)e.UserState).Dispose();
            ((WebClient)sender).Dispose();

            if (Download_AlreadyDownloaded.Count % 1000 == 0)
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }
        }

        public static void AddPic2FLP(string ThumbURL, string FilePath, Image e6Pic = null)
        {
            UIDrawController.SuspendDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
            Form_Loader._FormReference.DownloadFLP_Downloaded.SuspendLayout();

            e6_DownloadItem e6_DownloadItemTemp;
            if (Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.Count < Form_Loader._DLHistoryMaxControls)
            {
                e6_DownloadItemTemp = new e6_DownloadItem();
                e6_DownloadItemTemp.DL_FolderIcon.Visible = true;
                e6_DownloadItemTemp.DL_ProgressBar.Dispose();
                Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.Add(e6_DownloadItemTemp);
                Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.SetChildIndex(e6_DownloadItemTemp, 0);
            }
            else
            {
                PictureBox LastPicBoxTemp = ((e6_DownloadItem)Form_Loader._FormReference.DownloadFLP_Downloaded.Controls[Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.Count - 1]).picBox_ImageHolder;
                LastPicBoxTemp.BackgroundImage.Dispose();

                for (int ControlIndex = Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.Count - 2; ControlIndex >= 0; ControlIndex--)
                {
                    e6_DownloadItem CurrentControl = (e6_DownloadItem)Form_Loader._FormReference.DownloadFLP_Downloaded.Controls[ControlIndex];
                    e6_DownloadItem PreviousControl = (e6_DownloadItem)Form_Loader._FormReference.DownloadFLP_Downloaded.Controls[ControlIndex + 1];
                    PreviousControl.DL_FolderIcon.Tag = CurrentControl.DL_FolderIcon.Tag;
                    PreviousControl.picBox_ImageHolder.BackgroundImage = CurrentControl.picBox_ImageHolder.BackgroundImage;
                }
                e6_DownloadItemTemp = (e6_DownloadItem)Form_Loader._FormReference.DownloadFLP_Downloaded.Controls[0];
            }
            e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;
            if (e6Pic == null)
            {
                if (FilePath.EndsWith("webm") || FilePath.EndsWith("swf"))
                {
                    e6_DownloadItemTemp.picBox_ImageHolder.LoadAsync(ThumbURL);
                }
                else
                {
                    Image DLedPic = Image.FromFile(FilePath);
                    int LargerSize = Math.Max(DLedPic.Width, DLedPic.Height);
                    if (LargerSize > 158)
                    {
                        double scale_factor = 158d / LargerSize;
                        Bitmap ResizedImage = new Bitmap(DLedPic, (int)(DLedPic.Width * scale_factor), (int)(DLedPic.Height * scale_factor)); // , Imaging.PixelFormat.Format32bppArgb)
                        e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = ResizedImage;
                        DLedPic.Dispose();
                    }
                    else
                    {
                        e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = DLedPic;
                    }
                }
            }
            else
            {
                e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = e6Pic;
            }

            if (Module_TableHolder.Download_Table.Rows.Count == 0)
            {
                GC.Collect();
            }

            Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
            UIDrawController.ResumeDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
        }



        public static Timer timer_Download;
        private static void DownloadTimer_Tick(object sender, EventArgs e)
        {
            timer_Download.Stop();
            if (Form_Loader._FormReference.cCheckGroupBox_Download.Checked && Module_TableHolder.Download_Table.Rows.Count > 0 && !Download_BGW.IsBusy)
            {
                Download_BGW.RunWorkerAsync();
            }
        }

        public static BackgroundWorker Download_BGW;
        private static void DownloadBGW_Start(object sender, DoWorkEventArgs e)
        {
            Download_Start();
        }

        private static void DownloadBGW_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            timer_Download.Start();
        }



        public static int DLThreadsCount = 4;
        public static void Download_Start()
        {
            while (Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Count < DLThreadsCount && Form_Loader._FormReference.cCheckGroupBox_Download.Checked && Module_TableHolder.Download_Table.Rows.Count > 0)
            {
                DataRow DataRowTemp = Module_TableHolder.Download_Table.NewRow();
                DataRowTemp.ItemArray = (object[])Module_TableHolder.Download_Table.Rows[0].ItemArray.Clone();
                if (DataRowTemp["DataRowRef"] != DBNull.Value)
                {
                    DownloadFrom_URL(DataRowTemp);
                }
                else // e6 download
                {
                    DownloadFrom_e6URL(DataRowTemp);
                }
                lock (Module_TableHolder.Download_Table)
                {
                    Module_TableHolder.Download_Table.Rows.RemoveAt(0);
                }
            }
            UpdateTreeViewText();
            UpdateTreeViewNodes();
        }

        private static void DownloadFrom_URL(DataRow DataRowRef)
        {
            Uri DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString) + @"\";
            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, (string)DataRowRef["Artist"]).ToString();
            Directory.CreateDirectory(FolderPath);

            string GetFileNameOnly = GetMediasFileNameOnly((string)DataRowRef["Grab_MediaURL"]);
            string ImageRename;

            if (GetFileNameOnly.Contains("ugoira"))
            {
                string WebMName = GetFileNameOnly;
                WebMName = WebMName.Substring(0, WebMName.IndexOf("_ugoira0")) + "_ugoira1920x1080.webm";
                ImageRename = RenameMediaFileName(WebMName, DataRowRef);

                string FilePath = Path.Combine(FolderPath, ImageRename).ToString();
                if (File.Exists(FilePath))
                {
                    Form_Loader._FormReference.BeginInvoke(new Action(() => { AddPic2FLP((string)DataRowRef["Grab_ThumbnailURL"], FilePath); }));
                }
                else
                {
                    Form_Loader._FormReference.Invoke(new Action(() =>
                    {
                        e6_DownloadItem e6_DownloadItemTemp = new e6_DownloadItem()
                        {
                            Tag = DataRowRef,
                        };
                        e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                        e6_DownloadItemTemp.DL_ProgressBar.BarColor = Color.Orange;
                        e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;
                        DownloadThumb(ref e6_DownloadItemTemp);
                        Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);
                        Module_FFmpeg.DownloadQueue_ConvertUgoira2WebM(ref e6_DownloadItemTemp);
                    }));
                }
            }
            else
            {
                ImageRename = RenameMediaFileName(GetFileNameOnly, DataRowRef);
                string FilePath = Path.Combine(FolderPath, ImageRename).ToString();
                if (IEDownload_Cache.Keys.Contains(GetFileNameOnly))
                {
                    if (ReSaveMedia(DataRowRef))
                    {
                        //Form_Loader._FormReference.BeginInvoke(new Action(() => { AddPic2FLP((string)DataRowRef["Grab_ThumbnailURL"], FilePath); }));
                    }
                }
                else
                {
                    Form_Loader._FormReference.Invoke(new Action(() =>
                    {
                        e6_DownloadItem e6_DownloadItemTemp = new e6_DownloadItem()
                        {
                            Tag = DataRowRef,
                        };
                        e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                        e6_DownloadItemTemp.DL_ProgressBar.BarColor = Color.Orange;
                        e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;

                        DataRow DataRow4Grid = DataRowRef["DataRowRef"] != DBNull.Value ? (DataRow)DataRowRef["DataRowRef"] : null;
                        if (DataRow4Grid != null && DataRow4Grid.RowState != DataRowState.Detached)
                        {
                            if (DataRow4Grid["Thumbnail_Image"] == DBNull.Value)
                            {
                                DataRow4Grid["Thumbnail_DLStart"] = true;
                                DownloadThumb(ref e6_DownloadItemTemp);
                            }
                            else
                            {
                                e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = (Image)((Image)DataRow4Grid["Thumbnail_Image"]).Clone();
                            }
                        }
                        else
                        {
                            DownloadThumb(ref e6_DownloadItemTemp);
                        }
                        Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);
                        DownloadFile(ref e6_DownloadItemTemp);
                    }));
                }
            }
        }

        private static void DownloadFrom_e6URL(DataRow DataRowRef)
        {
            string PicURL = (string)DataRowRef["Grab_MediaURL"];

            string GetFileNameOnly = GetMediasFileNameOnly(PicURL);
            if (DownloadFolderCache != null && DownloadFolderCache.Contains(GetFileNameOnly)) return;

            string ThumbLink = PicURL.Replace("net/data/", "net/data/preview/");
            ThumbLink = ThumbLink.Remove(ThumbLink.LastIndexOf(".") + 1) + "jpg";

            string DLPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, @"e621\").ToString();
            string PoolName = DataRowRef["e6_PoolName"] != DBNull.Value ? (string)DataRowRef["e6_PoolName"] : null;
            if (PoolName != null)
            {
                DLPath += PoolName + @"\";
            }
            Directory.CreateDirectory(DLPath);

            string PoolPostIndex = DataRowRef["e6_PoolPostIndex"] != DBNull.Value ? (string)DataRowRef["e6_PoolPostIndex"] : null;
            string PostID = (string)DataRowRef["e6_PostID"];

            switch (Properties.Settings.Default.Naming_e6)
            {
                case 1:
                    {
                        if (PoolName != null)
                        {
                            GetFileNameOnly = string.Format("{0}_{1}", PostID, GetFileNameOnly);
                        }
                        break;
                    }

                case 2:
                    {
                        GetFileNameOnly = string.Format("{0}_{1}", PostID, GetFileNameOnly);
                        break;
                    }
            }
            if (PoolPostIndex != null)
            {
                GetFileNameOnly = string.Format("{0}_{1}", PoolPostIndex, GetFileNameOnly);
            }
            string FilePath = Path.Combine(DLPath, GetFileNameOnly).ToString();

            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                e6_DownloadItem e6_DownloadItemTemp = new e6_DownloadItem()
                {
                    Tag = DataRowRef,
                };
                e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                e6_DownloadItemTemp.DL_ProgressBar.BarColor = Color.Orange;
                e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;
                if (PicURL.EndsWith(".swf"))
                {
                    e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = Properties.Resources.E6Image_Flash;
                }
                else
                {
                    e6_DownloadItemTemp.picBox_ImageHolder.Tag = true;
                    e6_DownloadItemTemp.picBox_ImageHolder.LoadAsync(ThumbLink);
                }
                DownloadFile(ref e6_DownloadItemTemp);
                Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);
            }));
        }

        public static void Grab_e621()
        {
            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(Module_CefSharp.GetHTMLSource());
            string BrowserAdress = Module_CefSharp.CefSharpBrowser.Address;

            if (BrowserAdress.StartsWith("https://e621.net/posts/", StringComparison.OrdinalIgnoreCase))  // single
            {
                string PicURL = WebDoc.DocumentNode.SelectSingleNode("//div[@id='image-download-link']/a").Attributes["href"].Value;
                if (Module_TableHolder.DownloadQueueContainsURL(PicURL) || Download_AlreadyDownloaded.Contains(PicURL))
                {
                    return;
                }

                string PostID = BrowserAdress;
                if (PostID.Contains("?"))
                {
                    PostID = PostID.Substring(0, PostID.IndexOf("?"));
                }
                PostID = PostID.Substring(PostID.LastIndexOf("/") + 1);
                AddDownloadQueueItem(null, BrowserAdress, PicURL, null, null, null, PostID, null, null);
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/posts", StringComparison.OrdinalIgnoreCase))  // multi
            {
                string inputtext = Custom_InputBox.Show(Form_Loader._FormReference, "e621 ReBot", "If you want to download media to a separate folder, enter a folder name below.", Form_Loader._FormReference.BQB_Start.PointToScreen(Point.Empty), "");
                if (!inputtext.Equals("✄") && !inputtext.Equals(""))
                {
                    inputtext = string.Join("", inputtext.Split(Path.GetInvalidFileNameChars()));
                }
                else
                {
                    inputtext = null;
                }

                if (BrowserAdress.StartsWith("https://e621.net/posts?tags=", StringComparison.OrdinalIgnoreCase))
                {
                    if (Properties.Settings.Default.API_Key.Equals("") || WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                    {
                        goto GrabPageOnly_Tags;
                    }

                    if (MessageBox.Show("Do you want to download all images with current tags?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        if (e6APIDL_BGW.IsBusy)
                        {
                            NotifyWhenDone = true;
                            MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        e6APIDL_BGW.DoWork += GrabAllImagesWithGivenTags;
                        string TagQuery = BrowserAdress;
                        TagQuery = TagQuery.Substring(TagQuery.IndexOf("tags=") + 5);
                        // TagQuery = WebUtility.UrlDecode(TagQuery).Replace(" ", "+")
                        e6APIDL_BGW.RunWorkerAsync(new string[] { TagQuery, inputtext });
                        return;
                    }
                    else
                    {
                        goto GrabPageOnly_Tags;
                    }
                }
            GrabPageOnly_Tags:
                HtmlNodeCollection NodeSelector = WebDoc.DocumentNode.SelectNodes("//div[@id='posts-container']/article");
                if (NodeSelector != null)
                {
                    foreach (HtmlNode Post in NodeSelector)
                    {
                        if (!Post.Attributes["class"].Value.Contains("blacklisted-active"))
                        {
                            string PicURL = Post.Attributes["data-file-url"].Value;
                            if (Module_TableHolder.DownloadQueueContainsURL(PicURL) || Download_AlreadyDownloaded.Contains(PicURL))
                            {
                                continue;
                            }
                            AddDownloadQueueItem(null, BrowserAdress, PicURL, null, null, null, Post.Attributes["data-id"].Value, inputtext, null);
                        }
                    }
                }
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/pools/", StringComparison.OrdinalIgnoreCase))
            {
                HtmlNode BottomMenuHolder = WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu");
                if (Properties.Settings.Default.API_Key.Equals("") || BottomMenuHolder.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                {
                    goto GrabPageOnly_Pools;
                }

                if (MessageBox.Show("Do you want to download the whole pool?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (e6APIDL_BGW.IsBusy)
                    {
                        NotifyWhenDone = true;
                        MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    e6APIDL_BGW.DoWork += GraBPoolInBG;
                    string ComicID = BrowserAdress.Replace("https://e621.net/pools/", "");
                    e6APIDL_BGW.RunWorkerAsync(ComicID);
                    return;
                }
            GrabPageOnly_Pools:
                HtmlNodeCollection NodeSelector = WebDoc.DocumentNode.SelectNodes("//div[@id='a-show']//article");
                if (NodeSelector != null)
                {
                    int GetCurrentPage = int.Parse(BottomMenuHolder.SelectSingleNode(".//li[@class='current-page']").InnerText);

                    var ComicPages = new List<string>();
                    if (GetCurrentPage > 1)
                    {
                        string PoolID = WebDoc.DocumentNode.SelectSingleNode("//div[@id='a-show']//a").Attributes["href"].Value.Replace("/posts?tags=pool%3A", "");
                        ComicPages = JObject.Parse(Module_e621Info.e621InfoDownload(string.Format("https://e621.net/pools/{0}.json", PoolID), false))["post_ids"].Values<string>().ToList(); ;
                    }

                    int PoolIndex = 0;
                    foreach (var Post in NodeSelector)
                    {
                        string PicURL = Post.Attributes["data-file-url"].Value;
                        //if (Form_Loader._FormReference.cTreeView_DownloadQueue.Nodes.ContainsKey(PicURL) || Download_AlreadyDownloaded.Contains(PicURL))
                        //{
                        //    continue;
                        //}
                        string PostID = Post.Attributes["data-id"].Value;
                        string PoolName = WebDoc.DocumentNode.SelectSingleNode("//div[@id='a-show']//a").InnerText;
                        PoolName = string.Join("", PoolName.Split(Path.GetInvalidFileNameChars()));
                        string PoolPostIndex = GetCurrentPage > 1 ? ComicPages.IndexOf(PostID).ToString() : PoolIndex.ToString();
                        AddDownloadQueueItem(null, BrowserAdress, PicURL, null, null, null, PostID, PoolName, PoolPostIndex);
                        PoolIndex += 1;
                    }
                }
                return;
            }

            if (BrowserAdress.Equals("https://e621.net/explore/posts/popular"))
            {
                HtmlNodeCollection NodeSelector = WebDoc.DocumentNode.SelectNodes("//div[@id='posts-container']/article");
                if (NodeSelector != null)
                {
                    foreach (var Post in NodeSelector)
                    {
                        if (!Post.Attributes["class"].Value.Contains("blacklisted-active"))
                        {
                            string PicURL = Post.Attributes["data-file-url"].Value;
                            if (Module_TableHolder.DownloadQueueContainsURL(PicURL) || Download_AlreadyDownloaded.Contains(PicURL))
                            {
                                continue;
                            }
                            AddDownloadQueueItem(null, BrowserAdress, PicURL, null, null, null, Post.Attributes["data-id"].Value, null, null);
                        }
                    }
                }
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/favorites", StringComparison.OrdinalIgnoreCase))
            {
                string inputtext = Custom_InputBox.Show(Form_Loader._FormReference, "e621 ReBot", "If you want to download media to a separate folder, enter a folder name below.", Form_Loader._FormReference.BQB_Start.PointToScreen(Point.Empty), "");
                if (!inputtext.Equals("✄") && !inputtext.Equals(""))
                {
                    inputtext = string.Join("", inputtext.Split(Path.GetInvalidFileNameChars()));
                }
                else
                {
                    inputtext = null;
                }

                if (Properties.Settings.Default.API_Key.Equals("") || WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                {
                    goto GrabPageOnly_Favorites;
                }

                if (MessageBox.Show("Do you want to download all favorites?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (e6APIDL_BGW.IsBusy)
                    {
                        MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    e6APIDL_BGW.DoWork += GrabAllImagesWithGivenTags;
                    string TagQuery = BrowserAdress;
                    if (TagQuery.Contains("user_id"))
                    {
                        TagQuery = WebDoc.DocumentNode.SelectSingleNode("//input[@id='tags']").Attributes["value"].Value;
                    }
                    else
                    {
                        TagQuery = TagQuery.Substring(TagQuery.IndexOf("?") + 1);
                    }
                    e6APIDL_BGW.RunWorkerAsync(new string[] { TagQuery, inputtext });
                    return;
                }
            GrabPageOnly_Favorites:

                HtmlNodeCollection NodeSelector = WebDoc.DocumentNode.SelectNodes("//div[@id='posts']/article");
                if (NodeSelector != null)
                {
                    foreach (var Post in NodeSelector)
                    {
                        if (!Post.Attributes["class"].Value.Contains("blacklisted-active"))
                        {
                            string PicURL = Post.Attributes["data-file-url"].Value;
                            if (Module_TableHolder.DownloadQueueContainsURL(PicURL) || Download_AlreadyDownloaded.Contains(PicURL))
                            {
                                continue;
                            }
                            AddDownloadQueueItem(null, BrowserAdress, PicURL, null, null, null, Post.Attributes["data-id"].Value, null, null);
                        }
                    }
                }
                return;
            }
        }

        public static BackgroundWorker e6APIDL_BGW;
        private static bool NotifyWhenDone = false;
        private static void E6APIDL_BGW_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            e6APIDL_BGW.DoWork -= GraBPoolInBG;
            e6APIDL_BGW.DoWork -= GrabAllImagesWithGivenTags;
            Report_Status("Suspended.", false);
            if (NotifyWhenDone)
            {
                NotifyWhenDone = false;
                MessageBox.Show("Grabbing posts from e621 API is done, you can now queue more.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void Report_Status(string StatusMessage, bool ButtonEnable)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.label_DownloadStatus.Text = string.Format("API DL Status: {0}", StatusMessage);
                Form_Loader._FormReference.bU_CancelAPIDL.Enabled = ButtonEnable;
            }));
        }



        public static List<string> DownloadFolderCache;
        public static void Load_DownloadFolderCache()
        {
            DownloadFolderCache = new List<string>();
            Thread ThreadTemp = new Thread(Load_DownloadFolderCache_BGW);
            ThreadTemp.Start();
        }

        private static void Load_DownloadFolderCache_BGW()
        {
            DirectoryInfo CacheFolder = new DirectoryInfo(Properties.Settings.Default.DownloadsFolderLocation + "\\e621");

            foreach (FileInfo FileFound in CacheFolder.GetFiles("*", SearchOption.AllDirectories))
            {
                string GetFileMD5 = FileFound.Name;
                if (GetFileMD5.Contains("_")) GetFileMD5 = GetFileMD5.Substring(GetFileMD5.LastIndexOf("_") + 1);
                //GetFileMD5 = GetFileMD5.Substring(0, GetFileMD5.LastIndexOf("."));

                if (!DownloadFolderCache.Contains(GetFileMD5)) DownloadFolderCache.Add(GetFileMD5);
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() => 
            { 
                Form_Loader._FormReference.bU_SkipDLCache.Enabled = true;
                MessageBox.Show(string.Format("Cached {0} files.", DownloadFolderCache.Count), "e621 ReBot");
            }));
        }


        public static void GrabAllImagesWithGivenTags(object sender, DoWorkEventArgs e)
        {
            string[] ArgumentPass = (string[])e.Argument;

            string TagQuery = ArgumentPass[0];
            string PostRequestString = "https://e621.net/posts.json?limit=320&tags=" + TagQuery;
            string FolderName = ArgumentPass[1];

            int PageCounter = 1;
        GrabAnotherPage:
            Report_Status(string.Format("Working on Tags page {0}.", PageCounter), true);
            string TempRequestStringHolder = PostRequestString + (PageCounter > 1 ? "&page=" + PageCounter : null);
            JToken JSON_Object = JObject.Parse(Module_e621Info.e621InfoDownload(TempRequestStringHolder, true))["posts"];
            foreach (JObject cPost in JSON_Object.Children())
            {
                if (!Blacklist_Check(cPost["tags"]))
                {
                    string PostID = cPost["id"].Value<string>();
                    AddDownloadQueueItem(null, "https://e621.net/posts/" + PostID, cPost["file"]["url"].Value<string>(), null, null, null, PostID, FolderName, null);
                }
            }
            PageCounter += 1;

            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                UpdateTreeViewText();
                UpdateTreeViewNodes();
                timer_Download.Start();
            }));

            if (e6APIDL_BGW.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            Thread.Sleep(500);
            if (JSON_Object.Children().Count() == 320)
            {
                goto GrabAnotherPage;
            }
            JSON_Object = null;
        }

        public static void GraBPoolInBG(object sender, DoWorkEventArgs e)
        {
            string PoolID = (string)e.Argument;

            JToken PoolJSON = JObject.Parse(Module_e621Info.e621InfoDownload(string.Format("https://e621.net/pools/{0}.json", PoolID), false));
            string PoolName = PoolJSON["name"].Value<string>().Replace("_", " ");
            PoolName = string.Join("", PoolName.Split(Path.GetInvalidFileNameChars()));
            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, @"e621\", PoolName).ToString();

            List<string> FoundComicPages = new List<string>();
            if (Directory.Exists(FolderPath))
            {
                foreach (string FileFound in Directory.GetFiles(FolderPath))
                {
                    string CutPageName = FileFound.Substring(FileFound.LastIndexOf("_") + 1);
                    FoundComicPages.Add(CutPageName);
                }
            }

            List<string> ComicPages = PoolJSON["post_ids"].Values<string>().ToList();
            int PageCount = (int)Math.Ceiling(PoolJSON["post_ids"].Count() / 320d);
            string PoolRequestString = "https://e621.net/posts.json?limit=320&tags=pool:" + PoolID;
            string e6JSONResult;
            int SkippedPagesCounter = 0;
            for (int p = 1; p <= PageCount; p++)
            {
                Report_Status(string.Format("Working on Pool page {0}.", p), true);
                string TempRequestStringHolder = PoolRequestString + (p > 1 ? "&page=" + p : null);
                e6JSONResult = Module_e621Info.e621InfoDownload(TempRequestStringHolder, true);

                JToken JSON_Object = JObject.Parse(e6JSONResult)["posts"];
                foreach (JObject Post in JSON_Object)
                {
                    string PicURL = Post["file"]["url"].Value<string>();
                    string PicName = PicURL.Substring(PicURL.LastIndexOf("/") + 1);
                    if (FoundComicPages.Contains(PicName))
                    {
                        SkippedPagesCounter += 1;
                        continue;
                    }

                    string PostID = Post["id"].Value<string>();
                    AddDownloadQueueItem(null, "https://e621.net/posts/" + PostID, PicURL, null, null, null, PostID, PoolName, ComicPages.IndexOf(PostID).ToString());
                }
                Form_Loader._FormReference.BeginInvoke(new Action(() =>
                {
                    UpdateTreeViewText();
                    UpdateTreeViewNodes();
                    timer_Download.Start();
                }));

                if (e6APIDL_BGW.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                Thread.Sleep(500);
                JSON_Object = null;
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() => { if (SkippedPagesCounter > 0) Form_Loader._FormReference.textBox_Info.Text = string.Format("{0} Downloader >>> {1}: {2} page{3} skipped as they already exist\n{4}", DateTime.Now.ToLongTimeString(), PoolName, SkippedPagesCounter, SkippedPagesCounter > 1 ? "s" : null, Form_Loader._FormReference.textBox_Info.Text); }));
        }

        private static bool Blacklist_Check(JToken PostTags)
        {
            if (Form_Loader._FormReference.Blacklist.Count > 0)
            {
                string TagsString = "";
                foreach (JProperty TagCategory in PostTags.Children())
                {
                    TagsString += string.Join(" ", TagCategory.First.ToObject<string[]>()).Trim() + " ";
                }

                foreach (string BlacklistLine in Form_Loader._FormReference.Blacklist)
                {
                    if (BlacklistLine.Contains("-"))
                    {
                        List<string> TagStringList = new List<string>();
                        TagStringList.AddRange(TagsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        List<string> BlacklistLineList = new List<string>();
                        BlacklistLineList.AddRange(BlacklistLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        int HitCounter = 0;
                        foreach (string BlacklistTag in BlacklistLineList)
                        {
                            string BlacklistTagTemp = BlacklistTag;
                            if (BlacklistTag.StartsWith("-"))
                            {
                                BlacklistTagTemp = BlacklistTag.Substring(1);
                                if (!TagStringList.Contains(BlacklistTagTemp))
                                {
                                    HitCounter += 1;
                                }
                                continue;
                            }

                            if (TagStringList.Contains(BlacklistTagTemp))
                            {
                                HitCounter += 1;
                            }
                        }

                        if (HitCounter == BlacklistLineList.Count)
                        {
                            return true;
                        }
                        continue;
                    }

                    if (BlacklistLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).All(f => TagsString.Contains(f)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }

    //public class HWndCounter
    //{
    //    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    //    private static extern IntPtr GetCurrentProcess();

    //    [System.Runtime.InteropServices.DllImport("user32.dll")]
    //    private static extern uint GetGuiResources(IntPtr hProcess, uint uiFlags);

    //    private enum ResourceType
    //    {
    //        Gdi = 0,
    //        User = 1
    //    }

    //    public static int GetWindowHandlesForCurrentProcess()
    //    {
    //        IntPtr processHandle = GetCurrentProcess();
    //        uint gdiObjects = GetGuiResources(processHandle, (uint)ResourceType.Gdi);
    //        uint userObjects = GetGuiResources(processHandle, (uint)ResourceType.User);

    //        return Convert.ToInt32(gdiObjects + userObjects);
    //    }
    //}
}
