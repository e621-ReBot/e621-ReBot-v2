
namespace e621_ReBot_v2.Forms
{
    partial class Form_CommandView
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
            this.Command_ListView = new System.Windows.Forms.ListView();
            this.Header_Command = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Header_Tags = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ContextMenuStrip_ListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuStrip_ListView.SuspendLayout();
            this.SuspendLayout();
            // 
            // Command_ListView
            // 
            this.Command_ListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Command_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Header_Command,
            this.Header_Tags});
            this.Command_ListView.ContextMenuStrip = this.ContextMenuStrip_ListView;
            this.Command_ListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Command_ListView.FullRowSelect = true;
            this.Command_ListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.Command_ListView.HideSelection = false;
            this.Command_ListView.LabelWrap = false;
            this.Command_ListView.Location = new System.Drawing.Point(0, 0);
            this.Command_ListView.Margin = new System.Windows.Forms.Padding(0);
            this.Command_ListView.MultiSelect = false;
            this.Command_ListView.Name = "Command_ListView";
            this.Command_ListView.ShowItemToolTips = true;
            this.Command_ListView.Size = new System.Drawing.Size(464, 281);
            this.Command_ListView.TabIndex = 2;
            this.Command_ListView.TabStop = false;
            this.Command_ListView.UseCompatibleStateImageBehavior = false;
            this.Command_ListView.View = System.Windows.Forms.View.Details;
            // 
            // Header_Command
            // 
            this.Header_Command.Text = "Command";
            this.Header_Command.Width = 128;
            // 
            // Header_Tags
            // 
            this.Header_Tags.Text = "Tags";
            this.Header_Tags.Width = 315;
            // 
            // ContextMenuStrip_ListView
            // 
            this.ContextMenuStrip_ListView.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextMenuStrip_ListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveToolStripMenuItem,
            this.RemoveToolStripMenuItem1});
            this.ContextMenuStrip_ListView.Name = "ContextMenuStrip1";
            this.ContextMenuStrip_ListView.Size = new System.Drawing.Size(118, 48);
            this.ContextMenuStrip_ListView.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip_ListView_Opening);
            // 
            // RemoveToolStripMenuItem
            // 
            this.RemoveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CommandToolStripMenuItem,
            this.TagsToolStripMenuItem});
            this.RemoveToolStripMenuItem.Name = "RemoveToolStripMenuItem";
            this.RemoveToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.RemoveToolStripMenuItem.Text = "Edit";
            // 
            // CommandToolStripMenuItem
            // 
            this.CommandToolStripMenuItem.Name = "CommandToolStripMenuItem";
            this.CommandToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.CommandToolStripMenuItem.Text = "Command";
            this.CommandToolStripMenuItem.Click += new System.EventHandler(this.CommandToolStripMenuItem_Click);
            // 
            // TagsToolStripMenuItem
            // 
            this.TagsToolStripMenuItem.Name = "TagsToolStripMenuItem";
            this.TagsToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.TagsToolStripMenuItem.Text = "Tags";
            this.TagsToolStripMenuItem.Click += new System.EventHandler(this.TagsToolStripMenuItem_Click);
            // 
            // RemoveToolStripMenuItem1
            // 
            this.RemoveToolStripMenuItem1.Name = "RemoveToolStripMenuItem1";
            this.RemoveToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.RemoveToolStripMenuItem1.Text = "Remove";
            this.RemoveToolStripMenuItem1.Click += new System.EventHandler(this.RemoveToolStripMenuItem1_Click);
            // 
            // Form_CommandView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.Command_ListView);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "Form_CommandView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Command List";
            this.Load += new System.EventHandler(this.Form_CommandView_Load);
            this.ContextMenuStrip_ListView.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ListView Command_ListView;
        internal System.Windows.Forms.ColumnHeader Header_Command;
        internal System.Windows.Forms.ColumnHeader Header_Tags;
        internal System.Windows.Forms.ContextMenuStrip ContextMenuStrip_ListView;
        internal System.Windows.Forms.ToolStripMenuItem RemoveToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem CommandToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem TagsToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem RemoveToolStripMenuItem1;
    }
}