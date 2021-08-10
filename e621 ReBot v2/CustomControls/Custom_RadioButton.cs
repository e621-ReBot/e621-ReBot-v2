using System;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public class Custom_RadioButton: RadioButton
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
