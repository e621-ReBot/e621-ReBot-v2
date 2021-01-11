namespace e621_ReBot_v2.Forms
{
    partial class Form_Notes
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
            this.panel_Holder = new System.Windows.Forms.Panel();
            this.Note_TextBox = new System.Windows.Forms.RichTextBox();
            this.Delete_Note_btn = new System.Windows.Forms.Button();
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.panel_Holder.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_Holder
            // 
            this.panel_Holder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Holder.Controls.Add(this.Note_TextBox);
            this.panel_Holder.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_Holder.Location = new System.Drawing.Point(1, 1);
            this.panel_Holder.Name = "panel_Holder";
            this.panel_Holder.Size = new System.Drawing.Size(302, 129);
            this.panel_Holder.TabIndex = 3;
            // 
            // Note_TextBox
            // 
            this.Note_TextBox.BackColor = System.Drawing.Color.Silver;
            this.Note_TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Note_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Note_TextBox.Location = new System.Drawing.Point(0, 0);
            this.Note_TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.Note_TextBox.Name = "Note_TextBox";
            this.Note_TextBox.Size = new System.Drawing.Size(300, 127);
            this.Note_TextBox.TabIndex = 3;
            this.Note_TextBox.Text = "";
            this.Note_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Note_TextBox_KeyDown);
            // 
            // Delete_Note_btn
            // 
            this.Delete_Note_btn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Delete_Note_btn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Delete_Note_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Delete_Note_btn.Location = new System.Drawing.Point(1, 117);
            this.Delete_Note_btn.Margin = new System.Windows.Forms.Padding(0);
            this.Delete_Note_btn.Name = "Delete_Note_btn";
            this.Delete_Note_btn.Size = new System.Drawing.Size(302, 23);
            this.Delete_Note_btn.TabIndex = 4;
            this.Delete_Note_btn.TabStop = false;
            this.Delete_Note_btn.Tag = "Delete";
            this.Delete_Note_btn.Text = "Delete Note";
            this.Delete_Note_btn.UseVisualStyleBackColor = true;
            this.Delete_Note_btn.Click += new System.EventHandler(this.Delete_Note_btn_Click);
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 0;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 100;
            this.toolTip_Display.ShowAlways = true;
            // 
            // Form_Notes
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(304, 141);
            this.Controls.Add(this.Delete_Note_btn);
            this.Controls.Add(this.panel_Holder);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Notes";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notes";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Notes_FormClosed);
            this.Load += new System.EventHandler(this.Form_Notes_Load);
            this.Shown += new System.EventHandler(this.Form_Notes_Shown);
            this.panel_Holder.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Panel panel_Holder;
        internal System.Windows.Forms.RichTextBox Note_TextBox;
        internal System.Windows.Forms.Button Delete_Note_btn;
        internal System.Windows.Forms.ToolTip toolTip_Display;
    }
}