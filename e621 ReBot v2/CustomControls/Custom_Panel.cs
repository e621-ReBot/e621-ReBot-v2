using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class Custom_Panel : Panel
    {
        private int CornerInt = 0;
        public int Corner 
        {
            get { return CornerInt; }
            set 
            {
                if (value > 3)
                {
                    CornerInt = 3;
                }
                else
                {
                    if (value < 0)
                    {
                        CornerInt = 0;
                    }
                    else
                    {
                        CornerInt = value;
                    }
                }
                Region = new Region(GetGraphicPath());
            } 
        }

        public Custom_Panel()
        {
            InitializeComponent();
            Region = new Region(GetGraphicPath());
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Custom_Panel
            // 
            this.Font = new Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.SizeChanged += new System.EventHandler(this.Custom_Panel_SizeChanged);
            this.Paint += new PaintEventHandler(this.Custom_Panel_Paint);
            this.ResumeLayout(false);

        }

        private GraphicsPath GetGraphicPath()
        {
            GraphicsPath TempRegionHolder = new GraphicsPath();
            switch (CornerInt)
            {
                case 0:
                    {
                        TempRegionHolder.AddLine(new Point(0, Height), new Point(0, 0));
                        TempRegionHolder.AddLine(new Point(0, 0), new Point(Width, 0));
                        break;
                    }
                case 1:
                    {
                        TempRegionHolder.AddLine(new Point(0, 0), new Point(Width, 0));
                        TempRegionHolder.AddLine(new Point(Width, 0), new Point(Width, Height));
                        break;
                    }
                case 2:
                    {
                        TempRegionHolder.AddLine(new Point(Width, 0), new Point(Width, Height));
                        TempRegionHolder.AddLine(new Point(Width, Height), new Point(0, Height));
                        break;
                    }
                default:
                    {
                        TempRegionHolder.AddLine(new Point(Width, Height), new Point(0, Height));
                        TempRegionHolder.AddLine(new Point(0, Height), new Point(0, 0));
                        break;
                    }
            }
            TempRegionHolder.CloseFigure();
            return TempRegionHolder;
        }

        private void Custom_Panel_SizeChanged(object sender, System.EventArgs e)
        {
            Region = new Region(GetGraphicPath());
        }

        private void Custom_Panel_Paint(object sender, PaintEventArgs e)
        {
            using (Pen TempPen = new Pen(Color.FromArgb(0, 45, 90), 4))
            {
                e.Graphics.DrawPath(TempPen, GetGraphicPath());
                if (CornerInt % 2 == 0)
                {
                    e.Graphics.DrawLine(TempPen, new Point(Width, 0), new Point(0, Height));
                }
                else
                {
                    e.Graphics.DrawLine(TempPen, new Point(0, 0), new Point(Width, Height));
                }
            }

        }
    }
}
