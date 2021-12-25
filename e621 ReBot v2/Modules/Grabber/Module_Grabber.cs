using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules.Grabber;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public static class Module_Grabber
    {
        static Module_Grabber()
        {
            timer_Grab = new Timer
            {
                Interval = Properties.Settings.Default.DelayGrabber
            };
            timer_Grab.Tick += TimerGrab_Tick;
            timer_Grab.Start();

            _GrabEnabler.Add(new Regex(@".+(inkbunny.net/)(s|gallery|scraps)(/\w+)"));
            _GrabEnabler.Add(new Regex(@".+(inkbunny.net/submissionsviewall.php)"));
            _GrabEnabler.Add(new Regex(@".+(pixiv.net/)(\w+/)(artworks|users)/\d+"));
            _GrabEnabler.Add(new Regex(@".+(www.furaffinity.net/)(view|full|gallery|scraps|favorites)(/.+/)"));
            _GrabEnabler.Add(new Regex(@".+(www.furaffinity.net/search/)"));
            _GrabEnabler.Add(new Regex(@".+(twitter.com/)(\w+/(media|status/\d+/?$))"));
            _GrabEnabler.Add(new Regex(@".+(.newgrounds.com/)(movies/|portal/view/\d+|art?.+)"));
            _GrabEnabler.Add(new Regex(@".+(www.hiccears.com/)(picture|gallery|artist-content)(.+)"));
            _GrabEnabler.Add(new Regex(@".+(.sofurry.com)/(view/\d+|browse/\w+/art?.+)"));
            _GrabEnabler.Add(new Regex(@".+(mastodon.social/)(@\w+)(/\d+|/media)"));
            _GrabEnabler.Add(new Regex(@".+(www.plurk.com/)(p/\w+|\w+$)"));
            _GrabEnabler.Add(new Regex(@".+(pawoo.net/)(@\w+)(/\d+|/media)"));
            _GrabEnabler.Add(new Regex(@".+(www.weasyl.com/)(~\w+/submissions/\d+/.+|submissions.+|collections.+)")); //collections are favorites*
            _GrabEnabler.Add(new Regex(@".+(www.weasyl.com/)(search.+)(find=submit)"));
            _GrabEnabler.Add(new Regex(@".+(baraag.net/)(@\w+)?(/\d+|/media)"));
            _GrabEnabler.Add(new Regex(@".+(www.hentai-foundry.com/)(pictures/user/.+|user/.+?/faves/pictures)"));
            _GrabEnabler.Add(new Regex(@".+(www.hentai-foundry.com/)(pictures/(featured|popular|random|recent/))"));

        }

        public static readonly List<Regex> _GrabEnabler = new List<Regex>();


        public static void Report_Info(string InfoMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.textBox_Info.Text = $"{DateTime.Now.ToLongTimeString()} Grabber >>> {InfoMessage}\n{Form_Loader._FormReference.textBox_Info.Text}";
            }
            ));
        }



        public static List<string> _GrabQueue_URLs = new List<string>();
        public static void PrepareLink(string WebAdress)
        {
            Uri TempURI = new Uri(WebAdress);
            switch (TempURI.Host)
            {
                case "inkbunny.net":
                    {
                        Module_Inkbunny.QueuePrep(WebAdress);
                        break;
                    }
                case "www.pixiv.net":
                    {
                        Module_Pixiv.QueuePrep(WebAdress);
                        break;
                    }
                case "www.furaffinity.net":
                    {
                        Module_FurAffinity.QueuePrep(WebAdress);
                        break;
                    }
                case "twitter.com":
                    {
                        Module_Twitter.QueuePrep(WebAdress);
                        break;
                    }
                case string TempHost when TempHost.Contains("newgrounds.com"):
                    {
                        Module_Newgrounds.QueuePrep(WebAdress);
                        break;
                    }
                case "www.hiccears.com":
                    {
                        Module_HicceArs.QueuePrep(WebAdress);
                        break;
                    }
                case "www.sofurry.com":
                    {
                        Module_SoFurry.QueuePrep(WebAdress);
                        break;
                    }
                case "mastodon.social":
                    {
                        Module_Mastodon.QueuePrep(WebAdress);
                        break;
                    }
                case "www.plurk.com":
                    {
                        Module_Plurk.QueuePrep(WebAdress);
                        break;
                    }
                case "pawoo.net":
                    {
                        Module_Mastodon.QueuePrep(WebAdress);
                        break;
                    }
                case "www.weasyl.com":
                    {
                        Module_Weasyl.QueuePrep(WebAdress);
                        break;
                    }
                case "baraag.net":
                    {
                        Module_Mastodon.QueuePrep(WebAdress);
                        break;
                    }
                case "www.hentai-foundry.com":
                    {
                        Module_HentaiFoundry.QueuePrep(WebAdress);
                        break;
                    }
            }
            //if (WebAdress.Contains("deviantart.com"))
            //{
            //    Module_DeviantArt.QueuePrep(WebAdress);
            //    return;
            //}
        }



        public static TreeNode CreateOrFindParentTreeNode(string NodeText, string NodeName, bool SkipSearch = false, object TagPass = null)
        {
            TreeNode TreeViewParentNode;
            if (SkipSearch || !Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.ContainsKey(NodeName))
            {
                TreeViewParentNode = Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.Add(NodeText);
                TreeViewParentNode.Name = NodeName;
                TreeViewParentNode.ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_cTreeView;
                TreeViewParentNode.Checked = true;
                TreeViewParentNode.Tag = TagPass;
            }
            else
            {
                TreeViewParentNode = Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.Find(NodeText, false)[0];
            }
            return TreeViewParentNode;
        }
        public static bool CreateChildTreeNode(ref TreeNode ParentNode, string NodeText, string NodeName, object TagPass = null)
        {
            if (!Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.Find(NodeText, true).Any())
            {
                TreeNode TreeViewChildNode = ParentNode.Nodes.Add(NodeText); ;
                TreeViewChildNode.Name = NodeName;
                TreeViewChildNode.ToolTipText = NodeName;
                TreeViewChildNode.ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_cTreeView;
                TreeViewChildNode.Checked = true;
                TreeViewChildNode.Tag = TagPass;
                return true;
            }
            else
            {
                return false;
            }
        }



        public static Timer timer_Grab;
        private static int ActiveBGWCount = 0;
        private static void TimerGrab_Tick(object sender, EventArgs e)
        {
            if (Form_Loader._FormReference.cCheckGroupBox_Grab.Checked && ActiveBGWCount < 4)
            {
                TreeNode ParentNodeTemp = null;
                foreach (TreeNode ParentNode in Form_Loader._FormReference.cTreeView_GrabQueue.Nodes)
                {
                    if (ParentNode.Checked)
                    {
                        if (ParentNode.Nodes.Count > 0)
                        {
                            TreeNode ChildNodeTemp = null;
                            foreach (TreeNode ChildNode in ParentNode.Nodes)
                            {
                                if (ChildNode.Checked)
                                {
                                    ChildNodeTemp = ChildNode;
                                    GrabberBGWStart(ChildNode.Name, ChildNode.Tag);
                                    break;
                                }
                            }
                            if (ChildNodeTemp != null)
                            {
                                if (ChildNodeTemp.Parent.Nodes.Count == 1)
                                {
                                    ParentNodeTemp = ChildNodeTemp.Parent;
                                    break;
                                }
                                else
                                {
                                    TreeNode TempParentHolder = ChildNodeTemp.Parent;
                                    ChildNodeTemp.Remove();

                                    int CheckedChildNodesCount = int.Parse(TempParentHolder.Tag.ToString()) - 1;
                                    TempParentHolder.Tag = CheckedChildNodesCount;
                                    Form_Loader._FormReference.UpdateParentNode_Tooltip(TempParentHolder);
                                    if (CheckedChildNodesCount == 0)
                                    {
                                        TempParentHolder.Checked = false;
                                    }

                                }
                            }
                        }
                        else
                        {
                            GrabberBGWStart(ParentNode.Name, ParentNode.Tag);
                            ParentNodeTemp = ParentNode;
                            break;
                        }
                    }
                }
                if (ParentNodeTemp != null)
                {
                    ParentNodeTemp.Remove();
                }
            }
        }



        public static OrderedDictionary _GrabQueue_WorkingOn = new OrderedDictionary();
        public static List<string> _Grabbed_MediaURLs = new List<string>();
        public static int PauseBetweenImages = 50;
        private static void GrabberBGWStart(string WebAdress, object NeededData)
        {
            ActiveBGWCount += 1;
            BackgroundWorker BGWTemp = new BackgroundWorker();
            BGWTemp.RunWorkerCompleted += GrabberBGWDone;

            _GrabQueue_WorkingOn.Add(WebAdress, null);
            _GrabQueue_URLs.Remove(WebAdress);

            Report_Status();


            Uri TempURI = new Uri(WebAdress);
            switch (TempURI.Host)
            {
                case "inkbunny.net":
                    {
                        BGWTemp.DoWork += Module_Inkbunny.RunGrabber;
                        break;
                    }
                case "www.pixiv.net":
                    {
                        BGWTemp.DoWork += Module_Pixiv.RunGrabber;
                        break;
                    }
                case "www.furaffinity.net":
                    {
                        BGWTemp.DoWork += Module_FurAffinity.RunGrabber;
                        break;
                    }
                case "twitter.com":
                    {
                        BGWTemp.DoWork += Module_Twitter.RunGrabber;
                        BGWTemp.RunWorkerAsync(NeededData);
                        return;
                    }
                case string TempHost when TempHost.Contains(".newgrounds.com"):
                    {
                        BGWTemp.DoWork += Module_Newgrounds.RunGrabber;
                        break;
                    }
                case "www.hiccears.com":
                    {
                        BGWTemp.DoWork += Module_HicceArs.RunGrabber;
                        break;
                    }
                case "www.sofurry.com":
                    {
                        BGWTemp.DoWork += Module_SoFurry.RunGrabber;
                        break;
                    }
                case "mastodon.social":
                    {
                        BGWTemp.DoWork += Module_Mastodon.RunGrabber;
                        BGWTemp.RunWorkerAsync(NeededData);
                        return;
                    }
                case "www.plurk.com":
                    {
                        BGWTemp.DoWork += Module_Plurk.RunGrabber;
                        break;
                    }
                case "pawoo.net":
                    {
                        BGWTemp.DoWork += Module_Mastodon.RunGrabber;
                        BGWTemp.RunWorkerAsync(NeededData);
                        return;
                    }
                case "www.weasyl.com":
                    {
                        BGWTemp.DoWork += Module_Weasyl.RunGrabber;
                        break;
                    }
                case "baraag.net":
                    {
                        BGWTemp.DoWork += Module_Mastodon.RunGrabber;
                        BGWTemp.RunWorkerAsync(NeededData);
                        return;
                    }
                case "www.hentai-foundry.com":
                    {
                        BGWTemp.DoWork += Module_HentaiFoundry.RunGrabber;
                        break;
                    }
            }
            BGWTemp.RunWorkerAsync(WebAdress);

            //if (WebAdress.Contains("deviantart.com"))
            //{
            //    BGWTemp.DoWork += Module_DeviantArt.RunGrabber;
            //    BGWTemp.RunWorkerAsync(NeededData ?? WebAdress);
            //    return;
            //}
        }

        private static void GrabberBGWDone(object sender, RunWorkerCompletedEventArgs e)
        {
            ActiveBGWCount -= 1;
            Report_Status();

            List<DataTable> TempTableHolder = new List<DataTable>();
            if (Properties.Settings.Default.GrabDisplayOrder.Equals("0"))
            {
                while (_GrabQueue_WorkingOn.Count > 0)
                {
                    if (_GrabQueue_WorkingOn[0] == null)
                    {
                        break;
                    }
                    else
                    {
                        TempTableHolder.Add((DataTable)_GrabQueue_WorkingOn[0]);
                        lock (_GrabQueue_WorkingOn)
                        {
                            _GrabQueue_WorkingOn.RemoveAt(0);
                        }
                    }
                }
            }
            else
            {
                for (int i = _GrabQueue_WorkingOn.Count - 1; i >= 0; i--)
                {
                    if (_GrabQueue_WorkingOn[i] != null)
                    {
                        TempTableHolder.Add((DataTable)_GrabQueue_WorkingOn[i]);
                    }
                    lock (_GrabQueue_WorkingOn)
                    {
                        _GrabQueue_WorkingOn.RemoveAt(i);
                    }
                }
                TempTableHolder.Reverse();
            }
            foreach (DataTable TempDataTable in TempTableHolder)
            {

                int StartRowIndex = Module_TableHolder.Database_Table.Rows.Count;
                lock (Module_TableHolder.Database_Table)
                {
                    Module_TableHolder.Database_Table.Merge(TempDataTable);
                }
                for (int rowindex = StartRowIndex; rowindex < Module_TableHolder.Database_Table.Rows.Count; rowindex++)
                {
                    DataRow TempDataRow = Module_TableHolder.Database_Table.Rows[rowindex];
                    Module_DB.DB_Media_CheckRecord(ref TempDataRow);
                    if (Form_Loader._GridFLPHolder.Controls.Count < Form_Loader._GridMaxControls)
                    {
                        Form_Loader._FormReference.AddGridItem(ref TempDataRow, false);
                    }
                }
                Form_Loader._FormReference.Paginator();
                if (Module_TableHolder.Database_Table.Rows.Count - Form_Loader._FormReference.GridIndexTracker > Form_Loader._GridMaxControls)
                {
                    Form_Loader._FormReference.GB_Right.Visible = true;
                }
            }
        }

        private static void Report_Status()
        {
            string StatusMessage = ActiveBGWCount == 0 ? "Waiting..." : string.Format("Grabbing - {0} of 4 hands active.", ActiveBGWCount);
            Form_Loader._FormReference.BeginInvoke(new Action(() => { Form_Loader._FormReference.label_GrabStatus.Text = string.Format("Status: {0}", StatusMessage); }));
        }



        public static string GrabPageSource(string WebAdress, ref CookieContainer CookieRef, bool NewgroundsSpecialRequest = false)
        {
            HttpWebRequest HTMLRequest = (HttpWebRequest)WebRequest.Create(WebAdress);
            HTMLRequest.CookieContainer = CookieRef ?? new CookieContainer();
            if (NewgroundsSpecialRequest)
            {
                HTMLRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }
            HTMLRequest.Timeout = 5000;
            HTMLRequest.UserAgent = Form_Loader.GlobalUserAgent;
            try
            {
                using (HttpWebResponse HTMLResponse = (HttpWebResponse)HTMLRequest.GetResponse())
                {
                    if (HTMLResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader ResponseReader = new StreamReader(HTMLResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            return ResponseReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        public static dynamic GetMediaSize(string MediaURL)
        {
            HttpWebRequest GetSizeRequest = (HttpWebRequest)WebRequest.Create(MediaURL);
            GetSizeRequest.Method = "HEAD";
            GetSizeRequest.UserAgent = Form_Loader.GlobalUserAgent;
            GetSizeRequest.CookieContainer = MediaURL.Contains("deviantart.com") ? Module_CookieJar.Cookies_DeviantArt : new CookieContainer(); //twitter sometimes not working otherwise
            if (MediaURL.Contains("pximg.net"))
            {
                GetSizeRequest.Referer = "http://www.pixiv.net";
            }
            GetSizeRequest.Timeout = 3000;
            try
            {
                using (HttpWebResponse GetSizeResponse = (HttpWebResponse)GetSizeRequest.GetResponse())
                {
                    if (GetSizeResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return (int)GetSizeResponse.ContentLength;
                    }
                    else
                    {
                        return DBNull.Value;
                    }
                }
            }
            catch (WebException)
            {
                return DBNull.Value;
            }
        }

        public static bool CheckURLExists(string MediaURL)
        {
            HttpWebRequest CheckExistsRequest = (HttpWebRequest)WebRequest.Create(MediaURL);
            CheckExistsRequest.Method = "HEAD";
            CheckExistsRequest.UserAgent = Form_Loader.GlobalUserAgent;
            CheckExistsRequest.Timeout = 3000;
            try
            {
                using (HttpWebResponse GetSizeResponse = (HttpWebResponse)CheckExistsRequest.GetResponse())
                {
                    if (GetSizeResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static void GrabDownloadThumb(ref DataRow RowRef)
        {
            string SiteReferer = "https://" + new Uri((string)RowRef["Grab_URL"]).Host;
            using (Custom_WebClient ThumbClient = new Custom_WebClient())
            {
                ThumbClient.Headers.Add(HttpRequestHeader.UserAgent, Form_Loader.GlobalUserAgent);
                ThumbClient.Headers.Add(HttpRequestHeader.Referer, SiteReferer);
                ThumbClient.DownloadDataCompleted += GrabDownloadThumbFinished;
                ThumbClient.DownloadDataAsync(new Uri((string)RowRef["Grab_ThumbnailURL"]), RowRef);
            }
        }

        public static void GrabDownloadThumbFinished(object sender, DownloadDataCompletedEventArgs e)
        {
            DataRow RowReference = (DataRow)e.UserState;
            if (RowReference.RowState == DataRowState.Detached)
            {
                return;
            }

            Image DownloadedImage;
            using (MemoryStream MemoryStreamTemp = new MemoryStream(e.Result))
            {
                DownloadedImage = Image.FromStream(MemoryStreamTemp);
            }
            RowReference["Thumbnail_Image"] = MakeImageThumb(DownloadedImage, ((string)RowReference["Grab_MediaURL"]).Contains("ugoira") ? "Ugoira" : null);
            DownloadedImage.Dispose();

            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref RowReference);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp.LoadImage();
            }
            else
            {
                WriteImageInfo(RowReference);
            }
        }

        public static Bitmap MakeImageThumb(Image ImagePass, string Text2DrawPass = null)
        {
            Bitmap ResizedImage;
            int LargerSize = Math.Max(ImagePass.Width, ImagePass.Height);
            if (LargerSize > 200)
            {
                float scale_factor = 200f / LargerSize;
                ResizedImage = new Bitmap(ImagePass, (int)(ImagePass.Width * scale_factor), (int)(ImagePass.Height * scale_factor)); // , Imaging.PixelFormat.Format32bppArgb)
            }
            else
            {
                ResizedImage = new Bitmap(ImagePass, ImagePass.Width, ImagePass.Height);
            }

            string Text2Draw = ImagePass.GetFrameCount(new FrameDimension(ImagePass.FrameDimensionsList[0])) > 1 ? "Animated" : null;
            if (Text2Draw == null)
            {
                Text2Draw = Text2DrawPass;
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
                            using (StringFormat sfTemp = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
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
            return ResizedImage;
        }

        public static Image WriteImageInfo(DataRow DataRowRef)
        {
            try
            {
                Image TempImage = new Bitmap(200, 200);
                using (Graphics gTemp = Graphics.FromImage(TempImage))
                {
                    gTemp.SmoothingMode = SmoothingMode.HighQuality;
                    gTemp.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    Image HoldTempImage = (Image)DataRowRef["Thumbnail_Image"];
                    Point CenterPoint = new Point(100 - (HoldTempImage.Width / 2), 100 - (HoldTempImage.Height / 2));
                    gTemp.DrawImage(HoldTempImage, CenterPoint);

                    if (DataRowRef["Info_MediaByteLength"] != DBNull.Value)
                    {
                        using (Font fontTemp = new Font("Arial Black", 14, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            using (GraphicsPath gpTemp = new GraphicsPath())
                            {
                                using (StringFormat sfTemp = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                {
                                    int bytestokB = (int)DataRowRef["Info_MediaByteLength"] / 1024;
                                    gpTemp.AddString(string.Format("{0:N0} kB", bytestokB), fontTemp.FontFamily, (int)fontTemp.Style, fontTemp.Size, new Rectangle(new Point(0, 178), new Size(200, 20)), sfTemp);
                                }
                                using (Pen penTemp = new Pen(Color.Black, 3))
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

                    if (DataRowRef["Thumbnail_FullInfo"] != DBNull.Value)
                    {
                        using (Font fontTemp = new Font("Arial Black", 14, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            using (GraphicsPath gpTemp = new GraphicsPath())
                            {
                                using (StringFormat sfTemp = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                {
                                    gpTemp.AddString(string.Format("{0} x {1} - {2}", DataRowRef["Info_MediaWidth"].ToString(), DataRowRef["Info_MediaHeight"].ToString(), DataRowRef["Info_MediaFormat"].ToString()), fontTemp.FontFamily, (int)fontTemp.Style, fontTemp.Size, new Rectangle(new Point(0, 0), new Size(200, 20)), sfTemp);
                                }
                                using (Pen penTemp = new Pen(Color.Black, 3))
                                {
                                    gTemp.DrawPath(penTemp, gpTemp);
                                }
                                using (SolidBrush brushTemp = new SolidBrush(Color.LightSteelBlue))
                                {
                                    gTemp.FillPath(brushTemp, gpTemp);
                                }
                            }
                        }
                        HoldTempImage.Dispose();
                        DataRowRef["Thumbnail_Image"] = TempImage;
                    }
                    else
                    {
                        using (Font fontTemp = new Font("Arial Black", 14, FontStyle.Bold, GraphicsUnit.Pixel))
                        {
                            using (GraphicsPath gpTemp = new GraphicsPath())
                            {
                                using (StringFormat sfTemp = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                {
                                    gpTemp.AddString(DataRowRef["Info_MediaFormat"].ToString(), fontTemp.FontFamily, (int)fontTemp.Style, fontTemp.Size, new Rectangle(new Point(0, 0), new Size(200, 20)), sfTemp);
                                }
                                using (Pen penTemp = new Pen(Color.Black, 3))
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
                    return TempImage;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}