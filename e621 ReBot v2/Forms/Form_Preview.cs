using CefSharp;
using CefSharp.WinForms;
using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_Preview : Form
    {
        public static Form_Preview _FormReference;
        public Form_Preview()
        {
            InitializeComponent();
            _FormReference = this;

            MediaBrowser = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
                RequestHandler = new PicBrowser_RequestHandler(),
                MenuHandler = new MediaBrowser_MenuHandler(),
                UseParentFormMessageInterceptor = false
            };
            panel_PicBrowserHolder.Controls.Add(MediaBrowser);
            //MediaBrowser.AddressChanged += MediaBrowser_AddressChanged;
            MediaBrowser.LoadingStateChanged += MediaBrowser_LoadingStateChanged;
            MediaBrowser.TitleChanged += MediaBrowser_TitleChanged;
            //MediaBrowser.FrameLoadEnd += MediaBrowser_FrameLoadEnd;

            foreach (Button ButtonTemp in panel_PalleteHolder.Controls)
            {
                ButtonTemp.Click += SetBrowserColour;
                ButtonTemp.GotFocus += Button_GotFocus;
            }

            GotFocus += Form_Preview_GotFocus;
            foreach (Button ButtonTemp in panel_Navigation.Controls)
            {
                ButtonTemp.Click += PB_Navigation_Click;
                ButtonTemp.GotFocus += Button_GotFocus;
            }
            foreach (Button ButtonTemp in panel_Rating.Controls)
            {
                ButtonTemp.Click += PB_Rating_Click;
                ButtonTemp.GotFocus += Button_GotFocus;
            }

            foreach (Button ButtonTemp in panel_Search.Controls)
            {
                ButtonTemp.Click += PB_SimilarSearch_Click;
                ButtonTemp.GotFocus += Button_GotFocus;
            }

            timer_LoadAllDelay = new Timer();
            timer_LoadAllDelay.Tick += Timer_LoadAllDelay_Tick;
        }

        public readonly ChromiumWebBrowser MediaBrowser;
        public DataRow Preview_RowHolder;
        public int Preview_RowIndex;
        private void Form_Preview_Load(object sender, EventArgs e)
        {
            Preview_RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(Preview_RowHolder);
            Label_AlreadyUploaded.Cursor = Form_Loader.Cursor_ReBotNav;
            PB_IQDBQ.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.API_Key);
        }

        public string URL2Navigate4Start;
        private void Form_Preview_Shown(object sender, EventArgs e)
        {
            timer_NavDelay.Start();
        }

        private void Form_Preview_Resize(object sender, EventArgs e)
        {
            ResizeForm();
        }

        private void Form_Preview_ResizeEnd(object sender, EventArgs e)
        {
            ResizeForm();
        }

        private void ResizeForm()
        {
            // Actual pixel size is -14 width, -7 height compared to design size, -16, -9 internal
            // Title bar height is 30px
            panel_PicBrowserHolder.Height = Height - 96;
            Label_Tags.Width = panel_PicBrowserHolder.Width;
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
            KeyDown -= Form_Preview_KeyDown;
        }

        private void Form_Preview_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 18) //Keys.Alt
            {
                Label_AlreadyUploaded.Cursor = Form_Loader.Cursor_ReBotNav;
            }
            KeyDown += Form_Preview_KeyDown;
        }

        private void SetBrowserColour(object sender, EventArgs e)
        {
            if (!MediaBrowser.IsLoading)
            {
                MediaBrowser.ExecuteScriptAsyncWhenPageLoaded($"document.body.style.background = '{((Button)sender).BackColor.Name}';");
            }
        }

        private void Timer_NavDelay_Tick(object sender, EventArgs e)
        {
            timer_NavDelay.Stop();
            NavURL(URL2Navigate4Start);
            ResizeEnd += Form_Preview_ResizeEnd;
            Resize += Form_Preview_Resize;
        }

        public void NavURL(string URL2Navigate)
        {
            Text = "Preview";
            panel_Navigation.Enabled = false;
            foreach (Control ControlTemp in flowLayoutPanel_DL.Controls)
            {
                ControlTemp.Visible = false;
            }
            PB_Upload.Enabled = false;
            panel_Search.Enabled = false;
            Label_AlreadyUploaded.Text = null;
            Label_Tags.Text = null;

            string ImageURL = (string)Preview_RowHolder["Grab_MediaURL"];
            string ImageName = Module_Downloader.GetMediasFileNameOnly(ImageURL);

            if (Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes.ContainsKey(ImageURL))
            {
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
                Label_Download.Visible = true;
            }
            else
            {
                PB_Download.Visible = Module_Downloader.Download_AlreadyDownloaded.Contains(ImageURL);
                if (!PB_Download.Visible)
                {
                    if (Preview_RowHolder["DL_FilePath"] != DBNull.Value && File.Exists((string)Preview_RowHolder["DL_FilePath"]))
                    {
                        if (ImageName.Contains("ugoira")
                        || ImageName.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                        || ImageName.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                        {
                            PB_ViewFile.Text = "▶";
                        }
                        else
                        {
                            PB_ViewFile.Text = "🔍";
                        }
                        PB_ViewFile.Visible = true;
                    }
                    else
                    {
                        PB_Download.Visible = true;
                        lock (Module_Downloader.Download_AlreadyDownloaded)
                        {
                            Module_Downloader.Download_AlreadyDownloaded.Remove(ImageName);
                        }
                        Preview_RowHolder["DL_FilePath"] = DBNull.Value;
                    }
                }
                if (PB_Download.Visible)
                {
                    if (ImageName.Contains("ugoira"))
                    {
                        Label_DownloadWarning.Visible = true;
                        toolTip_Display.SetToolTip(Label_DownloadWarning, $"This is an Ugoira, you need to download it in order to view the animated version.");
                    }
                }
            }

            MediaBrowser.Load(URL2Navigate);
        }

        private void MediaBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (MediaBrowser.Address.Equals("about:blank"))
            {
                return;
            }

            if (!LoadAllImagesMod)
            {
                Invoke(new Action(() =>
                {
                    panel_Navigation.Enabled = true;
                    UpdateNavButtons();
                    UpdateButtons();
                }));
            }
            if (e.IsLoading)
            {
                Invoke(new Action(() => { timer_LoadDoneDelay.Stop(); }));
            }
            else
            {
                MediaBrowser.ExecuteScriptAsyncWhenPageLoaded("document.body.style.background = 'dimGray';");

                Invoke(new Action(() => { timer_LoadDoneDelay.Start(); }));
            }
        }

        private void MediaBrowser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            DocumentTitle = e.Title;
        }

        private string DocumentTitle;
        private void Timer_LoadDoneDelay_Tick(object sender, EventArgs e)
        {
            timer_LoadDoneDelay.Stop();
            string MediaFormat = (string)Preview_RowHolder["Info_MediaFormat"];
            switch (MediaFormat)
            {
                case "ugoira":
                    {
                        Text = string.Format("Preview ({0}) - Ugoira", Preview_RowIndex + 1);
                        AutoTags();
                        break;
                    }

                case "mp4":
                case "swf":
                    {
                        Text = string.Format("Preview ({0}) - .{1} ({2:N2} kB)   ", Preview_RowIndex + 1, MediaFormat, (int)Preview_RowHolder["Info_MediaByteLength"] / 1024d);
                        if (Preview_RowHolder["Info_MediaMD5"] == DBNull.Value)
                        {
                            if (Preview_RowHolder["DL_FilePath"] != DBNull.Value && File.Exists((string)Preview_RowHolder["DL_FilePath"]))
                            {
                                byte[] MediaBytes = null;
                                for (int numTries = 0; numTries < 3; numTries++)
                                {
                                    try
                                    {
                                        MediaBytes = File.ReadAllBytes((string)Preview_RowHolder["DL_FilePath"]);
                                        break;
                                    }
                                    catch (IOException)
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                if (MediaBytes == null || MediaBytes.Length == 0)
                                {
                                    return;
                                }

                                // Get MD5
                                using (MD5CryptoServiceProvider MD5Provider = new MD5CryptoServiceProvider())
                                {
                                    MediaBytes = MD5Provider.ComputeHash(MediaBytes);
                                    StringBuilder MD5String = new StringBuilder();
                                    foreach (byte HashByte in MediaBytes)
                                    {
                                        MD5String.Append(HashByte.ToString("X2").ToLower()); //X2 becase it prints Hexadecimal https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx
                                    }
                                    Preview_RowHolder["Info_MediaMD5"] = MD5String;
                                    Text += $"   [MD5: {MD5String}]";
                                }
                            }
                        }
                        AutoTags();
                        if (Preview_RowHolder["Info_MediaMD5"] != DBNull.Value)  CheckMD5();
                        break;
                    }

                default:
                    {
                        if (Preview_RowHolder["Info_MediaMD5"] == DBNull.Value)
                        {
                            MatchCollection ImageResolution = Regex.Matches(DocumentTitle, @"\((\d+)×(\d+)\)");
                            Preview_RowHolder["Info_MediaWidth"] = int.Parse(ImageResolution[0].Groups[1].Value);
                            Preview_RowHolder["Info_MediaHeight"] = int.Parse(ImageResolution[0].Groups[2].Value);

                            GetCachedImage();
                            CheckMD5();
                            AutoTags();
                            Preview_RowHolder["Info_TooBig"] = Module_Uploader.Media2BigCheck(ref Preview_RowHolder);

                            //if (((string)Preview_RowHolder["Grab_ThumbnailURL"]).EndsWith(".webp", StringComparison.OrdinalIgnoreCase)) //more custom handling for webp
                            //{
                            //    CachedImagePath = Module_Downloader.IEDownload_Cache[Module_Downloader.GetMediasFileNameOnly((string)Preview_RowHolder["Grab_MediaURL"])];
                            //    Preview_RowHolder["Thumbnail_Image"] = Module_Grabber.MakeImageThumb(Image.FromFile(CachedImagePath));
                            //    if (GridItemTemp != null)
                            //    {
                            //        GridItemTemp.LoadImage();
                            //    }
                            //}
                        }

                        if (Preview_RowHolder["Thumbnail_FullInfo"] == DBNull.Value)
                        {
                            Preview_RowHolder["Thumbnail_FullInfo"] = true;

                            //string CachedImagePath = Module_Downloader.IEDownload_Cache[Module_Downloader.GetMediasFileNameOnly(MediaURL)];
                            //if (Preview_RowHolder["Grab_ThumbnailURL"] == DBNull.Value)
                            //{
                            //    Preview_RowHolder["Grab_ThumbnailURL"] = null; //Just to make it non-dbnull
                            //    Preview_RowHolder["Thumbnail_Image"] = Module_Grabber.MakeImageThumb(Image.FromFile(CachedImagePath));
                            //}

                            e6_GridItem GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                            if (GridItemTemp == null)
                            {
                                if (Preview_RowHolder["Thumbnail_Image"] == DBNull.Value && Preview_RowHolder["Thumbnail_DLStart"] == DBNull.Value)
                                {
                                    string CachedImagePath = Module_Downloader.MediaBrowser_MediaCache[Module_Downloader.GetMediasFileNameOnly((string)Preview_RowHolder["Grab_MediaURL"])];
                                    Preview_RowHolder["Thumbnail_Image"] = Module_Grabber.MakeImageThumb(Image.FromFile(CachedImagePath));
                                }
                                Module_Grabber.WriteImageInfo(Preview_RowHolder);
                            }
                            else
                            {
                                GridItemTemp.LoadImage();
                            }
                        }
                        Text = string.Format("Preview ({0}) - {1}×{2}.{3} ({4:N2} kB)   [MD5: {5}]", Preview_RowIndex + 1, (int)Preview_RowHolder["Info_MediaWidth"], (int)Preview_RowHolder["Info_MediaHeight"], (string)Preview_RowHolder["Info_MediaFormat"], (int)Preview_RowHolder["Info_MediaByteLength"] / 1024d, (string)Preview_RowHolder["Info_MediaMD5"]);
                        break;
                    }
            }
            PB_Upload.Enabled = true;
            panel_Search.Enabled = Preview_RowHolder["Uploaded_As"] == DBNull.Value;
            Label_AlreadyUploaded.Text = Preview_RowHolder["Uploaded_As"] == DBNull.Value ? null : string.Format("Already uploaded as #{0}", Preview_RowHolder["Uploaded_As"]);
            Label_Tags.Text = (string)Preview_RowHolder["Upload_Tags"];
            if (Preview_RowHolder["Info_TooBig"] == DBNull.Value)
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
                    DateTime DateTimeNowTemp = DateTime.UtcNow;
                    TimeSpan TimeSpanTemp = DateTimeNowTemp - LastTickTime;
                    if (TimeSpanTemp.TotalMilliseconds > 500)
                    {
                        PB_Navigation_Click(PB_Next, null);
                    }
                    else
                    {
                        LastTickTime = DateTimeNowTemp;
                        timer_LoadAllDelay.Interval = (int)Math.Max(100, 500 - TimeSpanTemp.TotalMilliseconds);
                        timer_LoadAllDelay.Start();
                    }
                    return;
                }
            }
            else
            {
                panel_Navigation.Enabled = true;
            }
            PB_LoadAllImages.Enabled = !(Preview_RowIndex == Module_TableHolder.Database_Table.Rows.Count - 1);
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
            string MediaName = (string)Preview_RowHolder["Grab_MediaURL"];
            MediaName = Module_Downloader.GetMediasFileNameOnly(MediaName, in Preview_RowHolder);
            byte[] MediaBytes = null;
            for (int numTries = 0; numTries < 10; numTries++)
            {
                try
                {
                    MediaBytes = File.ReadAllBytes(Module_Downloader.MediaBrowser_MediaCache[MediaName]);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                }
            }
            if (MediaBytes == null || MediaBytes.Length == 0)
            {
                return;
            }

            // Get MD5
            using (MD5CryptoServiceProvider MD5Provider = new MD5CryptoServiceProvider())
            {
                MediaBytes = MD5Provider.ComputeHash(MediaBytes);
                StringBuilder MD5String = new StringBuilder();
                foreach (byte HashByte in MediaBytes)
                {
                    MD5String.Append(HashByte.ToString("X2").ToLower()); //X2 becase it prints Hexadecimal https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5.aspx
                }
                Preview_RowHolder["Info_MediaMD5"] = MD5String;
            }
        }

        private void CheckMD5()
        {
            string MD5Check = Module_e621Info.e621InfoDownload($"https://e621.net/posts.json?md5={(string)Preview_RowHolder["Info_MediaMD5"]}");
            if (MD5Check != null && MD5Check.Length > 24)
            {
                JObject MD5CheckJSON = JObject.Parse(MD5Check);
                Preview_RowHolder["Uploaded_As"] = MD5CheckJSON["post"]["id"].Value<string>();
                Preview_RowHolder["Upload_Rating"] = MD5CheckJSON["post"]["rating"].Value<string>().ToUpper();
                List<string> TagList = new List<string>();
                foreach (JProperty pTag in MD5CheckJSON["post"]["tags"].Children())
                {
                    foreach (JToken cTag in pTag.First)
                    {
                        TagList.Add(cTag.Value<string>());
                    }
                };
                Preview_RowHolder["Upload_Tags"] = string.Join(" ", TagList);
                Module_DB.DB_Media_CreateRecord(ref Preview_RowHolder);
                Label_AlreadyUploaded.Text = $"Already uploaded as #{(string)Preview_RowHolder["Info_MediaMD5"]}";
                e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Preview_RowHolder);
                if (e6_GridItemTemp != null)
                {
                    e6_GridItemTemp.cLabel_isUploaded.Text = MD5CheckJSON["post"]["id"].Value<string>();
                }
            }
        }

        public void AutoTags()
        {
            string MediaFormat = (string)Preview_RowHolder["Info_MediaFormat"];
            List<string> CurrentTags = new List<string>();
            CurrentTags.AddRange(Preview_RowHolder["Upload_Tags"].ToString().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            CurrentTags = CurrentTags.Distinct().ToList();
            string animated_tag = null;

            // /// = = = = = Check if GIF is animated
            if (CurrentTags.Contains("animated"))
            {
                animated_tag = string.Empty;
            }
            else
            {
                switch (MediaFormat)
                {
                    case "ugoira":
                        {
                            animated_tag = " animated no_sound webm";
                            break;
                        }

                    case "gif":
                        {
                            byte[] bytes = File.ReadAllBytes(Module_Downloader.MediaBrowser_MediaCache[Module_Downloader.GetMediasFileNameOnly((string)Preview_RowHolder["Grab_MediaURL"])]); // File.ReadAllBytes(Preview_RowHolder("Image_FilePath"))
                            using (MemoryStream TempStream = new MemoryStream(bytes))
                            {
                                using (Image gif = Image.FromStream(TempStream))
                                {
                                    int frameCount = gif.GetFrameCount(new FrameDimension(gif.FrameDimensionsList[0]));
                                    if (frameCount > 1)
                                    {
                                        animated_tag = " animated no_sound";
                                    }
                                }
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

            string ratio_tag = null;
            string resolution_tag = null;
            if (animated_tag == null)
            {
                // = = = = = Add tags regarding image size
                int ImageWidth = (int)Preview_RowHolder["Info_MediaWidth"];
                int ImageHeight = (int)Preview_RowHolder["Info_MediaHeight"];
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
            }

            if (!(string.IsNullOrEmpty(animated_tag) || CurrentTags.Contains(animated_tag)))
            {
                Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + animated_tag;
            }

            if (!(MediaFormat.Equals("mp4") || MediaFormat.Equals("swf")))
            {
                if (!string.IsNullOrEmpty(ratio_tag) && !CurrentTags.Contains(ratio_tag))
                {
                    Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + ratio_tag;
                }

                if (!string.IsNullOrEmpty(resolution_tag) && !CurrentTags.Contains(resolution_tag))
                {
                    Preview_RowHolder["Upload_Tags"] = (string)Preview_RowHolder["Upload_Tags"] + resolution_tag;
                }
            }

            if (Preview_RowHolder["Grab_DateTime"] != DBNull.Value)
            {
                string YearTag = ((DateTime)Preview_RowHolder["Grab_DateTime"]).Year.ToString();
                if (!CurrentTags.Contains(YearTag))
                {
                    Preview_RowHolder["Upload_Tags"] = YearTag + " " + (string)Preview_RowHolder["Upload_Tags"];
                }
            }

            if (Form_Tagger._FormReference != null)
            {
                int CursorLocation = Form_Tagger._FormReference.textBox_Tags.Text.Length - Form_Tagger._FormReference.textBox_Tags.SelectionStart;
                Form_Tagger._FormReference.textBox_Tags.Text = string.Format("{0}{1}{2} {3}", ratio_tag, animated_tag, resolution_tag, Form_Tagger._FormReference.textBox_Tags.Text);
                Form_Tagger._FormReference.textBox_Tags.Text = Form_Tagger._FormReference.textBox_Tags.Text.TrimStart();
                Form_Tagger._FormReference.textBox_Tags.SelectionStart = Form_Tagger._FormReference.textBox_Tags.Text.Length - CursorLocation;
            }
        }

        private void PB_Navigation_Click(object sender, EventArgs e)
        {
            timer_LoadDoneDelay.Stop();
            panel_Navigation.Focus();
            if (Form_Tagger._FormReference != null)
            {
                Form_Tagger._FormReference.Close();
            }
            Button_BrowserSmall SenderButton = (Button_BrowserSmall)sender;

            int WantedChange = int.Parse(SenderButton.Tag.ToString());
            int WouldBeNewIndex = Preview_RowIndex + WantedChange;

            if (WouldBeNewIndex == -1 || WouldBeNewIndex == Module_TableHolder.Database_Table.Rows.Count) return; //click can be spammed faster than adresschanged event by holding the button
            Preview_RowIndex += WantedChange;
            Preview_RowHolder = Module_TableHolder.Database_Table.Rows[Preview_RowIndex];
            NavURL((string)Preview_RowHolder["Grab_MediaURL"]);
        }

        public void UpdateNavButtons()
        {
            PB_Previous.Enabled = Preview_RowIndex != 0;
            PB_Previous.BackColor = PB_Previous.Enabled ? Color.FromArgb(0, 45, 90) : Color.DimGray;
            PB_Next.Enabled = Preview_RowIndex != Preview_RowHolder.Table.Rows.Count - 1;
            PB_Next.BackColor = PB_Next.Enabled ? Color.FromArgb(0, 45, 90) : Color.DimGray;
        }

        private void PB_Rating_Click(object sender, EventArgs e)
        {
            Label_Tags.Focus();

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
            Label_Tags.Focus();
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
                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(ref Preview_RowHolder, !(PB_Upload.BackColor == Color.LimeGreen)))
                {
                    // e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                }
                else
                {
                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 1 : -1;
                }

                if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
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
            PB_Download.Visible = false;
            switch ((string)Preview_RowHolder["Info_MediaFormat"])
            {
                case "ugoira":
                    {
                        Label_DownloadWarning.Visible = false;
                        Add2ConversionQueue(ref Preview_RowHolder);
                        break;
                    }

                case "mp4":
                    {
                        new Thread(() =>
                        {
                            var DomainURL = new Uri((string)Preview_RowHolder["Grab_URL"]);
                            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
                            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

                            string MediaURL = (string)Preview_RowHolder["Grab_MediaURL"];
                            string VideoFileName = MediaURL.Substring(MediaURL.LastIndexOf("/") + 1);
                            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)Preview_RowHolder["Artist"]).Replace("/", "-"));
                            string FullFilePath = $"{FullFolderPath}\\{VideoFileName}";

                            if (Directory.Exists("ManualDownload")) Directory.Delete("ManualDownload", true);
                            Directory.CreateDirectory("ManualDownload").Attributes = FileAttributes.Hidden;
                            Module_Downloader.FileDownloader((string)Preview_RowHolder["Grab_MediaURL"], "M", "ManualDownload", FullFolderPath, Preview_RowHolder);
                            Directory.Delete("ManualDownload", true);

                            if (_FormReference != null && _FormReference.IsHandleCreated)
                            {
                                _FormReference.BeginInvoke(new Action(() =>
                                {
                                    Label_Download.Visible = false;
                                    PB_ViewFile.Visible = true;
                                    PB_ViewFile.Text = "▶";
                                }));
                            }
                            Preview_RowHolder["DL_FilePath"] = FullFilePath;
                            lock (Module_Downloader.Download_AlreadyDownloaded)
                            {
                                Module_Downloader.Download_AlreadyDownloaded.Add((string)Preview_RowHolder["Grab_MediaURL"]);
                            }
                        }).Start();
                        break;
                    }

                case "swf":
                    {
                        Label_DownloadWarning.Visible = false;
                        Add2ConversionQueue(ref Preview_RowHolder);
                        break;
                    }

                default:
                    {
                        Module_Downloader.ReSaveMedia(ref Preview_RowHolder);
                        PB_ViewFile.Text = "🔍";
                        PB_ViewFile.Visible = true;
                        break;
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
                Preview_RowHolder["DL_FilePath"] = DBNull.Value;
                PB_ViewFile.Visible = false;
                PB_Download.Visible = true;
                Label_DownloadWarning.Visible = true;
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
            if (ModifierKeys.HasFlag(Keys.Control) || ModifierKeys.HasFlag(Keys.Shift))
            {
                string PostIDReturned = null;
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    PostIDReturned = Form_IDForm.Show(this, SenderButton.PointToScreen(Point.Empty), "Enter Post ID", Color.DarkOrange);
                    if (PostIDReturned != null)
                    {
                        InferiorSub(PostIDReturned, Preview_RowHolder);
                    }
                }
                else
                {
                    PostIDReturned = Form_IDForm.Show(this, SenderButton.PointToScreen(Point.Empty), "Enter Post ID");
                    if (PostIDReturned != null)
                    {
                        SuperiorSub(PostIDReturned, Preview_RowHolder);
                    }
                }
            }
            else
            {
                Form_SimilarSearch Form_SimilarSearchTemp = new Form_SimilarSearch(SenderButton.Text, SenderButton.PointToScreen(Point.Empty));
                Form_SimilarSearchTemp.ShowDialog();
            }
        }

        private static void InferiorSub(string PostID, DataRow RowRefference)
        {
            string PostTest = Module_e621Info.e621InfoDownload($"https://e621.net/posts/{PostID}.json", true);
            if (PostTest == null || PostTest.Length < 10)
            {
                MessageBox.Show($"Post with ID#{PostID} does not exist.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JToken PostData = JObject.Parse(PostTest)["post"];
            RowRefference["Upload_Rating"] = PostData["rating"].Value<string>().ToUpper();
            RowRefference["Uploaded_As"] = PostID;
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref RowRefference);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp._Rating = (string)RowRefference["Upload_Rating"];
                e6_GridItemTemp.cLabel_isUploaded.Text = PostID;
            }
            _FormReference.Label_AlreadyUploaded.Text = $"Already uploaded as #{PostID}";
            if (Properties.Settings.Default.ManualInferiorSave)
            {
                Module_DB.DB_Media_CreateRecord(ref RowRefference);
            }
            _FormReference.UpdateButtons();
        }

        public static void SuperiorSub(string PostID, DataRow RowRefference)
        {
            string PostTest = Module_e621Info.e621InfoDownload($"https://e621.net/posts/{PostID}.json", true);
            if (PostTest == null || PostTest.Length < 10)
            {
                MessageBox.Show($"Post with ID#{PostID} does not exist.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JToken PostData = JObject.Parse(PostTest)["post"];
            RowRefference["Upload_Rating"] = PostData["rating"].Value<string>().ToUpper();
            List<string> SortTags = new List<string>();
            foreach (JProperty pTag in PostData["tags"].Children())
            {
                foreach (JToken cTag in pTag.First)
                {
                    SortTags.Add(cTag.Value<string>());
                }
            }
            SortTags.Sort();

            if (!PostData["pools"].ToString().Equals("[]"))
            {
                foreach (JToken pPool in PostData["pools"].Children())
                {
                    SortTags.Add($"pool:{pPool.Value<string>()}");
                }
            }

            string InferiorParentID = PostData["relationships"]["parent_id"].Value<string>();
            if (InferiorParentID != null)
            {
                SortTags.Add($"parent:{InferiorParentID}");
                RowRefference["Inferior_ParentID"] = InferiorParentID;
            }
            RowRefference["Upload_Tags"] = string.Join(" ", SortTags);
            RowRefference["Inferior_ID"] = PostID;
            string InferiorDescription = PostData["description"].Value<string>();

            string CurrentDescriptionConstruct = null;
            if (RowRefference["Grab_TextBody"] == DBNull.Value)
            {
                CurrentDescriptionConstruct = $"[code]{(string)RowRefference["Grab_Title"]}[/code]";
            }
            else
            {
                CurrentDescriptionConstruct = $"[section{(Properties.Settings.Default.ExpandedDescription ? ",expanded" : null)}={(string)RowRefference["Grab_Title"]}]\n{(string)RowRefference["Grab_TextBody"]}\n[/section]";
            }

            if (!string.IsNullOrEmpty(InferiorDescription) && InferiorDescription != CurrentDescriptionConstruct)
            {
                RowRefference["Inferior_Description"] = InferiorDescription;
            }

            if (PostData["sources"].Children().Count() > 0)
            {
                List<string> SourceList = new List<string>();
                foreach (JToken cChild in PostData["sources"])
                {
                    SourceList.Add(cChild.Value<string>());
                }
                RowRefference["Inferior_Sources"] = SourceList;
            }

            if (PostData["relationships"]["has_children"].Value<bool>())
            {
                List<string> ChildList = new List<string>();
                foreach (JToken cChild in PostData["relationships"]["children"])
                {
                    ChildList.Add(cChild.Value<string>());
                }
                RowRefference["Inferior_Children"] = ChildList;
            }

            if (PostData["has_notes"].Value<bool>())
            {
                // when they fix api this should no longer maker 2 requests to get notes
                PostTest = Module_e621Info.e621InfoDownload("https://e621.net/notes.json?search[post_id]=" + PostID, true);
                if (!PostTest.StartsWith("{")) // no notes then
                {
                    RowRefference["Inferior_HasNotes"] = true;
                    double NewNoteSizeRatio = Math.Max((int)RowRefference["Info_MediaWidth"], (int)RowRefference["Info_MediaHeight"]) / (double)Math.Max(PostData["file"]["width"].Value<int>(), PostData["file"]["height"].Value<int>());
                    RowRefference["Inferior_NotesSizeRatio"] = NewNoteSizeRatio;
                }
            }
            if (Properties.Settings.Default.RemoveBVAS)
            {
                RowRefference["Upload_Tags"] = ((string)RowRefference["Upload_Tags"]).Replace("better_version_at_source", "");
            }
            _FormReference.Label_Tags.Text = (string)RowRefference["Upload_Tags"];

            DataRow DataRowTemp = _FormReference.Preview_RowHolder;
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref DataRowTemp);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp._Rating = (string)RowRefference["Upload_Rating"];
                int TagCounter = ((string)RowRefference["Upload_Tags"]).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Count();
                e6_GridItemTemp.cLabel_TagWarning.Visible = TagCounter < 5;
                e6_GridItemTemp.cCheckBox_UPDL.Checked = true;
                e6_GridItemTemp.toolTip_Display.SetToolTip(e6_GridItemTemp.cLabel_isSuperior, $"Media will be uploaded as superior of #{PostID}\n{e6_GridItemTemp.cLabel_isSuperior.Tag}"); ;
                e6_GridItemTemp.cLabel_isSuperior.Visible = true;
            }
            else
            {

                if (DataRowTemp["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(ref DataRowTemp, true))
                {
                    // e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                }
                else
                {
                    Form_Loader._FormReference.UploadCounter += (bool)RowRefference["UPDL_Queued"] ? 0 : 1;
                    Form_Loader._FormReference.DownloadCounter += (bool)RowRefference["UPDL_Queued"] ? 0 : 1;
                    RowRefference["UPDL_Queued"] = true;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
                    {
                        Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                    }
                    Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;
                }
            }
            _FormReference.UpdateButtons();
        }

        private bool LoadAllImagesMod;
        private void PB_LoadAllImages_Click(object sender, EventArgs e)
        {
            Label_Tags.Focus();
            if (LoadAllImagesMod)
            {
                PB_LoadAllImages.ForeColor = Color.LightSteelBlue;
                panel_Navigation.Enabled = true;
                panel_Rating.Enabled = true;
                PB_Tagger.Enabled = true;
                UpdateNavButtons(); // color doesn't change automatically when panel is enabled.
                LoadAllImagesMod = false;
                timer_LoadAllDelay.Stop();
            }
            else
            {
                PB_LoadAllImages.ForeColor = Color.Red;
                panel_Navigation.Enabled = false;
                panel_Rating.Enabled = false;
                PB_Tagger.Enabled = false;
                LoadAllImagesMod = true;
                PB_Navigation_Click(PB_Next, null);
            }
        }

        private DateTime LastTickTime;
        private readonly Timer timer_LoadAllDelay;
        private void Timer_LoadAllDelay_Tick(object sender, EventArgs e)
        {
            timer_LoadAllDelay.Stop();
            PB_Navigation_Click(PB_Next, null);
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
                KeyDown += Form_Preview_KeyDown;
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
                                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(ref Preview_RowHolder, false))
                                {
                                    // cCheckBox_UPDL.Checked = false;
                                }
                                else
                                {
                                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? -1 : 0;
                                }
                                if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
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
                                if (Preview_RowHolder["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(ref Preview_RowHolder))
                                {
                                    // cCheckBox_UPDL.Checked = false;
                                }
                                else
                                {
                                    Form_Loader._FormReference.UploadCounter += (bool)Preview_RowHolder["UPDL_Queued"] ? 0 : 1;
                                }
                                if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
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
            panel_Navigation.Focus();
        }

        private void Form_Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            MediaBrowser.Dispose();
            _FormReference = null;
        }

        private void Form_Preview_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Loader._FormReference.Activate(); // Focus back the Main Form
        }
    }
}