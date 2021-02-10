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
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public static class Module_Grabber
    {
        static Module_Grabber()
        {
            Initilize_GrabEnabler();

            timer_Grab = new Timer
            {
                Interval = Properties.Settings.Default.DelayGrabber
            };
            timer_Grab.Tick += TimerGrab_Tick;
            timer_Grab.Start();
        }

        public static readonly List<string> _GrabEnabler = new List<string>();
        private static void Initilize_GrabEnabler()
        {
            _GrabEnabler.Add("inkbunny.net/s/");
            _GrabEnabler.Add("inkbunny.net/gallery/");
            _GrabEnabler.Add("inkbunny.net/scraps/");
            _GrabEnabler.Add("inkbunny.net/submissionsviewall.php");
            _GrabEnabler.Add("pixiv.net/member_illust.php");
            _GrabEnabler.Add("pixiv.net/member.php");
            _GrabEnabler.Add("pixiv.net/en/artworks/");
            _GrabEnabler.Add("pixiv.net/en/users/");
            _GrabEnabler.Add("hiccears.com/artist-content.php");
            _GrabEnabler.Add("hiccears.com/gallery.php");
            _GrabEnabler.Add("hiccears.com/picture.php");
            _GrabEnabler.Add("furaffinity.net/gallery/");
            _GrabEnabler.Add("furaffinity.net/scraps/");
            _GrabEnabler.Add("furaffinity.net/favorites/");
            _GrabEnabler.Add("furaffinity.net/view/");
            _GrabEnabler.Add("furaffinity.net/full/");
            _GrabEnabler.Add("furaffinity.net/search/");
            // GrabberList_GrabEnabler.Add("twitter.com")
            _GrabEnabler.Add("newgrounds.com/art/view/");
            _GrabEnabler.Add("newgrounds.com/art/");
            _GrabEnabler.Add("newgrounds.com/portal/view/");
            _GrabEnabler.Add("newgrounds.com/portal/");
            //_GrabEnabler.Add("plurk.com/");
            _GrabEnabler.Add("plurk.com/p/");
            _GrabEnabler.Add("sofurry.com/view/");
            _GrabEnabler.Add("sofurry.com/browse/user/art");
            _GrabEnabler.Add("mastodon.social/@");
        }



        public static void Report_Info(string InfoMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.textBox_Info.Text = string.Format("{0} Grabber >>> {1}\n", DateTime.Now.ToLongTimeString(), InfoMessage) + Form_Loader._FormReference.textBox_Info.Text;
            }
            ));
        }



        public static List<string> _GrabQueue_URLs = new List<string>();
        public static void PrepareLink(string WebAdress)
        {
            if (WebAdress.Contains("hiccears.com"))
            {
                Module_HicceArs.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("inkbunny.net"))
            {
                Module_Inkbunny.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("pixiv.net"))
            {
                Module_Pixiv.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("furaffinity.net"))
            {
                Module_FurAffinity.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("twitter.com"))
            {
                Module_Twitter.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("newgrounds.com"))
            {
                Module_Newgrounds.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("sofurry.com"))
            {
                Module_SoFurry.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("mastodon.social/@"))
            {
                Module_Mastodon.QueuePrep(WebAdress);
                return;
            }
            if (WebAdress.Contains("plurk.com"))
            {
                Module_Plurk.QueuePrep(WebAdress);
                return;
            }
        }



        public static TreeNode CreateOrFindParentTreeNode(string NodeText, string NodeName, bool SkipSearch = false, string[] TagPass = null)
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
        public static bool CreateChildTreeNode(ref TreeNode ParentNode, string NodeText, string NodeName, string[] TagPass = null)
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

            if (WebAdress.Contains("hiccears.com"))
            {
                BGWTemp.DoWork += Module_HicceArs.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("inkbunny.net"))
            {
                BGWTemp.DoWork += Module_Inkbunny.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("pixiv.net"))
            {
                BGWTemp.DoWork += Module_Pixiv.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("furaffinity.net"))
            {
                BGWTemp.DoWork += Module_FurAffinity.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("twitter.com"))
            {
                BGWTemp.DoWork += Module_Twitter.RunGrabber;
                BGWTemp.RunWorkerAsync(NeededData);
                return;
            }
            if (WebAdress.Contains("newgrounds.com"))
            {
                BGWTemp.DoWork += Module_Newgrounds.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("sofurry.com"))
            {
                BGWTemp.DoWork += Module_SoFurry.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }
            if (WebAdress.Contains("mastodon.social/@"))
            {
                BGWTemp.DoWork += Module_Mastodon.RunGrabber;
                BGWTemp.RunWorkerAsync(NeededData);
                return;
            }
            if (WebAdress.Contains("plurk.com"))
            {
                BGWTemp.DoWork += Module_Plurk.RunGrabber;
                BGWTemp.RunWorkerAsync(WebAdress);
                return;
            }

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
                Module_TableHolder.Database_Table.Merge(TempDataTable);
                for (int rowindex = StartRowIndex; rowindex < Module_TableHolder.Database_Table.Rows.Count; rowindex++)
                {
                    DataRow TempDataRow = Module_TableHolder.Database_Table.Rows[rowindex];
                    Module_DB.DB_CheckMediaRecord(ref TempDataRow);
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
            HTMLRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0";
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
            catch
            {
                return null;
            }
        }

        public static dynamic GetMediaSize(string MediaURL)
        {
            HttpWebRequest GetSizeRequest = (HttpWebRequest)WebRequest.Create(MediaURL);
            GetSizeRequest.Method = "HEAD";
            GetSizeRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0";
            GetSizeRequest.CookieContainer = new CookieContainer(); //twitter sometimes not working otherwise
            if (MediaURL.Contains("pximg.net")) GetSizeRequest.Referer = "http://www.pixiv.net";
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
            CheckExistsRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0";
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

        public static void DownloadThumb(ref DataRow RowRef)
        {
            string SiteReferer = "https://" + new Uri((string)RowRef["Grab_URL"]).Host;
            using (WebClient ThumbClient = new WebClient())
            {
                ThumbClient.Headers.Add(HttpRequestHeader.Referer, SiteReferer);
                ThumbClient.DownloadDataCompleted += DownloadThumbFinished;
                ThumbClient.DownloadDataAsync(new Uri((string)RowRef["Grab_ThumbnailURL"]), RowRef);
            }
        }

        public static void DownloadThumbFinished(object sender, DownloadDataCompletedEventArgs e)
        {
            DataRow RowReference = (DataRow)e.UserState;

            Image DownloadedImage;
            using (var TempStream = new MemoryStream(e.Result))
            {
                DownloadedImage = Image.FromStream(TempStream);
            }

            Bitmap ResizedImage;
            int LargerSize = Math.Max(DownloadedImage.Width, DownloadedImage.Height);
            if (LargerSize > 200)
            {
                float scale_factor = 200f / LargerSize;
                ResizedImage = new Bitmap(DownloadedImage, (int)(DownloadedImage.Width * scale_factor), (int)(DownloadedImage.Height * scale_factor)); // , Imaging.PixelFormat.Format32bppArgb)
            }
            else
            {
                ResizedImage = new Bitmap(DownloadedImage, DownloadedImage.Width, DownloadedImage.Height);
            }

            string Text2Draw = DownloadedImage.GetFrameCount(new FrameDimension(DownloadedImage.FrameDimensionsList[0])) > 1 ? "Animated" : null;
            if (Text2Draw == null)
            {
                Text2Draw = ((string)RowReference["Grab_MediaURL"]).Contains("ugoira") ? "Ugoira" : null;
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
            RowReference["Thumbnail_Image"] = ResizedImage;

            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref RowReference);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp.LoadImage();
            }

            DownloadedImage.Dispose();
            ((WebClient)sender).Dispose();
        }
    }

}
