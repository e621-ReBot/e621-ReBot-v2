namespace e621_ReBot_v2.Forms
{
    partial class Form_Menu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Menu));
            this.Menu_FlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.MB_Browser = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_Grid = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_Jobs = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_Download = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_Info = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_Settings = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_PuzzleGame = new e621_ReBot_v2.CustomControls.Button_Menu();
            this.MB_MenuClose = new e621_ReBot_v2.CustomControls.Button_MenuClose();
            this.timer_FadeIn = new System.Windows.Forms.Timer(this.components);
            this.timer_FadeOut = new System.Windows.Forms.Timer(this.components);
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.Menu_FlowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Browser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Jobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Download)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Info)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Settings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_PuzzleGame)).BeginInit();
            this.SuspendLayout();
            // 
            // Menu_FlowLayoutPanel
            // 
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Browser);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Grid);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Jobs);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Download);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Info);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_Settings);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_PuzzleGame);
            this.Menu_FlowLayoutPanel.Controls.Add(this.MB_MenuClose);
            this.Menu_FlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Menu_FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Menu_FlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.Menu_FlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.Menu_FlowLayoutPanel.Name = "Menu_FlowLayoutPanel";
            this.Menu_FlowLayoutPanel.Size = new System.Drawing.Size(40, 360);
            this.Menu_FlowLayoutPanel.TabIndex = 8;
            // 
            // MB_Browser
            // 
            this.MB_Browser.BackColor = System.Drawing.Color.Transparent;
            this.MB_Browser.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Browser;
            this.MB_Browser.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Browser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Browser.Location = new System.Drawing.Point(4, 8);
            this.MB_Browser.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Browser.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Browser.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Browser.Name = "MB_Browser";
            this.MB_Browser.Size = new System.Drawing.Size(34, 31);
            this.MB_Browser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Browser.TabIndex = 8;
            this.MB_Browser.TabStop = false;
            this.MB_Browser.Tag = "0";
            this.toolTip_Display.SetToolTip(this.MB_Browser, "Webbrowser");
            // 
            // MB_Grid
            // 
            this.MB_Grid.BackColor = System.Drawing.Color.Transparent;
            this.MB_Grid.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Grid;
            this.MB_Grid.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Grid.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Grid.Location = new System.Drawing.Point(4, 55);
            this.MB_Grid.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Grid.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Grid.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Grid.Name = "MB_Grid";
            this.MB_Grid.Size = new System.Drawing.Size(34, 31);
            this.MB_Grid.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Grid.TabIndex = 9;
            this.MB_Grid.TabStop = false;
            this.MB_Grid.Tag = "1";
            this.toolTip_Display.SetToolTip(this.MB_Grid, "Grid");
            this.MB_Grid.Visible = false;
            // 
            // MB_Jobs
            // 
            this.MB_Jobs.BackColor = System.Drawing.Color.Transparent;
            this.MB_Jobs.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Jobs;
            this.MB_Jobs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Jobs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Jobs.Location = new System.Drawing.Point(4, 102);
            this.MB_Jobs.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Jobs.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Jobs.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Jobs.Name = "MB_Jobs";
            this.MB_Jobs.Size = new System.Drawing.Size(34, 31);
            this.MB_Jobs.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Jobs.TabIndex = 11;
            this.MB_Jobs.TabStop = false;
            this.MB_Jobs.Tag = "2";
            this.toolTip_Display.SetToolTip(this.MB_Jobs, "Jobs");
            // 
            // MB_Download
            // 
            this.MB_Download.BackColor = System.Drawing.Color.Transparent;
            this.MB_Download.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Download;
            this.MB_Download.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Download.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Download.Location = new System.Drawing.Point(4, 149);
            this.MB_Download.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Download.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Download.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Download.Name = "MB_Download";
            this.MB_Download.Size = new System.Drawing.Size(34, 31);
            this.MB_Download.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Download.TabIndex = 10;
            this.MB_Download.TabStop = false;
            this.MB_Download.Tag = "3";
            this.toolTip_Display.SetToolTip(this.MB_Download, "Downloads");
            // 
            // MB_Info
            // 
            this.MB_Info.BackColor = System.Drawing.Color.Transparent;
            this.MB_Info.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Info;
            this.MB_Info.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Info.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Info.Location = new System.Drawing.Point(4, 196);
            this.MB_Info.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Info.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Info.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Info.Name = "MB_Info";
            this.MB_Info.Size = new System.Drawing.Size(34, 31);
            this.MB_Info.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Info.TabIndex = 12;
            this.MB_Info.TabStop = false;
            this.MB_Info.Tag = "4";
            this.toolTip_Display.SetToolTip(this.MB_Info, "Info");
            // 
            // MB_Settings
            // 
            this.MB_Settings.BackColor = System.Drawing.Color.Transparent;
            this.MB_Settings.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_Settings;
            this.MB_Settings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_Settings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_Settings.Location = new System.Drawing.Point(4, 243);
            this.MB_Settings.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_Settings.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_Settings.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_Settings.Name = "MB_Settings";
            this.MB_Settings.Size = new System.Drawing.Size(34, 31);
            this.MB_Settings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_Settings.TabIndex = 13;
            this.MB_Settings.TabStop = false;
            this.MB_Settings.Tag = "5";
            this.toolTip_Display.SetToolTip(this.MB_Settings, "Settings");
            // 
            // MB_PuzzleGame
            // 
            this.MB_PuzzleGame.BackColor = System.Drawing.Color.Transparent;
            this.MB_PuzzleGame.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.MenuButton_PuzzleGame;
            this.MB_PuzzleGame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_PuzzleGame.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_PuzzleGame.Location = new System.Drawing.Point(4, 290);
            this.MB_PuzzleGame.Margin = new System.Windows.Forms.Padding(4, 8, 8, 8);
            this.MB_PuzzleGame.MaximumSize = new System.Drawing.Size(34, 31);
            this.MB_PuzzleGame.MinimumSize = new System.Drawing.Size(34, 31);
            this.MB_PuzzleGame.Name = "MB_PuzzleGame";
            this.MB_PuzzleGame.Size = new System.Drawing.Size(34, 31);
            this.MB_PuzzleGame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MB_PuzzleGame.TabIndex = 15;
            this.MB_PuzzleGame.TabStop = false;
            this.MB_PuzzleGame.Tag = "6";
            this.toolTip_Display.SetToolTip(this.MB_PuzzleGame, "Puzzle Game");
            // 
            // MB_MenuClose
            // 
            this.MB_MenuClose.BackColor = System.Drawing.Color.Transparent;
            this.MB_MenuClose.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MB_MenuClose.BackgroundImage")));
            this.MB_MenuClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.MB_MenuClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MB_MenuClose.FlatAppearance.BorderSize = 0;
            this.MB_MenuClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.MB_MenuClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.MB_MenuClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MB_MenuClose.Location = new System.Drawing.Point(20, 337);
            this.MB_MenuClose.Margin = new System.Windows.Forms.Padding(20, 8, 4, 4);
            this.MB_MenuClose.MaximumSize = new System.Drawing.Size(18, 15);
            this.MB_MenuClose.MinimumSize = new System.Drawing.Size(18, 15);
            this.MB_MenuClose.Name = "MB_MenuClose";
            this.MB_MenuClose.Size = new System.Drawing.Size(18, 15);
            this.MB_MenuClose.TabIndex = 14;
            this.toolTip_Display.SetToolTip(this.MB_MenuClose, "Close Menu");
            this.MB_MenuClose.UseVisualStyleBackColor = false;
            this.MB_MenuClose.Click += new System.EventHandler(this.MB_MenuClose_Click);
            // 
            // timer_FadeIn
            // 
            this.timer_FadeIn.Interval = 16;
            this.timer_FadeIn.Tick += new System.EventHandler(this.Timer_FadeIn_Tick);
            // 
            // timer_FadeOut
            // 
            this.timer_FadeOut.Interval = 16;
            this.timer_FadeOut.Tick += new System.EventHandler(this.Timer_FadeOut_Tick);
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 0;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 20;
            this.toolTip_Display.ShowAlways = true;
            // 
            // Form_Menu
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.ForestGreen;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(40, 360);
            this.Controls.Add(this.Menu_FlowLayoutPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Menu";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form_Menu";
            this.TransparencyKey = System.Drawing.Color.ForestGreen;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Menu_FormClosed);
            this.Load += new System.EventHandler(this.Form_Menu_Load);
            this.Menu_FlowLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MB_Browser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Jobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Download)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Info)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_Settings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MB_PuzzleGame)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.FlowLayoutPanel Menu_FlowLayoutPanel;
        private CustomControls.Button_Menu MB_Browser;
        private CustomControls.Button_Menu MB_Jobs;
        private CustomControls.Button_Menu MB_Info;
        private CustomControls.Button_Menu MB_Settings;
        private System.Windows.Forms.Timer timer_FadeIn;
        private System.Windows.Forms.Timer timer_FadeOut;
        private System.Windows.Forms.ToolTip toolTip_Display;
        private CustomControls.Button_MenuClose MB_MenuClose;
        internal CustomControls.Button_Menu MB_Grid;
        internal CustomControls.Button_Menu MB_Download;
        private CustomControls.Button_Menu MB_PuzzleGame;
    }
}