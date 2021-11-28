using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_HentaiFoundry
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_HentaiFoundry);
            string[] WebAdressSplit = WebAdress.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (WebAdress.Contains("hentai-foundry.com/pictures/user/") && WebAdressSplit.Length == 7 && !WebAdressSplit[5].Equals("page"))
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

            HtmlNodeCollection HtmlNodecollectionTemp = WebDoc.DocumentNode.SelectNodes(".//div[@id='yw0']//div[@class='thumb_square']");
            if (HtmlNodecollectionTemp == null || HtmlNodecollectionTemp.Count == 0)
            {
                return;
            }

            foreach (HtmlNode ChildNode in HtmlNodecollectionTemp)
            {
                string DirectLink2Post = "https://www.hentai-foundry.com" + ChildNode.SelectSingleNode(".//a[@class='thumbLink']").Attributes["href"].Value;
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

                string WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode(".//div[@class='thumbTitle']").InnerText.Trim());
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
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_HentaiFoundry);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode("html");

                string Post_TimeTemp = PostNode.SelectSingleNode(".//section[@id='yw0']//time[@datetime]").Attributes["datetime"].Value;
                DateTime Post_Time = DateTime.Parse(Post_TimeTemp);

                string Post_Title = PostNode.SelectSingleNode(".//section[@id='picBox']//span[@class='imageTitle']").InnerText.Trim();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = PostNode.SelectSingleNode(".//section[@id='picBox']//a").InnerText.Trim();

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//section[@id='descriptionBox']//div[@class='picDescript']");
                string Post_Text = null; //Module_Html2Text.Html2Text_HentaiFoundry(Post_TextNode);

                HtmlNode ImageNodeTest = PostNode.SelectSingleNode(".//section[@id='picBox']/div[@class='boxbody']").SelectSingleNode(".//img | .//embed");
                string Post_MediaURL = "https:" + ImageNodeTest.Attributes["src"].Value;
                if (Post_MediaURL.Contains("/thumb.php"))
                {
                    Post_MediaURL = "https:" + PostNode.SelectSingleNode(".//section[@id='picBox']//img[@class='center']").Attributes["onClick"].Value.Split(new string[] { "&#039;" }, StringSplitOptions.RemoveEmptyEntries)[1];
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
            return "Error encountered during Hentai Foundry grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on Hentai Foundry", Title, Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            if (MediaURL.EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
            {
                TempDataRow["Thumbnail_Image"] = new Bitmap(Properties.Resources.E6Image_Flash);
            }
            else
            {
                string picID = MediaURL.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[4];
                TempDataRow["Grab_ThumbnailURL"] = $"https://thumbs.hentai-foundry.com/thumb.php?pid={picID}&size=200";
            }
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}