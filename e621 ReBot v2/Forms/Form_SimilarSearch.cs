﻿using e621_ReBot_v2.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using e621_ReBot_v2.CustomControls;
using System.Data;

namespace e621_ReBot_v2.Forms
{

    public partial class Form_SimilarSearch : Form
    {
        public Form_SimilarSearch()
        {
            InitializeComponent();
            _FormReference = this;
        }

        public Form_SimilarSearch(string NamePass, Point PointRef) : this()
        {
            Text = NamePass;
            Location = new Point(PointRef.X - 9, PointRef.Y - 2);
        }

        public static Form_SimilarSearch _FormReference;
        private void Form_Similar_Load(object sender, EventArgs e)
        {
            Label_SearchCheck.Text = string.Format("Checking {0} status...", Text);
        }

        private void Form_Similar_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Form_Preview._FormReference != null)
            {
                Form_Preview._FormReference.Activate();
                Form_Preview._FormReference.Focus();
            }
            _FormReference = null;
            Dispose();
        }

        private void Form_Similar_Shown(object sender, EventArgs e)
        {
            new Thread(CheckSiteStatus).Start();
        }

        private void CheckSiteStatus()
        {
            HttpWebRequest SiteRequest;
            if (Text.Equals("SauceNao"))
            {
                SiteRequest = (HttpWebRequest)WebRequest.Create("https://saucenao.com/");
            }
            else
            {
                SiteRequest = (HttpWebRequest)WebRequest.Create("https://e621.net/iqdb_queries");
            }
            SiteRequest.UserAgent = Form_Loader.GlobalUserAgent;
            SiteRequest.Method = "HEAD";
            SiteRequest.Timeout = 5000;
            string LabelText = null;
            Color LabelColor = Color.LightSteelBlue;
            try
            {
                SiteRequest.GetResponse().Close();
                LabelText = Text + " is Online.";
                LabelColor = Color.LimeGreen;
            }
            catch (WebException)
            {
                LabelText = Text + " is Offline.";
                LabelColor = Color.Red;
            }
            finally
            {
                if (_FormReference != null)
                {
                    Invoke(new Action(() =>
                    {
                        Label_SearchCheck.Text = LabelText;
                        Label_SearchCheck.ForeColor = LabelColor;
                        timer_Delay.Start();
                    }));
                }
            }
        }

        private void Timer_Delay_Tick(object sender, EventArgs e)
        {
            if (timer_Delay.Tag == null)
            {
                if (Label_SearchCheck.Text.Contains("Online"))
                {
                    timer_Delay.Tag = "Something";
                    Label_SearchCheck.Text = "Checking for similar images...";
                    Label_SearchCheck.ForeColor = Color.LightSteelBlue;
                }
                else
                {
                    Close();
                }
            }
            else
            {
                timer_Delay.Stop();
                BackgroundWorker TempBGW = new BackgroundWorker();
                if (Text.Equals("SauceNao"))
                {
                    TempBGW.DoWork += CheckSauceNaoImages;
                }
                else
                {
                    if (((string)Form_Preview._FormReference.Preview_RowHolder["Grab_MediaURL"]).Contains("https://img.pawoo.net/media_attachments/"))
                    {
                        MessageBox.Show("e621 doesn't have Pawoo whitelisted yet.", "e621 ReBot");
                        Close();
                    }
                    TempBGW.DoWork += CheckIQDBQImages;
                }
                TempBGW.RunWorkerCompleted += BGWorkDone;
                TempBGW.RunWorkerAsync();
            }
        }

        private bool Move2Center = false;
        private void BGWorkDone(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Move2Center)
            {
                Point OwnerFormPoint = Form_Preview._FormReference.PointToScreen(Point.Empty);
                Location = new Point(OwnerFormPoint.X + Form_Preview._FormReference.Width / 2 - Width / 2, OwnerFormPoint.Y + Form_Preview._FormReference.Height / 2 - Height / 2);
            }
        }

        //Has 100 daily search and 5 per 30s search limit.
        private async void CheckSauceNaoImages(object sender, DoWorkEventArgs e)
        {
            string ResponseString = null;
            using (HttpClient TempClient = new HttpClient())
            {
                TempClient.Timeout = TimeSpan.FromSeconds(30);
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    try
                    {
                        using (HttpResponseMessage HttpResponseMsg = await TempClient.GetAsync($"https://saucenao.com/search.php?db=29&url={(string)Form_Preview._FormReference.Preview_RowHolder["Grab_MediaURL"]}", cts.Token))
                        {
                            if (HttpResponseMsg.IsSuccessStatusCode)
                            {
                                ResponseString = await HttpResponseMsg.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                if (_FormReference != null) //already closed
                                {
                                    Invoke(new Action(() =>
                                    {
                                        MessageBox.Show($"Error at Check SauceNao Images response!\n\nStatus code: {HttpResponseMsg.StatusCode}\n{ResponseString}", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        Close();
                                    }));
                                }
                                return;
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        if (_FormReference != null) //already closed
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show($"SauceNao Search error: {ex.Message}", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Close();
                            }));
                        }
                        return;
                    }
                    catch (TaskCanceledException ex) when (ex.CancellationToken != cts.Token)
                    {
                        if (_FormReference != null) //already closed
                        {
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("SauceNao Search timed out.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Close();
                            }));
                        }
                        return;
                    }
                }        
            }

            HtmlDocument WebDoc = new HtmlDocument();
            WebDoc.LoadHtml(ResponseString);

            HtmlNodeCollection ResultCollection = WebDoc.DocumentNode.SelectNodes("//div[@id='middle']/div[@class='result']");
            if (ResultCollection.Last().InnerText.Contains("Low similarity results have been hidden."))
            {
                ResultCollection.RemoveAt(ResultCollection.Count - 1);
            }
            if (ResultCollection.Count > 0 && ResultCollection.Last().InnerText.Contains("No results found..."))
            {
                ResultCollection.RemoveAt(ResultCollection.Count - 1);
            }

            Dictionary<string, string> ResultList = new Dictionary<string, string>();
            if (ResultCollection.Count > 0)
            {
                foreach (HtmlNode Post in ResultCollection)
                {
                    HtmlNode DataHolder = Post.SelectSingleNode(".//table[@class='resulttable']");
                    string PostID = DataHolder.SelectSingleNode(".//div[@class='resultmatchinfo']//a").Attributes["href"].Value;
                    PostID = PostID.Replace("https://e621.net/post/show/", "");
                    string ImgLink = DataHolder.SelectSingleNode(".//td[@class='resulttableimage']//img").Attributes["src"].Value;
                    ResultList.Add(PostID, ImgLink);
                }
            }
            else
            {
                if (_FormReference != null) //already closed
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("No probable matches found.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Close();
                    }));
                }
                return;
            }
            if (_FormReference == null) return; //already closed

            Invoke(new Action(() => Label_SearchCheck.Text = "Getting image data..."));

            JToken e6info_Data = JObject.Parse(Module_e621Info.e621InfoDownload($"https://e621.net/posts.json?tags=id:{string.Join(",", ResultList.Keys)}"))["posts"];

            // deleted images won't appear
            List<GroupBox> ResultGBs = new List<GroupBox>();
            int IndexCounter = 0;
            foreach (JObject picPost in e6info_Data.Children())
            {
                string PostID = picPost["id"].Value<string>();
                string PostRating = picPost["rating"].Value<string>().ToUpper();
                int FileSize = (int)(picPost["file"]["size"].Value<int>() / 1024d);
                string PicFormat = "." + picPost["file"]["ext"].Value<string>();
                string PicWidth = picPost["file"]["width"].Value<string>();
                string PicHeight = picPost["file"]["height"].Value<string>();
                string PicMD5 = picPost["file"]["md5"].Value<string>();
                string PicPreview = ResultList[PostID]; //picPost["preview"]["url"].Value<string>();
                string TagString = string.Join(" ", picPost.SelectTokens("$.tags.*[*]").ToList());

                GroupBox PicGB = new GroupBox()
                {
                    Size = new Size(156, 169), //150x150 pb
                    ForeColor = Color.LightSteelBlue,
                    Text = $"{PostID} - {PostRating} ({FileSize} KB {PicFormat})",
                    Tag = PostRating
                };
                PictureBox PicBox = new PictureBox()
                {
                    Cursor = Cursors.No,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Parent = PicGB,
                    Dock = DockStyle.Fill,
                    Name = PostID,
                    Tag = TagString,
                    InitialImage = Properties.Resources.E6Image_Loading
                };
                PicBox.MouseClick += ItemClick;
                if (_FormReference == null) return; //already closed

                Invoke(new Action(() => PicBox.LoadAsync(PicPreview)));
                toolTip_Display.SetToolTip(PicBox, $"Resolution: {PicWidth}x{PicHeight}\nFile size: {FileSize} KB {PicFormat}\nMD5: {PicMD5}");
                PicGB.Controls.Add(PicBox);
                ResultGBs.Add(PicGB);
                IndexCounter += 1;
            }
            ResultList = null;
            e6info_Data = null;

            Move2Center = true;
            if (_FormReference == null) return; //already closed

            Invoke(new Action(() =>
            {
                FlowLayoutPanel_Holder.Controls.Clear();
                FlowLayoutPanel_Holder.SuspendLayout();
    
                if (ResultGBs.Count < 5) // 39 height diff, 16 width
                {
                    Width = 16 + 162 * ResultGBs.Count;
                    Height = 39 + 177;
                }
                else
                {
                    Width = 16 + 162 * 4;
                    int HeightCounter = (int)(ResultGBs.Count / 4d);
                    Height = 39 + 177 * HeightCounter;
                }
                FlowLayoutPanel_Holder.Controls.AddRange(ResultGBs.ToArray());
                FlowLayoutPanel_Holder.ResumeLayout();
            }));
        }

        private void CheckIQDBQImages(object sender, DoWorkEventArgs e)
        {
            string ResponseString = Module_e621Info.e621InfoDownload($"https://e621.net/iqdb_queries.json?url={(string)Form_Preview._FormReference.Preview_RowHolder["Grab_MediaURL"]}", true);
            if (ResponseString == null || ResponseString.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                if (_FormReference != null) //already closed
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("No probable matches found.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Close();
                    }));
                }
                return;
            }
            if (_FormReference == null) return; //already closed
            Invoke(new Action(() => Label_SearchCheck.Text = "Getting image data..."));

            List<GroupBox> ResultGBs = new List<GroupBox>();
            JArray e6info_Data = JArray.Parse(ResponseString);
            foreach (JObject picPost in e6info_Data.Children())
            {
                JToken PostHolder = picPost["post"]["posts"];
                string PostID = PostHolder["id"].Value<string>();
                string PostRating = PostHolder["rating"].Value<string>().ToUpper();
                int FileSize = (int)(PostHolder["file_size"].Value<int>() / 1024d);
                string PicWidth = PostHolder["image_width"].Value<string>();
                string PicHeight = PostHolder["image_height"].Value<string>();
                string PicFormat = null;
                string PicMD5 = null;
                string PicPreview = null;

                if (PostHolder["is_deleted"].Value<bool>())
                {
                    PicPreview = "https://e621.net/images/deleted-preview.png";
                }
                else
                {
                    PicFormat = $".{PostHolder["file_ext"].Value<string>()}";
                    PicMD5 = PostHolder["md5"].Value<string>();
                    PicPreview = PostHolder["preview_file_url"].Value<string>();
                }

                GroupBox PicGB = new GroupBox()
                {
                    Size = new Size(156, 169), //150x150 pb
                    ForeColor = Color.LightSteelBlue,
                    Text = $"{PostID} - {PostRating} ({FileSize} KB {PicFormat})",
                    Tag = PostRating
                };
                PictureBox PicBox = new PictureBox()
                {
                    Cursor = Cursors.No,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Parent = PicGB,
                    Dock = DockStyle.Fill,
                    Name = PostID,
                    Tag = PostHolder["tag_string"].Value<string>(),
                    InitialImage = Properties.Resources.E6Image_Loading
                };
                PicBox.MouseClick += ItemClick;
                if (PostHolder["is_deleted"].Value<bool>())
                {
                    PicBox.BackgroundImage = Properties.Resources.E6Image_Deleted;
                }
                else
                {
                    PicBox.InitialImage = Properties.Resources.E6Image_Loading;
                    if (_FormReference != null)
                    {
                        Invoke(new Action(() => PicBox.LoadAsync(PicPreview)));
                    }
                }
                toolTip_Display.SetToolTip(PicBox, $"Resolution: {PicWidth}x{PicHeight}\nFile size: {FileSize} KB {PicFormat}\nMD5: {PicMD5}");
                PicGB.Controls.Add(PicBox);
                ResultGBs.Add(PicGB);
            }
            e6info_Data = null;

            Move2Center = true;
            if (_FormReference == null) return; //already closed
            Invoke(new Action(() =>
            {
                FlowLayoutPanel_Holder.Controls.Clear();
                FlowLayoutPanel_Holder.SuspendLayout();
        
                if (ResultGBs.Count < 5) // 39 height diff, 16 width
                {
                    Width = 16 + 162 * ResultGBs.Count;
                    Height = 39 + 177;
                }
                else
                {
                    Width = 16 + 162 * 4;
                    int HeightCounter = (int)(ResultGBs.Count / 4d);
                    Height = 39 + 177 * HeightCounter;
                }
                FlowLayoutPanel_Holder.Controls.AddRange(ResultGBs.ToArray());
                FlowLayoutPanel_Holder.ResumeLayout();
            }));
        }

        private void ItemClick(object sender, MouseEventArgs e)
        {
            PictureBox Post = (PictureBox)sender;
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                Form_Preview.SuperiorSub(Post.Name, Form_Preview._FormReference.Preview_RowHolder);
                Close();
                return;
            }

            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                Process.Start($"https://e621.net/posts/{Post.Name}");
                return;
            }

            DataRow DataRowTemp = Form_Preview._FormReference.Preview_RowHolder;
            DataRowTemp["Upload_Rating"] = Post.Parent.Tag.ToString();
            DataRowTemp["Uploaded_As"] = Post.Name;
            DataRowTemp["Upload_Tags"] = Post.Tag.ToString();
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref DataRowTemp);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp.cLabel_isUploaded.Text = Post.Name;
            }
            Form_Preview._FormReference.Label_Tags.Text = (string)DataRowTemp["Upload_Tags"];
            Form_Preview._FormReference.Label_AlreadyUploaded.Text = $"Already uploaded as #{Post.Name}";
            Form_Preview._FormReference.UpdateRatingDLButtons();
            if (Properties.Settings.Default.ManualInferiorSave)
            {
                Module_DB.DB_Media_CreateRecord(DataRowTemp);
            }
            Close();
        }

        private void Form_Similar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                e.SuppressKeyPress = true; // fix alt key causing cursor to become arrow and act strange
            }
        }
    }
}