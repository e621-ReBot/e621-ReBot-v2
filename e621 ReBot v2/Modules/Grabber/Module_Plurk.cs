using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace e621_ReBot_v2.Modules.Grabber
{
    public static class Module_Plurk
    {
        public static void QueuePrep(string WebAdress)
        {
            Module_CookieJar.GetCookies(WebAdress, ref Module_CookieJar.Cookies_Plurk);
            if (WebAdress.Contains("plurk.com/p/"))
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

            foreach (HtmlNode ChildNode in WebDoc.DocumentNode.SelectNodes(".//div[@id='timeline_cnt']//div[@data-type='plurk']"))
            {
                string DirectLink2Post = "https://www.plurk.com" + ChildNode.SelectSingleNode(".//td[@class='td_response_count']/a").Attributes["href"].Value;
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
            string HTMLSource = Module_Grabber.GrabPageSource(WebAdress, ref Module_CookieJar.Cookies_Plurk);
            if (HTMLSource != null)
            {
                DataTable TempDataTable = new DataTable();
                Module_TableHolder.Create_DBTable(ref TempDataTable);

                HtmlDocument WebDoc = new HtmlDocument();
                WebDoc.LoadHtml(HTMLSource);

                string Post_URL = WebAdress;

                HtmlNode PostNode = WebDoc.DocumentNode;

                DateTime Post_Time = DateTime.Parse(PostNode.SelectSingleNode(".//time[@class='timeago']").Attributes["datetime"].Value);

                string ArtistName = PostNode.SelectSingleNode(".//article[@id='permanent-plurk']//div[@class='user']//a").InnerText.Trim();
                string ArtistProfileName = PostNode.SelectSingleNode(".//article[@id='permanent-plurk']//div[@class='avatar']//a").Attributes["href"].Value.Substring(1);
                ArtistName = $"{ArtistName} (@{ArtistProfileName})";

                HtmlNode Post_TextNode = PostNode.SelectSingleNode(".//article[@id='permanent-plurk']//div[@class='text_holder']");
                string Post_Text = null;
                List<string> PlurkImages = new List<string>();

                if (Post_TextNode != null)
                {
                    foreach (HtmlNode DescriptionLine in Post_TextNode.ChildNodes)
                    {
                        switch (DescriptionLine.Name)
                        {
                            case "#text":
                            case "span":
                                {
                                    Post_Text += DescriptionLine.InnerText.Trim();
                                    break;
                                }

                            case "br":
                                {
                                    Post_Text += Environment.NewLine;
                                    break;
                                }

                            case "a":
                                {
                                    if (DescriptionLine.Attributes["class"] != null)
                                    {
                                        if (DescriptionLine.Attributes["class"].Value.Contains("ex_link pictureservices"))
                                        {
                                            PlurkImages.Add(DescriptionLine.Attributes["href"].Value);
                                        }
                                        else if (DescriptionLine.Attributes["class"].Value.Contains("ex_link"))
                                        {
                                            Post_Text += string.Format("\"{0}\":{1} ", DescriptionLine.InnerText, DescriptionLine.Attributes["href"].Value);
                                        }
                                    }
                                    else
                                    {
                                        Post_Text += DescriptionLine.InnerText.Trim();
                                    }
                                    break;
                                }

                            case "img":
                                {
                                    //skip
                                    break;
                                }

                            default:
                                {
                                    Post_Text += "UNKNOWN ELEMENT" + Environment.NewLine;
                                    break;
                                }
                        }
                    }
                }

                if (Post_Text != null)
                {
                    Post_Text = WebUtility.HtmlDecode(Post_Text).Trim() + " ";
                }

                string Post_MediaURL;
                int SkipCounter = 0;
                if (PlurkImages.Count > 0)
                {
                    foreach (string tImage in PlurkImages)
                    {
                        Post_MediaURL = tImage;
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
                        FillDataRow(ref TempDataRow, Post_URL, Post_Time, Post_Text, Post_MediaURL, ArtistName);
                        TempDataTable.Rows.Add(TempDataRow);
                    }
                }

                string PrintText;
                if (TempDataTable.Rows.Count == 0)
                {
                    lock (Module_Grabber._GrabQueue_WorkingOn)
                    {
                        Module_Grabber._GrabQueue_WorkingOn.Remove(Post_URL);
                    }
                    PrintText = $"Grabbing skipped - {(PlurkImages.Count > 0 ? "All media already grabbed" : "No media found")} [@{Post_URL}]";
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
            return "Error encountered during Plurk grab";
        }

        private static void FillDataRow(ref DataRow TempDataRow, string URL, DateTime DateTime, string TextBody, string MediaURL, string Artist)
        {
            TempDataRow["Grab_URL"] = URL;
            TempDataRow["Grab_DateTime"] = DateTime;
            TempDataRow["Grab_Title"] = WebUtility.HtmlDecode(string.Format("Plurk by {0}", Artist)); ;
            if (TextBody != null) TempDataRow["Grab_TextBody"] = TextBody;
            TempDataRow["Grab_MediaURL"] = MediaURL;
            string ThubnailURLTemp = MediaURL.Replace("https://images.plurk.com/", "https://images.plurk.com/mx_");
            ThubnailURLTemp = ThubnailURLTemp.Remove(ThubnailURLTemp.Length - 4) + ".jpg";
            TempDataRow["Grab_ThumbnailURL"] = ThubnailURLTemp;
            TempDataRow["Info_MediaFormat"] = MediaURL.Substring(MediaURL.LastIndexOf(".") + 1);
            TempDataRow["Info_MediaByteLength"] = Module_Grabber.GetMediaSize(MediaURL);
            TempDataRow["Upload_Tags"] = DateTime.Year;
            TempDataRow["Artist"] = Artist;
        }
    }
}
