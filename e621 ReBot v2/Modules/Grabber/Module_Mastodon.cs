using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using CefSharp.DevTools.Debugger;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Mastodon
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Mastodon);
            string NumericPartCheck = WebAdress.Substring(WebAdress.LastIndexOf("/") + 1);
            if (NumericPartCheck.All(char.IsDigit))
            {
                Queue_Single(WebAdress, Module_CefSharp.GetHTMLSource());
            }
            else
            {
                Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
            }
        }

        private static void Queue_Single(string WebAdress, string HTMLSource)
        {
            if (Module_Grabber._GrabQueue_URLs.Contains(WebAdress))
            {
                Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", WebAdress));
                return;
            }
            else
            {
                HtmlDocument WebDocTemp = new HtmlDocument();
                WebDocTemp.LoadHtml(HTMLSource);
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Add(WebAdress);
                }
                Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress, true, new string[] { WebAdress, WebDocTemp.DocumentNode.SelectSingleNode(".//div[@class='focusable detailed-status__wrapper']").OuterHtml });
            }
        }

        private static void Queue_Multi(string WebAdress, string HTMLSource)
        {
            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            HtmlNodeCollection MediaNodeTest = WebDoc.DocumentNode.SelectNodes(".//div[@class='account-gallery__container']//a[@class='media-gallery__item-thumbnail']");

            if (MediaNodeTest == null)
            {
                Module_Grabber.Report_Info(string.Format("Skipped grabbing - Page contains no media [@{0}]", WebAdress));
                return;
            }
            TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

            foreach (HtmlNode ChildNode in MediaNodeTest)
            {
                HtmlNode ChildNodeHolder = ChildNode.ParentNode;

                string DirectLink2Post = ChildNodeHolder.SelectSingleNode("./a").Attributes["href"].Value;
                string GetUserName = WebAdress.Substring(WebAdress.IndexOf("/@"));
                GetUserName = GetUserName.Substring(0, GetUserName.LastIndexOf("/"));
                DirectLink2Post = $"{WebAdress.Substring(0, WebAdress.IndexOf("/@"))}{DirectLink2Post.Replace("/@undefined", GetUserName)}";

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

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, DirectLink2Post, DirectLink2Post, new string[] { DirectLink2Post, null }))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }

            if (TreeViewParentNode.Nodes.Count == 0)
            {
                TreeViewParentNode.Remove();
            }
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            string[] MastodonData = (string[])e.Argument;
            string WebAdress = MastodonData[0];
            string HTMLSource = MastodonData[1];
            Module_Grabber.Report_Info(Grab(WebAdress, HTMLSource).ToString());
            ((BackgroundWorker)sender).Dispose();
        }

        public static string Grab(string WebAdress, string HTMLSource)
        {
            //https://baraag.net/api/v1/statuses/<status_id>
            //https://baraag.net/api/v1/accounts/<account_id>/statuses?only_media=true&limit=40
            //https://baraag.net/api/v1/accounts/<account_id>/statuses?max_id=<status_id>&only_media=true&limit=40

            DataTable TempDataTable = new DataTable();
            Module_TableHolder.Create_DBTable(ref TempDataTable);

            string Post_URL = WebAdress;
            DateTime Post_Time;
            string ArtistName;
            string Post_Text = null;
            string Post_MediaURL;
            int SkipCounter = 0;

            HtmlDocument WebDoc = new HtmlDocument();
            if (HTMLSource == null)
            {
                Uri WebUri = new Uri(Post_URL);
                string APIURL = $"https://{WebUri.Host}/api/v1/statuses/{Post_URL.Substring(Post_URL.LastIndexOf("/") + 1)}";

                //wants session_id in cookie
                JObject JSONData = JObject.Parse(Module_Grabber.GrabPageSource(APIURL, ref Module_CookieJar.Cookies_Mastodon));

                Post_Time = JSONData["created_at"].Value<DateTime>();

                ArtistName = $"{JSONData["account"]["display_name"].Value<string>()} (@{JSONData["account"]["username"].Value<string>()})";

                if (!string.IsNullOrEmpty(JSONData["content"].Value<string>()))
                {
                    WebDoc.LoadHtml(JSONData["content"].Value<string>());

                    HtmlNode Post_TextNode = WebDoc.DocumentNode;
                    if (Post_TextNode != null && !string.IsNullOrEmpty(Post_TextNode.InnerText.Trim()))
                    {
                        Post_Text = Module_Html2Text.Html2Text_Mastodon(Post_TextNode);
                    }
                }

                if (JSONData["media_attachments"] != null)
                {
                    foreach (JToken MediaNode in JSONData["media_attachments"].Children())
                    {
                        Post_MediaURL = MediaNode["url"].Value<string>();
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
                        JToken OrigData = MediaNode["meta"].Any() && MediaNode["meta"]["original"] != null ? MediaNode["meta"]["original"] : null;
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, MediaNode["preview_url"].Value<string>(), ArtistName, OrigData == null ? null : OrigData["width"].Value<string>(), OrigData == null ? null : OrigData["height"].Value<string>());
                        TempDataTable.Rows.Add(TempDataRow);
                    }
                }
            }
            else
            {
                WebDoc.LoadHtml(HTMLSource);

                HtmlNode PostNode = WebDoc.DocumentNode;

                Post_Time = DateTime.ParseExact(PostNode.SelectSingleNode(".//a[@class='detailed-status__datetime']/span").InnerText, "MMM dd, yyyy, HH:mm", null);

                ArtistName = PostNode.SelectSingleNode(".//span[@class='display-name']//strong[@class='display-name__html']").InnerText.Trim();
                ArtistName += " (" + PostNode.SelectSingleNode(".//span[@class='display-name']/span[@class='display-name__account']").InnerText.Trim() + ")";

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='status__content']/div");
                if (Post_TextNode != null && !string.IsNullOrEmpty(Post_TextNode.InnerText.Trim()))
                {
                    Post_Text = Module_Html2Text.Html2Text_Mastodon(Post_TextNode.FirstChild);
                }

                HtmlNode MediaNodeHitTest = PostNode.SelectSingleNode(".//div[@class='video-player inline']");
                if (MediaNodeHitTest != null)
                {
                    HtmlNode VideoNodeHitTest = MediaNodeHitTest.SelectSingleNode(".//video");
                    if (VideoNodeHitTest != null)
                    {
                        Post_MediaURL = VideoNodeHitTest.Attributes["src"].Value;
                        if (!Module_Grabber._Grabbed_MediaURLs.Contains(Post_MediaURL))
                        {
                            lock (Module_Grabber._Grabbed_MediaURLs)
                            {
                                Module_Grabber._Grabbed_MediaURLs.Add(Post_MediaURL);
                            }
                            DataRow TempDataRow = TempDataTable.NewRow();
                            FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, VideoNodeHitTest.Attributes["poster"].Value, ArtistName, null, null);
                            TempDataTable.Rows.Add(TempDataRow);
                        }
                    }
                }
                MediaNodeHitTest = PostNode.SelectSingleNode(".//div[@class='media-gallery']");
                if (MediaNodeHitTest != null)
                {
                    //remove retweets
                    HtmlNode RTHitTest = PostNode.SelectSingleNode(".//video");
                    if (RTHitTest == null)
                    {
                        foreach (HtmlNode MediaNode in MediaNodeHitTest.SelectNodes(".//img"))
                        {
                            Post_MediaURL = MediaNode.ParentNode.Attributes["href"].Value;
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
                            FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, MediaNode.Attributes["src"].Value, ArtistName, null, null);
                            TempDataTable.Rows.Add(TempDataRow);
                            Thread.Sleep(Module_Grabber.PauseBetweenImages);
                        }
                    }
                }
            }

            string PrintText;
            if (TempDataTable.Rows.Count == 0)
            {
                lock (Module_Grabber._GrabQueue_WorkingOn)
                {
                    Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                }
                PrintText = $"Grabbing skipped - {(SkipCounter > 0 ? "All media already grabbed" : "No media found")} [@{Post_URL}]";
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

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string TextBody, string MediaURL, string ThumbnailURL, string Artist, string ImageWidth = null, string ImageHeight = null)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("Created by {0}", Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL.Replace("small", "original");
            TempDataRow["Grab_ThumbnailURL"] = ThumbnailURL;
            if (ImageWidth != null) TempDataRow["Thumbnail_FullInfo"] = true;
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            if (ImageWidth != null) TempDataRow["Info_MediaWidth"] = ImageWidth;
            if (ImageHeight != null) TempDataRow["Info_MediaHeight"] = ImageHeight;
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}