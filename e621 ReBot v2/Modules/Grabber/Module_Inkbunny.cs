using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using e621_ReBot_v2.CustomControls;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Inkbunny
    {

        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Inkbunny);
            if (WebAdress.Contains("inkbunny.net/gallery/")
            || WebAdress.Contains("inkbunny.net/scraps/")
            || WebAdress.Contains("inkbunny.net/submissionsviewall.php"))
            {
                Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
            }
            else // WebAdress.Contains("inkbunny.net/s/")
            {
                Queue_Single(WebAdress, Module_CefSharp.GetHTMLSource());
            }
        }

        private static void Queue_Single(string WebAdress, string HTMLSource)
        {
            string InkbunnyLinkFix = WebAdress;
            if (InkbunnyLinkFix.Contains("-"))
            {
                InkbunnyLinkFix = InkbunnyLinkFix.Substring(0, InkbunnyLinkFix.IndexOf("-"));
            }
            if (InkbunnyLinkFix.Contains("#"))
            {
                InkbunnyLinkFix = InkbunnyLinkFix.Replace("#pictop", "");
            }

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);
            string SubmissionType = WebDoc.DocumentNode.SelectSingleNode(".//div[@class='elephant elephant_bottom elephant_white']/div[@class='content']/div[3]/div[1]/div[2]/div[1]/div/text()").InnerText.Trim();
            if (SubmissionType.Equals("Writing - Document") || SubmissionType.Equals("Music - Single Track"))
            {
                Module_Grabber.Report_Info($"Skipped grabbing - Unsupported submission type: {SubmissionType} [@{InkbunnyLinkFix}]");
                return;
            }

            if (Module_Grabber._GrabQueue_URLs.Contains(InkbunnyLinkFix))
            {
                Module_Grabber.Report_Info($"Skipped grabbing - Already in queue [@{InkbunnyLinkFix}]");
                return;
            }
            else
            {
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Add(InkbunnyLinkFix);
                }
                Module_Grabber.CreateOrFindParentTreeNode(InkbunnyLinkFix, InkbunnyLinkFix, true);
            }
        }

        private static void Queue_Multi(string WebAdress, string HTMLSource)
        {
            TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            // Get Submission element
            HtmlNode ThumbNodeSelector = WebDoc.DocumentNode.SelectSingleNode(".//div[contains(@class, 'CompleteFromSubmission ')]");
            string ThumbClassName = ThumbNodeSelector.Attributes["class"].Value;

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes(string.Format("//div[@class='{0}']", ThumbClassName)))
            {
                string SubmissionType = ChildNode.SelectSingleNode(".//img[@class='widget_thumbnailFromSubmission_icon'][2]").Attributes["title"].Value;
                string DirectLink2Post = "https://inkbunny.net" + ChildNode.SelectSingleNode(".//a").Attributes["href"].Value;
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

                if (SubmissionType.Equals("Type: Writing - Document") || SubmissionType.Equals("Type: Music - Single Track"))
                {
                    Module_Grabber.Report_Info($"Skipped grabbing - Unsupported submission type: {SubmissionType.Replace("Type:", "")} [@{DirectLink2Post}]");
                    continue;
                }

                string WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode(".//div[@class='widget_thumbnailFromSubmission_title']").Attributes["title"].Value);
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

        public static string Grab(string WebAdress)
        {
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_Inkbunny);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode("html");

                string Post_TimeTemp = PostNode.SelectSingleNode(".//span[@id='submittime_exact']").InnerText;
                Post_TimeTemp = Post_TimeTemp.Substring(0, Post_TimeTemp.LastIndexOf(":") + 3);
                DateTime Post_Time = DateTime.Parse(Post_TimeTemp);

                string Post_Title = PostNode.SelectSingleNode(".//div[@id='pictop']//h1").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = PostNode.SelectNodes(".//div[@class='elephant elephant_555753'][2]/div[3]/table/tr/td[1]//a[@href]").LastOrDefault().Attributes["href"].Value;

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='elephant elephant_bottom elephant_white']/div[@class='content']//span");
                string Post_Text = Module_Html2Text.Html2Text_Inkbunny(Post_TextNode);

                string Post_MediaURL;
                int SkipCounter = 0;
                if (PostNode.SelectSingleNode(".//form[@id='changethumboriginal_form']") == null)
                {
                    if (PostNode.SelectSingleNode(".//div[@id='size_container']").InnerText.ToLower().Contains("download"))
                    {
                        Post_MediaURL = PostNode.SelectSingleNode(".//div[@id='size_container']/a").Attributes["href"].Value;
                    }
                    else
                    {
                        Post_MediaURL = PostNode.SelectSingleNode(".//div[@class='content magicboxParent']//img[@class='shadowedimage']").Attributes["src"].Value.Replace("files/screen", "files/full");
                    }

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
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, ArtistName);
                    TempDataTable.Rows.Add(TempDataRow);
                }
                else
                {
                    HtmlNode ParentNode = PostNode.SelectSingleNode(".//div[@id='files_area']").ParentNode; //.content
                    Custom_ProgressBar TempcPB = new Custom_ProgressBar(Post_URL, ParentNode.SelectNodes(".//img").Count);
                    Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cFlowLayoutPanel_ProgressBarHolder.Controls.Add(TempcPB); }));
                    foreach (HtmlNode ImageNode in ParentNode.SelectNodes(".//img"))
                    {
                        TempcPB.Value += 1;
                        Post_MediaURL = ImageNode.Attributes["src"].Value;
                        //if video
                        if (Post_MediaURL.Contains("images78/overlays"))
                        {
                            HttpWebRequest TempInkbunny_HTMLRequest = (HttpWebRequest)WebRequest.Create(Post_URL + "-p" + TempcPB.Value);
                            TempInkbunny_HTMLRequest.CookieContainer = Module_CookieJar.Cookies_Inkbunny;
                            TempInkbunny_HTMLRequest.Timeout = 5000;
                            HttpWebResponse TempInkbunny_HTMLResponse = (HttpWebResponse)TempInkbunny_HTMLRequest.GetResponse();
                            HtmlDocument TempWebDoc = new HtmlDocument();
                            TempWebDoc.Load(TempInkbunny_HTMLResponse.GetResponseStream(), Encoding.UTF8);
                            TempInkbunny_HTMLResponse.Dispose();

                            if (TempWebDoc.DocumentNode.SelectSingleNode(".//div[@id='size_container']").InnerText.ToLower().Contains("download"))
                            {
                                Post_MediaURL = TempWebDoc.DocumentNode.SelectSingleNode(".//div[@id='size_container']/a").Attributes["href"].Value;
                            }
                            else
                            {
                                Post_MediaURL = TempWebDoc.DocumentNode.SelectSingleNode(".//div[@class='content magicboxParent']//img[@class='shadowedimage']").Attributes["src"].Value;
                            }
                            TempWebDoc = null;
                        }
                        Post_MediaURL = Post_MediaURL.Replace("thumbnails/medium", "files/full").Replace("_noncustom", "");
                        Post_MediaURL = Post_MediaURL.Substring(0, Post_MediaURL.Length - 4);

                        List<string> MediaTypes = new List<string>() { ".jpg", ".png", ".gif", ".mp4", ".swf" };
                        while (MediaTypes.Count > 0)
                        {
                            string MediaLinkFix = Post_MediaURL + MediaTypes[0];
                            if (Module_Grabber.CheckURLExists(MediaLinkFix))
                            {
                                Post_MediaURL = MediaLinkFix;
                                break;
                            }
                            else
                            {
                                MediaTypes.RemoveAt(0);
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
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, ArtistName);
                        TempDataTable.Rows.Add(TempDataRow);

                        Thread.Sleep(Module_Grabber.PauseBetweenImages);
                    }
                    Form_Loader._FormReference.BeginInvoke(new Action(() => { TempcPB.Dispose(); }));
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
            return "Error encountered during Inkbunny grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode($"⮚ \"{Title}\" ⮘ by {Artist} on Inkbunny");
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            switch (MediaURL.Substring(MediaURL.LastIndexOf(".")))
            {
                case ".mp4":
                case ".swf":
                    {
                        string ThumbnailURLTemp = MediaURL.Replace(MediaURL.Substring(MediaURL.Length - 4), ".jpg").Replace("files/full", "thumbnails/large");
                        TempDataRow["Grab_ThumbnailURL"] = Module_Grabber.CheckURLExists(ThumbnailURLTemp) ? ThumbnailURLTemp : "https://nl.ib.metapix.net/images78/overlays/video.png";
                        break;
                    }

                case ".gif":
                    {
                        TempDataRow["Grab_ThumbnailURL"] = MediaURL.Replace("files/full", "files/screen");
                        break;
                    }

                default:
                    {
                        string ThumbnailURLTemp = MediaURL.Replace("files/full", "thumbnails/large");
                        ThumbnailURLTemp = ThumbnailURLTemp.Substring(0, ThumbnailURLTemp.Length - 4) + "_noncustom.jpg";
                        TempDataRow["Grab_ThumbnailURL"] = ThumbnailURLTemp;
                        break;
                    }
            }
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}