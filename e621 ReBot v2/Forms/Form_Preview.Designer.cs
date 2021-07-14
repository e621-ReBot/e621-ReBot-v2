
namespace e621_ReBot_v2.Forms
{
    partial class Form_Preview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Preview));
            this.panel_Navigation = new System.Windows.Forms.Panel();
            this.PB_Next = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.PB_Previous = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.panel_Rating = new System.Windows.Forms.Panel();
            this.PB_Safe = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.PB_Questionable = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.PB_Explicit = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.flowLayoutPanel_DL = new System.Windows.Forms.FlowLayoutPanel();
            this.PB_Download = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.Label_DownloadWarning = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.Label_Download = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.PB_ViewFile = new System.Windows.Forms.Button();
            this.PB_LoadAllImages = new System.Windows.Forms.Button();
            this.PB_SauceNao = new System.Windows.Forms.Button();
            this.PB_IQDBQ = new System.Windows.Forms.Button();
            this.Label_Tags = new System.Windows.Forms.Label();
            this.timer_NavDelay = new System.Windows.Forms.Timer(this.components);
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.Label_AlreadyUploaded = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.PB_Upload = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.PB_Tagger = new e621_ReBot_v2.CustomControls.Button_BrowserSmall();
            this.panel_Search = new System.Windows.Forms.Panel();
            this.navCancelFix_Timer = new System.Windows.Forms.Timer(this.components);
            this.Pic_WebBrowser = new e621_ReBot_v2.CustomControls.WebBrowserWithZoom();
            this.panel_Navigation.SuspendLayout();
            this.panel_Rating.SuspendLayout();
            this.flowLayoutPanel_DL.SuspendLayout();
            this.panel_Search.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Navigation
            // 
            this.panel_Navigation.Controls.Add(this.PB_Next);
            this.panel_Navigation.Controls.Add(this.PB_Previous);
            this.panel_Navigation.Location = new System.Drawing.Point(2, 2);
            this.panel_Navigation.Margin = new System.Windows.Forms.Padding(0);
            this.panel_Navigation.Name = "panel_Navigation";
            this.panel_Navigation.Size = new System.Drawing.Size(60, 32);
            this.panel_Navigation.TabIndex = 0;
            // 
            // PB_Next
            // 
            this.PB_Next.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Next.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.BB_Forward_Small;
            this.PB_Next.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PB_Next.ButtonAngle = 11D;
            this.PB_Next.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Next.FlatAppearance.BorderSize = 0;
            this.PB_Next.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Next.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Next.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Next.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Next.Location = new System.Drawing.Point(26, 0);
            this.PB_Next.Margin = new System.Windows.Forms.Padding(0);
            this.PB_Next.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Next.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Next.Name = "PB_Next";
            this.PB_Next.Size = new System.Drawing.Size(32, 32);
            this.PB_Next.TabIndex = 2;
            this.PB_Next.Tag = "1";
            this.toolTip_Display.SetToolTip(this.PB_Next, "Navigate to next  image.\r\nYou can also use \"Arrow Down\" or \"Arrow Right\" key on k" +
        "eyboard.");
            this.PB_Next.UseVisualStyleBackColor = false;
            // 
            // PB_Previous
            // 
            this.PB_Previous.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Previous.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.BB_Backward_Small;
            this.PB_Previous.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PB_Previous.ButtonAngle = 11D;
            this.PB_Previous.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Previous.FlatAppearance.BorderSize = 0;
            this.PB_Previous.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Previous.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Previous.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Previous.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Previous.Location = new System.Drawing.Point(0, 0);
            this.PB_Previous.Margin = new System.Windows.Forms.Padding(0);
            this.PB_Previous.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Previous.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Previous.Name = "PB_Previous";
            this.PB_Previous.Size = new System.Drawing.Size(32, 32);
            this.PB_Previous.TabIndex = 1;
            this.PB_Previous.Tag = "-1";
            this.toolTip_Display.SetToolTip(this.PB_Previous, "Navigate to previous image.\r\nYou can also use \"Arrow Up\" or \"Arrow Left\" key on k" +
        "eyboard.");
            this.PB_Previous.UseVisualStyleBackColor = false;
            // 
            // panel_Rating
            // 
            this.panel_Rating.Controls.Add(this.PB_Safe);
            this.panel_Rating.Controls.Add(this.PB_Questionable);
            this.panel_Rating.Controls.Add(this.PB_Explicit);
            this.panel_Rating.Location = new System.Drawing.Point(71, 2);
            this.panel_Rating.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.panel_Rating.Name = "panel_Rating";
            this.panel_Rating.Size = new System.Drawing.Size(84, 32);
            this.panel_Rating.TabIndex = 3;
            // 
            // PB_Safe
            // 
            this.PB_Safe.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Safe.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PB_Safe.ButtonAngle = 11D;
            this.PB_Safe.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Safe.FlatAppearance.BorderSize = 0;
            this.PB_Safe.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LimeGreen;
            this.PB_Safe.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LimeGreen;
            this.PB_Safe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Safe.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Safe.ForeColor = System.Drawing.Color.LimeGreen;
            this.PB_Safe.Location = new System.Drawing.Point(52, 0);
            this.PB_Safe.Margin = new System.Windows.Forms.Padding(0);
            this.PB_Safe.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Safe.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Safe.Name = "PB_Safe";
            this.PB_Safe.Size = new System.Drawing.Size(32, 32);
            this.PB_Safe.TabIndex = 3;
            this.PB_Safe.Tag = "";
            this.PB_Safe.Text = "S";
            this.toolTip_Display.SetToolTip(this.PB_Safe, "Set image rating as Safe.\r\nYou can also use \"S\" key on keyboard (key placement on" +
        " QWERTY keyboard, not the actual character).");
            this.PB_Safe.UseVisualStyleBackColor = false;
            // 
            // PB_Questionable
            // 
            this.PB_Questionable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Questionable.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PB_Questionable.ButtonAngle = 11D;
            this.PB_Questionable.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Questionable.FlatAppearance.BorderSize = 0;
            this.PB_Questionable.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gold;
            this.PB_Questionable.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gold;
            this.PB_Questionable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Questionable.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Questionable.ForeColor = System.Drawing.Color.Gold;
            this.PB_Questionable.Location = new System.Drawing.Point(26, 0);
            this.PB_Questionable.Margin = new System.Windows.Forms.Padding(0);
            this.PB_Questionable.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Questionable.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Questionable.Name = "PB_Questionable";
            this.PB_Questionable.Size = new System.Drawing.Size(32, 32);
            this.PB_Questionable.TabIndex = 2;
            this.PB_Questionable.Tag = "";
            this.PB_Questionable.Text = "Q";
            this.toolTip_Display.SetToolTip(this.PB_Questionable, "Set image rating as Questionable.\r\nYou can also use \"Q\" key on keyboard (key plac" +
        "ement on QWERTY keyboard, not the actual character).");
            this.PB_Questionable.UseVisualStyleBackColor = false;
            // 
            // PB_Explicit
            // 
            this.PB_Explicit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Explicit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PB_Explicit.ButtonAngle = 11D;
            this.PB_Explicit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Explicit.FlatAppearance.BorderSize = 0;
            this.PB_Explicit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Red;
            this.PB_Explicit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
            this.PB_Explicit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Explicit.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Explicit.ForeColor = System.Drawing.Color.Red;
            this.PB_Explicit.Location = new System.Drawing.Point(0, 0);
            this.PB_Explicit.Margin = new System.Windows.Forms.Padding(0);
            this.PB_Explicit.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Explicit.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Explicit.Name = "PB_Explicit";
            this.PB_Explicit.Size = new System.Drawing.Size(32, 32);
            this.PB_Explicit.TabIndex = 1;
            this.PB_Explicit.Tag = "";
            this.PB_Explicit.Text = "E";
            this.PB_Explicit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip_Display.SetToolTip(this.PB_Explicit, "Set image rating as Explicit.\r\nYou can also use \"E\" key on keyboard (key placemen" +
        "t on QWERTY keyboard, not the actual character).");
            this.PB_Explicit.UseVisualStyleBackColor = false;
            // 
            // flowLayoutPanel_DL
            // 
            this.flowLayoutPanel_DL.Controls.Add(this.PB_Download);
            this.flowLayoutPanel_DL.Controls.Add(this.Label_DownloadWarning);
            this.flowLayoutPanel_DL.Controls.Add(this.Label_Download);
            this.flowLayoutPanel_DL.Controls.Add(this.PB_ViewFile);
            this.flowLayoutPanel_DL.Location = new System.Drawing.Point(243, 2);
            this.flowLayoutPanel_DL.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.flowLayoutPanel_DL.Name = "flowLayoutPanel_DL";
            this.flowLayoutPanel_DL.Size = new System.Drawing.Size(56, 32);
            this.flowLayoutPanel_DL.TabIndex = 5;
            // 
            // PB_Download
            // 
            this.PB_Download.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Download.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuIcon_Download;
            this.PB_Download.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PB_Download.ButtonAngle = 11D;
            this.PB_Download.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Download.FlatAppearance.BorderSize = 0;
            this.PB_Download.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gold;
            this.PB_Download.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gold;
            this.PB_Download.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Download.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Download.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_Download.Location = new System.Drawing.Point(12, 4);
            this.PB_Download.Margin = new System.Windows.Forms.Padding(12, 4, 0, 0);
            this.PB_Download.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Download.MinimumSize = new System.Drawing.Size(24, 24);
            this.PB_Download.Name = "PB_Download";
            this.PB_Download.Size = new System.Drawing.Size(24, 24);
            this.PB_Download.TabIndex = 5;
            this.PB_Download.Tag = "";
            this.toolTip_Display.SetToolTip(this.PB_Download, "Download Image.\r\nYou can also use \"D\" key on keyboard (key placement on QWERTY ke" +
        "yboard, not actual character).");
            this.PB_Download.UseVisualStyleBackColor = false;
            this.PB_Download.Click += new System.EventHandler(this.PB_Download_Click);
            // 
            // Label_DownloadWarning
            // 
            this.Label_DownloadWarning.BackColor = System.Drawing.Color.Transparent;
            this.Label_DownloadWarning.ExtraSize = 0;
            this.Label_DownloadWarning.Font = new System.Drawing.Font("Arial Black", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Label_DownloadWarning.ForeColor = System.Drawing.Color.Orange;
            this.Label_DownloadWarning.Location = new System.Drawing.Point(36, 0);
            this.Label_DownloadWarning.Margin = new System.Windows.Forms.Padding(0);
            this.Label_DownloadWarning.Name = "Label_DownloadWarning";
            this.Label_DownloadWarning.Size = new System.Drawing.Size(10, 30);
            this.Label_DownloadWarning.StokeSize = 3;
            this.Label_DownloadWarning.TabIndex = 0;
            this.Label_DownloadWarning.Text = "!";
            this.toolTip_Display.SetToolTip(this.Label_DownloadWarning, "This is {0}, you need to download it in order to view the animated version.");
            // 
            // Label_Download
            // 
            this.Label_Download.BackColor = System.Drawing.Color.Transparent;
            this.Label_Download.ExtraSize = 0;
            this.Label_Download.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Download.ForeColor = System.Drawing.Color.DarkOrange;
            this.Label_Download.Location = new System.Drawing.Point(0, 30);
            this.Label_Download.Margin = new System.Windows.Forms.Padding(0);
            this.Label_Download.Name = "Label_Download";
            this.Label_Download.Size = new System.Drawing.Size(56, 32);
            this.Label_Download.StokeSize = 3;
            this.Label_Download.TabIndex = 6;
            this.Label_Download.Text = "100%";
            this.toolTip_Display.SetToolTip(this.Label_Download, "Download (Orange) or Conversion (Purple) status.\r\n");
            // 
            // PB_ViewFile
            // 
            this.PB_ViewFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_ViewFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_ViewFile.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.PB_ViewFile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_ViewFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_ViewFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_ViewFile.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PB_ViewFile.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_ViewFile.Location = new System.Drawing.Point(16, 63);
            this.PB_ViewFile.Margin = new System.Windows.Forms.Padding(16, 1, 0, 0);
            this.PB_ViewFile.Name = "PB_ViewFile";
            this.PB_ViewFile.Size = new System.Drawing.Size(24, 30);
            this.PB_ViewFile.TabIndex = 102;
            this.PB_ViewFile.TabStop = false;
            this.PB_ViewFile.Text = "▶";
            this.toolTip_Display.SetToolTip(this.PB_ViewFile, "Click to view, Ctrl+Click to view file in folder.");
            this.PB_ViewFile.UseVisualStyleBackColor = false;
            this.PB_ViewFile.Visible = false;
            this.PB_ViewFile.VisibleChanged += new System.EventHandler(this.PB_ViewFile_VisibleChanged);
            this.PB_ViewFile.Click += new System.EventHandler(this.PB_ViewFile_Click);
            // 
            // PB_LoadAllImages
            // 
            this.PB_LoadAllImages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_LoadAllImages.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_LoadAllImages.Enabled = false;
            this.PB_LoadAllImages.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.PB_LoadAllImages.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_LoadAllImages.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_LoadAllImages.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_LoadAllImages.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_LoadAllImages.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_LoadAllImages.Location = new System.Drawing.Point(486, 8);
            this.PB_LoadAllImages.Margin = new System.Windows.Forms.Padding(0);
            this.PB_LoadAllImages.Name = "PB_LoadAllImages";
            this.PB_LoadAllImages.Size = new System.Drawing.Size(22, 22);
            this.PB_LoadAllImages.TabIndex = 114;
            this.PB_LoadAllImages.TabStop = false;
            this.PB_LoadAllImages.Text = "L";
            this.toolTip_Display.SetToolTip(this.PB_LoadAllImages, "Load all Images. Click again to stop.\r\n\r\nIt just navigates trough whole image lis" +
        "t to cache it for browser.");
            this.PB_LoadAllImages.UseVisualStyleBackColor = false;
            this.PB_LoadAllImages.Click += new System.EventHandler(this.PB_LoadAllImages_Click);
            // 
            // PB_SauceNao
            // 
            this.PB_SauceNao.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_SauceNao.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_SauceNao.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.PB_SauceNao.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_SauceNao.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_SauceNao.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_SauceNao.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_SauceNao.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_SauceNao.Location = new System.Drawing.Point(0, 0);
            this.PB_SauceNao.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.PB_SauceNao.Name = "PB_SauceNao";
            this.PB_SauceNao.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.PB_SauceNao.Size = new System.Drawing.Size(100, 31);
            this.PB_SauceNao.TabIndex = 113;
            this.PB_SauceNao.TabStop = false;
            this.PB_SauceNao.Text = "SauceNao";
            this.toolTip_Display.SetToolTip(this.PB_SauceNao, resources.GetString("PB_SauceNao.ToolTip"));
            this.PB_SauceNao.UseVisualStyleBackColor = false;
            // 
            // PB_IQDBQ
            // 
            this.PB_IQDBQ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_IQDBQ.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_IQDBQ.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.PB_IQDBQ.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_IQDBQ.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_IQDBQ.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_IQDBQ.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_IQDBQ.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_IQDBQ.Location = new System.Drawing.Point(104, 0);
            this.PB_IQDBQ.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.PB_IQDBQ.Name = "PB_IQDBQ";
            this.PB_IQDBQ.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.PB_IQDBQ.Size = new System.Drawing.Size(72, 31);
            this.PB_IQDBQ.TabIndex = 115;
            this.PB_IQDBQ.TabStop = false;
            this.PB_IQDBQ.Text = "IQDBQ";
            this.toolTip_Display.SetToolTip(this.PB_IQDBQ, resources.GetString("PB_IQDBQ.ToolTip"));
            this.PB_IQDBQ.UseVisualStyleBackColor = false;
            // 
            // Label_Tags
            // 
            this.Label_Tags.BackColor = System.Drawing.Color.Transparent;
            this.Label_Tags.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Label_Tags.Location = new System.Drawing.Point(0, 35);
            this.Label_Tags.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.Label_Tags.Name = "Label_Tags";
            this.Label_Tags.Size = new System.Drawing.Size(1084, 16);
            this.Label_Tags.TabIndex = 118;
            // 
            // timer_NavDelay
            // 
            this.timer_NavDelay.Tick += new System.EventHandler(this.Timer_NavDelay_Tick);
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 15000;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 100;
            // 
            // Label_AlreadyUploaded
            // 
            this.Label_AlreadyUploaded.BackColor = System.Drawing.Color.Transparent;
            this.Label_AlreadyUploaded.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Label_AlreadyUploaded.ExtraSize = 0;
            this.Label_AlreadyUploaded.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Label_AlreadyUploaded.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.Label_AlreadyUploaded.Location = new System.Drawing.Point(513, 7);
            this.Label_AlreadyUploaded.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.Label_AlreadyUploaded.Name = "Label_AlreadyUploaded";
            this.Label_AlreadyUploaded.Size = new System.Drawing.Size(330, 24);
            this.Label_AlreadyUploaded.StokeSize = 3;
            this.Label_AlreadyUploaded.TabIndex = 116;
            this.Label_AlreadyUploaded.Text = "Already uploaded as #0123456789";
            this.toolTip_Display.SetToolTip(this.Label_AlreadyUploaded, "Click to navigate to post.\r\nAlt+Click to open in your default browser.");
            this.Label_AlreadyUploaded.Visible = false;
            this.Label_AlreadyUploaded.TextChanged += new System.EventHandler(this.Label_AlreadyUploaded_TextChanged);
            this.Label_AlreadyUploaded.Click += new System.EventHandler(this.Label_AlreadyUploaded_Click);
            // 
            // PB_Upload
            // 
            this.PB_Upload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Upload.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.UP_Icon;
            this.PB_Upload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PB_Upload.ButtonAngle = 11D;
            this.PB_Upload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Upload.FlatAppearance.BorderSize = 0;
            this.PB_Upload.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Upload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Upload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Upload.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Upload.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_Upload.Location = new System.Drawing.Point(203, 2);
            this.PB_Upload.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.PB_Upload.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Upload.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Upload.Name = "PB_Upload";
            this.PB_Upload.Size = new System.Drawing.Size(32, 32);
            this.PB_Upload.TabIndex = 4;
            this.PB_Upload.Tag = "";
            this.toolTip_Display.SetToolTip(this.PB_Upload, "Toggle upload status.\r\n\"+\" or \"1\" will activate.\r\n\"-\" or \"0\" will deactivate.");
            this.PB_Upload.UseVisualStyleBackColor = false;
            this.PB_Upload.Click += new System.EventHandler(this.PB_Upload_Click);
            // 
            // PB_Tagger
            // 
            this.PB_Tagger.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.PB_Tagger.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PB_Tagger.ButtonAngle = 11D;
            this.PB_Tagger.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PB_Tagger.FlatAppearance.BorderSize = 0;
            this.PB_Tagger.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Tagger.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.PB_Tagger.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PB_Tagger.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.PB_Tagger.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.PB_Tagger.Location = new System.Drawing.Point(163, 2);
            this.PB_Tagger.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.PB_Tagger.MaximumSize = new System.Drawing.Size(32, 32);
            this.PB_Tagger.MinimumSize = new System.Drawing.Size(32, 32);
            this.PB_Tagger.Name = "PB_Tagger";
            this.PB_Tagger.Size = new System.Drawing.Size(32, 32);
            this.PB_Tagger.TabIndex = 1;
            this.PB_Tagger.Tag = "";
            this.PB_Tagger.Text = "T";
            this.toolTip_Display.SetToolTip(this.PB_Tagger, "Open Tagger. Press Ctrl to center it to screen.\r\nYou can also use \"T\" key on keyb" +
        "oard (key placement on QWERTY keyboard, not actual character).");
            this.PB_Tagger.UseVisualStyleBackColor = false;
            this.PB_Tagger.Click += new System.EventHandler(this.PB_Tagger_Click);
            // 
            // panel_Search
            // 
            this.panel_Search.Controls.Add(this.PB_SauceNao);
            this.panel_Search.Controls.Add(this.PB_IQDBQ);
            this.panel_Search.Location = new System.Drawing.Point(306, 2);
            this.panel_Search.Name = "panel_Search";
            this.panel_Search.Size = new System.Drawing.Size(180, 32);
            this.panel_Search.TabIndex = 119;
            // 
            // navCancelFix_Timer
            // 
            this.navCancelFix_Timer.Interval = 250;
            this.navCancelFix_Timer.Tick += new System.EventHandler(this.NavCancelFix_Timer_Tick);
            // 
            // Pic_WebBrowser
            // 
            this.Pic_WebBrowser.AllowWebBrowserDrop = false;
            this.Pic_WebBrowser.Location = new System.Drawing.Point(0, 50);
            this.Pic_WebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.Pic_WebBrowser.Name = "Pic_WebBrowser";
            this.Pic_WebBrowser.ScriptErrorsSuppressed = true;
            this.Pic_WebBrowser.ScrollBarsEnabled = false;
            this.Pic_WebBrowser.Size = new System.Drawing.Size(1084, 570);
            this.Pic_WebBrowser.TabIndex = 69;
            this.Pic_WebBrowser.TabStop = false;
            this.Pic_WebBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.Pic_WebBrowser_DocumentCompleted);
            this.Pic_WebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.Pic_WebBrowser_Navigated);
            this.Pic_WebBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.Pic_WebBrowser_Navigating);
            // 
            // Form_Preview
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1077, 590);
            this.Controls.Add(this.panel_Search);
            this.Controls.Add(this.Label_Tags);
            this.Controls.Add(this.Pic_WebBrowser);
            this.Controls.Add(this.Label_AlreadyUploaded);
            this.Controls.Add(this.PB_LoadAllImages);
            this.Controls.Add(this.flowLayoutPanel_DL);
            this.Controls.Add(this.PB_Upload);
            this.Controls.Add(this.PB_Tagger);
            this.Controls.Add(this.panel_Rating);
            this.Controls.Add(this.panel_Navigation);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(440, 240);
            this.Name = "Form_Preview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preview";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Preview_FormClosed);
            this.Load += new System.EventHandler(this.Form_Preview_Load);
            this.Shown += new System.EventHandler(this.Form_Preview_Shown);
            this.ResizeEnd += new System.EventHandler(this.Form_Preview_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form_Preview_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form_Preview_KeyUp);
            this.Resize += new System.EventHandler(this.Form_Preview_Resize);
            this.panel_Navigation.ResumeLayout(false);
            this.panel_Rating.ResumeLayout(false);
            this.flowLayoutPanel_DL.ResumeLayout(false);
            this.panel_Search.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_DL;
        internal System.Windows.Forms.Button PB_ViewFile;
        internal System.Windows.Forms.Button PB_LoadAllImages;
        internal System.Windows.Forms.Button PB_SauceNao;
        internal System.Windows.Forms.Button PB_IQDBQ;
        internal CustomControls.Custom_LabelWithStroke Label_AlreadyUploaded;
        internal System.Windows.Forms.Panel panel_Navigation;
        internal CustomControls.Button_BrowserSmall PB_Next;
        internal CustomControls.Button_BrowserSmall PB_Previous;
        internal System.Windows.Forms.Panel panel_Rating;
        internal CustomControls.Button_BrowserSmall PB_Safe;
        internal CustomControls.Button_BrowserSmall PB_Questionable;
        internal CustomControls.Button_BrowserSmall PB_Explicit;
        internal CustomControls.Button_BrowserSmall PB_Tagger;
        internal CustomControls.Button_BrowserSmall PB_Upload;
        internal CustomControls.Custom_LabelWithStroke Label_DownloadWarning;
        internal CustomControls.Button_BrowserSmall PB_Download;
        internal CustomControls.Custom_LabelWithStroke Label_Download;
        internal CustomControls.WebBrowserWithZoom Pic_WebBrowser;
        internal System.Windows.Forms.Label Label_Tags;
        private System.Windows.Forms.Timer timer_NavDelay;
        internal System.Windows.Forms.ToolTip toolTip_Display;
        internal System.Windows.Forms.Panel panel_Search;
        internal System.Windows.Forms.Timer navCancelFix_Timer;
    }
}