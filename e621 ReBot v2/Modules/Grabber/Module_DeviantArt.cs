using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_DeviantArt
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_DeviantArt);
            if (WebAdress.Contains("deviantart.com/") && WebAdress.Contains("/art/"))
            {
                Queue_Single(WebAdress);
            }
            else
            {
                if (FolderID == null)
                {
                    Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
                }
                else
                {
                    Queue_Multi_API(WebAdress);
                }
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

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes("//div[@class='_3kK_E']//a[@data-hook='deviation_link']"))
            {
                string DirectLink2Post = ChildNode.Attributes["href"].Value;
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

        public static string FolderID;
        private static void Queue_Multi_API(string WebAdress)
        {
            //https://www.deviantart.com/developers/http/v1/20150217/gallery_folders/f6104e0d969bbbdcf2154e4b221aa3a6
            //https://www.deviantart.com/_napi/da-user-profile/api/gallery/contents?username=UserName&offset=0&limit=50&folderid=01234567
            bool hasMore = false;
            string UserName = WebAdress.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Reverse().Skip(1).Take(1).FirstOrDefault();
            int FolderOffset = 0;
            do
            {
                string DataSource = Module_Grabber.GrabPageSource(string.Format("https://www.deviantart.com/_napi/da-user-profile/api/gallery/contents?username={0}&offset={1}&limit=50&folderid={2}", UserName, FolderOffset, FolderID), ref Module_CookieJar.Cookies_DeviantArt);
                if (DataSource != null && DataSource.Length > 50)
                {
                    JToken JSONHolder = JObject.Parse(DataSource);
                    hasMore = JSONHolder["hasMore"].Value<bool>();
                    FolderOffset += 50;

                    foreach (JToken JTokenTemp in JSONHolder["results"])
                    {
                        TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(WebAdress, WebAdress);

                        string DirectLink2Post = JTokenTemp["deviation"]["url"].Value<string>();
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

                        JToken JTokenTempData = JTokenTemp;
                        string WorkTitle = JTokenTemp["deviation"]["title"].Value<string>();
                        if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, WorkTitle, DirectLink2Post, JTokenTempData))
                        {
                            Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                        }
                    }
                }
                else
                {
                    hasMore = false;
                }
                Thread.Sleep(500);
            } while (hasMore);
        }

        public static void RunGrabber(object sender, DoWorkEventArgs e)
        {
            Grab(e.Argument.ToString());
            ((BackgroundWorker)sender).Dispose();
        }

        private static void Grab(string WebAdress)
        {
            //https://www.deviantart.com/_napi/da-browse/shared_api/deviation/extended_fetch?deviationid=802459566&type=art&include_session=false
            //https://www.deviantart.com/_napi/da-browse/shared_api/deviation/extended_fetch?deviationid=878047560&type=art&include_session=false
            string DataSource = WebAdress.StartsWith("{") ? WebAdress : Module_Grabber.GrabPageSource(string.Format("https://www.deviantart.com/_napi/da-browse/shared_api/deviation/extended_fetch?deviationid={0}&type=art&include_session=false", WebAdress.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries).Last()), ref Module_CookieJar.Cookies_DeviantArt);
            JToken JSONHolder = null;
            if (DataSource != null && !DataSource.StartsWith("{\"error\":", StringComparison.OrdinalIgnoreCase))
            {
                JSONHolder = JObject.Parse(DataSource)["deviation"];
            }
            else
            {
                DataSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_DeviantArt);
            }

            if (DataSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                string Post_URL = WebAdress;
                DateTime Post_Time = DateTime.UtcNow;
                string Post_Title = null;
                string ArtistName = null;
                string Post_Text = null;
                string Post_MediaURL = null;
                string Post_Thumb = null;
                int[] ImageSizes = null;

                if (JSONHolder != null)
                {
                    Post_URL = JSONHolder["url"].Value<string>();
                    Post_Time = JSONHolder["publishedTime"].Value<DateTime>();
                    Post_Title = JSONHolder["title"].Value<string>();
                    ArtistName = JSONHolder["author"]["username"].Value<string>();
                    Post_Text = ""; //JSONHolder["extended"]["description"].Value<string>();

                    string TokenTest = JSONHolder["media"]["token"] != null ? JSONHolder["media"]["token"].Last.Value<string>() : null;
                    if (JSONHolder["media"]["types"].Last["c"] != null)
                    {
                        string FixView = JSONHolder["media"]["types"].Last["c"].Value<string>().Replace("<prettyName>", JSONHolder["media"]["prettyName"].Value<string>());
                        FixView = string.Format("{0}100{1}", FixView.Substring(0, FixView.IndexOf(",q_") + 3), FixView.Substring(FixView.IndexOf(",strp/")));
                        Post_MediaURL = string.Format("{0}/{1}", JSONHolder["media"]["baseUri"].Value<string>(), FixView);
                        if (TokenTest != null)
                        {
                            Post_MediaURL += string.Format("?token={0}", TokenTest);
                        }
                    }
                    else
                    {
                        Post_MediaURL = JSONHolder["media"]["baseUri"].Value<string>();
                        if (TokenTest != null)
                        {
                            Post_MediaURL += string.Format("?token={0}", TokenTest);
                        }
                    }
                    if (JSONHolder["media"]["types"][2]["c"] != null)
                    {
                        Post_Thumb = string.Format("{0}/{1}", JSONHolder["media"]["baseUri"].Value<string>(), JSONHolder["media"]["types"][2]["c"].Value<string>().Replace("<prettyName>", JSONHolder["media"]["prettyName"].Value<string>()));
                        if (TokenTest != null)
                        {
                            Post_Thumb += string.Format("?token={0}", TokenTest);
                        } 
                    }
                    else
                    {
                        Post_Thumb = Post_MediaURL; //gifs https://www.deviantart.com/leokatana/art/Commission-Varali-100x100-animated-pixel-doll-878961662
                    }

                    if (JSONHolder["extended"] != null && JSONHolder["extended"]["originalFile"] != null)
                    {
                        ImageSizes = new int[3];
                        ImageSizes[0] = JSONHolder["extended"]["originalFile"]["width"].Value<int>();
                        ImageSizes[1] = JSONHolder["extended"]["originalFile"]["height"].Value<int>();
                        ImageSizes[2] = JSONHolder["extended"]["originalFile"]["filesize"].Value<int>();
                    }
                }
                else
                {
                    HtmlDocument WebDoc = new HtmlDocument();
                    WebDoc.LoadHtml(DataSource);

                    HtmlNode PostNode = WebDoc.DocumentNode.SelectSingleNode("html");

                    string Post_TimeTemp = PostNode.SelectSingleNode(".//span//time").Attributes["datetime"].Value;
                    Post_Time = DateTime.Parse(Post_TimeTemp);

                    Post_Title = PostNode.SelectSingleNode(".//h1[@data-hook='deviation_title']").InnerText.Trim();
                    Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                    ArtistName = PostNode.SelectSingleNode(".//div[@data-hook='deviation_meta']//a[@data-hook='user_link']").Attributes["data-username"].Value;

                    //HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//div[@class='elephant elephant_bottom elephant_white']/div[@class='content']//span");
                    Post_Text = ""; //Module_Html2Text.Html2Text_DeviantArt(Post_TextNode);

                    Post_MediaURL = PostNode.SelectSingleNode(".//div[@data-hook='art_stage']//img").Attributes["src"].Value.Replace(",q_75,strp/", ",q_100,strp/"); ;
                    Post_Thumb = Post_MediaURL;
                    HtmlNode DLButton = PostNode.SelectSingleNode(".//section[@data-hook='action_bar']//a[@data-hook='download_button']");
                    if (DLButton != null)
                    {
                        Post_MediaURL = HttpUtility.HtmlDecode(DLButton.Attributes["href"].Value);
                    } 

                    //https://www.deviantart.com/rkasai14/art/Isekai-Maou-to-Shoukan-Shoujo-no-Dorei-Majutsu-18-877266841 has zip file
                    //HtmlNode ImageSizeNodeParent = PostNode.SelectSingleNode(".//div[@data-hook='deviation_meta']").ParentNode;
                    //ImageSizes = ImageSizeNodeParent.ChildNodes[ImageSizeNodeParent.ChildNodes.Count - 3].LastChild.InnerText.Trim().Replace("Image detailsImage size", ""); //-3 is actually -2 since index starts at 0
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
                FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, Post_Thumb, ImageSizes, ArtistName);
                TempDataTable.Rows.Add(TempDataRow);

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
                    Module_Grabber.Report_Info(PrintText);
                }
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Remove(Post_URL);
                }
            }

        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ThumbURL, int[] ImageSizes, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on DeviantArt", Title, Artist)); ;
            if (TextBody != null)
            {
                TempDataRow["Grab_TextBody"] = TextBody;
            } 
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbURL;
            string FormatFinder = MediaURL.Substring(0, MediaURL.IndexOf("?token="));
            TempDataRow["Info_MediaFormat"] = FormatFinder.Substring(FormatFinder.LastIndexOf(".") + 1); //-\w+(?:\.\w+\?token)

            //int ThumbWidth = 200;
            //int ThumbHeight = 200;
            //if (!ImageSizes.Equals(""))
            //{
            //    TempDataRow["Info_MediaWidth"] = ImageSizes.Substring(0, ImageSizes.IndexOf("x"));
            //    ImageSizes = ImageSizes.Substring(ImageSizes.IndexOf("x") + 1);
            //    TempDataRow["Info_MediaHeight"] = ImageSizes.Substring(0, ImageSizes.IndexOf("px"));

            //    if (TempDataRow["Info_MediaWidth"] != TempDataRow["Info_MediaHeight"])
            //    {
            //        double ScaleFactor = (double)200 / Math.Max((int)TempDataRow["Info_MediaWidth"], (int)TempDataRow["Info_MediaHeight"]);
            //        ThumbWidth = Math.Min(200, (int)((int)TempDataRow["Info_MediaWidth"] * ScaleFactor));
            //        ThumbHeight = Math.Min(200, (int)((int)TempDataRow["Info_MediaHeight"] * ScaleFactor));
            //    }
            //}
            //if (ThumbURL.Contains("?token="))
            //{
            //    string ThumbConstructor = ThumbURL.Substring(0, ThumbURL.IndexOf("/v1/")) + string.Format("/v1/fill/w_{0},h_{1},q_70,strp/", ThumbWidth, ThumbHeight) + ThumbURL.Substring(ThumbURL.IndexOf(",strp/") + 6);
            //    TempDataRow["Grab_ThumbnailURL"] = Regex.Replace(ThumbConstructor, @"-\w+(?=.\w+\?token=)", "");
            //}
            if (ImageSizes != null)
            {
                TempDataRow["Info_MediaWidth"] = ImageSizes[0];
                TempDataRow["Info_MediaHeight"] = ImageSizes[1];
                TempDataRow["Info_MediaByteLength"] = ImageSizes[2];//Module_Grabber.GetMediaSize(MediaURL);
            }
            //ImageSizes = ImageSizes.Substring(ImageSizes.IndexOf("px") + 3);

            //TempDataRow["Info_MediaByteLength"] = (int)double.Parse(ImageSizes.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);
            //TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            //TempDataRow["Thumbnail_FullInfo"] = true;
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}
