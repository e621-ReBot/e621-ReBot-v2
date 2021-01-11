using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Button_Unfocusable : Button
    {
        public Button_Unfocusable()
        {
            SetStyle(ControlStyles.Selectable, false);
            FlatStyle = FlatStyle.Flat;
            Font = new Font("Microsoft Sans Serif", 11, FontStyle.Regular, GraphicsUnit.Pixel);
            TabStop = false;
        }
    }
}
