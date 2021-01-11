namespace e621_ReBot_v2.CustomControls
{
    partial class e6_GridItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox_ImageHolder = new System.Windows.Forms.PictureBox();
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.cLabel_isSuperior = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.cLabel_TagWarning = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.cLabel_isUploaded = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.cCheckBox_UPDL = new e621_ReBot_v2.CustomControls.Custom_CheckBox();
            this.cLabel_Rating = new e621_ReBot_v2.CustomControls.Custom_LabelWithStroke();
            this.cPanel_Rating = new e621_ReBot_v2.CustomControls.Custom_Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ImageHolder)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox_ImageHolder
            // 
            this.pictureBox_ImageHolder.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_ImageHolder.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.E6Image_Loading;
            this.pictureBox_ImageHolder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox_ImageHolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_ImageHolder.InitialImage = null;
            this.pictureBox_ImageHolder.Location = new System.Drawing.Point(2, 2);
            this.pictureBox_ImageHolder.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox_ImageHolder.Name = "pictureBox_ImageHolder";
            this.pictureBox_ImageHolder.Size = new System.Drawing.Size(200, 200);
            this.pictureBox_ImageHolder.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox_ImageHolder.TabIndex = 1;
            this.pictureBox_ImageHolder.TabStop = false;
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutoPopDelay = 0;
            this.toolTip_Display.InitialDelay = 500;
            this.toolTip_Display.ReshowDelay = 100;
            this.toolTip_Display.ShowAlways = true;
            // 
            // cLabel_isSuperior
            // 
            this.cLabel_isSuperior.BackColor = System.Drawing.Color.Transparent;
            this.cLabel_isSuperior.ExtraSize = 0;
            this.cLabel_isSuperior.Font = new System.Drawing.Font("Arial Black", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cLabel_isSuperior.ForeColor = System.Drawing.Color.Orange;
            this.cLabel_isSuperior.Location = new System.Drawing.Point(165, 184);
            this.cLabel_isSuperior.Margin = new System.Windows.Forms.Padding(0);
            this.cLabel_isSuperior.Name = "cLabel_isSuperior";
            this.cLabel_isSuperior.Size = new System.Drawing.Size(17, 17);
            this.cLabel_isSuperior.StokeSize = 2;
            this.cLabel_isSuperior.TabIndex = 7;
            this.cLabel_isSuperior.Tag = "Click to navigate to post, Alt+Click to open in your default browser.";
            this.cLabel_isSuperior.Text = "S";
            this.toolTip_Display.SetToolTip(this.cLabel_isSuperior, "Click to navigate to post, Alt+Click to open in your default browser.");
            this.cLabel_isSuperior.Visible = false;
            this.cLabel_isSuperior.Click += new System.EventHandler(this.CLabel_isSuperior_Click);
            // 
            // cLabel_TagWarning
            // 
            this.cLabel_TagWarning.BackColor = System.Drawing.Color.Transparent;
            this.cLabel_TagWarning.ExtraSize = 0;
            this.cLabel_TagWarning.Font = new System.Drawing.Font("Arial Black", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cLabel_TagWarning.ForeColor = System.Drawing.Color.Orange;
            this.cLabel_TagWarning.Location = new System.Drawing.Point(187, 159);
            this.cLabel_TagWarning.Margin = new System.Windows.Forms.Padding(0);
            this.cLabel_TagWarning.Name = "cLabel_TagWarning";
            this.cLabel_TagWarning.Size = new System.Drawing.Size(10, 22);
            this.cLabel_TagWarning.StokeSize = 2;
            this.cLabel_TagWarning.TabIndex = 6;
            this.cLabel_TagWarning.Text = "!";
            this.toolTip_Display.SetToolTip(this.cLabel_TagWarning, "Image has less than minimum required tags.\r\nUploading like this may lead to getti" +
        "ng a record.");
            // 
            // cLabel_isUploaded
            // 
            this.cLabel_isUploaded.BackColor = System.Drawing.Color.Transparent;
            this.cLabel_isUploaded.ExtraSize = 0;
            this.cLabel_isUploaded.Font = new System.Drawing.Font("Arial Black", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cLabel_isUploaded.ForeColor = System.Drawing.Color.Orange;
            this.cLabel_isUploaded.Location = new System.Drawing.Point(28, 88);
            this.cLabel_isUploaded.Margin = new System.Windows.Forms.Padding(0);
            this.cLabel_isUploaded.Name = "cLabel_isUploaded";
            this.cLabel_isUploaded.Size = new System.Drawing.Size(140, 26);
            this.cLabel_isUploaded.StokeSize = 4;
            this.cLabel_isUploaded.TabIndex = 5;
            this.cLabel_isUploaded.Text = "#1234567";
            this.toolTip_Display.SetToolTip(this.cLabel_isUploaded, "Click to navigate to post, Ctrl+Click to open in your default browser.");
            this.cLabel_isUploaded.Visible = false;
            this.cLabel_isUploaded.TextChanged += new System.EventHandler(this.CLabel_isUploaded_TextChanged);
            this.cLabel_isUploaded.Click += new System.EventHandler(this.CLabel_isUploaded_Click);
            // 
            // cCheckBox_UPDL
            // 
            this.cCheckBox_UPDL.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.cCheckBox_UPDL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cCheckBox_UPDL.Location = new System.Drawing.Point(183, 183);
            this.cCheckBox_UPDL.Margin = new System.Windows.Forms.Padding(2);
            this.cCheckBox_UPDL.Name = "cCheckBox_UPDL";
            this.cCheckBox_UPDL.Size = new System.Drawing.Size(17, 17);
            this.cCheckBox_UPDL.TabIndex = 4;
            this.cCheckBox_UPDL.UseVisualStyleBackColor = true;
            this.cCheckBox_UPDL.CheckedChanged += new System.EventHandler(this.CCheckBox_UPDL_CheckedChanged);
            // 
            // cLabel_Rating
            // 
            this.cLabel_Rating.BackColor = System.Drawing.Color.Transparent;
            this.cLabel_Rating.ExtraSize = 0;
            this.cLabel_Rating.Font = new System.Drawing.Font("Arial Black", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cLabel_Rating.ForeColor = System.Drawing.Color.Red;
            this.cLabel_Rating.Location = new System.Drawing.Point(2, 176);
            this.cLabel_Rating.Margin = new System.Windows.Forms.Padding(0);
            this.cLabel_Rating.Name = "cLabel_Rating";
            this.cLabel_Rating.Size = new System.Drawing.Size(24, 24);
            this.cLabel_Rating.StokeSize = 4;
            this.cLabel_Rating.TabIndex = 2;
            this.cLabel_Rating.Text = "E";
            this.cLabel_Rating.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cPanel_Rating
            // 
            this.cPanel_Rating.BackColor = System.Drawing.Color.Red;
            this.cPanel_Rating.Corner = 3;
            this.cPanel_Rating.Location = new System.Drawing.Point(1, 179);
            this.cPanel_Rating.Name = "cPanel_Rating";
            this.cPanel_Rating.Size = new System.Drawing.Size(24, 24);
            this.cPanel_Rating.TabIndex = 8;
            this.cPanel_Rating.Visible = false;
            // 
            // e6_GridItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(45)))), ((int)(((byte)(90)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.cPanel_Rating);
            this.Controls.Add(this.cLabel_isSuperior);
            this.Controls.Add(this.cLabel_TagWarning);
            this.Controls.Add(this.cLabel_isUploaded);
            this.Controls.Add(this.cCheckBox_UPDL);
            this.Controls.Add(this.cLabel_Rating);
            this.Controls.Add(this.pictureBox_ImageHolder);
            this.Name = "e6_GridItem";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(204, 204);
            this.Load += new System.EventHandler(this.E6_GridItem_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ImageHolder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.PictureBox pictureBox_ImageHolder;
        internal Custom_LabelWithStroke cLabel_Rating;
        internal Custom_LabelWithStroke cLabel_isUploaded;
        internal Custom_LabelWithStroke cLabel_TagWarning;
        internal Custom_LabelWithStroke cLabel_isSuperior;
        internal Custom_CheckBox cCheckBox_UPDL;
        internal System.Windows.Forms.ToolTip toolTip_Display;
        internal Custom_Panel cPanel_Rating;
    }
}
