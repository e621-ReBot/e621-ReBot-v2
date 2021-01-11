namespace e621_ReBot_v2.Forms
{
    partial class Form_APIKey
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
            this.APIKey_TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // APIKey_TextBox
            // 
            this.APIKey_TextBox.BackColor = System.Drawing.Color.Silver;
            this.APIKey_TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.APIKey_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.APIKey_TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.APIKey_TextBox.Location = new System.Drawing.Point(1, 1);
            this.APIKey_TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.APIKey_TextBox.MaxLength = 24;
            this.APIKey_TextBox.Name = "APIKey_TextBox";
            this.APIKey_TextBox.PasswordChar = '✘';
            this.APIKey_TextBox.Size = new System.Drawing.Size(247, 26);
            this.APIKey_TextBox.TabIndex = 1;
            this.APIKey_TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.APIKey_TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.APIKey_TextBox_KeyDown);
            // 
            // Form_APIKey
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(249, 28);
            this.Controls.Add(this.APIKey_TextBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_APIKey";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "API Key";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_APIKey_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox APIKey_TextBox;
    }
}