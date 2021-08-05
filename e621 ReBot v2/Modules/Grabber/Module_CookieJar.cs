using CefSharp;
using System;
using System.Net;

namespace e621_ReBot_v2.Modules
{
    public static class Module_CookieJar
    {
        public static CookieContainer Cookies_Inkbunny;
        public static CookieContainer Cookies_Pixiv;
        public static CookieContainer Cookies_FurAffinity;
        public static CookieContainer Cookies_Twitter;
        public static CookieContainer Cookies_Newgrounds;
        public static CookieContainer Cookies_HicceArs;
        public static CookieContainer Cookies_SoFurry;
        //Mastadon?
        public static CookieContainer Cookies_Plurk;
        public static CookieContainer Cookies_Weasyl;
        public static CookieContainer Cookies_DeviantArt;

        public static void GetCookies(string WebAdress, ref CookieContainer WhichCookie)
        {
            if (WhichCookie == null)
            {
                WhichCookie = new CookieContainer();
            }
            string BaseURL = new Uri(WebAdress).Scheme + "://" + new Uri(WebAdress).Host;
            var CookieList = Cef.GetGlobalCookieManager().VisitUrlCookiesAsync(BaseURL, true).Result;
            foreach (var CookieHolder in CookieList)
            {
                System.Net.Cookie TempCookie = new System.Net.Cookie()
                {
                    Domain = CookieHolder.Domain,
                    Expires = CookieHolder.Expires == null ? DateTime.Now.AddMonths(1) : (DateTime)CookieHolder.Expires,
                    Name = CookieHolder.Name,
                    Value = CookieHolder.Value
                };
                WhichCookie.Add(TempCookie);
            }
        }
    }
}
