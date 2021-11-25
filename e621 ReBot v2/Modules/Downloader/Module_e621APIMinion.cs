using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Modules
{
    public static class Module_e621APIMinion
    {
        static Module_e621APIMinion()
        {
            WorkerMinion = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            WorkerMinion.DoWork += WorkWorkYouMinion;
            WorkerMinion.RunWorkerCompleted += YouCanRestNowMinion;
            timer_WorkDelay = new Timer
            {
                Interval = 500
            };
            timer_WorkDelay.Tick += AllowMinion2Rest;
        }

        public static BackgroundWorker WorkerMinion;
        public static List<Tuple<string, Action<object>, object>> _WorkQueue = new List<Tuple<string, Action<object>, object>>();
        private static readonly Timer timer_WorkDelay;

        public static void AddWork2Queue(string WorkTag, Action<object> WorkMethod, object WorkParameterPass)
        {
            _WorkQueue.Add(new Tuple<string, Action<object>, object>(WorkTag, WorkMethod, WorkParameterPass));
            if (!WorkerMinion.IsBusy)
            {
                WorkerMinion.RunWorkerAsync();
            }
        }

        public static void WorkWorkYouMinion(object sender, DoWorkEventArgs e)
        {
            Tuple<string, Action<object>, object> TupleTemp = _WorkQueue[0];
            lock (_WorkQueue)
            {
                _WorkQueue.RemoveAt(0);
            }
            e.Result = TupleTemp.Item1; //TupleTemp.Item2.Method.ReflectedType.Name;
            TupleTemp.Item2.Invoke(TupleTemp.Item3);
        }

        public static void YouCanRestNowMinion(object sender, RunWorkerCompletedEventArgs e)
        {
            Report_Status("Suspended.", false);
            if (_WorkQueue.Any())
            {
                timer_WorkDelay.Start();
            }
            //if (NotifyWhenDone)
            //{
            //    NotifyWhenDone = false;
            //    MessageBox.Show("Grabbing posts from e621 API is done, you can now queue more.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
        }

        private static void AllowMinion2Rest(object sender, EventArgs e)
        {
            timer_WorkDelay.Stop();
            if (_WorkQueue.Any())
            {
                WorkerMinion.RunWorkerAsync();
            }
        }



        // - - - - - - - - - - -

        private static void Report_Status(string StatusMessage, bool ButtonEnable, string TooltipPass = null)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.label_DownloadStatus.Text = $"API DL Status: {StatusMessage}";
                Form_Loader._FormReference.bU_CancelAPIDL.Enabled = ButtonEnable;
                Form_Loader._FormReference.toolTip_Display.SetToolTip(Form_Loader._FormReference.label_DownloadStatus, TooltipPass ?? null);
            }));
        }

        public static void GrabAllImagesWithGivenTags(object ParameterPass)
        {
            string[] ArgumentPass = (string[])ParameterPass;
            string TagQuery = ArgumentPass[0];
            string FolderName = ArgumentPass[1];

            string PostRequestString = $"https://e621.net/posts.json?limit=320&tags={TagQuery}";
            int PageCounter = 1;
        GrabAnotherPage:
            Report_Status($"Working on Tags - page {PageCounter}", true, HttpUtility.UrlDecode(PostRequestString));
            JToken JSON_Object = JObject.Parse(Module_e621Info.e621InfoDownload($"{PostRequestString}{(PageCounter > 1 ? $"&page={PageCounter}" : null)}", true))["posts"];
            foreach (JObject cPost in JSON_Object.Children())
            {
                List<string> TempTagList = CreateTagList(cPost["tags"], cPost["rating"].Value<string>());
                if (!Blacklist_Check(TempTagList))
                {
                    string PostID = cPost["id"].Value<string>();
                    Module_Downloader.AddDownloadQueueItem(
                        DataRowRef: null,
                        URL: $"https://e621.net/posts/{PostID}",
                        Media_URL: cPost["file"]["url"].Value<string>(),
                        e6_PostID: PostID,
                        e6_PoolName: FolderName
                        );
                }
            }
            PageCounter += 1;

            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Module_Downloader.UpdateTreeViewText();
                Module_Downloader.UpdateTreeViewNodes();
                Module_Downloader.timer_Download.Start();
            }));
            if (WorkerMinion.CancellationPending)
            {
                return;
            }
            if (JSON_Object.Children().Count() == 320)
            {
                Thread.Sleep(500);
                goto GrabAnotherPage;
            }
            JSON_Object = null;
        }

        public static void GraBPoolInBG(object ParameterPass)
        {
            string PoolID = (string)ParameterPass;

            JToken PoolJSON = JObject.Parse(Module_e621Info.e621InfoDownload($"https://e621.net/pools/{PoolID}.json", false));
            string PoolName = PoolJSON["name"].Value<string>().Replace("_", " ");
            PoolName = string.Join("", PoolName.Split(Path.GetInvalidFileNameChars()));
            string FolderPath = Path.Combine(Properties.Settings.Default.DownloadsFolderLocation, @"e621\", PoolName).ToString();

            List<string> FoundComicPages = new List<string>();
            if (Directory.Exists(FolderPath))
            {
                foreach (string FileFound in Directory.GetFiles(FolderPath))
                {
                    string CutPageName = FileFound.Substring(FileFound.LastIndexOf("_") + 1);
                    FoundComicPages.Add(CutPageName);
                }
            }

            List<string> ComicPages = PoolJSON["post_ids"].Values<string>().ToList();
            int PageCount = (int)Math.Ceiling(PoolJSON["post_ids"].Count() / 320d);
            string PoolRequestString = $"https://e621.net/posts.json?limit=320&tags=pool:{PoolID}";
            string e6JSONResult;
            int SkippedPagesCounter = 0;
            for (int p = 1; p <= PageCount; p++)
            {
                Report_Status($"Working on Pool#{PoolID} - Page {p}.", true);
                e6JSONResult = Module_e621Info.e621InfoDownload($"{PoolRequestString}{(p > 1 ? $"&page={p}" : null)}", true);

                JToken JSON_Object = JObject.Parse(e6JSONResult)["posts"];
                foreach (JObject Post in JSON_Object)
                {
                    string PicURL = Post["file"]["url"].Value<string>();
                    string PicName = PicURL.Substring(PicURL.LastIndexOf("/") + 1);
                    if (FoundComicPages.Contains(PicName))
                    {
                        SkippedPagesCounter += 1;
                        continue;
                    }

                    string PostID = Post["id"].Value<string>();
                    Module_Downloader.AddDownloadQueueItem(
                        DataRowRef: null,
                        URL: $"https://e621.net/posts/{PostID}",
                        Media_URL: PicURL,
                        e6_PostID: PostID,
                        e6_PoolName: PoolName,
                        e6_PoolPostIndex: ComicPages.IndexOf(PostID).ToString()
                        );
                }

                Form_Loader._FormReference.BeginInvoke(new Action(() =>
                {
                    Module_Downloader.UpdateTreeViewText();
                    Module_Downloader.UpdateTreeViewNodes();
                    Module_Downloader.timer_Download.Start();
                }));
                if (WorkerMinion.CancellationPending)
                {
                    return;
                }
                JSON_Object = null;
            }
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                if (SkippedPagesCounter > 0)
                {
                    Form_Loader._FormReference.textBox_Info.Text = $"{DateTime.Now.ToLongTimeString()} Downloader >>> {PoolName}: {SkippedPagesCounter} page{(SkippedPagesCounter > 1 ? "s" : null)} skipped as they already exist\n{Form_Loader._FormReference.textBox_Info.Text}";
                }
            }));
        }

        private static List<string> CreateTagList(JToken PostTags, string RatingTag)
        {
            List<string> TempList = new List<string>();
            foreach (JProperty TagCategory in PostTags.Children())
            {
                TempList.AddRange(TagCategory.First.ToObject<string[]>());
            }
            TempList.Add("rating:" + RatingTag);
            return TempList;
        }

        private static bool Blacklist_Check(List<string> PostTags)
        {
            if (Form_Loader._FormReference.Blacklist.Count > 0)
            {

                foreach (string BlacklistLine in Form_Loader._FormReference.Blacklist)
                {
                    if (BlacklistLine.Contains("-"))
                    {
                        List<string> BlacklistLineList = new List<string>();
                        BlacklistLineList.AddRange(BlacklistLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        int HitCounter = 0;
                        foreach (string BlacklistTag in BlacklistLineList)
                        {
                            string BlacklistTagTemp = BlacklistTag;
                            if (BlacklistTag.StartsWith("-"))
                            {
                                BlacklistTagTemp = BlacklistTag.Substring(1);
                                if (!PostTags.Contains(BlacklistTagTemp))
                                {
                                    HitCounter += 1;
                                }
                                continue;
                            }

                            if (PostTags.Contains(BlacklistTagTemp))
                            {
                                HitCounter += 1;
                            }
                        }

                        if (HitCounter == BlacklistLineList.Count)
                        {
                            return true;
                        }
                        continue;
                    }

                    if (BlacklistLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).All(f => PostTags.Contains(f)))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
    }
}