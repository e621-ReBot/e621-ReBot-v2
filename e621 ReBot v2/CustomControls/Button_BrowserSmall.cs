using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class Button_BrowserSmall:Button
    {
        private double Angle = 0d;

        public double ButtonAngle
        {
            get
            {
                return Angle;
            }

            set
            {
                Angle = value;
                Refresh();
            }
        }

        public Button_BrowserSmall()
        {
            InitializeComponent();
            // Me.SetStyle(ControlStyles.Selectable, False) 'disables PerformClick
            MinimumSize = new Size(32, 32);
            MaximumSize = new Size(32, 32);
            Size = new Size(32, 32);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Button_BrowserSmall
            // 
            Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
            ResumeLayout(false);

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        public void ExtendedDraw(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            int cx = Size.Width / 2;
            int cy = Size.Height / 2;
            int m_N = 6;
            double theta = Angle;
            double dtheta = 2d * Math.PI / m_N;
            var HexagonPoints = new Point[m_N];
            for (int i = 0, loopTo = HexagonPoints.Length - 1; i <= loopTo; i++)
            {
                HexagonPoints[i].X = (int)(cx + cx * Math.Cos(theta));
                HexagonPoints[i].Y = (int)(cy + cy * Math.Sin(theta));
                theta += dtheta;
            }

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
            using (Pen TempPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawPolygon(TempPen, HexagonPoints);
            }
        }
    }
}