using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{

    public enum CustomPBStyle
    {
        Hex,
        Round
    }

    public partial class Custom_ProgressBar : UserControl
    {
        #region

        private CustomPBStyle _pbStyle = CustomPBStyle.Hex;
        [Category("Appearance")]
        [Browsable(true)]
        [Description("Set Progress Bar style")]
        public CustomPBStyle PBStyle
        {
            get
            {
                return _pbStyle;
            }

            set
            {
                _pbStyle = value;
                Invalidate();
            }
        }

        private Color _barColor = Color.RoyalBlue;
        [Category("Appearance")]
        [Browsable(true)]
        [Description("Set bar color")]
        public Color BarColor
        {
            get
            {
                return _barColor;
            }

            set
            {
                _barColor = value;
                Invalidate();
            }
        }

        private Color _lineColor = Color.DimGray;
        [Category("Appearance")]
        [Browsable(true)]
        [Description("Set line color")]
        public Color LineColor
        {
            get
            {
                return _lineColor;
            }

            set
            {
                _lineColor = value;
                Invalidate();
            }
        }

        private int min = 0; // Minimum value for progress range
        public int Minimum
        {
            set
            {
                // Prevent a negative value.
                if (value < 0)
                {
                    min = 0;
                }

                // Make sure that the minimum value is never set higher than the maximum value.
                if (value > max)
                {
                    min = value;
                    min = value;
                }

                // Make sure that the value is still in range.
                if (val < min)
                {
                    val = min;
                }
                Invalidate();
            }

            get
            {
                return min;
            }
        }

        private int max = 100; // Maximum value for progress range
        public int Maximum
        {
            set
            {
                // Make sure that the maximum value is never set lower than the minimum value.
                if (value < min)
                {
                    min = value;
                }

                max = value;

                // Make sure that the value is still in range.
                if (val > max)
                {
                    val = max;
                }
                Invalidate();
            }

            get
            {
                return max;
            }
        }

        private int val = 0; // Current progress
        public int Value
        {
            set
            {
                // Make sure that the value does not stray outside the valid range.
                if (value < min)
                {
                    val = min;
                }
                else if (value > max)
                {
                    val = max;
                }
                else
                {
                    val = value;
                }
                Invalidate();
            }

            get
            {
                return val;
            }
        }


        private int _lineThickness = 4;
        public int LineThickness
        {
            get
            {
                return _lineThickness;
            }

            set
            {
                _lineThickness = value;
                Invalidate();
            }
        }

        private int _barThickness = 8;
        public int BarThickness
        {
            get
            {
                return _barThickness;
            }

            set
            {
                _barThickness = value;
                Invalidate();
            }
        }

        private int _strokeThickness = 2;
        public int StrokeThickness
        {
            get
            {
                return _strokeThickness;
            }

            set
            {
                _strokeThickness = value;
                Invalidate();
            }
        }

        private Point[] HexStrokeEdgeOuter;
        private Point[] HexStrokeEdgeInner;
        private Point[] HexEdgeOuter;
        private Point[] HexEdgeInner;
        private Point[] HexLinePoints;

        internal ToolTip toolTip_cPB;
        private IContainer components;

        #endregion

        public Custom_ProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            InitializeComponent();
            CreateHexPoints();
        }

        public Custom_ProgressBar(string namePass, int maxPass)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            Text = namePass;
            Maximum = maxPass;
            InitializeComponent();
            CreateHexPoints();
        }

        private void InitializeComponent()
        {
            components = new Container();
            toolTip_cPB = new ToolTip(components);
            SuspendLayout();
            // 
            // toolTip_cPB
            // 
            toolTip_cPB.AutomaticDelay = 0;
            // 
            // Custom_ProgressBar
            // 
            BackColor = Color.Transparent;
            Margin = new Padding(0, 4, 0, 2);
            MaximumSize = new Size(48, 48);
            MinimumSize = new Size(48, 48);
            Size = new Size(48, 48);
            toolTip_cPB.SetToolTip(this, Text);
            ResumeLayout(false);
            Paint += CProgressBar_Paint;

        }

        private void CreateHexPoints()
        {
            HexStrokeEdgeOuter = new Point[6];
            HexStrokeEdgeInner = new Point[6];
            HexEdgeOuter = new Point[6];
            HexEdgeInner = new Point[6];
            HexLinePoints = new Point[6];
            for (int i = 0; i < 6; i++)
            {
                HexStrokeEdgeOuter[i] = new Point(
                    (int)(24 + (24 - 0) * (float)Math.Cos(i * 60 * Math.PI / 180f)),
                    (int)(24 + (24 - 0) * (float)Math.Sin(i * 60 * Math.PI / 180f)));
                HexStrokeEdgeInner[i] = new Point(
                    (int)(24 + (24 - 14) * (float)Math.Cos(i * 60 * Math.PI / 180f)),
                    (int)(24 + (24 - 14) * (float)Math.Sin(i * 60 * Math.PI / 180f)));
                HexEdgeOuter[i] = new Point(
                    (int)(24 + (24 - 1) * (float)Math.Cos(i * 60 * Math.PI / 180f)),
                    (int)(24 + (24 - 1) * (float)Math.Sin(i * 60 * Math.PI / 180f)));
                HexEdgeInner[i] = new Point(
                    (int)(24 + (24 - 13) * (float)Math.Cos(i * 60 * Math.PI / 180f)),
                    (int)(24 + (24 - 13) * (float)Math.Sin(i * 60 * Math.PI / 180f)));
                HexLinePoints[i] = new Point(
                    (int)(24 + (24 - 7) * (float)Math.Cos(i * 60 * Math.PI / 180f)),
                    (int)(24 + (24 - 7) * (float)Math.Sin(i * 60 * Math.PI / 180f)));
            }
        }

        private void CProgressBar_Paint(object sender, PaintEventArgs e)
        {
            using (Bitmap NewBitmapTemp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                NewBitmapTemp.MakeTransparent();
                using (Graphics gTemp = Graphics.FromImage(NewBitmapTemp))
                {
                    //gTemp.CompositingMode = CompositingMode.SourceCopy;
                    //gTemp.CompositingQuality = CompositingQuality.HighQuality;
                    gTemp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gTemp.SmoothingMode = SmoothingMode.HighQuality;
                    gTemp.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    Pen TempPen = new Pen(_lineColor, _lineThickness);
                    _pbStyle = Form_Loader._customPBStyle | _pbStyle;
                    if (_pbStyle == CustomPBStyle.Hex)
                    {

                        if (Value == 0)
                        {
                            //This takes ~150ms for 10k loops
                            gTemp.DrawPolygon(TempPen, HexLinePoints);
                        }
                        else
                        {
                            //This takes ~400ms for 10k loops
                            using (Pen BarPen1 = new Pen(Color.Black, _barThickness + _strokeThickness))
                            {
                                gTemp.DrawPolygon(BarPen1, HexLinePoints);
                            }
                            using (Pen BarPen2 = new Pen(_barColor, _barThickness))
                            {
                                gTemp.DrawPolygon(BarPen2, HexLinePoints);
                            }

                            using (GraphicsPath ProgressPerc = new GraphicsPath())
                            {
                                float PartPerc = (float)Value / Maximum * 360f;
                                ProgressPerc.AddPie(ClientRectangle, PartPerc, 360 - PartPerc);
                                using (Region ClearRegion = new Region(ProgressPerc))
                                {
                                    gTemp.Clip = ClearRegion;
                                    gTemp.Clear(Color.Transparent);
                                    gTemp.DrawPolygon(TempPen, HexLinePoints);
                                    gTemp.ResetClip();
                                }
                            }

                            //    GraphicsPath barPath = new GraphicsPath();
                            //    Point EndLinePoint = Point.Empty;

                            //    // Which hexdrant is it
                            //    int HexdrantIndex = (int)(3.6d * Value - 1) / 60;

                            //    for (int i = 0; i < 6; i++)
                            //    {
                            //        if (i == HexdrantIndex)
                            //        {
                            //            //https://www.geeksforgeeks.org/program-for-point-of-intersection-of-two-lines/
                            //            double a1 = HexLinePoints[HexdrantIndex == 5 ? 0 : HexdrantIndex + 1].Y - HexLinePoints[HexdrantIndex].Y;
                            //            double b1 = HexLinePoints[HexdrantIndex].X - (HexdrantIndex == 5 ? HexLinePoints[0].X : HexLinePoints[HexdrantIndex + 1].X);
                            //            double c1 = a1 * HexLinePoints[HexdrantIndex].X + b1 * HexLinePoints[HexdrantIndex].Y;

                            //            double angleRadians = (Math.PI / 180) * 3.6d * Value;
                            //            double a2 = Math.Round(24 + Math.Sin(angleRadians) * 24) - 24;
                            //            double b2 = 24 - Math.Round(24 + Math.Cos(angleRadians) * 24);
                            //            double c2 = a2 * 24 + b2 * 24;

                            //            double determinant = a1 * b2 - a2 * b1;
                            //            int ix = (int)((b2 * c1 - b1 * c2) / determinant);
                            //            int iy = (int)((a1 * c2 - a2 * c1) / determinant);

                            //            if (i == 0)
                            //            {
                            //                EndLinePoint = new Point(ix, iy);
                            //            }
                            //            else
                            //            {
                            //                barPath.AddLine(new Point(HexLinePoints[HexdrantIndex].X, HexLinePoints[HexdrantIndex].Y), new Point(ix, iy));
                            //            }
                            //        }
                            //        else
                            //        {
                            //            if (i < HexdrantIndex)
                            //            {
                            //                if (i == 0)
                            //                {
                            //                    barPath.AddLine(new Point(38, 29), new Point(HexLinePoints[1].X, HexLinePoints[1].Y));
                            //                }
                            //                else
                            //                {
                            //                    barPath.AddLine(new Point(HexLinePoints[i].X, HexLinePoints[i].Y), new Point(HexLinePoints[i == 5 ? 0 : i + 1].X, HexLinePoints[i == 5 ? 0 : i + 1].Y));
                            //                }
                            //            }
                            //        }
                            //    }
                            //    using (Pen barPen1 = new Pen(Color.Black, 10))
                            //    {
                            //        gTemp.DrawPath(barPen1, barPath);
                            //        gTemp.SetClip(new Rectangle(24, 24, 24, 24));
                            //        gTemp.DrawLine(barPen1, HexdrantIndex == 0 ? EndLinePoint : new Point(36, 32), new Point(44, 18)); //41, 24
                            //        gTemp.ResetClip();
                            //    }
                            //    using (Pen barPen2 = new Pen(_barColor, 8))
                            //    {
                            //        gTemp.DrawPath(barPen2, barPath);
                            //        gTemp.SetClip(new Rectangle(24, 24, 24, 24));
                            //        gTemp.DrawLine(barPen2, HexdrantIndex == 0 ? EndLinePoint : new Point(36, 32), new Point(44, 18));
                            //        gTemp.ResetClip();
                            //    }
                            //    barPath.Dispose();
                        }
                    }
                    else
                    {
                        float progressAngle = (float)Value / Maximum * 360f;
                        float remainderAngle = 360f - progressAngle;
                        gTemp.DrawEllipse(TempPen, new Rectangle(new Point(8, 8), new Size(32, 32)));

                        Rectangle ArcRectangle = new Rectangle(new Point(8, 8), new Size(32, 32));
                        using (Pen TempPenStroke = new Pen(Color.Black, _barThickness + _strokeThickness))
                        {
                            gTemp.DrawArc(TempPenStroke, ArcRectangle, 0, progressAngle);
                        }
                        using (Pen TempPenLineMain = new Pen(_barColor, _barThickness))
                        {
                            gTemp.DrawArc(TempPenLineMain, ArcRectangle, 0, progressAngle);
                        }
                    }
                    TempPen.Dispose();

                    //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    e.Graphics.DrawImage(NewBitmapTemp, 0, 0, Width, Height);
                }
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
