using e621_ReBot.Modules;
using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public class Module_FFmpeg
    {
        public static void ReportConversionProgress(string ConversionType, double PercentProgress, in DataRow DataRowRef)
        {
            string PercentString = PercentProgress.ToString("P0");
            switch (ConversionType)
            {
                case "CU": //Upload Convert Ugoira
                    {
                        Module_Converter.Report_Status($"Converting Ugoira to WebM...{PercentString}");
                        break;
                    }

                case "CV": //Upload Convert Video
                    {
                        Module_Converter.Report_Status($"Converting Video to WebM...{PercentString}");
                        break;
                    }

                case "CDU": //Download Convert Ugoira
                    {
                        Module_Converter.Report_Status($"Downloading Ugoira...{PercentString}");
                        break;
                    }

                default: //case "CDV": //Download Convert Video
                    {
                        Module_Converter.Report_Status($"Downloading Video...{PercentString}");
                        break;
                    }
            }

            if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, DataRowRef))
            {
                Form_Preview._FormReference.BeginInvoke(new Action(() =>
                {
                    if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated)
                    {
                        Form_Preview._FormReference.Label_Download.ForeColor = ConversionType.Contains("D") ? Color.DarkOrange : Color.DarkOrchid;
                        Form_Preview._FormReference.Label_Download.Text = PercentString;
                        Form_Preview._FormReference.PB_Download.Visible = false;
                        Form_Preview._FormReference.Label_DownloadWarning.Visible = false;
                    }
                }));
            }
        }

        private static void FFmpeg4Ugoira(string ConversionType, string TempFolderName, string FullFolderPath, string UgoiraFileName, int UgoiraDuration, in DataRow DataRowRef = null, Custom_ProgressBar RoundProgressBarRef = null)
        {
            using (Process FFmpeg = new Process())
            {
                // APNGs are too big.
                // If TotalUgoiraLength < 10000 Then '10 seconds
                // Dim Framerate As Double = 1 / (TotalUgoiraLength / 1000 / UgoiraJObject("frames").Count)
                //FFmpeg.StartInfo.Arguments = string.Format("-hide_banner -y  -f concat -i {0}\input.txt apng -plays 0 ""{2}\{3}.png"" -y", Framerate, TempFolderName, FolderPath, UgoiraFileName) '-r is framerate
                //FFmpeg.StartInfo.Arguments = $"-hide_banner -y -f concat -i {TempFolderName}\\input.txt -plays 0 \"{FolderPath}\\{UgoiraFileName}.apng\"";
                // else

                Form_Loader._FormReference.UploadQueueProcess = FFmpeg;
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.Arguments = $"-hide_banner -y -f concat -i {TempFolderName}\\input.txt -c:v libvpx-vp9 -pix_fmt yuv420p -lossless 1 -an -row-mt 1 \"{FullFolderPath}\\{UgoiraFileName}.webm\"";
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
                            switch (ConversionType)
                            {
                                case "C":
                                    {
                                        ReportConversionProgress("CU", CurrentTime.TotalMilliseconds / UgoiraDuration, in DataRowRef);
                                        break;
                                    }

                                case "D":
                                    {
                                        RoundProgressBarRef.Value = (int)(100d - CurrentTime.TotalMilliseconds / UgoiraDuration * 100d);
                                        break;
                                    }

                                case "U":
                                    {
                                        Module_Uploader.Report_Status(string.Format("Converting Ugoira to WebM...{0}", (CurrentTime.TotalMilliseconds / UgoiraDuration).ToString("P0")));
                                        break;
                                    }
                            }
                        }
                        ReadLine = FFmpeg.StandardError.ReadLine(); // doesn't want to exit on first pass otherwise
                    }
                }
                FFmpeg.WaitForExit();
            }
        }

        private static void FFmpeg4Video(string ConversionType, string TempFolderName, string TempVideoFileName, string TempVideoFormat, string FullFolderPath = null, string VideoFileName = null, in DataRow DataRowRef = null, Custom_ProgressBar RoundProgressBarRef = null)
        {
            TimeSpan VideoDuration = new TimeSpan();
            using (Process FFmpeg = new Process())
            {
                switch (ConversionType)
                {
                    case "C":
                        {
                            Form_Loader._FormReference.ConversionQueueProcess = FFmpeg;
                            break;
                        }

                    case "U":
                        {
                            Form_Loader._FormReference.UploadQueueProcess = FFmpeg;
                            break;
                        }
                }
                FFmpeg.StartInfo.FileName = "ffmpeg.exe";
                FFmpeg.StartInfo.UseShellExecute = false;
                FFmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                FFmpeg.StartInfo.CreateNoWindow = true;
                FFmpeg.StartInfo.RedirectStandardError = true;
                for (int RunCount = 0; RunCount < 2; RunCount++)
                {
                    if (RunCount == 0)
                    {
                        FFmpeg.StartInfo.Arguments = $"-hide_banner -y -i {TempFolderName}\\{TempVideoFileName}.{TempVideoFormat} -c:v libvpx-vp9 -pix_fmt yuv420p -b:a 192k -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 1 -an -row-mt 1 -f webm NUL";
                    }
                    else
                    {
                        // FFmpeg.StartInfo.Arguments = String.Format("-hide_banner -y -i {0}\{1}.{2} -c:v libvpx-vp9 -pix_fmt yuv420p -b:v 0 -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 ""{3}\{4}.webm""", TempFolderName, VideoFileName, VideoFormat, FolderPath, VideoFileName)
                        FFmpeg.StartInfo.Arguments = $"-hide_banner -y -i {TempFolderName}\\{TempVideoFileName}.{TempVideoFormat} -c:v libvpx-vp9 -pix_fmt yuv420p -b:a 192k -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 \"{FullFolderPath}\\{VideoFileName}.webm\"";
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
                                int FrameCount = (int)(VideoDuration.TotalSeconds * double.Parse(ReadLine.Substring(ReadLine.IndexOf(" kb/s,") + 7, 7).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]));
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
                                //if (RunCount == 0)
                                //{
                                //    int CurrentFrame = int.Parse(ReadLine.Substring(0, ReadLine.IndexOf(" fps=")).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                //    if (ConversionType.Equals("C"))
                                //    {
                                //        ReportConversionProgress("CV", CurrentFrame / FrameCount / 2d, in DataRowRef);
                                //    }
                                //    else
                                //    {
                                //        Module_Uploader.Report_Status($"Converting Video to WebM...{(CurrentFrame / FrameCount / 2d):P0}");
                                //    }
                                //}
                                //else
                                //{
                                TimeSpan CurrentTime = TimeSpan.Parse(ReadLine.Substring(ReadLine.IndexOf("time=") + 5, 11));
                                switch (ConversionType)
                                {
                                    case "C":
                                        {
                                            ReportConversionProgress("CV", CurrentTime.TotalMilliseconds / VideoDuration.TotalMilliseconds, in DataRowRef);
                                            break;
                                        }

                                    case "D":
                                        {
                                            RoundProgressBarRef.Value = (int)(100d - CurrentTime.TotalMilliseconds / VideoDuration.TotalMilliseconds * 100d);
                                            break;
                                        }

                                    case "U":
                                        {
                                            Module_Uploader.Report_Status($"Converting {TempVideoFormat} Video to WebM...{(CurrentTime.TotalMilliseconds / VideoDuration.TotalMilliseconds):P0}");
                                            break;
                                        }
                                }
                                //}
                                break;
                            }
                            ReadLine = FFmpeg.StandardError.ReadLine();  // doesn't want to exit on first pass otherwise
                        }
                    }
                    FFmpeg.WaitForExit();
                }
            }
        }

        private static string UgoiraJSONResponse(string WorkURL)
        {
            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            Module_CookieJar.GetCookies(WorkURL, ref Module_CookieJar.Cookies_Pixiv);
            HttpWebRequest PixivDownload = (HttpWebRequest)WebRequest.Create(string.Format("https://www.pixiv.net/ajax/illust/{0}/ugoira_meta", WorkID));
            PixivDownload.CookieContainer = Module_CookieJar.Cookies_Pixiv;
            PixivDownload.Timeout = 5000;
            PixivDownload.UserAgent = Form_Loader.GlobalUserAgent;
            using (StreamReader PixivStreamReader = new StreamReader(PixivDownload.GetResponse().GetResponseStream()))
            {
                return PixivStreamReader.ReadToEnd();
            }
        }

        // = = =

        public static void UploadQueue_Ugoira2WebM(ref DataRow DataRowRef, out byte[] bytes2Send, out string FileName, out string ExtraSourceURL)
        {
            string WorkURL = (string)DataRowRef["Grab_URL"];

            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse(WorkURL))["body"];

            string UgoiraFileName = UgoiraJObject["originalSrc"].Value<string>();
            ExtraSourceURL = UgoiraFileName;
            UgoiraFileName = UgoiraFileName.Substring(UgoiraFileName.LastIndexOf("/") + 1);
            UgoiraFileName = UgoiraFileName.Substring(0, UgoiraFileName.LastIndexOf("."));

            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine($"file {UgoiraFrame["file"].Value<string>()}"); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine($"duration {UgoiraFrame["delay"].Value<int>() / 1000d}");
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            if (Directory.Exists("UgoiraTemp")) Directory.Delete("UgoiraTemp", true);
            Directory.CreateDirectory("UgoiraTemp").Attributes = FileAttributes.Hidden;
            File.WriteAllText(@"UgoiraTemp\input.txt", UgoiraConcat.ToString());
            UgoiraConcat = null;

            DataRowRef["Upload_Tags"] = (string)DataRowRef["Upload_Tags"] + (TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime");

            Module_Downloader.FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "U", "UgoiraTemp", null, in DataRowRef, true);
            UgoiraJObject = null;

            Module_Uploader.Report_Status("Converting Ugoira to WebM...");
            FFmpeg4Ugoira("U", "UgoiraTemp", "UgoiraTemp", UgoiraFileName, TotalUgoiraLength);
            Module_Uploader.Report_Status("Converting Ugoira to WebM...100%");
            Form_Loader._FormReference.UploadQueueProcess = null;

            FileName = $"{UgoiraFileName}.webm";
            bytes2Send = File.ReadAllBytes($"UgoiraTemp\\{FileName}");
            Directory.Delete("UgoiraTemp", true);
        }

        public static void UploadQueue_Videos2WebM(ref DataRow DataRowRef, out byte[] bytes2Send, out string FileName, out string ExtraSourceURL)
        {
            string WorkURL = (string)DataRowRef["Grab_MediaURL"];
            ExtraSourceURL = WorkURL;

            string VideoFileName = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string VideoFormat = VideoFileName.Substring(VideoFileName.LastIndexOf(".") + 1);
            VideoFileName = VideoFileName.Substring(0, VideoFileName.LastIndexOf("."));
            //string VideoFileName = TempVideoFileName;

            if (Directory.Exists("VideoTemp")) Directory.Delete("VideoTemp", true);
            Directory.CreateDirectory("VideoTemp").Attributes = FileAttributes.Hidden;
            Module_Downloader.FileDownloader(WorkURL, "U", "VideoTemp", null, in DataRowRef);

            Module_Uploader.Report_Status($"Converting {VideoFormat} Video to WebM...");
            FFmpeg4Video("U", "VideoTemp", VideoFileName, VideoFormat, "VideoTemp", VideoFileName);
            Module_Uploader.Report_Status($"Converting {VideoFormat} Video to WebM...100%");
            Form_Loader._FormReference.UploadQueueProcess = null;

            FileName = $"{VideoFileName}.webm";
            bytes2Send = File.ReadAllBytes($"VideoTemp\\{FileName}");
            Directory.Delete("VideoTemp", true);
        }

        // = = =

        public static void ConversionQueue_Ugoira2WebM(ref DataRow DataRowRef)
        {
            string WorkURL = (string)DataRowRef["Grab_URL"];
            var DomainURL = new Uri(WorkURL);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse(WorkURL))["body"];

            string UgoiraFileName = UgoiraJObject["originalSrc"].Value<string>();
            UgoiraFileName = UgoiraFileName.Substring(UgoiraFileName.LastIndexOf("/") + 1);
            UgoiraFileName = UgoiraFileName.Substring(0, UgoiraFileName.LastIndexOf("."));

            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)DataRowRef["Artist"]).Replace("/", "-"));
            string FullFilePath = $"{FullFolderPath}\\{UgoiraFileName}.webm";

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string TempFolderName = $"UgoiraTemp{WorkID}";

            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine($"file {UgoiraFrame["file"].Value<string>()}"); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine($"duration {UgoiraFrame["delay"].Value<int>() / 1000d}");
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            if (Directory.Exists(TempFolderName)) Directory.Delete(TempFolderName, true);
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            File.WriteAllText($"{TempFolderName}\\input.txt", UgoiraConcat.ToString());
            UgoiraConcat = null;

            DataRowRef["Upload_Tags"] = $"{(string)DataRowRef["Upload_Tags"]}{(TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime")}";

            Module_Downloader.FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "C", TempFolderName, FullFolderPath, in DataRowRef, true);
            UgoiraJObject = null;

            Module_Converter.Report_Status("Converting Ugoira to WebM...");
            Directory.CreateDirectory(FullFolderPath);
            FFmpeg4Ugoira("C", TempFolderName, FullFolderPath, UgoiraFileName, TotalUgoiraLength, in DataRowRef);
            Module_Converter.Report_Status("Converting Ugoira to WebM...100%");
            Form_Loader._FormReference.ConversionQueueProcess = null;

            Directory.Delete(TempFolderName, true);
        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info($"Ugoira WebM already exists, skipped coverting {WorkURL}");
            }
            else
            {
                Module_Converter.Report_Info($"Converted Ugoira from: {WorkURL} to WebM");
            }
            DataRowRef["DL_FilePath"] = FullFilePath;
        }

        public static void ConversionQueue_Videos2WebM(ref DataRow DataRowRef)
        {
            string WorkURL = (string)DataRowRef["Grab_MediaURL"];
            var DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            string VideoFileName = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string VideoFormat = VideoFileName.Substring(VideoFileName.LastIndexOf(".") + 1);
            VideoFileName = VideoFileName.Substring(0, VideoFileName.LastIndexOf("."));

            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)DataRowRef["Artist"]).Replace("/", "-"));
            string FullFilePath = $"{FullFolderPath}\\{VideoFileName}.webm";

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string TempFolderName = $"VideoTemp{VideoFileName}";
            if (Directory.Exists(TempFolderName)) Directory.Delete(TempFolderName, true);
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            Module_Downloader.FileDownloader(WorkURL, "C", TempFolderName, FullFolderPath, in DataRowRef);

            Module_Converter.Report_Status($"Converting {VideoFormat} Video to WebM...");
            Directory.CreateDirectory(FullFolderPath);
            FFmpeg4Video("C", TempFolderName, VideoFileName, VideoFormat, FullFolderPath, VideoFileName, in DataRowRef);
            Module_Converter.Report_Status($"Converting {VideoFormat} Video to WebM...100%");
            Form_Loader._FormReference.ConversionQueueProcess = null;

            Directory.Delete(TempFolderName, true);
        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info($"Video WebM already exists, skipped coverting {WorkURL}");
            }
            else
            {
                Module_Converter.Report_Info($"Converted Video from: {WorkURL} to WebM");
            }
            DataRowRef["DL_FilePath"] = FullFilePath;
        }

        // = = =

        public static void DownloadQueue_ConvertUgoira2WebM(ref e6_DownloadItem ControlReference)
        {
            BackgroundWorker ConvertWorker = new BackgroundWorker();
            ConvertWorker.DoWork += DownloadQueue_UgoiraConvertTask;
            ConvertWorker.RunWorkerCompleted += DownloadQueue_ConvertFinished;
            ConvertWorker.RunWorkerAsync(ControlReference);
        }

        public static void DownloadQueue_ConvertVideo2WebM(ref e6_DownloadItem ControlReference)
        {
            BackgroundWorker ConvertWorker = new BackgroundWorker();
            ConvertWorker.DoWork += DownloadQueue_VideoConvertTask;
            ConvertWorker.RunWorkerCompleted += DownloadQueue_ConvertFinished;
            ConvertWorker.RunWorkerAsync(ControlReference);
        }

        private static void DownloadQueue_UgoiraConvertTask(object sender, DoWorkEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.Argument;
            DataRow DataRowRef = (DataRow)e6_DownloadItemRef.Tag;
            DataRow GridDataRow = (DataRow)DataRowRef["DataRowRef"];

            string WorkURL = (string)DataRowRef["Grab_URL"];
            var DomainURL = new Uri(WorkURL);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            JToken UgoiraJObject = JObject.Parse(UgoiraJSONResponse(WorkURL))["body"];

            string UgoiraFileName = UgoiraJObject["originalSrc"].Value<string>();
            UgoiraFileName = UgoiraFileName.Substring(UgoiraFileName.LastIndexOf("/") + 1);
            UgoiraFileName = UgoiraFileName.Substring(0, UgoiraFileName.LastIndexOf("."));

            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)DataRowRef["Artist"]).Replace("/", "-"));
            string FullFilePath = $"{FullFolderPath}\\{UgoiraFileName}.webm";

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string WorkID = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string TempFolderName = $"UgoiraTemp{WorkID}";

            int TotalUgoiraLength = 0;
            StringBuilder UgoiraConcat = new StringBuilder();
            foreach (JToken UgoiraFrame in UgoiraJObject["frames"])
            {
                UgoiraConcat.AppendLine($"file {UgoiraFrame["file"].Value<string>()}"); // FFmpeg wants / instead \
                UgoiraConcat.AppendLine($"duration {UgoiraFrame["delay"].Value<int>() / 1000d}");
                TotalUgoiraLength += UgoiraFrame["delay"].Value<int>();
            }
            if (Directory.Exists(TempFolderName)) Directory.Delete(TempFolderName, true);
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            File.WriteAllText($"{TempFolderName}\\input.txt", UgoiraConcat.ToString());
            UgoiraConcat = null;

            Module_Downloader.FileDownloader(UgoiraJObject["originalSrc"].Value<string>(), "D", TempFolderName, FullFolderPath, null, true, e6_DownloadItemRef.DL_ProgressBar);
            UgoiraJObject = null;

            if (GridDataRow.RowState != DataRowState.Detached)
            {
                GridDataRow["Upload_Tags"] = (string)GridDataRow["Upload_Tags"] + (TotalUgoiraLength < 30000 ? " short_playtime" : " long_playtime") + " animated webm no_sound";
            }
            e6_DownloadItemRef.DL_ProgressBar.BarColor = Color.DarkOrchid;

            Directory.CreateDirectory(FullFolderPath);
            FFmpeg4Ugoira("D", TempFolderName, FullFolderPath, UgoiraFileName, TotalUgoiraLength, null, e6_DownloadItemRef.DL_ProgressBar);

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
                if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, GridDataRow))
                {
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

        private static void DownloadQueue_VideoConvertTask(object sender, DoWorkEventArgs e)
        {
            e6_DownloadItem e6_DownloadItemRef = (e6_DownloadItem)e.Argument;
            DataRow DataRowRef = (DataRow)e6_DownloadItemRef.Tag;
            DataRow GridDataRow = (DataRow)DataRowRef["DataRowRef"];

            var DomainURL = new Uri((string)DataRowRef["Grab_URL"]);
            string HostString = DomainURL.Host.Remove(DomainURL.Host.LastIndexOf(".")).Replace("www.", "");
            HostString = $"{new CultureInfo("en-US", false).TextInfo.ToTitleCase(HostString)}\\";

            string WorkURL = (string)DataRowRef["Grab_MediaURL"];
            string VideoFileName = WorkURL.Substring(WorkURL.LastIndexOf("/") + 1);
            string VideoFormat = VideoFileName.Substring(VideoFileName.LastIndexOf(".") + 1);
            VideoFileName = VideoFileName.Substring(0, VideoFileName.LastIndexOf("."));

            string FullFolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, HostString, ((string)DataRowRef["Artist"]).Replace("/", "-"));
            string FullFilePath = $"{FullFolderPath}\\{VideoFileName}.webm";

            bool SkippedConvert = false;
            if (File.Exists(FullFilePath))
            {
                SkippedConvert = true;
                goto SkipDLandConvert;
            }

            string TempFolderName = $"VideoTemp{VideoFileName}";
            if (Directory.Exists(TempFolderName)) Directory.Delete(TempFolderName, true);
            Directory.CreateDirectory(TempFolderName).Attributes = FileAttributes.Hidden;
            Module_Downloader.FileDownloader(WorkURL, "D", TempFolderName, FullFolderPath, null, false, e6_DownloadItemRef.DL_ProgressBar);

            e6_DownloadItemRef.DL_ProgressBar.BarColor = Color.DarkOrchid;
            Directory.CreateDirectory(FullFolderPath);
            FFmpeg4Video("D", TempFolderName, VideoFileName, VideoFormat, FullFolderPath, VideoFileName, null, e6_DownloadItemRef.DL_ProgressBar);

            Directory.Delete(TempFolderName, true);
        SkipDLandConvert:
            if (SkippedConvert)
            {
                Module_Converter.Report_Info($"Video WebM already exists, skipped coverting {WorkURL}");
            }
            else
            {
                Module_Converter.Report_Info($"Converted Video from: {WorkURL} to WebM");
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, GridDataRow))
                {
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

        // = = =

        public static void DragDropConvert(string VideoPath)
        {
            string VideoFileName = VideoPath.Substring(VideoPath.LastIndexOf("\\") + 1);
            VideoFileName = VideoFileName.Substring(0, VideoFileName.LastIndexOf("."));

            string FullFolderPath = Path.GetDirectoryName(VideoPath);
            string FullFilePath = $"{FullFolderPath}\\{VideoFileName}.webm";

            if (File.Exists(FullFilePath) && (MessageBox.Show("Converted video file already exists, do you want to continue regardless?", "e621 ReBot", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes))
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
                        FFmpeg.StartInfo.Arguments = $"-hide_banner -y -i \"{VideoPath}\" -c:v libvpx-vp9 -pix_fmt yuv420p -b:a 192k -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 1 -an -row-mt 1 -f webm NUL";
                    }
                    else
                    {
                        FFmpeg.StartInfo.Arguments = $"-hide_banner -y -i \"{VideoPath}\" -c:v libvpx-vp9 -pix_fmt yuv420p -b:a 192k -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2 -row-mt 1 \"{FullFilePath}\"";
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