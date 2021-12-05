using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Custom_LabelWithStroke : Label
    {
        public Custom_LabelWithStroke()
        {
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Margin = new Padding(0, 0, 0, 0);
            //this.Font = new Font("Arial Black", 16, FontStyle.Bold);
        }

        public int StokeSize { get; set; } = 2;

        public int ExtraSize { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath g = new GraphicsPath())
            {
                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.AddString(Text, Font.FontFamily, (int)Font.Style, Font.Size + ExtraSize, ClientRectangle, sf);
                }
                using (Pen TempPen = new Pen(Color.Black, StokeSize))
                {
                    e.Graphics.DrawPath(TempPen, g);
                }
                e.Graphics.FillPath(new SolidBrush(ForeColor), g);
            }
        }
    }
}