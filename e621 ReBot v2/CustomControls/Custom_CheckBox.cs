using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Custom_CheckBox :CheckBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(new Point(0, 0), new Size(16, 16)));
            if (Checked)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.LimeGreen), new Rectangle(new Point(1, 1), new Size(15, 15)));
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.DrawString("✔", new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Pixel), new SolidBrush(Color.Black), new Point(0, 2));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.DimGray), new Rectangle(new Point(1, 1), new Size(15, 15)));
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Custom_CheckBox
            // 
            Font = new Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            ResumeLayout(false);

        }
    }
}
