using System;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using e621_ReBot_v2.CustomControls;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_HicceArs
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_HicceArs);
            if (WebAdress.Contains("hiccears.com/p/"))
            {
                Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
            }
            else
            {
                Queue_Single(WebAdress);
            }
        }

        private static void Queue_Single(string WebAdress)
        {
            if (Module_Grabber._GrabQueue_URLs.Contains(WebAdress))
            {
                Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", WebAdress));
                return;
            }
            else
            {
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Add(WebAdress);
                }
                Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress, true);
            }
        }

        private static void Queue_Multi(string WebAdress, string HTMLSource)
        {
            TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            HtmlNodeCollection NodeTest = WebDoc.DocumentNode.SelectNodes(".//section[@class='section']/div[@class='grid grid-3-3-3-3 centered']/a[@class='album-preview']");

            string DirectLink2Post;
            string WorkTitle;
            HtmlNode ChildNodeHolder;
            foreach (HtmlNode ChildNode in NodeTest)
            {
                ChildNodeHolder = ChildNode;
                DirectLink2Post = $"https://www.hiccears.com{ChildNodeHolder.Attributes["href"].Value}";
                WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode(".//p[@class='album-preview-title']").InnerText);
                if (Module_Grabber._GrabQueue_URLs.Contains(DirectLink2Post))
                {
                    Module_Grabber.Report_Info($"Skipped grabbing - Already in queue [@{DirectLink2Post}]");
                    continue;
                }
                else
                {
                    lock (Module_Grabber._GrabQueue_URLs)
                    {
                        Module_Grabber._GrabQueue_URLs.Add(DirectLink2Post);
                    }
                }

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, WorkTitle, DirectLink2Post))
                {
                    Module_Grabber.Report_Info($"Skipped grabbing - Already in queue [@{DirectLink2Post}]");
                }
            }

            if (TreeViewParentNode.Nodes.Count == 0)
            {
                TreeViewParentNode.Remove();
            }
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            Module_Grabber.Report_Info(Grab(e.Argument.ToString()));
            ((BackgroundWorker)sender).Dispose();
        }

        public static string Grab(string WebAdress, bool UnitTestRun = false)
        {
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_HicceArs);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode(".//div[@class='content-grid pt-0']");

                DateTime Post_Time = DateTime.ParseExact(PostNode.SelectSingleNode(".//div[@class='information-line-list']//p[@class='information-line-text']").InnerText, "yyyy-MM-dd HH:mm UTC", null);

                string Post_Title = PostNode.SelectSingleNode(".//div[@class='section-header']//h2[@class='section-title']").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = PostNode.SelectSingleNode(".//div[@class='sidebar-box-items']//p[@class='user-status-title']").InnerText;

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='widget-box-content']");
                string Post_Text = null;
                if (Post_TextNode != null)
                {
                    Post_Text = Module_Html2Text.Html2Text_HicceArs(Post_TextNode);
                }

                string Post_MediaURL;
                string Post_ThumbURL;
                int SkipCounter = 0;
                bool CookieError = false;
                if (Post_URL.Contains("hiccears.com/file/"))
                {
                    HtmlNode ImageLinkNode = PostNode.SelectSingleNode(".//a[@class='button secondary download-button pl-5 pr-5 mb-3']");

                    Post_MediaURL = $"https://www.hiccears.com{ImageLinkNode.Attributes["href"].Value}";

                    string ThumbImageUrl = PostNode.SelectSingleNode(".//a[@class='button primary pl-5 pr-5 mb-3 mb-3']").Attributes["href"].Value;
                    HtmlNodeCollection ThumbNodes = PostNode.SelectNodes(".//div[@class='grid grid-3-3-3-3 centered']/a");
                    if (ThumbNodes.Count == 1)
                    {
                        ThumbImageUrl = ThumbNodes[0].SelectSingleNode(".//img").Attributes["src"].Value;
                    }
                    else
                    {
                        int NodeIndex = 0;
                        string NodeURL;
                        foreach (HtmlNode HtmlNodeTemp in ThumbNodes)
                        {
                            NodeURL = HtmlNodeTemp.Attributes["href"].Value;
                            if (NodeURL.Equals(ThumbImageUrl))
                            {
                                NodeIndex = (NodeIndex == ThumbNodes.Count - 1) ? 0 : NodeIndex++;
                                break;
                            }
                            NodeIndex++;
                        }
                        ThumbImageUrl = ThumbNodes[NodeIndex].SelectSingleNode(".//img").Attributes["src"].Value;
                    }
                    Post_ThumbURL = $"https://www.hiccears.com{ThumbImageUrl}";

                    if (Module_Grabber._Grabbed_MediaURLs.Contains(Post_MediaURL))
                    {
                        goto Skip2Exit;
                    }

                    DataRow TempDataRow = TempDataTable.NewRow();
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, Post_ThumbURL, ArtistName, ref CookieError);
                    if (TempDataRow != null)
                    {
                        TempDataTable.Rows.Add(TempDataRow);
                        lock (Module_Grabber._Grabbed_MediaURLs)
                        {
                            Module_Grabber._Grabbed_MediaURLs.Add(Post_MediaURL);
                        }
                    }
                }
                else
                {
                    HtmlNode ParentNode = PostNode.SelectSingleNode(".//div[@class='marketplace-content grid-column']/div[@class='grid grid-3-3-3-3 centered']");
                    Custom_ProgressBar TempcPB = new Custom_ProgressBar(Post_URL, ParentNode.SelectNodes("./a").Count);
                    if (!UnitTestRun)
                    {
                        Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cFlowLayoutPanel_ProgressBarHolder.Controls.Add(TempcPB); }));
                    }
                    foreach (HtmlNode ImageNode in ParentNode.SelectNodes("./a"))
                    {
                        TempcPB.Value += 1;
                        Post_MediaURL = $"https://www.hiccears.com{ImageNode.Attributes["href"].Value.Replace("/preview", "/download")}";
                        Post_ThumbURL = $"https://www.hiccears.com{ImageNode.SelectSingleNode(".//img").Attributes["src"].Value}";

                        if (Module_Grabber._Grabbed_MediaURLs.Contains(Post_MediaURL))
                        {
                            SkipCounter += 1;
                            continue;
                        }

                        DataRow TempDataRow = TempDataTable.NewRow();
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, Post_ThumbURL, ArtistName, ref CookieError);
                        if (TempDataRow != null)
                        {
                            TempDataTable.Rows.Add(TempDataRow);
                            lock (Module_Grabber._Grabbed_MediaURLs)
                            {
                                Module_Grabber._Grabbed_MediaURLs.Add(Post_MediaURL);
                            }
                        }

                        Thread.Sleep(Module_Grabber.PauseBetweenImages);
                    }
                    if (!UnitTestRun)
                    {
                        Form_Loader._FormReference.BeginInvoke(new Action(() => { TempcPB.Dispose(); }));
                    }
                }

            Skip2Exit:
                string PrintText;
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_WorkingOn)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    if (CookieError)
                    {
                        PrintText = $"Grabbing skipped - You need to relog on HicceArs to refresh cookies";
                    }
                    else
                    {
                        PrintText = $"Grabbing skipped - All media already grabbed [@{Post_URL}]";
                    }
                }
                else
                {
                    Module_Grabber._GrabQueue_WorkingOn[Post_URL] = TempDataTable;
                    PrintText = $"Finished grabbing: {Post_URL}";
                    if (SkipCounter > 0)
                    {
                        PrintText += $", {SkipCounter} media container{(SkipCounter > 1 ? "s have" : " has")} been skipped";
                    }
                }
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Remove(Post_URL);
                }
                //Module_Grabber.Report_Info(PrintText);
                return PrintText;
            }
            return "Error encountered during HicceArs grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ThumbURL, string Artist, ref bool CookieError)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on HicceArs", Title, Artist));
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbURL;
            //TempDataRow["Info_MediaFormat"] = null;
            //TempDataRow["Info_MediaByteLength"] = null;
            //TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
            CheckMedia(ref TempDataRow, ref CookieError);
        }

        private static void CheckMedia(ref DataRow TempDataRow, ref bool CookieError)
        {
            HttpWebRequest HicceArsMediaCheck = (HttpWebRequest)WebRequest.Create((string)TempDataRow["Grab_MediaURL"]);
            HicceArsMediaCheck.CookieContainer = Module_CookieJar.Cookies_HicceArs;
            HicceArsMediaCheck.Method = "HEAD";
            try
            {
                using (HttpWebResponse HicceArsMediaCheckHead = (HttpWebResponse)HicceArsMediaCheck.GetResponse())
                {
                    if (HicceArsMediaCheckHead.StatusCode == HttpStatusCode.OK)
                    {
                        string contentType = HicceArsMediaCheckHead.ContentType;
                        if (contentType.Substring(0, 4).Equals("text"))
                        {
                            CookieError = true;
                            TempDataRow = null;
                        }
                        else
                        {
                            contentType = contentType.Substring(contentType.IndexOf("/") + 1).Replace("jpeg", "jpg");
                            TempDataRow["Info_MediaFormat"] = contentType;
                            TempDataRow["Info_MediaByteLength"] = (int)HicceArsMediaCheckHead.ContentLength;
                        }
                    }
                    return;
                }
            }
            catch
            {
                return;
            }
        }

        public static bool CheckHicceArsCookies()
        {
            Module_CookieJar.GetCookies("https://www.hiccears.com/", ref Module_CookieJar.Cookies_HicceArs);
            HttpWebRequest HicceArsMediaCheck = (HttpWebRequest)WebRequest.Create("https://www.hiccears.com/file/b462f5d5-5eeb-4f87-86af-d3524d616474/e13d92f2-f446-4d75-9675-a5d364083342/download");
            HicceArsMediaCheck.CookieContainer = Module_CookieJar.Cookies_HicceArs;
            HicceArsMediaCheck.Method = "HEAD";
            bool TestResults = true;
            try
            {
                using (HttpWebResponse HicceArsMediaCheckHead = (HttpWebResponse)HicceArsMediaCheck.GetResponse())
                {
                    if (HicceArsMediaCheckHead.StatusCode == HttpStatusCode.OK)
                    {
                        string contentType = HicceArsMediaCheckHead.ContentType;
                        if (contentType.Substring(0, 4).Equals("text"))
                        {
                            TestResults = false;
                        }
                    }
                    else
                    {
                        TestResults = false;
                    }
                }
            }
            catch
            {
                TestResults = false;
            }
            if (!TestResults)
            {
                Module_Grabber.Report_Info("HicceArs Test - You need to relog to refresh cookies, media links are behind login.");
            }
            return TestResults;
        }

        public static string GetHicceArsMediaType(string URL)
        {
            HttpWebRequest HicceArsMediaCheck = (HttpWebRequest)WebRequest.Create(URL);
            HicceArsMediaCheck.CookieContainer = Module_CookieJar.Cookies_HicceArs;
            HicceArsMediaCheck.Method = "HEAD";
            try
            {
                using (HttpWebResponse HicceArsMediaCheckHead = (HttpWebResponse)HicceArsMediaCheck.GetResponse())
                {
                    if (HicceArsMediaCheckHead.StatusCode == HttpStatusCode.OK)
                    {
                        string contentType = HicceArsMediaCheckHead.ContentType;
                        contentType = contentType.Substring(contentType.IndexOf("/") + 1).Replace("jpeg", "jpg");
                        return contentType;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}