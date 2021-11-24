using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_FurAffinity
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_FurAffinity);
            if (WebAdress.Contains("furaffinity.net/gallery/")
            || WebAdress.Contains("furaffinity.net/scraps/")
            || WebAdress.Contains("furaffinity.net/favorites/")
            || WebAdress.Contains("furaffinity.net/search/"))
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

            HtmlNodeCollection HtmlNodecollectionTemp = WebDoc.DocumentNode.SelectNodes(".//section[contains(@id, 'gallery')]/figure");
            if (HtmlNodecollectionTemp.Count == 0)
            {
                return;
            }

            foreach (HtmlNode ChildNode in HtmlNodecollectionTemp)
            {
                HtmlNode figcaptionNode = ChildNode.SelectSingleNode("./figcaption");
                string DirectLink2Post = "https://www.furaffinity.net" + figcaptionNode.SelectSingleNode(".//a").Attributes["href"].Value;
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

                string SubmissionType = ChildNode.Attributes["class"].Value;
                if (!SubmissionType.Contains("t-image"))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Unsupported submission type: {0} [@{1}]", SubmissionType, DirectLink2Post));
                    continue;
                }

                string WorkTitle = WebUtility.HtmlDecode(figcaptionNode.SelectSingleNode(".//a").Attributes["title"].Value);
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
        }

        public static string Grab(string WebAdress)
        {
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_FurAffinity);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode("html");

                string Post_TimeTemp = PostNode.SelectSingleNode(".//span[@class='popup_date']").Attributes["title"].Value;
                if (!(Post_TimeTemp.Contains("AM") || Post_TimeTemp.Contains("PM")))
                {
                    Post_TimeTemp = PostNode.SelectSingleNode(".//span[@class='popup_date']").InnerText;
                }
                DateTime Post_Time = DateTime.Parse(Post_TimeTemp);

                string Post_Title = PostNode.SelectSingleNode(".//div[@class='submission-title' or @class='classic-submission-title information']/h2").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = PostNode.SelectSingleNode(".//div[@class='submission-id-sub-container' or @class='classic-submission-title information']/a").InnerText.Trim();

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='submission-description' or @class='submission-description user-submitted-links']");
                if (Post_TextNode == null) //classic theme selected
                {
                    Post_TextNode = PostNode.SelectSingleNode(".//div[@id='page-submission']//table[@class='maintable']//table[@class='maintable']/tr[2]/td"); ;
                }
                string Post_Text = Module_Html2Text.Html2Text_FurAffinity(Post_TextNode);

                string Post_MediaURL = null;
                HtmlNode DownloadNode = PostNode.SelectSingleNode(".//div[@class='download' or @class='download fullsize']/a");
                if (DownloadNode == null) //classic theme selected
                {
                    DownloadNode = PostNode.SelectSingleNode(".//div[@id='page-submission']//div[@class='alt1 actions aligncenter']//a[text()='Download']");
                }
                Post_MediaURL = "https:" + DownloadNode.Attributes["href"].Value;
                // View: https://d.facdn.net/art/artist/id/1578103468.alphathewerewolf_1a4f642b-5935-429f-a048-90ec115235e3_jpeg.jpg
                // Download: https://d.facdn.net/download/art/artist/id/1578103468.alphathewerewolf_1a4f642b-5935-429f-a048-90ec115235e3_jpeg.jpg
                Post_MediaURL = Post_MediaURL.Replace("/download/", "/");

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

                HtmlNode PicSizeNode = PostNode.SelectSingleNode(".//section[@class='info text']/div[4]/span | .//td[@class='alt1 stats-container']//b[text()='Resolution:']/following-sibling::text()");
                string[] PicSizes = PicSizeNode.InnerText.Trim().Replace(" x ","x").Split(new string[] { "x" }, StringSplitOptions.RemoveEmptyEntries);

                DataRow TempDataRow = TempDataTable.NewRow();
                FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, PicSizes[0], PicSizes[1], ArtistName);
                TempDataTable.Rows.Add(TempDataRow);

            Skip2Exit:
                string PrintText;
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_WorkingOn)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    PrintText = $"Grabbing skipped - Media already grabbed [@{Post_URL}]";
                }
                else
                {
                    Module_Grabber._GrabQueue_WorkingOn[Post_URL] = TempDataTable;
                    PrintText = $"Finished grabbing: {Post_URL}";
                }
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Remove(Post_URL);
                }
                return PrintText;
            }
            return "Error encountered during FurAffinity grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ImageWidth, string ImageHeight, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on FurAffinity", Title, Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            string ThumbnailURLTemp = string.Format("https://t.facdn.net/{0}@200-{1}.jpg", URL.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last(), MediaURL.Remove(MediaURL.LastIndexOf("/")).Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last());
            TempDataRow["Grab_ThumbnailURL"] = ThumbnailURLTemp;
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
