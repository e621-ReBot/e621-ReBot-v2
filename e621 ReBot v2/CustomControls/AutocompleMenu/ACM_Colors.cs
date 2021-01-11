using System;
using System.Drawing;

namespace ACM_AutocompleteMenu
{
    [Serializable]
    public class ACM_Colors
    {
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public Color SelectedForeColor { get; set; }
        public Color SelectedBackColor { get; set; }
        public Color SelectedBackColor2 { get; set; }
        public Color HighlightingColor { get; set; }

        public ACM_Colors()
        {
            ForeColor = Color.Black;
            BackColor = Color.White;
            SelectedForeColor = Color.Black;
            SelectedBackColor = Color.RoyalBlue; //Bottom color
            SelectedBackColor2 = Color.LightSteelBlue; //top color
            HighlightingColor = Color.Orange;
        }
    }
}
