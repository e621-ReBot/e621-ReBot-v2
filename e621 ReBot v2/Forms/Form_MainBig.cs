using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2
{
    public partial class Form_MainBig : Form_Main
    {
        public Form_MainBig()
        {
            InitializeComponent();
        }

        private void Form_MainBig_Load(object sender, System.EventArgs e)
        {
            Location = new Point(Screen.FromControl(this).WorkingArea.Width - Width, 0);
            flowLayoutPanel_BrowserButtons.Location = new Point(1556, 2);
            QuickButtonPanel.Location = new Point(QuickButtonPanel.Location.X - 300, QuickButtonPanel.Location.Y - 148); //for some strange reason panel is offset from actual location,return it back
            flowLayoutPanel_Grid.Size = new Size(1862, 950);
            flowLayoutPanel_Grid.Padding = new Padding(0, 32, 0, 0);
        }
    }
}
