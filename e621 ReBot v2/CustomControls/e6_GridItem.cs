using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace e621_ReBot_v2.CustomControls
{
    public enum GridItemStyle
    {
        Defult,
        Simple
    }

    public partial class e6_GridItem : UserControl
    {
        private GridItemStyle giStyle = GridItemStyle.Defult;
        [Category("Appearance")]
        [Browsable(true)]
        [Description("Set Grid Item style")]
        public GridItemStyle _Style
        {
            get
            {
                return giStyle;
            }

            set
            {
                giStyle = value;
                cLabel_Rating.Visible = giStyle == GridItemStyle.Defult;
                cPanel_Rating.Visible = giStyle != GridItemStyle.Defult;
                Invalidate();
            }
        }



        public e6_GridItem()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            pictureBox_ImageHolder.SizeMode = PictureBoxSizeMode.CenterImage;

            pictureBox_ImageHolder.Cursor = Form_Loader.Cursor_Default;
            cLabel_Rating.Cursor = Form_Loader.Cursor_Default;
            cLabel_isSuperior.Cursor = Form_Loader.Cursor_ReBotNav;
            cLabel_TagWarning.Cursor = Form_Loader.Cursor_Default;
            cLabel_isUploaded.Cursor = Form_Loader.Cursor_ReBotNav;
            cPanel_Rating.Cursor = Form_Loader.Cursor_Default;

            SuspendLayout();
            pictureBox_ImageHolder.Parent = this;
            cLabel_Rating.Parent = pictureBox_ImageHolder;
            cLabel_Rating.Location = new Point(cLabel_Rating.Location.X - 2, cLabel_Rating.Location.Y - 2);
            cLabel_isSuperior.Parent = pictureBox_ImageHolder;
            cLabel_isSuperior.Location = new Point(cLabel_isSuperior.Location.X - 2, cLabel_isSuperior.Location.Y - 2);
            cLabel_TagWarning.Parent = pictureBox_ImageHolder;
            cLabel_TagWarning.Location = new Point(cLabel_TagWarning.Location.X - 2, cLabel_TagWarning.Location.Y - 2);
            cLabel_isUploaded.Parent = pictureBox_ImageHolder;
            cLabel_isUploaded.Location = new Point(cLabel_isUploaded.Location.X - 2, cLabel_isUploaded.Location.Y - 2);
            cPanel_Rating.Parent = pictureBox_ImageHolder;
            cPanel_Rating.Location = new Point(cPanel_Rating.Location.X - 2, cPanel_Rating.Location.Y - 2);
            ResumeLayout();

            DoubleClickTimerCheck = new Timer
            {
                Interval = 500
            };
            DoubleClickTimerCheck.Tick += DoubleClickTimerCheck_Tick;

            pictureBox_ImageHolder.MouseDown += E6_GridItem_MouseDown;
            cLabel_Rating.MouseDown += E6_GridItem_MouseDown;
            cLabel_TagWarning.MouseDown += E6_GridItem_MouseDown;

            //ChangePBRegion();
        }



        public DataRow _DataRowReference { get; set; }
        private void E6_GridItem_Load(object sender, EventArgs e)
        {
            _Style = Form_Loader._customGIStyle;
            if (_DataRowReference != null)
            {
                if (_DataRowReference["Thumbnail_Image"] != DBNull.Value)
                {
                    if (_DataRowReference["Thumbnail_FullInfo"] != DBNull.Value)
                    {
                        pictureBox_ImageHolder.Image = (Image)_DataRowReference["Thumbnail_Image"];
                    }
                    else
                    {
                        LoadImage();
                    }
                }
                cLabel_isSuperior.Visible = _DataRowReference["Inferior_ID"] != DBNull.Value;
            }
        }

        private void CLabel_isUploaded_TextChanged(object sender, EventArgs e)
        {
            cLabel_isUploaded.TextChanged -= CLabel_isUploaded_TextChanged;
            cLabel_isUploaded.Text = "#" + cLabel_isUploaded.Text;
            cLabel_isUploaded.Visible = true;
            cLabel_isUploaded.TextChanged += CLabel_isUploaded_TextChanged;
        }

        private void CLabel_isUploaded_Click(object sender, EventArgs e)
        {
            string e6Post = "https://e621.net/post/show/" + cLabel_isUploaded.Text.Remove(0, 1);
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                Process.Start(e6Post);
            }
            else
            {
                Form_Loader._FormReference.BringToFront();
                Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = 0;
                if (!Module_CefSharp.CefSharpBrowser.Address.Equals(e6Post))
                {
                    Module_CefSharp.CefSharpBrowser.Load(e6Post);
                }
            }
        }



        public bool _IsSelectedCheck { get; set; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ControlSnapshot == null)
            {
                if (_IsSelectedCheck)
                {
                    ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.DarkOrange, 2, ButtonBorderStyle.Solid, Color.DarkOrange, 2, ButtonBorderStyle.Solid, Color.DarkOrange, 2, ButtonBorderStyle.Solid, Color.DarkOrange, 2, ButtonBorderStyle.Solid);
                }
                else
                {
                    ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.SteelBlue, ButtonBorderStyle.Solid);
                }
            }
        }



        private Timer DoubleClickTimerCheck;
        private void E6_GridItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    Module_CefSharp.CefSharpBrowser.Load(_DataRowReference["Grab_URL"].ToString());
                    Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = 0;
                    return;
                }

                if (ModifierKeys.HasFlag(Keys.Alt))
                {
                    Process.Start(_DataRowReference["Grab_URL"].ToString());
                    return;
                }

                //If Application.OpenForms.OfType(Of Form).Contains(Form_Tagger) Then Form_Tagger.Close()

                if (_IsSelectedCheck)
                {
                    if (DoubleClickTimerCheck.Enabled)
                    {
                        DoubleClicked();
                    }
                    else
                    {
                        Unselect_e6GridItem();
                        DoubleClickTimerCheck.Start();
                    }
                }
                else
                {
                    if (DoubleClickTimerCheck.Enabled)
                    {
                        Select_e6GridItem();
                        DoubleClicked();
                    }
                    else
                    {
                        Select_e6GridItem();
                    }
                }
            }
        }

        public void Select_e6GridItem()
        {
            if (Form_Loader._FormReference._Selected_e6GridItem != null)
            {
                Form_Loader._FormReference._Selected_e6GridItem.Unselect_e6GridItem();
            }

            if (_IsSelectedCheck)
            {
                _IsSelectedCheck = false;
            }
            else
            {
                _IsSelectedCheck = true;
                Form_Loader._FormReference._Selected_e6GridItem = this;
            }
            DoubleClickTimerCheck.Start();
            Refresh();
        }

        public void Unselect_e6GridItem()
        {
            Form_Loader._FormReference._Selected_e6GridItem._IsSelectedCheck = false;
            Form_Loader._FormReference._Selected_e6GridItem = null;
            DoubleClickTimerCheck.Stop();
            Refresh();
        }

        private void DoubleClicked()
        {
            DoubleClickTimerCheck.Stop();

            string Nav2URL = _DataRowReference["Grab_MediaURL"].ToString();
            if (Nav2URL.ToLower().EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) | Nav2URL.ToLower().EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
            {
                Nav2URL = _DataRowReference["Grab_ThumbnailURL"].ToString();
            }

            if (Form_Preview._FormReference == null)
            {
                new Form_Preview();
            }
            Form_Preview._FormReference.Preview_RowHolder = _DataRowReference;
            Form_Preview._FormReference.Preview_RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(_DataRowReference);
            Form_Preview._FormReference.URL2Navigate = Nav2URL;
            Form_Preview._FormReference.NavURL(Nav2URL);
            Form_Preview._FormReference.BringToFront();
            if (Form_Preview._FormReference.WindowState == FormWindowState.Minimized)
            {
                Form_Preview._FormReference.WindowState = FormWindowState.Normal;
            }
            Form_Preview._FormReference.Show();
        }

        private void DoubleClickTimerCheck_Tick(object sender, EventArgs e)
        {
            DoubleClickTimerCheck.Stop();
        }

        private void CCheckBox_UPDL_CheckedChanged(object sender, EventArgs e)
        {
            bool cCheckBox_State = cCheckBox_UPDL.Checked;
            DataRow DataRowTemp = _DataRowReference;
            if (cCheckBox_State && _DataRowReference["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(ref DataRowTemp, false))
            {
                //cCheckBox_UPDL.Checked = false;
            }
            else
            {
                if (!cLabel_isUploaded.Visible)
                {
                    Form_Loader._FormReference.UploadCounter += cCheckBox_State ? 1 : -1;
                }
            }

            if (!Properties.Settings.Default.API_Key.Equals(""))
            {
                Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
            }
            Form_Loader._FormReference.DownloadCounter += cCheckBox_State ? 1 : -1;
            Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;

            _DataRowReference["UPDL_Queued"] = cCheckBox_State;
            if (Form_Preview._FormReference != null && Form_Preview._FormReference.Preview_RowHolder == _DataRowReference)
            {
                Form_Preview._FormReference.PB_Upload.BackColor = cCheckBox_State ? Color.LimeGreen : Color.FromArgb(0, 45, 90);
            }
        }



        private void CLabel_isSuperior_Click(object sender, EventArgs e)
        {
            string e6Post = "https://e621.net/post/show/" + _DataRowReference["Inferior_ID"].ToString();
            if (ModifierKeys.HasFlag(Keys.Alt))
            {
                Process.Start(e6Post);
            }
            else
            {
                Form_Loader._FormReference.BringToFront();
                Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = 0;
                if (!Module_CefSharp.CefSharpBrowser.Address.Equals(e6Post))
                {
                    Module_CefSharp.CefSharpBrowser.Load(e6Post);
                }
            }
        }



        private string giRating = "E";
        public string _Rating
        {
            get
            {
                return giRating;
            }

            set
            {
                giRating = value;
                cLabel_Rating.Text = giRating;
                _DataRowReference["Upload_Rating"] = giRating;
                cLabel_Rating.Visible = giStyle == GridItemStyle.Defult;
                cPanel_Rating.Visible = giStyle != GridItemStyle.Defult;

                switch (giRating)
                {
                    case "E":
                        {
                            cLabel_Rating.ForeColor = Color.Red;
                            cPanel_Rating.BackColor = Color.Red;
                            break;
                        }

                    case "Q":
                        {
                            cLabel_Rating.ForeColor = Color.Gold;
                            cPanel_Rating.BackColor = Color.Gold;
                            break;
                        }

                    default:
                        {
                            cLabel_Rating.ForeColor = Color.LimeGreen;
                            cPanel_Rating.BackColor = Color.LimeGreen;
                            break;
                        }

                }
                Invalidate();
            }
        }



        public void LoadImage()
        {
            pictureBox_ImageHolder.BackgroundImage = null;
            //if (pictureBox_ImageHolder.Image != null)
            //{
            //    pictureBox_ImageHolder.Image.Dispose();
            //}
            pictureBox_ImageHolder.Image = Module_Grabber.WriteImageInfo(_DataRowReference);
        }



        private Bitmap ControlSnapshot;
        private Bitmap BackgroundRemovalImage;
        private void Animation_Remove()
        {
            for (int i = 0; i < 12; i++)
            {
                using (Graphics TempGraphics = Graphics.FromImage(BackgroundRemovalImage))
                {
                    TempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    TempGraphics.InterpolationMode = InterpolationMode.High;
                    TempGraphics.CompositingQuality = CompositingQuality.HighQuality;
                    TempGraphics.Clear(Color.Transparent);
                    TempGraphics.DrawImage(ControlSnapshot, new Rectangle(new Point(8 * i, 8 * i), new Size(Width - 17 * i, Height - 17 * i)));
                }
                BackgroundImage = BackgroundRemovalImage;
                Invoke(new Action(() => { Refresh(); })); 
                Thread.Sleep(1);
            }
            Invoke(new Action(() => 
            {
                ControlSnapshot.Dispose();
                BackgroundRemovalImage.Dispose();
                lock (Module_TableHolder.Database_Table)
                {
                    _DataRowReference.Delete();
                }
                Dispose();
            }));
        }

        public void StartAnimation_Remove()
        {
            ControlSnapshot = new Bitmap(Width, Height);
            BackgroundRemovalImage = new Bitmap(Width, Height);
            DrawToBitmap(ControlSnapshot, ClientRectangle);
            RemoveControls();
            new Thread(Animation_Remove).Start();
        }

        private void RemoveControls()
        {
            cPanel_Rating.Dispose();
            cLabel_isSuperior.Dispose();
            cLabel_TagWarning.Dispose();
            cLabel_isUploaded.Dispose();
            cCheckBox_UPDL.Dispose();
            cLabel_Rating.Dispose();
            if (pictureBox_ImageHolder.BackgroundImage != null)
            {
                pictureBox_ImageHolder.BackgroundImage.Dispose();
            }
            if (pictureBox_ImageHolder.Image != null)
            {
                pictureBox_ImageHolder.Image.Dispose();
            }
            pictureBox_ImageHolder.Dispose();
        }



        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
