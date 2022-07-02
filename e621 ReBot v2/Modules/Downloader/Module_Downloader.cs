using e621_ReBot.Modules;
using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules.Grabber;
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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
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
            timer_DownloadRemovalThreading = new Timer
            {
                Interval = 250
            };
            timer_DownloadRemovalThreading.Tick += DownloadRemovalThreading_Tick;
            for (int i = 0; i < 4; i++)
            {
                Custom_WebClient ThumbClient = new Custom_WebClient();
                ThumbClient.DownloadDataCompleted += DownloadThumbFinished;
                Holder_ThumbClient.Add(ThumbClient);
                Custom_WebClient FileClient = new Custom_WebClient();
                FileClient.DownloadProgressChanged += Download_ProgressReport;
                FileClient.DownloadFileCompleted += DownloadFileFinished;
                Holder_FileClient.Add(FileClient);
            }
            timer_CacheProgressTimer = new Timer
            {
                Interval = 1000
            };
            timer_CacheProgressTimer.Tick += CacheProgressTimer_Tick;

            _DownloadEnabler.Add(new Regex(@".+(e621.net/posts)(/\d+|\?.+)?"));
            _DownloadEnabler.Add(new Regex(@".+(e621.net/pools/\d+)"));
            _DownloadEnabler.Add(new Regex(@".+(e621.net/favorites)"));
            _DownloadEnabler.Add(new Regex(@".+(e621.net/explore/posts/popular)"));
        }

        private static readonly List<Regex> _DownloadEnabler = new List<Regex>();
        public static void DownloadEnabler(string WebAdress)
        {
            foreach (Regex URLTest in _DownloadEnabler)
            {
                Match MatchTemp = URLTest.Match(WebAdress);
                if (MatchTemp.Success)
                {
                    Form_Loader._FormReference.BB_Download.Visible = true;
                    return;
                }
            }
        }

        public static Dictionary<string, string> MediaBrowser_MediaCache = new Dictionary<string, string>();

        public static List<string> Download_AlreadyDownloaded = new List<string>();



        // - - - - - - - - - - -



        public static string GetMediasFileNameOnly(string FullNamePath, in DataRow DataRowPass = null)
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
            if (FullNamePath.EndsWith("/download", StringComparison.OrdinalIgnoreCase))
            {
                FullNamePath = $"{FullNamePath.Substring(0, FullNamePath.LastIndexOf("/download"))}.";
                if (DataRowPass != null) FullNamePath += DataRowPass["Info_MediaFormat"];
            }

            FullNamePath = HttpUtility.UrlDecode(FullNamePath).Substring(FullNamePath.LastIndexOf("/") + 1);

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
                        if (((string)DataRowPass["Grab_Title"]).Contains("Created by"))
                        {
                            goto case 2;
                        }
                        if (((string)DataRowPass["Grab_Title"]).Contains("Plurk by"))
                        {
                            goto case 2;
                        }
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

        public static bool ReSaveMedia(ref DataRow DataRowRef)
        {
            var DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)DataRowRef["Artist"]).Replace("/", "-"));
            Directory.CreateDirectory(FullFolderPath);

            string MediaURL = (string)DataRowRef["Grab_MediaURL"];
            string MediaName = GetMediasFileNameOnly(MediaURL);
            string MediaRename = RenameMediaFileName(MediaName, DataRowRef);
            string FullFilePath = Path.Combine(FullFolderPath, MediaRename);

            if (DataRowRef.ItemArray.Length > 9)
            {
                DataRowRef["DL_FilePath"] = FullFilePath;
            }
            else
            {
                DataRow DataRow4Grid = DataRowRef["DataRowRef"] != DBNull.Value ? (DataRow)DataRowRef["DataRowRef"] : null;
                if (DataRow4Grid != null && DataRow4Grid.RowState != DataRowState.Detached)
                {
                    ((DataRow)DataRowRef["DataRowRef"])["DL_FilePath"] = FullFilePath;
                }
            }

            if (!Download_AlreadyDownloaded.Contains(MediaURL))
            {
                lock (Download_AlreadyDownloaded)
                {
                    Download_AlreadyDownloaded.Add(MediaURL);
                }
            }

            if (!File.Exists(FullFilePath) && MediaBrowser_MediaCache.ContainsKey(MediaName))
            {
                File.Copy(MediaBrowser_MediaCache[MediaName], FullFilePath, true);
                return true;
            }

            return false;
        }



        // - - - - - - - - - - -



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



        // - - - - - - - - - - -



        public static void AddDownloadQueueItem(DataRow DataRowRef, string URL, string Media_URL, string Thumbnail_URL = null, string MediaFormat = null, string Artist = null, string Grab_Title = null, string e6_PostID = null, string e6_PoolName = null, string e6_PoolPostIndex = null)
        {
            DataRow DataRowTemp = Module_TableHolder.Download_Table.NewRow();
            if (DataRowRef != null) DataRowTemp["DataRowRef"] = DataRowRef;
            DataRowTemp["Grab_URL"] = URL;
            DataRowTemp["Grab_MediaURL"] = Media_URL;
            if (Thumbnail_URL != null) DataRowTemp["Grab_ThumbnailURL"] = Thumbnail_URL;
            if (MediaFormat != null) DataRowTemp["Info_MediaFormat"] = MediaFormat;
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



        // - - - - - - - - - - -



        private static readonly List<Custom_WebClient> Holder_ThumbClient = new List<Custom_WebClient>();
        private static readonly List<Custom_WebClient> Holder_FileClient = new List<Custom_WebClient>();

        private static void StartDLClient(ref e6_DownloadItem e6_DownloadItemRef, string DLType)
        {
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;
            string SiteReferer = $"https://{new Uri((string)DataRowTemp["Grab_URL"]).Host}";

            Custom_WebClient WebClientSelected = null;
            foreach (Custom_WebClient WebClientTemp in (DLType.Equals("Thumb") ? Holder_ThumbClient : Holder_FileClient))
            {
                if (!WebClientTemp.IsBusy)
                {
                    WebClientSelected = WebClientTemp;
                    break;
                }
            }
            WebClientSelected.Headers.Add(HttpRequestHeader.Referer, SiteReferer);
            switch (SiteReferer)
            {
                case "https://e621.net":
                    {
                        WebClientSelected.Headers.Add(HttpRequestHeader.UserAgent, Properties.Settings.Default.AppName);
                        break;
                    }

                case "https://www.hiccears.com":
                    {

                        WebClientSelected.Headers.Add(HttpRequestHeader.Cookie, Module_CookieJar.GetHicceArsCookie());
                        break;
                    }

            }
            if (DLType.Equals("Thumb"))
            {
                WebClientSelected.DownloadDataAsync(new Uri((string)DataRowTemp["Grab_ThumbnailURL"]), e6_DownloadItemRef);
            }
            else
            {
                e6_DownloadItemRef.SetTooltip((string)DataRowTemp["Grab_MediaURL"]);
                WebClientSelected.DownloadFileAsync(new Uri((string)DataRowTemp["Grab_MediaURL"]), e6_DownloadItemRef.DL_FolderIcon.Tag.ToString(), e6_DownloadItemRef);
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
                    string Text2Draw = null;
                    if (ImageFormat.Gif.Equals(DownloadedImage.RawFormat))
                    {
                        Text2Draw = DownloadedImage.GetFrameCount(new FrameDimension(DownloadedImage.FrameDimensionsList[0])) > 1 ? "Animated" : null;
                    }
                    if (Text2Draw == null)
                    {
                        Text2Draw = ((string)DataRow4Grid["Grab_MediaURL"]).Contains("ugoira") ? "ugoira" : null;
                    }
                    else
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
        }

        private static void Download_ProgressReport(object sender, DownloadProgressChangedEventArgs e)
        {
            ((e6_DownloadItem)e.UserState).DL_ProgressBar.Value = e.ProgressPercentage;
        }

        public static void DownloadFileFinished(object sender, AsyncCompletedEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.UserState;
            e6_DownloadItemRef._DownloadFinished = true;
            e6_DownloadItemRef.DL_ProgressBar.Visible = false;

            if (e.Cancelled) //timeout detected, cancelled it;
            {
                Form_Loader._FormReference.cCheckGroupBox_Download.Checked = false;
                MessageBox.Show("Timeout has been detected, further downloads have been paused!", "e621 ReBot Downloader", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                string PicURL = (string)((DataRow)e6_DownloadItemRef.Tag)["Grab_MediaURL"];
                if (!Module_TableHolder.DownloadQueueContainsURL(PicURL) && !Download_AlreadyDownloaded.Contains(PicURL))
                {
                    lock (Module_TableHolder.Download_Table)
                    {
                        Module_TableHolder.Download_Table.Rows.InsertAt((DataRow)e6_DownloadItemRef.Tag, 0);
                    }
                }
                e6_DownloadItemRef.Dispose();
                UpdateTreeViewText();
            }
            else
            {
                if (e.Error != null)
                {
                    string ErrorMsg = e.Error.InnerException == null ? e.Error.Message : e.Error.InnerException.Message;
                    if (ErrorMsg.Contains("An existing connection was forcibly closed by the remote host."))
                    {
                        string PicURL = (string)((DataRow)e6_DownloadItemRef.Tag)["Grab_MediaURL"];
                        if (!Module_TableHolder.DownloadQueueContainsURL(PicURL) && !Download_AlreadyDownloaded.Contains(PicURL))
                        {
                            lock (Module_TableHolder.Download_Table)
                            {
                                Module_TableHolder.Download_Table.Rows.InsertAt((DataRow)e6_DownloadItemRef.Tag, 0);
                            }
                        }
                        e6_DownloadItemRef.Dispose();
                        UpdateTreeViewText();
                    }
                    else
                    {
                        MessageBox.Show(ErrorMsg, "e621 ReBot Downloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw e.Error;
                    }
                }
            }
            if (!timer_DownloadRemovalThreading.Enabled)
            {
                timer_DownloadRemovalThreading.Start();
            }
        }

        private static readonly Timer timer_DownloadRemovalThreading;
        public static void DownloadRemovalThreading_Tick(object sender, EventArgs e)
        {
            timer_DownloadRemovalThreading.Stop();
            UIDrawController.SuspendDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
            Form_Loader._FormReference.DownloadFLP_InProgress.SuspendLayout();
            Form_Loader._FormReference.DownloadFLP_Downloaded.SuspendLayout();
            for (int i = Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Count - 1; i >= 0; i--)
            {
                e6_DownloadItem e6_DownloadItemTemp = (e6_DownloadItem)Form_Loader._FormReference.DownloadFLP_InProgress.Controls[i];
                if (e6_DownloadItemTemp._DownloadFinished)
                {
                    if (!e6_DownloadItemTemp._AlreadyCopied)
                    {
                        DataRow DataRowTemp = (DataRow)e6_DownloadItemTemp.Tag;

                        lock (Download_AlreadyDownloaded)
                        {
                            Download_AlreadyDownloaded.Add((string)DataRowTemp["Grab_MediaURL"]);
                        }
                        Image ImageHolder = e6_DownloadItemTemp.picBox_ImageHolder.Tag == null ? e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage : null;

                        AddPic2FLP((string)DataRowTemp["Grab_MediaURL"], e6_DownloadItemTemp.DL_FolderIcon.Tag.ToString(), ImageHolder);
                        e6_DownloadItemTemp.picBox_ImageHolder.Tag = null;
                        e6_DownloadItemTemp._AlreadyCopied = true;

                        if (e6_DownloadItemTemp.DataRow4Grid != null)
                        {
                            e6_DownloadItemTemp.DataRow4Grid["DL_FilePath"] = e6_DownloadItemTemp.DL_FolderIcon.Tag.ToString();
                        }
                    }
                    if (Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Count > DLThreadsCount || !Form_Loader._FormReference.cCheckGroupBox_Download.Checked || Module_TableHolder.Download_Table.Rows.Count == 0)
                    {
                        e6_DownloadItemTemp.Dispose();
                    }
                }
            }
            Form_Loader._FormReference.DownloadFLP_InProgress.ResumeLayout();
            Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
            UIDrawController.ResumeDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);

            DLThreadsWaiting = 0;
            foreach (e6_DownloadItem e6_DownloadItemTemp in Form_Loader._FormReference.DownloadFLP_InProgress.Controls)
            {
                if (e6_DownloadItemTemp._DownloadFinished)
                {
                    DLThreadsWaiting += 1;
                }
            }

            if (Download_AlreadyDownloaded.Count % 1000 == 0)
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }
        }

        public static void AddPic2FLP(string ThumbURL, string FilePath, Image e6Pic = null)
        {
            e6_DownloadItem e6_DownloadItemTemp;
            if (Form_Loader._FormReference.DownloadFLP_Downloaded.Controls.Count < Form_Loader._DLHistoryMaxControls)
            {
                e6_DownloadItemTemp = new e6_DownloadItem();
                e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;
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
                if (FilePath.EndsWith("webm") || FilePath.EndsWith("mp4") || FilePath.EndsWith("swf"))
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
        private static int DLThreadsWaiting = 0;

        public static void Download_Start()
        {
            if (Form_Loader._FormReference.cCheckGroupBox_Download.Checked && Module_TableHolder.Download_Table.Rows.Count > 0)
            {
                int HowMany2Start = Math.Max(0, DLThreadsCount - Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Count) + DLThreadsWaiting;
                while (HowMany2Start > 0)
                {
                    if (Module_TableHolder.Download_Table.Rows.Count == 0)
                    {
                        break;
                    }
                    HowMany2Start -= 1;
                    DLThreadsWaiting = Math.Max(0, DLThreadsWaiting - 1);
                    DataRow DataRowTemp = Module_TableHolder.Download_Table.NewRow();
                    DataRowTemp.ItemArray = (object[])Module_TableHolder.Download_Table.Rows[0].ItemArray.Clone();
                    if (DataRowTemp["DataRowRef"] != DBNull.Value)
                    {
                        DownloadFrom_URL(DataRowTemp);
                    }
                    else // e6 download
                    {
                        if (!DownloadFrom_e6URL(DataRowTemp)) //Duplicate skip
                        {
                            DLThreadsWaiting += 1;
                            HowMany2Start += 1;
                        }
                    }
                    lock (Module_TableHolder.Download_Table)
                    {
                        Module_TableHolder.Download_Table.Rows.RemoveAt(0);
                    }
                }
            }
            UpdateTreeViewText();
            UpdateTreeViewNodes();
        }

        private static e6_DownloadItem FindDownloadItem()
        {
            foreach (e6_DownloadItem e6_DownloadItemTemp in Form_Loader._FormReference.DownloadFLP_InProgress.Controls)
            {
                if (e6_DownloadItemTemp._DownloadFinished)
                {
                    e6_DownloadItemTemp.DL_ProgressBar.Value = 0;
                    e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                    e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = null;
                    e6_DownloadItemTemp.picBox_ImageHolder.Image = null;
                    e6_DownloadItemTemp._DownloadFinished = false;
                    e6_DownloadItemTemp._AlreadyCopied = false;
                    return e6_DownloadItemTemp;
                }
            }
            return null;
        }

        private static void DownloadFrom_URL(DataRow DataRowRef)
        {
            string WorkURL = (string)DataRowRef["Grab_URL"];
            var DomainURL = new Uri(WorkURL);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            string PurgeArtistName = ((string)DataRowRef["Artist"]).Replace("/", "-");
            PurgeArtistName = Path.GetInvalidFileNameChars().Aggregate(PurgeArtistName, (current, c) => current.Replace(c.ToString(), string.Empty));
            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, PurgeArtistName);
            Directory.CreateDirectory(FolderPath);

            DataRow DataRow4Grid = DataRowRef["DataRowRef"] != DBNull.Value ? (DataRow)DataRowRef["DataRowRef"] : null;
            string GetFileNameOnly = GetMediasFileNameOnly((string)DataRowRef["Grab_MediaURL"], DataRow4Grid);
            if (GetFileNameOnly.EndsWith(".", StringComparison.Ordinal))
            {
                GetFileNameOnly += Module_HicceArs.GetHicceArsMediaType((string)DataRowRef["Grab_MediaURL"]);
            }

            string ImageRename = null;
            switch (GetFileNameOnly)
            {
                case string UgoiraTest when GetFileNameOnly.Contains("ugoira"):
                    {
                        string WebMName = $"{GetFileNameOnly.Substring(0, GetFileNameOnly.IndexOf("_ugoira0"))}_ugoira1920x1080.webm";
                        ImageRename = RenameMediaFileName(WebMName, DataRowRef);

                        string FilePath = Path.Combine(FolderPath, ImageRename);
                        if (File.Exists(FilePath))
                        {
                            Form_Loader._FormReference.BeginInvoke(new Action(() =>
                            {
                                Form_Loader._FormReference.DownloadFLP_Downloaded.SuspendLayout();
                                UIDrawController.SuspendDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
                                AddPic2FLP((string)DataRowRef["Grab_ThumbnailURL"], FilePath);
                                Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
                                UIDrawController.ResumeDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
                            }));
                            return;
                        }

                        Form_Loader._FormReference.Invoke(new Action(() =>
                        {
                            e6_DownloadItem e6_DownloadItemTemp = FindDownloadItem();
                            bool AddNew = false;
                            if (e6_DownloadItemTemp == null)
                            {
                                e6_DownloadItemTemp = new e6_DownloadItem();
                                AddNew = true;
                            }
                            e6_DownloadItemTemp.Tag = DataRowRef;
                            e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                            e6_DownloadItemTemp.DL_ProgressBar.BarColor = Color.Orange;
                            e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;

                            StartDLClient(ref e6_DownloadItemTemp, "Thumb");
                            if (AddNew) Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);
                            Module_FFmpeg.DownloadQueue_ConvertUgoira2WebM(ref e6_DownloadItemTemp);
                        }));
                        break;
                    }

                default:
                    {
                        ImageRename = RenameMediaFileName(GetFileNameOnly, DataRowRef);

                        string FilePath = Path.Combine(FolderPath, ImageRename);
                        if (File.Exists(FilePath) || (MediaBrowser_MediaCache.Keys.Contains(GetFileNameOnly) && ReSaveMedia(ref DataRowRef)))
                        {
                            Form_Loader._FormReference.BeginInvoke(new Action(() =>
                            {
                                Form_Loader._FormReference.DownloadFLP_Downloaded.SuspendLayout();
                                UIDrawController.SuspendDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
                                AddPic2FLP((string)DataRowRef["Grab_ThumbnailURL"], FilePath);
                                Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
                                UIDrawController.ResumeDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
                            }));
                            return;
                        }

                        Form_Loader._FormReference.Invoke(new Action(() =>
                        {
                            e6_DownloadItem e6_DownloadItemTemp = FindDownloadItem();
                            bool AddNew = false;
                            if (e6_DownloadItemTemp == null)
                            {
                                e6_DownloadItemTemp = new e6_DownloadItem();
                                AddNew = true;
                            }
                            e6_DownloadItemTemp.Tag = DataRowRef;
                            e6_DownloadItemTemp.DL_ProgressBar.Visible = true;
                            e6_DownloadItemTemp.DL_ProgressBar.BarColor = Color.Orange;
                            e6_DownloadItemTemp.DL_FolderIcon.Tag = FilePath;

                            if (DataRow4Grid == null || DataRow4Grid.RowState == DataRowState.Detached)
                            {
                                //Weasyl special
                                if (((string)DataRowRef["Grab_ThumbnailURL"]).Contains("cdn.weasyl.com"))
                                {
                                    e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = Properties.Resources.BrowserIcon_Weasly;
                                    e6_DownloadItemTemp.picBox_ImageHolder.Tag = true;
                                }
                                else
                                {
                                    StartDLClient(ref e6_DownloadItemTemp, "Thumb");
                                }
                            }
                            else
                            {
                                e6_DownloadItemTemp.DataRow4Grid = DataRow4Grid;
                                //Weasyl special
                                if (DataRow4Grid["Grab_ThumbnailURL"] == DBNull.Value || string.IsNullOrEmpty((string)DataRow4Grid["Grab_ThumbnailURL"]))
                                {
                                    e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = Properties.Resources.BrowserIcon_Weasly;
                                    e6_DownloadItemTemp.picBox_ImageHolder.Tag = true;
                                }
                                else
                                {
                                    if (DataRow4Grid["Thumbnail_Image"] == DBNull.Value)
                                    {
                                        DataRow4Grid["Thumbnail_DLStart"] = true;
                                        StartDLClient(ref e6_DownloadItemTemp, "Thumb");
                                    }
                                    else
                                    {
                                        e6_DownloadItemTemp.picBox_ImageHolder.BackgroundImage = (Image)((Image)DataRow4Grid["Thumbnail_Image"]).Clone();
                                    }
                                }
                            }
                            if (AddNew) Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);

                            switch ((string)DataRowRef["Info_MediaFormat"])
                            {
                                //case "mp4":
                                case "swf":
                                    {
                                        if (Properties.Settings.Default.Converter_DontConvertVideos)
                                        {
                                            StartDLClient(ref e6_DownloadItemTemp, "File");
                                        }
                                        else
                                        {
                                            Module_FFmpeg.DownloadQueue_ConvertVideo2WebM(ref e6_DownloadItemTemp);
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        StartDLClient(ref e6_DownloadItemTemp, "File");
                                        break;
                                    }
                            }
                        }));
                        break;
                    }
            }
        }

        private static bool DownloadFrom_e6URL(DataRow DataRowRef)
        {
            string PicURL = (string)DataRowRef["Grab_MediaURL"];

            string GetFileNameOnly = GetMediasFileNameOnly(PicURL);
            if (DownloadFolderCache != null && DownloadFolderCache.Contains(GetFileNameOnly))
            {
                return false;
            }

            string ThumbLink = PicURL.Replace("net/data/", "net/data/preview/");
            ThumbLink = ThumbLink.Remove(ThumbLink.LastIndexOf(".") + 1) + "jpg";

            string DLPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, @"e621\").ToString();
            string PoolName = DataRowRef["e6_PoolName"] != DBNull.Value ? (string)DataRowRef["e6_PoolName"] : null;
            if (PoolName != null)
            {
                DLPath += PoolName + @"\";
            }

            Directory.CreateDirectory(DLPath);

            string PoolPostIndex = DataRowRef["e6_PoolPostIndex"] == DBNull.Value ? null : (string)DataRowRef["e6_PoolPostIndex"];
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
                e6_DownloadItem e6_DownloadItemTemp = FindDownloadItem();
                bool AddNew = false;
                if (e6_DownloadItemTemp == null)
                {
                    e6_DownloadItemTemp = new e6_DownloadItem();
                    AddNew = true;
                }
                e6_DownloadItemTemp.Tag = DataRowRef;
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
                StartDLClient(ref e6_DownloadItemTemp, "File");
                if (AddNew)
                {
                    Form_Loader._FormReference.DownloadFLP_InProgress.Controls.Add(e6_DownloadItemTemp);
                }
            }));
            return true;
        }

        private static string LastGrabFolder = null;
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
                AddDownloadQueueItem(DataRowRef: null, URL: BrowserAdress, Media_URL: PicURL, e6_PostID: PostID);
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/posts", StringComparison.OrdinalIgnoreCase))  // multi
            {
                string inputtext = Custom_InputBox.Show(Form_Loader._FormReference, "e621 ReBot", "If you want to download media to a separate folder, enter a folder name below.", Form_Loader._FormReference.BQB_Start.PointToScreen(Point.Empty), LastGrabFolder);
                if (!string.IsNullOrEmpty(inputtext) && !inputtext.Equals("✄"))
                {
                    inputtext = string.Join("", inputtext.Split(Path.GetInvalidFileNameChars()));
                    inputtext = inputtext.Trim();
                }
                else
                {
                    inputtext = null;
                }
                LastGrabFolder = inputtext;

                if (BrowserAdress.StartsWith("https://e621.net/posts?tags=", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(Properties.Settings.Default.API_Key) || WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                    {
                        goto GrabPageOnly_Tags;
                    }

                    if (MessageBox.Show("Do you want to download all images with current tags?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        //if (e6APIDL_BGW.IsBusy)
                        //{
                        //    NotifyWhenDone = true;
                        //    MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    return;
                        //}

                        string TagQuery = BrowserAdress;
                        TagQuery = TagQuery.Substring(TagQuery.IndexOf("tags=") + 5);
                        // TagQuery = WebUtility.UrlDecode(TagQuery).Replace(" ", "+")
                        Module_e621APIMinion.AddWork2Queue("Posts", Module_e621APIMinion.GrabAllImagesWithGivenTags, new string[] { TagQuery, inputtext });
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
                            AddDownloadQueueItem(DataRowRef: null, URL: BrowserAdress, Media_URL: PicURL, e6_PostID: Post.Attributes["data-id"].Value, e6_PoolName: inputtext);
                        }
                    }
                }
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/pools/", StringComparison.OrdinalIgnoreCase))
            {
                HtmlNode BottomMenuHolder = WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu");
                if (string.IsNullOrEmpty(Properties.Settings.Default.API_Key) || BottomMenuHolder.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                {
                    goto GrabPageOnly_Pools;
                }

                if (MessageBox.Show("Do you want to download the whole pool?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //if (e6APIDL_BGW.IsBusy)
                    //{
                    //    NotifyWhenDone = true;
                    //    MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

                    string ComicID = BrowserAdress.Replace("https://e621.net/pools/", "");
                    Module_e621APIMinion.AddWork2Queue("Pool", Module_e621APIMinion.GraBPoolInBG, ComicID);
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
                        ComicPages = JObject.Parse(Module_e621Info.e621InfoDownload($"https://e621.net/pools/{PoolID}.json"))["post_ids"].Values<string>().ToList();
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
                        AddDownloadQueueItem(DataRowRef: null, URL: BrowserAdress, Media_URL: PicURL, e6_PostID: PostID, e6_PoolName: PoolName, e6_PoolPostIndex: PoolPostIndex);
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
                            AddDownloadQueueItem(DataRowRef: null, URL: BrowserAdress, Media_URL: PicURL, e6_PostID: Post.Attributes["data-id"].Value);
                        }
                    }
                }
                return;
            }

            if (BrowserAdress.StartsWith("https://e621.net/favorites", StringComparison.OrdinalIgnoreCase))
            {
                string inputtext = Custom_InputBox.Show(Form_Loader._FormReference, "e621 ReBot", "If you want to download media to a separate folder, enter a folder name below.", Form_Loader._FormReference.BQB_Start.PointToScreen(Point.Empty), LastGrabFolder);
                if (!string.IsNullOrEmpty(inputtext) && !inputtext.Equals("✄"))
                {
                    inputtext = string.Join("", inputtext.Split(Path.GetInvalidFileNameChars()));
                    inputtext = inputtext.Trim();
                }
                else
                {
                    inputtext = null;
                }
                LastGrabFolder = inputtext;

                if (string.IsNullOrEmpty(Properties.Settings.Default.API_Key) || WebDoc.DocumentNode.SelectSingleNode("//div[@class='paginator']/menu").ChildNodes.Count <= 3)
                {
                    goto GrabPageOnly_Favorites;
                }

                if (MessageBox.Show("Do you want to download all favorites?\nPress no if you want current page only.", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //if (e6APIDL_BGW.IsBusy)
                    //{
                    //    MessageBox.Show("You are already grabbing post from e621 API, you need to wait for it to finish.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

                    string TagQuery = BrowserAdress;
                    if (TagQuery.Contains("user_id"))
                    {
                        TagQuery = WebDoc.DocumentNode.SelectSingleNode("//input[@id='tags']").Attributes["value"].Value;
                    }
                    else
                    {
                        if (TagQuery.Contains("?"))
                        {
                            TagQuery = TagQuery.Substring(TagQuery.IndexOf("?") + 1);
                        }
                        else
                        {
                            TagQuery = WebDoc.DocumentNode.SelectSingleNode("//input[@id='tags']").Attributes["value"].Value;
                        }
                    }
                    Module_e621APIMinion.AddWork2Queue("Favorites", Module_e621APIMinion.GrabAllImagesWithGivenTags, new string[] { TagQuery, inputtext });
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
                            AddDownloadQueueItem(DataRowRef: null, URL: BrowserAdress, Media_URL: PicURL, e6_PostID: Post.Attributes["data-id"].Value);
                        }
                    }
                }
                return;
            }
        }



        // - - - - - - - - - - -



        public static List<string> DownloadFolderCache = new List<string>();

        public static void Load_DownloadFolderCache()
        {
            DownloadFolderCache.Clear();
            timer_CacheProgressTimer.Start();
            new Thread(Load_DownloadFolderCache_BGW).Start();
        }

        private static readonly Timer timer_CacheProgressTimer;

        private static void CacheProgressTimer_Tick(object sender, EventArgs e)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() => { Form_Loader._FormReference.bU_SkipDLCache.Text = DownloadFolderCache.Count.ToString(); }));
        }

        private static void Load_DownloadFolderCache_BGW()
        {
            DirectoryInfo CacheFolder = new DirectoryInfo(Properties.Settings.Default.DownloadsFolderLocation + "\\e621");

            foreach (FileInfo FileFound in CacheFolder.GetFiles("*", SearchOption.AllDirectories))
            {
                string GetFileMD5 = FileFound.Name;
                if (GetFileMD5.Contains("_"))
                {
                    GetFileMD5 = GetFileMD5.Substring(GetFileMD5.LastIndexOf("_") + 1);
                }
                //GetFileMD5 = GetFileMD5.Substring(0, GetFileMD5.LastIndexOf("."));

                if (!DownloadFolderCache.Contains(GetFileMD5))
                {
                    DownloadFolderCache.Add(GetFileMD5);
                }
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                timer_CacheProgressTimer.Stop();
                Form_Loader._FormReference.bU_SkipDLCache.Text = Form_Loader._FormReference.bU_SkipDLCache.Tag.ToString();
                Form_Loader._FormReference.bU_SkipDLCache.Enabled = true;
                MessageBox.Show(string.Format("Cached {0} files.", DownloadFolderCache.Count), "e621 ReBot");
            }));
        }



        // - - - - - - - - - - -



        public static void FileDownloader(string DownloadURL, string ActionType, string TempFolder, string DownloadFolder, in DataRow DataRowRef = null, bool isUgoira = false, Custom_ProgressBar RoundProgressBarRef = null)
        {
            HttpWebRequest FileDownloader = (HttpWebRequest)WebRequest.Create(DownloadURL);
            if (isUgoira) FileDownloader.Referer = "https://www.pixiv.net/";
            if (DownloadURL.Contains("https://www.hiccears.com/file/")) FileDownloader.CookieContainer = Module_CookieJar.Cookies_HicceArs;
            FileDownloader.Timeout = 5000;

            using (MemoryStream DownloadedBytes = new MemoryStream())
            {
                using (WebResponse DownloaderReponse = FileDownloader.GetResponse())
                {
                    using (Stream DownloadStream = DownloaderReponse.GetResponseStream())
                    {
                        byte[] DownloadBuffer = new byte[65536]; // 64 kB buffer
                        while (DownloadedBytes.Length < DownloaderReponse.ContentLength)
                        {
                            int DownloadStreamPartLength = DownloadStream.Read(DownloadBuffer, 0, DownloadBuffer.Length);
                            if (DownloadStreamPartLength > 0)
                            {
                                DownloadedBytes.Write(DownloadBuffer, 0, DownloadStreamPartLength);
                                double ReportPercentage = DownloadedBytes.Length / (double)DownloaderReponse.ContentLength;

                                switch (ActionType)
                                {
                                    case "U":
                                        {
                                            Module_Uploader.Report_Status(string.Format("Downloading {0}...{1:P0}", isUgoira ? "Ugoira" : "Media", ReportPercentage));
                                            break;
                                        }
                                    case "C":
                                        {
                                            string ReportType = isUgoira ? "CDU" : "CDV";
                                            Module_FFmpeg.ReportConversionProgress(ReportType, ReportPercentage, in DataRowRef);
                                            break;
                                        }

                                    case "D":
                                        {

                                            RoundProgressBarRef.BeginInvoke(new Action(() => { RoundProgressBarRef.Value = (int)(ReportPercentage * 100); }));
                                            break;
                                        }

                                    case "M":
                                        {
                                            if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, DataRowRef))
                                            {
                                                Form_Preview._FormReference.BeginInvoke(new Action(() =>
                                                {
                                                    if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated)
                                                    {
                                                        Form_Preview._FormReference.Label_Download.Text = $"{ReportPercentage:P0}";
                                                        Form_Preview._FormReference.Label_Download.Visible = true;
                                                        Form_Preview._FormReference.PB_Download.Visible = false;
                                                        Form_Preview._FormReference.Label_DownloadWarning.Visible = false;
                                                    }
                                                }));
                                            }
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                string FileName = GetMediasFileNameOnly(DownloadURL);
                //HicceArs filename fix for extension is not needed, e621 detects it.
                if (isUgoira)
                {
                    using (ZipArchive UgoiraZip = new ZipArchive(DownloadedBytes))
                    {
                        UgoiraZip.ExtractToDirectory(TempFolder);
                        if (DownloadFolder != null && Properties.Settings.Default.Converter_KeepOriginal)
                        {
                            Directory.CreateDirectory(DownloadFolder);
                            DownloadedBytes.Seek(0, SeekOrigin.Begin);
                            using (FileStream TempFileStream = new FileStream($"{DownloadFolder}\\{FileName}", FileMode.Create))
                            {
                                DownloadedBytes.WriteTo(TempFileStream);
                            }
                        }
                    }
                }
                else
                {
                    using (FileStream TempFileStream = new FileStream($"{TempFolder}\\{FileName}", FileMode.Create))
                    {
                        DownloadedBytes.WriteTo(TempFileStream);
                    }
                    if (DownloadFolder != null && (Properties.Settings.Default.Converter_KeepOriginal || ActionType.Equals("M")))
                    {
                        Directory.CreateDirectory(DownloadFolder);
                        File.Copy($"{TempFolder}\\{FileName}", $"{DownloadFolder}\\{FileName}", true);
                    }
                }
            }
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