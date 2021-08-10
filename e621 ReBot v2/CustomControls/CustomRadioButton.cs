using System;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    class CustomRadioButton: RadioButton
    {

        protected override void OnTabStopChanged(EventArgs e)
        {
            base.OnTabStopChanged(e);

            if (TabStop)
            {
                TabStop = false;
            }           
        }
    }
}
