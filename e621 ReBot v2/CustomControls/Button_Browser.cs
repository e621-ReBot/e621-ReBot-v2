using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class Button_Browser : Button
    {
        public Button_Browser()
        {
            SetStyle(ControlStyles.Selectable, false);
            MinimumSize = new Size(64, 56);
            MaximumSize = new Size(64, 56);
            Size = new Size(64, 56);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Text = "";
            BackgroundImageLayout = ImageLayout.Zoom;
            BackgroundImage = Properties.Resources.MenuIcon_Info;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        public void ExtendedDraw(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            Point[] HexagonPoints = new Point[8];
            HexagonPoints[0] = new Point(15, 0);
            HexagonPoints[1] = new Point(49, 0);
            HexagonPoints[2] = new Point(64, 26);
            HexagonPoints[3] = new Point(64, 29);
            HexagonPoints[4] = new Point(49, 56);
            HexagonPoints[5] = new Point(15, 56);
            HexagonPoints[6] = new Point(0, 29);
            HexagonPoints[7] = new Point(0, 26);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
            // e.Graphics.DrawPolygon(New Pen(Color.Black, 2), HexagonPoints)
            using (Pen TempPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(TempPen, HexagonPoints[0], HexagonPoints[1]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[1], HexagonPoints[2]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[2], HexagonPoints[3]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[3], HexagonPoints[4]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[4], HexagonPoints[5]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[5], HexagonPoints[6]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[6], HexagonPoints[7]);
                e.Graphics.DrawLine(TempPen, HexagonPoints[7], HexagonPoints[0]);
            }         
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Button_Browser
            // 
            Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            ResumeLayout(false);

        }
    }
}
