using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Custom_FlowLayoutPanel : FlowLayoutPanel
    {
        public Custom_FlowLayoutPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Custom_FlowLayoutPanel
            // 
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.ResumeLayout(false);

        }
    }
}
