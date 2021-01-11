using CefSharp;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules.CefSharp
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
}
