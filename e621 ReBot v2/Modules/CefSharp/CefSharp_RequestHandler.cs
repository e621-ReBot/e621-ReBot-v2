using e621_ReBot_v2.Modules.Grabber;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CefSharp
{
    class CefSharp_RequestHandler : Handler.RequestHandler
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

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // To allow the resource load to proceed with default handling return null. To specify a handler for the resource return a IResourceRequestHandler object. If this callback returns null the same method will be called on the associated IRequestContextHandler, if any
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
}