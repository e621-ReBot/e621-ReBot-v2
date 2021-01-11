using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Button_App_Minimize : PictureBox
    {
        public Button_App_Minimize()
        {
            SetStyle(ControlStyles.Selectable, false);
            Width = 18;
            Height = 18;

            MouseEnter += Minimize_Btn_Hover;
            MouseLeave += Minimize_Btn_Hover;
            Click += Button_App_Minimize_Click;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        private void ExtendedDraw(PaintEventArgs e)
        {
            Point[] HexagonPoints = new Point[8];
            HexagonPoints[0] = new Point(0, 3);
            HexagonPoints[1] = new Point(6, 0);
            HexagonPoints[2] = new Point(13, 0);
            HexagonPoints[3] = new Point(17, 3);
            HexagonPoints[4] = new Point(17, 15);
            HexagonPoints[5] = new Point(12, 17);
            HexagonPoints[6] = new Point(6, 17);
            HexagonPoints[7] = new Point(0, 14);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
        }

        private void Minimize_Btn_Hover(object sender, EventArgs e)
        {
            BackgroundImage = BackgroundImage == null ? Properties.Resources.Minimize_Btn : null;
        }

        private void Button_App_Minimize_Click(object sender, EventArgs e)
        {
            Form_Loader._FormReference.WindowState = FormWindowState.Minimized;
        }
    }
}
