namespace e621_ReBot_v2.Forms
{
    partial class Form_Tagger
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
            this.textBox_Title = new System.Windows.Forms.TextBox();
            this.Info_Label = new System.Windows.Forms.Label();
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.textBox_Tags = new System.Windows.Forms.RichTextBox();
            this.cGroupBoxColored_AutocompleteSelector = new e621_ReBot_v2.CustomControls.Custom_GroupBoxColored();
            this.TB_ACPools = new System.Windows.Forms.RadioButton();
            this.TB_ACTags = new System.Windows.Forms.RadioButton();
            this.TB_CommandLine = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.TB_ParentOffset = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.TB_ArtistAlias = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.TB_Description = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.timer_TagCount = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip_EditItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMS_EditTag = new System.Windows.Forms.ToolStripMenuItem();
            this.CMS_RemoveTag = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_EditCat = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMS_AddTag = new System.Windows.Forms.ToolStripMenuItem();
            this.CMS_RemoveCategory = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_AddCat = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMS_AddCategory = new System.Windows.Forms.ToolStripMenuItem();
            this.panel_TagBoxOutline = new System.Windows.Forms.Panel();
            this.flowLayoutPanel_Holder = new System.Windows.Forms.FlowLayoutPanel();
            this.TB_Done = new e621_ReBot_v2.CustomControls.Button_Unfocusable();
            this.cGroupBoxColored_AutocompleteSelector.SuspendLayout();
            this.contextMenuStrip_EditItem.SuspendLayout();
            this.contextMenuStrip_EditCat.SuspendLayout();
            this.contextMenuStrip_AddCat.SuspendLayout();
            this.panel_TagBoxOutline.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_Title
            // 
            this.textBox_Title.Font = new System.Drawing.Font("Arial Unicode MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Title.Location = new System.Drawing.Point(1, 1);
            this.textBox_Title.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_Title.Name = "textBox_Title";
            this.textBox_Title.ShortcutsEnabled = false;
            this.textBox_Title.Size = new System.Drawing.Size(303, 22);
            this.textBox_Title.TabIndex = 2;
            this.textBox_Title.TabStop = false;
            this.textBox_Title.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_Title_KeyDown);
            // 
            // Info_Label
            // 
            this.Info_Label.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Info_Label.Enabled = false;
            this.Info_Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Info_Label.Location = new System.Drawing.Point(208, 58);
            this.Info_Label.Margin = new System.Windows.Forms.Padding(2);
            this.Info_Label.Name = "Info_Label";
            this.Info_Label.Size = new System.Drawing.Size(134, 13);
            this.Info_Label.TabIndex = 8;
            this.Info_Label.Text = "Right click for options";
            this.Info_Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 0;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 100;
            // 
            // textBox_Tags
            // 
            this.textBox_Tags.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_Tags.DetectUrls = false;
            this.textBox_Tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_Tags.Font = new System.Drawing.Font("Arial Unicode MS", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.textBox_Tags.Location = new System.Drawing.Point(0, 0);
            this.textBox_Tags.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Tags.Name = "textBox_Tags";
            this.textBox_Tags.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBox_Tags.Size = new System.Drawing.Size(340, 120);
            this.textBox_Tags.TabIndex = 0;
            this.textBox_Tags.Text = "";
            this.toolTip_Display.SetToolTip(this.textBox_Tags, "Add tags for image here.");
            this.textBox_Tags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_Tags_KeyDown);
            this.textBox_Tags.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_Tags_KeyUp);
            this.textBox_Tags.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.TextBox_Tags_PreviewKeyDown);
            // 
            // cGroupBoxColored_AutocompleteSelector
            // 
            this.cGroupBoxColored_AutocompleteSelector.BorderColor = System.Drawing.Color.Black;
            this.cGroupBoxColored_AutocompleteSelector.BottomBorderFix = 1;
            this.cGroupBoxColored_AutocompleteSelector.Controls.Add(this.TB_ACPools);
            this.cGroupBoxColored_AutocompleteSelector.Controls.Add(this.TB_ACTags);
            this.cGroupBoxColored_AutocompleteSelector.Enabled = false;
            this.cGroupBoxColored_AutocompleteSelector.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.cGroupBoxColored_AutocompleteSelector.Location = new System.Drawing.Point(208, 23);
            this.cGroupBoxColored_AutocompleteSelector.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.cGroupBoxColored_AutocompleteSelector.Name = "cGroupBoxColored_AutocompleteSelector";
            this.cGroupBoxColored_AutocompleteSelector.Size = new System.Drawing.Size(135, 35);
            this.cGroupBoxColored_AutocompleteSelector.TabIndex = 14;
            this.cGroupBoxColored_AutocompleteSelector.TabStop = false;
            this.cGroupBoxColored_AutocompleteSelector.Text = "Autocomplete:";
            this.cGroupBoxColored_AutocompleteSelector.TextOffset = 0;
            this.toolTip_Display.SetToolTip(this.cGroupBoxColored_AutocompleteSelector, "Autocomplete the selected type.");
            // 
            // TB_ACPools
            // 
            this.TB_ACPools.AutoSize = true;
            this.TB_ACPools.Location = new System.Drawing.Point(72, 15);
            this.TB_ACPools.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.TB_ACPools.Name = "TB_ACPools";
            this.TB_ACPools.Size = new System.Drawing.Size(51, 17);
            this.TB_ACPools.TabIndex = 1;
            this.TB_ACPools.Text = "Pools";
            this.TB_ACPools.UseVisualStyleBackColor = true;
            this.TB_ACPools.CheckedChanged += new System.EventHandler(this.TB_ACPools_CheckedChanged);
            // 
            // TB_ACTags
            // 
            this.TB_ACTags.AutoSize = true;
            this.TB_ACTags.Checked = true;
            this.TB_ACTags.Location = new System.Drawing.Point(19, 15);
            this.TB_ACTags.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.TB_ACTags.Name = "TB_ACTags";
            this.TB_ACTags.Size = new System.Drawing.Size(49, 17);
            this.TB_ACTags.TabIndex = 0;
            this.TB_ACTags.TabStop = true;
            this.TB_ACTags.Text = "Tags";
            this.TB_ACTags.UseVisualStyleBackColor = true;
            this.TB_ACTags.CheckedChanged += new System.EventHandler(this.TB_ACTags_CheckedChanged);
            // 
            // TB_CommandLine
            // 
            this.TB_CommandLine.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TB_CommandLine.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TB_CommandLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TB_CommandLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TB_CommandLine.Location = new System.Drawing.Point(1, 47);
            this.TB_CommandLine.Margin = new System.Windows.Forms.Padding(0);
            this.TB_CommandLine.Name = "TB_CommandLine";
            this.TB_CommandLine.Size = new System.Drawing.Size(100, 24);
            this.TB_CommandLine.TabIndex = 7;
            this.TB_CommandLine.TabStop = false;
            this.TB_CommandLine.Text = "Command Line";
            this.toolTip_Display.SetToolTip(this.TB_CommandLine, "Click to execute tag command.\r\nCommands automatically apply tags that you have se" +
        "t for given command.\r\n\r\nCtrl+Click to add new command.\r\nShift+Click to open list" +
        " of current commands.");
            this.TB_CommandLine.UseVisualStyleBackColor = true;
            this.TB_CommandLine.Click += new System.EventHandler(this.TB_CommandLine_Click);
            // 
            // TB_ParentOffset
            // 
            this.TB_ParentOffset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TB_ParentOffset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TB_ParentOffset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TB_ParentOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TB_ParentOffset.Location = new System.Drawing.Point(100, 47);
            this.TB_ParentOffset.Margin = new System.Windows.Forms.Padding(0);
            this.TB_ParentOffset.Name = "TB_ParentOffset";
            this.TB_ParentOffset.Size = new System.Drawing.Size(100, 24);
            this.TB_ParentOffset.TabIndex = 6;
            this.TB_ParentOffset.TabStop = false;
            this.TB_ParentOffset.Text = "Parent Offset";
            this.toolTip_Display.SetToolTip(this.TB_ParentOffset, "Click to set Parent Offset.\r\nParent Offset automaticaly sets this image as a chil" +
        "d of another.\r\n\r\nCtrl+Click to remove.");
            this.TB_ParentOffset.UseVisualStyleBackColor = true;
            this.TB_ParentOffset.Click += new System.EventHandler(this.TB_ParentOffset_Click);
            // 
            // TB_ArtistAlias
            // 
            this.TB_ArtistAlias.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TB_ArtistAlias.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TB_ArtistAlias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TB_ArtistAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TB_ArtistAlias.Location = new System.Drawing.Point(100, 24);
            this.TB_ArtistAlias.Margin = new System.Windows.Forms.Padding(0);
            this.TB_ArtistAlias.Name = "TB_ArtistAlias";
            this.TB_ArtistAlias.Size = new System.Drawing.Size(100, 24);
            this.TB_ArtistAlias.TabIndex = 5;
            this.TB_ArtistAlias.TabStop = false;
            this.TB_ArtistAlias.Text = "Artist Alias";
            this.toolTip_Display.SetToolTip(this.TB_ArtistAlias, "Click to set Artist Alias.\r\nArtist Alias automaticaly tags images with set artist" +
        " tag for given Artist.\r\n\r\nCtrl+Click to remove Alias.");
            this.TB_ArtistAlias.UseVisualStyleBackColor = true;
            this.TB_ArtistAlias.Click += new System.EventHandler(this.TB_ArtistAlias_Click);
            // 
            // TB_Description
            // 
            this.TB_Description.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TB_Description.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TB_Description.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TB_Description.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TB_Description.Location = new System.Drawing.Point(1, 24);
            this.TB_Description.Margin = new System.Windows.Forms.Padding(0);
            this.TB_Description.Name = "TB_Description";
            this.TB_Description.Size = new System.Drawing.Size(100, 24);
            this.TB_Description.TabIndex = 4;
            this.TB_Description.TabStop = false;
            this.TB_Description.Text = "< Description";
            this.toolTip_Display.SetToolTip(this.TB_Description, "Click to show/hide post description.\r\nHiding will save any changes.\r\n\r\nCtrl+Click" +
        " to copy title and description to clipboard.");
            this.TB_Description.UseVisualStyleBackColor = true;
            this.TB_Description.Click += new System.EventHandler(this.TB_Description_Click);
            // 
            // timer_TagCount
            // 
            this.timer_TagCount.Interval = 3000;
            this.timer_TagCount.Tick += new System.EventHandler(this.Timer_TagCount_Tick);
            // 
            // contextMenuStrip_EditItem
            // 
            this.contextMenuStrip_EditItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMS_EditTag,
            this.CMS_RemoveTag});
            this.contextMenuStrip_EditItem.Name = "ContextMenuStrip_EditItem";
            this.contextMenuStrip_EditItem.ShowImageMargin = false;
            this.contextMenuStrip_EditItem.Size = new System.Drawing.Size(114, 48);
            // 
            // CMS_EditTag
            // 
            this.CMS_EditTag.Name = "CMS_EditTag";
            this.CMS_EditTag.Size = new System.Drawing.Size(113, 22);
            this.CMS_EditTag.Text = "Edit Tag";
            this.CMS_EditTag.Click += new System.EventHandler(this.CMS_EditTag_Click);
            // 
            // CMS_RemoveTag
            // 
            this.CMS_RemoveTag.Name = "CMS_RemoveTag";
            this.CMS_RemoveTag.Size = new System.Drawing.Size(113, 22);
            this.CMS_RemoveTag.Text = "Remove Tag";
            this.CMS_RemoveTag.Click += new System.EventHandler(this.CMS_RemoveTag_Click);
            // 
            // contextMenuStrip_EditCat
            // 
            this.contextMenuStrip_EditCat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMS_AddTag,
            this.CMS_RemoveCategory});
            this.contextMenuStrip_EditCat.Name = "ContextMenuStrip_Edit";
            this.contextMenuStrip_EditCat.ShowImageMargin = false;
            this.contextMenuStrip_EditCat.Size = new System.Drawing.Size(144, 48);
            // 
            // CMS_AddTag
            // 
            this.CMS_AddTag.Name = "CMS_AddTag";
            this.CMS_AddTag.Size = new System.Drawing.Size(143, 22);
            this.CMS_AddTag.Text = "Add Tag";
            this.CMS_AddTag.Click += new System.EventHandler(this.CMS_AddTag_Click);
            // 
            // CMS_RemoveCategory
            // 
            this.CMS_RemoveCategory.Name = "CMS_RemoveCategory";
            this.CMS_RemoveCategory.Size = new System.Drawing.Size(143, 22);
            this.CMS_RemoveCategory.Text = "Remove Category";
            this.CMS_RemoveCategory.ToolTipText = "Removes container and all tags within.";
            this.CMS_RemoveCategory.Click += new System.EventHandler(this.CMS_RemoveCategory_Click);
            // 
            // contextMenuStrip_AddCat
            // 
            this.contextMenuStrip_AddCat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMS_AddCategory});
            this.contextMenuStrip_AddCat.Name = "ContextMenuStrip1";
            this.contextMenuStrip_AddCat.ShowImageMargin = false;
            this.contextMenuStrip_AddCat.Size = new System.Drawing.Size(123, 26);
            // 
            // CMS_AddCategory
            // 
            this.CMS_AddCategory.Name = "CMS_AddCategory";
            this.CMS_AddCategory.Size = new System.Drawing.Size(122, 22);
            this.CMS_AddCategory.Text = "Add Category";
            this.CMS_AddCategory.ToolTipText = "A container where you can put your tags.";
            this.CMS_AddCategory.Click += new System.EventHandler(this.CMS_AddCategory_Click);
            // 
            // panel_TagBoxOutline
            // 
            this.panel_TagBoxOutline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_TagBoxOutline.Controls.Add(this.textBox_Tags);
            this.panel_TagBoxOutline.Location = new System.Drawing.Point(1, 72);
            this.panel_TagBoxOutline.Margin = new System.Windows.Forms.Padding(0);
            this.panel_TagBoxOutline.Name = "panel_TagBoxOutline";
            this.panel_TagBoxOutline.Size = new System.Drawing.Size(342, 122);
            this.panel_TagBoxOutline.TabIndex = 12;
            // 
            // flowLayoutPanel_Holder
            // 
            this.flowLayoutPanel_Holder.AutoSize = true;
            this.flowLayoutPanel_Holder.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel_Holder.Location = new System.Drawing.Point(1, 196);
            this.flowLayoutPanel_Holder.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_Holder.MinimumSize = new System.Drawing.Size(32, 1);
            this.flowLayoutPanel_Holder.Name = "flowLayoutPanel_Holder";
            this.flowLayoutPanel_Holder.Size = new System.Drawing.Size(342, 1);
            this.flowLayoutPanel_Holder.TabIndex = 13;
            this.flowLayoutPanel_Holder.SizeChanged += new System.EventHandler(this.FlowLayoutPanel_Holder_SizeChanged);
            this.flowLayoutPanel_Holder.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.FlowLayoutPanel_Holder_ControlRemoved);
            // 
            // TB_Done
            // 
            this.TB_Done.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.CheckMark_Icon;
            this.TB_Done.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.TB_Done.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TB_Done.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TB_Done.Location = new System.Drawing.Point(303, 1);
            this.TB_Done.Margin = new System.Windows.Forms.Padding(0);
            this.TB_Done.Name = "TB_Done";
            this.TB_Done.Size = new System.Drawing.Size(40, 22);
            this.TB_Done.TabIndex = 3;
            this.TB_Done.TabStop = false;
            this.TB_Done.UseVisualStyleBackColor = true;
            this.TB_Done.Click += new System.EventHandler(this.TB_Done_Click);
            // 
            // Form_Tagger
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(344, 195);
            this.ContextMenuStrip = this.contextMenuStrip_AddCat;
            this.Controls.Add(this.cGroupBoxColored_AutocompleteSelector);
            this.Controls.Add(this.flowLayoutPanel_Holder);
            this.Controls.Add(this.panel_TagBoxOutline);
            this.Controls.Add(this.TB_CommandLine);
            this.Controls.Add(this.TB_ParentOffset);
            this.Controls.Add(this.TB_ArtistAlias);
            this.Controls.Add(this.TB_Description);
            this.Controls.Add(this.TB_Done);
            this.Controls.Add(this.textBox_Title);
            this.Controls.Add(this.Info_Label);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 234);
            this.Name = "Form_Tagger";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Tagger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Tagger_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Tagger_FormClosed);
            this.Load += new System.EventHandler(this.Form_Tagger_Load);
            this.Shown += new System.EventHandler(this.Form_Tagger_Shown);
            this.Move += new System.EventHandler(this.Form_Tagger_Move);
            this.cGroupBoxColored_AutocompleteSelector.ResumeLayout(false);
            this.cGroupBoxColored_AutocompleteSelector.PerformLayout();
            this.contextMenuStrip_EditItem.ResumeLayout(false);
            this.contextMenuStrip_EditCat.ResumeLayout(false);
            this.contextMenuStrip_AddCat.ResumeLayout(false);
            this.panel_TagBoxOutline.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox textBox_Title;
        private CustomControls.Button_Unfocusable TB_Done;
        private CustomControls.Button_Unfocusable TB_Description;
        private System.Windows.Forms.ToolTip toolTip_Display;
        private CustomControls.Button_Unfocusable TB_ArtistAlias;
        internal CustomControls.Button_Unfocusable TB_ParentOffset;
        internal CustomControls.Button_Unfocusable TB_CommandLine;
        internal System.Windows.Forms.Label Info_Label;
        private System.Windows.Forms.Timer timer_TagCount;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_EditItem;
        internal System.Windows.Forms.ToolStripMenuItem CMS_EditTag;
        internal System.Windows.Forms.ToolStripMenuItem CMS_RemoveTag;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_EditCat;
        internal System.Windows.Forms.ToolStripMenuItem CMS_AddTag;
        internal System.Windows.Forms.ToolStripMenuItem CMS_RemoveCategory;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_AddCat;
        internal System.Windows.Forms.ToolStripMenuItem CMS_AddCategory;
        internal System.Windows.Forms.Panel panel_TagBoxOutline;
        internal System.Windows.Forms.RichTextBox textBox_Tags;
        internal System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_Holder;
        private CustomControls.Custom_GroupBoxColored cGroupBoxColored_AutocompleteSelector;
        private System.Windows.Forms.RadioButton TB_ACPools;
        private System.Windows.Forms.RadioButton TB_ACTags;
    }
}