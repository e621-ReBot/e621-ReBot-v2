using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.Modules
{
    public static class Module_Uploader
    {
        static Module_Uploader()
        {
            timer_Upload = new Timer
            {
                Interval = Properties.Settings.Default.DelayUploader
            };
            timer_Upload.Tick += Timer_Upload_Tick;
            timer_UploadDisable = new Timer
            {
                Interval = 1000
            };
            timer_UploadDisable.Tick += Timer_UploadDisable_Tick;
            Upload_BGW = new BackgroundWorker();
            Upload_BGW.DoWork += UploadBGW_Start;
            Upload_BGW.RunWorkerCompleted += UploadBGW_Done;
        }

        public static bool Media2BigCheck(ref DataRow DataRowPass)
        {
            //https://e621.net/wiki_pages/howto:sites_and_sources
            if (DataRowPass["Info_MediaByteLength"] != DBNull.Value)
            {
                string FileType = (string)DataRowPass["Info_MediaFormat"];
                int BytesLength = (int)DataRowPass["Info_MediaByteLength"];
                switch (FileType)
                {
                    case "gif":
                        {
                            if (BytesLength > 20971520)
                            {
                                DataRowPass["Info_TooBig"] = true;
                                return true; // 20 * 1024 * 1024 = 20MB Limit
                            }
                            break;
                        }

                    case "jpg":
                    case "jpeg":
                    case "png":
                    case "webp":
                        {
                            if (BytesLength > 104857600)
                            {
                                DataRowPass["Info_TooBig"] = true;
                                return true; // 100 * 1024 * 1024 = 100MB Limit
                            }
                            break;
                        }
                }
            }

            if (DataRowPass["Info_MediaWidth"] != DBNull.Value)
            {
                int MediaWidth = (int)DataRowPass["Info_MediaWidth"];
                int MediaHeight = (int)DataRowPass["Info_MediaHeight"];
                int BiggerR = Math.Max(MediaWidth, MediaHeight);
                if (BiggerR > 15000)
                {
                    DataRowPass["Info_TooBig"] = true;
                    return true;
                }
            }

            return false;
        }

        public static bool Media2Big4User(ref DataRow DataRowPass, bool ShowMsgBox = true)
        {
            if ((bool)DataRowPass["Info_TooBig"])
            {
                if (ShowMsgBox)
                {
                    MessageBox.Show("File is too big too be uploaded.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void UploadBtnClicked(bool CurrentPageOnly = false)
        {
            int StartIndex = CurrentPageOnly ? Form_Loader._FormReference.GridIndexTracker : 0;
            int EndIndex = (CurrentPageOnly ? Form_Loader._FormReference.GridIndexTracker + Form_Loader._FormReference.flowLayoutPanel_Grid.Controls.Count : Module_TableHolder.Database_Table.Rows.Count) - 1;

            for (int i = StartIndex; i <= EndIndex; i++)
            {
                if ((bool)Module_TableHolder.Database_Table.Rows[i]["UPDL_Queued"] && Module_TableHolder.Database_Table.Rows[i]["Uploaded_As"] == DBNull.Value && ((string)Module_TableHolder.Database_Table.Rows[i]["Upload_Tags"]).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length < 8)
                {
                    if (MessageBox.Show("There are images with insufficient number of tags selected for upload. Are you sure you want to proceed?", "e621 ReBot", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    {
                        return;
                    }
                    break;
                }
            }

            Form_Loader._FormReference.cTreeView_UploadQueue.BeginUpdate();
            bool TooBigCheck;
            bool ToUpload;
            bool AlreadyUploaded;
            DataTable UploadTableTemp = new DataTable();
            Module_TableHolder.Create_UploadTable(ref UploadTableTemp);

            for (int i = StartIndex; i <= EndIndex; i++)
            {
                ToUpload = (bool)Module_TableHolder.Database_Table.Rows[i]["UPDL_Queued"];
                AlreadyUploaded = Module_TableHolder.Database_Table.Rows[i]["Uploaded_As"] != DBNull.Value;
                TooBigCheck = Module_TableHolder.Database_Table.Rows[i]["Info_TooBig"] != DBNull.Value && (bool)Module_TableHolder.Database_Table.Rows[i]["Info_TooBig"];
                if (ToUpload && !AlreadyUploaded && !TooBigCheck && !Module_TableHolder.UploadQueueContainsURL((string)Module_TableHolder.Database_Table.Rows[i]["Grab_MediaURL"]))
                {
                    DataRow UploadRowTemp = UploadTableTemp.NewRow();
                    UploadRowTemp["Grab_MediaURL"] = (string)Module_TableHolder.Database_Table.Rows[i]["Grab_MediaURL"];
                    UploadRowTemp["DataRowRef"] = Module_TableHolder.Database_Table.Rows[i];
                    UploadRowTemp["CopyNotes"] = Module_TableHolder.Database_Table.Rows[i]["Inferior_HasNotes"] != DBNull.Value;
                    //UploadRowTemp["MoveChildren"] = Module_TableHolder.Database_Table.Rows[i]["Inferior_Children"] != DBNull.Value;
                    if (Module_TableHolder.Database_Table.Rows[i]["Inferior_ID"] != DBNull.Value)
                    {
                        //if (Properties.Settings.Default.DontFlag)
                        //{
                        //    UploadRowTemp["ChangeParent"] = true;
                        //}
                        //else
                        //{
                        //    UploadRowTemp["FlagInferior"] = true;
                        //}
                        UploadRowTemp["ReplaceInferior"] = true;
                    }
                    else
                    {
                        UploadRowTemp["Upload"] = true;
                    }
                    CreateUploadJobNode(ref UploadRowTemp);
                    UploadTableTemp.Rows.Add(UploadRowTemp);
                }
            }
            lock (Module_TableHolder.Upload_Table)
            {
                Module_TableHolder.Upload_Table.Merge(UploadTableTemp);
            }
            Form_Loader._FormReference.cTreeView_UploadQueue.EndUpdate();

            int NodeCount = Form_Loader._FormReference.cTreeView_UploadQueue.Nodes.Count;
            Form_Loader._FormReference.cCheckGroupBox_Upload.Text = $"Uploader{(NodeCount > 0 ? $" ({NodeCount})" : null)}";

            if (Form_Loader._FormReference.cCheckGroupBox_Upload.Checked && !Upload_BGW.IsBusy)
            {
                timer_Upload.Start();
            }
        }

        private static void CreateUploadJobNode(ref DataRow DataRowPass)
        {
            TreeNode TreeNodeParent = new TreeNode((string)DataRowPass["Grab_MediaURL"])
            {
                Tag = DataRowPass,
                ContextMenuStrip = Form_Loader._FormReference.contextMenuStrip_cTreeView
            };
            for (int c = 2; c < DataRowPass.ItemArray.Length; c++)
            {
                if (DataRowPass.ItemArray[c].GetType() == typeof(bool) && (bool)DataRowPass.ItemArray[c])
                {
                    TreeNodeParent.Nodes.Add(Module_TableHolder.Upload_Table.Columns[c].Caption);
                }
            }
            //TreeNodeParent.Expand();
            Form_Loader._FormReference.cTreeView_UploadQueue.Nodes.Add(TreeNodeParent);
        }

        public static void ReverseUploadNodes()
        {
            Form_Loader._FormReference.cTreeView_UploadQueue.Nodes.Clear();
            foreach (DataRow DataRowTemp in Module_TableHolder.Upload_Table.Rows)
            {
                DataRow UploadRowTemp = DataRowTemp;
                CreateUploadJobNode(ref UploadRowTemp);
            }
        }


        public static void Report_Info(string InfoMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.textBox_Info.Text = $"{DateTime.Now.ToLongTimeString()}, Uploader >>> {InfoMessage}\n{Form_Loader._FormReference.textBox_Info.Text}";
            }
            ));
        }

        public static void Report_Status(string StatusMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() => { Form_Loader._FormReference.label_UploadStatus.Text = string.Format("Status: {0}", StatusMessage); }));
        }

        public static KeyValuePair<HttpWebResponse, string> SEND_Request(string SendMethod, string URL, Dictionary<string, string> SEND_Dictionary, in byte[] bytes2Send = null)
        {
            string MPF_Boundary = "--------" + DateTime.Now.Ticks.ToString("x");
            MultipartFormDataContent EncodedContent = new MultipartFormDataContent(MPF_Boundary);
            foreach (KeyValuePair<string, string> DataPair in SEND_Dictionary)
            {
                if (DataPair.Key.Equals("upload[file]"))
                {
                    EncodedContent.Add(new ByteArrayContent(bytes2Send), "upload[file]", DataPair.Value);
                }
                else
                {
                    EncodedContent.Add(new StringContent(DataPair.Value), DataPair.Key);
                }
            }
            //string PostText = EncodedContent.ReadAsStringAsync().Result.ToString();

            using (Stream EncodedContentStream = EncodedContent.ReadAsStreamAsync().Result)
            {
                HttpWebRequest e6Uploader = (HttpWebRequest)WebRequest.Create(URL);
                e6Uploader.UserAgent = Properties.Settings.Default.AppName;
                e6Uploader.ContentType = "multipart/form-data; boundary=" + MPF_Boundary;
                e6Uploader.ContentLength = EncodedContentStream.Length;
                e6Uploader.Method = SendMethod;
                if (bytes2Send == null)
                {
                    e6Uploader.Timeout = 33333; // timeout is for request start until response end
                    EncodedContentStream.CopyTo(e6Uploader.GetRequestStream());
                }
                else
                {
                    Report_Status("Uploading Media...0%");
                    e6Uploader.AllowWriteStreamBuffering = false;
                    e6Uploader.SendChunked = true;
                    e6Uploader.Timeout = 99999; // timeout is for request start until response end    
                    int SentBytesCount = 0;
                    using (Stream UploadStream = e6Uploader.GetRequestStream())
                    {
                        byte[] UploadBuffer = new byte[65536]; // 64 kB buffer
                        int ReadBytes;
                        using (Stream InputStream = EncodedContentStream)
                        {
                            while (SentBytesCount < EncodedContentStream.Length)
                            {
                                ReadBytes = InputStream.Read(UploadBuffer, 0, UploadBuffer.Length);
                                UploadStream.Write(UploadBuffer, 0, ReadBytes);
                                double ReportPercentage = SentBytesCount / (double)EncodedContentStream.Length;
                                Report_Status(string.Format("Uploading Media...{0}", ReportPercentage.ToString("P0")));
                                SentBytesCount += ReadBytes;
                            }
                        }
                    }
                    Report_Status("Uploading Media...100%");
                }

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



        public static Timer timer_Upload;
        private static void Timer_Upload_Tick(object sender, EventArgs e)
        {
            timer_Upload.Stop();
            if (Form_Loader._FormReference.cCheckGroupBox_Upload.Checked && Form_Loader._FormReference.cTreeView_UploadQueue.Nodes.Count > 0 && !Upload_BGW.IsBusy)
            {
                Upload_BGW.RunWorkerAsync();
            }
        }

        public static Timer timer_UploadDisable;
        private static void Timer_UploadDisable_Tick(object sender, EventArgs e)
        {
            timer_UploadDisable.Stop();
            if (timer_UploadDisable.Tag.ToString().Equals("Total"))
            {
                new Thread(Module_Credits.Check_Credit_All).Start();
            }
            else
            {
                List<DateTime> TimecopyList = Module_Credits.Timestamps_Upload;
                for (int i = 0; i < Module_Credits.Timestamps_Upload.Count; i++)
                {
                    if (Module_Credits.Timestamps_Upload[0] < DateTime.UtcNow)
                    {
                        TimecopyList.RemoveAt(0);
                        Module_Credits.Credit_Upload_Hourly += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                Module_Credits.Timestamps_Upload = TimecopyList;
                Module_Credits.Credit_Refresh_Display();
                if (Module_Credits.Credit_Upload_Hourly > 0)
                {
                    Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = true;
                }
                else
                {
                    timer_UploadDisable.Stop(); // Timer Interval doesn't update otherwise.
                    TimeSpan TempTimeSpan = Module_Credits.Timestamps_Upload[0] - DateTime.UtcNow;
                    if (TempTimeSpan.TotalSeconds < 60d)
                    {
                        timer_UploadDisable.Interval = 1000; // second
                        Form_Loader._FormReference.cCheckGroupBox_Upload.Text = string.Format("Upload Queue (available again in {0} seconds)", TempTimeSpan.Seconds);
                    }
                    else
                    {
                        timer_UploadDisable.Interval = 60000; // minute
                        Form_Loader._FormReference.cCheckGroupBox_Upload.Text = string.Format("Upload Queue (available again in {0} minutes)", TempTimeSpan.Minutes + 1);
                    }
                    timer_UploadDisable.Start();
                }
            }
        }



        public static BackgroundWorker Upload_BGW;
        private static DataRow TaskRow;
        private static void UploadBGW_Start(object sender, DoWorkEventArgs e)
        {
            TaskRow = Module_TableHolder.Upload_Table.Rows[0];
            for (int c = 2; c <= TaskRow.ItemArray.Length; c++)
            {
                if (TaskRow[c].GetType() == typeof(bool) && (bool)TaskRow[c])
                {
                    typeof(Module_Uploader).GetMethod("UploadTask_" + Module_TableHolder.Upload_Table.Columns[c].ColumnName).Invoke(null, null); //needs public methods
                    return;
                }
            }
        }

        private static bool FailedUploadTask;
        private static void UploadBGW_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            Report_Status("Waiting...");
            Module_Credits.Credit_Refresh_Display();

            if (FailedUploadTask)
            {
                FailedUploadTask = false;
                return;
            }

            TreeView UploadTreeview = Form_Loader._FormReference.cTreeView_UploadQueue;

            for (int c = 2; c <= TaskRow.ItemArray.Length; c++)
            {
                if (TaskRow[c].GetType() == typeof(bool) && (bool)TaskRow[c])
                {
                    lock (Module_TableHolder.Upload_Table)
                    {
                        TaskRow[c] = false;
                    }
                    UploadTreeview.Nodes[0].Nodes[0].Remove();
                    break;
                }
            }

            if (UploadTreeview.Nodes[0].Nodes.Count == 0)
            {
                UploadTreeview.Nodes[0].Remove();
                lock (Module_TableHolder.Upload_Table)
                {
                    Module_TableHolder.Upload_Table.Rows.Remove(TaskRow);
                }
            }

            int NodeCount = UploadTreeview.Nodes.Count;
            Form_Loader._FormReference.cCheckGroupBox_Upload.Text = $"Uploader{(NodeCount > 0 ? $" ({NodeCount})" : null)}";

            if (NodeCount > 0)
            {
                // To finish notes/flags even when there's no upload credit left, instead of waiting for upload credit
                if (timer_UploadDisable.Enabled)
                {
                    if (!UploadTreeview.Nodes[0].Nodes[0].Text.Equals("Upload"))
                    {
                        Upload_BGW.RunWorkerAsync();
                    }
                }
                else
                {
                    timer_Upload.Start();
                }
            }
        }

        public static void UploadTask_Upload()
        {
            Report_Status("Uploading...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            string Upload_Sources = (string)DataRowRef["Grab_URL"];
            if (DataRowRef["Inferior_Sources"] != DBNull.Value)
            {
                foreach (string InferiorSource in (List<string>)DataRowRef["Inferior_Sources"])
                {
                    if (!Upload_Sources.Contains(InferiorSource))
                    {
                        Upload_Sources += "%0A" + InferiorSource;
                    }
                }
            }

            string Upload_Description = null;
            string Upload_DescriptionNoExtras = null;
            if (DataRowRef["Grab_TextBody"] == DBNull.Value)
            {
                Upload_Description = $"[code]{(string)DataRowRef["Grab_Title"]}[/code]";
            }
            else
            {
                Upload_Description = string.Format("[section{0}={1}]\n{2}\n[/section]", Properties.Settings.Default.ExpandedDescription ? ",expanded" : null, (string)DataRowRef["Grab_Title"], (string)DataRowRef["Grab_TextBody"]);
            }
            Upload_DescriptionNoExtras = Upload_Description;

            if (DataRowRef["Inferior_ID"] != DBNull.Value)
            {
                Upload_Description = $"Superior version of post #{DataRowRef["Inferior_ID"]}\n\n{Upload_Description}";
            }

            // FA link fix
            string Upload_MediaURL = (string)DataRowRef["Grab_MediaURL"];
            if (Upload_MediaURL.Contains("d.facdn.net/art"))
            {
                Upload_MediaURL = Upload_MediaURL.Replace("d.facdn.net/art", "d.facdn.net/download/art");
            }

            Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>();

            bool isByteUpload = false;
            string UploadedURLReport = Upload_MediaURL;

            string MediaFormat = (string)DataRowRef["Info_MediaFormat"];
            switch (MediaFormat)
            {
                case "ugoira":
                    {
                        POST_Dictionary.Add("upload[file]", Upload_MediaURL);
                        Upload_MediaURL = Upload_MediaURL.Substring(Upload_MediaURL.LastIndexOf("/") + 1);
                        UploadedURLReport = $"{Upload_MediaURL.Substring(0, Upload_MediaURL.IndexOf("_ugoira0."))}_ugoira1920x1080.webm, converted from {(string)DataRowRef["Grab_URL"]}";
                        Upload_Description += "\nConverted from Ugoira using FFmpeg: -c:v libvpx-vp9 -pix_fmt yuv420p -lossless 1 -an";
                        isByteUpload = true;
                        break;
                    }

                case "mp4":
                case "swf":
                    {
                        POST_Dictionary.Add("upload[file]", Upload_MediaURL);
                        string VideoFileName = Upload_MediaURL.Remove(Upload_MediaURL.Length - 4);
                        VideoFileName = $"{VideoFileName.Substring(VideoFileName.LastIndexOf("/") + 1)}.webm";
                        UploadedURLReport = $"{VideoFileName}, converted from {(string)DataRowRef["Grab_URL"]}";
                        Upload_Description += "\nConverted using FFmpeg: -c:v libvpx-vp9 -pix_fmt yuv420p -b:a 192k -b:v 2000k -minrate 1500k -maxrate 2500k -crf 16 -quality good -speed 4 -pass 2";
                        isByteUpload = true;
                        break;
                    }

                 default:
                    {
                        string EscapedURL = new Uri(Upload_MediaURL).AbsoluteUri;
                        if (EscapedURL.Contains("https://img.pawoo.net/media_attachments/") || EscapedURL.Contains("https://www.hiccears.com/file/"))
                        {
                            POST_Dictionary.Add("upload[file]", Upload_MediaURL);
                            isByteUpload = true;
                        }
                        else
                        {
                            POST_Dictionary.Add("upload[direct_url]", EscapedURL);
                        }
                        break;
                    }
            }

            byte[] bytes2Send = null;
            if (isByteUpload)
            {
                if (DataRowRef["DL_FilePath"] != DBNull.Value && File.Exists((string)DataRowRef["DL_FilePath"]))
                {
                    string CachedFileName = (string)DataRowRef["DL_FilePath"];
                    CachedFileName = CachedFileName.Substring(CachedFileName.LastIndexOf(@"\") + 1);
                    POST_Dictionary["upload[file]"] = CachedFileName;
                    bytes2Send = File.ReadAllBytes((string)DataRowRef["DL_FilePath"]);
                }
                else
                {
                    string FileName;
                    string ExtraSourceURL;
                    switch (MediaFormat)
                    {
                        case "ugoira":
                            {
                                Module_FFmpeg.UploadQueue_Ugoira2WebM(ref DataRowRef, out bytes2Send, out FileName, out ExtraSourceURL);
                                break;
                            }

                        case "mp4":
                        case "swf":
                            {
                                Module_FFmpeg.UploadQueue_Videos2WebM(ref DataRowRef, out bytes2Send, out FileName, out ExtraSourceURL);
                                break;
                            }

                        default:
                            {
                                Directory.CreateDirectory("MediaTemp").Attributes = FileAttributes.Hidden;
                                Module_Downloader.FileDownloader(Upload_MediaURL, "U", "MediaTemp", null, in DataRowRef);
                                FileName = Module_Downloader.GetMediasFileNameOnly(Upload_MediaURL);
                                ExtraSourceURL = Upload_MediaURL;
                                bytes2Send = File.ReadAllBytes($"MediaTemp\\{FileName}");
                                break;
                            }
                    }
                    POST_Dictionary["upload[file]"] = FileName;
                    Upload_Sources = ExtraSourceURL + "%0A" + Upload_Sources;
                }
            }

            if (DataRowRef["Inferior_Description"] != DBNull.Value)
            {
                string Inferior_Description = (string)DataRowRef["Inferior_Description"];
                if (!(Inferior_Description.Equals(Upload_DescriptionNoExtras) || Inferior_Description.Equals((string)DataRowRef["Grab_TextBody"])))
                {
                    Upload_Description += $"\n - - - - - \n{Inferior_Description}";
                }
            }

            string ParentTag = null;
            string PostTags = (string)DataRowRef["Upload_Tags"];
            if (DataRowRef["Upload_ParentOffset"] != DBNull.Value && ((DataRow)DataRowRef["Upload_ParentOffset"])["Uploaded_As"] != DBNull.Value)
            {
                if (PostTags.Contains("parent:"))
                {
                    string ParentTag2Remove = PostTags.Substring(PostTags.IndexOf("parent:"));
                    ParentTag2Remove = ParentTag2Remove.Substring(0, ParentTag2Remove.IndexOf(" "));
                    PostTags = PostTags.Replace(ParentTag2Remove, "");
                }
                ParentTag = " parent:" + (string)((DataRow)DataRowRef["Upload_ParentOffset"])["Uploaded_As"];
            }

            if (!Module_APIControler.APIEnabled)
            {
                return;
            }

            POST_Dictionary.Add("upload[source]", Upload_Sources);
            POST_Dictionary.Add("upload[rating]", ((string)DataRowRef["Upload_Rating"]).ToLower());
            POST_Dictionary.Add("upload[tag_string]", PostTags + ParentTag);
            POST_Dictionary.Add("upload[description]", Upload_Description);
            POST_Dictionary.Add("login", Properties.Settings.Default.UserName);
            POST_Dictionary.Add("api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key));

            KeyValuePair<HttpWebResponse, string> e6Response = SEND_Request("POST", "https://e621.net/uploads.json", POST_Dictionary, in bytes2Send);
            switch (e6Response.Key.StatusCode)
            {
                case HttpStatusCode.OK:
                    {
                        string Response = e6Response.Value;
                        JObject Upload_Reponse_Data = JObject.Parse(Response);
                        if (Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] < 2)
                        {
                            Module_Credits.Credit_Upload_Hourly -= 1;
                            Module_Credits.Timestamps_Upload.Add(DateTime.UtcNow.AddHours(1d));
                            Module_Credits.Credit_Upload_Total -= 1;
                        }
                        DisplayUploadSuccess(ref DataRowRef, Upload_Reponse_Data["post_id"].Value<string>());
                        Report_Info($"Uploaded: {UploadedURLReport}");
                        break;
                    }

                case HttpStatusCode.PreconditionFailed:
                    {
                        string Response = e6Response.Value;
                        JObject Upload_Reponse_Data = JObject.Parse(Response);
                        if (Upload_Reponse_Data["reason"].Value<string>().Equals("duplicate"))
                        {
                            DisplayUploadSuccess(ref DataRowRef, Upload_Reponse_Data["post_id"].Value<string>());
                            Report_Info($"Error uploading: {UploadedURLReport}, duplicate of #{Upload_Reponse_Data["post_id"].Value<string>()}");
                        }
                        else
                        {
                            FailedUploadTask = true;
                            Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = false;
                            MessageBox.Show($"Some other Error {e6Response.Key.StatusCode}\n{e6Response.Value}", "e621 ReBot - Upload");
                        }
                        break;
                    }

                default:
                    {
                        FailedUploadTask = true;
                        Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = false;
                        Report_Info($"Error uploading: {UploadedURLReport}");
                        MessageBox.Show($"Error Code {e6Response.Key.StatusCode}\n{e6Response.Value}", "e621 ReBot - Upload");
                        break;
                    }
            }
            e6Response.Key.Dispose();
            if (Directory.Exists("MediaTemp"))
            {
                Directory.Delete("MediaTemp", true);
            }
            //e6Response = default;
        }

        private static void DisplayUploadSuccess(ref DataRow DataRowRef, string ID, bool Record2DB = true)
        {
            DataRowRef["Uploaded_As"] = ID;
            if (Record2DB) Module_DB.DB_Media_CreateRecord(ref DataRowRef);

            DataRow DataRowTemp = DataRowRef;
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref DataRowTemp);
                if (e6_GridItemTemp == null)
                {
                    Form_Loader._FormReference.UploadCounter -= 1;
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
                    {
                        Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                    }
                    DataRowTemp["UPDL_Queued"] = false;
                }
                else
                {
                    e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                    e6_GridItemTemp.cLabel_isUploaded.Text = (string)DataRowTemp["Uploaded_As"];
                }

                if (Form_Preview._FormReference != null && Form_Preview._FormReference.IsHandleCreated && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, DataRowTemp))
                {
                    Form_Preview._FormReference.PB_Upload.BackColor = Color.FromArgb(0, 45, 90);
                    Form_Preview._FormReference.Label_AlreadyUploaded.Text = $"Already uploaded as #{(string)DataRowTemp["Uploaded_As"]}";
                }
            }));
        }

        public static void UploadTask_ReplaceInferior()
        {
            Report_Status("Flagging for replacement...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
            {
                { "post_replacement[replacement_url]", (string)DataRowRef["Grab_MediaURL"] }, //Superior image
                { "post_replacement[source]", (string)DataRowRef["Grab_URL"] },
                { "post_replacement[reason]", "Superior version" },
                { "login", Properties.Settings.Default.UserName },
                { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
            };

            KeyValuePair<HttpWebResponse, string> e6Flag4ReplaceResponse = SEND_Request("POST", $"https://e621.net/post_replacements.json?post_id={(string)DataRowRef["Inferior_ID"]}", POST_Dictionary);
            switch (e6Flag4ReplaceResponse.Key.StatusCode)
            {
                case HttpStatusCode.Created:
                    {
                        //if (Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] == 0)
                        //{
                        //    Module_Credits.Credit_Flag -= 1;
                        //    Module_Credits.Timestamps_Flag.Add(DateTime.UtcNow.AddHours(1d));
                        //}
                        if (Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] < 2)
                        {
                            Module_Credits.Credit_Upload_Hourly -= 1;
                            Module_Credits.Timestamps_Upload.Add(DateTime.UtcNow.AddHours(1d));
                            Module_Credits.Credit_Upload_Total -= 1;
                        }
                        DisplayUploadSuccess(ref DataRowRef, (string)DataRowRef["Inferior_ID"], false);
                        Report_Info($"Flagged #{(string)DataRowRef["Inferior_ID"]} for replacement as inferior of @{(string)DataRowRef["Grab_MediaURL"]}");
                        break;
                    }

                //case (HttpStatusCode)422:
                //    {
                //        //Report_Info($"Hourly flag limit reached, did not flag #{(string)DataRowRef["Uploaded_As"]}");
                //        //Module_Credits.Credit_Flag = 0;
                //        //UploadTask_ChangeParent();

                //        //Form_Loader._FormReference.Invoke(new Action(() =>
                //        //{
                //        //    TreeNode clonedNode = new TreeNode()
                //        //    {
                //        //        Text = $"Flag #{(string)DataRowRef["Inferior_ID"]} as inferior of #{(string)DataRowRef["Uploaded_As"]}"
                //        //    };
                //        //    Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Add(clonedNode);
                //        //}));
                //        //Module_Retry.timer_RetryDisable.Start();
                //        //Module_Retry.timer_Retry.Start();
                //        break;
                //    }

                default:
                    {
                        MessageBox.Show($"Error Code {e6Flag4ReplaceResponse.Key.StatusCode}\n{e6Flag4ReplaceResponse.Value}", "e621 ReBot - Replace Inferior");
                        break;
                    }
            }
            e6Flag4ReplaceResponse.Key.Dispose();
            //e6Flag4ReplaceResponse = default;
        }

        //

        public static void UploadTask_CopyNotes()
        {
            Report_Status("Copying Notes...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            JArray NoteList = JArray.Parse(Module_e621Info.e621InfoDownload($"https://e621.net/notes.json?search[post_id]={(string)DataRowRef["Inferior_ID"]}"));
            foreach (JObject Note in NoteList.Reverse())
            {
                if (!Note["is_active"].Value<bool>())
                {
                    NoteList.Remove(Note);
                }
            }

            double RatioData = (double)DataRowRef["Inferior_NotesSizeRatio"];
            for (int i = 0; i < NoteList.Count; i++)
            {
                Report_Status($"Copying Notes: {i + 1}/{NoteList.Count}");

                Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
                {
                    { "note[post_id]", (string)DataRowRef["Uploaded_As"] },
                    { "note[x]", (NoteList[i]["x"].Value<int>() * RatioData).ToString() },
                    { "note[y]", (NoteList[i]["y"].Value<int>() * RatioData).ToString() },
                    { "note[width]", (NoteList[i]["width"].Value<int>() * RatioData).ToString() },
                    { "note[height]", (NoteList[i]["height"].Value<int>() * RatioData).ToString() },
                    { "note[body]", NoteList[i]["body"].Value<string>() },
                    { "login", Properties.Settings.Default.UserName },
                    { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
                };

                KeyValuePair<HttpWebResponse, string> e6NoteResponse = SEND_Request("POST", "https://e621.net/notes.json", POST_Dictionary);
                switch (e6NoteResponse.Key.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            if (Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] == 0)
                            {
                                Module_Credits.Credit_Notes -= 1;
                                Module_Credits.Timestamps_Notes.Add(DateTime.UtcNow.AddHours(1d));
                            }
                            break;
                        }

                    case (HttpStatusCode)422:
                        {
                            Report_Info($"Note edit limit reached, copied {(i + 1)} out of {NoteList.Count} notes");
                            Module_Credits.Credit_Notes = 0;

                            Form_Loader._FormReference.Invoke(new Action(() =>
                            {
                                TreeNode clonedNode = new TreeNode()
                                {
                                    Text = $"Copy Notes from #{(string)DataRowRef["Inferior_ID"]} to #{(string)DataRowRef["Uploaded_As"]}",
                                    Tag = RatioData
                                };
                                Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Add(clonedNode);
                            }));
                            Module_Retry.timer_RetryDisable.Start();
                            Module_Retry.timer_Retry.Start();
                            e6NoteResponse.Key.Dispose();
                            return;
                        }

                    default:
                        {
                            MessageBox.Show($"Error Code {e6NoteResponse.Key.StatusCode}\n{e6NoteResponse.Value}", "e621 Rebot - Copy Notes");
                            break;
                        }
                }
                e6NoteResponse.Key.Dispose();
                Thread.Sleep(500);
            }
        }

        public static void UploadTask_MoveChildren()
        {
            Report_Status("Moving Children...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            List<string> ChildrenList = (List<string>)DataRowRef["Inferior_Children"];

            for (int i = 0; i < ChildrenList.Count; i++)
            {
                Report_Status($"Moving Children: {(i + 1)}/{ChildrenList.Count}");

                Dictionary<string, string> POST_Parent = new Dictionary<string, string>()
                {
                    { "post[old_parent_id]", (string)DataRowRef["Inferior_ID"] }, // Child's current/old parent
                    { "post[parent_id]", (string)DataRowRef["Uploaded_As"] }, // Child's new parent / id of new image
                    { "login", Properties.Settings.Default.UserName },
                    { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
                };
                // Child's new parent / id of new image
                string ChangeChildsParentURL = $"https://e621.net/posts/{ChildrenList[i]}.json";
                KeyValuePair<HttpWebResponse, string> e6ChildResponse = SEND_Request("PATCH", ChangeChildsParentURL, POST_Parent);
                if (e6ChildResponse.Key.StatusCode == HttpStatusCode.OK)
                {
                    Report_Info($"Post #{ChildrenList[i]} set as child of #{(string)DataRowRef["Uploaded_As"]}");
                }
                else
                {
                    MessageBox.Show($"Error Code {e6ChildResponse.Key.StatusCode}\n{e6ChildResponse.Value}", "e621 Rebot - Move Children");
                }
                e6ChildResponse.Key.Dispose();
                Thread.Sleep(500);
            }
        }

        public static void UploadTask_FlagInferior()
        {
            Report_Status("Flagging...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
            {
                { "post_flag[post_id]", (string)DataRowRef["Inferior_ID"] }, // Inferior image
                { "post_flag[reason_name]", "inferior" },
                { "post_flag[parent_id]", (string)DataRowRef["Uploaded_As"] }, //Superior image
                { "login", Properties.Settings.Default.UserName },
                { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
            };

            KeyValuePair<HttpWebResponse, string> e6FlagResponse = SEND_Request("POST", "https://e621.net/post_flags.json", POST_Dictionary);
            switch (e6FlagResponse.Key.StatusCode)
            {
                case HttpStatusCode.Created:
                    {
                        if (Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] == 0)
                        {
                            Module_Credits.Credit_Flag -= 1;
                            Module_Credits.Timestamps_Flag.Add(DateTime.UtcNow.AddHours(1d));
                        }

                        Report_Info($"Flagged #{(string)DataRowRef["Inferior_ID"]} as inferior of #{(string)DataRowRef["Uploaded_As"]}");
                        break;
                    }

                case (HttpStatusCode)422:
                    {
                        Report_Info($"Hourly flag limit reached, did not flag #{(string)DataRowRef["Uploaded_As"]}");
                        Module_Credits.Credit_Flag = 0;
                        UploadTask_ChangeParent();

                        Form_Loader._FormReference.Invoke(new Action(() =>
                        {
                            TreeNode clonedNode = new TreeNode()
                            {
                                Text = $"Flag #{(string)DataRowRef["Inferior_ID"]} as inferior of #{(string)DataRowRef["Uploaded_As"]}"
                            };
                            Form_Loader._FormReference.cTreeView_RetryQueue.Nodes.Add(clonedNode);
                        }));
                        Module_Retry.timer_RetryDisable.Start();
                        Module_Retry.timer_Retry.Start();
                        break;
                    }

                default:
                    {
                        MessageBox.Show($"Error Code {e6FlagResponse.Key.StatusCode}\n{e6FlagResponse.Value}", "e621 ReBot - Flag Inferior");
                        break;
                    }
            }
            e6FlagResponse.Key.Dispose();
            e6FlagResponse = default;
        }

        public static void UploadTask_ChangeParent()
        {
            Report_Status("Changing Parent...");
            DataRow DataRowRef = (DataRow)TaskRow["DataRowRef"];

            Dictionary<string, string> POST_Dictionary = new Dictionary<string, string>()
            {
                { "post[parent_id]", (string)DataRowRef["Uploaded_As"] },
                { "post[edit_reason]", "For future flag." },
                { "login", Properties.Settings.Default.UserName },
                { "api_key", Module_Cryptor.Decrypt(Properties.Settings.Default.API_Key) }
            };

            string ChangeParentURL = $"https://e621.net/posts/{(string)DataRowRef["Inferior_ID"]}.json";
            KeyValuePair<HttpWebResponse, string> e6ChangeParentResponse = SEND_Request("PATCH", ChangeParentURL, POST_Dictionary);
            if (e6ChangeParentResponse.Key.StatusCode != HttpStatusCode.OK)
            {
                Report_Info($"Error changing parent of {(string)DataRowRef["Inferior_ID"]} to {(string)DataRowRef["Inferior_ID"]}");
                MessageBox.Show($"Error Code {e6ChangeParentResponse.Key.StatusCode}\n{e6ChangeParentResponse.Value}", "e621 ReBot - Change Parent");
            }
        }
    }
}