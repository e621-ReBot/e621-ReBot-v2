using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public partial class e6_DownloadItem : UserControl
    {
        public e6_DownloadItem()
        {
            InitializeComponent();
        }

        private void e6_DownloadItem_Load(object sender, System.EventArgs e)
        {
            SuspendLayout();
            DL_ProgressBar.Parent = picBox_ImageHolder;
            DL_FolderIcon.Parent = picBox_ImageHolder;
            ResumeLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.SteelBlue, ButtonBorderStyle.Solid);
        }

        private void DL_FolderIcon_Click(object sender, System.EventArgs e)
        {
            Process.Start("explorer.exe", string.Format("/select, \"{0}\"", DL_FolderIcon.Tag.ToString()));
        }

        private void PicBox_ImageHolder_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            picBox_ImageHolder.BackgroundImage = picBox_ImageHolder.Image;
            picBox_ImageHolder.Image = null;
            picBox_ImageHolder.Tag = null;
        }

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
    }
}
