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
        protected override void WndProc(ref Message message)
        {
            if (message.Msg == Program.WM_SHOWFIRSTINSTANCE)
            {
                WinApi.ShowToFront(Handle);
            }
            base.WndProc(ref message);
        }

        public Form_Updater()
        {
            InitializeComponent();
        }

        private void Form_Updater_Load(object sender, EventArgs e)
        {
            if (File.Exists("e621 ReBot v2.exe"))
            {
                Label_Current.Text = $"Current Version: {FileVersionInfo.GetVersionInfo("e621 ReBot v2.exe").FileVersion}";
            }
            else 
            {
                MessageBox.Show("e621 ReBot v2.exe not found.", "e621 ReBot Updater");
                ClosePrematurely = true;
                Close();
            }
        }

        private void Form_Updater_Shown(object sender, EventArgs e)
        {
            timer_Delay.Start();
        }

        private void Timer_Delay_Tick(object sender, EventArgs e)
        {
            timer_Delay.Stop();
            WebBrowser_Updater.Navigate("http://e621rebot.infinityfreeapp.com");
        }

        private void Timer_Close_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void WebBrowser_Updater_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string OFVString;
                CookieContainer RequestCookieContainer = new CookieContainer();
                string SplitCookieString = WebBrowser_Updater.Document.Cookie.Substring(WebBrowser_Updater.Document.Cookie.LastIndexOf("=") + 1);
                Cookie CreateCookie = new Cookie()
                {
                    Domain = "e621rebot.infinityfreeapp.com",
                    Name = "_test",
                    Value = SplitCookieString
                };
                RequestCookieContainer.Add(CreateCookie);
                HttpWebRequest VersionRequest = (HttpWebRequest)WebRequest.Create("http://e621rebot.infinityfreeapp.com/version.txt");
                VersionRequest.CookieContainer = RequestCookieContainer;
                HttpWebResponse VersionResponse = (HttpWebResponse)VersionRequest.GetResponse();
                using (StreamReader DownloadStream = new StreamReader(VersionResponse.GetResponseStream()))
                {
                    OFVString = DownloadStream.ReadToEnd();
                    Label_Latest.Text = $"Latest Version: {OFVString}";
                }

                string CFVString = FileVersionInfo.GetVersionInfo("e621 ReBot v2.exe").FileVersion;
                string[] CFVHolder = CFVString.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string[] OFVHolder = OFVString.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                int CurrentVersion = (int)(int.Parse(CFVHolder[1]) * Math.Pow(10, 6) + int.Parse(CFVHolder[2]) * Math.Pow(10, 3) + int.Parse(CFVHolder[3]));
                int OnlineVersion = (int)(int.Parse(OFVHolder[1]) * Math.Pow(10, 6) + int.Parse(OFVHolder[2]) * Math.Pow(10, 3) + int.Parse(OFVHolder[3]));
                if (OnlineVersion <= CurrentVersion)
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
                                    Label_Download.BeginInvoke(new Action(() => Label_Download.Text = $"Downloading: {ReportPercentage.ToString("P2")}"));
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
                        foreach (ZipArchiveEntry ZipEntry in UpdateZip.Entries)
                        {
                            ExtractCounter += 1;
                            Label_Download.BeginInvoke(new Action(() => Label_Download.Text = $"Extracting: {ExtractCounter} of {UpdateZip.Entries.Count}"));
                            if (ZipEntry.FullName.EndsWith("/"))
                            {
                                Directory.CreateDirectory(ZipEntry.FullName);
                            }
                            else
                            {
                                ZipEntry.ExtractToFile(Path.Combine("./", ZipEntry.FullName), true);
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

        private bool ClosePrematurely = false;
        private void Form_Updater_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ClosePrematurely)
            {
                File.WriteAllText("update.check", DateTime.UtcNow.ToString());
                Process.Start("e621 ReBot v2.exe");
            }
        }
    }
}