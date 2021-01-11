using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ACM_AutocompleteMenu
{
    /// <summary>
    /// Control for displaying menu items, hosted in AutocompleteMenu.
    /// </summary>
    public interface ACM_AutocompleteListView
    {
        /// <summary>
        /// Image list
        /// </summary>
        ImageList ImageList { get; set; }

        /// <summary>
        /// Index of current selected item
        /// </summary>
        int SelectedItemIndex { get; set; }

        /// <summary>
        /// Index of current selected item
        /// </summary>
        int HighlightedItemIndex { get; set; }

        /// <summary>
        /// List of visible elements
        /// </summary>
        IList<ACM_AutocompleteItem> VisibleItems { get; set; }

        /// <summary>
        /// Duration (ms) of tooltip showing
        /// </summary>
        int ToolTipDuration { get; set; }

        /// <summary>
        /// Occurs when user selected item for inserting into text
        /// </summary>
        event EventHandler ItemSelected;

        /// <summary>
        /// Occurs when current hovered item is changing
        /// </summary>
        event EventHandler<HoveredEventArgs> ItemHovered;

        /// <summary>
        /// Shows tooltip
        /// </summary>
        /// <param name="autocompleteItem"></param>
        /// <param name="control"></param>
        void ShowToolTip(ACM_AutocompleteItem autocompleteItem, Control control = null);

        /// <summary>
        /// Returns rectangle of item
        /// </summary>
        Rectangle GetItemRectangle(int itemIndex);

        /// <summary>
        /// Colors
        /// </summary>
        ACM_Colors Colors { get; set; }
    }

    [System.ComponentModel.ToolboxItem(false)]
    public class AutocompleteListView : UserControl, ACM_AutocompleteListView
    {
        private readonly ToolTip toolTip = new ToolTip();
        public int HighlightedItemIndex { get; set; }
        private int oldItemCount;
        private int selectedItemIndex = -1;
        private IList<ACM_AutocompleteItem> visibleItems;

        /// <summary>
        /// Duration (ms) of tooltip showing
        /// </summary>
        public int ToolTipDuration { get; set; }

        /// <summary>
        /// Occurs when user selected item for inserting into text
        /// </summary>
        public event EventHandler ItemSelected;


        /// <summary>
        /// Occurs when current hovered item is changing
        /// </summary>
        public event EventHandler<HoveredEventArgs> ItemHovered;

        /// <summary>
        /// Colors
        /// </summary>
        public ACM_Colors Colors { get; set; }

        internal AutocompleteListView()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.Font = new Font(FontFamily.GenericSansSerif, 9);
            ItemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = ItemHeight;
            BackColor = Color.White;
            LeftPadding = 18;
            ToolTipDuration = 3000;
            Colors = new ACM_Colors();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip.Dispose();
            }
            base.Dispose(disposing);
        }


        private int itemHeight;

        public int ItemHeight
        {
            get { return itemHeight; }
            set { 
                itemHeight = value;
                VerticalScroll.SmallChange = value;
                oldItemCount = -1;
                AdjustScroll();
            }
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                ItemHeight = Font.Height + 2;
            }
        }

        public int LeftPadding { get; set; }

        public ImageList ImageList { get; set; }

        public IList<ACM_AutocompleteItem> VisibleItems
        {
            get { return visibleItems; }
            set
            {
                visibleItems = value;
                SelectedItemIndex = -1;
                AdjustScroll();
                Invalidate();
            }
        }

        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                ACM_AutocompleteItem item = null;
                if (value >= 0 && value < VisibleItems.Count)
                    item = VisibleItems[value];

                selectedItemIndex = value;

                OnItemHovered(new HoveredEventArgs() { Item = item });

                if (item != null)
                {
                    ShowToolTip(item);
                    ScrollToSelected();
                }

                Invalidate();
            }
        }

        private void OnItemHovered(HoveredEventArgs e)
        {
            if (ItemHovered != null)
                ItemHovered(this, e);
        }

        private void AdjustScroll()
        {
            if (VisibleItems == null)
                return;
            if (oldItemCount == VisibleItems.Count)
                return;

            int needHeight = ItemHeight*VisibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);
            AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = VisibleItems.Count;
        }


        private void ScrollToSelected()
        {
            int y = SelectedItemIndex*ItemHeight - VerticalScroll.Value;
            if (y < 0)
                VerticalScroll.Value = SelectedItemIndex*ItemHeight;
            if (y > ClientSize.Height - ItemHeight)
                VerticalScroll.Value = Math.Min(VerticalScroll.Maximum,
                                                SelectedItemIndex*ItemHeight - ClientSize.Height + ItemHeight);
            //some magic for update scrolls
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        public Rectangle GetItemRectangle(int itemIndex)
        {
            var y = itemIndex * ItemHeight - VerticalScroll.Value;
            return new Rectangle(0, y, ClientSize.Width - 1, ItemHeight - 1);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Colors.BackColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool rtl = RightToLeft == RightToLeft.Yes;
            AdjustScroll();
            int startI = VerticalScroll.Value/ItemHeight - 1;
            int finishI = (VerticalScroll.Value + ClientSize.Height)/ItemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, VisibleItems.Count);
            for (int i = startI; i < finishI; i++)
            {
                int y = i * ItemHeight - VerticalScroll.Value;
                if (ImageList != null && VisibleItems[i].ImageIndex >= 0)
                    if (rtl)
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], Width - 1 - LeftPadding, y);
                    else
                        e.Graphics.DrawImage(ImageList.Images[VisibleItems[i].ImageIndex], 1, y);

                var textRect = new Rectangle(LeftPadding, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);
                if (rtl)
                    textRect = new Rectangle(1, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);

                if (i == SelectedItemIndex)
                {
                    Brush selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + ItemHeight),
                                                                  Colors.SelectedBackColor2, Colors.SelectedBackColor);
                    e.Graphics.FillRectangle(selectedBrush, textRect);
                    using(var pen = new Pen(Colors.SelectedBackColor2))
                        e.Graphics.DrawRectangle(pen, textRect);
                }

                if (i == HighlightedItemIndex)
                using (var pen = new Pen(Colors.HighlightingColor))
                    e.Graphics.DrawRectangle(pen, textRect);

                var sf = new StringFormat();
                if (rtl)
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                var args = new PaintItemEventArgs(e.Graphics, e.ClipRectangle)
                               {
                                   Font = Font,
                                   TextRect = new RectangleF(textRect.Location, textRect.Size),
                                   StringFormat = sf,
                                   IsSelected = i == SelectedItemIndex,
                                   IsHovered = i == HighlightedItemIndex,
                                   Colors = Colors
                               };
                //call drawing
                VisibleItems[i].OnPaint(args);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(true);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                SelectedItemIndex = PointToItemIndex(e.Location);
                ScrollToSelected();
                Invalidate();
            }
        }

        private Point mouseEnterPoint;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseEnterPoint = Control.MousePosition;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseEnterPoint != Control.MousePosition)
            {
                HighlightedItemIndex = PointToItemIndex(e.Location);
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SelectedItemIndex = PointToItemIndex(e.Location);
            Invalidate();
            OnItemSelected();
        }

        private void OnItemSelected()
        {
            if (ItemSelected != null)
                ItemSelected(this, EventArgs.Empty);
        }

        private int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value)/ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var host = Parent as AutocompleteMenuHost;
            if (host != null)
                if (host.Menu.ProcessKey((char) keyData, Keys.None))
                    return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SelectItem(int itemIndex)
        {
            SelectedItemIndex = itemIndex;
            ScrollToSelected();
            Invalidate();
        }

        public void SetItems(List<ACM_AutocompleteItem> items)
        {
            VisibleItems = items;
            SelectedItemIndex = -1;
            AdjustScroll();
            Invalidate();
        }

        public void ShowToolTip(ACM_AutocompleteItem autocompleteItem, Control control = null)
        {
            string title = autocompleteItem.ToolTipTitle;
            string text = autocompleteItem.ToolTipText;
            if (control == null)
                control = this;

            if (string.IsNullOrEmpty(title))
            {
                toolTip.ToolTipTitle = null;
                toolTip.SetToolTip(control, null);
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                toolTip.ToolTipTitle = null;
                toolTip.Show(title, control, Width + 3, 0, ToolTipDuration);
            }
            else
            {
                toolTip.ToolTipTitle = title;
                toolTip.Show(text, control, Width + 3, 0, ToolTipDuration);
            }
        }
    }
}