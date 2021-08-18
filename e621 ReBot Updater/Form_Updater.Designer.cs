
namespace e621_ReBot_Updater
{
    partial class Form_Updater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Updater));
            this.Label_Latest = new System.Windows.Forms.Label();
            this.Label_Current = new System.Windows.Forms.Label();
            this.WebBrowser_Updater = new System.Windows.Forms.WebBrowser();
            this.Label_Download = new System.Windows.Forms.Label();
            this.timer_Delay = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorker_Update = new System.ComponentModel.BackgroundWorker();
            this.timer_Close = new System.Windows.Forms.Timer(this.components);
            this.label_Title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Label_Latest
            // 
            this.Label_Latest.BackColor = System.Drawing.Color.Transparent;
            this.Label_Latest.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Latest.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.Label_Latest.Location = new System.Drawing.Point(8, 70);
            this.Label_Latest.Margin = new System.Windows.Forms.Padding(8);
            this.Label_Latest.Name = "Label_Latest";
            this.Label_Latest.Size = new System.Drawing.Size(290, 24);
            this.Label_Latest.TabIndex = 4;
            this.Label_Latest.Text = "Latest Version:  Checking...";
            this.Label_Latest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Label_Current
            // 
            this.Label_Current.BackColor = System.Drawing.Color.Transparent;
            this.Label_Current.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Current.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.Label_Current.Location = new System.Drawing.Point(8, 42);
            this.Label_Current.Margin = new System.Windows.Forms.Padding(8);
            this.Label_Current.Name = "Label_Current";
            this.Label_Current.Size = new System.Drawing.Size(290, 24);
            this.Label_Current.TabIndex = 3;
            this.Label_Current.Text = "Current Version: 1.0.0.0";
            this.Label_Current.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // WebBrowser_Updater
            // 
            this.WebBrowser_Updater.AllowWebBrowserDrop = false;
            this.WebBrowser_Updater.IsWebBrowserContextMenuEnabled = false;
            this.WebBrowser_Updater.Location = new System.Drawing.Point(13, 164);
            this.WebBrowser_Updater.MinimumSize = new System.Drawing.Size(20, 20);
            this.WebBrowser_Updater.Name = "WebBrowser_Updater";
            this.WebBrowser_Updater.ScriptErrorsSuppressed = true;
            this.WebBrowser_Updater.Size = new System.Drawing.Size(280, 177);
            this.WebBrowser_Updater.TabIndex = 5;
            this.WebBrowser_Updater.TabStop = false;
            this.WebBrowser_Updater.Visible = false;
            this.WebBrowser_Updater.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser_Updater_DocumentCompleted);
            // 
            // Label_Download
            // 
            this.Label_Download.BackColor = System.Drawing.Color.Transparent;
            this.Label_Download.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Download.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.Label_Download.Location = new System.Drawing.Point(8, 98);
            this.Label_Download.Margin = new System.Windows.Forms.Padding(8);
            this.Label_Download.Name = "Label_Download";
            this.Label_Download.Size = new System.Drawing.Size(290, 24);
            this.Label_Download.TabIndex = 6;
            this.Label_Download.Text = "Downloading: 00.00%";
            this.Label_Download.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Label_Download.Visible = false;
            // 
            // timer_Delay
            // 
            this.timer_Delay.Interval = 1000;
            this.timer_Delay.Tick += new System.EventHandler(this.Timer_Delay_Tick);
            // 
            // backgroundWorker_Update
            // 
            this.backgroundWorker_Update.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker_Update_DoWork);
            this.backgroundWorker_Update.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_Update_RunWorkerCompleted);
            // 
            // timer_Close
            // 
            this.timer_Close.Interval = 1000;
            this.timer_Close.Tick += new System.EventHandler(this.Timer_Close_Tick);
            // 
            // label_Title
            // 
            this.label_Title.BackColor = System.Drawing.Color.Transparent;
            this.label_Title.Font = new System.Drawing.Font("Arial", 9.75F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Title.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.label_Title.Location = new System.Drawing.Point(12, 17);
            this.label_Title.Margin = new System.Windows.Forms.Padding(8);
            this.label_Title.Name = "label_Title";
            this.label_Title.Size = new System.Drawing.Size(286, 24);
            this.label_Title.TabIndex = 7;
            this.label_Title.Text = "e621 ReBot Updater";
            this.label_Title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form_Updater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.BackgroundImage = global::e621_ReBot_Updater.Properties.Resources.E621ReBotUpdaterBG;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(302, 128);
            this.Controls.Add(this.label_Title);
            this.Controls.Add(this.Label_Current);
            this.Controls.Add(this.Label_Latest);
            this.Controls.Add(this.Label_Download);
            this.Controls.Add(this.WebBrowser_Updater);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Updater";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "e621 ReBot Updater";
            this.TransparencyKey = System.Drawing.Color.DarkGreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Updater_FormClosing);
            this.Load += new System.EventHandler(this.Form_Updater_Load);
            this.Shown += new System.EventHandler(this.Form_Updater_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label Label_Latest;
        internal System.Windows.Forms.Label Label_Current;
        internal System.Windows.Forms.WebBrowser WebBrowser_Updater;
        internal System.Windows.Forms.Label Label_Download;
        private System.Windows.Forms.Timer timer_Delay;
        private System.ComponentModel.BackgroundWorker backgroundWorker_Update;
        private System.Windows.Forms.Timer timer_Close;
        internal System.Windows.Forms.Label label_Title;
    }
}

