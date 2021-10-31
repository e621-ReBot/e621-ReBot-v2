namespace e621_ReBot_v2.Forms
{
    partial class Form_PoolWatcher
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
            this.contextMenuStrip_Remove = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_RemoveNode = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_Add = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem_AddNode = new System.Windows.Forms.ToolStripMenuItem();
            this.TreeView_PoolWatcher = new System.Windows.Forms.TreeView();
            this.contextMenuStrip_Remove.SuspendLayout();
            this.contextMenuStrip_Add.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip_Remove
            // 
            this.contextMenuStrip_Remove.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_Remove.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_RemoveNode});
            this.contextMenuStrip_Remove.Name = "ContextMenuStrip1";
            this.contextMenuStrip_Remove.Size = new System.Drawing.Size(118, 26);
            // 
            // ToolStripMenuItem_RemoveNode
            // 
            this.ToolStripMenuItem_RemoveNode.Name = "ToolStripMenuItem_RemoveNode";
            this.ToolStripMenuItem_RemoveNode.Size = new System.Drawing.Size(117, 22);
            this.ToolStripMenuItem_RemoveNode.Text = "Remove";
            this.ToolStripMenuItem_RemoveNode.ToolTipText = "Remove pool from the Pool Watcher";
            this.ToolStripMenuItem_RemoveNode.Click += new System.EventHandler(this.ToolStripMenuItem_RemoveNode_Click);
            // 
            // contextMenuStrip_Add
            // 
            this.contextMenuStrip_Add.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_Add.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_AddNode});
            this.contextMenuStrip_Add.Name = "ContextMenuStrip1";
            this.contextMenuStrip_Add.Size = new System.Drawing.Size(97, 26);
            // 
            // ToolStripMenuItem_AddNode
            // 
            this.ToolStripMenuItem_AddNode.Name = "ToolStripMenuItem_AddNode";
            this.ToolStripMenuItem_AddNode.Size = new System.Drawing.Size(96, 22);
            this.ToolStripMenuItem_AddNode.Text = "Add";
            this.ToolStripMenuItem_AddNode.ToolTipText = "Add pool to the Pool Watcher";
            this.ToolStripMenuItem_AddNode.Click += new System.EventHandler(this.ToolStripMenuItem_AddNode_Click);
            // 
            // TreeView_PoolWatcher
            // 
            this.TreeView_PoolWatcher.BackColor = System.Drawing.Color.Silver;
            this.TreeView_PoolWatcher.ContextMenuStrip = this.contextMenuStrip_Add;
            this.TreeView_PoolWatcher.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeView_PoolWatcher.ForeColor = System.Drawing.Color.Black;
            this.TreeView_PoolWatcher.Location = new System.Drawing.Point(1, 1);
            this.TreeView_PoolWatcher.Name = "TreeView_PoolWatcher";
            this.TreeView_PoolWatcher.ShowLines = false;
            this.TreeView_PoolWatcher.Size = new System.Drawing.Size(462, 279);
            this.TreeView_PoolWatcher.TabIndex = 0;
            this.TreeView_PoolWatcher.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_PoolWatcher_BeforeSelect);
            this.TreeView_PoolWatcher.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeView_PoolWatcher_NodeMouseClick);
            this.TreeView_PoolWatcher.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeView_PoolWatcher_KeyDown);
            this.TreeView_PoolWatcher.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeView_PoolWatcher_MouseDown);
            // 
            // Form_PoolWatcher
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.TreeView_PoolWatcher);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_PoolWatcher";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Pool Watcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_PoolWatcher_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_PoolWatcher_FormClosed);
            this.Load += new System.EventHandler(this.Form_PoolWatcher_Load);
            this.contextMenuStrip_Remove.ResumeLayout(false);
            this.contextMenuStrip_Add.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.ContextMenuStrip contextMenuStrip_Remove;
        internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_RemoveNode;
        internal System.Windows.Forms.ContextMenuStrip contextMenuStrip_Add;
        internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_AddNode;
        internal System.Windows.Forms.TreeView TreeView_PoolWatcher;
    }
}