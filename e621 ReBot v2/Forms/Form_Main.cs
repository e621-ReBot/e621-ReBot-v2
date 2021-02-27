using ACM_AutocompleteMenu;
using CefSharp;
using e621_ReBot.Modules;
using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules;
using e621_ReBot_v2.Modules.Grabber;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2
{
    //Comment Searching RegEx ^((\s*/+)).
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();

            ServicePointManager.DefaultConnectionLimit = 64;
            Panel_Holder.MouseDown += Holder_MouseDown;
            Title_Label.MouseDown += Holder_MouseDown;
            Version_Label.MouseDown += Holder_MouseDown;
            label_UserLevel.MouseDown += Holder_MouseDown;
            label_Credit_Upload.MouseDown += Holder_MouseDown;
            label_Credit_Flag.MouseDown += Holder_MouseDown;
            label_Credit_Note.MouseDown += Holder_MouseDown;
            Panel_Holder.MouseMove += Holder_MouseMove;
            Title_Label.MouseMove += Holder_MouseMove;
            Version_Label.MouseMove += Holder_MouseMove;
            label_UserLevel.MouseMove += Holder_MouseMove;
            label_Credit_Upload.MouseMove += Holder_MouseMove;
            label_Credit_Flag.MouseMove += Holder_MouseMove;
            label_Credit_Note.MouseMove += Holder_MouseMove;
            Panel_Holder.MouseUp += Holder_MouseUp;
            Title_Label.MouseUp += Holder_MouseUp;
            Version_Label.MouseUp += Holder_MouseUp;
            label_UserLevel.MouseUp += Holder_MouseUp;
            label_Credit_Upload.MouseUp += Holder_MouseUp;
            label_Credit_Flag.MouseUp += Holder_MouseUp;
            label_Credit_Note.MouseUp += Holder_MouseUp;

            BQB_Start.Click += BrowserQuickButton_Click;
            BQB_HicceArs.Click += BrowserQuickButton_Click;
            BQB_Inkbunny.Click += BrowserQuickButton_Click;
            BQB_Pixiv.Click += BrowserQuickButton_Click;
            BQB_FurAffinity.Click += BrowserQuickButton_Click;
            BQB_Twitter.Click += BrowserQuickButton_Click;
            BQB_Newgrounds.Click += BrowserQuickButton_Click;
            BQB_SoFurry.Click += BrowserQuickButton_Click;
            BQB_Mastodon.Click += BrowserQuickButton_Click;
            BQB_Plurk.Click += BrowserQuickButton_Click;

            cCheckGroupBox_Grab.Paint += CCheckGroupBox_Jobs_Paint;
            cCheckGroupBox_Upload.Paint += CCheckGroupBox_Jobs_Paint;
            cCheckGroupBox_Convert.Paint += CCheckGroupBox_Jobs_Paint;
            cCheckGroupBox_Retry.Paint += CCheckGroupBox_Jobs_Paint;

            UpdateDays_1.CheckedChanged += UpdateDays_CheckedChanged;
            UpdateDays_3.CheckedChanged += UpdateDays_CheckedChanged;
            UpdateDays_7.CheckedChanged += UpdateDays_CheckedChanged;
            UpdateDays_15.CheckedChanged += UpdateDays_CheckedChanged;
            UpdateDays_30.CheckedChanged += UpdateDays_CheckedChanged;
            Naming_e6_0.CheckedChanged += NamingE6_CheckedChanged;
            Naming_e6_1.CheckedChanged += NamingE6_CheckedChanged;
            Naming_e6_2.CheckedChanged += NamingE6_CheckedChanged;
            Naming_web_0.CheckedChanged += NamingWeb_CheckedChanged;
            Naming_web_1.CheckedChanged += NamingWeb_CheckedChanged;
            Naming_web_2.CheckedChanged += NamingWeb_CheckedChanged;
            radioButton_GridItemStyle0.CheckedChanged += RadioButton_GridItemStyle_CheckedChanged;
            radioButton_GridItemStyle1.CheckedChanged += RadioButton_GridItemStyle_CheckedChanged;
            radioButton_ProgressBarStyle0.CheckedChanged += RadioButton_ProgressBarStyle_CheckedChanged;
            radioButton_ProgressBarStyle1.CheckedChanged += RadioButton_ProgressBarStyle_CheckedChanged;
            radioButton_GrabDisplayOrder0.CheckedChanged += RadioButton_GrabDisplayOrder_CheckedChanged;
            radioButton_GrabDisplayOrder1.CheckedChanged += RadioButton_GrabDisplayOrder_CheckedChanged;

            cTreeView_GrabQueue.BeforeSelect += CTreeView_BeforeSelect;
            cTreeView_GrabQueue.NodeMouseClick += CTreeView_NodeMouseClick;
            cTreeView_UploadQueue.BeforeSelect += CTreeView_BeforeSelect;
            cTreeView_UploadQueue.NodeMouseClick += CTreeView_NodeMouseClick;
            cTreeView_ConversionQueue.BeforeSelect += CTreeView_BeforeSelect;
            cTreeView_ConversionQueue.NodeMouseClick += CTreeView_NodeMouseClick;
            cTreeView_RetryQueue.BeforeSelect += CTreeView_BeforeSelect;
            cTreeView_RetryQueue.NodeMouseClick += CTreeView_NodeMouseClick;
            cTreeView_DownloadQueue.BeforeSelect += CTreeView_BeforeSelect;
            cTreeView_DownloadQueue.NodeMouseClick += CTreeView_NodeMouseClick;

            contextMenuStrip_cTreeView.Opening += ContextMenuStrip_cTreeView_Opening;
            contextMenuStrip_cTreeView.Closing += ContextMenuStrip_cTreeView_Closing;
            contextMenuStrip_Conversion.Opening += ContextMenuStrip_cTreeView_Opening;
            contextMenuStrip_Conversion.Closing += ContextMenuStrip_cTreeView_Closing;
            contextMenuStrip_Download.Opening += ContextMenuStrip_cTreeView_Opening;
            contextMenuStrip_Download.Closing += ContextMenuStrip_cTreeView_Closing;

            tabPage_Grid.MouseWheel += TabPage_Grid_MouseWheel;

            RadioButton_DL1.CheckedChanged += RadioButton_DL_CheckedChanged;
            RadioButton_DL2.CheckedChanged += RadioButton_DL_CheckedChanged;
            RadioButton_DL3.CheckedChanged += RadioButton_DL_CheckedChanged;
            RadioButton_DL4.CheckedChanged += RadioButton_DL_CheckedChanged;

            textBox_DelayGrabber.Enter += TextBox_Delay_Enter;
            textBox_DelayUploader.Enter += TextBox_Delay_Enter;
            textBox_DelayDownload.Enter += TextBox_Delay_Enter;
            textBox_DelayGrabber.KeyDown += TextBox_Delay_KeyDown;
            textBox_DelayUploader.KeyDown += TextBox_Delay_KeyDown;
            textBox_DelayDownload.KeyDown += TextBox_Delay_KeyDown;
            textBox_DelayGrabber.KeyPress += TextBox_Delay_KeyPress;
            textBox_DelayUploader.KeyPress += TextBox_Delay_KeyPress;
            textBox_DelayDownload.KeyPress += TextBox_Delay_KeyPress;
        }

        private void Form_Main_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            //heavy shit
            Module_CefSharp.InitializeBrowser("about:blank");
            SetQuickButtonPanelRegion();
            CreateTrackList();
            Module_Downloader.Load_IECache();

            Version_Label.Text = "v" + Application.ProductVersion;

            Module_DB.CreateDBs();

            if (!Properties.Settings.Default.AppName.Equals(""))
            {
                AppName_Label.Text = Properties.Settings.Default.AppName;
            }

            if (!Properties.Settings.Default.UserID.Equals(""))
            {
                BackgroundWorker BGWTemp = new BackgroundWorker();
                BGWTemp.DoWork += Module_Credits.Check_Credit_All;
                BGWTemp.RunWorkerAsync();
            }

            if (Properties.Settings.Default.API_Key.Equals(""))
            {
                bU_APIKey.Text = "Add API Key";
            }
            else
            {
                bU_APIKey.Text = "Remove API Key";
                bU_PoolWatcher.Enabled = true;
                bU_Blacklist.Enabled = true;
                bU_RefreshCredit.Enabled = true;
                if (!Properties.Settings.Default.PoolWatcher.Equals(""))
                {
                    BackgroundWorker NewBGW = new BackgroundWorker();
                    NewBGW.DoWork += Form_PoolWatcher.PoolWatcher_Check4New;
                    NewBGW.RunWorkerAsync();
                }
            }


            if (!Properties.Settings.Default.LastStats.Equals(""))
            {
                string[] LastStatsString = Properties.Settings.Default.LastStats.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                label_Credit_Upload.Text = LastStatsString[0];
                label_Credit_Flag.Text = LastStatsString[1];
                label_Credit_Note.Text = LastStatsString[2];
            }

            CheckBox_ConverterKeepOriginal.Checked = Properties.Settings.Default.Converter_KeepOriginal;
            CheckBox_ConverterDontConvertVideos.Checked = Properties.Settings.Default.Converter_DontConvertVideos;
            CheckBox_RemoveBVAS.Checked = Properties.Settings.Default.RemoveBVAS;
            CheckBox_ExpandedDescription.Checked = Properties.Settings.Default.ExpandedDescription;
            CheckBox_AutocompleteTags.Checked = Properties.Settings.Default.AutocompleteTags;
            CheckBox_BigMode.Checked = Properties.Settings.Default.LoadBigForm;
            CheckBox_DontFlag.Checked = Properties.Settings.Default.DontFlag;
            CheckBox_DontFlag.Visible = !Properties.Settings.Default.UserLevel.Equals("") && Module_Credits.UserLevels[Properties.Settings.Default.UserLevel] > 2;

            if (Properties.Settings.Default.DownloadsFolderLocation.Equals(""))
            {
                label_DownloadsFolder.Text = Application.StartupPath + @"\Downloads\";
                Properties.Settings.Default.DownloadsFolderLocation = label_DownloadsFolder.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                label_DownloadsFolder.Text = Properties.Settings.Default.DownloadsFolderLocation;
            }

            Read_AutoTags();
            Read_AutoPools();
            AutoTags.AllowsTabKey = true;
            AutoTags.LeftPadding = 0;
            Read_Genders();

            ((RadioButton)cGroupBoxColored_Update.Controls.Find("UpdateDays_" + Properties.Settings.Default.UpdateDays, false).FirstOrDefault()).Checked = true;

            ((RadioButton)cGroupBoxColored_NamingE621.Controls[Properties.Settings.Default.Naming_e6]).Checked = true;
            ((RadioButton)cGroupBoxColored_NamingWeb.Controls[Properties.Settings.Default.Naming_web]).Checked = true;

            TrackBar_Volume.Value = Module_VolumeControl.GetApplicationVolume();

            ((RadioButton)tabPage_Settings.Controls.Find("radioButton_GridItemStyle" + Properties.Settings.Default.GridItemStyle, true).FirstOrDefault()).Checked = true;
            ((RadioButton)tabPage_Settings.Controls.Find("radioButton_ProgressBarStyle" + Properties.Settings.Default.ProgressBarStyle, true).FirstOrDefault()).Checked = true;
            ((RadioButton)tabPage_Settings.Controls.Find("radioButton_GrabDisplayOrder" + Properties.Settings.Default.GrabDisplayOrder, true).FirstOrDefault()).Checked = true;
            ((RadioButton)tabPage_Download.Controls.Find("RadioButton_DL" + Properties.Settings.Default.DLThreadsCount, true).FirstOrDefault()).Checked = true;

            textBox_DelayGrabber.Text = Properties.Settings.Default.DelayGrabber.ToString();
            textBox_DelayUploader.Text = Properties.Settings.Default.DelayUploader.ToString();
            textBox_DelayDownload.Text = Properties.Settings.Default.DelayDownload.ToString();

            if (Properties.Settings.Default.Bookmarks != null)
            {
                foreach (string BookmarkedURL in Properties.Settings.Default.Bookmarks)
                {
                    URL_ComboBox.Items.Add(BookmarkedURL);
                }
                panel_ComboBoxBlocker.Visible = false;
            }

            comboBox_PuzzleRows.SelectedIndex = 2;
            comboBox_PuzzleCollumns.SelectedIndex = 2;
            labelPuzzle_SelectedPost.Cursor = Form_Loader.Cursor_ReBotNav;
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            QuickButtonPanel.Visible = !Properties.Settings.Default.FirstRun;

            if (Properties.Settings.Default.Blacklist != null && Properties.Settings.Default.Blacklist.Count > 0)
            {
                foreach (string BlacklistString in Properties.Settings.Default.Blacklist)
                {
                    Blacklist.Add(BlacklistString);
                }
            }

            if (!Properties.Settings.Default.API_Key.Equals("") && !Properties.Settings.Default.PoolWatcher.Equals(""))
            {
                BackgroundWorker NewBGW = new BackgroundWorker();
                NewBGW.DoWork += Form_PoolWatcher.PoolWatcher_Check4New;
                NewBGW.RunWorkerAsync();
            }

            RetryQueue_Load();

            timer_FadeIn.Start();
        }

        private void Timer_FadeIn_Tick(object sender, EventArgs e)
        {
            //to remove shitty flicker
            Opacity += 0.05;
            if (Opacity == 1)
            {
                timer_FadeIn.Stop();
                Menu_Btn_Click(null, null);

                Module_UpdaterUpdater.UpdateTheUpdater();

                if (Properties.Settings.Default.FirstRun)
                {
                    TrackBar_Volume.Value = 25;
                    TrackBar_Volume_Scroll(null, null);
                    panel_Browser.Visible = true;
                    Module_CefSharp.CefSharpBrowser.Load("https://e621.net/session/new");
                    MessageBox.Show("Thanks for trying me out." + Environment.NewLine + Environment.NewLine + "For start, you should log in into e621 and provide me with API key so I could do the tasks you require." + Environment.NewLine + Environment.NewLine + "I opened the login page for you.", "e621 ReBot");
                }

                if (Properties.Settings.Default.Note.Equals(""))
                {
                    bU_NoteRemove.Enabled = false;
                }
                else
                {
                    bU_NoteAdd.Text = "Edit Note";
                    toolTip_Display.SetToolTip(bU_NoteAdd, "Edit existing note.");
                    new Form_Notes();
                    Form_Notes._FormReference.StartPosition = FormStartPosition.Manual;
                    Form_Notes._FormReference.Location = new Point(Location.X + Width / 2 - Form_Notes._FormReference.Width / 2, Location.Y + Height / 2 - Form_Notes._FormReference.Height / 2);
                    Form_Notes._FormReference.ShowDialog();
                }
            }
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cTreeView_UploadQueue.Nodes.Count > 0 | Module_TableHolder.Download_Table.Rows.Count > 0 | cTreeView_ConversionQueue.Nodes.Count > 0)
            {
                if (MessageBox.Show("There are currently some jobs active, are you sure you want to close me?", "e621 ReBot Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Cef.Shutdown();

            Module_CefSharp.CefSharpBrowser.Dispose();
            Properties.Settings.Default.LastStats = string.Format("{0},{1},{2}", label_Credit_Upload.Text, label_Credit_Flag.Text, label_Credit_Note.Text);
            RetryQueue_Save();
            if (AutoTagsListChanged)
            {
                File.WriteAllText("tags.txt", string.Join("✄", AutoTagsList_Tags));
            }
        }

        public Process ConversionQueueProcess;
        public Process UploadQueueProcess;
        private void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ConversionQueueProcess != null)
            {
                ConversionQueueProcess.Kill();
            }

            if (UploadQueueProcess != null)
            {
                UploadQueueProcess.Kill();
            }
            Application.Exit();
        }









        #region "Drag"

        private bool FormMoving;
        private Point FormMoving_CursorPosition;
        private bool CanMoveAgain;
        private void Holder_MouseDown(object sender, MouseEventArgs e)
        {
            FormMoving = true;
            FormMoving_CursorPosition = e.Location;
            CanMoveAgain = true;
            timer_Refresh.Start();
        }

        private void Holder_MouseMove(object sender, MouseEventArgs e)
        {
            if (FormMoving && CanMoveAgain)
            {
                Location = new Point(Location.X - FormMoving_CursorPosition.X + e.X, Location.Y - FormMoving_CursorPosition.Y + e.Y);
                if (Form_Menu._FormReference != null)
                {
                    Form_Menu._FormReference.Location = new Point(Location.X - 16, Location.Y + 128);
                }
                CanMoveAgain = false;
            }
        }

        private void Holder_MouseUp(object sender, MouseEventArgs e)
        {
            FormMoving = false;
            timer_Refresh.Stop();
        }

        private void Timer_Refresh_Tick(object sender, EventArgs e)
        {
            CanMoveAgain = true;
        }

        #endregion









        private void Opene6_btn_Click(object sender, EventArgs e)
        {
            Button_Unfocusable e6Btn = (Button_Unfocusable)sender;

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                Process.Start("https://e621.net/");
            }
            else
            {
                QuickButtonPanel.Visible = false;
                Module_CefSharp.CefSharpBrowser.Load(e6Btn.Tag.ToString());
                panel_Browser.Visible = true;
                cTabControl_e621ReBot.SelectedIndex = 0;
            }
        }

        private void Menu_Btn_Click(object sender, EventArgs e)
        {
            Form_Menu Form_Menu_Temp = new Form_Menu
            {
                Owner = this,
                Location = new Point(Location.X - 128, Location.Y + 128)
            };
            Menu_Btn.Visible = false;
            Form_Menu_Temp.Show();
        }

        private void CTabControl_e621ReBot_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Form_Menu._FormReference != null)
            {
                Button_Menu TemoMBHolder = (Button_Menu)Form_Menu._FormReference.Menu_FlowLayoutPanel.Controls[cTabControl_e621ReBot.SelectedIndex];
                TemoMBHolder.Active = true;
                TemoMBHolder.MB_Highlight();
                TemoMBHolder.ClearOthers();
            }

            if (Form_Tagger._FormReference != null && Form_Tagger._FormReference.Owner == this)
            {
                Form_Tagger._FormReference.Close();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (cTabControl_e621ReBot.SelectedIndex == 1)
            {
                switch (keyData)
                {
                    case Keys.Left:
                        {
                            if (GB_Left.Visible) GB_Left_Click(null, null);
                            break;
                        }

                    case Keys.Right:
                        {
                            if (GB_Right.Visible) GB_Right_Click(null, null);
                            break;
                        }
                }

                if (_Selected_e6GridItem != null)
                {
                    switch (keyData)
                    {
                        case Keys.E:
                        case Keys.Q:
                        case Keys.S:
                            {
                                _Selected_e6GridItem._Rating = keyData.ToString();
                                if (Form_Preview._FormReference != null && ReferenceEquals(_Selected_e6GridItem._DataRowReference, Form_Preview._FormReference.Preview_RowHolder))
                                {
                                    Form_Preview._FormReference.UpdateButtons();
                                }
                                break;
                            }

                        case Keys.T:
                            {
                                Form_Tagger.OpenTagger(this, _Selected_e6GridItem._DataRowReference, Cursor.Position);
                                break;
                            }

                        case Keys.Oemplus:
                        case Keys.Add:
                        case Keys.NumPad1:
                        case Keys.D1:
                            {
                                _Selected_e6GridItem.cCheckBox_UPDL.Checked = true;
                                break;
                            }

                        case Keys.OemMinus:
                        case Keys.Subtract:
                        case Keys.NumPad0:
                        case Keys.D0:
                            {
                                _Selected_e6GridItem.cCheckBox_UPDL.Checked = false;
                                break;
                            }

                        case Keys.Delete:
                            {
                                if (cTreeView_UploadQueue.Nodes.ContainsKey((string)_Selected_e6GridItem._DataRowReference["Grab_MediaURL"]))
                                {
                                    MessageBox.Show("Image can't be removed while it is queued for upload.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                                //else if (this.TreeView_DownloadQueue.Nodes.ContainsKey((string)Selectione6Img_Selected.RowReference["Grab_MediaURL"]))
                                //{
                                //    MessageBox.Show("Image can't be removed while it is queued for download.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //}
                                else
                                {
                                    flowLayoutPanel_Grid.SuspendLayout();
                                    Module_Grabber._Grabbed_MediaURLs.Remove((string)_Selected_e6GridItem._DataRowReference["Grab_MediaURL"]);
                                    _Selected_e6GridItem._DataRowReference.Delete();
                                    _Selected_e6GridItem.Dispose();
                                    flowLayoutPanel_Grid.ResumeLayout();
                                    Paginator();
                                    if (Form_Preview._FormReference != null) Form_Preview._FormReference.Close();
                                }
                                break;
                            }
                    }
                }
            }

            return false;
        }

        private void BU_KoFi_Click(object sender, EventArgs e)
        {
            Process.Start("https://ko-fi.com/e621rebot");
        }







        #region "Browser Tab"

        private void Panel_Browser_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel_Browser.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(panel_BrowserDisplay.Location.X, panel_BrowserDisplay.Location.Y - 1), new Point(panel_BrowserDisplay.Width, panel_BrowserDisplay.Location.Y - 1));
        }

        private void SetQuickButtonPanelRegion()
        {

            GraphicsPath PanelRegionPath = new GraphicsPath();
            Bitmap RegionImage = new Bitmap(Properties.Resources.BrowserButtonsRegion);

            int BGWidth = RegionImage.Width;
            int BGHeight = RegionImage.Height;

            BitmapData bmpData = RegionImage.LockBits(new Rectangle(0, 0, BGWidth, BGHeight), ImageLockMode.ReadWrite, RegionImage.PixelFormat);

            // PixelSize is 4 bytes for a 32bpp Argb image.
            int PixelSize = 4; // 32bits/8=4 bytes
            // Declare an array to hold the bytes of the bitmap.
            int numBytes = bmpData.Stride * bmpData.Height; // BGWidth * PixelSize * BGHeight
            byte[] rgbValues = new byte[numBytes - 1 + 1];
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, numBytes);

            int LineStart;
            for (int y = 20; y <= BGHeight - 76; y++)
            {
                LineStart = 0;
                for (int x = 79; x <= BGWidth - 79; x++)
                {
                    // Get the various pixel locations  This calculation is for a 32bpp Argb bitmap
                    int ByteLocation = (y * BGWidth * PixelSize) + (x * PixelSize);

                    if (rgbValues[ByteLocation + 3] != 0)
                    {
                        if (LineStart == 0)
                        {
                            LineStart = x;
                        }
                    }
                    else
                    {
                        if (LineStart != 0)
                        {
                            PanelRegionPath.AddRectangle(new Rectangle(new Point(LineStart, y), new Size(x - LineStart, 1)));
                            break;
                        }
                    }
                }
            }
            RegionImage.UnlockBits(bmpData);
            QuickButtonPanel.Region = new Region(PanelRegionPath);
        }

        private void BrowserQuickButton_Click(object sender, EventArgs e)
        {
            QuickButtonPanel.Visible = false;
            Button_Browser _sender = (Button_Browser)sender;
            URL_ComboBox.Text = _sender.Tag.ToString();
            Module_CefSharp.CefSharpBrowser.Load(_sender.Tag.ToString());
            panel_Browser.Visible = true;
        }

        private void BB_Bookmarks_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                Properties.Settings.Default.Bookmarks.Clear();
                URL_ComboBox.Enabled = false;
                URL_ComboBox.Items.Clear();
                URL_ComboBox.Enabled = true;
                BB_Bookmarks.BackColor = Color.Gray;
                panel_ComboBoxBlocker.Visible = true;
            }
            else
            {
                string WebAdress = WebUtility.UrlDecode(Module_CefSharp.CefSharpBrowser.Address);
                if (Properties.Settings.Default.Bookmarks == null)
                {
                    Properties.Settings.Default.Bookmarks = new StringCollection();
                }

                if (Properties.Settings.Default.Bookmarks.Contains(WebAdress))
                {
                    Properties.Settings.Default.Bookmarks.Remove(WebAdress);
                    BB_Bookmarks.BackColor = Color.Gray;
                    toolTip_Display.SetToolTip(BB_Bookmarks, "Bookmark current page." + Environment.NewLine + Environment.NewLine + "Hold Ctrl when clicking to clear all Bookmarks.");
                    URL_ComboBox.Items.Remove(WebAdress);
                    URL_ComboBox.Text = WebAdress; // Fix bug, item removal removing text.
                    panel_ComboBoxBlocker.Visible = URL_ComboBox.Items.Count == 0;
                    if (Properties.Settings.Default.Bookmarks.Count == 0)
                    {
                        Properties.Settings.Default.Bookmarks = null;
                    }
                }
                else
                {
                    Properties.Settings.Default.Bookmarks.Add(WebAdress);
                    BB_Bookmarks.BackColor = Color.RoyalBlue;
                    toolTip_Display.SetToolTip(BB_Bookmarks, "Remove Bookmark." + Environment.NewLine + Environment.NewLine + "Hold Ctrl when clicking to clear all Bookmarks.");
                    URL_ComboBox.Items.Add(WebAdress);
                    panel_ComboBoxBlocker.Visible = false;
                }
            }
            Properties.Settings.Default.Save();
        }

        private void BB_Backward_Click(object sender, EventArgs e)
        {
            BB_Backward.Enabled = false;
            Module_CefSharp.CefSharpBrowser.Back();
        }

        private void BB_Backward_EnabledChanged(object sender, EventArgs e)
        {
            BB_Backward.BackgroundImage = (Image)Properties.Resources.ResourceManager.GetObject("BB_Backward_" + BB_Backward.Enabled.ToString());
        }

        private void BB_Reload_Click(object sender, EventArgs e)
        {
            Module_CefSharp.CefSharpBrowser.Reload();
            Module_Twitter.TwitterJSONHolder = null;
        }

        private void BB_Forward_Click(object sender, EventArgs e)
        {
            BB_Forward.Enabled = false;
            Module_CefSharp.CefSharpBrowser.Forward();
        }

        private void BB_Forward_EnabledChanged(object sender, EventArgs e)
        {
            BB_Forward.BackgroundImage = (Image)Properties.Resources.ResourceManager.GetObject("BB_Forward_" + BB_Forward.Enabled.ToString());
        }

        private void BB_Home_Click(object sender, EventArgs e)
        {
            QuickButtonPanel.Visible = !QuickButtonPanel.Visible;
        }

        private void URL_ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Detect Paste
            if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.V)
            {
                if (Clipboard.GetDataObject().GetDataPresent(DataFormats.StringFormat))
                {
                    string ClipboardText = (string)Clipboard.GetDataObject().GetData(DataFormats.StringFormat);
                    ClipboardText = WebUtility.UrlDecode(ClipboardText);
                    e.SuppressKeyPress = true; // disable original paste

                    if (URL_ComboBox.SelectedText.Length > 0)
                    {
                        URL_ComboBox.Text = URL_ComboBox.Text.Replace(URL_ComboBox.SelectedText, ClipboardText);
                    }
                    else
                    {
                        URL_ComboBox.Text = ClipboardText;
                    }
                    URL_ComboBox.SelectionStart = URL_ComboBox.Text.Length;
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true; // disable sound
                    BB_Navigate.PerformClick();
                }
            }
        }

        private void URL_ComboBox_TextChanged(object sender, EventArgs e)
        {
            BB_Navigate.Enabled = !URL_ComboBox.Text.Equals("");
        }

        private void URL_ComboBox_Leave(object sender, EventArgs e)
        {
            URL_ComboBox.Text = URL_ComboBox.Text.Trim();
        }

        private void URL_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BB_Navigate.PerformClick();
        }

        private void BB_Navigate_Click(object sender, EventArgs e)
        {
            if (!URL_ComboBox.Text.Equals(""))
            {
                Title_Label.Focus();
                BB_Bookmarks.Enabled = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(CheckURLExists), URL_ComboBox.Text);
            }
        }

        private void CheckURLExists(object WebURLObject)
        {
            string WebURL = WebURLObject.ToString();
            if (WebURL.Equals("about:blank"))
            {
                Module_CefSharp.CefSharpBrowser.Load(WebURL);
                return;
            }

            if (!WebURL.Contains("."))
            {
                Module_CefSharp.CefSharpBrowser.Load("https://www.google.com/search?q=" + WebURL);
                return;
            }

            string WebURLCheck = WebURL;
            if (!WebURLCheck.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                WebURLCheck = "http://" + WebURLCheck;

            HttpWebRequest URLChecker = (HttpWebRequest)WebRequest.Create(WebURLCheck);
            URLChecker.CookieContainer = new CookieContainer();
            URLChecker.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0";
            URLChecker.Method = "HEAD";
            URLChecker.Timeout = 5000;
            HttpWebResponse UrlCheckerRepose;
            try
            {
                UrlCheckerRepose = (HttpWebResponse)URLChecker.GetResponse();
                switch (UrlCheckerRepose.StatusCode)
                {
                    case HttpStatusCode.OK: //200
                        {
                            Module_CefSharp.CefSharpBrowser.Load(WebURLCheck);
                            break;
                        }

                    case HttpStatusCode.BadRequest: //400
                    case HttpStatusCode.Forbidden: //403
                    case HttpStatusCode.NotFound: //404
                    case HttpStatusCode.InternalServerError: //500
                    case HttpStatusCode.BadGateway: //502
                    case HttpStatusCode.ServiceUnavailable: //503
                    case HttpStatusCode.GatewayTimeout: //504
                        {
                            Module_CefSharp.CefSharpBrowser.Load("https://www.google.com/search?q=" + WebURL);
                            break;
                        }
                }
                UrlCheckerRepose.Dispose();
            }
            catch (Exception)
            {
                Module_CefSharp.CefSharpBrowser.Load("https://www.google.com/search?q=" + WebURL);
            }
            finally
            {
                //nothing?
            }
        }

        private List<UnmanagedMemoryStream> TrackList = new List<UnmanagedMemoryStream>();
        private void CreateTrackList()
        {
            TrackList.Add(Properties.Resources.PeasantPissed3);
            TrackList.Add(Properties.Resources.PeasantWhat3);
            TrackList.Add(Properties.Resources.PeasantYes2);
            TrackList.Add(Properties.Resources.PeasantYesAttack2);
            TrackList.Add(Properties.Resources.PeasantYesAttack4);
            TrackList.Add(Properties.Resources.PeonReady1);
            TrackList.Add(Properties.Resources.PeonYes3);
            TrackList.Add(Properties.Resources.PeonYesAttack1);
            TrackList.Add(Properties.Resources.PeonYesAttack3);
        }
        private void Worker_Sound()
        {
            int trackNum = new Random().Next(0, 8);
            int ChanceProc = new Random().Next(0, 100);

            if (ChanceProc < 25)
            {
                UnmanagedMemoryStream Track = TrackList[trackNum];
                SoundPlayer WorkerPlayer = new SoundPlayer(Track);
                Track.Seek(0, SeekOrigin.Begin);
                WorkerPlayer.Play();
            }
        }

        private void BB_Grab_All_Click(object sender, EventArgs e)
        {
            if (BB_Grab_All.Text.Equals("Stop"))
            {
                timer_TwitterGrabber.Stop();
                LastBrowserPosition = 0;
                LastBrowserPositionCounter = 0;
                BB_Grab_All.Text = "Grab All";
                BB_Grab.Visible = true;
            }
            else
            {
                Worker_Sound();
                BB_Grab_All.Text = "Stop";
                timer_TwitterGrabber.Start();
                BB_Grab.Visible = false;
            }
        }

        public int LastBrowserPosition;
        public int LastBrowserPositionCounter;
        private void Timer_TwitterGrabber_Tick(object sender, EventArgs e)
        {
            Module_CefSharp.CefSharpBrowser.ExecuteScriptAsync("window.scrollBy(0, 2048);");
            int WindowPosition = int.Parse(Module_CefSharp.CefSharpBrowser.EvaluateScriptAsync("window.pageYOffset;").Result.Result.ToString());
            cTreeView_GrabQueue.SuspendLayout();
            cTreeView_GrabQueue.BeginUpdate();
            Module_Grabber.PrepareLink(Module_CefSharp.CefSharpBrowser.Address);
            if (WindowPosition == LastBrowserPosition)
            {
                LastBrowserPositionCounter += 1;
                if (LastBrowserPositionCounter == 3)
                {
                    timer_TwitterGrabber.Stop();
                    BB_Grab_All.Text = "Grab All";
                    LastBrowserPosition = 0;
                    LastBrowserPositionCounter = 0;
                    BB_Grab.Visible = true;
                }
            }
            else
            {
                LastBrowserPositionCounter = 0;
                LastBrowserPosition = WindowPosition;
            }
            cTreeView_GrabQueue.EndUpdate();
            cTreeView_GrabQueue.ResumeLayout();
            //Module_CefSharp.CefSharpBrowser.ExecuteScriptAsync("document.querySelectorAll(\"article div[data-testid='tweet'] div[role='button']:not([aria-label])\").forEach(button=>button.click());");
        }

        private void BB_Grab_Click(object sender, EventArgs e)
        {
            if (Module_CefSharp.CefSharpBrowser.Address.Contains("twitter.com"))
            {
                Module_CefSharp.CefSharpBrowser.ExecuteScriptAsync("document.querySelectorAll(\"article div[data-testid='tweet'] div[role='button']:not([aria-label])\").forEach(button=>button.click())");
            }
            cTreeView_GrabQueue.SuspendLayout();
            cTreeView_GrabQueue.BeginUpdate();
            BB_Grab.Visible = false;
            Worker_Sound();
            Module_Grabber.PrepareLink(Module_CefSharp.CefSharpBrowser.Address);
            cTreeView_GrabQueue.EndUpdate();
            cTreeView_GrabQueue.ResumeLayout();
        }

        private void BB_Download_Click(object sender, EventArgs e)
        {
            BB_Download.Visible = false;
            Worker_Sound();
            Module_Downloader.Grab_e621();
            Module_Downloader.UpdateTreeViewNodes();
            Module_Downloader.timer_Download.Start();
        }

        private void DevTools_Button_Click(object sender, EventArgs e)
        {
            //https://blog.dotnetframework.org/2018/10/26/intercepting-ajax-requests-in-cefsharp-chrome-for-c/
            Module_CefSharp.CefSharpBrowser.ShowDevTools();
        }

        #endregion









        #region "Grid Tab"

        private void TabPage_Grid_Paint(object sender, PaintEventArgs e)
        {
            Form_Loader.LastAltState = !Form_Loader.LastAltState;
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(0, tabPage_Grid.Height - 27), new Point(tabPage_Grid.Width, tabPage_Grid.Height - 27)); //Horizontal, at bottom
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(36 - 1, tabPage_Grid.Height - 27), new Point(36 - 1, tabPage_Grid.Height)); // Vertical, left
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(tabPage_Grid.Width - 36, tabPage_Grid.Height - 27), new Point(tabPage_Grid.Width - 36, tabPage_Grid.Height)); // Vertical, right
        }

        public int GridIndexTracker = 0;
        public e6_GridItem _Selected_e6GridItem;
        public void AddGridItem(ref DataRow RowReff, bool MultiAdd)
        {
            if (!MultiAdd)
            {
                UIDrawController.SuspendDrawing(flowLayoutPanel_Grid);
                flowLayoutPanel_Grid.SuspendLayout();
            }

            e6_GridItem NewGridItem = new e6_GridItem()
            {
                _DataRowReference = RowReff,
                _Rating = (string)RowReff["Upload_Rating"]

            };
            NewGridItem.cCheckBox_UPDL.Checked = (bool)RowReff["UPDL_Queued"];
            if (RowReff["Thumbnail_Image"] == DBNull.Value)
            {
                if (RowReff["Thumbnail_DLStart"] == DBNull.Value)
                {
                    RowReff["Thumbnail_DLStart"] = true;
                    Module_Grabber.DownloadThumb(ref RowReff);
                }
            }
            else
            {
                NewGridItem.pictureBox_ImageHolder.Image = (Image)RowReff["Thumbnail_Image"];
                NewGridItem.pictureBox_ImageHolder.BackgroundImage = null;
            }
            if (RowReff["Uploaded_As"] != DBNull.Value)
            {
                NewGridItem.cLabel_isUploaded.Text = (string)RowReff["Uploaded_As"];
            }
            flowLayoutPanel_Grid.Controls.Add(NewGridItem);

            if (RowReff["Info_TooBig"] == DBNull.Value && RowReff["Info_MediaByteLength"] != DBNull.Value && RowReff["Info_MediaWidth"] != DBNull.Value)
            {
                RowReff["Info_TooBig"] = Module_Uploader.Media2BigCheck(ref RowReff);
            }
            if (!MultiAdd)
            {
                flowLayoutPanel_Grid.ResumeLayout();
                UIDrawController.ResumeDrawing(flowLayoutPanel_Grid);
            }
        }

        public void Paginator()
        {
            int CurrentPage = (int)((float)GridIndexTracker / Form_Loader._GridMaxControls + 1);
            int TotalPages = (int)Math.Ceiling((float)Module_TableHolder.Database_Table.Rows.Count / Form_Loader._GridMaxControls);
            Label_PageShower.Text = string.Format("{0} / {1}", CurrentPage, TotalPages);
            Label_LeftPage.Text = (CurrentPage - 1).ToString();
            Label_RightPage.Text = (CurrentPage + 1).ToString();
        }

        public e6_GridItem IsE6PicVisibleInGrid(ref DataRow RefDataRow)
        {
            int RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(RefDataRow);
            if (RowIndex >= GridIndexTracker && RowIndex < (GridIndexTracker + Form_Loader._GridMaxControls))
            {
                return (e6_GridItem)flowLayoutPanel_Grid.Controls[RowIndex - GridIndexTracker];
            }
            return null;
        }

        private void FlowLayoutPanel_Grid_ControlAdded(object sender, ControlEventArgs e)
        {
            Form_Menu._FormReference.MB_Grid.Visible = true;

            e6_GridItem E6ImagePic = (e6_GridItem)e.Control;
            if (Name == "Form_MainBig")
            {
                E6ImagePic.Margin = new Padding(2, 1, 1, 24);
            }

            if (((string)E6ImagePic._DataRowReference["Upload_Tags"]).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length > 5)
            {
                E6ImagePic.cLabel_TagWarning.Visible = false;
            }
            if (Form_Preview._FormReference != null)
            {
                Form_Preview._FormReference.UpdateNavButtons();
            }
        }

        private void FlowLayoutPanel_Grid_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (flowLayoutPanel_Grid.Controls.Count == 0)
            {
                if (Module_TableHolder.Database_Table.Rows.Count > 0)
                {
                    GB_Left_Click(null, null);
                }
                else
                {
                    cTabControl_e621ReBot.SelectedIndex = 0;
                    GB_Right.Visible = false;
                    if (Form_Menu._FormReference != null)
                    {
                        Form_Menu._FormReference.MB_Grid.Visible = false;
                    }
                }
            }
            else
            {
                int CountImagesAfterThisPage = Module_TableHolder.Database_Table.Rows.Count - GridIndexTracker;
                if (CountImagesAfterThisPage >= Form_Loader._GridMaxControls)
                {
                    GB_Right.Visible = CountImagesAfterThisPage > Form_Loader._GridMaxControls;
                    DataRow DataRowTemp = Module_TableHolder.Database_Table.Rows[GridIndexTracker + flowLayoutPanel_Grid.Controls.Count];
                    AddGridItem(ref DataRowTemp, true);
                }
                else
                {
                    GB_Right.Visible = false;
                }
            }
        }

        public void PopulateGrid(int StartIndex)
        {
            int LoopTo = Math.Min(StartIndex + Form_Loader._GridMaxControls - 1, Module_TableHolder.Database_Table.Rows.Count - 1);
            for (int x = StartIndex; x <= LoopTo; x++)
            {
                DataRow DataRowTemp = Module_TableHolder.Database_Table.Rows[x];
                AddGridItem(ref DataRowTemp, true);
            }
        }

        private void ClearGrid()
        {
            flowLayoutPanel_Grid.ControlRemoved -= FlowLayoutPanel_Grid_ControlRemoved;
            while (flowLayoutPanel_Grid.Controls.Count > 0)
            {
                flowLayoutPanel_Grid.Controls[0].Dispose();
            }
            flowLayoutPanel_Grid.Controls.Clear();
            flowLayoutPanel_Grid.ControlRemoved += FlowLayoutPanel_Grid_ControlRemoved;
            _Selected_e6GridItem = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void GB_Left_Click(object sender, EventArgs e)
        {
            UIDrawController.SuspendDrawing(flowLayoutPanel_Grid);
            flowLayoutPanel_Grid.SuspendLayout();
            ClearGrid();
            GridIndexTracker -= Form_Loader._GridMaxControls;
            if (GridIndexTracker <= 0)
            {
                GridIndexTracker = 0;
                GB_Left.Visible = false;
            }

            if (Module_TableHolder.Database_Table.Rows.Count - Form_Loader._GridMaxControls > GridIndexTracker)
            {
                GB_Right.Visible = true;
            }
            PopulateGrid(GridIndexTracker);
            Paginator();
            flowLayoutPanel_Grid.ResumeLayout();
            UIDrawController.ResumeDrawing(flowLayoutPanel_Grid);
        }

        private void GB_Left_VisibleChanged(object sender, EventArgs e)
        {
            Label_LeftPage.Visible = GB_Left.Visible;
        }

        private void GB_Right_Click(object sender, EventArgs e)
        {
            UIDrawController.SuspendDrawing(flowLayoutPanel_Grid);
            flowLayoutPanel_Grid.SuspendLayout();
            ClearGrid();
            GridIndexTracker += Form_Loader._GridMaxControls;
            if (GridIndexTracker > Module_TableHolder.Database_Table.Rows.Count - Form_Loader._GridMaxControls)
            {
                GB_Right.Visible = false;
            }
            else
            {
                GB_Right.Visible = true;
            }

            GB_Left.Visible = true;
            PopulateGrid(GridIndexTracker);
            Paginator();
            flowLayoutPanel_Grid.ResumeLayout();
            UIDrawController.ResumeDrawing(flowLayoutPanel_Grid);
        }

        private void GB_Right_VisibleChanged(object sender, EventArgs e)
        {
            Label_RightPage.Visible = GB_Right.Visible;
        }

        private void TabPage_Grid_MouseWheel(object sender, MouseEventArgs e)
        {
            tabPage_Grid.MouseWheel -= TabPage_Grid_MouseWheel;
            if (e.Delta > 0) // Up / Left
            {
                if (GB_Left.Visible)
                {
                    GB_Left_Click(null, null);
                }
            }
            else
            {
                if (GB_Right.Visible) // Down / Right
                {
                    GB_Right_Click(null, null);
                }

            }
            var ScrollDisableTimer = new Timer
            {
                Interval = 100
            };
            ScrollDisableTimer.Tick += ScrollDisable_Tick;
            ScrollDisableTimer.Start();
        }

        private void ScrollDisable_Tick(object sender, EventArgs e)
        {
            Timer ScrollDisableTimer = (Timer)sender;
            ScrollDisableTimer.Stop();
            ScrollDisableTimer.Dispose();
            tabPage_Grid.MouseWheel += TabPage_Grid_MouseWheel;
        }

        public int UploadCounter;
        public int DownloadCounter;
        private void GB_Check_Click(object sender, EventArgs e)
        {
            foreach (e6_GridItem e6_GridItemTemp in flowLayoutPanel_Grid.Controls)
            {
                e6_GridItemTemp.cCheckBox_UPDL.Checked = true;
            }
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                foreach (DataRow PicImageRow in Module_TableHolder.Database_Table.Rows)
                {
                    PicImageRow["UPDL_Queued"] = true;
                }
            }
        }

        private void GB_Inverse_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                foreach (DataRow PicImageRow in Module_TableHolder.Database_Table.Rows)
                {
                    PicImageRow["UPDL_Queued"] = !(bool)PicImageRow["UPDL_Queued"];
                }
                foreach (e6_GridItem e6_GridItemTemp in flowLayoutPanel_Grid.Controls)
                {
                    e6_GridItemTemp.cCheckBox_UPDL.Checked = (bool)e6_GridItemTemp._DataRowReference["UPDL_Queued"];
                }
            }
            else
            {
                foreach (e6_GridItem e6_GridItemTemp in flowLayoutPanel_Grid.Controls)
                {
                    e6_GridItemTemp.cCheckBox_UPDL.Checked = !e6_GridItemTemp.cCheckBox_UPDL.Checked;
                }
            }
        }

        private void GB_Uncheck_Click(object sender, EventArgs e)
        {
            foreach (e6_GridItem e6_GridItemTemp in flowLayoutPanel_Grid.Controls)
            {
                e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
            }
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                foreach (DataRow PicImageRow in Module_TableHolder.Database_Table.Rows)
                {
                    PicImageRow["UPDL_Queued"] = false;
                }
            }
        }

        private void GB_Clear_Click(object sender, EventArgs e)
        {
            if (cTreeView_UploadQueue.Nodes.Count > 0) //| this.TreeView_DownloadQueue.Nodes.Count > 0)
            {
                MessageBox.Show("Images can't be removed while they are queued for upload.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Form_Tagger._FormReference != null) Form_Tagger._FormReference.Close();
                if (Form_Preview._FormReference != null) Form_Preview._FormReference.Close();

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    lock (Module_TableHolder.Database_Table)
                    {
                        for (int i = Module_TableHolder.Database_Table.Select().Length - 1; i >= 0; i--)
                        {
                            if (Module_TableHolder.Database_Table.Rows[i]["Uploaded_As"] != DBNull.Value) Module_TableHolder.Database_Table.Rows.RemoveAt(i);
                        }
                    }


                    if (Module_TableHolder.Database_Table.Rows.Count == 0)
                    {
                        GB_Left.Visible = false;
                        GB_Right.Visible = false;
                        Label_PageShower.Text = "1 / 1";
                        cTabControl_e621ReBot.SelectedIndex = 0;
                        Module_Grabber._Grabbed_MediaURLs.Clear();
                        ClearGrid();
                        GridIndexTracker = 0;
                        lock (Module_TableHolder.Database_Table)
                        {
                            Module_TableHolder.Database_Table.Clear();
                        }
                        if (Form_Menu._FormReference != null) Form_Menu._FormReference.MB_Grid.Visible = false;
                    }
                    else
                    {
                        UIDrawController.SuspendDrawing(flowLayoutPanel_Grid);
                        flowLayoutPanel_Grid.SuspendLayout();
                        ClearGrid();
                        GridIndexTracker = 0;
                        GB_Left.Visible = false;
                        if (Module_TableHolder.Database_Table.Rows.Count - Form_Loader._GridMaxControls > GridIndexTracker) GB_Right.Visible = true;
                        PopulateGrid(GridIndexTracker);
                        Paginator();
                        flowLayoutPanel_Grid.ResumeLayout();
                        UIDrawController.ResumeDrawing(flowLayoutPanel_Grid);
                    }
                }
                else
                {
                    GB_Left.Visible = false;
                    GB_Right.Visible = false;
                    Label_PageShower.Text = "1 / 1";
                    cTabControl_e621ReBot.SelectedIndex = 0;
                    Module_Grabber._Grabbed_MediaURLs.Clear();
                    ClearGrid();
                    GridIndexTracker = 0;
                    Module_TableHolder.Database_Table.Clear();
                    if (Form_Menu._FormReference != null) Form_Menu._FormReference.MB_Grid.Visible = false;
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                }
            }

        }

        private void GB_Download_Click(object sender, EventArgs e)
        {
            GB_Download.Enabled = false;
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                foreach (e6_GridItem e6_GridItemTemp in Form_Loader._GridFLPHolder.Controls)
                {
                    if ((bool)e6_GridItemTemp._DataRowReference["UPDL_Queued"])
                    {
                        if (Module_TableHolder.DownloadQueueContainsURL((string)e6_GridItemTemp._DataRowReference["Grab_MediaURL"]) || Module_Downloader.Download_AlreadyDownloaded.Contains((string)e6_GridItemTemp._DataRowReference["Grab_MediaURL"]))
                        {
                            continue;
                        }
                        else
                        {
                            Module_Downloader.AddDownloadQueueItem(e6_GridItemTemp._DataRowReference, (string)e6_GridItemTemp._DataRowReference["Grab_URL"], (string)e6_GridItemTemp._DataRowReference["Grab_MediaURL"], (string)e6_GridItemTemp._DataRowReference["Grab_ThumbnailURL"], (string)e6_GridItemTemp._DataRowReference["Artist"], (string)e6_GridItemTemp._DataRowReference["Grab_Title"]);
                        }
                    }
                }
            }
            else
            {
                foreach (DataRow DataRowTemp in Module_TableHolder.Database_Table.Rows)
                {
                    if ((bool)DataRowTemp["UPDL_Queued"])
                    {
                        if (Module_TableHolder.DownloadQueueContainsURL((string)DataRowTemp["Grab_MediaURL"]) || Module_Downloader.Download_AlreadyDownloaded.Contains((string)DataRowTemp["Grab_MediaURL"]))
                        {
                            continue;
                        }
                        else
                        {
                            Module_Downloader.AddDownloadQueueItem(DataRowTemp, (string)DataRowTemp["Grab_URL"], (string)DataRowTemp["Grab_MediaURL"], (string)DataRowTemp["Grab_ThumbnailURL"], (string)DataRowTemp["Artist"], (string)DataRowTemp["Grab_Title"]);
                        }
                    }
                }
            }
            Module_Downloader.UpdateTreeViewNodes();
            Module_Downloader.timer_Download.Start();
            BeginInvoke(new Action(() => { GB_Download.Enabled = true; }));
        }

        private void GB_Upload_Click(object sender, EventArgs e)
        {
            Module_Uploader.UploadBtnClicked();
        }

        #endregion









        #region "Download Tab"

        private void CCheckGroupBox_Download_Paint(object sender, PaintEventArgs e)
        {
            using (Pen TempPen = new Pen(Color.Black, 1))
            {
                Custom_CheckGroupBox cCGPCTemp = (Custom_CheckGroupBox)sender;
                e.Graphics.DrawLine(TempPen, new Point(0, 32), new Point(cCGPCTemp.Width, 32));
            }
        }

        private void BU_CancelAPIDL_Click(object sender, EventArgs e)
        {
            bU_CancelAPIDL.Enabled = false;
            Module_Downloader.e6APIDL_BGW.CancelAsync();
        }

        private void CCheckGroupBox_Download_CheckedChanged(object sender, EventArgs e)
        {
            if (cCheckGroupBox_Download.Checked)
            {
                Module_Downloader.timer_Download.Start();
            }

        }

        private void RadioButton_DL_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton WhichRadioButton = (RadioButton)sender;
            if (!WhichRadioButton.Text.Equals("") && WhichRadioButton.Checked && cCheckGroupBox_Download.Checked)
            {
                int NewValue = int.Parse(WhichRadioButton.Text);
                if (Module_Downloader.DLThreadsCount != NewValue)
                {
                    Module_Downloader.DLThreadsCount = NewValue;
                    Properties.Settings.Default.DLThreadsCount = NewValue;
                    Properties.Settings.Default.Save();
                    //Module_Downloader.Download_Start();
                }
            }
        }

        private void BU_ClearDLHistory_Click(object sender, EventArgs e)
        {
            Module_Downloader.Download_AlreadyDownloaded.Clear();
            bU_ClearDLHistory.Enabled = false;
            UIDrawController.SuspendDrawing(DownloadFLP_Downloaded);
            DownloadFLP_Downloaded.SuspendLayout();
            DownloadFLP_Downloaded.Controls.Clear();
            DownloadFLP_Downloaded.ResumeLayout();
            UIDrawController.ResumeDrawing(DownloadFLP_Downloaded);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        private void BU_DownloadFolder_Click(object sender, EventArgs e)
        {
            string Path = Properties.Settings.Default.DownloadsFolderLocation;
            Directory.CreateDirectory(Path);
            Process.Start(Path);
        }

        private void DownloadFLP_Downloaded_ControlAdded(object sender, ControlEventArgs e)
        {
            bU_ClearDLHistory.Enabled = true;
        }

        private void ToolStripMenuItem_RemoveDLNode_Click(object sender, EventArgs e)
        {
            if (WhichNodeIsIt.TreeView != null)
            {
                Module_TableHolder.DownloadQueueRemoveURL(WhichNodeIsIt.Text);
                Module_Downloader.UpdateTreeViewText();
                Module_Downloader.UpdateTreeViewNodes();
            }
        }

        private void ToolStripMenuItem_RemoveDLAll_Click(object sender, EventArgs e)
        {
            lock (Module_TableHolder.Download_Table)
            {
                Module_TableHolder.Download_Table.Rows.Clear();
            }
            Module_Downloader.TreeViewPage = 0;
            Module_Downloader.UpdateTreeViewNodes();
            Module_Downloader.UpdateTreeViewText();
        }

        private void BU_DownloadPageUp_Click(object sender, EventArgs e)
        {
            Module_Downloader.TreeViewPage -= 1;
            Module_Downloader.UpdateTreeViewNodes();
        }

        private void BU_DownloadPageDown_Click(object sender, EventArgs e)
        {
            Module_Downloader.TreeViewPage += 1;
            Module_Downloader.UpdateTreeViewNodes();
        }

        private void BU_SkipDLCache_Click(object sender, EventArgs e)
        {
            bU_SkipDLCache.Enabled = false;
            Module_Downloader.Load_DownloadFolderCache();
        }

        private void BU_ReverseDownload_Click(object sender, EventArgs e)
        {
            if (cCheckGroupBox_Download.Checked | Module_Downloader.e6APIDL_BGW.IsBusy)
            {
                MessageBox.Show("Download queue and API should be stopped before attempting to reverse the order.", "e621 ReBot");
                return;
            }
            Module_TableHolder.Download_Table = Module_TableHolder.ReverseDataTable(Module_TableHolder.Download_Table);
            Module_Downloader.UpdateTreeViewNodes();
        }

        #endregion









        #region "Jobs Tab"

        private void CCheckGroupBox_Grab_Paint(object sender, PaintEventArgs e)
        {
            using (Pen TempPen = new Pen(Color.Black, 1))
            {
                Custom_CheckGroupBox cCGPCTemp = (Custom_CheckGroupBox)sender;
                e.Graphics.DrawLine(TempPen, new Point(52, 32), new Point(52, cCGPCTemp.Height - 2));
            }
        }

        private void CCheckGroupBox_Jobs_Paint(object sender, PaintEventArgs e)
        {
            using (Pen TempPen = new Pen(Color.Black, 1))
            {
                Custom_CheckGroupBox cCGPCTemp = (Custom_CheckGroupBox)sender;
                e.Graphics.DrawLine(TempPen, new Point(0, 32), new Point(cCGPCTemp.Width, 32));
            }
        }

        private void ToolStripMenuItem_ClearReports_Click(object sender, EventArgs e)
        {
            textBox_Info.Clear();
        }

        private void ContextMenuStrip_cTreeView_Opening(object sender, CancelEventArgs e)
        {
            WhichNodeIsIt.ForeColor = Color.Orange;
        }

        private void ContextMenuStrip_cTreeView_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            WhichNodeIsIt.ForeColor = Color.LightSteelBlue;
        }

        private void CTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            label_GrabStatus.Focus();
        }

        private TreeNode WhichNodeIsIt;
        private void CTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            WhichNodeIsIt = e.Node;
        }

        private void CTreeView_GrabQueue_AfterCheck(object sender, TreeViewEventArgs e)
        {
            cTreeView_GrabQueue.AfterCheck -= CTreeView_GrabQueue_AfterCheck;

            int ChildNodesTotal;
            int CheckedChildNodesCount;

            if (e.Node.Parent != null)
            {
                int ParentNodeTag = e.Node.Parent.Tag != null ? int.Parse(e.Node.Parent.Tag.ToString()) : 0;
                CheckedChildNodesCount = ParentNodeTag + (e.Node.Checked ? 1 : -1);
                e.Node.Parent.Tag = CheckedChildNodesCount;
                UpdateParentNode_Tooltip(e.Node.Parent);
                e.Node.Parent.Checked = CheckedChildNodesCount != 0;
            }
            else
            {
                ChildNodesTotal = e.Node.Nodes.Count;
                foreach (TreeNode ChildNode in e.Node.Nodes)
                {
                    ChildNode.Checked = e.Node.Checked;
                }
                CheckedChildNodesCount = e.Node.Checked ? ChildNodesTotal : 0;
                e.Node.Tag = CheckedChildNodesCount;
                UpdateParentNode_Tooltip(e.Node);
            }

            cTreeView_GrabQueue.AfterCheck += CTreeView_GrabQueue_AfterCheck;
        }

        private void ToolStripMenuItem_RemoveNode_Click(object sender, EventArgs e)
        {
            TreeNode ParentNode = WhichNodeIsIt.Parent ?? WhichNodeIsIt;
            if (WhichNodeIsIt.TreeView.Name.Contains("Grab"))
            {
                if (ParentNode != null)
                {
                    if (ParentNode.Nodes.Count > 1)
                    {
                        if (WhichNodeIsIt.Checked)
                        {
                            WhichNodeIsIt.Checked = false;
                        }
                    }
                }
                WhichNodeIsIt.TreeView.Nodes.Remove(WhichNodeIsIt);
                if (ParentNode != null)
                {
                    UpdateParentNode_Tooltip(ParentNode);
                }
                return;
            }
            if (WhichNodeIsIt.TreeView.Name.Contains("Upload"))
            {
                DataRow DataRowTemp = (DataRow)ParentNode.Tag;
                if (ParentNode != null && ParentNode.Nodes.Count == 1)
                {
                    lock (Module_TableHolder.Upload_Table)
                    {
                        Module_TableHolder.Upload_Table.Rows.Remove(DataRowTemp);
                    }
                    ParentNode.Remove();
                }
                else
                {
                    lock (Module_TableHolder.Upload_Table)
                    {
                        DataRowTemp[WhichNodeIsIt.Text.Replace(" ", "")] = false;
                    }
                    WhichNodeIsIt.TreeView.Nodes.Remove(WhichNodeIsIt);
                }
                return;
            }
            WhichNodeIsIt.Remove();
        }

        private void ToolStripMenuItem_RemoveAll_Click(object sender, EventArgs e)
        {
            if (WhichNodeIsIt.TreeView.Name.Contains("Upload"))
            {
                lock (Module_TableHolder.Upload_Table)
                {
                    Module_TableHolder.Upload_Table.Rows.Clear();
                }
            }
            WhichNodeIsIt.TreeView.Nodes.Clear();
        }

        private void ToolStripMenuItem_ExpandAll_Click(object sender, EventArgs e)
        {
            if (WhichNodeIsIt.TreeView != null)
            {
                WhichNodeIsIt.TreeView.ExpandAll();
            }
        }

        private void ToolStripMenuItem_CollapseAll_Click(object sender, EventArgs e)
        {
            if (WhichNodeIsIt.TreeView != null)
            {
                WhichNodeIsIt.TreeView.CollapseAll();
            }
        }

        public void UpdateParentNode_Tooltip(TreeNode ParentNode)
        {

            int CheckedChildNodesCount = int.Parse(ParentNode.Tag.ToString());
            ParentNode.Tag = CheckedChildNodesCount;
            ParentNode.ToolTipText = string.Format("Selected: {0}/{1}", CheckedChildNodesCount, ParentNode.Nodes.Count);
            // ^ doesnt update while hovered.
        }

        private void CCheckGroupBox_Grab_CheckedChanged(object sender, EventArgs e)
        {
            ContextMenuStrip ContexMenuHolder = cCheckGroupBox_Grab.Checked ? null : contextMenuStrip_cTreeView;
            foreach (TreeNode ParentNode in cTreeView_GrabQueue.Nodes)
            {
                ParentNode.ContextMenuStrip = ContexMenuHolder;
                foreach (TreeNode Childnode in ParentNode.Nodes)
                {
                    Childnode.ContextMenuStrip = ContexMenuHolder;
                }
            }
        }

        private void CCheckGroupBox_Upload_CheckedChanged(object sender, EventArgs e)
        {
            if (Module_Uploader.timer_UploadDisable.Enabled)
            {
                cCheckGroupBox_Upload.CheckedChanged -= CCheckGroupBox_Upload_CheckedChanged;
                cCheckGroupBox_Upload.Checked = false;
                cCheckGroupBox_Upload.CheckedChanged += CCheckGroupBox_Upload_CheckedChanged;
                MessageBox.Show("There is no upload credit remaining, please wait for hourly limit to reset or for posts to be approved.", "e621 ReBot");
            }
            else
            {
                int NodeCount = cTreeView_UploadQueue.Nodes.Count;
                cCheckGroupBox_Upload.Text = "Uploader" + (NodeCount > 0 ? string.Format(" ({0})", NodeCount) : null);
                Module_Uploader.timer_Upload.Enabled = ((CheckBox)sender).Checked && !Module_Uploader.Upload_BGW.IsBusy;
            }
        }

        public void CCheckGroupBox_Retry_CheckedChanged(object sender, EventArgs e)
        {
            if (Module_Retry.timer_RetryDisable.Enabled)
            {
                Module_Retry.timer_Retry.Enabled = false;
                if (Module_Credits.Credit_Flag == 0 && Module_Credits.Credit_Notes == 0)
                {
                    Module_Retry.RetryDisable("Flag and Note limit reached, please wait for limits to reset.");
                    return;
                }

                if (Module_Credits.Credit_Flag == 0)
                {
                    Module_Retry.RetryDisable("Flag limit reached, please wait for limit to reset.");
                    return;
                }
                if (Module_Credits.Credit_Notes == 0)
                {
                    Module_Retry.RetryDisable("Note limit reached, please wait for limit to reset.");
                    return;
                }
            }
            else
            {
                Module_Retry.timer_Retry.Enabled = ((CheckBox)sender).Checked && !Module_Retry.Retry_BGW.IsBusy;
            }
        }

        private void RetryQueue_Save()
        {
            if (cTreeView_RetryQueue.Nodes.Count > 0)
            {
                Dictionary<string, object> Data = new Dictionary<string, object>();
                foreach (TreeNode RetryNode in cTreeView_RetryQueue.Nodes)
                {
                    Data.Add(RetryNode.Text, RetryNode.Tag);
                }
                string SerializedData = JsonConvert.SerializeObject(Data);
                Properties.Settings.Default.RetrySave = SerializedData;
            }
            else
            {
                Properties.Settings.Default.RetrySave = null;
            }
            Properties.Settings.Default.Save();
        }

        private void RetryQueue_Load()
        {
            if (!Properties.Settings.Default.RetrySave.Equals(""))
            {
                cTreeView_RetryQueue.BeginUpdate();
                JObject TempJson = JObject.Parse(Properties.Settings.Default.RetrySave);
                foreach (JProperty cItem in TempJson.Children())
                {
                    TreeNode TempNode = new TreeNode
                    {
                        Text = cItem.Name
                    };
                    if (!cItem.Value.ToString().Equals(""))
                    {
                        TempNode.Tag = cItem.Value.Value<double>();
                    }
                    cTreeView_RetryQueue.Nodes.Add(TempNode);
                }

                cTreeView_RetryQueue.EndUpdate();
            }
        }

        private void CCheckGroupBox_Convert_CheckedChanged(object sender, EventArgs e)
        {
            Module_Converter.timer_Conversion.Enabled = ((CheckBox)sender).Checked && !Module_Converter.Conversion_BGW.IsBusy;
        }

        private void ToolStripMenuItem_RemoveConversionNode_Click(object sender, EventArgs e)
        {
            if (WhichNodeIsIt.Index == 0 && Module_Converter.Conversion_BGW.IsBusy)
            {
                MessageBox.Show("You can't remove task from queue when conversion is already in progress.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                WhichNodeIsIt.Remove();
                int NodesRemaining = cTreeView_ConversionQueue.Nodes.Count;
                if (NodesRemaining > 0)
                {
                    cCheckGroupBox_Convert.Text = string.Format("Conversionist ({0})", NodesRemaining);
                }
                else
                {
                    cCheckGroupBox_Convert.Text = "Conversionist";
                }
            }
        }

        private void ToolStripMenuItem_RemoveConversionAll_Click(object sender, EventArgs e)
        {
            if (Module_Converter.Conversion_BGW.IsBusy)
            {
                MessageBox.Show("You can't remove tasks from queue while conversion is in progress.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                cTreeView_ConversionQueue.Nodes.Clear();
                cCheckGroupBox_Convert.Text = "Conversionist";
            }
        }

        #endregion









        #region "Info Tab"

        private void PictureBox_Discord_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/7ncEzah");
        }

        private void Label_Forum_Click(object sender, EventArgs e)
        {
            Process.Start("https://e621.net/forum_topics/25939");
        }

        private void PictureBox_GitHub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/e621-ReBot/e621-ReBot-v2");
        }

        private void Label_FlashPlayer_Click(object sender, EventArgs e)
        {
            Process.Start("https://get.adobe.com/flashplayer/otherversions/");
        }

        private void PictureBox_KoFi_Click(object sender, EventArgs e)
        {
            Process.Start("https://ko-fi.com/e621rebot");
        }

        #endregion









        #region "Settings"


        private void TabPage_Settings_Enter(object sender, EventArgs e)
        {
            TrackBar_Volume.Value = Module_VolumeControl.GetApplicationVolume();
        }

        private void BU_APIKey_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.API_Key.Equals(""))
            {
                Form_APIKey Form_APIKey_Temp = new Form_APIKey
                {
                    Owner = this
                };
                Form_APIKey_Temp.Show();
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to remove API Key?", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Properties.Settings.Default.API_Key = "";
                    Properties.Settings.Default.Save();
                    bU_APIKey.Text = "Add API Key";
                    MessageBox.Show("Uploading will remain disabled until you add the API Key again.", "e621 ReBot");
                }
            }
        }

        private void BU_APIKey_TextChanged(object sender, EventArgs e)
        {
            if (bU_APIKey.Text.Equals("Add API Key"))
            {
                GB_Upload.Enabled = false;
                bU_PoolWatcher.Enabled = false;
                bU_RefreshCredit.Enabled = false;
                if (Form_Preview._FormReference != null)
                {
                    Form_Preview._FormReference.PB_IQDBQ.Enabled = false;
                }
            }
            else
            {
                GB_Upload.Enabled = UploadCounter > 0;
                bU_PoolWatcher.Enabled = true;
                bU_RefreshCredit.Enabled = true;
                if (Form_Preview._FormReference != null)
                {
                    Form_Preview._FormReference.PB_IQDBQ.Enabled = true;
                }
            }
        }

        private void BU_NoteAdd_Click(object sender, EventArgs e)
        {
            new Form_Notes();
            Form_Notes._FormReference.Delete_Note_btn.Tag = "Save";
            Form_Notes._FormReference.ShowDialog();
        }

        private void BU_NoteRemove_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Note = "";
            Properties.Settings.Default.Save();
            bU_NoteAdd.Text = "Add Note";
            toolTip_Display.SetToolTip(bU_NoteAdd, "Leave a note for yourself that will appear when application starts.");
            bU_NoteRemove.Enabled = false;
        }

        private void CheckBox_RemoveBVAS_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RemoveBVAS = CheckBox_RemoveBVAS.Checked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_ExpandedDescription_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ExpandedDescription = CheckBox_ExpandedDescription.Checked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_AutocompleteTags_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutocompleteTags = CheckBox_AutocompleteTags.Checked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_BigMode_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LoadBigForm = CheckBox_BigMode.Checked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_DontFlag_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DontFlag = CheckBox_DontFlag.Checked;
            Properties.Settings.Default.Save();
        }

        private Timer FolderDialogFix;
        private void BU_DownloadFolderChange_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderDialogTemp = new FolderBrowserDialog()
            {
                SelectedPath = Application.StartupPath
            };

            //To make shitty FolderBrowserDialog scroll folder into view.
            FolderDialogFix = new Timer();
            FolderDialogFix.Tick += FolderDialogFix_Tick;
            FolderDialogFix.Start();
            if (FolderDialogTemp.ShowDialog() == DialogResult.OK)
            {
                label_DownloadsFolder.Text = FolderDialogTemp.SelectedPath + @"\Downloads\";
                Properties.Settings.Default.DownloadsFolderLocation = label_DownloadsFolder.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void FolderDialogFix_Tick(object sender, EventArgs e)
        {
            FolderDialogFix.Stop();
            SendKeys.Send("{TAB}{TAB}{RIGHT}");
            FolderDialogFix.Dispose();
        }

        private void Textbox_AutoTagsEditor_Enter(object sender, EventArgs e)
        {
            textbox_AutoTagsEditor.Text = null;
        }

        private void Textbox_AutoTagsEditor_Leave(object sender, EventArgs e)
        {
            textbox_AutoTagsEditor.Text = "Type here";
            bU_AutoTagsAdd.Enabled = false;
            bU_AutoTagsRemove.Enabled = false;
        }

        public AutocompleteMenu AutoTags = new AutocompleteMenu();
        public List<string> AutoTagsList_Tags = new List<string>();
        public List<MulticolumnAutocompleteItem> AutoTagsList_Pools = new List<MulticolumnAutocompleteItem>();
        private bool AutoTagsListChanged;
        private void Textbox_AutoTagsEditor_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        e.SuppressKeyPress = true;
                        break;
                    }

                case Keys.Escape:
                    {
                        if (!AutoTags.Visible)
                        {
                            textbox_AutoTagsEditor.Focus();
                        }
                        e.SuppressKeyPress = true;
                        break;
                    }
            }
        }

        private void Textbox_AutoTagsEditor_TextChanged(object sender, EventArgs e)
        {
            if (textbox_AutoTagsEditor.TextLength > 0)
            {
                if (AutoTagsList_Tags.Contains(textbox_AutoTagsEditor.Text))
                {
                    bU_AutoTagsAdd.Enabled = false;
                    bU_AutoTagsRemove.Enabled = true;
                }
                else
                {
                    bU_AutoTagsAdd.Enabled = true;
                    bU_AutoTagsRemove.Enabled = false;
                }
            }
            else
            {
                bU_AutoTagsAdd.Enabled = false;
                bU_AutoTagsRemove.Enabled = false;
            }
        }

        private void BU_AutoTagsAdd_Click(object sender, EventArgs e)
        {
            textbox_AutoTagsEditor.Text = textbox_AutoTagsEditor.Text.ToLower();
            Module_DB.DB_CreateCTRecord(textbox_AutoTagsEditor.Text);
            AutoTagsList_Tags.Add(textbox_AutoTagsEditor.Text);
            AutoTags.SetAutocompleteItems(AutoTagsList_Tags);
            cGroupBoxColored_AutocompleteTagEditor.Focus();
        }

        private void BU_AutoTagsRemove_Click(object sender, EventArgs e)
        {
            textbox_AutoTagsEditor.Text = textbox_AutoTagsEditor.Text.ToLower();
            Module_DB.DB_DeleteCTRecord(textbox_AutoTagsEditor.Text);
            AutoTagsList_Tags.Remove(textbox_AutoTagsEditor.Text);
            AutoTags.SetAutocompleteItems(AutoTagsList_Tags);
            AutoTagsListChanged = true;
            cGroupBoxColored_AutocompleteTagEditor.Focus();
        }

        private void BU_PoolWatcher_Click(object sender, EventArgs e)
        {
            new Form_PoolWatcher(bU_PoolWatcher.PointToScreen(Point.Empty), this);
            Form_PoolWatcher._FormReference.ShowDialog();
        }

        public List<string> Blacklist = new List<string>();
        private void BU_Blacklist_Click(object sender, EventArgs e)
        {
            new Form_Blacklist(bU_Blacklist.PointToScreen(Point.Empty), this);
            Form_Blacklist._FormReference.ShowDialog();
        }

        private void BU_RefreshCredit_Click(object sender, EventArgs e)
        {
            bU_RefreshCredit.Enabled = false;
            BackgroundWorker BGWRunnerTemp = new BackgroundWorker();
            BGWRunnerTemp.DoWork += Module_Credits.Check_Credit_All;
            BGWRunnerTemp.RunWorkerAsync();
        }

        private void UpdateDays_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton SenderTemp = (RadioButton)sender;
            if (SenderTemp.Checked)
            {
                Properties.Settings.Default.UpdateDays = int.Parse(SenderTemp.Text);
                Properties.Settings.Default.Save();
            }
        }

        private void NamingE6_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton SenderTemp = (RadioButton)sender;
            if (SenderTemp.Checked)
            {
                Properties.Settings.Default.Naming_e6 = int.Parse(SenderTemp.Tag.ToString());
                Properties.Settings.Default.Save();
            }
        }

        private void NamingWeb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton SenderTemp = (RadioButton)sender;
            if (SenderTemp.Checked)
            {
                Properties.Settings.Default.Naming_web = int.Parse(SenderTemp.Tag.ToString());
                Properties.Settings.Default.Save();
            }
        }

        private void TrackBar_Volume_Scroll(object sender, EventArgs e)
        {
            uint vol = (uint)(ushort.MaxValue / 100 * TrackBar_Volume.Value);
            Module_VolumeControl.waveOutSetVolume(IntPtr.Zero, (vol & 0xFFFF) | (vol << 16));
        }

        private void TrackBar_Volume_ValueChanged(object sender, EventArgs e)
        {
            cGroupBoxColored_Volume.Text = string.Format("Volume ({0}%)", TrackBar_Volume.Value.ToString());
        }

        private void Panel_CheckBoxOptions_Paint(object sender, PaintEventArgs e)
        {
            int TempHeightHolder = panel_CheckBoxOptions.Height - (CheckBox_DontFlag.Visible ? 0 : 24);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(0, 0), new Point(0, TempHeightHolder));
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(0, 0), new Point(9, 0));
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(0, TempHeightHolder), new Point(9, TempHeightHolder));
        }

        private void RadioButton_GridItemStyle_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton RadioButtonHolder = (RadioButton)sender;
            if (RadioButtonHolder.Checked)
            {
                Properties.Settings.Default.GridItemStyle = RadioButtonHolder.Name.Substring(RadioButtonHolder.Name.Length - 1);
                Properties.Settings.Default.Save();

                Form_Loader._customGIStyle = Properties.Settings.Default.GridItemStyle.Equals("0") ? GridItemStyle.Defult : GridItemStyle.Simple;

                if (flowLayoutPanel_Grid.Controls.Count > 0)
                {
                    foreach (e6_GridItem e6GridItemTemp in flowLayoutPanel_Grid.Controls)
                    {
                        e6GridItemTemp._Style = Form_Loader._customGIStyle;
                    }
                }
            }
        }

        private void RadioButton_ProgressBarStyle_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton RadioButtonHolder = (RadioButton)sender;
            if (RadioButtonHolder.Checked)
            {
                Properties.Settings.Default.ProgressBarStyle = RadioButtonHolder.Name.Substring(RadioButtonHolder.Name.Length - 1);
                Properties.Settings.Default.Save();

                Form_Loader._customPBStyle = Properties.Settings.Default.ProgressBarStyle.Equals("0") ? CustomPBStyle.Hex : CustomPBStyle.Round;

                if (cFlowLayoutPanel_ProgressBarHolder.Controls.Count > 0)
                {
                    foreach (Custom_ProgressBar cPBTemp in cFlowLayoutPanel_ProgressBarHolder.Controls)
                    {
                        cPBTemp.PBStyle = Form_Loader._customPBStyle;
                    }
                }

                if (flowLayoutPanel_Grid.Controls.Count > 0)
                {
                    foreach (e6_GridItem e6GridItemTemp in flowLayoutPanel_Grid.Controls)
                    {
                        e6GridItemTemp._Style = Form_Loader._customGIStyle;
                    }
                }
            }
        }

        private void RadioButton_GrabDisplayOrder_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton RadioButtonHolder = (RadioButton)sender;
            if (RadioButtonHolder.Checked)
            {
                Properties.Settings.Default.GrabDisplayOrder = RadioButtonHolder.Name.Substring(RadioButtonHolder.Name.Length - 1);
                Properties.Settings.Default.Save();
            }
        }

        private void BU_ResetSettings_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all settings?", "e621 ReBot", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Properties.Settings.Default.Reset();
                Cef.Shutdown();
                Directory.Delete(Application.StartupPath + @"\CefSharp Cache", true); // Delete CefSharp Cache Folder
                Close();
            }
        }

        private void BU_DLTags_Click(object sender, EventArgs e)
        {
            bU_DLTags.Enabled = false;
            bU_DLPools.Enabled = false;
            BackgroundWorker TmpBGW = new BackgroundWorker();
            TmpBGW.DoWork += DLWork_Tags;
            TmpBGW.RunWorkerAsync();
        }

        private void DLWork_Tags(object sender, DoWorkEventArgs e)
        {
            List<string> TagList = new List<string>();
            string[] TempStringHold;
            List<string> PageList;
            string TempTagHolder;
            for (int p = 1; p <= 999; p++)
            {
                string e6Tags_Response = Module_e621Info.e621InfoDownload("https://e621.net/tags.json?search[order]=count&limit=1000&page=" + p);
                if (e6Tags_Response.StartsWith("{", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                else
                {
                    // Uses ~50 extra MB at the end but is less than <500 initally and it's faster
                    TempStringHold = e6Tags_Response.Split(new[] { "\"name\":\"" }, StringSplitOptions.RemoveEmptyEntries);
                    PageList = new List<string>();
                    foreach (string StringSection in TempStringHold)
                    {
                        TempTagHolder = StringSection.Substring(0, StringSection.IndexOf("\""));
                        PageList.Add(Regex.Unescape(TempTagHolder));
                    }
                    PageList.RemoveAt(0);
                    TagList.AddRange(PageList);
                }
                BeginInvoke(new Action(() => { bU_DLTags.Text = string.Format("Downloaded {0}k Tags", p); }));
                Thread.Sleep(1000);
            }
            TagList = TagList.Distinct().ToList();
            File.WriteAllText("tags.txt", string.Join("✄", TagList));
            BeginInvoke(new Action(() =>
            {
                MessageBox.Show("Downloaded all tags.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                bU_DLTags.Text = "DL Tags";
                bU_DLTags.Enabled = true;
                bU_DLPools.Enabled = true;
            }
            ));
            AutoTagsList_Tags.Clear();
            Read_AutoTags();
        }

        private void Read_AutoTags()
        {
            AutoTagsList_Tags.AddRange(File.ReadAllText("tags.txt").Split(new string[] { "✄" }, StringSplitOptions.RemoveEmptyEntries));
            AutoTagsList_Tags.AddRange(Module_DB.DB_ReadCTTable());
            AutoTags.SetAutocompleteItems(AutoTagsList_Tags);

            List<string> CloneTags = new List<string>(AutoTagsList_Tags);
            CloneTags.Sort();
            AutoCompleteStringCollection TempACSC = new AutoCompleteStringCollection();
            TempACSC.AddRange(CloneTags.ToArray());
            BeginInvoke(new Action(() => { textbox_AutoTagsEditor.AutoCompleteCustomSource = TempACSC; }));
        }

        private void BU_DLPools_Click(object sender, EventArgs e)
        {
            bU_DLPools.Enabled = false;
            bU_DLTags.Enabled = false;
            BackgroundWorker TmpBGW = new BackgroundWorker();
            TmpBGW.DoWork += DLWork_Pools;
            TmpBGW.RunWorkerAsync();
        }

        private void DLWork_Pools(object sender, DoWorkEventArgs e)
        {
            List<string> PoolList = new List<string>();
            string Pool_ID;
            string Pool_Name;
            string[] TempStringHold;
            List<string> PageList = new List<string>();
            for (int p = 1; p <= 999; p++)
            {
                string e6Pool_Response = Module_e621Info.e621InfoDownload("https://e621.net/pools.json?search[order]=created_at&limit=1000&page=" + p);
                if (e6Pool_Response.Length < 10)
                {
                    break;
                }
                else
                {
                    TempStringHold = e6Pool_Response.Split(new[] { "},{\"" }, StringSplitOptions.RemoveEmptyEntries);
                    PageList = new List<string>();
                    foreach (string StringSection in TempStringHold)
                    {
                        Pool_ID = StringSection.Substring(StringSection.IndexOf("\"id\":") + 5);
                        Pool_ID = Pool_ID.Substring(0, Pool_ID.IndexOf("\"name\":") - 1);
                        Pool_Name = StringSection.Substring(StringSection.IndexOf("\"name\":") + 8);
                        Pool_Name = Pool_Name.Substring(0, Pool_Name.IndexOf("\"created_at\":") - 2);
                        PageList.Add(string.Format("{0},{1}", Pool_ID, Regex.Unescape(Pool_Name)));
                    }
                    PoolList.AddRange(PageList);
                }
                BeginInvoke(new Action(() => { bU_DLPools.Text = string.Format("Downloaded {0}k Pools", p); }));
                Thread.Sleep(1000);
            }
            File.WriteAllText("pools.txt", string.Join("✄", PoolList));
            BeginInvoke(new Action(() =>
            {
                MessageBox.Show("Downloaded all pools.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                bU_DLPools.Text = "DL Pools";
                bU_DLPools.Enabled = true;
                bU_DLTags.Enabled = true;
            }));
            AutoTagsList_Pools.Clear();
            Read_AutoPools();
        }

        private void Read_AutoPools()
        {
            string[] DataSplitter;
            int[] columnWidth = new int[] { 48, 272 };
            List<string> TempList = new List<string>(File.ReadAllText("pools.txt").Split(new string[] { "✄" }, StringSplitOptions.RemoveEmptyEntries));
            foreach (string PoolData in TempList)
            {
                DataSplitter = PoolData.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                AutoTagsList_Pools.Add(new MulticolumnAutocompleteItem(new[] { "#" + DataSplitter[0], DataSplitter[1].Replace("_", " ") }, "pool:" + DataSplitter[0], DataSplitter[1]) { ColumnWidth = columnWidth });
            }
        }

        private void CheckBox_ConverterKeepOriginal_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Converter_KeepOriginal = CheckBox_ConverterKeepOriginal.Checked;
            Properties.Settings.Default.Save();
        }

        private void CheckBox_ConverterDontConvertVideos_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Converter_DontConvertVideos = CheckBox_ConverterDontConvertVideos.Checked;
            Properties.Settings.Default.Save();
        }

        private void TextBox_Delay_Enter(object sender, EventArgs e)
        {
            TextBox SenderTB = (TextBox)sender;
            SenderTB.Text = null;
        }

        private void TextBox_Delay_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter key pressed
            if (e.KeyCode == Keys.Enter)
            {
                cGroupBoxColored_ActionDelay.Focus();
                e.Handled = true;
                return;
            }
        }

        private void TextBox_Delay_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Back) return;

            // Don't allow anything that isn't a number
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            TextBox SenderTB = (TextBox)sender;
            // Don't allow 0 as first value
            if (SenderTB.Text.Length == 0 && e.KeyChar == '0')
            {
                e.Handled = true;
                return;
            }
        }

        private void TextBox_DelayGrabber_Leave(object sender, EventArgs e)
        {
            int TBValue;
            if (!int.TryParse(textBox_DelayGrabber.Text, out TBValue)) TBValue = 0;
            if (TBValue < 250) TBValue = 250;
            textBox_DelayGrabber.Text = TBValue.ToString();
            Properties.Settings.Default.DelayGrabber = TBValue;
            Properties.Settings.Default.Save();

            bool RestartTimeCheck = Module_Grabber.timer_Grab.Enabled;
            Module_Grabber.timer_Grab.Stop();
            Module_Grabber.timer_Grab.Interval = TBValue;
            if (RestartTimeCheck) Module_Grabber.timer_Grab.Start();
        }

        private void TextBox_DelayUploader_Leave(object sender, EventArgs e)
        {
            int TBValue;
            if (!int.TryParse(textBox_DelayUploader.Text, out TBValue)) TBValue = 0;
            if (TBValue < 1000) TBValue = 1000;
            textBox_DelayUploader.Text = TBValue.ToString();
            Properties.Settings.Default.DelayUploader = TBValue;
            Properties.Settings.Default.Save();

            bool RestartTimeCheck = Module_Uploader.timer_Upload.Enabled;
            Module_Uploader.timer_Upload.Stop();
            Module_Uploader.timer_Upload.Interval = TBValue;
            if (RestartTimeCheck) Module_Uploader.timer_Upload.Start();
        }

        private void TextBox_DelayDownload_Leave(object sender, EventArgs e)
        {
            int TBValue;
            if (!int.TryParse(textBox_DelayDownload.Text, out TBValue)) TBValue = 0;
            if (TBValue < 250) TBValue = 250;
            textBox_DelayDownload.Text = TBValue.ToString();
            Properties.Settings.Default.DelayDownload = TBValue;
            Properties.Settings.Default.Save();

            bool RestartTimeCheck = Module_Downloader.timer_Download.Enabled;
            Module_Downloader.timer_Download.Stop();
            Module_Downloader.timer_Download.Interval = TBValue;
            if (RestartTimeCheck) Module_Downloader.timer_Download.Start();
        }

        private void Label_DragDropConvert_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, label_DragDropConvert.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void Label_DragDropConvert_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Label_DragDropConvert_DragDrop(object sender, DragEventArgs e)
        {
            List<string> fileList = new List<string>();

            string[] DropList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string FilePath in DropList)
            {
                if (FilePath.ToLower().EndsWith(".mp4") || FilePath.ToLower().EndsWith(".swf")) fileList.Add(FilePath);
            }

            switch (fileList.Count)
            {
                case 0:
                    {
                        MessageBox.Show("No supported video files detected.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }

                case 1:
                    {
                        Module_FFmpeg.DragDropConvert(fileList[0]);
                        break;
                    }

                default:
                    {
                        MessageBox.Show("Can only convert one video at a time.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }

            }
        }

        private void BU_GetGenders_Click(object sender, EventArgs e)
        {
            List<string> GenderStringList = new List<string>();

            foreach (string GenderTag in new string[] { "ambiguous_gender", "male", "female", "intersex" })
            {
                GenderStringList.Add(GenderTag);

                //add all tags that are alliased to this tag
                JArray GenderJArray = JArray.Parse(Module_e621Info.e621InfoDownload("https://e621.net/tag_aliases.json?search[name_matches]=" + GenderTag)); //name_matches~=consequent_name
                Thread.Sleep(1000);
                foreach (JToken GenderAlias in GenderJArray)
                {
                    if (GenderAlias["status"].Value<string>().Equals("active")) GenderStringList.Add(GenderAlias["antecedent_name"].Value<string>());
                }

                //add all tags that implicate this tag
                GenderJArray = JArray.Parse(Module_e621Info.e621InfoDownload("https://e621.net/tag_implications.json?search[name_matches]=" + GenderTag)); //name_matches~=consequent_name
                Thread.Sleep(1000);
                foreach (JToken GenderImplications in GenderJArray)
                {
                    if (GenderImplications["status"].Value<string>().Equals("active"))
                    {
                        string SubGenderTag = GenderImplications["antecedent_name"].Value<string>();

                        GenderStringList.Add(SubGenderTag);

                        //add all tags that are alliased to this tag
                        string GenderSubAliasString = Module_e621Info.e621InfoDownload("https://e621.net/tag_aliases.json?search[name_matches]=" + SubGenderTag);
                        Thread.Sleep(1000);
                        if (GenderSubAliasString.Substring(0, 1).Equals("["))
                        {
                            JArray SubGenderJArray = JArray.Parse(GenderSubAliasString);
                            foreach (JToken SubGenderAlias in SubGenderJArray)
                            {
                                if (SubGenderAlias["status"].Value<string>().Equals("active")) GenderStringList.Add(SubGenderAlias["antecedent_name"].Value<string>());
                            }

                            //add all tags that implicate this tag
                            string GenderSubImplicationsString = Module_e621Info.e621InfoDownload("https://e621.net/tag_implications.json?search[name_matches]=" + SubGenderTag);
                            Thread.Sleep(1000);
                            if (GenderSubImplicationsString.Substring(0, 1).Equals("["))
                            {
                                SubGenderJArray = JArray.Parse(GenderSubImplicationsString);
                                foreach (JToken SubGenderImplications in SubGenderJArray)
                                {
                                    if (SubGenderImplications["status"].Value<string>().Equals("active"))
                                    {
                                        GenderStringList.Add(SubGenderImplications["antecedent_name"].Value<string>());

                                        //add all tags that are alliased to this tag
                                        string GenderSub2AliasString = Module_e621Info.e621InfoDownload("https://e621.net/tag_aliases.json?search[name_matches]=" + GenderStringList.Last());
                                        Thread.Sleep(1000);
                                        if (GenderSub2AliasString.Substring(0, 1).Equals("["))
                                        {
                                            JArray Sub2GenderJArray = JArray.Parse(GenderSub2AliasString);
                                            foreach (JToken Sub2GenderAlias in Sub2GenderJArray)
                                            {
                                                if (Sub2GenderAlias["status"].Value<string>().Equals("active")) GenderStringList.Add(Sub2GenderAlias["antecedent_name"].Value<string>());
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            GenderStringList.Distinct();
            File.WriteAllText("genders.txt", string.Join("✄", GenderStringList));
            MessageBox.Show("Downloaded all genders.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public List<string> Gender_Tags = new List<string>();
        private void Read_Genders()
        {
            Gender_Tags.AddRange(Properties.Resources.genders.Split(new string[] { "✄" }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void BU_AppData_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "e621_ReBot_v2"));
        }

        #endregion









        #region "Puzzle Game"

        private void Panel_GameStart_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel_GameStart.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void PB_GameThumb_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pB_GameThumb.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void GB_StartGame_Click(object sender, EventArgs e)
        {
            gamePanel_Main.Enabled = true;
            gamePanel_Main.LoadPuzzle();
        }

        private void GB_RestartGame_Click(object sender, EventArgs e)
        {
            gamePanel_Main.ResetPuzzle();
        }

        private void CC_GameThumb_CheckedChanged(object sender, EventArgs e)
        {
            pB_GameThumb.Visible = CC_GameThumb.Checked;
        }

        private void LabelPuzzle_SelectedPost_Click(object sender, EventArgs e)
        {
            string e6Post = "https://e621.net/post/show/" + labelPuzzle_SelectedPost.Tag.ToString();
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                Process.Start(e6Post);
            }
            else
            {
                QuickButtonPanel.Visible = false;
                panel_Browser.Visible = true;
                Form_Loader._FormReference.BringToFront();
                Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = 0;
                if (!Module_CefSharp.CefSharpBrowser.Address.Equals(e6Post)) Module_CefSharp.CefSharpBrowser.Load(e6Post);
            }
        }

        #endregion
    }
}
