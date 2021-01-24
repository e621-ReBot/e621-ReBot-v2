
namespace e621_ReBot_v2.CustomControls
{
    partial class e6_DownloadItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DL_FolderIcon = new System.Windows.Forms.PictureBox();
            this.DL_ProgressBar = new e621_ReBot_v2.CustomControls.Custom_ProgressBar();
            this.picBox_ImageHolder = new System.Windows.Forms.PictureBox();
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.DL_FolderIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_ImageHolder)).BeginInit();
            this.SuspendLayout();
            // 
            // DL_FolderIcon
            // 
            this.DL_FolderIcon.BackColor = System.Drawing.Color.Transparent;
            this.DL_FolderIcon.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.FolderIconSmall;
            this.DL_FolderIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.DL_FolderIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DL_FolderIcon.Location = new System.Drawing.Point(61, 65);
            this.DL_FolderIcon.Name = "DL_FolderIcon";
            this.DL_FolderIcon.Size = new System.Drawing.Size(40, 32);
            this.DL_FolderIcon.TabIndex = 3;
            this.DL_FolderIcon.TabStop = false;
            this.DL_FolderIcon.Visible = false;
            this.DL_FolderIcon.Click += new System.EventHandler(this.DL_FolderIcon_Click);
            // 
            // DL_ProgressBar
            // 
            this.DL_ProgressBar.BackColor = System.Drawing.Color.Transparent;
            this.DL_ProgressBar.BarColor = System.Drawing.Color.RoyalBlue;
            this.DL_ProgressBar.BarThickness = 8;
            this.DL_ProgressBar.LineColor = System.Drawing.Color.DimGray;
            this.DL_ProgressBar.LineThickness = 4;
            this.DL_ProgressBar.Location = new System.Drawing.Point(57, 57);
            this.DL_ProgressBar.Margin = new System.Windows.Forms.Padding(0, 4, 0, 2);
            this.DL_ProgressBar.Maximum = 100;
            this.DL_ProgressBar.MaximumSize = new System.Drawing.Size(48, 48);
            this.DL_ProgressBar.Minimum = 0;
            this.DL_ProgressBar.MinimumSize = new System.Drawing.Size(48, 48);
            this.DL_ProgressBar.Name = "DL_ProgressBar";
            this.DL_ProgressBar.PBStyle = e621_ReBot_v2.CustomControls.CustomPBStyle.Hex;
            this.DL_ProgressBar.Size = new System.Drawing.Size(48, 48);
            this.DL_ProgressBar.StrokeThickness = 2;
            this.DL_ProgressBar.TabIndex = 4;
            this.DL_ProgressBar.Value = 0;
            this.DL_ProgressBar.Visible = false;
            // 
            // picBox_ImageHolder
            // 
            this.picBox_ImageHolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picBox_ImageHolder.InitialImage = global::e621_ReBot_v2.Properties.Resources.E6Image_Loading;
            this.picBox_ImageHolder.Location = new System.Drawing.Point(1, 1);
            this.picBox_ImageHolder.Name = "picBox_ImageHolder";
            this.picBox_ImageHolder.Size = new System.Drawing.Size(158, 158);
            this.picBox_ImageHolder.TabIndex = 5;
            this.picBox_ImageHolder.TabStop = false;
            this.picBox_ImageHolder.LoadCompleted += new System.ComponentModel.AsyncCompletedEventHandler(this.PicBox_ImageHolder_LoadCompleted);
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 15000;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 100;
            this.toolTip_Display.ShowAlways = true;
            // 
            // e6_DownloadItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.DL_FolderIcon);
            this.Controls.Add(this.DL_ProgressBar);
            this.Controls.Add(this.picBox_ImageHolder);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "e6_DownloadItem";
            this.Size = new System.Drawing.Size(160, 160);
            this.Load += new System.EventHandler(this.E6_DownloadItem_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DL_FolderIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_ImageHolder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.PictureBox DL_FolderIcon;
        internal Custom_ProgressBar DL_ProgressBar;
        internal System.Windows.Forms.PictureBox picBox_ImageHolder;
        internal System.Windows.Forms.ToolTip toolTip_Display;
    }
}
