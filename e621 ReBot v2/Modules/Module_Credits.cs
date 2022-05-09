using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace e621_ReBot_v2.Modules
{
    public class Module_Credits
    {
        public static int Credit_Upload_Hourly = 30;
        public static int Credit_Upload_Total;
        public static int Credit_Flag = 10;
        public static int Credit_Notes = 50;

        public static List<DateTime> Timestamps_Upload = new List<DateTime>();
        public static List<DateTime> Timestamps_Flag = new List<DateTime>();
        public static List<DateTime> Timestamps_Notes = new List<DateTime>();

        public readonly static Dictionary<string, int> UserLevels = new Dictionary<string, int>() { { "Member", 0 }, { "Privileged", 1 }, { "Contributor", 2 }, { "Janitor", 3 }, { "Admin", 4 } };

        private static void Credit_Reset()
        {
            Credit_Upload_Hourly = 30;
            Credit_Notes = 50;
            Credit_Flag = 10;
        }

        public static void Credit_Refresh_Display()
        {
            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                int UserLevelInt = UserLevels[Properties.Settings.Default.UserLevel];
                if (UserLevelInt > 1)
                {
                    Form_Loader._FormReference.label_Credit_Upload.Text = "Infinity";
                }
                else
                {
                    if (Credit_Upload_Total == 0)
                    {
                        Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = false;
                        Timestamps_Upload.Clear();
                        Timestamps_Upload.Add(DateTime.UtcNow.AddHours(1));
                        Module_Uploader.timer_UploadDisable.Interval = 3600000;
                        Module_Uploader.timer_UploadDisable.Tag = "Total";
                        Module_Uploader.timer_UploadDisable.Start();
                        return;
                    }
                    if (Credit_Upload_Hourly == 0)
                    {
                        Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = false;
                        Module_Uploader.timer_UploadDisable.Interval = 1000;
                        Module_Uploader.timer_UploadDisable.Tag = "Hourly";
                        Module_Uploader.timer_UploadDisable.Start();
                    }
                    Form_Loader._FormReference.label_Credit_Upload.Text = $"{Credit_Upload_Hourly}/{Credit_Upload_Total}";
                }
                Form_Loader._FormReference.label_Credit_Flag.Text = UserLevelInt == 0 ? Credit_Flag.ToString() : "Inf.";
                Form_Loader._FormReference.label_Credit_Note.Text = UserLevelInt == 0 ? Credit_Notes.ToString() : "Inf.";
            }));
        }

        public static void Check_Credit_All()
        {
            Credit_Reset();
            Check_Credit_Upload();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserLevel))
            {
                if (UserLevels[Properties.Settings.Default.UserLevel] == 0)
                {
                    Check_Credit_Flag();
                    Check_Credit_Notes();
                }
                if (UserLevels[Properties.Settings.Default.UserLevel] > 1)
                {
                    Form_Loader._FormReference.Invoke(new Action(() => { Form_Loader._FormReference.label_Credit_Upload.Text = "Infinity"; }));
                }
                Credit_Refresh_Display();
            }
            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                Form_Loader._FormReference.bU_RefreshCredit.Enabled = true;
                if (Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Count > 0)
                {
                    Module_Retry.timer_Retry.Start(); //has to be created on main thread.
                } 
            }));
        }

        private static void Check_Credit_Upload()
        {
            Timestamps_Upload.Clear();

            string HTML_UserInfo = Module_e621Info.e621InfoDownload($"https://e621.net/users/{Properties.Settings.Default.UserID}.json");
            if (HTML_UserInfo != null && HTML_UserInfo.Length > 24)
            {
                JObject UserJObject = JObject.Parse(HTML_UserInfo);

                Properties.Settings.Default.UserName = UserJObject["name"].Value<string>();
                Properties.Settings.Default.AppName = $"e621 ReBot ({UserJObject["name"].Value<string>()})";
                Properties.Settings.Default.UserLevel = UserJObject["level_string"].Value<string>(); ;
                Properties.Settings.Default.Save();

                Form_Loader._FormReference.BeginInvoke(new Action(() =>
                {
                    Form_Loader._FormReference.AppName_Label.Text = $"e621 ReBot ({Properties.Settings.Default.UserName})";
                    Form_Loader._FormReference.label_UserLevel.Text = Properties.Settings.Default.UserLevel;
                    Form_Loader._FormReference.label_UserLevel.Visible = true;
                }));

                if (UserLevels[Properties.Settings.Default.UserLevel] < 2)
                {
                    Credit_Upload_Total = UserJObject["upload_limit"].Value<int>();

                    string HTML_UploadHistory = Module_e621Info.e621InfoDownload($"https://e621.net/posts.json?limit=30&tags=user:{Properties.Settings.Default.UserName}");
                    if (HTML_UploadHistory != null && HTML_UploadHistory.Length > 24)
                    {
                        JObject PostHistory = JObject.Parse(HTML_UploadHistory);
                        foreach (JObject UploadedPost in PostHistory["posts"])
                        {
                            DateTime TempTime = UploadedPost["created_at"].Value<DateTime>().ToUniversalTime().AddHours(1);
                            if (DateTime.UtcNow > TempTime)
                            {
                                break;
                            }
                            else
                            {
                                Timestamps_Upload.Add(TempTime);
                                Credit_Upload_Hourly -= 1;
                            }
                        }
                    }
                    HTML_UploadHistory = Module_e621Info.e621InfoDownload($"https://e621.net/post_replacements.json?search[creator_id]={Properties.Settings.Default.UserID}");
                    if (HTML_UploadHistory != null && HTML_UploadHistory.Length > 24)
                    {
                        JArray PostHistory = JArray.Parse(HTML_UploadHistory);
                        foreach (JObject UploadedPost in PostHistory)
                        {
                            DateTime TempTime = UploadedPost["created_at"].Value<DateTime>().ToUniversalTime().AddHours(1);
                            if (DateTime.UtcNow > TempTime)
                            {
                                break;
                            }
                            else
                            {
                                Timestamps_Upload.Add(TempTime);
                                Credit_Upload_Hourly -= 1;
                            }
                        }
                    }
                    Timestamps_Upload.Sort();
                }
            }
        }

        private static void Check_Credit_Flag()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
            {
                Timestamps_Flag.Clear();

                string HTML_FlagHistory = Module_e621Info.e621InfoDownload($"https://e621.net/post_flags.json?search[creator_id]={Properties.Settings.Default.UserID}", true);
                if (HTML_FlagHistory != null && HTML_FlagHistory.Length > 24)
                {
                    JArray FlagHistory = JArray.Parse(HTML_FlagHistory);
                    for (int x = 0; x <= 10; x++)
                    {
                        DateTime TempTime = FlagHistory[x]["created_at"].Value<DateTime>().ToUniversalTime().AddHours(1);
                        if (DateTime.UtcNow > TempTime)
                        {
                            break;
                        }
                        else
                        {
                            Timestamps_Flag.Add(TempTime);
                            Credit_Flag -= 1;
                        }
                    }
                    Timestamps_Flag.Sort();
                }
            }
        }

        private static void Check_Credit_Notes()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
            {
                Timestamps_Notes.Clear();

                string HTML_NoteHistory = Module_e621Info.e621InfoDownload($"https://e621.net/note_versions.json?search[updater_id]={Properties.Settings.Default.UserID}");
                if (HTML_NoteHistory != null && HTML_NoteHistory.Length > 24)
                {
                    JArray NoteHistory = JArray.Parse(HTML_NoteHistory);
                    for (int x = 0; x <= 50; x++)
                    {
                        DateTime TempTime = NoteHistory[x]["created_at"].Value<DateTime>().ToUniversalTime().AddHours(1);
                        if (DateTime.UtcNow > TempTime)
                            break;
                        else
                        {
                            Timestamps_Notes.Add(TempTime);
                            Credit_Notes -= 1;
                        }
                    }
                    Timestamps_Notes.Sort();
                }
            }
        }
    }
}