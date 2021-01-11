namespace e621_ReBot_v2
{
    partial class Form_Loader
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
            this.timer_CursorFix = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timer_CursorFix
            // 
            this.timer_CursorFix.Enabled = true;
            this.timer_CursorFix.Tick += new System.EventHandler(this.Timer_CursorFix_Tick);
            // 
            // Form_Loader
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::e621_ReBot_v2.Properties.Resources.E6_LogoBigRemakeImg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(256, 256);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Loader";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Loader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer_CursorFix;
    }
}