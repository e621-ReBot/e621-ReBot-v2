using e621_ReBot_v2.Modules.Grabber;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
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
            if (request.Url.Contains("https://twitter.com/i/api/graphql/") && (request.Url.Contains("/UserTweets?variables=") || request.Url.Contains("/UserMedia?variables=")) && response.MimeType.Equals("application/json"))
            {
                byte[] byteHolder = memoryStreamHolder.ToArray();
                string Data2String = Encoding.UTF8.GetString(byteHolder);
                if (Data2String.Length > 64)
                {
                    JObject JObjectTemp = JObject.Parse(Data2String);

                    var TweetsTest = JObjectTemp.SelectTokens("data.user.result.timeline.timeline.instructions.[0].entries.[*].content.itemContent.tweet_results.result");
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
