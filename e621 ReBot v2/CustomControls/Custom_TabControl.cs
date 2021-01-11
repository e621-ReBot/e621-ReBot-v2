using System;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Custom_TabControl : TabControl
    {
        public Custom_TabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            Appearance = TabAppearance.FlatButtons;
            ItemSize = new Size(0, 1);
            SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tab in TabPages)
            {
                tab.Text = "";
            }
            BackColor = Color.FromArgb(0, 45, 90);

            //Size read wrongly here.
            //Region = new Region(new Rectangle(new Point(4, 5), new Size(Width - 8, Height - 9)));

            Resize += Custom_TabControl_Resize;
        }

        private void Custom_TabControl_Resize(object sender, EventArgs e)
        {
            //if (DesignMode)
            //{
            //    return;
            //}

            Region = new Region(new Rectangle(new Point(4, 5), new Size(Width - 8, Height - 9)));
        }
    }
}
