using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ACM_AutocompleteMenu
{
    /// <summary>
    /// Item of autocomplete menu
    /// </summary>
    public class ACM_AutocompleteItem
    {
        public object Tag;
        string toolTipTitle;
        string toolTipText;
        string menuText;

        /// <summary>
        /// Parent AutocompleteMenu
        /// </summary>
        public AutocompleteMenu Parent { get; internal set; }

        /// <summary>
        /// Text for inserting into textbox
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Image index for this item
        /// </summary>
        public int ImageIndex { get; set; }

        /// <summary>
        /// Title for tooltip.
        /// </summary>
        /// <remarks>Return null for disable tooltip for this item</remarks>
        public virtual string ToolTipTitle
        {
            get { return toolTipTitle; }
            set { toolTipTitle = value; }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        /// <remarks>For display tooltip text, ToolTipTitle must be not null</remarks>
        public virtual string ToolTipText
        {
            get { return toolTipText; }
            set { toolTipText = value; }
        }

        /// <summary>
        /// Menu text. This text is displayed in the drop-down menu.
        /// </summary>
        public virtual string MenuText
        {
            get { return menuText; }
            set { menuText = value; }
        }


        public ACM_AutocompleteItem()
        {
            ImageIndex = -1;
        }

        public ACM_AutocompleteItem(string text) : this()
        {
            Text = text;
        }

        public ACM_AutocompleteItem(string text, int imageIndex)
            : this(text)
        {
            ImageIndex = imageIndex;
        }

        public ACM_AutocompleteItem(string text, int imageIndex, string menuText)
            : this(text, imageIndex)
        {
            this.menuText = menuText;
        }

        public ACM_AutocompleteItem(string text, int imageIndex, string menuText, string toolTipTitle, string toolTipText)
            : this(text, imageIndex, menuText)
        {
            this.toolTipTitle = toolTipTitle;
            this.toolTipText = toolTipText;
        }

        /// <summary>
        /// Returns text for inserting into Textbox
        /// </summary>
        public virtual string GetTextForReplace()
        {
            return Text;
        }

        /// <summary>
        /// Compares fragment text with this item
        /// </summary>
        public virtual CompareResult Compare(string fragmentText)
        {
            if (Text.StartsWith(fragmentText, StringComparison.InvariantCultureIgnoreCase) &&
                   Text != fragmentText)
                return CompareResult.VisibleAndSelected;

            return CompareResult.Hidden;
        }

        /// <summary>
        /// Returns text for display into popup menu
        /// </summary>
        public override string ToString()
        {
            return menuText ?? Text;
        }

        /// <summary>
        /// This method is called after item was inserted into text
        /// </summary>
        public virtual void OnSelected(SelectedEventArgs e)
        {
        }

        public virtual void OnPaint(PaintItemEventArgs e)
        {
            using (var brush = new SolidBrush(e.IsSelected ? e.Colors.SelectedForeColor : e.Colors.ForeColor))
                e.Graphics.DrawString(ToString(), e.Font, brush, e.TextRect, e.StringFormat);
        }
    }

    public enum CompareResult
    {
        /// <summary>
        /// Item do not appears
        /// </summary>
        Hidden,
        /// <summary>
        /// Item appears
        /// </summary>
        Visible,
        /// <summary>
        /// Item appears and will selected
        /// </summary>
        VisibleAndSelected
    }

    public class MulticolumnAutocompleteItem : ACM_AutocompleteItem
    {
        protected readonly bool CompareBySubstring = true; //{ get; set; }
        public string[] MenuTextByColumns { get; set; }

        protected readonly string lowercaseText;

        protected readonly string AlternateSearch;
        public int[] ColumnWidth { get; set; }

        public MulticolumnAutocompleteItem(string[] menuTextByColumns, string insertingText, string alternateSearch = null)
            : base(insertingText)
        {
            MenuTextByColumns = menuTextByColumns;
            lowercaseText = insertingText.ToLower();
            AlternateSearch = alternateSearch.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            //comic name
            if (AlternateSearch != null && AlternateSearch.Contains(fragmentText.ToLower()))
            {
                return CompareResult.Visible;
            }

            //comic id, but exclude "pool:" or any startwith version
            if (Regex.IsMatch(fragmentText, @"(pool:)\S+")) //@"^((?!pool:|pool|poo|po|p).)*$"))
            {
                if (lowercaseText.Contains(fragmentText.ToLower()))
                {
                    return CompareResult.Visible;
                }
            }

            //if (lowercaseText.StartsWith(fragmentText, StringComparison.OrdinalIgnoreCase))
            //{
            //    return CompareResult.VisibleAndSelected;
            //}

            return CompareResult.Hidden;
        }

        public override void OnPaint(PaintItemEventArgs e)
        {
            if (ColumnWidth != null && ColumnWidth.Length != MenuTextByColumns.Length)
                throw new Exception("ColumnWidth.Length != MenuTextByColumns.Length");

            int[] columnWidth = ColumnWidth;
            if (columnWidth == null)
            {
                columnWidth = new int[MenuTextByColumns.Length];
                float step = e.TextRect.Width / MenuTextByColumns.Length;
                for (int i = 0; i < MenuTextByColumns.Length; i++)
                    columnWidth[i] = (int)step;
            }

            //draw columns
            Pen pen = Pens.Silver;
            float x = e.TextRect.X;
            e.StringFormat.FormatFlags = e.StringFormat.FormatFlags | StringFormatFlags.NoWrap;

            using (var brush = new SolidBrush(e.IsSelected ? e.Colors.SelectedForeColor : e.Colors.ForeColor))
                for (int i = 0; i < MenuTextByColumns.Length; i++)
                {
                    var width = columnWidth[i];
                    var rect = new RectangleF(x, e.TextRect.Top, width, e.TextRect.Height);
                    e.Graphics.DrawLine(pen, new PointF(x, e.TextRect.Top), new PointF(x, e.TextRect.Bottom));
                    e.Graphics.DrawString(MenuTextByColumns[i], e.Font, brush, rect, e.StringFormat);
                    x += width;
                }
        }
    }
}
