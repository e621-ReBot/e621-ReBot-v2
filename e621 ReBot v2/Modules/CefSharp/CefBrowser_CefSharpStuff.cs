using CefSharp.Handler;
using e621_ReBot_v2;
using e621_ReBot_v2.Modules;
using e621_ReBot_v2.Modules.Grabber;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CefSharp
{
    class CefSharp_LifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            if (MessageBox.Show("Prevented new window from opening, do you want to navigate to it instead?", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // If WebBrowserX.Url.ToString.Contains("www.pixiv.net/member_illust.php?mode=medium&illust_id=") AndAlso WebBrowserX.Document.ActiveElement.InnerText = "See all" Then
                // WebBrowserX.Navigate("https://www.pixiv.net" & WebBrowserX.Document.ActiveElement.GetAttribute("href"))
                // Else
                chromiumWebBrowser.Load(targetUrl);
            }
            return true;
        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            //nothing
        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            //nothing
        }
    }

    class CefSharp_RequestHandler : RequestHandler
    {
        private readonly List<string> AdBlock = new List<string>();
        public CefSharp_RequestHandler()
        {
            // e6
            AdBlock.Add("ads.dragonfru");

            // FA
            AdBlock.Add("googlesyndication.com");
            AdBlock.Add("amazon-adsystem.com");
            AdBlock.Add("rv.furaffinity.net");
            AdBlock.Add("adservice.google");
            AdBlock.Add("quantserve.com");

            // Pixiv
            AdBlock.Add("pixon.ads");
            AdBlock.Add("microad.jp");
            AdBlock.Add("microad.net");
            AdBlock.Add("pixon.ads");
            AdBlock.Add("microadinc.com");
            AdBlock.Add("adsymptotic.com");
            AdBlock.Add("adingo.jp");
            AdBlock.Add("doubleclick.net");
            AdBlock.Add("amazon-adsystem.com");
            AdBlock.Add("pubmatic.com");
            AdBlock.Add("onesignal.com");
            AdBlock.Add("imp.pixiv.net");

            //Plurk
            AdBlock.Add("ads.yap.yahoo.com");

            //Newgrounds
            AdBlock.Add("a.adtng.com");

            //Hentai Foundry
            AdBlock.Add(".jads.co");
            AdBlock.Add("iadoremakingpics.com");
            AdBlock.Add("suchenachmuschi.space");
            AdBlock.Add("img.hentai-foundry.com/themes/Hentai/");
        }

        public int RequestCount { get; set; }
        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            RequestCount++;
            // Block Ads and shit
            if (AdBlock.Any(s => request.Url.Contains(s)))
            {
                return true;
            }

            //Console.WriteLine(request.Url);
            return false;
        }

        // To allow the resource load to proceed with default handling return null. To specify a handler for the resource return a IResourceRequestHandler object. If this callback returns null the same method will be called on the associated IRequestContextHandler, if any
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (chromiumWebBrowser.Address.Contains("https://twitter.com") && request.Url.StartsWith("https://twitter.com/i/api/graphql/", StringComparison.OrdinalIgnoreCase) && request.Headers["authorization"] != null)
            {
                Module_Twitter.TwitterAuthorizationBearer = request.Headers["authorization"];
                return new CefSharp_ResourceRequestHandler();
            }

            if (chromiumWebBrowser.Address.Contains("https://www.deviantart.com/") && request.Url.Contains("https://www.deviantart.com/_napi/da-user-profile/api/gallery/contents") && request.Url.Contains("&folderid="))
            {
                Module_DeviantArt.FolderID = request.Url.Substring(request.Url.IndexOf("&folderid=") + 10);
                return null;
            }

            if (AdBlock.Any(s => request.Url.Contains(s)))
            {
                disableDefaultHandling = true;
                return new CefSharp_ResourceRequestHandler_AdBlocker();
            }

            //Console.WriteLine(request.Url);
            return null;
        }
    }

    class CefSharp_ResourceRequestHandler : ResourceRequestHandler
    {
        private readonly MemoryStream memoryStreamHolder = new MemoryStream();
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return new ResponseFilter.StreamResponseFilter(memoryStreamHolder);
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (request.Url.Contains("https://twitter.com/i/api/graphql/") && response.MimeType.Equals("application/json") && ((request.Url.Contains("/UserTweets?variables=") || request.Url.Contains("/UserMedia?variables=") || request.Url.Contains("/UserByScreenName?variables="))))
            {
                byte[] byteHolder = memoryStreamHolder.ToArray();
                string Data2String = Encoding.UTF8.GetString(byteHolder);
                if (Data2String.Length > 64)
                {
                    JObject JObjectTemp = JObject.Parse(Data2String);

                    IEnumerable<JToken> TweetsTest = JObjectTemp.SelectTokens("data.user.result.timeline.timeline.instructions.[0].entries.[*].content.itemContent.tweet_results.result");
                    if (TweetsTest != null)
                    {
                        JArray TweetHolder = new JArray(TweetsTest);
                        if (Module_Twitter.TwitterJSONHolder != null)
                        {
                            Module_Twitter.TwitterJSONHolder.Merge(TweetHolder, new JsonMergeSettings
                            {
                                MergeArrayHandling = MergeArrayHandling.Union
                            });
                        }
                        else
                        {
                            Module_Twitter.TwitterJSONHolder = TweetHolder;
                        }
                    }
                }
                Form_Loader._FormReference.BeginInvoke(new Action(() => Module_CefSharp.timer_Twitter.Start()));
            }
        }
    }

    class CefSharp_ResourceRequestHandler_AdBlocker : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }
}