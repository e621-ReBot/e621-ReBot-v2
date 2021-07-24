using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Mastodon
    {
        public static void QueuePrep(string WebAdress)
        {
            //Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Mastodon);
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
                Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress, true, new string[] { WebAdress, WebDocTemp.DocumentNode.SelectSingleNode(".//div[@class='p-author h-card']").ParentNode.OuterHtml });
            }
        }

        private static void Queue_Multi(string WebAdress, string HTMLSource)
        {
            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            HtmlNodeCollection MediaNodeTest = WebDoc.DocumentNode.SelectNodes(".//div[@data-component='MediaGallery']");
            if (MediaNodeTest == null)
            {
                Module_Grabber.Report_Info(string.Format("Skipped grabbing - Page contains no media [@{0}]", WebAdress));
                return;
            }

            TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

            foreach (HtmlNode ChildNode in MediaNodeTest)
            {
                HtmlNode ChildNodeHolder = ChildNode.ParentNode;

                string DirectLink2Post = ChildNodeHolder.SelectSingleNode(".//a[@class='status__relative-time u-url u-uid']").Attributes["href"].Value;
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

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, DirectLink2Post, DirectLink2Post, new string[] { DirectLink2Post, ChildNodeHolder.OuterHtml }))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            string[] MastodonData = (string[])e.Argument;
            string WebAdress = MastodonData[0];
            string HTMLSource = MastodonData[1];
            Grab(WebAdress, HTMLSource);
            ((BackgroundWorker)sender).Dispose();
        }

        private static void Grab(string WebAdress, string HTMLSource)
        {
            DataTable TempDataTable = new DataTable();
            Module_TableHolder.Create_DBTable(ref TempDataTable);

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(HTMLSource);

            string Post_URL = WebAdress;

            HtmlNode PostNode = WebDoc.DocumentNode;

            DateTime Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//time").Attributes["datetime"].Value);

            string ArtistName = PostNode.SelectSingleNode(".//span[@class='display-name']//strong[@class='display-name__html p-name emojify']").InnerText.Trim();
            ArtistName += " (" + PostNode.SelectSingleNode(".//span[@class='display-name']/span[@class='display-name__account']").InnerText.Trim() + ")";

            HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='status__content emojify']");
            string Post_Text = Post_TextNode.InnerText;
            if (Post_Text != null)
            {
                Post_Text = WebUtility.HtmlDecode(Post_Text).Trim();
            }

            string Post_MediaURL;
            int SkipCounter = 0;
            if (PostNode.SelectSingleNode(".//div[@data-component='MediaGallery']") != null)
            {
                JObject MediaJObject = JObject.Parse(HttpUtility.HtmlDecode(PostNode.SelectSingleNode(".//div[@data-component='MediaGallery']").Attributes["data-props"].Value));

                if (MediaJObject["media"].Children().Any())
                {
                    foreach (JToken ImageNode in MediaJObject["media"].Children())
                    {
                        Post_MediaURL = ImageNode["url"].Value<string>();
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
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, ImageNode["preview_url"].Value<string>(), ImageNode["meta"]["original"]["width"].Value<string>(), ImageNode["meta"]["original"]["height"].Value<string>(), ArtistName);
                        TempDataTable.Rows.Add(TempDataRow);
                    }
                }
            }

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

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string TextBody, string MediaURL, string ThumbnailURL, string ImageWidth, string ImageHeight, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("Created by {0}", Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbnailURL;
            TempDataRow["Thumbnail_FullInfo"] = true;
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            TempDataRow["Info_MediaWidth"] = ImageWidth;
            TempDataRow["Info_MediaHeight"] = ImageHeight;
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}
