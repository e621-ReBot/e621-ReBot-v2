using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using e621_ReBot_v2.CustomControls;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Pixiv
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Pixiv);
            if (WebAdress.Contains("www.pixiv.net/member.php?id=")
            || WebAdress.Contains("www.pixiv.net/member_illust.php?id=")
            || WebAdress.Contains("www.pixiv.net/en/users/"))
            {
                if (MessageBox.Show("Do you want to go grab all works?" + Environment.NewLine + "(If you chose not to, only visible ones will be grabbed.)", "Grab", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Queue_All(WebAdress);
                }
                else
                {
                    Queue_Multi(WebAdress, Module_CefSharp.GetHTMLSource());
                }
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

            string PermalinkFix = WebAdress;
            if (PermalinkFix.Contains("&p="))
            {
                PermalinkFix = PermalinkFix.Remove(PermalinkFix.IndexOf("&p="));
            }

            if (PermalinkFix.Contains("&type="))
            {
                PermalinkFix = PermalinkFix.Remove(PermalinkFix.IndexOf("&type="));
            }

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes(".//div[@type='illust']"))
            {
                string DirectLink2Post = "https://www.pixiv.net" + ChildNode.SelectSingleNode(".//a").Attributes["href"].Value;
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

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, DirectLink2Post, DirectLink2Post))
                {
                    Module_Grabber.Report_Info(string.Format("Skipped grabbing - Already in queue [@{0}]", DirectLink2Post));
                }
            }
        }

        private static void Queue_All(string WebAdress)
        {
            string UserID = WebAdress.Substring("https://www.pixiv.net/en/users/".Length);
            if (UserID.Contains("/"))
            {
                UserID = UserID.Remove(UserID.IndexOf("/"));
            }

            JObject JSONDictionary = JObject.Parse(Module_Grabber.GrabPageSource("https://www.pixiv.net/ajax/user/" + UserID + "/profile/all", ref Module_CookieJar.Cookies_Pixiv));
            string PermalinkFix = WebAdress;
            if (PermalinkFix.Contains("&p="))
            {
                PermalinkFix = PermalinkFix.Remove(PermalinkFix.IndexOf("&p="));
            }

            if (PermalinkFix.Contains("&type="))
            {
                PermalinkFix = PermalinkFix.Remove(PermalinkFix.IndexOf("&type="));
            }

            TreeNode TreeViewParentNode = Module_Grabber.CreateOrFindParentTreeNode(PermalinkFix, PermalinkFix);

            string WorkType = "";
            if (WebAdress.Contains("illustrations"))
            {
                WorkType = "illust";
            }
            else if (WebAdress.Contains("manga"))
            {
                WorkType = "manga";
            }

            var WorkList = new List<string>();
            if (WorkType.Equals(""))
            {
                // Thanks https://stackoverflow.com/questions/16795045/accessing-all-items-in-the-jtoken-json-net/38253969
                if (JSONDictionary["body"]["illusts"].HasValues)
                {
                    WorkList.AddRange(((JObject)JSONDictionary["body"]["illusts"]).Properties().Select(f => f.Name).ToList());
                }
                if (JSONDictionary["body"]["manga"].HasValues)
                {
                    WorkList.AddRange(((JObject)JSONDictionary["body"]["manga"]).Properties().Select(f => f.Name).ToList());
                }
            }
            else
            {
                WorkType += WorkType.Equals("illust") ? "s" : null;
                WorkList.AddRange(((JObject)JSONDictionary["body"][WorkType]).Properties().Select(f => f.Name).ToList());
            }
            WorkList.Sort();

            foreach (string PostID in WorkList)
            {
                string DirectLink2Post = "https://www.pixiv.net/en/artworks/" + PostID;
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

                if (!Module_Grabber.CreateChildTreeNode(ref TreeViewParentNode, DirectLink2Post, DirectLink2Post))
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
            string URL_Parameters = null;
            if (Post_URL.Contains("www.pixiv.net/member_illust.php"))
            {
                URL_Parameters = HttpUtility.ParseQueryString(Post_URL.Remove(0, Post_URL.IndexOf("?") + 1))["illust_id"];
            }
            else // www.pixiv.net/en/artworks/
            {
                URL_Parameters = Post_URL.Substring(Post_URL.LastIndexOf("/") + 1);
            }

            string JSONSourceTest = Module_Grabber.GrabPageSource("https://www.pixiv.net/ajax/illust/" + URL_Parameters, ref Module_CookieJar.Cookies_Pixiv);
            if (JSONSourceTest != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                JObject JSONDictionary = JObject.Parse(JSONSourceTest);

                DateTime Post_Time = JSONDictionary["body"]["createDate"].Value<DateTime>();

                string Post_Title = JSONDictionary["body"]["illustTitle"].Value<string>();
                Post_Title = Post_Title.Replace("[", "⟦").Replace("]", "⟧");

                string ArtistName = JSONDictionary["body"]["userName"].Value<string>();

                HtmlDocument TempHTMLDoc = new HtmlDocument();
                TempHTMLDoc.LoadHtml(JSONDictionary["body"]["illustComment"].Value<string>());
                HtmlNode Post_TextNode = TempHTMLDoc.DocumentNode;
                string Post_Text = Module_Html2Text.Html2Text_Pixiv(Post_TextNode);
                TempHTMLDoc = null;

                int PicCount = JSONDictionary["body"]["pageCount"].Value<int>();
                string Post_MediaURL;
                int SkipCounter = 0;
                if (PicCount == 1)
                {
                    Post_MediaURL = JSONDictionary["body"]["urls"]["original"].Value<string>();

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
                    FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, JSONDictionary["body"]["urls"]["thumb"].Value<string>(), JSONDictionary["body"]["width"].Value<string>(), JSONDictionary["body"]["height"].Value<string>(), ArtistName);
                    TempDataTable.Rows.Add(TempDataRow);
                }
                else
                {
                    JObject JSONPages = JObject.Parse(Module_Grabber.GrabPageSource("https://www.pixiv.net/ajax/illust/" + URL_Parameters + "/pages", ref Module_CookieJar.Cookies_Pixiv));

                    Custom_ProgressBar TempcPB = new Custom_ProgressBar(Post_URL, PicCount);
                    Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cFlowLayoutPanel_ProgressBarHolder.Controls.Add(TempcPB); }));
                    for (int p = 0; p <= PicCount - 1; p++)
                    {
                        TempcPB.Value += 1;
                        Post_MediaURL = JSONPages["body"][p]["urls"]["original"].Value<string>();

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

                        string ThumbnailLink = JSONPages["body"][p]["urls"]["thumb_mini"].Value<string>().Replace("/128x128/", "/360x360_70/"); // /250x250_80_a2/ is cut off, that's not good.
                        DataRow TempDataRow = TempDataTable.NewRow();
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Title, Post_Text, Post_MediaURL, ThumbnailLink, JSONPages["body"][p]["width"].Value<string>(), JSONPages["body"][p]["height"].Value<string>(), ArtistName);
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
                    PrintText = $"Grabbing skipped - All media already grabbed[@{Post_URL}]";
                }
                else
                {
                    Module_Grabber._GrabQueue_WorkingOn[Post_URL] = TempDataTable;
                    PrintText = $"Finished grabbing: {Post_URL}";
                    if (SkipCounter > 0)
                    {
                        PrintText += $", {SkipCounter} media container{(SkipCounter > 1 ? "s" : null)} has been skipped";
                    }
                }
                lock (Module_Grabber._GrabQueue_URLs)
                {
                    Module_Grabber._GrabQueue_URLs.Remove(Post_URL);
                }
                //Module_Grabber.Report_Info(PrintText);
                return PrintText;
            }
            return "Error encountered during Pixiv grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string Title, string TextBody, string MediaURL, string ThumbURL, string ImageWidth, string ImageHeight, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("⮚ \"{0}\" ⮘ by {1} on Pixiv", Title, Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            TempDataRow["Grab_ThumbnailURL"] = ThumbURL;
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
