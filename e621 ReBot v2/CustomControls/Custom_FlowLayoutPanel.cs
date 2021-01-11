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
    }
}
