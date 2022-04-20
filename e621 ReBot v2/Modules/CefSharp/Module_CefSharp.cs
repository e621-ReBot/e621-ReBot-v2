using CefSharp;
using CefSharp.WinForms;
using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules.Grabber;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Modules
{
    public static class Module_CefSharp
    {

        static Module_CefSharp()
        {
            timer_EnableCheck = new Timer
            {
                Interval = 500
            };
            timer_EnableCheck.Tick += EnableCheck_Timer_Tick;
            timer_Twitter = new Timer
            {
                Interval = 500
            };
            timer_Twitter.Tick += Twitter_Timer_Tick;
        }

        public static bool BrowserStartup = true;
        public static ChromiumWebBrowser CefSharpBrowser;

        public static void InitializeBrowser(string WebAdress)
        {
            CefSettings CefSharp_Settings = new CefSettings
            {
                CachePath = $"{Application.StartupPath}\\CefSharp Cache",
                PersistSessionCookies = true,
                BackgroundColor = (uint)ColorTranslator.ToWin32(Color.DimGray),
                BrowserSubprocessPath = $"{Application.StartupPath}\\CefSharp Browser\\CefSharp.BrowserSubprocess.exe",
                LogSeverity = LogSeverity.Error
            };
            CefSharp_Settings.BackgroundColor = Cef.ColorSetARGB(255, 105, 105, 105);
            CefSharp_Settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = MediaBrowser_SchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new MediaBrowser_SchemeHandlerFactory()
            });

            Cef.EnableHighDPISupport();
            Cef.Initialize(CefSharp_Settings);
            CefSharpBrowser = new ChromiumWebBrowser(WebAdress)
            {
                Dock = DockStyle.Fill,
                RequestHandler = new CefSharp_RequestHandler(),
                LifeSpanHandler = new CefSharp_LifeSpanHandler(),
                UseParentFormMessageInterceptor = false
            };
            CefSharpBrowser.AddressChanged += CefSharp_AddressChanged;
            CefSharpBrowser.LoadingStateChanged += CefSharp_LoadingStateChanged;
            CefSharpBrowser.FrameLoadEnd += CefSharpBrowser_FrameLoadEnd;
            CefSharpBrowser.TitleChanged += CefSharpBrowser_TitleChanged;

            Form_Loader._FormReference.panel_BrowserDisplay.Controls.Add(CefSharpBrowser);

            //Fix Browser not being able to load page on first click, before visible, on tutorial.
            string FixBrowser = CefSharpBrowser.Handle.ToString();
        }

        private static void CefSharpBrowser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            Form_Loader._FormReference.Invoke(new Action(() => { CefSharpBrowser.Text = e.Title; }));
        }

        private static void CefSharp_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            timer_EnableCheck.Stop();
            timer_Twitter.Stop();
            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                Form_Loader._FormReference.URL_ComboBox.Text = WebUtility.UrlDecode(e.Address);
                FrameLoad.Clear();
                FrameLoad.Add(Form_Loader._FormReference.URL_ComboBox.Text, 0);
                Form_Loader._FormReference.timer_TwitterGrabber.Stop();
                Module_Twitter.TwitterJSONHolder = null;
                Module_DeviantArt.FolderID = null;
            }));
        }

        //private static int PreviousReqCount = -1;
        private static void CefSharp_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (BrowserStartup)
            {
                if (!e.IsLoading)
                {
                    BrowserStartup = false;
                }
                return;
            }

            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                string CefAdress = HttpUtility.UrlDecode(CefSharpBrowser.Address);
                Form_Loader._FormReference.BB_Bookmarks.Enabled = !e.IsLoading;
                Form_Loader._FormReference.BB_Bookmarks.Enabled = !CefAdress.Equals("about:blank");
                Form_Loader._FormReference.BB_Backward.Enabled = e.CanGoBack;
                Form_Loader._FormReference.BB_Forward.Enabled = e.CanGoForward;
                Form_Loader._FormReference.BB_Reload.Enabled = !CefAdress.Equals("about:blank");
                Form_Loader._FormReference.BB_Reload.BackColor = Form_Loader._FormReference.BB_Reload.Enabled ? Color.RoyalBlue : Color.Gray;

                if (e.IsLoading)
                {
                    Form_Loader._FormReference.BB_Grab.Visible = false;
                    Form_Loader._FormReference.BB_Grab_All.Visible = false;
                    Form_Loader._FormReference.BB_Download.Visible = false;
                }
                else
                {
                    //if (PreviousReqCount < _RequestHandler.RequestCount)
                    //{
                    //    PreviousReqCount = _RequestHandler.RequestCount;

                    if (Properties.Settings.Default.FirstRun)
                    {
                        switch (CefAdress)
                        {
                            case string _1 when _1.Equals("https://e621.net/posts"):
                                {
                                    CefSharpBrowser.Load("https://e621.net/users/home");
                                    break;
                                }

                            case string _2 when _2.Equals("https://e621.net/users/home"):
                                {
                                    timer_CefTutorial = new Timer();
                                    timer_CefTutorial.Tick += Timer_CefTutorial_Tick;
                                    timer_CefTutorial.Start();
                                    break;
                                }

                            case string _3 when _3.StartsWith("https://e621.net/users/", StringComparison.OrdinalIgnoreCase) && _3.EndsWith("/api_key/view", StringComparison.OrdinalIgnoreCase):
                                {
                                    MessageBox.Show("Generate you API Key and copy it into the floating box.", "e621 Bot");
                                    new Form_APIKey().Show();
                                    break;
                                }
                        }
                        return;
                    }

                    if (Properties.Settings.Default.Bookmarks != null)
                    {
                        if (Properties.Settings.Default.Bookmarks.Contains(WebUtility.UrlDecode(CefAdress)))
                        {
                            Form_Loader._FormReference.BB_Bookmarks.BackColor = Color.RoyalBlue;
                            Form_Loader._FormReference.toolTip_Display.SetToolTip(Form_Loader._FormReference.BB_Bookmarks, "Remove Bookmark." + Environment.NewLine + Environment.NewLine + "Hold Ctrl when clicking to clear all Bookmarks.");
                        }
                        else
                        {
                            Form_Loader._FormReference.BB_Bookmarks.BackColor = Color.Gray;
                            Form_Loader._FormReference.toolTip_Display.SetToolTip(Form_Loader._FormReference.BB_Bookmarks, "Bookmark current page." + Environment.NewLine + Environment.NewLine + "Hold Ctrl when clicking to clear all Bookmarks.");
                        }
                    }

                    if (CefAdress.Contains("mastodon.social/@"))
                    {
                        CefSharpBrowser.ExecuteScriptAsync("document.querySelectorAll(\"button[class='status__content__spoiler-link']\").forEach(button=>button.click())");
                        CefSharpBrowser.ExecuteScriptAsync("document.querySelectorAll(\"button[class='spoiler-button__overlay']\").forEach(button=>button.click())");
                    }

                    GrabEnabler(CefAdress);
                    Module_Downloader.DownloadEnabler(CefAdress);
                }
                Form_Loader._FormReference.QuickButtonPanel.Visible = false;
            }));
        }

        private static Dictionary<string, int> FrameLoad = new Dictionary<string, int>();
        private static void CefSharpBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if ((CefSharpBrowser.Address.Contains("https://www.pixiv.net") || e.Url.Contains("https://www.pixiv.net")) && e.Frame.Name.Equals("footer"))
            {
                FrameLoad[HttpUtility.UrlDecode(CefSharpBrowser.Address)] = 1;
            }
        }

        // = = = = = 

        private static Timer timer_CefTutorial;
        private static void Timer_CefTutorial_Tick(object sender, EventArgs e)
        {
            timer_CefTutorial.Stop();

            MessageBox.Show("I will grab some needed data from this page, then I'm going to point you to next step.", "e621 ReBot");

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(CefSharpBrowser.GetSourceAsync().Result);

            string TextNode = WebDoc.DocumentNode.SelectSingleNode(".//a[@id='subnav-profile-link']").Attributes["href"].Value;
            TextNode = TextNode.Substring(TextNode.LastIndexOf("/") + 1);

            Properties.Settings.Default.UserID = TextNode;
            Properties.Settings.Default.Save();

            //Thread ThreadTemp = new Thread(Module_Credits.Check_Credit_All);
            //ThreadTemp.Start();
            Module_Credits.Check_Credit_All(); //Doing it in bg causes no username issue?

            CefSharpBrowser.Load($"https://e621.net/users/{Properties.Settings.Default.UserID}/api_key");
        }

        // = = = = = 

        private static void GrabEnabler(string WebAdress)
        {
            foreach (Regex URLTest in Module_Grabber._GrabEnabler)
            {
                Match MatchTemp = URLTest.Match(WebAdress);
                if (MatchTemp.Success)
                {
                    Form_Loader._FormReference.BB_Grab.Tag = MatchTemp.Value;

                    // Twitter is special
                    if (WebAdress.Contains("https://twitter.com/"))
                    {
                        Form_Loader._FormReference.BB_Grab_All.Text = "Grab All";
                        Form_Loader._FormReference.LastBrowserPosition = 0;
                        Form_Loader._FormReference.LastBrowserPositionCounter = 0;
                        //timer_Twitter.Start();
                        return;
                    }

                    if (WebAdress.StartsWith("https://www.pixiv.net", StringComparison.OrdinalIgnoreCase))
                    {
                        if (FrameLoad[WebAdress] == 1)
                        {
                            FrameLoad[WebAdress] = 2;
                            timer_EnableCheck.Start();
                        }
                        return;
                    }

                    timer_EnableCheck.Start();
                    return;
                }
            }

            List<string> DeviantArtGrabEnabler = new List<string>(new string[] { "/gallery", "/art/" });
            if (WebAdress.Contains("https://www.deviantart.com/") && DeviantArtGrabEnabler.Any(s => WebAdress.Contains(s)))
            {
                Form_Loader._FormReference.BB_Grab.Tag = WebAdress;
                Form_Loader._FormReference.BB_Grab.Visible = true;
                Form_Loader._FormReference.BB_Grab_All.Visible = false;
            }
        }

        public static Timer timer_EnableCheck;
        public static void EnableCheck_Timer_Tick(object sender, EventArgs e)
        {
            timer_EnableCheck.Stop();
            string WebAdress = HttpUtility.UrlDecode(CefSharpBrowser.Address);
            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(GetHTMLSource());

            if (WebAdress.StartsWith("https://www.pixiv.net/en/artworks/", StringComparison.OrdinalIgnoreCase))
            {
                if (WebDoc.DocumentNode.SelectSingleNode(".//main/section//figure//button[@type='submit']") == null)
                {
                    Form_Loader._FormReference.BB_Grab.Visible = true;
                }
                //var test = CefSharpBrowser.EvaluateScriptAsync("(function() {return document.querySelector(\"main figure button[type ='submit']\");})();").Result; //refuses to work even if it works in console, returns null always 
                return;
            }

            if (WebAdress.StartsWith("https://www.hiccears.com/", StringComparison.OrdinalIgnoreCase) &&
            (WebAdress.Contains("/contents/") || WebAdress.Contains("/file/")))
            {
                if (WebDoc.DocumentNode.SelectSingleNode(".//div[@class='marketplace-sidebar']//button[@class='button payable']") == null && //buyable and usually censored/watermarked
                WebDoc.DocumentNode.SelectSingleNode(".//div[@class='post-open-body']") == null) //writing
                {
                    Form_Loader._FormReference.BB_Grab.Visible = true;
                }
                return;
            }

            if (WebAdress.StartsWith("https://www.plurk.com/", StringComparison.OrdinalIgnoreCase))
            {
                if (WebDoc.DocumentNode.SelectSingleNode(".//div[@id='timeline_cnt']") != null)
                {
                    Form_Loader._FormReference.BB_Grab.Visible = true;
                }
                return;
            }
            Form_Loader._FormReference.BB_Grab.Visible = true;
        }

        public static Timer timer_Twitter;
        public static void Twitter_Timer_Tick(object sender, EventArgs e)
        {
            timer_Twitter.Stop();

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(GetHTMLSource());

            HtmlNode AccountNode = WebDoc.DocumentNode.SelectSingleNode(".//header[@role='banner']//div[@data-testid='SideNav_AccountSwitcher_Button']");

            if (AccountNode == null)
            {
                if (WebDoc.DocumentNode.SelectSingleNode(".//main[@role='main']//div[@data-testid='emptyState']") == null)
                {
                    if (CefSharpBrowser.Address.Contains("/status/"))
                    {
                        if (!CefSharpBrowser.Address.Contains("/photo/"))
                        {
                            HtmlNode TweetNodeFinder = WebDoc.DocumentNode.SelectSingleNode(".//article[@data-testid='tweet']");
                            if (TweetNodeFinder == null)
                            {
                                timer_Twitter.Start();
                                return;
                            }

                            if (TweetNodeFinder.InnerHtml.Contains("The following media includes potentially sensitive content."))
                            {
                                CefSharpBrowser.ExecuteScriptAsync("document.querySelector(\"article[data-testid='tweet'] article[role='article'] div[role='button']:not([aria-label])\").click()");
                            }
                            Form_Loader._FormReference.BB_Grab.Visible = true;
                            Form_Loader._FormReference.BB_Grab_All.Visible = false;
                        }
                        return;
                    }

                    if (!CefSharpBrowser.Address.Contains("/search?"))
                    {
                        HtmlNode TweetNodeFinder = WebDoc.DocumentNode.SelectSingleNode(".//h1[@id='accessible-list-2']");
                        if (TweetNodeFinder != null)
                        {
                            TweetNodeFinder = TweetNodeFinder.NextSibling; //SelectSingleNode(".//div[@data-testid='primaryColumn']//section[@aria-labelledby='accessible-list-2']/div");
                        }

                        if (TweetNodeFinder == null)
                        {
                            timer_Twitter.Start();
                            return;
                        }

                        if (TweetNodeFinder.SelectNodes(".//article[@data-testid='tweet']") == null)
                        {
                            timer_Twitter.Start();
                        }
                        else
                        {
                            CefSharpBrowser.ExecuteScriptAsync("document.querySelectorAll(\"article[data-testid='tweet'] article[role='article'] div[role='button']:not([aria-label])\").forEach(button=>button.click());");
                            CefSharpBrowser.ExecuteScriptAsync("document.onwheel = function(){document.querySelectorAll(\"article[data-testid='tweet'] article[role='article'] div[role='button']:not([aria-label])\").forEach(button=>button.click());};");
                            Form_Loader._FormReference.BB_Grab_All.Visible = true;
                            if (!Form_Loader._FormReference.BB_Grab_All.Text.Equals("Stop")) Form_Loader._FormReference.BB_Grab.Visible = true;
                        }
                    }
                }
                else
                {
                    CefSharpBrowser.ExecuteScriptAsync("document.querySelector(\"main[role='main'] div[data-testid='emptyState'] div[role='button']\").click()");
                }
            }
            else
            {
                if (CefSharpBrowser.Address.Contains("/status/"))
                {
                    if (!CefSharpBrowser.Address.Contains("/photo/"))
                    {
                        Form_Loader._FormReference.BB_Grab.Visible = true;
                        Form_Loader._FormReference.BB_Grab_All.Visible = false;
                    }
                    return;
                }
                Form_Loader._FormReference.BB_Grab_All.Visible = true;
                if (!Form_Loader._FormReference.BB_Grab_All.Text.Equals("Stop")) Form_Loader._FormReference.BB_Grab.Visible = true;
            }
        }

        public static string GetHTMLSource()
        {
            return Task.Run(async () => await CefSharpBrowser.GetMainFrame().GetSourceAsync()).Result;
        }
    }
}