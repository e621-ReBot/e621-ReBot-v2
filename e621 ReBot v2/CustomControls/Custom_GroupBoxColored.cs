using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

//https://social.msdn.microsoft.com/Forums/vstudio/en-US/aaebe362-c6cb-4a2d-a8c7-6c27583aa691/changing-the-color-of-groupbox-edge-in-vbnet-how-to-do-this?forum=vbgeneral
namespace e621_ReBot_v2.CustomControls
{
    public class Custom_GroupBoxColored : GroupBox
    {
        public Custom_GroupBoxColored() : base()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            BorderColor = Color.Black;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Custom_GroupBoxColored
            // 
            Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
            ResumeLayout(false);
        }

        private Color _borderColor = Color.Black;
        [Category("Appearance")]
        [Browsable(true)]
        [Description("Set border color")]
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        private int _textOffset = 0;
        [Browsable(true)]
        [Description("Set text offset")]
        public int TextOffset
        {
            get
            {
                return _textOffset;
            }
            set
            {
                _textOffset = value;
                Invalidate();
            }
        }

        private int _BottomBorderFix = 0;
        [Browsable(true)]
        [Description("Set bottom border offset")]
        public int BottomBorderFix
        {
            get
            {
                return _BottomBorderFix;
            }
            set
            {
                _BottomBorderFix = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Size tSize = TextRenderer.MeasureText(Text, Font);

            Rectangle borderRect = e.ClipRectangle;
            borderRect.Y += tSize.Height / 2;
            borderRect.Height -= borderRect.Y + _BottomBorderFix; 
            ControlPaint.DrawBorder(e.Graphics, borderRect, BorderColor, ButtonBorderStyle.Solid);

            Rectangle textRect = e.ClipRectangle;
            textRect.X += 8;
            textRect.Width = tSize.Width + 2 + _textOffset;
            textRect.Height = tSize.Height;

            e.Graphics.FillRectangle(new SolidBrush(BackColor), textRect);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textRect);
        }
    }
}