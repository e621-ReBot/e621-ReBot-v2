namespace e621_ReBot_v2.Forms
{
    partial class Form_Blacklist
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Blacklist));
            this.toolTip_Display = new System.Windows.Forms.ToolTip(this.components);
            this.textBox_Blacklist = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // toolTip_Display
            // 
            this.toolTip_Display.AutomaticDelay = 100;
            this.toolTip_Display.AutoPopDelay = 15000;
            this.toolTip_Display.InitialDelay = 100;
            this.toolTip_Display.ReshowDelay = 100;
            this.toolTip_Display.ShowAlways = true;
            // 
            // textBox_Blacklist
            // 
            this.textBox_Blacklist.BackColor = System.Drawing.Color.Silver;
            this.textBox_Blacklist.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_Blacklist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_Blacklist.Location = new System.Drawing.Point(1, 1);
            this.textBox_Blacklist.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_Blacklist.Multiline = true;
            this.textBox_Blacklist.Name = "textBox_Blacklist";
            this.textBox_Blacklist.Size = new System.Drawing.Size(302, 139);
            this.textBox_Blacklist.TabIndex = 1;
            this.toolTip_Display.SetToolTip(this.textBox_Blacklist, resources.GetString("textBox_Blacklist.ToolTip"));
            this.textBox_Blacklist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_Blacklist_KeyDown);
            // 
            // Form_Blacklist
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(304, 141);
            this.Controls.Add(this.textBox_Blacklist);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Blacklist";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Blacklist";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Blacklist_FormClosed);
            this.Load += new System.EventHandler(this.Form_Blacklist_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ToolTip toolTip_Display;
        internal System.Windows.Forms.TextBox textBox_Blacklist;
    }
}