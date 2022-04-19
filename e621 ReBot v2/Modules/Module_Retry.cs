using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Modules
{
    public class Module_Retry
    {
        static Module_Retry()
        {
            timer_Retry = new Timer
            {
                Interval = 1000
            };
            timer_Retry.Tick += Timer_Retry_Tick;
            timer_RetryDisable = new Timer
            {
                Interval = 1000
            };
            timer_RetryDisable.Tick += Timer_RetryDisable_Tick;
            Retry_BGW = new BackgroundWorker();
            Retry_BGW.DoWork += RetryBGW_Start;
            Retry_BGW.RunWorkerCompleted += RetryBGW_Done;
        }

        public static KeyValuePair<HttpWebResponse, string> SEND_Request(string SendMethod, string URL, Dictionary<string, string> SEND_Dictionary)
        {
            string MPF_Boundary = "--------" + DateTime.Now.Ticks.ToString("x");
            MultipartFormDataContent EncodedContent = new MultipartFormDataContent(MPF_Boundary);
            foreach (KeyValuePair<string, string> DataPair in SEND_Dictionary)
            {
                EncodedContent.Add(new StringContent(DataPair.Value), DataPair.Key);
            }
            //string PostText = EncodedContent.ReadAsStringAsync().Result.ToString();

            using (Stream EncodedContentStream = EncodedContent.ReadAsStreamAsync().Result)
            {
                HttpWebRequest e6Uploader = (HttpWebRequest)WebRequest.Create(URL);
                e6Uploader.UserAgent = Properties.Settings.Default.AppName;
                e6Uploader.ContentType = "multipart/form-data; boundary=" + MPF_Boundary;
                e6Uploader.ContentLength = EncodedContentStream.Length;
                e6Uploader.Method = SendMethod;
                e6Uploader.Timeout = 33333; // timeout is for request start until response end
                EncodedContentStream.CopyTo(e6Uploader.GetRequestStream());

                Report_Status("Waiting for e621 response...");
                HttpWebResponse e6Response = null;
                string Response = null;
                try
                {
                    e6Response = (HttpWebResponse)e6Uploader.GetResponse();
                }
                catch (WebException webex)
                {
                    e6Response = (HttpWebResponse)webex.Response;
                }
                finally
                {
                    using (StreamReader TempSR = new StreamReader(e6Response.GetResponseStream()))
                    {
                        Response = TempSR.ReadToEnd();
                    }
                    // e6Response.Dispose()
                }
                return new KeyValuePair<HttpWebResponse, string>(e6Response, Response);
            }
        }

        private static void Report_Status(string StatusMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.label_RetryStatus.Text = string.Format("Retry: {0}", StatusMessage);
            }
            ));
        }

        public static void RetryDisable(string message)
        {
            Form_Loader._FormReference.cCheckGroupBox_Retry.CheckedChanged -= Form_Loader._FormReference.CCheckGroupBox_Retry_CheckedChanged;
            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = false;
                MessageBox.Show(message, "e621 ReBot");
            }));
            Form_Loader._FormReference.cCheckGroupBox_Retry.CheckedChanged += Form_Loader._FormReference.CCheckGroupBox_Retry_CheckedChanged;

        }

        public static Timer timer_Retry;
        private static void Timer_Retry_Tick(object sender, EventArgs e)
        {
            if (Form_Loader._FormReference.cCheckGroupBox_Retry.Checked && Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Count > 0 && !Retry_BGW.IsBusy)
            {
                timer_Retry.Stop();
                Retry_BGW.RunWorkerAsync();
            }
        }

        public static Timer timer_RetryDisable;
        private static void Timer_RetryDisable_Tick(object sender, EventArgs e)
        {
            timer_RetryDisable.Stop();
            if (Module_Credits.Timestamps_Flag.Count > 0)
            {
                for (int i = 0; i < Module_Credits.Timestamps_Flag.Count; i++)
                {
                    if (DateTime.UtcNow > Module_Credits.Timestamps_Flag[i])
                    {
                        Module_Credits.Timestamps_Flag.RemoveAt(i);
                        Module_Credits.Credit_Flag += 1;
                    }
                }
            }

            if (Module_Credits.Timestamps_Notes.Count > 0)
            {
                for (int i = 0; i < Module_Credits.Timestamps_Notes.Count; i++)
                {
                    if (DateTime.UtcNow > Module_Credits.Timestamps_Notes[i])
                    {
                        Module_Credits.Timestamps_Notes.RemoveAt(i);
                        Module_Credits.Credit_Notes += 1;
                    }
                }
            }

            List<DateTime> TempList = new List<DateTime>();
            if (Module_Credits.Timestamps_Flag.Count > 0)
            {
                TempList.Add(Module_Credits.Timestamps_Flag[0]);
            }
            if (Module_Credits.Timestamps_Notes.Count > 0)
            {
                TempList.Add(Module_Credits.Timestamps_Notes[0]);
            }
            TempList.Sort();
            if (TempList.Count > 0)
            {
                timer_RetryDisable.Stop();
                TimeSpan TempTimeSpan = TempList[0] - DateTime.UtcNow;
                if (TempTimeSpan.TotalSeconds < 60d)
                {
                    timer_RetryDisable.Interval = 1000; // second
                    Form_Loader._FormReference.cCheckGroupBox_Retry.Text = string.Format("Retry Queue (available again in {0} seconds)", TempTimeSpan.Seconds);
                }
                else
                {
                    timer_RetryDisable.Interval = 60000; // minute
                    Form_Loader._FormReference.cCheckGroupBox_Retry.Text = string.Format("Retry Queue (available again in {0} minutes)", TempTimeSpan.Minutes + 1);
                }
                timer_RetryDisable.Start();
            }
            else
            {
                Module_Credits.Credit_Refresh_Display();
                Form_Loader._FormReference.cCheckGroupBox_Retry.Text = "Retry Queue";
                Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = true;
            }
        }

        public static BackgroundWorker Retry_BGW;
        private static void RetryBGW_Start(object sender, DoWorkEventArgs e)
        {
            bool CanDoNotes = Module_Credits.Credit_Notes > 0;
            bool CanDoFlag = Module_Credits.Credit_Flag > 0;

            if (CanDoNotes | CanDoFlag)
            {
                foreach (TreeNode TaskNode in Form_Loader._FormReference.cTreeView_RetryQueue.Nodes)
                {
                    string[] JobDescCut = TaskNode.Text.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                    string InferiorID = JobDescCut[1];
                    InferiorID = InferiorID.Substring(0, InferiorID.IndexOf(" "));
                    string UploadedAs = JobDescCut[2];

                    if (TaskNode.Text.StartsWith("Copy Notes", StringComparison.OrdinalIgnoreCase))
                    {
                        if (CanDoNotes)
                        {
                            RetryTask_CopyNotes(InferiorID, UploadedAs, (double)TaskNode.Tag);
                            return;
                        }
                    }
                    else
                    {
                        if (CanDoFlag)
                        {
                            RetryTask_FlagInferior(InferiorID, UploadedAs);
                            return;
                        }
                    }
                }
            }
            else
            {
                RetryDisable("Flag and Note limit reached, please wait for limits to reset.");
                timer_RetryDisable.Start();
            }
        }

        private static void RetryBGW_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            Report_Status("Waiting...");
            Module_Credits.Credit_Refresh_Display();
            if (Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Count > 0)
            {
                timer_Retry.Start();
            }
        }

        private static void RetryTask_CopyNotes(string InferiorID, string UploadedAs, double NoteRatioData)
        {
            JArray OldPostNoteList = JArray.Parse(Module_e621Info.e621InfoDownload($"https://e621.net/notes.json?search[post_id]={InferiorID}"));
            foreach (JObject Note in OldPostNoteList.Reverse())
            {
                if (!Note["is_active"].Value<bool>())
                {
                    OldPostNoteList.Remove(Note);
                }
            }
            Thread.Sleep(500);

            List<string> ExistingNotes = new List<string>();
            string NoteTest = Module_e621Info.e621InfoDownload($"https://e621.net/notes.json?search[post_id]={UploadedAs}");
            if (!NoteTest.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                JArray NewPostNoteList = JArray.Parse(NoteTest);
                foreach (JObject Note in NewPostNoteList.Reverse())
                {
                    if (Note["is_active"].Value<bool>())
                    {
                        ExistingNotes.Add(Note["body"].Value<string>());
                    }
                }
                NewPostNoteList = null;
            }
            Thread.Sleep(500);

            for (int i = 0; i < OldPostNoteList.Count; i++)
            {
                Report_Status(string.Format("Copying Notes: {0}/{1}", i + 1, OldPostNoteList.Count));

                if (ExistingNotes.Contains(OldPostNoteList[i]["body"].Value<string>()))
                {
                    ExistingNotes.Remove(OldPostNoteList[i]["body"].Value<string>());
                    continue;
                }

                Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
                {
                    { "note[post_id]", UploadedAs },
                    { "note[x]", (OldPostNoteList[i]["x"].Value<int>() * NoteRatioData).ToString() },
                    { "note[y]", (OldPostNoteList[i]["y"].Value<int>() * NoteRatioData).ToString() },
                    { "note[width]", (OldPostNoteList[i]["width"].Value<int>() * NoteRatioData).ToString() },
                    { "note[height]", (OldPostNoteList[i]["height"].Value<int>() * NoteRatioData).ToString() },
                    { "note[body]", OldPostNoteList[i]["body"].Value<string>() },
                    { "login", Properties.Settings.Default.UserName },
                    { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
                };

                KeyValuePair<HttpWebResponse, string> e6NoteResponse = Module_Uploader.SEND_Request("POST", "https://e621.net/notes.json", POST_Dictionary);
                if (e6NoteResponse.Key.StatusCode == HttpStatusCode.OK)
                {
                    Module_Credits.Credit_Notes -= 1;
                    Module_Credits.Timestamps_Notes.Add(DateTime.UtcNow.AddHours(1d));
                    if (Module_Credits.Credit_Notes == 0)
                    {
                        timer_RetryDisable.Interval = 60000;
                        timer_RetryDisable.Start();
                        Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = false; }));
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Error Code {0}\n{1}", e6NoteResponse.Key.StatusCode, e6NoteResponse.Value), "Copy Notes - Retry");
                }
                e6NoteResponse.Key.Dispose();
                Thread.Sleep(500);
            }
            Module_Uploader.Report_Info(string.Format("Copied notes from #{0} to #{1}", InferiorID, UploadedAs));
        }

        private static void RetryTask_FlagInferior(string InferiorID, string UploadedAs)
        {
            Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
            {
                { "post_flag[post_id]", InferiorID }, // Inferior image
                { "post_flag[reason_name]", "inferior" },
                { "post_flag[parent_id]", UploadedAs }, //Superior image
                { "login", Properties.Settings.Default.UserName },
                { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
            };

            KeyValuePair<HttpWebResponse, string> e6FlagResponse = SEND_Request("POST", "https://e621.net/post_flags.json", POST_Dictionary);
            if (e6FlagResponse.Key.StatusCode == HttpStatusCode.Created)
            {
                Module_Credits.Credit_Flag -= 1;
                Module_Credits.Timestamps_Flag.Add(DateTime.UtcNow.AddHours(1d));
                Report_Status(string.Format("Flagged #{0} as inferior of #{1}", InferiorID, UploadedAs));
                if (Module_Credits.Credit_Flag == 0)
                {
                    timer_RetryDisable.Interval = 60000;
                    timer_RetryDisable.Start();
                    Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = false; }));
                }
            }
            else
            {
                MessageBox.Show(string.Format("Error Code {0}\n{1}", e6FlagResponse.Key.StatusCode, e6FlagResponse.Value), "Flag Inferior - Retry");
            }
            e6FlagResponse.Key.Dispose();
        }
    }
}
