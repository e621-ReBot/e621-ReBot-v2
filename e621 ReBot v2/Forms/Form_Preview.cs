using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_Preview : Form
    {
        public static Form_Preview _FormReference;
        public Form_Preview()
        {
            InitializeComponent();
            _FormReference = this;
            GotFocus += Form_Preview_GotFocus;
            PB_Previous.Click += PB_Navigation_Click;
            PB_Next.Click += PB_Navigation_Click;
            PB_Explicit.Click += PB_Rating_Click;
            PB_Questionable.Click += PB_Rating_Click;
            PB_Safe.Click += PB_Rating_Click;

            PB_Previous.GotFocus += Button_GotFocus;
            PB_Next.GotFocus += Button_GotFocus;
            PB_Explicit.GotFocus += Button_GotFocus;
            PB_Questionable.GotFocus += Button_GotFocus;
            PB_Safe.GotFocus += Button_GotFocus;

            PB_IQDBQ.Click += PB_SimilarSearch_Click;
            PB_SauceNao.Click += PB_SimilarSearch_Click;
        }

        public DataRow Preview_RowHolder;
        public int Preview_RowIndex;
        private void Form_Preview_Load(object sender, EventArgs e)
        {
            Preview_RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(Preview_RowHolder);
            UpdateNavButtons();
            Label_AlreadyUploaded.Cursor = Form_Loader.Cursor_ReBotNav;
            PB_IQDBQ.Enabled = !Properties.Settings.Default.API_Key.Equals("");
        }

        public string URL2Navigate;
        private void Form_Preview_Shown(object sender, EventArgs e)
        {
            timer_NavDelay.Start();
        }

        private void Timer_NavDelay_Tick(object sender, EventArgs e)
        {
            timer_NavDelay.Stop();
            NavURL(URL2Navigate);
            ResizeEnd += Form_Preview_ResizeEnd;
            Resize += Form_Preview_Resize;
        }

        public void NavURL(string URL2Navigate)
        {
            if (URL2Navigate.Contains("pximg.net"))
            {
                Pic_WebBrowser.Navigate(URL2Navigate, "", null, "Referer: http://www.pixiv.net");
            }
            else
            {
                Pic_WebBrowser.Navigate(URL2Navigate);
            }
        }


        private void Form_Preview_ResizeEnd(object sender, EventArgs e)
        {
            ResizeBrowserAndZoom();
        }

        private FormWindowState LastWindowState;
        private void Form_Preview_Resize(object sender, EventArgs e)
        {
            if (!(LastWindowState == WindowState))
            {
                ResizeBrowserAndZoom();
            }
            LastWindowState = WindowState;
        }

        private void Form_Preview_GotFocus(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void Form_Preview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                e.SuppressKeyPress = true; // fix alt key causing cursor to become arrow and act strange
                Label_AlreadyUploaded.Cursor = Form_Loader.Cursor_BrowserNav;
            }
            base.KeyDown -= Form_Preview_KeyDown;
        }

        private void Form_Preview_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 18) //Keys.Alt
            {
                Label_AlreadyUploaded.Cursor = Form_Loader.Cursor_ReBotNav;
            }
            base.KeyDown += Form_Preview_KeyDown;
        }

        private void ResizeBrowserAndZoom()
        {
            // Actual pixel size is -14 width, -7 height compared to design size, -16, -9 internal
            // Title bar height is 30px
            int BrowserWidth = Width - 16; // 16 = 2 * 8
            int BrowserHeight = Height - 30 - 8 - Pic_WebBrowser.Location.Y;
            Pic_WebBrowser.Size = new Size(BrowserWidth, BrowserHeight);
            Label_Tags.Width = BrowserWidth;
            FitImageHeight2Browser();
        }

        private bool ZoomAfterLoad;
        public void FitImageHeight2Browser()
        {
            if (Pic_WebBrowser.Document is null)
            {
                // MessageBox.Show("Errored somehow, no document!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Text = "Errored somehow, no document!";
                ZoomAfterLoad = true;
                return;
            }

            if (Pic_WebBrowser.Document.Images is null)
            {
                // MessageBox.Show("Errored somehow, no image!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                Text = "Errored somehow, no image!";
                ZoomAfterLoad = true;
                return;
            }

            if (!(Pic_WebBrowser.Document.Images.Count == 0))
            {
                Rectangle ImageRect = Pic_WebBrowser.Document.Images[0].ClientRectangle;
                double WRatio = (double)ImageRect.Width / (Pic_WebBrowser.Size.Width - 8);
                double HRatio = (double)ImageRect.Height / (Pic_WebBrowser.Size.Height - 8);
                double Ratio = 100d;
                double BiggerR = Math.Max(WRatio, HRatio);
                double SmallerR = Math.Min(WRatio, HRatio);
                if (BiggerR > 1d)
                {
                    Ratio *= 1d / BiggerR;
                    SmallerR *= 1d / BiggerR;
                    if (SmallerR > 1d)
                    {
                        Ratio *= 1d / SmallerR;
                    }
                }

                Pic_WebBrowser.Zoom((int)Math.Floor(Ratio));
                // Pic_WebBrowser.Document.Body.Style = String.Format("zoom: {0}%", Ratio - 0.5) 'Doesn't work for some people.
            }
        }

        private void Pic_WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            Text = "Preview";
            PB_Upload.Enabled = false;
            PB_Download.Enabled = false;
            PB_Download.Visible = true;
            Label_DownloadWarning.Visible = false;
            Label_Download.Visible = false;
            panel_Search.Enabled = false;
            Label_AlreadyUploaded.Text = null;
            Label_Tags.Text = null;

            string ImageURL = (string)Preview_RowHolder["Grab_MediaURL"];
            string ImageName = Module_Downloader.GetMediasFileNameOnly(ImageURL);
            if (Module_Downloader.Download_AlreadyDownloaded.Contains(ImageName))
            {
                PB_Download.Visible = false;
                Label_Download.Visible = true;
            }

            if (ImageName.Contains("ugoira"))
            {
                Label_DownloadWarning.Visible = true;
                toolTip_Display.SetToolTip(Label_DownloadWarning, string.Format("This is an {0}, you need to download it in order to view the animated version.", "Ugoira"));
            }
            else
            {
                if (ImageName.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || ImageName.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                {
                    toolTip_Display.SetToolTip(Label_DownloadWarning, string.Format("This is an {0}, you need to download it in order to view the animated version.", ImageName.Substring(ImageName.Length - 3).ToUpper()));
                    Label_DownloadWarning.Visible = true;
                }
            }

            if (Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes.ContainsKey(ImageURL))
            {
                PB_Download.Visible = false;
                Label_DownloadWarning.Visible = false;
                Label_Download.Visible = true;
                if (Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes[0].Name.Equals(ImageURL))
                {
                    string StatusLabelText = Form_Loader._FormReference.label_ConversionStatus.Text;
                    Label_Download.ForeColor = StatusLabelText.Contains("Downloading") ? Color.DarkOrange : Color.DarkOrchid;
                    Label_Download.Text = StatusLabelText.Substring(StatusLabelText.LastIndexOf("...") + 3);
                }
                else
                {
                    Label_Download.Text = "0%";
                    Label_Download.ForeColor = Color.DarkOrange;
                }

            }

            if (PB_Download.Visible)
            {
                PB_ViewFile.Visible = false;
            }
            else
            {
                if (!Label_Download.Visible)
                {
                    if (Preview_RowHolder["DL_FilePath"] != DBNull.Value && File.Exists((string)Preview_RowHolder["DL_FilePath"]))
                    {
                        PB_ViewFile.Visible = true;
                        if (ImageName.Contains("ugoira") || ImageName.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || ImageName.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                        {
                            PB_ViewFile.Text = "▶";
                        }
                        else
                        {
                            PB_ViewFile.Text = "🔍";
                        }
                    }
                    else
                    {
                        PB_Download.Visible = true;
                        Module_Downloader.Download_AlreadyDownloaded.Remove(ImageName);
                        Preview_RowHolder["DL_FilePath"] = DBNull.Value;
                    }
                }
            }
        }

        private void Pic_WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Pic_WebBrowser.Document.BackColor = Color.DimGray;
            UpdateNavButtons();
            UpdateButtons();
            FitImageHeight2Browser();
        }

        private void Pic_WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (Pic_WebBrowser.Url.ToString().Equals("about:blank"))
            {
                return;
            }

            if (Pic_WebBrowser.Document.Images.Count > 1) // Fix for that navigation canceled page
            {
                navCancelFix_Timer.Start();
                return;
            }

            if (ZoomAfterLoad)
            {
                ZoomAfterLoad = false;
                FitImageHeight2Browser();
            }

            if (((string)Preview_RowHolder["Grab_MediaURL"]).ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || ((string)Preview_RowHolder["Grab_MediaURL"]).ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
            {
                Text = string.Format("Preview ({0}) - {1:N0} kB   .{2}", Preview_RowIndex + 1, (int)((int)Preview_RowHolder["Info_MediaByteLength"] / 1024d), (string)Preview_RowHolder["Info_MediaFormat"]);
                AutoTags();
            }
            else
            {
                if (((string)Preview_RowHolder["Grab_MediaURL"]).Contains("ugoira"))
                {
                    Text = string.Format("Preview ({0}) - {1:N0} kB   Ugoira", Preview_RowIndex + 1, (int)((int)Preview_RowHolder["Info_MediaByteLength"] / 1024d));
                    AutoTags();
                }
                else
                {
                    if (Preview_RowHolder["Thumbnail_FullInfo"] == DBNull.Value)
                    {
                        Preview_RowHolder["Info_MediaWidth"] = Pic_WebBrowser.Document.Images[0].ClientRectangle.Size.Width;
                        Preview_RowHolder["Info_MediaHeight"] = Pic_WebBrowser.Document.Images[0].ClientRectangle.Size.Height;
                        Preview_RowHolder["Thumbnail_FullInfo"] = true;
                        e6_GridItem GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                        if (GridItemTemp != null)
                        {
                            GridItemTemp.LoadImage();
                        }
                        else
                        {
                            e6_GridItem.WriteImageInfo(Preview_RowHolder);
                        }
                    }

                    if (Preview_RowHolder["Info_MediaMD5"] == DBNull.Value)
                    {
                        GetCachedImage();
                        CheckMD5();
                        AutoTags();
                        Preview_RowHolder["Info_TooBig"] = Module_Uploader.Media2BigCheck(ref Preview_RowHolder);
                    }
                    Text = string.Format("Preview ({0}) - {1} x {2}, {3:N0} kB   .{4}     [MD5: {5}]", Preview_RowIndex + 1, Pic_WebBrowser.Document.Images[0].ClientRectangle.Size.Width, Pic_WebBrowser.Document.Images[0].ClientRectangle.Size.Height, (int)Preview_RowHolder["Info_MediaByteLength"] / 1024, (string)Preview_RowHolder["Info_MediaFormat"], (string)Preview_RowHolder["Info_MediaMD5"]);
                }
            }

            PB_Upload.Enabled = true;
            PB_Download.Enabled = true;
            panel_Search.Enabled = Preview_RowHolder["Uploaded_As"] == DBNull.Value;
            Label_AlreadyUploaded.Text = Preview_RowHolder["Uploaded_As"] == DBNull.Value ? null : string.Format("Already uploaded as #{0}", Preview_RowHolder["Uploaded_As"]);
            Label_Tags.Text = (string)Preview_RowHolder["Upload_Tags"];

            if (Preview_RowHolder["Info_TooBig"] == DBNull.Value && Preview_RowHolder["Info_MediaByteLength"] != DBNull.Value && Preview_RowHolder["Info_MediaWidth"] == DBNull.Value)
            {
                Preview_RowHolder["Info_TooBig"] = Module_Uploader.Media2BigCheck(ref Preview_RowHolder);
            }
            if (LoadAllImagesMod)
            {
                if (Preview_RowIndex == Module_TableHolder.Database_Table.Rows.Count - 1)
                {
                    PB_LoadAllImages.PerformClick();
                }
                else
                {
                    PB_Navigation_Click(PB_Next, null);
                    return;
                }
            }
            PB_LoadAllImages.Enabled = !(Preview_RowIndex == Module_TableHolder.Database_Table.Rows.Count - 1);
        }

        private void NavCancelFix_Timer_Tick(object sender, EventArgs e)
        {
            navCancelFix_Timer.Stop();
            SendKeys.SendWait("{ESC}");
            NavURL(URL2Navigate);
        }

        public void UpdateButtons()
        {
            foreach (Button TempButton in panel_Rating.Controls)
            {
                if (Preview_RowHolder["Upload_Rating"].Equals(TempButton.Text))
                {
                    TempButton.BackColor = TempButton.ForeColor;
                    TempButton.Enabled = false;
                }
                else
                {
                    TempButton.BackColor = Color.FromArgb(0, 45, 90);
                    TempButton.Enabled = true;
                }
            }
            PB_Upload.BackColor = (bool)Preview_RowHolder["UPDL_Queued"] ? Color.LimeGreen : Color.FromArgb(0, 45, 90);
        }

        private void GetCachedImage()
        {
            string ImageName = Module_Downloader.GetMediasFileNameOnly((string)Preview_RowHolder["Grab_MediaURL"]);
            string ImageExtension = ImageName.Substring(ImageName.LastIndexOf("."));
            ImageName = ImageName.Substring(0, ImageName.LastIndexOf("."));
            ImageExtension = ImageExtension.Replace(".jpeg", ".jpg"); // Cache saves jpeg as jpg
            DirectoryInfo CacheFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\IE");
            FileInfo[] FileList = CacheFolder.GetFiles(string.Format("*{0}*", ImageExtension), SearchOption.AllDirectories);
            foreach (FileInfo FileFound in FileList)
            {
                if (FileFound.Name.Contains(ImageName + "["))
                {
                    string FilenameCut = FileFound.Name.Replace("[1].", ".");
                    if (!Module_Downloader.IEDownload_Cache.ContainsKey(FilenameCut))
                    {
                        Module_Downloader.IEDownload_Cache.Add(FilenameCut, FileFound.FullName);
                    }

                    byte[] ImageBytes = null;
                    for (int numTries = 0; numTries < 10; numTries++)
                    {
                        FileStream FileStreamTemp = null;
                        try
                        {
                            FileStreamTemp = new FileStream(FileFound.FullName, FileMode.Open, FileAccess.Read);
                            int FileStreamTempLenght = (int)FileStreamTemp.Length;
                            ImageBytes = new byte[FileStreamTempLenght];
                            FileStreamTemp.Read(ImageBytes, 0, FileStreamTempLenght);
                            break;
                        }
                        catch (IOException)
                        {
                                Thread.Sleep(500);
                        }
                        finally
                        {
                            if (FileStreamTemp != null) FileStreamTemp.Dispose();
                        }
                    }
                    if (ImageBytes == null | ImageBytes.Length == 0) return;
                    Preview_RowHolder["Info_MediaByteLength"] = ImageBytes.Length;

                    // Get MD5
                    using (MD5CryptoServiceProvider MD5Provider = new MD5CryptoServiceProvider())
                    {
                        byte[] byteHash = MD5Provider.ComputeHash(ImageBytes);
                        string MD5String = "";
                        foreach (byte Hash in byteHash)
                        {
                            MD5String += Hash.ToString("X2").ToLower(); //X2 becase it prints Hexadecimal https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx
                        }
                        MD5Provider.Dispose();
                        Preview_RowHolder["Info_MediaMD5"] = MD5String;
                    }
                    break;
                }
            }
        }

        private void CheckMD5()
        {
            string MD5Check = Module_e621Info.e621InfoDownload("https://e621.net/posts.json?md5=" + Preview_RowHolder["Info_MediaMD5"].ToString());
            if (MD5Check != null && MD5Check.Length > 24)
            {
                JObject MD5CheckJSON = JObject.Parse(MD5Check);
                Preview_RowHolder["Uploaded_As"] = MD5CheckJSON["post"]["id"].Value<string>();
                Module_DB.DB_CreateMediaRecord(ref Preview_RowHolder);
                Label_AlreadyUploaded.Text = string.Format("Already uploaded as #{0}", (string)Preview_RowHolder["Info_MediaMD5"]);
                e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                if (e6_GridItemTemp != null)
                {
                    e6_GridItemTemp.cLabel_isUploaded.Text = MD5CheckJSON["post"]["id"].Value<string>();
                }
            }
        }

        public void AutoTags()
        {
            List<string> CurrentTags = new List<string>();
            CurrentTags.AddRange(Preview_RowHolder["Upload_Tags"].ToString().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            CurrentTags = CurrentTags.Distinct().ToList();
            string animated_tag = "";
            // /// = = = = = Check if GIF is animated

            if (!CurrentTags.Contains("animated"))
            {
                if (((string)Preview_RowHolder["Grab_MediaURL"]).Contains("ugoira"))
                {
                    animated_tag = " animated no_sound webm";
                }
                else
                {
                    switch ((string)Preview_RowHolder["Info_MediaFormat"])
                    {
                        case "gif":
                            {
                                string ImageName = Module_Downloader.GetMediasFileNameOnly((string)Preview_RowHolder["Grab_MediaURL"]);
                                byte[] bytes = File.ReadAllBytes(Module_Downloader.IEDownload_Cache[ImageName]); // File.ReadAllBytes(Preview_RowHolder("Image_FilePath"))
                                using (MemoryStream TempStream = new MemoryStream(bytes))
                                {
                                    Image gif = Image.FromStream(TempStream);
                                    int frameCount = gif.GetFrameCount(new FrameDimension(gif.FrameDimensionsList[0]));
                                    if (frameCount > 1)
                                    {
                                        animated_tag = " animated no_sound";
                                    }
                                    gif.Dispose();
                                }
                                break;
                            }

                        case "swf":
                        case "mp4":
                            {
                                animated_tag = " animated webm";
                                break;
                            }
                    }
                }

            }

            // /// = = = = = Add tags regarding image size
            string ratio_tag = "";
            string resolution_tag = "";
            HtmlDocument WebBrowserDocument = null;
            //Invoke(new Action(() => WebBrowserDocument = Pic_WebBrowser.Document));
            WebBrowserDocument = Pic_WebBrowser.Document;
            int ImageWidth = WebBrowserDocument.Images[0].ClientRectangle.Size.Width;
            int ImageHeight = WebBrowserDocument.Images[0].ClientRectangle.Size.Height;
            if (ImageWidth == ImageHeight)
            {
                if (ImageWidth > 15000)
                {
                    return;
                }

                ratio_tag = " 1:1";
                switch (ImageWidth)
                {
                    case int @case when @case >= 10000:
                        {
                            resolution_tag = " superabsurd_res";
                            break;
                        }

                    case int case1 when case1 >= 3200:
                        {
                            resolution_tag = " absurd_res";
                            break;
                        }

                    case int case2 when case2 >= 1600:
                        {
                            resolution_tag = " hi_res";
                            break;
                        }

                    case int case3 when case3 <= 500:
                        {
                            resolution_tag = " low_res";
                            break;
                        }
                }
            }
            else
            {
                int size_bigger = Math.Max(ImageWidth, ImageHeight);
                if (size_bigger > 15000)
                {
                    return;
                }

                int size_smaller = Math.Min(ImageWidth, ImageHeight);
                double size_ratio = (double)size_bigger / size_smaller;
                bool ReverseRatio = ImageWidth < ImageHeight;
                switch (size_ratio)
                {
                    case 4d / 3d:
                        {
                            ratio_tag = ReverseRatio ? " 3:4" : " 4:3";
                            break;
                        }

                    case 16d / 9d:
                        {
                            ratio_tag = ReverseRatio ? " 9:16" : " 16:9";
                            break;
                        }

                    case 16d / 10d:
                        {
                            ratio_tag = ReverseRatio ? " 10:16 5:8" : " 16:10 8:5";
                            break;
                        }

                    case 18d / 9d:
                        {
                            ratio_tag = ReverseRatio ? " 9:18 1:2" : " 18:9 2:1";
                            break;
                        }

                    case 21d / 9d:
                        {
                            ratio_tag = ReverseRatio ? " 9:21 3:7" : " 21:9 7:3";
                            break;
                        }

                    case 36d / 10d:
                        {
                            ratio_tag = ReverseRatio ? " 10:36" : " 36:10";
                            break;
                        }

                    case 5d / 4d:
                        {
                            ratio_tag = ReverseRatio ? " 4:5" : " 5:4";
                            break;
                        }

                    case 3d / 2d:
                        {
                            ratio_tag = ReverseRatio ? " 2:3" : " 3:2";
                            break;
                        }

                    case 3d / 1d:
                        {
                            ratio_tag = ReverseRatio ? " 1:3" : " 3:1";
                            break;
                        }
                }

                if (ImageWidth > ImageHeight)
                {
                    switch (ImageWidth)
                    {
                        case int @case when @case >= 10000:
                            {
                                resolution_tag = " superabsurd_res";
                                break;
                            }

                        case int @case when @case >= 3200:
                            {
                                resolution_tag = " absurd_res";
                                break;
                            }

                        case int @case when @case >= 1600:
                            {
                                resolution_tag = " hi_res";
                                break;
                            }

                        case int @case when @case <= 500:
                            {
                                resolution_tag = " low_res";
                                break;
                            }
                    }
                }
                else
                {
                    switch (ImageHeight)
                    {
                        case int @case when @case >= 10000:
                            {
                                resolution_tag = " superabsurd_res";
                                break;
                            }

                        case int @case when @case >= 2400:
                            {
                                resolution_tag = " absurd_res";
                                break;
                            }

                        case int @case when @case >= 1200:
                            {
                                resolution_tag = " hi_res";
                                break;
                            }

                        case int @case when @case <= 500:
                            {
                                resolution_tag = " low_res";
                                break;
                            }
                    }
                }
            }

            // SoFurry is DBNull, so fix that
            string TestString = animated_tag + ratio_tag + resolution_tag;
            if (Preview_RowHolder["Upload_Tags"] == null && TestString != null)
            {
                Preview_RowHolder["Upload_Tags"] = "";
            }

            if (!animated_tag.Equals("") && !CurrentTags.Contains(animated_tag))
            {
                Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + animated_tag;
            }

            if (!((string)Preview_RowHolder["Info_MediaFormat"]).Equals("mp4"))
            {
                if (!ratio_tag.Equals("") && !CurrentTags.Contains(ratio_tag))
                {
                    Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + ratio_tag;
                }

                if (!resolution_tag.Equals("") && !CurrentTags.Contains(resolution_tag))
                {
                    Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + resolution_tag;
                }
            }

            string YearTag = ((DateTime)Preview_RowHolder["Grab_DateTime"]).Year.ToString();
            if (!CurrentTags.Contains(YearTag))
            {
                Preview_RowHolder["Upload_Tags"] = YearTag + " " + (string)Preview_RowHolder["Upload_Tags"];
            }

            if (Form_Tagger._FormReference != null)
            {
                BeginInvoke(new Action(() =>
                {
                    int CursorLocation = Form_Tagger._FormReference.textBox_Tags.Text.Length - Form_Tagger._FormReference.textBox_Tags.SelectionStart;
                    Form_Tagger._FormReference.textBox_Tags.Text = string.Format("{0}{1}{2} {3}", ratio_tag, animated_tag, resolution_tag, Form_Tagger._FormReference.textBox_Tags.Text);
                    Form_Tagger._FormReference.textBox_Tags.Text = Form_Tagger._FormReference.textBox_Tags.Text.TrimStart();
                    Form_Tagger._FormReference.textBox_Tags.SelectionStart = Form_Tagger._FormReference.textBox_Tags.Text.Length - CursorLocation;
                }));
            }
        }

        private void PB_Navigation_Click(object sender, EventArgs e)
        {
            panel_Navigation.Focus();
            if (Form_Tagger._FormReference != null)
            {
                Form_Tagger._FormReference.Close();
            }

            Button_BrowserSmall SenderButton = (Button_BrowserSmall)sender;

            Preview_RowIndex += int.Parse(SenderButton.Tag.ToString());
            Preview_RowHolder = Module_TableHolder.Database_Table.Rows[Preview_RowIndex];

            string Nav2Url = (string)Preview_RowHolder["Grab_MediaURL"];
            if (Nav2Url.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || Nav2Url.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
            {
                Nav2Url = (string)Preview_RowHolder["Grab_ThumbnailURL"];
            }
            NavURL(Nav2Url);
        }

        public void UpdateNavButtons()
        {
            PB_Previous.Enabled = !(Preview_RowIndex == 0);
            PB_Previous.BackColor = PB_Previous.Enabled ? Color.FromArgb(0, 45, 90) : Color.DimGray;
            PB_Next.Enabled = !(Preview_RowIndex == Preview_RowHolder.Table.Rows.Count - 1);
            PB_Next.BackColor = PB_Next.Enabled ? Color.FromArgb(0, 45, 90) : Color.DimGray;
        }

        private void PB_Rating_Click(object sender, EventArgs e)
        {
            panel_Rating.Focus();

            Button_BrowserSmall SenderButton = (Button_BrowserSmall)sender;
            Preview_RowHolder["Upload_Rating"] = SenderButton.Text;

            e6_GridItem GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
            if (GridItemTemp != null)
            {
                GridItemTemp._Rating = SenderButton.Text;
            }
            UpdateButtons();
        }

        private void PB_Tagger_Click(object sender, EventArgs e)
        {
            panel_Rating.Focus();
            Open_Tagger(Preview_RowHolder);
        }

        public Point TaggerLocator;
        public void Open_Tagger(DataRow RowRef)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                TaggerLocator = new Point(0, 0);
            }
            Form_Tagger.OpenTagger(this, RowRef, TaggerLocator);
        }

        private void PB_Upload_Click(object sender, EventArgs e)
        {
            panel_Rating.Focus();
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp.cCheckBox_UPDL.Checked = !e6_GridItemTemp.cCheckBox_UPDL.Checked;
            }
            else
            {
                Preview_RowHolder["UPDL_Queued"] = !(bool)Preview_RowHolder["UPDL_Queued"];
                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(Preview_RowHolder, !(PB_Upload.BackColor == Color.LimeGreen)))
                {
                    // e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                }
                else
                {
                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 1 : -1;
                }

                if (!Properties.Settings.Default.API_Key.Equals(""))
                {
                    Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                }
                Form_Loader._FormReference.DownloadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 1 : -1;
                Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;
                UpdateButtons();
            }
        }

        private void PB_Download_Click(object sender, EventArgs e)
        {
            flowLayoutPanel_DL.Focus();
            PB_Download.Enabled = false;
            PB_Download.Visible = false;
            string URL = (string)Preview_RowHolder["Grab_MediaURL"];
            string ImageName = URL.Substring(URL.LastIndexOf("/") + 1);
            if (ImageName.Contains("ugoira"))
            {
                Label_DownloadWarning.Visible = false;
                Add2ConversionQueue(ref Preview_RowHolder);
            }
            else
            {
                if (ImageName.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || ImageName.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                {
                    Label_DownloadWarning.Visible = false;
                    Add2ConversionQueue(ref Preview_RowHolder);
                }
                else
                {
                    Module_Downloader.ReSaveMedia(Preview_RowHolder);
                    PB_ViewFile.Visible = true;
                    PB_ViewFile.Text = "🔍";
                }
            }
        }

        private void Add2ConversionQueue(ref DataRow DataRowRef)
        {
            TreeNode TempTreeNode = new TreeNode()
            {
                Text = (string)DataRowRef["Grab_MediaURL"],
                Name = (string)DataRowRef["Grab_MediaURL"],
                Tag = DataRowRef,
                ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_Conversion
            };
            Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes.Add(TempTreeNode);
            Module_Converter.timer_Conversion.Enabled = Form_Loader._FormReference.cCheckGroupBox_Convert.Checked && !Module_Converter.Conversion_BGW.IsBusy;
        }

        private void PB_ViewFile_Click(object sender, EventArgs e)
        {
            flowLayoutPanel_DL.Focus();
            string FilePath = (string)Preview_RowHolder["DL_FilePath"];
            if (File.Exists(FilePath))
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    Process.Start("explorer.exe", "/select," + FilePath);
                }
                else
                {
                    Process.Start(FilePath);
                }
            }
            else
            {
                PB_Download.Visible = true;
                PB_Download.Enabled = true;
                Label_DownloadWarning.Visible = true;
                PB_ViewFile.Visible = false;
                Preview_RowHolder["DL_FilePath"] = DBNull.Value;
            }
        }

        private void PB_ViewFile_VisibleChanged(object sender, EventArgs e)
        {
            flowLayoutPanel_DL.Focus();
            if (PB_ViewFile.Visible)
            {
                Label_DownloadWarning.Visible = false;
                PB_Download.Visible = false;
                Label_Download.Visible = false;
            }
        }

        private void PB_SimilarSearch_Click(object sender, EventArgs e)
        {
            panel_Search.Focus();
            Button SenderButton = (Button)sender;
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (Form_e6Post._FormReference == null)
                {
                    new Form_e6Post(SenderButton.PointToScreen(Point.Empty), this);
                }
                Form_e6Post._FormReference.Show();
                Form_e6Post._FormReference.BringToFront();
            }
            else
            {
                new Form_SimilarSearch(SenderButton.Text, SenderButton.PointToScreen(Point.Empty), this);
                Form_SimilarSearch._FormReference.ShowDialog();
            }
        }

        private void PB_IQDBQ_Click(object sender, EventArgs e)
        {
            panel_Search.Focus();
        }

        private bool LoadAllImagesMod;
        private void PB_LoadAllImages_Click(object sender, EventArgs e)
        {
            panel_Navigation.Focus();
            if (LoadAllImagesMod)
            {
                PB_LoadAllImages.ForeColor = Color.LightSteelBlue;
                panel_Navigation.Enabled = true;
                PB_Tagger.Enabled = true;
                UpdateNavButtons(); // color doesn't change automaticelly when panel is enabled.
                LoadAllImagesMod = false;
            }
            else
            {
                PB_LoadAllImages.ForeColor = Color.Red;
                panel_Navigation.Enabled = false;
                PB_Tagger.Enabled = false;
                LoadAllImagesMod = true;
                PB_Navigation_Click(PB_Next, null);
            }
        }

        private void Label_AlreadyUploaded_TextChanged(object sender, EventArgs e)
        {
            Label_AlreadyUploaded.Visible = Label_AlreadyUploaded.Text.Length > 0;
        }

        private void Label_AlreadyUploaded_Click(object sender, EventArgs e)
        {
            string E6Post = "https://e621.net/post/show/" + Label_AlreadyUploaded.Text.Substring(Label_AlreadyUploaded.Text.IndexOf("#") + 1);
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                Process.Start(E6Post);
                // Fix keyup not triggering when losing focus
                base.KeyDown += Form_Preview_KeyDown;
            }
            else
            {
                Form_Loader._FormReference.BringToFront();
                Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = 0;
                if (!Module_CefSharp.CefSharpBrowser.Address.Equals(E6Post))
                {
                    Module_CefSharp.CefSharpBrowser.Load(E6Post);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Left:
                    {
                        PB_Previous.PerformClick();
                        return true;
                    }

                case Keys.Down:
                case Keys.Right:
                    {
                        PB_Next.PerformClick();
                        return true;
                    }

                case Keys.E:
                    {
                        PB_Explicit.PerformClick();
                        return true;
                    }

                case Keys.Q:
                    {
                        PB_Questionable.PerformClick();
                        return true;
                    }

                case Keys.S:
                    {
                        PB_Safe.PerformClick();
                        return true;
                    }

                case Keys.T:
                    {
                        PB_Tagger.PerformClick();
                        return true;
                    }

                case Keys.OemMinus:
                case Keys.Subtract:
                case Keys.NumPad0:
                case Keys.D0:
                    {
                        e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                        if (e6_GridItemTemp != null)
                        {
                            e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                        }
                        else
                        {
                            if (PB_Upload.BackColor == Color.LimeGreen)
                            {
                                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(Preview_RowHolder, false))
                                {
                                    // cCheckBox_UPDL.Checked = false;
                                }
                                else
                                {
                                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? -1 : 0;
                                }
                                if (!Properties.Settings.Default.API_Key.Equals(""))
                                {
                                    Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                                }
                                Form_Loader._FormReference.DownloadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? -1 : 0;
                                Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;
                                Preview_RowHolder["UPDL_Queued"] = false;
                                PB_Upload.BackColor = Color.FromArgb(0, 45, 90);
                            }
                        }
                        return true;
                    }

                case Keys.Oemplus:
                case Keys.Add:
                case Keys.NumPad1:
                case Keys.D1:
                    {
                        e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                        if (e6_GridItemTemp != null)
                        {
                            e6_GridItemTemp.cCheckBox_UPDL.Checked = true;
                        }
                        else
                        {
                            if (PB_Upload.BackColor != Color.LimeGreen)
                            {
                                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(Preview_RowHolder))
                                {
                                    // cCheckBox_UPDL.Checked = false;
                                }
                                else
                                {
                                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 0 : 1;
                                }
                                if (!Properties.Settings.Default.API_Key.Equals(""))
                                {
                                    Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                                }
                                Form_Loader._FormReference.DownloadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 0 : 1;
                                Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;
                                Preview_RowHolder["UPDL_Queued"] = true;
                                PB_Upload.BackColor = Color.LimeGreen;
                            }
                        }

                        return true;
                    }

                case Keys.D:
                    {
                        PB_Download.PerformClick();
                        return true;
                    }

                case Keys.F:
                    {
                        PB_SauceNao.PerformClick();
                        return true;
                    }

                case Keys.I:
                    {
                        PB_IQDBQ.PerformClick();
                        return true;
                    }
            }
            return false;
        }

        private void Button_GotFocus(object sender, EventArgs e)
        {
            panel_Rating.Focus();
        }

        private void Form_Preview_FormClosed(object sender, FormClosedEventArgs e)
        {
            _FormReference = null;
            Form_Loader._FormReference.Activate(); // Focus back the Main Form
        }

    }
}
