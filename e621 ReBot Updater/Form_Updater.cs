using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace e621_ReBot_Updater
{
    public partial class Form_Updater : Form
    {
        public Form_Updater()
        {
            InitializeComponent();
        }

        private void Form_Updater_Load(object sender, EventArgs e)
        {
            Label_Current.Text = string.Format("Current Version: {0}", FileVersionInfo.GetVersionInfo("e621 ReBot v2.exe").FileVersion);
        }

        private void Form_Updater_Shown(object sender, EventArgs e)
        {
            timer_Delay.Start();
        }

        private void timer_Delay_Tick(object sender, EventArgs e)
        {
            timer_Delay.Stop();
            WebBrowser_Updater.Navigate("http://e621rebot.infinityfreeapp.com");
        }

        private void WebBrowser_Updater_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string OnlineVersion;
                CookieContainer RequestCookieContainer = new CookieContainer();
                string[] SplitCookieString = WebBrowser_Updater.Document.Cookie.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                Cookie CreateCookie = new Cookie()
                {
                    Domain = "e621rebot.infinityfreeapp.com",
                    Name = SplitCookieString[0],
                    Value = SplitCookieString[1]
                };
                RequestCookieContainer.Add(CreateCookie);
                HttpWebRequest VersionRequest = (HttpWebRequest)WebRequest.Create("http://e621rebot.infinityfreeapp.com/version.txt");
                VersionRequest.CookieContainer = RequestCookieContainer;
                HttpWebResponse VersionResponse = (HttpWebResponse)VersionRequest.GetResponse();
                using (StreamReader DownloadStream = new StreamReader(VersionResponse.GetResponseStream()))
                {
                    OnlineVersion = DownloadStream.ReadToEnd();
                    Label_Latest.Text = string.Format("Latest Version: {0}", OnlineVersion);
                }

                int CurrentVersion = int.Parse(FileVersionInfo.GetVersionInfo("e621 ReBot v2.exe").FileVersion.Replace(".", ""));
                if (int.Parse(OnlineVersion.Replace(".", "")) <= CurrentVersion)
                {
                    timer_Close.Start();
                }
                else
                {
                    BackgroundImage = Properties.Resources.E621ReBotUpdaterBigBG;
                    Label_Download.Visible = true;
                    backgroundWorker_Update.RunWorkerAsync(RequestCookieContainer);
                }

            }
            catch
            {
                Close();
            }         
        }

        private void BackgroundWorker_Update_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                CookieContainer RequestCookieContainer = (CookieContainer)e.Argument;
                // update
                using (MemoryStream DownloadedBytes = new MemoryStream())
                {
                    HttpWebRequest UpdateDownloader = (HttpWebRequest)WebRequest.Create("http://e621rebot.infinityfreeapp.com/e621%20ReBot%20v2.zip");
                    UpdateDownloader.CookieContainer = RequestCookieContainer;
                    using (HttpWebResponse UpdateDownloaderReponse = (HttpWebResponse)UpdateDownloader.GetResponse())
                    {
                        using (Stream DownloadStream = UpdateDownloaderReponse.GetResponseStream())
                        {
                            byte[] DownloadBuffer = new byte[65536]; // 64 kB buffer
                            while (DownloadedBytes.Length < UpdateDownloaderReponse.ContentLength)
                            {
                                int DownloadStreamPartLength = DownloadStream.Read(DownloadBuffer, 0, DownloadBuffer.Length);
                                if (DownloadStreamPartLength > 0)
                                {
                                    DownloadedBytes.Write(DownloadBuffer, 0, DownloadStreamPartLength);
                                    double ReportPercentage = DownloadedBytes.Length / (double)UpdateDownloaderReponse.ContentLength;
                                    Label_Download.BeginInvoke(new Action(() => Label_Download.Text = string.Format("Downloading: {0}", ReportPercentage.ToString("P2"))));
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    int ExtractCounter = 0;
                    using (ZipArchive UpdateZip = new ZipArchive(DownloadedBytes))
                    {
                        foreach (ZipArchiveEntry entry in UpdateZip.Entries)
                        {
                            ExtractCounter += 1;
                            Label_Download.BeginInvoke(new Action(() => Label_Download.Text = string.Format("Extracting: {0} of {1}", ExtractCounter, UpdateZip.Entries.Count)));
                            if (entry.FullName.EndsWith("/"))
                            {
                                Directory.CreateDirectory(entry.FullName);
                            }
                            else
                            {
                                entry.ExtractToFile(Path.Combine("./", entry.FullName), true);
                            }
                        }
                    }
                }
            }
            catch
            {
                Close();
            }

        }

        private void BackgroundWorker_Update_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void Form_Updater_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText("update.check", DateTime.UtcNow.ToString());
            Process.Start("e621 ReBot v2.exe");
        }
    }
}
