using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public static class Module_e621Info
    {
        public static string e621InfoDownload(string URL, bool Auth = false)
        {
            string HTML_Source;

            HttpWebRequest e621Info = (HttpWebRequest)WebRequest.Create(URL);
            e621Info.UserAgent = Properties.Settings.Default.AppName;
            if (Auth)
            {
                string AuthString = string.Format("{0}:{1}", Properties.Settings.Default.UserName, Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key));
                e621Info.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(AuthString)));
            }
            try
            {
                using (HttpWebResponse e621InfoDL = (HttpWebResponse)e621Info.GetResponse())
                {
                    if (e621InfoDL.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader ResponseReader = new StreamReader(e621InfoDL.GetResponseStream(), Encoding.UTF8))
                        {
                            HTML_Source = ResponseReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return HTML_Source;
            }
            catch
            {
                return null;
            }
        }
    }
}
