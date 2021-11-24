using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using e621_ReBot_v2.CustomControls;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Newgrounds
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Newgrounds);
            if (WebAdress.Contains("newgrounds.com/art/view/")
            || WebAdress.Contains("newgrounds.com/portal/view/")) //movies
            {
                Queue_Single(WebAdress);

            }
            else
            {
                Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
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

            var test = WebDoc.DocumentNode.SelectNodes(".//div[@data-id='item']");
            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes(".//div[@data-id='item']"))
            {
                string DirectLink2Post = ChildNode.SelectSingleNode("./a").Attributes["href"].Value;
                if (Module_Grabber._GrabQueue_URLs.Contains(DirectLink2Post))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                    continue;
                }
                else
                {
                    lock (Module_Grabber._GrabQueue_URLs)
                    {
                        Module_Grabber._GrabQueue_URLs.Add(DirectLink2Post);
                    }
                }

                string WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode(".//img").Attributes["alt"].Value);
                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, WorkTitle, DirectLink2Post))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            Module_Grabber.Report_Info(Grab(e.Argument.ToString()));
            ((BackgroundWorker)sender).Dispose();
            Thread.Sleep(5000); //Slow it down so that it does not get Error 429, 50 requests per minute seems to be the rate limit.
        }

        public static string Grab(string WebAdress, bool UnitTestRun = false)
        {
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_Newgrounds);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode; //.SelectSingleNode("div[@class='body-guts top']");

                //https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
                string Post_TimeTemp = PostNode.SelectNodes(".//dl[@class='sidestats'][2]/dd")[0].InnerText + " " + PostNode.SelectNodes(".//dl[@class='sidestats'][2]/dd")[1].InnerText;
                Post_TimeTemp = Post_TimeTemp.Substring(0, Post_TimeTemp.Length - 4);
                DateTime Post_Time = DateTime.Parse(Post_TimeTemp);

                string Post_Title = PostNode.SelectSingleNode(".//h2[@itemprop='name']").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = PostNode.SelectSingleNode(".//div[@class='item-details-main']").InnerText.Trim();

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@id='author_comments']");
                string Post_Text = Module_Html2Text.Html2Text_Newgrounds(Post_TextNode);

                string Post_MediaURL;
                int SkipCounter = 0;
                HtmlNodeCollection PictureNodes = PostNode.SelectNodes(".//div[@itemtype='https://schema.org/MediaObject']/div[@class='pod-body']//img"); ///div[@class='image']//img");
                if (PictureNodes != null)
                {
                    Custom_ProgressBar TempcPB = new Custom_ProgressBar(Post_URL, PictureNodes.Count);
                    if (!UnitTestRun)
                    {
                        Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cFlowLayoutPanel_ProgressBarHolder.Controls.Add(TempcPB); }));
                    }

                    string ThumbnailURLTemp = null;
                    foreach (HtmlNode ImageNode in PictureNodes)
                    {
                        TempcPB.Value += 1;

                        if (PictureNodes.Count == 1 || TempcPB.Value == 1)
                        {
                            Post_MediaURL = ImageNode.Attributes["src"].Value;
                            Post_MediaURL = Post_MediaURL.Substring(0, Post_MediaURL.IndexOf("?"));
                            if (ThumbnailURLTemp == null)
                            {
                                ThumbnailURLTemp = PostNode.SelectSingleNode(".//meta[@property='og:image']").Attributes["content"].Value;
                                ThumbnailURLTemp = ThumbnailURLTemp.Substring(0, ThumbnailURLTemp.IndexOf("?"));
                            }
                        }
                        else
                        {
                            if (ImageNode.Attributes["src"] != null && ImageNode.Attributes["src"].Value.Contains("thumbnails"))
                            {
                                continue;
                            }
                            else
                            {
                                Post_MediaURL = ImageNode.Attributes["data-smartload-src"].Value;
                                //ThumbnailURLTemp = Post_MediaURL;
                            }

                        }

                        if (Module_Grabber._Grabbed_MediaURLs.Contains(Post_MediaURL))
                        {
                            SkipCounter += 1;
                            continue;
                        }
                        else
                        {
                            lock (Module_Grabber._Grabbed_MediaURLs)
                            {
                                Module_Grabber._Grabbed_MediaURLs.Add(Post_MediaURL);
                            }
                        }

                        DataRow TempDataRow = TempDataTable.NewRow();
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, ThumbnailURLTemp, ArtistName);
                        if (TempcPB.Value == 1)
                        {
                            HtmlNode ResolutionNodeHolder = PostNode.SelectSingleNode(".//dl[@class='sidestats'][2]/dd[4]");
                            if (ResolutionNodeHolder != null)
                            {
                                string ResolutionStringHolder = ResolutionNodeHolder.InnerText.Trim();
                                ResolutionStringHolder = ResolutionStringHolder.Substring(0, ResolutionStringHolder.Length - 3);
                                string[] ResolutionHolder = ResolutionStringHolder.Split(new string[] { " x " }, StringSplitOptions.RemoveEmptyEntries);
                                TempDataRow["Info_MediaWidth"] = ResolutionHolder[0];
                                TempDataRow["Info_MediaHeight"] = ResolutionHolder[1];
                                TempDataRow["Thumbnail_FullInfo"] = true;
                                if (ThumbnailURLTemp.EndsWith(".wnebp", StringComparison.OrdinalIgnoreCase))
                                {
                                    Module_Grabber.WriteImageInfo(TempDataRow); //do it here so data shown on image properly due to custom handling of webp
                                }
                            }
                        }
                        TempDataTable.Rows.Add(TempDataRow);

                        Thread.Sleep(Module_Grabber.PauseBetweenImages);
                    }
                    if (!UnitTestRun)
                    {
                        Form_Loader._FormReference.BeginInvoke(new Action(() => { TempcPB.Dispose(); }));
                    }               
                }
                else
                {
                    string VideoURL = Post_URL.Replace("/view/", "/video/");
                    JObject VideoJSON = JObject.Parse(Module_Grabber.GrabPageSource(VideoURL, ref Module_CookieJar.Cookies_Newgrounds, true));

                    Post_MediaURL = VideoJSON["sources"].First.First.First["src"].Value<string>();
                    VideoJSON = null;

                    string ThumbnailURLTemp = PostNode.SelectSingleNode(".//meta[@property='og:image']").Attributes["content"].Value;
                    ThumbnailURLTemp = ThumbnailURLTemp.Substring(0, ThumbnailURLTemp.IndexOf("?"));

                    if (Module_Grabber._Grabbed_MediaURLs.Contains(Post_MediaURL))
                    {
                        goto Skip2Exit;
                    }
                    else
                    {
                        lock (Module_Grabber._Grabbed_MediaURLs)
                        {
                            Module_Grabber._Grabbed_MediaURLs.Add(Post_MediaURL);
                        }
                    }

                    DataRow TempDataRow = TempDataTable.NewRow();
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, ThumbnailURLTemp, ArtistName);
                    TempDataTable.Rows.Add(TempDataRow);
                }

            Skip2Exit:
                string PrintText;
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_WorkingOn)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    PrintText = $"Grabbing skipped - All media already grabbed [@{Post_URL}]";
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
            return "Error encountered during Newgrounds grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ThumbURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on Newgrounds", Title, Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbURL;
            if (ThumbURL.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                TempDataRow["Thumbnail_Image"] = new Bitmap(Properties.Resources.BrowserIcon_Newgrounds);
                Module_Grabber.WriteImageInfo(TempDataRow); //do it here so data shown on grid properly due to custom handling of webp
            }
            string FormatTemp = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            if (FormatTemp.Contains("?"))
            {
                FormatTemp = FormatTemp.Substring(0, FormatTemp.IndexOf("?"));
            } 
            TempDataRow["Info_MediaFormat"] = FormatTemp;
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}
