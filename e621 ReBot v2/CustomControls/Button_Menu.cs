using e621_ReBot_v2.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class Button_Menu : PictureBox
    {
        public Button_Menu()
        {
            SetStyle(ControlStyles.Selectable, false);
            MinimumSize = new Size(34, 31);
            MaximumSize = new Size(34, 31);
            Size = new Size(34, 31);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            SizeMode = PictureBoxSizeMode.CenterImage;
            BackgroundImageLayout = ImageLayout.Center;
            BackgroundImage = Properties.Resources.Menu_Button;

            MouseEnter += Button_Menu_MouseEnter;
            MouseLeave += Button_Menu_MouseLeave;
            Click += Button_Menu_Click;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ExtendedDraw(e);
        }

        public void ExtendedDraw(PaintEventArgs e)
        {
            Point[] HexagonPoints = new Point[8];
            HexagonPoints[0] = new Point(0, 12);
            HexagonPoints[1] = new Point(8, 0);
            HexagonPoints[2] = new Point(25, 0);
            HexagonPoints[3] = new Point(34, 12);
            HexagonPoints[4] = new Point(34, 18);
            HexagonPoints[5] = new Point(26, 31);
            HexagonPoints[6] = new Point(8, 31);
            HexagonPoints[7] = new Point(0, 18);
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(HexagonPoints);
            Region = new Region(path);
        }

        private void Button_Menu_MouseEnter(object sender, System.EventArgs e)
        {
            MB_Highlight();
        }

        private void Button_Menu_MouseLeave(object sender, System.EventArgs e)
        {
            if (!Active)
            {
                Image = null;
            }
        }

        public void MB_Highlight()
        {
            string ConstructName = Name;
            ConstructName = ConstructName.Substring(ConstructName.IndexOf("_"));
            Image = (Image)Properties.Resources.ResourceManager.GetObject("MenuButton" + ConstructName + "_Highlight");
        }

        public bool Active;
        private void Button_Menu_Click(object sender, System.EventArgs e)
        {
            Active = true;
            Form_Loader._FormReference.Focus();
            Form_Loader._FormReference.cTabControl_e621ReBot.SelectedIndex = int.Parse(Tag.ToString());
            ClearOthers();
        }

        public void ClearOthers()
        {
            foreach (Button_Menu TempBtn in Form_Menu._FormReference.Menu_FlowLayoutPanel.Controls.OfType<Button_Menu>())
            {
                if (TempBtn != this)
                {
                    TempBtn.Active = false;
                    TempBtn.Image = null;
                }
            }
        }
    }
}
