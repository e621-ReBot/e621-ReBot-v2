using System;
using System.Collections.Generic;
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
            if (WebAdress.Contains("hiccears.com/artist-content.php"))
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

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes(".//div[@class='col-md-2 imagelist-item  '] | .//div[@class='col-md-2 imagelist-item item-subscription']"))
            {
                string DirectLink2Post = "https://www.hiccears.com" + ChildNode.SelectSingleNode(".//a").Attributes["href"].Value.Remove(0, 1);
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

                if (DirectLink2Post.Contains("/novel.php?nid="))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Unsupported submission type: Novel [@{0}]", DirectLink2Post));
                    continue;
                }

                string WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode("./div[@class='title']/a").InnerText);
                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, WorkTitle, DirectLink2Post))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            Grab(e.Argument.ToString());
            ((BackgroundWorker)sender).Dispose();
        }

        private static void Grab(string WebAdress)
        {
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_HicceArs);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode(".//div[@class='col-md-10 nopadding']/div");

                DateTime Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//span[@class='pull-right']").InnerText.Replace("&nbsp;", " "));

                string Post_Title = PostNode.SelectSingleNode(".//a").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = WebDoc.DocumentNode.SelectSingleNode(".//h4/a").InnerText;

                HtmlNode Post_TextNode = PostNode.SelectNodes(".//div[@class='panel-body']")[1];
                string Post_Text = Module_Html2Text.Html2Text_HicceArs(Post_TextNode);

                string Post_MediaURL;
                int SkipCounter = 0;
                if (Post_URL.Contains("hiccears.com/picture.php"))
                {
                    Post_MediaURL = "https://www.hiccears.com" + PostNode.SelectSingleNode(".//div[@class='row']/a").Attributes["href"].Value.Remove(0, 1);

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
                    HtmlNode ParentNode = PostNode.SelectSingleNode(".//div[@class='panel-body']");
                    Custom_ProgressBar TempcPB = new Custom_ProgressBar(Post_URL, ParentNode.SelectNodes(".//img[@class='img-thumbnail']").Count);
                    Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cFlowLayoutPanel_ProgressBarHolder.Controls.Add(TempcPB); }));
                    foreach (HtmlNode ImageNode in ParentNode.SelectNodes(".//img[@class='img-thumbnail']"))
                    {
                        TempcPB.Value += 1;
                        Post_MediaURL = "https://www.hiccears.com" + ImageNode.Attributes["src"].Value.Replace("/thumbnails/", "/imgs/").Remove(0, 1);
                        Post_MediaURL = Post_MediaURL.Remove(Post_MediaURL.Length - 4);

                        List<string> MediaTypes = new List<string>() { ".png", ".jpg", ".gif" };
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
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_URLs)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    Module_Grabber.Report_Info(string.Format("Grabbing skipped - All media already grabbed [@{0}]", Post_URL));
                }
                else
                {
                    Module_Grabber._GrabQueue_WorkingOn[Post_URL] = TempDataTable;
                    string PrintText = string.Format("Finished grabbing: {0}", Post_URL);
                    if (SkipCounter > 0)
                    {
                        PrintText += SkipCounter == 1 ? string.Format(", {0} media container has been skipped", SkipCounter) : string.Format(", {0} media containers have been skipped", SkipCounter);
                    }
                    Module_Grabber.Report_Info(PrintText);
                }
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Remove(Post_URL);
                }
            }
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on HicceArs", Title, Artist));
            if (TextBody != null)
            {
                TempDataRow["Grab_TextBody"] = TextBody;
            }
            TempDataRow["Grab_MediaURL"] = MediaURL;
            string ThumbnailURLTemp = MediaURL.Replace("/upl0ads/imgs/", "/upl0ads/thumbnails/");
            if (!ThumbnailURLTemp.Contains("/img/subscription"))
            {
                ThumbnailURLTemp = ThumbnailURLTemp.Remove(ThumbnailURLTemp.Length - 4) + ".png";
            }
            TempDataRow["Grab_ThumbnailURL"] = ThumbnailURLTemp;
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }

    }
}
