using e621_ReBot_v2.Modules.Grabber;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace CefSharp
{
    class CefSharp_ResourceRequestHandler : Handler.ResourceRequestHandler
    {
        private readonly MemoryStream memoryStreamHolder = new MemoryStream();
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return new ResponseFilter.StreamResponseFilter(memoryStreamHolder);
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (request.Url.Contains("https://twitter.com/i/api/2/timeline/") && response.MimeType.Equals("application/json"))
            {
                byte[] byteHolder = memoryStreamHolder.ToArray();
                string Data2String = Encoding.UTF8.GetString(byteHolder);
                if (Data2String.Length > 64)
                {
                    JObject TempJHolder = JObject.Parse(Data2String);

                    if (TempJHolder["globalObjects"]["tweets"] != null && Module_Twitter.TwitterJSONHolder != null)
                    {
                        Module_Twitter.TwitterJSONHolder.Merge(TempJHolder["globalObjects"]["tweets"], new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Union
                        });
                    }
                    else
                    {
                        Module_Twitter.TwitterJSONHolder = TempJHolder;
                    }
                } 
            }
        }
    }

    class CefSharp_ResourceRequestHandler_AdBlocker : Handler.ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }
}
