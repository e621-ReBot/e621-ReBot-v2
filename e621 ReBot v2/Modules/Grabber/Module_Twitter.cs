﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Twitter
    {

        public static void QueuePrep(string WebAdress)
        {
            //Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Twitter);
            if (WebAdress.Contains("/status/"))
            {
                if (WebAdress.Contains("/photo/"))
                {
                    // Queue_PhotoStatus(WebAdress)
                }
                else
                {
                    Queue_Status(WebAdress, Module_CefSharp.GetHTMLSource());
                }
            }
            else
            {
                Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
            }
        }

        private static void Queue_Status(string WebAdress, string HTMLSource)
        {
            if (Module_Grabber._GrabQueue_URLs.Contains(WebAdress))
            {
                Module_Grabber.Report_Info($"Skipped grabbing - Already in queue [@{WebAdress}]");
                return;
            }
            else
            {
                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                HtmlNode TweetNode = WebDoc.DocumentNode.SelectSingleNode($".//article[@data-testid='tweet']//a[@role='link' and @href='{WebAdress.Substring(19)}']/ancestor::article"); //[text()='·'] does not work, AND should be and
                if (TweetNode == null)
                {
                    TweetNode = WebDoc.DocumentNode.SelectSingleNode(".//span[.='·']/ancestor::article");
                }

                if (TweetNode.InnerHtml.Contains("This Tweet is unavailable."))
                {
                    Module_Grabber.Report_Info($"Skipped grabbing - Tweet deleted [@{WebAdress}]");
                    return;
                }

                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Add(WebAdress);
                }
                Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress, true, new string[] { WebAdress, TweetNode.InnerHtml });
            }
        }

        private static void Queue_Multi(string WebAdress, string HTMLSource)
        {
            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            // TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

            TreeNode TreeViewParentNode;
            bool AddNodeCheck = true;
            if (!Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.ContainsKey(WebAdress))
            {
                TreeViewParentNode = new TreeNode
                {
                    Text = WebAdress,
                    Name = WebAdress,
                    ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_cTreeView,
                    Checked = true
                };
            }
            else
            {
                TreeViewParentNode = Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.Find(WebAdress, false)[0];
                AddNodeCheck = false;
            }

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectSingleNode(".//h1[@id and @role='heading' and @dir]").NextSibling.SelectNodes(".//article[@data-testid='tweet']"))
            {
                HtmlNode IsTweetRelevantTest = ChildNode.SelectSingleNode("./div/div/div");
                if (IsTweetRelevantTest.FirstChild.InnerText.Length > 0)
                {
                    continue; //It's not a tweet, it's a retweet or pin
                }
                string DirectLink2Post = "https://twitter.com" + IsTweetRelevantTest.LastChild.SelectSingleNode(".//time[@datetime]").ParentNode.Attributes["href"].Value;
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

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, DirectLink2Post, DirectLink2Post, new string[] { WebAdress, IsTweetRelevantTest.LastChild.SelectSingleNode("./div[2]").OuterHtml }))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }
            if (AddNodeCheck && TreeViewParentNode.Nodes.Count > 0)
            {
                TreeViewParentNode.Tag = TreeViewParentNode.Nodes.Count;
                Form_Loader._FormReference.cTreeView_GrabQueue.Nodes.Add(TreeViewParentNode);
            }
            if (!Form_Loader._FormReference.BB_Grab_All.Text.Equals("Stop")) Form_Loader._FormReference.BB_Grab.Visible = true;
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            string[] TwitterData = (string[])e.Argument;
            string WebAdress = TwitterData[0];
            string HTMLSource = TwitterData[1];
            if (WebAdress.Contains("/status/"))
            {
                if (WebAdress.Contains("/photo/"))
                {
                    // Grab_PhotoStatus(WebAdress)
                }
                else
                {
                    Grab_Status(WebAdress, HTMLSource);
                }
            }
            else
            {
                Grab_Tweet(WebAdress, HTMLSource);
            }
            ((BackgroundWorker)sender).Dispose();
        }

        public static void Grab_Status(string WebAdress, string HTMLSource)
        {
            DataTable TempDataTable = new DataTable();
            Module_TableHolder.Create_DBTable(ref TempDataTable);

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            string Post_URL = WebAdress;

            HtmlNode PostNode = WebDoc.DocumentNode;

            //DateTime.Parse(Post_TimeTemp, "h:mm tt · MMM d, yyyy", CultureInfo.InvariantCulture);
            //.//span[text()[contains(.,'M · ')]]
            DateTime Post_Time;

            string ArtistName = PostNode.SelectSingleNode(".//div[@id and @data-testid='User-Names']").InnerText;
            ArtistName = ArtistName.Replace("@", " (@") + ")";

            HtmlNode TestTextNode = PostNode.SelectSingleNode(".//div[@id and @data-testid='tweetText']");
            string Post_Text = null;
            if (TestTextNode != null)
            {
                Post_Text = TestTextNode.InnerText;
                if (Post_Text != null)
                {
                    Post_Text = WebUtility.HtmlDecode(Post_Text).Trim();
                }
            }

            string Post_MediaURL;
            int SkipCounter = 0;
            HtmlNodeCollection VideoNodes = PostNode.SelectNodes(".//div[@data-testid='tweetPhoto']//img[@alt='Embedded video']");
            if (VideoNodes != null)
            {
                //HtmlNode VideoNodeTest = PostNode.SelectSingleNode(".//div[@data-testid='previewInterstitial'] | .//video");
                string JSONData = Grab_TwitterStatus_API(Post_URL);
                JObject ParseStatusJSON = JObject.Parse(JSONData);

                JToken MediaHolder = ParseStatusJSON["extended_entities"];
                if (MediaHolder != null)
                {
                    Post_Time = DateTime.ParseExact(ParseStatusJSON["created_at"].Value<string>(), "ddd MMM dd HH:mm:ss K yyyy", null);
                    SkipCounter = ScrapeJSON4TweetData(MediaHolder, ref TempDataTable, Post_URL, Post_Time, Post_Text, ArtistName);
                }
            }
            else
            {
                Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//a[@aria-label and @role='link']/time").Attributes["datetime"].Value);

                HtmlNodeCollection ImageNodes = PostNode.SelectNodes(".//div[@data-testid='tweetPhoto']//img[@alt='Image']");
                if (ImageNodes == null) //Fix for translated twitter HTML. Make it a "While null" loop in case more regions are needed?
                {
                    ImageNodes = PostNode.SelectNodes(".//div[@data-testid='tweetPhoto']//img[@alt='Imagem']"); //image -> imagem | Brazilian Portuguese
                }
                if (ImageNodes != null)
                {
                    //twitter carousel displayes them strangely when there's 4. 1-3-2-4 (as html order).
                    if (ImageNodes.Count == 4)
                    {
                        (ImageNodes[2], ImageNodes[1]) = (ImageNodes[1], ImageNodes[2]);
                    }

                    foreach (HtmlNode ImageNode in ImageNodes)
                    {
                        string PictureLinkHolder = ImageNode.Attributes["src"].Value;
                        Post_MediaURL = PictureLinkHolder.Substring(0, PictureLinkHolder.IndexOf("?"));
                        PictureLinkHolder = PictureLinkHolder.Substring(PictureLinkHolder.IndexOf("=") + 1);
                        PictureLinkHolder = PictureLinkHolder.Remove(PictureLinkHolder.IndexOf("&"));
                        Post_MediaURL += "." + PictureLinkHolder;

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
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, Post_MediaURL, ArtistName);
                        TempDataTable.Rows.Add(TempDataRow);

                        Thread.Sleep(Module_Grabber.PauseBetweenImages);
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
            Module_Grabber.Report_Info(PrintText);
        }

        private static void Grab_Tweet(string WebAdress, string HTMLSource)
        {
            DataTable TempDataTable = new DataTable();
            Module_TableHolder.Create_DBTable(ref TempDataTable);

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            HtmlNode PostNode = WebDoc.DocumentNode;

            string Post_URL = $"https://twitter.com{PostNode.SelectSingleNode(".//time").ParentNode.Attributes["href"].Value}";
            string TweetID = PostNode.SelectSingleNode(".//time").ParentNode.Attributes["href"].Value;
            TweetID = TweetID.Substring(TweetID.LastIndexOf("/") + 1);

            DateTime Post_Time;

            string ArtistName = PostNode.SelectSingleNode(".//div[@id and @data-testid='User-Names']").InnerText;
            ArtistName = ArtistName.Substring(0, ArtistName.IndexOf("·")).Replace("@", " (@") + ")";

            string Post_Text = null;
            HtmlNode PostNodeTest = PostNode.SelectSingleNode(".//div[@id and @dir='auto']");
            if (PostNodeTest != null)
            {
                PostNodeTest = PostNodeTest.ParentNode.FirstChild;
                Post_Text = PostNodeTest.InnerText;
            }
            if (Post_Text != null)
            {
                Post_Text = WebUtility.HtmlDecode(Post_Text).Trim();
            }

            string Post_MediaURL;
            int SkipCounter = 0;
            JToken TweetHolder = null;
            if (TwitterJSONHolder != null)
            {
                lock (TwitterJSONHolder)
                {
                    TweetHolder = TwitterJSONHolder.Children().Where(JT => (string)JT["rest_id"] == TweetID).FirstOrDefault();
                }
            }

            if (TwitterJSONHolder != null && TweetHolder != null)
            {
                TweetHolder = TweetHolder["legacy"];
                JToken MediaHolder = TweetHolder["extended_entities"];
                if (MediaHolder != null)
                {
                    Post_Time = DateTime.ParseExact(TweetHolder["created_at"].Value<string>(), "ddd MMM dd HH:mm:ss K yyyy", null);
                    SkipCounter = ScrapeJSON4TweetData(MediaHolder, ref TempDataTable, Post_URL, Post_Time, Post_Text, ArtistName);
                }
            }
            else
            {
                Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//a[@aria-label and @role='link']/time").Attributes["datetime"].Value);

                HtmlNodeCollection VideoNodes = PostNode.SelectNodes(".//div[@data-testid='tweetPhoto']//img[@alt='Embedded video']");
                if (VideoNodes != null)
                {
                    //HtmlNode VideoNodeTest = PostNode.SelectSingleNode(".//div[@data-testid='previewInterstitial'] | .//video");
                    string JSONData = Grab_TwitterStatus_API(Post_URL);
                    JObject ParseStatusJSON = JObject.Parse(JSONData);

                    JToken MediaHolder = ParseStatusJSON["extended_entities"];
                    if (MediaHolder != null)
                    {
                        Post_Time = DateTime.ParseExact(ParseStatusJSON["created_at"].Value<string>(), "ddd MMM dd HH:mm:ss K yyyy", null);
                        SkipCounter = ScrapeJSON4TweetData(MediaHolder, ref TempDataTable, Post_URL, Post_Time, Post_Text, ArtistName);
                    }
                }
                else
                {
                    Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//a[@aria-label and @role='link']/time").Attributes["datetime"].Value);

                    HtmlNodeCollection ImageNodes = PostNode.SelectNodes(".//div[@data-testid='tweetPhoto']//img[@alt='Image']");
                    if (ImageNodes != null)
                    {
                        //twitter carousel displayes them strangely when there's 4. 1-3-2-4 (as html order).
                        if (ImageNodes.Count == 4)
                        {
                            (ImageNodes[2], ImageNodes[1]) = (ImageNodes[1], ImageNodes[2]);
                        }

                        foreach (HtmlNode ImageNode in ImageNodes)
                        {
                            string PictureLinkHolder = ImageNode.Attributes["src"].Value;
                            Post_MediaURL = PictureLinkHolder.Substring(0, PictureLinkHolder.IndexOf("?"));
                            PictureLinkHolder = PictureLinkHolder.Substring(PictureLinkHolder.IndexOf("=") + 1);
                            PictureLinkHolder = PictureLinkHolder.Remove(PictureLinkHolder.IndexOf("&"));
                            Post_MediaURL += "." + PictureLinkHolder;

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
                            FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, Post_MediaURL, ArtistName);
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
            Module_Grabber.Report_Info(PrintText);
        }

        private static int ScrapeJSON4TweetData(JToken JSONData, ref DataTable TempDataTable, string Post_URL, DateTime Post_Time, string Post_Text, string ArtistName)
        {
            int SkipCounter = 0;
            JToken MediaHolderTemp = null;
            for (int i = 0; i < JSONData["media"].Count(); i++)
            {
                MediaHolderTemp = JSONData["media"][i];
                if (MediaHolderTemp["type"].Value<string>().Equals("video"))
                {
                    JToken BestVideo = null;
                    foreach (JToken VideoCheck in MediaHolderTemp["video_info"]["variants"])
                    {
                        if (VideoCheck["bitrate"] != null)
                        {
                            if (BestVideo == null)
                            {
                                BestVideo = VideoCheck;
                                continue;
                            }

                            if (VideoCheck["bitrate"].Value<int>() > BestVideo["bitrate"].Value<int>())
                            {
                                BestVideo = VideoCheck;
                            }
                        }
                    }
                    string VideoURL = BestVideo["url"].Value<string>();
                    if (VideoURL.Contains("?"))
                    {
                        VideoURL = VideoURL.Substring(0, VideoURL.IndexOf("?"));
                    }

                    if (Module_Grabber._Grabbed_MediaURLs.Contains(VideoURL))
                    {
                        SkipCounter += 1;
                        continue;
                    }
                    else
                    {
                        lock (Module_Grabber._Grabbed_MediaURLs)
                        {
                            Module_Grabber._Grabbed_MediaURLs.Add(VideoURL);
                        }
                    }
                    DataRow TempDataRow = TempDataTable.NewRow();
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, VideoURL, MediaHolderTemp["media_url_https"].Value<string>(), ArtistName, MediaHolderTemp["original_info"]["width"].Value<string>(), MediaHolderTemp["original_info"]["height"].Value<string>());
                    TempDataTable.Rows.Add(TempDataRow);
                }
                else
                {
                    string Post_MediaURL = MediaHolderTemp["media_url_https"].Value<string>();

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
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, Post_MediaURL, ArtistName, MediaHolderTemp["original_info"]["width"].Value<string>(), MediaHolderTemp["original_info"]["height"].Value<string>());
                    TempDataTable.Rows.Add(TempDataRow);
                }
                Thread.Sleep(Module_Grabber.PauseBetweenImages);
            }
            return SkipCounter;
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string TextBody, string MediaURL, string ThumbURL, string Artist, string ImageWidth = null, string ImageHeight = null)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("Created by {0} on Twitter", Artist));
            if (TextBody != null)
            {
                TempDataRow["Grab_TextBody"] = TextBody;
            }
            TempDataRow["Grab_MediaURL"] = MediaURL + (MediaURL.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ? null : ":orig");
            TempDataRow["Grab_ThumbnailURL"] = ThumbURL + ":small";
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            if (ImageWidth != null)
            {
                TempDataRow["Thumbnail_FullInfo"] = true;
                TempDataRow["Info_MediaWidth"] = ImageWidth;
                TempDataRow["Info_MediaHeight"] = ImageHeight;
            }
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize((string)TempDataRow["Grab_MediaURL"]);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }

        public static string TwitterAuthorizationBearer;
        private static string Grab_TwitterStatus_API(string StatusPermalink)
        {
            string StatusID = StatusPermalink.Substring(StatusPermalink.IndexOf("/status/") + "/status/".Length);
            HttpWebRequest Twitter_HTMLRequest = (HttpWebRequest)WebRequest.Create($"https://api.twitter.com/1.1/statuses/show.json?tweet_mode=extended&id={StatusID}"); //("https://twitter.com/i/api/2/timeline/conversation/" & StatusID & ".json")
            Twitter_HTMLRequest.UserAgent = Form_Loader.GlobalUserAgent;

            Module_CookieJar.GetCookies(StatusPermalink, ref Module_CookieJar.Cookies_Twitter);
            Twitter_HTMLRequest.CookieContainer = Module_CookieJar.Cookies_Twitter;
            Dictionary<string, string> TempCookieHolder = new Dictionary<string, string>();
            foreach (Cookie Cookie in Module_CookieJar.Cookies_Twitter.GetCookies(new Uri("https://twitter.com/")))
            {
                TempCookieHolder.Add(Cookie.Name, Cookie.Value);
            }
            Twitter_HTMLRequest.Headers.Add("x-csrf-token", TempCookieHolder["ct0"]);
            //Twitter_HTMLRequest.Headers.Add("x-guest-token", TempCookieHolder("gt")) //Not really needed.
            Twitter_HTMLRequest.Headers.Add("authorization", TwitterAuthorizationBearer);

            HttpWebResponse Twitter_HTMLResponse = (HttpWebResponse)Twitter_HTMLRequest.GetResponse();
            string ReponseString;
            using (StreamReader StreamReaderTemp = new StreamReader(Twitter_HTMLResponse.GetResponseStream()))
            {
                ReponseString = StreamReaderTemp.ReadToEnd();
            }
            Twitter_HTMLResponse.Dispose();

            return ReponseString;
        }

        public static JArray TwitterJSONHolder;
    }
}