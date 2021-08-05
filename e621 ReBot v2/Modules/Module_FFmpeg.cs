using e621_ReBot.Modules;
using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public class Module_FFmpeg
    {

        private static string UgoiraJSONResponse(string WorkURL)
        {
            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            Module_CookieJar.GetCookies(WorkURL, ref Module_CookieJar.Cookies_Pixiv);
            HttpWebRequest PixivDownload = (HttpWebRequest)WebRequest.Create(string.Format("https://www.pixiv.net/ajax/illust/{0}/ugoira_meta", WorkID));
            PixivDownload.CookieContainer = Module_CookieJar.Cookies_Pixiv;
            PixivDownload.Timeout = 5000;
            PixivDownload.UserAgent = Form_Loader.GlobalUserAgent;
            using (var PixivStreamReader = new StreamReader(PixivDownload.GetResponse().GetResponseStream()))
            {
                return PixivStreamReader.ReadToEnd();
            }
        }

        private static void FileDownloader(string DownloadURL, string ActionType, string TempFolder, string DownloadFolder, DataRow DataRowRef = null, bool isUgoira = false, Custom_ProgressBar RoundProgressBarRef = null)
        {
            if (ActionType.Equals("C") && Form_Preview._FormReference != null)
            {
                Label DLLabel = Form_Preview._FormReference.Label_Download;
                DLLabel.BeginInvoke(new Action(() =>
                {
                    DLLabel.Text = "0%";
                    DLLabel.Visible = true;
                }));
            }

            HttpWebRequest FileDownloader = (HttpWebRequest)WebRequest.Create(DownloadURL);
            if (isUgoira)
            {
                FileDownloader.Referer = "https://www.pixiv.net/";
            }
            FileDownloader.Timeout = 5000;
            using (MemoryStream DownloadedBytes = new MemoryStream())
            {
                using (WebResponse UgoiraDownloaderReponse = FileDownloader.GetResponse())
                {
                    using (var DownloadStream = UgoiraDownloaderReponse.GetResponseStream())
                    {
                        byte[] DownloadBuffer = new byte[65536]; // 64 kB buffer
                        while (DownloadedBytes.Length < UgoiraDownloaderReponse.ContentLength)
                        {
                            int DownloadStreamPartLength = DownloadStream.Read(DownloadBuffer, 0, DownloadBuffer.Length);
                            if (DownloadStreamPartLength > 0)
                            {
                                DownloadedBytes.Write(DownloadBuffer, 0, DownloadStreamPartLength);
                                double ReportPercentage = DownloadedBytes.Length / (double)UgoiraDownloaderReponse.ContentLength;

                                switch (ActionType)
                                {
                                    case "U":
                                        {
                                            Module_Uploader.Report_Status(string.Format("Downloading {0}...{1}", isUgoira ? "Ugoira" : "Video", ReportPercentage.ToString("P0")));
                                            break;
                                        }
                                    case "C":
                                        {
                                            string ReportType = isUgoira ? "CDU" : "CDV";
                                            ReportConversionProgress(ReportType, ReportPercentage, ref DataRowRef);
                                            break;
                                        }

                                    default: //case "D":
                                        {
                                            RoundProgressBarRef.BeginInvoke(new Action(() => { RoundProgressBarRef.Value = (int)(ReportPercentage * 100); }));
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                string FileName = Module_Downloader.GetMediasFileNameOnly(DownloadURL);
                if (isUgoira)
                {
                    using (ZipArchive UgoiraZip = new ZipArchive(DownloadedBytes))
                    {
                        UgoiraZip.ExtractToDirectory(TempFolder);
                        if (DownloadFolder != null && Properties.Settings.Default.Converter_KeepOriginal)
                        {
                            Directory.CreateDirectory(DownloadFolder);
                            DownloadedBytes.Seek(0, SeekOrigin.Begin);
                            using (FileStream TempFileStream = new FileStream(string.Format(@"{0}\{1}", DownloadFolder, FileName), FileMode.Create))
                            {
                                DownloadedBytes.WriteTo(TempFileStream);
                            }
                        }
                    }
                }
                else
                {
                    using (FileStream TempFileStream = new FileStream(string.Format(@"{0}\{1}", TempFolder, FileName), FileMode.Create))
                    {
                        DownloadedBytes.WriteTo(TempFileStream);        
                    }
                    if (DownloadFolder != null && Properties.Settings.Default.Converter_KeepOriginal)
                    {
                        Directory.CreateDirectory(DownloadFolder);
                        File.Copy(string.Format(@"{0}\{1}", TempFolder, FileName), string.Format(@"{0}\{1}", DownloadFolder, FileName), false);
                    }
                }
                DownloadedBytes.Dispose();
            }
        }





        public static void UploadQueue_Ugoira2WebM(ref DataRow DataRowRef, out byte[] bytes2Send, out string FileName, out string ExtraSourceURL)
        {
            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse((string)DataRowRef["Grab_URL"]))["body"];

            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine(string.Format("file UgoiraTemp/{0}", UgoiraFrame["file"].Value<string>())); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine("duration " + UgoiraFrame["delay"].Value<int>() / 1000d);
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            Directory.CreateDirectory("UgoiraTemp").Attributes = FileAttributes.Hidden;
            File.WriteAllText(@"UgoiraTemp\input.txt", UgoiraConcat.ToString());
            DataRowRef["Upload_Tags"] = (string)DataRowRef["Upload_Tags"] + (TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime");
            FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "U", "UgoiraTemp", null, DataRowRef, true);

            Module_Uploader.Report_Status("Converting Ugoira to WebM...");
            ExtraSourceURL = UgoiraJObject["originalSrc"].Value<string>();
            string UgoiraFileName = ExtraSourceURL.Remove(ExtraSourceURL.Length - 4).Substring(ExtraSourceURL.LastIndexOf("/") + 1);
            using (Process FFmpeg = new Process())
            {
                Form_Loader._FormReference.UploadQueueProcess = FFmpeg;
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -f concat -i UgoiraTemp\input.txt -c:v libvpx-vp9 -pix_fmt yuv420p -lossless 1 -an -row-mt 1 UgoiraTemp\{0}.webm", UgoiraFileName);
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.RedirectStandardError = true;
                FFmpeg.Start();
                while (!FFmpeg.HasExited)
                {
                    string ReadLine = FFmpeg.StandardError.ReadLine();
                    while (ReadLine != null)
                    {
                        if (ReadLine.StartsWith("frame= ", StringComparison.OrdinalIgnoreCase))
                        {
                            TimeSpan CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                            Module_Uploader.Report_Status(string.Format("Converting Ugoira to WebM...{0}", (CurrentTime.TotalMilliseconds / TotalUgoiraLength).ToString("P0")));
                            break;
                        }
                        ReadLine = FFmpeg.StandardError.ReadLine();
                    }
                }
                Module_Uploader.Report_Status("Converting Ugoira to WebM...100%");
                FFmpeg.WaitForExit();
            }
            Form_Loader._FormReference.UploadQueueProcess = null;
            bytes2Send = File.ReadAllBytes(string.Format(@"UgoiraTemp\{0}.webm", UgoiraFileName));
            FileName = UgoiraFileName + ".webm";
            Directory.Delete("UgoiraTemp", true);
        }

        public static void UploadQueue_Videos2WebM(ref DataRow DataRowRef, out byte[] bytes2Send, out string FileName, out string ExtraSourceURL)
        {
            string WorkURL = (string)DataRowRef["Grab_MediaURL"];
            ExtraSourceURL = WorkURL;

            Directory.CreateDirectory("VideoTemp").Attributes = FileAttributes.Hidden;

            FileDownloader(WorkURL, "U", "VideoTemp", null, DataRowRef);

            Module_Uploader.Report_Status("Converting Video to WebM...");
            string FullVideoFileName = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string VideoFileName = FullVideoFileName.Remove(FullVideoFileName.Length - 4);
            TimeSpan VideoDuration = new TimeSpan();
            int FrameCount = 0;
            using (var FFmpeg = new Process())
            {
                Form_Loader._FormReference.UploadQueueProcess = FFmpeg;
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.RedirectStandardError = true;
                FFmpeg.StartInfo.RedirectStandardOutput = true;
                for (int RunCount = 0; RunCount < 2; RunCount++)
                {
                    if (RunCount == 0)
                    {
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i VideoTemp\{0} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 1 -an -row-mt 1 -f webm NUL", FullVideoFileName);
                    }
                    else
                    {
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i VideoTemp\{0} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 VideoTemp\{1}.webm", FullVideoFileName, VideoFileName);
                    }

                    FFmpeg.Start();
                    System.Threading.Thread.Sleep(1000);
                    if (RunCount == 0)
                    {
                        string ReadLine = FFmpeg.StandardError.ReadLine();
                        while (ReadLine != null)
                        {
                            if (ReadLine.Contains("Duration:"))
                            {
                                VideoDuration = TimeSpan.Parse(ReadLine.Substring(12, 11));
                            }
                            else if (ReadLine.Contains("Stream #0:") && ReadLine.Contains(": Video:"))
                            {
                                FrameCount = (int)(VideoDuration.TotalSeconds * double.Parse(ReadLine.Substring(ReadLine.IndexOf(" kb/s,") + 7, 7).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]));
                                break;
                            }
                            ReadLine = FFmpeg.StandardError.ReadLine();
                        }
                    }

                    while (!FFmpeg.HasExited)
                    {
                        string ReadLine = FFmpeg.StandardError.ReadLine();
                        while (ReadLine != null)
                        {
                            if (ReadLine.StartsWith("frame= ", StringComparison.OrdinalIgnoreCase))
                            {
                                if (RunCount == 0)
                                {
                                    int CurrentFrame = int.Parse(ReadLine.Substring(0, ReadLine.IndexOf(" fps=")).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                    Module_Uploader.Report_Status(string.Format("Converting Video to WebM...{0}", (CurrentFrame / FrameCount / 2d).ToString("P0")));
                                }
                                else
                                {
                                    TimeSpan CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                                    Module_Uploader.Report_Status(string.Format("Converting Video to WebM...{0}", (0.5d + CurrentTime.TotalMilliseconds / VideoDuration.TotalMilliseconds / 2d).ToString("P0")));
                                }
                                break;
                            }
                            ReadLine = FFmpeg.StandardError.ReadLine();  // doesn't want to exit on first pass otherwise
                        }
                    }
                    FFmpeg.WaitForExit();
                }
            }
            Form_Loader._FormReference.UploadQueueProcess = null;
            Module_Uploader.Report_Status("Converting Ugoira to WebM...100%");
            bytes2Send = File.ReadAllBytes(string.Format(@"VideoTemp\{0}.webm", VideoFileName));
            FileName = VideoFileName + ".webm";
            Directory.Delete("VideoTemp", true);
        }






        public static void ConversionQueue_Ugoira2WebM(ref DataRow DataRowRef)
        {
            string WorkURL = (string)DataRowRef["Grab_URL"];
            Uri DomainURL = new Uri(WorkURL);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString) + @"\";

            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse(WorkURL))["body"];
            string UgoiraFileName = UgoiraJObject["originalSrc"].Value<string>();
            UgoiraFileName = UgoiraFileName.Remove(UgoiraFileName.Length - 4).Substring(UgoiraFileName.LastIndexOf("/") + 1);

            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, (string)DataRowRef["Artist"]);
            string FullFilePath = string.Format(@"{0}\{1}.webm", FolderPath, UgoiraFileName);

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string TempFolderName = "UgoiraTemp" + WorkID;
            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine(string.Format("file {0}/{1}", TempFolderName, UgoiraFrame["file"].Value<string>())); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine("duration " + UgoiraFrame["delay"].Value<int>() / 1000d);
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            if (Directory.Exists(TempFolderName))
            {
                Directory.Delete(TempFolderName, true);
            }
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            File.WriteAllText(TempFolderName + @"\input.txt", UgoiraConcat.ToString());
            DataRowRef["Upload_Tags"] = (string)DataRowRef["Upload_Tags"] + (TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime");
            FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "C", TempFolderName, FolderPath, DataRowRef, true);
            Module_Converter.Report_Status("Converting Ugoira to WebM...)");
            Directory.CreateDirectory(FolderPath);
            using (Process FFmpeg = new Process())
            {
                Form_Loader._FormReference.ConversionQueueProcess = FFmpeg;
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                // APNGs are too big.
                // If TotalUgoiraLength < 10000 Then '10 seconds
                // Dim Framerate As Double = 1 / (TotalUgoiraLength / 1000 / UgoiraJObject("frames").Count)
                // FFmpeg.StartInfo.Arguments = String.Format("-hide_banner -framerate {0} -i {1}\%06d.jpg -f apng -plays 0 ""{2}\{3}.png"" -y", Framerate, TempFolderName, FolderPath, UgoiraFileName) '-r is framerate
                // Else
                FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -f concat -i {0}\input.txt -c:v libvpx-vp9 -pix_fmt yuv420p -lossless 1 -an -row-mt 1 ""{1}\{2}.webm""", TempFolderName, FolderPath, UgoiraFileName);
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.RedirectStandardError = true;
                FFmpeg.Start();
                while (!FFmpeg.HasExited)
                {
                    string ReadLine = FFmpeg.StandardError.ReadLine();
                    while (ReadLine != null)
                    {
                        if (ReadLine.StartsWith("frame= ", StringComparison.OrdinalIgnoreCase))
                        {
                            var CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                            ReportConversionProgress("CU", CurrentTime.TotalMilliseconds / TotalUgoiraLength, ref DataRowRef);
                            break;
                        }
                        ReadLine = FFmpeg.StandardError.ReadLine();
                    }
                }
                Module_Converter.Report_Status("Converting Ugoira to WebM...100%");
                FFmpeg.WaitForExit();
            }
            Form_Loader._FormReference.ConversionQueueProcess = null;
            Directory.Delete(TempFolderName, true);

        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info(string.Format("Ugoira WebM already exists, skipped coverting {0}", WorkURL));
            }
            else
            {
                Module_Converter.Report_Info(string.Format("Converted Ugoira from: {0} to WebM", WorkURL));
            }
            DataRowRef["DL_FilePath"] = FullFilePath;
        }

        public static void ConversionQueue_Videos2WebM(ref DataRow DataRowRef)
        {
            string VideoFileName = (string)DataRowRef["Grab_MediaURL"];
            string VideoFormat = VideoFileName.Substring(VideoFileName.Length - 3);
            VideoFileName = VideoFileName.Remove(VideoFileName.Length - 4).Substring(VideoFileName.LastIndexOf("/") + 1);

            var DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString) + @"\";

            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, (string)DataRowRef["Artist"]);
            string FullFilePath = string.Format(@"{0}\{1}.webm", FolderPath, VideoFileName);

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string TempFolderName = "VideoTemp" + VideoFileName;
            if (Directory.Exists(TempFolderName))
            {
                Directory.Delete(TempFolderName, true);
            }
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;

            FileDownloader((string)DataRowRef["Grab_MediaURL"], "C", TempFolderName, FolderPath, DataRowRef);

            Module_Converter.Report_Status(string.Format("Converting {0} Video to WebM...", VideoFormat));
            Directory.CreateDirectory(FolderPath);
            TimeSpan VideoDuration = new TimeSpan();
            int FrameCount = 0;
            using (Process FFmpeg = new Process())
            {
                Form_Loader._FormReference.ConversionQueueProcess = FFmpeg;
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.RedirectStandardError = true;
                for (int RunCount = 0; RunCount < 2; RunCount++)
                {
                    if (RunCount == 0)
                    {
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i {0}\{1}.{2} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 1 -an -row-mt 1 -f webm NUL", TempFolderName, VideoFileName, VideoFormat);
                    }
                    else
                    {
                        // FFmpeg.StartInfo.Arguments = String.Format("-hide_banner -y -i {0}\{1}.{2} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 0 -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 ""{3}\{4}.webm""", TempFolderName, VideoFileName, VideoFormat, FolderPath, VideoFileName)
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i {0}\{1}.{2} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 ""{3}\{4}.webm""", TempFolderName, VideoFileName, VideoFormat, FolderPath, VideoFileName);
                    }

                    FFmpeg.Start();
                    System.Threading.Thread.Sleep(1000);
                    if (RunCount == 0)
                    {
                        string ReadLine = FFmpeg.StandardError.ReadLine();
                        while (ReadLine != null)
                        {
                            if (ReadLine.Contains("Duration:"))
                            {
                                VideoDuration = TimeSpan.Parse(ReadLine.Substring(12, 11));
                            }
                            else if (ReadLine.Contains("Stream #0:") && ReadLine.Contains(": Video:"))
                            {
                                // Stream #0:1(eng): Video: h264 (Main) (avc1 / 0x31637661), yuv420p(tv), 1920x1080 [SAR 1:1 DAR 16:9], 14070 kb/s, 29.97 fps, 29.97 tbr, 30k tbn, 59.94 tbc (default)
                                FrameCount = (int)(VideoDuration.TotalSeconds * double.Parse(ReadLine.Substring(ReadLine.IndexOf(" kb/s,") + 7, 7).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]));
                                break;
                            }
                            ReadLine = FFmpeg.StandardError.ReadLine();
                        }
                    }

                    while (!FFmpeg.HasExited)
                    {
                        string ReadLine = FFmpeg.StandardError.ReadLine();
                        while (ReadLine != null)
                        {
                            if (ReadLine.StartsWith("frame= ", StringComparison.OrdinalIgnoreCase))
                            {
                                if (RunCount == 0)
                                {
                                    int CurrentFrame = int.Parse(ReadLine.Substring(0, ReadLine.IndexOf(" fps=")).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                    ReportConversionProgress("CV", CurrentFrame / FrameCount / 2d, ref DataRowRef);
                                }
                                else
                                {
                                    var CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                                    ReportConversionProgress("CV", 0.5d + CurrentTime.TotalMilliseconds / VideoDuration.TotalMilliseconds / 2d, ref DataRowRef);
                                }
                                break;
                            }
                            ReadLine = FFmpeg.StandardError.ReadLine();  // doesn't want to exit on first pass otherwise
                        }
                    }
                    Module_Converter.Report_Status(string.Format("Converting {0} Video to WebM...100%", VideoFormat));
                    FFmpeg.WaitForExit();
                }
            }
            Form_Loader._FormReference.ConversionQueueProcess = null;
            Directory.Delete(TempFolderName, true);

        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info(string.Format("Video WebM already exists, skipped coverting {0}", (string)DataRowRef["Grab_MediaURL"]));
            }
            else
            {
                Module_Converter.Report_Info(string.Format("Converted Video from: {0} to WebM", (string)DataRowRef["Grab_MediaURL"]));
            }
            DataRowRef["DL_FilePath"] = FullFilePath;
        }

        private static void ReportConversionProgress(string ConversionType, double PercentProgress, ref DataRow DataRowRef)
        {
            Label ConversionLabel = Form_Loader._FormReference.label_ConversionStatus;
            Custom_LabelWithStroke DLLabel = null;
            if (Form_Preview._FormReference != null && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, DataRowRef))
            {
                DLLabel = Form_Preview._FormReference.Label_Download;
            }

            string PercentString = PercentProgress.ToString("P0");
            switch (ConversionType)
            {
                case "CU": //Upload Convert Ugoira
                    {
                        Module_Converter.Report_Status(string.Format("Converting Ugoira to WebM...{0}", PercentString));
                        break;
                    }

                case "CV": //Upload Convert Video
                    {
                        Module_Converter.Report_Status(string.Format("Converting Video to WebM...{0}", PercentString));
                        break;
                    }

                case "CDU": //Download Convert Ugoira
                    {
                        Module_Converter.Report_Status(string.Format("Downloading Ugoira...{0}", PercentString));
                        break;
                    }

                default: //case "CDV": //Download Convert Video
                    {
                        Module_Converter.Report_Status(string.Format("Downloading Video...{0}", PercentString));
                        break;
                    }
            }
            if (DLLabel != null)
            {
                Form_Preview._FormReference.BeginInvoke(new Action(() =>
                {
                    if (ConversionType.Contains("D"))
                    {
                        DLLabel.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        DLLabel.ForeColor = Color.DarkOrchid;
                    }
                    DLLabel.Text = string.Format("{0}", PercentString);
                }));
            }
        }




        public static void DownloadQueue_ConvertUgoira2WebM(ref e6_DownloadItem ControlReference)
        {
            BackgroundWorker ConvertWorker = new BackgroundWorker();
            ConvertWorker.DoWork += DownloadQueue_UgoiraConvertTask;
            ConvertWorker.RunWorkerCompleted += DownloadQueue_ConvertFinished;
            ConvertWorker.RunWorkerAsync(ControlReference);
        }

        private static void DownloadQueue_UgoiraConvertTask(object sender, DoWorkEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.Argument;
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;
            DataRow GridDataRow = (DataRow)DataRowTemp["DataRowRef"];

            string WorkURL = (string)DataRowTemp["Grab_URL"];
            Uri DomainURL = new Uri(WorkURL);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString) + @"\";

            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse(WorkURL))["body"];
            string UgoiraFileName = UgoiraJObject["originalSrc"].Value<string>();
            UgoiraFileName = UgoiraFileName.Remove(UgoiraFileName.Length - 4).Substring(UgoiraFileName.LastIndexOf("/") + 1);

            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, (string)DataRowTemp["Artist"]);
            string FullFilePath = string.Format(@"{0}\{1}.webm", FolderPath, UgoiraFileName);

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string TempFolderName = "UgoiraTemp" + WorkID;
            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine(string.Format("file {0}/{1}", TempFolderName, UgoiraFrame["file"].Value<string>())); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine("duration " + UgoiraFrame["delay"].Value<int>() / 1000d);
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            if (Directory.Exists(TempFolderName))
            {
                Directory.Delete(TempFolderName, true);
            }
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            File.WriteAllText(TempFolderName + @"\input.txt", UgoiraConcat.ToString());
            UgoiraConcat = null;

            FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "D", TempFolderName, FolderPath, null, true, e6_DownloadItemRef.DL_ProgressBar);
            UgoiraJObject = null;

            if (GridDataRow.RowState != DataRowState.Detached)
            {
                GridDataRow["Upload_Tags"] = (string)GridDataRow["Upload_Tags"] + (TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime") + " animated webm no_sound";
            }
            e6_DownloadItemRef.DL_ProgressBar.BarColor = Color.DarkOrchid;

            Directory.CreateDirectory(FolderPath);
            using (Process FFmpeg = new Process())
            {
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -f concat -i {0}\input.txt -c:v libvpx-vp9 -pix_fmt yuv420p -lossless 1 -an -row-mt 1 ""{1}\{2}.webm""", TempFolderName, FolderPath, UgoiraFileName);
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.RedirectStandardError = true;
                FFmpeg.Start();
                while (!FFmpeg.HasExited)
                {
                    string ReadLine = FFmpeg.StandardError.ReadLine();
                    while (ReadLine != null)
                    {
                        if (ReadLine.StartsWith("frame= ", StringComparison.OrdinalIgnoreCase))
                        {
                            var CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                            e6_DownloadItemRef.DL_ProgressBar.Value = (int)(100d - CurrentTime.TotalMilliseconds / TotalUgoiraLength * 100d);
                            break;
                        }
                        ReadLine = FFmpeg.StandardError.ReadLine();
                    }
                }
                FFmpeg.WaitForExit();
            }
            Directory.Delete(TempFolderName, true);

        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info(string.Format("Ugoira WebM already exists, skipped coverting {0}", WorkURL));
            }
            else
            {
                Module_Converter.Report_Info(string.Format("Converted Ugoira from: {0} to WebM", WorkURL));
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                if (Form_Preview._FormReference != null && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, GridDataRow))
                {
                    Form_Preview._FormReference.Label_Download.Visible = false;
                    Form_Preview._FormReference.PB_ViewFile.Visible = true;
                }
            }));
            e6_DownloadItemRef.DL_FolderIcon.Tag = FullFilePath;
            if (GridDataRow.RowState != DataRowState.Detached)
            {
                GridDataRow["DL_FilePath"] = FullFilePath;
            }
            e.Result = e6_DownloadItemRef;
        }

        private static void DownloadQueue_ConvertFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.Result;
            DataRow DataRowTemp = (DataRow)e6_DownloadItemRef.Tag;

            lock (Module_Downloader.Download_AlreadyDownloaded)
            {
                Module_Downloader.Download_AlreadyDownloaded.Add((string)DataRowTemp["Grab_MediaURL"]);
            }

            Image ImageHolder = e6_DownloadItemRef.picBox_ImageHolder.Image ?? e6_DownloadItemRef.picBox_ImageHolder.BackgroundImage;
            Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
            UIDrawController.SuspendDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);
            Module_Downloader.AddPic2FLP((string)DataRowTemp["Grab_ThumbnailURL"], e6_DownloadItemRef.DL_FolderIcon.Tag.ToString(), ImageHolder);
            Form_Loader._FormReference.DownloadFLP_Downloaded.ResumeLayout();
            UIDrawController.ResumeDrawing(Form_Loader._FormReference.DownloadFLP_Downloaded);

            e6_DownloadItemRef.Dispose();
            ((BackgroundWorker)sender).Dispose();
            Module_Downloader.timer_Download.Start();
        }



        public static void DragDropConvert(string VideoPath)
        {
            string VideoFileName = VideoPath;
            string VideoFormat = VideoFileName.Substring(VideoFileName.Length - 3);
            VideoFileName = VideoFileName.Remove(VideoFileName.Length - 4).Substring(VideoFileName.LastIndexOf(@"\") + 1);

            string FolderPath = Path.GetDirectoryName(VideoPath);
            string NewFilePath = string.Format(@"{0}\{1}.webm", FolderPath, VideoFileName);

            if (File.Exists(NewFilePath) && (MessageBox.Show("Converted video file already exists, do you want to continue regardless?", "e621 ReBot", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes))
            {
                return;
            } 

            using (Process FFmpeg = new Process())
            {
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                FFmpeg.StartInfo.CreateNoWindow = false;
                FFmpeg.StartInfo.RedirectStandardError = false;
                FFmpeg.StartInfo.RedirectStandardOutput = false;
                for (int RunCount = 0; RunCount < 2; RunCount++)
                {
                    if (RunCount == 0)
                    {
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i ""{0}"" -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 1 -an -row-mt 1 -f webm NUL", VideoPath);
                    }
                    else
                    {
                        FFmpeg.StartInfo.Arguments = string.Format(@"-hide_banner -y -i ""{0}"" -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 ""{1}""", VideoPath, NewFilePath);
                    }
                    FFmpeg.Start();
                    FFmpeg.WaitForExit();
                    if (FFmpeg.ExitCode < 0)
                    {
                        return; //canceled by user
                    }
                }
            }
        }
    }
}
