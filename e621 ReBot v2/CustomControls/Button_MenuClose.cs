using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class Button_MenuClose : Button
    {
        public Button_MenuClose()
        {
            SetStyle(ControlStyles.Selectable, false);
            MinimumSize = new Size(18, 15);
            MaximumSize = new Size(18, 15);
            Size = new Size(18, 15);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            BackgroundImageLayout = ImageLayout.Center;
            BackgroundImage = Properties.Resources.MenuButton_MenuClose;

            MouseEnter += MenuButton_MouseEnter;
            MouseLeave += MenuButton_MouseLeave;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        public void ExtendedDraw(PaintEventArgs e)
        {
            Point[] HexagonPoints = new Point[8];
            HexagonPoints[0] = new Point(0, 6);
            HexagonPoints[1] = new Point(5, 0);
            HexagonPoints[2] = new Point(12, 0);
            HexagonPoints[3] = new Point(17, 6);
            HexagonPoints[4] = new Point(17, 8);
            HexagonPoints[5] = new Point(12, 14);
            HexagonPoints[6] = new Point(5, 14);
            HexagonPoints[7] = new Point(0, 8);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), new Point(5, 13), new Point(12, 13)); // there's no line at the bottom so draw it.
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            BackgroundImage = Properties.Resources.MenuButton_MenuClose_Highlight;
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            BackgroundImage = Properties.Resources.MenuButton_MenuClose;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Button_MenuClose
            // 
            this.Font = new Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.ResumeLayout(false);

        }
    }
}
