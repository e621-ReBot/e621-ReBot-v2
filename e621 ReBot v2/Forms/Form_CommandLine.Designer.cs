
namespace e621_ReBot_v2.Forms
{
    partial class Form_CommandLine
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
            this.CommandLine_Textbox = new System.Windows.Forms.TextBox();
            this.TagLine_Textbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CommandLine_Textbox
            // 
            this.CommandLine_Textbox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.CommandLine_Textbox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.CommandLine_Textbox.Location = new System.Drawing.Point(1, 1);
            this.CommandLine_Textbox.Margin = new System.Windows.Forms.Padding(1);
            this.CommandLine_Textbox.MaxLength = 16;
            this.CommandLine_Textbox.Name = "CommandLine_Textbox";
            this.CommandLine_Textbox.Size = new System.Drawing.Size(162, 20);
            this.CommandLine_Textbox.TabIndex = 2;
            this.CommandLine_Textbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.CommandLine_Textbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CommandLine_Textbox_KeyDown);
            // 
            // TagLine_Textbox
            // 
            this.TagLine_Textbox.Location = new System.Drawing.Point(1, 1);
            this.TagLine_Textbox.Margin = new System.Windows.Forms.Padding(1);
            this.TagLine_Textbox.MaxLength = 1024;
            this.TagLine_Textbox.Multiline = true;
            this.TagLine_Textbox.Name = "TagLine_Textbox";
            this.TagLine_Textbox.Size = new System.Drawing.Size(162, 99);
            this.TagLine_Textbox.TabIndex = 3;
            this.TagLine_Textbox.Visible = false;
            this.TagLine_Textbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TagLine_Textbox_KeyDown);
            // 
            // Form_CommandLine
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(164, 22);
            this.Controls.Add(this.CommandLine_Textbox);
            this.Controls.Add(this.TagLine_Textbox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_CommandLine";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Enter command string";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_CommandLine_FormClosing);
            this.Load += new System.EventHandler(this.Form_CommandLine_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox CommandLine_Textbox;
        internal System.Windows.Forms.TextBox TagLine_Textbox;
    }
}