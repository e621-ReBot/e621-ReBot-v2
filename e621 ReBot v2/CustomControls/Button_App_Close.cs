using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Button_App_Close : PictureBox
    {
        public Button_App_Close()
        {
            SetStyle(ControlStyles.Selectable, false);
            Width = 22;
            Height = 25;

            MouseEnter += Close_Btn_Hover;
            MouseLeave += Close_Btn_Hover;
            Click += Button_App_Close_Click;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        private void ExtendedDraw(PaintEventArgs e)
        {
            Point[] HexagonPoints = new Point[8];
            HexagonPoints[0] = new Point(0, 4);
            HexagonPoints[1] = new Point(7, 0);
            HexagonPoints[2] = new Point(13, 0);
            HexagonPoints[3] = new Point(22, 5);
            HexagonPoints[4] = new Point(22, 19);
            HexagonPoints[5] = new Point(13, 24);
            HexagonPoints[6] = new Point(8, 24);
            HexagonPoints[7] = new Point(0, 19);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
        }

        private void Close_Btn_Hover(object sender, EventArgs e)
        {
            BackgroundImage = BackgroundImage == null ? Properties.Resources.CloseBtn_Active : null;
        }

        private void Button_App_Close_Click(object sender, EventArgs e)
        {
            Form_Loader._FormReference.Close();
        }
    }
}
