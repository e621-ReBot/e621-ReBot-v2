using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Button_Unfocusable : Button
    {
        public Button_Unfocusable()
        {
            SetStyle(ControlStyles.Selectable, false);
            InitializeComponent();
            FlatStyle = FlatStyle.Flat;
            TabStop = false;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Button_Unfocusable
            // 
            Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
            ResumeLayout(false);
        }
    }
}
