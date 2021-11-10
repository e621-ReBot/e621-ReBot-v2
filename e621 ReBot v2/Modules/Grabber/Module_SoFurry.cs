using System;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_SoFurry
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_SoFurry);
            if (WebAdress.Contains("sofurry.com/view/"))
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

            HtmlNodeCollection ArtNodes = WebDoc.DocumentNode.SelectNodes(".//div[@id='yw1']//a[@class='sfArtworkSmallInner']");
            if (ArtNodes == null)
            {
                ArtNodes = WebDoc.DocumentNode.SelectNodes(".//div[@id='yw0']//a[@class='sfArtworkSmallInner']");
            }

            foreach (HtmlNode ChildNode in ArtNodes)
            {
                string DirectLink2Post = "https://www.sofurry.com" + ChildNode.Attributes["href"].Value;
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

                string WorkTitle = WebUtility.HtmlDecode(ChildNode.SelectSingleNode("./img").Attributes["alt"].Value);
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
            string Post_URL = WebAdress;
            string Post_ID = WebAdress.Substring(WebAdress.LastIndexOf("/") + 1);

            string JSONSource = Module_Grabber.GrabPageSource("https://api2.sofurry.com/std/getSubmissionDetails?id=" + Post_ID, ref Module_CookieJar.Cookies_SoFurry);
            if (JSONSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                JObject SoFurryJSON = JObject.Parse(JSONSource);

                bool UnsupportedType = false;
                if (!SoFurryJSON["contentType"].Value<string>().Equals("1")) // (0=story, 1=art, 2=music, 3=journal, 4=photo)
                {
                    UnsupportedType = true;
                    goto Skip2Exit;
                }

                DateTime Post_Time = DateTime.UtcNow;

                string Post_Title = SoFurryJSON["title"].Value<string>();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = SoFurryJSON["author"].Value<string>();

                HtmlDocument WebDocTemp = new HtmlDocument();
                WebDocTemp.LoadHtml(SoFurryJSON["description"].Value<string>());

                string Post_Text = Module_Html2Text.Html2Text_SoFurry(WebDocTemp.DocumentNode);

                // contentSourceUrl is enough but combine with fileName
                string Post_MediaURL = string.Format("{0}&{1}", SoFurryJSON["contentSourceUrl"].Value<string>(), SoFurryJSON["fileName"].Value<string>());
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
                FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, SoFurryJSON["thumbnailSourceUrl"].Value<string>(), ArtistName);
                if (Post_MediaURL.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || Post_MediaURL.EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                {
                    TempDataRow["Info_MediaWidth"] = SoFurryJSON["width"].Value<string>();
                    TempDataRow["Info_MediaHeight"] = SoFurryJSON["height"].Value<string>();
                }
                TempDataTable.Rows.Add(TempDataRow);

            Skip2Exit:
                string PrintText;
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_WorkingOn)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    PrintText = $"Grabbing skipped - {(UnsupportedType ? "Unsupported submission type" : "All media already grabbed")} [@{Post_URL}]";
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
                //Module_Grabber.Report_Info(PrintText);
                return PrintText;
            }
            return "Error encountered during SoFurry grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ThumbnailURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            //TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("\"{0}\" by {1} on SoFurry", Title, Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbnailURL;
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            //TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            //TempDataRow["Upload_Tags"] = DateTime.Year
            TempDataRow["Upload_Tags"] = "";
            TempDataRow["Artist"] = Artist;
        }
    }
}
