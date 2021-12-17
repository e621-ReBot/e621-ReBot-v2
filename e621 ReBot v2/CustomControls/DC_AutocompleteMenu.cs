using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DC_AutocompleteMenuNS
{
    [ToolboxItem(true)]
    [ProvideProperty("AutocompleteMenu", typeof(Control))]
    public class DC_AutocompleteMenu : Component, IExtenderProvider

    {
        public DC_AutocompleteMenu()
        {
            Host = new DC_AutocompleteMenuHost(this);
            Host.ListView.ItemSelected += new EventHandler(ListView_ItemSelected);
            Host.ListView.ItemHovered += new EventHandler<HoveredEventArgs>(ListView_ItemHovered);
            VisibleItems = new List<DC_AutocompleteItem>();
            Enabled = true;
            AppearInterval = 100;
            timer.Tick += Timer_Tick;
            MaximumSize = new Size(200, 200);
            AutoPopup = true;

            SearchPattern = @"[\S]";
            MinFragmentLength = 2;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
                Host.Dispose();
            }
            base.Dispose(disposing);
        }

        private static readonly Dictionary<Control, DC_AutocompleteMenu> AutocompleteMenuByControls = new Dictionary<Control, DC_AutocompleteMenu>();
        private static readonly Dictionary<Control, IDC_TextBoxWrapper> WrapperByControls = new Dictionary<Control, IDC_TextBoxWrapper>();

        private IDC_TextBoxWrapper targetControlWrapper;
        private readonly Timer timer = new Timer();

        private IEnumerable<DC_AutocompleteItem> sourceItems = new List<DC_AutocompleteItem>();
        [Browsable(false)]
        public IList<DC_AutocompleteItem> VisibleItems { get { return Host.ListView.VisibleItems; } private set { Host.ListView.VisibleItems = value; } }
        private Size maximumSize;

        /// <summary>
        /// Tooltip shown duration (in ms)
        /// </summary>
        [Description("Tooltip shown duration (in ms)")]
        [DefaultValue(3000)]
        public int ToolTipDuration
        {
            get { return Host.ListView.ToolTipDuration; }
            set { Host.ListView.ToolTipDuration = value; }
        }

        void ListView_ItemSelected(object sender, EventArgs e)
        {
            OnSelecting();
        }

        void ListView_ItemHovered(object sender, HoveredEventArgs e)
        {
            OnHovered(e);
        }

        public void OnHovered(HoveredEventArgs e)
        {
            if (Hovered != null)
                Hovered(this, e);
        }

        [Browsable(false)]
        public int SelectedItemIndex
        {
            get { return Host.ListView.SelectedItemIndex; }
            internal set { Host.ListView.SelectedItemIndex = value; }
        }

        internal DC_AutocompleteMenuHost Host { get; set; }

        /// <summary>
        /// Called when user selected the control and needed wrapper over it.
        /// You can assign own Wrapper for target control.
        /// </summary>
        [Description("Called when user selected the control and needed wrapper over it. You can assign own Wrapper for target control.")]
        public event EventHandler<WrapperNeededEventArgs> WrapperNeeded;

        protected void OnWrapperNeeded(WrapperNeededEventArgs args)
        {
            if (WrapperNeeded != null)
            {
                WrapperNeeded(this, args);
            }
            if (args.Wrapper == null)
            {
                args.Wrapper = TextBoxWrapper.Create(args.TargetControl);
            }
        }

        IDC_TextBoxWrapper CreateWrapper(Control control)
        {
            if (WrapperByControls.ContainsKey(control))
            {
                return WrapperByControls[control];
            }

            var args = new WrapperNeededEventArgs(control);
            OnWrapperNeeded(args);
            if (args.Wrapper != null)
            {
                WrapperByControls[control] = args.Wrapper;
            }
            return args.Wrapper;
        }

        /// <summary>
        /// Current target control wrapper
        /// </summary>
        [Browsable(false)]
        public IDC_TextBoxWrapper TargetControlWrapper
        {
            get { return targetControlWrapper; }
            set
            {
                targetControlWrapper = value;
                if (value != null && !WrapperByControls.ContainsKey(value.TargetControl))
                {
                    WrapperByControls[value.TargetControl] = value;
                    SetAutocompleteMenu(value.TargetControl, this);
                }
            }
        }

        /// <summary>
        /// Maximum size of popup menu
        /// </summary>
        [DefaultValue(typeof(Size), "200, 200")]
        [Description("Maximum size of popup menu")]
        public Size MaximumSize
        {
            get { return maximumSize; }
            set
            {
                maximumSize = value;
                (Host.ListView as Control).MaximumSize = maximumSize;
                (Host.ListView as Control).Size = maximumSize;
                Host.CalcSize();
            }
        }

        /// <summary>
        /// Font
        /// </summary>
        public Font Font
        {
            get { return (Host.ListView as Control).Font; }
            set { (Host.ListView as Control).Font = value; }
        }

        /// <summary>
        /// Left padding of text
        /// </summary>
        [DefaultValue(18)]
        [Description("Left padding of text")]
        public int LeftPadding
        {
            get
            {
                if (Host.ListView is DC_AutocompleteListView)
                {
                    return (Host.ListView as DC_AutocompleteListView).LeftPadding;
                }

                else
                {
                    return 0;
                }
            }
            set
            {
                if (Host.ListView is DC_AutocompleteListView)
                {
                    (Host.ListView as DC_AutocompleteListView).LeftPadding = value;
                }
            }
        }

        /// <summary>
        /// Colors of foreground and background
        /// </summary>
        [Browsable(true)]
        [Description("Colors of foreground and background.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DC_Colors Colors
        {
            get { return Host.ListView.Colors; }
            set { Host.ListView.Colors = value; }
        }

        /// <summary>
        /// AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or by Ctrl-Space.
        /// </summary>
        [DefaultValue(true)]
        [Description("AutocompleteMenu will popup automatically (when user writes text). Otherwise it will popup only programmatically or by Ctrl-Space.")]
        public bool AutoPopup { get; set; }

        /// <summary>
        /// AutocompleteMenu will capture focus when opening.
        /// </summary>
        [DefaultValue(false)]
        [Description("AutocompleteMenu will capture focus when opening.")]
        public bool CaptureFocus { get; set; }

        /// <summary>
        /// Indicates whether the component should draw right-to-left for RTL languages.
        /// </summary>
        [DefaultValue(typeof(RightToLeft), "No")]
        [Description("Indicates whether the component should draw right-to-left for RTL languages.")]
        public RightToLeft RightToLeft
        {
            get { return Host.RightToLeft; }
            set { Host.RightToLeft = value; }
        }

        /// <summary>
        /// Fragment
        /// </summary>
        [Browsable(false)]
        public Range Fragment { get; internal set; }

        /// <summary>
        /// Regex pattern for serach fragment around caret
        /// </summary>
        [Description("Regex pattern for serach fragment around caret")]
        [DefaultValue(@"[\S]")]
        public string SearchPattern { get; set; }

        /// <summary>
        /// Minimum fragment length for popup
        /// </summary>
        [Description("Minimum fragment length for popup")]
        [DefaultValue(2)]
        public int MinFragmentLength { get; set; }

        /// <summary>
        /// Allows TAB for select menu item
        /// </summary>
        [Description("Allows TAB for select menu item")]
        [DefaultValue(true)]
        public bool AllowsTabKey { get; set; }

        /// <summary>
        /// Interval of menu appear (ms)
        /// </summary>
        [Description("Interval of menu appear (ms)")]
        [DefaultValue(500)]
        public int AppearInterval { get; set; }

        [DefaultValue(null)]
        public string[] Items
        {
            get
            {
                if (sourceItems == null)
                {
                    return null;
                }
                List<string> list = new List<string>();
                {
                    foreach (DC_AutocompleteItem item in sourceItems)
                    {
                        list.Add(item.ToString());
                    }
                }
                return list.ToArray();
            }
            set { SetAutocompleteItems(value); }
        }

        /// <summary>
        /// The control for menu displaying.
        /// Set to null for restore default ListView (AutocompleteListView).
        /// </summary>
        [Browsable(false)]
        public IDC_AutocompleteListView ListView
        {
            get { return Host.ListView; }
            set
            {
                if (ListView != null)
                {
                    var ctrl = value as Control;
                    ctrl.RightToLeft = RightToLeft;
                    ctrl.Font = Font;
                    ctrl.MaximumSize = MaximumSize;
                }
                Host.ListView = value;
                Host.ListView.ItemSelected += new EventHandler(ListView_ItemSelected);
                Host.ListView.ItemHovered += new EventHandler<HoveredEventArgs>(ListView_ItemHovered);
            }
        }

        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Updates size of the menu
        /// </summary>
        public void Update()
        {
            Host.CalcSize();
        }

        /// <summary>
        /// Returns rectangle of item
        /// </summary>
        public Rectangle GetItemRectangle(int itemIndex)
        {
            return Host.ListView.GetItemRectangle(itemIndex);
        }

        #region IExtenderProvider Members

        bool IExtenderProvider.CanExtend(object extendee)
        {
            //find AutocompleteMenu with lowest hashcode
            if (Container != null)
            {
                foreach (object comp in Container.Components)
                {
                    if (comp is DC_AutocompleteMenu)
                    {
                        if (comp.GetHashCode() < GetHashCode())
                        {
                            return false;
                        }
                    }
                }
            }

            //we are main autocomplete menu on form ...
            //check extendee as TextBox
            if (!(extendee is Control))
            {
                return false;
            }

            var temp = TextBoxWrapper.Create(extendee as Control);
            return temp != null;
        }

        public void SetAutocompleteMenu(Control control, DC_AutocompleteMenu menu)
        {
            if (menu != null)
            {
                if (WrapperByControls.ContainsKey(control))
                {
                    return;
                }

                var wrapper = menu.CreateWrapper(control);
                if (wrapper == null)
                {
                    return;
                }

                if (control.IsHandleCreated)
                {
                    menu.SubscribeForm(wrapper);
                }
                else
                {
                    control.HandleCreated += (o, e) => menu.SubscribeForm(wrapper);
                }

                AutocompleteMenuByControls[control] = this;

                wrapper.LostFocus += menu.Control_LostFocus;
                wrapper.Scroll += menu.Control_Scroll;
                wrapper.KeyDown += menu.Control_KeyDown;
                wrapper.MouseDown += menu.Control_MouseDown;

            }
            else
            {
                AutocompleteMenuByControls.TryGetValue(control, out menu);
                AutocompleteMenuByControls.Remove(control);
                IDC_TextBoxWrapper wrapper = null;
                WrapperByControls.TryGetValue(control, out wrapper);
                WrapperByControls.Remove(control);
                if (wrapper != null && menu != null)
                {
                    wrapper.LostFocus -= menu.Control_LostFocus;
                    wrapper.Scroll -= menu.Control_Scroll;
                    wrapper.KeyDown -= menu.Control_KeyDown;
                    wrapper.MouseDown -= menu.Control_MouseDown;
                }
            }
        }

        #endregion

        /// <summary>
        /// User selects item
        /// </summary>
        [Description("Occurs when user selects item.")]
        public event EventHandler<SelectingEventArgs> Selecting;

        /// <summary>
        /// It fires after item was inserting
        /// </summary>
        [Description("Occurs after user selected item.")]
        public event EventHandler<SelectedEventArgs> Selected;

        /// <summary>
        /// It fires when item was hovered
        /// </summary>
        [Description("Occurs when user hovered item.")]
        public event EventHandler<HoveredEventArgs> Hovered;

        /// <summary>
        /// Occurs when popup menu is opening
        /// </summary>
        public event EventHandler<CancelEventArgs> Opening;

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if (TargetControlWrapper != null)
            {
                ShowAutocomplete(false);
            }
        }

        private Form myForm;

        void SubscribeForm(IDC_TextBoxWrapper wrapper)
        {
            if (wrapper == null)
            {
                return;
            }
            var form = wrapper.TargetControl.FindForm();
            if (form == null)
            {
                return;
            }
            if (myForm != null)
            {
                if (myForm == form)
                {
                    return;
                }
                UnsubscribeForm(wrapper);
            }

            myForm = form;

            form.LocationChanged += new EventHandler(Form_LocationChanged);
            form.ResizeBegin += new EventHandler(Form_LocationChanged);
            form.FormClosing += new FormClosingEventHandler(Form_FormClosing);
            form.LostFocus += new EventHandler(Form_LocationChanged);
        }

        void UnsubscribeForm(IDC_TextBoxWrapper wrapper)
        {
            if (wrapper == null)
            {
                return;
            }
            var form = wrapper.TargetControl.FindForm();
            if (form == null)
            {
                return;
            }

            form.LocationChanged -= new EventHandler(Form_LocationChanged);
            form.ResizeBegin -= new EventHandler(Form_LocationChanged);
            form.FormClosing -= new FormClosingEventHandler(Form_FormClosing);
            form.LostFocus -= new EventHandler(Form_LocationChanged);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Close();
        }

        private void Form_LocationChanged(object sender, EventArgs e)
        {
            Close();
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Close();
        }

        IDC_TextBoxWrapper FindWrapper(Control sender)
        {
            while (sender != null)
            {
                if (WrapperByControls.ContainsKey(sender))
                    return WrapperByControls[sender];

                sender = sender.Parent;
            }

            return null;
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            TargetControlWrapper = FindWrapper(sender as Control);

            if (Host.Visible)
            {
                if (ProcessKey((char)e.KeyCode, Control.ModifierKeys))
                {
                    e.SuppressKeyPress = true;
                }
                else
                {
                    if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
                    {
                        ResetTimer();
                    }
                    else
                    {
                        ResetTimer(1);
                    }
                }
                return;
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                    case Keys.F1:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.PageUp:
                    case Keys.PageDown:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.ControlKey:
                        {
                            timer.Stop();
                            return;
                        }
                }

                if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Space)
                {
                    ShowAutocomplete(true);
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            ResetTimer();
        }

        void ResetTimer()
        {
            ResetTimer(-1);
        }

        void ResetTimer(int interval)
        {
            timer.Interval = interval <= 0 ? AppearInterval : interval;
            timer.Stop();
            timer.Start();
        }

        private void Control_Scroll(object sender, ScrollEventArgs e)
        {
            Close();
        }

        private void Control_LostFocus(object sender, EventArgs e)
        {
            if (!Host.Focused)
            {
                Close();
            }
        }

        public DC_AutocompleteMenu GetAutocompleteMenu(Control control)
        {
            if (AutocompleteMenuByControls.ContainsKey(control))
                return AutocompleteMenuByControls[control];
            else
                return null;
        }

        bool forcedOpened = false;

        internal void ShowAutocomplete(bool forced)
        {
            if (forced)
            {
                forcedOpened = true;
            }

            if (TargetControlWrapper != null && TargetControlWrapper.Readonly)
            {
                Close();
                return;
            }

            if (!Enabled)
            {
                Close();
                return;
            }

            if (!forcedOpened && !AutoPopup)
            {
                Close();
                return;
            }

            //build list
            BuildAutocompleteList(forcedOpened);

            //show popup menu
            if (VisibleItems.Count > 0)
            {
                if (forced && VisibleItems.Count == 1 && Host.ListView.SelectedItemIndex == 0)
                {
                    //do autocomplete if menu contains only one line and user press CTRL-SPACE
                    OnSelecting();
                    Close();
                }
                else
                {
                    ShowMenu();
                }

            }
            else
            {
                Close();
            }
        }

        private void ShowMenu()
        {
            if (!Host.Visible)
            {
                var args = new CancelEventArgs();
                OnOpening(args);
                if (!args.Cancel)
                {
                    //calc screen point for popup menu
                    Point point = TargetControlWrapper.TargetControl.Location;
                    point.Offset(2, TargetControlWrapper.TargetControl.Height + 2);
                    point = TargetControlWrapper.GetPositionFromCharIndex(Fragment.Start);
                    point.Offset(2, TargetControlWrapper.TargetControl.Font.Height + 2);
                    //
                    Host.Show(TargetControlWrapper.TargetControl, point);
                    if (CaptureFocus)
                    {
                        (Host.ListView as Control).Focus();
                        //ProcessKey((char) Keys.Down, Keys.None);
                    }
                }
            }
            else
                (Host.ListView as Control).Invalidate();
        }

        private void BuildAutocompleteList(bool forced)
        {
            List<DC_AutocompleteItem> visibleItems = new List<DC_AutocompleteItem>();

            bool foundSelected = false;
            int selectedIndex = -1;
            //get fragment around caret
            Range fragment = GetFragment(SearchPattern);
            string text = fragment.Text;
            //
            if (sourceItems != null)
            {
                if (forced || (text.Length >= MinFragmentLength /* && tb.Selection.Start == tb.Selection.End*/))
                {
                    Fragment = fragment;
                    //build popup menu
                    foreach (DC_AutocompleteItem item in sourceItems)
                    {
                        item.Parent = this;
                        CompareResult res = item.Compare(text.ToLower());
                        if (res != CompareResult.Hidden)
                        {
                            visibleItems.Add(item);
                        }
                        if (res == CompareResult.VisibleAndSelected && !foundSelected)
                        {
                            foundSelected = true;
                            selectedIndex = visibleItems.Count - 1;
                        }
                    }

                }
            }
            //if (visibleItems.Count == 1)
            //{
            //    VisibleItems.Clear();
            //    foundSelected = false;
            //}
            //else
            //{
            //    VisibleItems = visibleItems;
            //}
            VisibleItems = visibleItems;

            SelectedItemIndex = foundSelected ? selectedIndex : 0;
            Host.ListView.HighlightedItemIndex = -1;
            Host.CalcSize();
        }

        internal void OnOpening(CancelEventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
        }

        private Range GetFragment(string searchPattern)
        {
            IDC_TextBoxWrapper tb = TargetControlWrapper;
            if (tb.SelectionLength > 0)
            {
                return new Range(tb);
            }

            string text = tb.Text;
            Regex regex = new Regex(searchPattern);
            Range result = new Range(tb);

            int startPos = tb.SelectionStart;
            //go forward
            int i = startPos;
            while (i >= 0 && i < text.Length)
            {
                if (!regex.IsMatch(text[i].ToString()))
                {
                    break;
                }
                i++;
            }
            result.End = i;

            //go backward
            i = startPos;
            while (i > 0 && (i - 1) < text.Length)
            {
                if (!regex.IsMatch(text[i - 1].ToString()))
                {
                    break;
                }
                i--;
            }
            result.Start = i;

            return result;
        }

        public void Close()
        {
            Host.Close();
            forcedOpened = false;
        }

        public void SetAutocompleteItems(IEnumerable<string> items)
        {
            var list = new List<DC_AutocompleteItem>();
            if (items == null)
            {
                sourceItems = null;
                return;
            }
            foreach (string item in items)
            {
                list.Add(new DC_AutocompleteItem(item));
            }
            SetAutocompleteItems(list);
        }

        public void SetAutocompleteItems(IEnumerable<DC_AutocompleteItem> items)
        {
            sourceItems = items;
        }

        public void AddItem(string item)
        {
            AddItem(new DC_AutocompleteItem(item));
        }

        public void AddItem(DC_AutocompleteItem item)
        {
            if (sourceItems == null)
            {
                sourceItems = new List<DC_AutocompleteItem>();
            }

            if (sourceItems is IList)
            {
                (sourceItems as IList).Add(item);
            }
            else
            {
                throw new Exception("Current autocomplete items does not support adding");
            }
        }

        /// <summary>
        /// Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(Control control, bool forced)
        {
            SetAutocompleteMenu(control, this);
            TargetControlWrapper = FindWrapper(control);
            ShowAutocomplete(forced);
        }

        internal virtual void OnSelecting()
        {
            if (SelectedItemIndex < 0 || SelectedItemIndex >= VisibleItems.Count)
            {
                return;
            }

            DC_AutocompleteItem item = VisibleItems[SelectedItemIndex];
            var args = new SelectingEventArgs
            {
                Item = item,
                SelectedIndex = SelectedItemIndex
            };

            OnSelecting(args);

            if (args.Cancel)
            {
                SelectedItemIndex = args.SelectedIndex;
                (Host.ListView as Control).Invalidate(true);
                return;
            }

            if (!args.Handled)
            {
                Range fragment = Fragment;
                ApplyAutocomplete(item, fragment);
            }

            Close();
            //
            var args2 = new SelectedEventArgs
            {
                Item = item,
                Control = TargetControlWrapper.TargetControl
            };
            item.OnSelected(args2);
            OnSelected(args2);
        }

        private void ApplyAutocomplete(DC_AutocompleteItem item, Range fragment)
        {
            string newText = item.GetTextForReplace();
            //replace text of fragment
            fragment.Text = newText;
            fragment.TargetWrapper.TargetControl.Focus();
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            if (Selecting != null)
            {
                Selecting(this, args);
            }
        }

        public void OnSelected(SelectedEventArgs args)
        {
            if (Selected != null)
            {
                Selected(this, args);
            }
        }

        public void SelectNext(int shift)
        {
            SelectedItemIndex = Math.Max(0, Math.Min(SelectedItemIndex + shift, VisibleItems.Count - 1));
            //
            (Host.ListView as Control).Invalidate();
        }

        public bool ProcessKey(char c, Keys keyModifiers)
        {
            int page = Host.Height / (Font.Height + 4);
            if (keyModifiers == Keys.None)
                switch ((Keys)c)
                {
                    case Keys.Down:
                        {
                            SelectNext(+1);
                            return true;
                        }

                    case Keys.PageDown:
                        {
                            SelectNext(+page);
                            return true;
                        }

                    case Keys.Up:
                        {
                            SelectNext(-1);
                            return true;
                        }

                    case Keys.PageUp:
                        {
                            SelectNext(-page);
                            return true;
                        }

                    case Keys.Enter:
                        {
                            OnSelecting();
                            return true;
                        }

                    case Keys.Tab:
                        {
                            if (!AllowsTabKey)
                            {
                                break;
                            }
                            OnSelecting();
                            return true;
                        }

                    case Keys.Left:
                    case Keys.Right:
                        {
                            Close();
                            return false;
                        }

                    case Keys.Escape:
                        {
                            Close();
                            return true;
                        }
                }

            return false;
        }

        public bool Visible
        {
            get { return Host != null && Host.Visible; }
        }
    }

    [ToolboxItem(false)]
    internal class DC_AutocompleteMenuHost : ToolStripDropDown
    {
        public ToolStripControlHost Host { get; set; }
        public readonly DC_AutocompleteMenu Menu;
        public DC_AutocompleteMenuHost(DC_AutocompleteMenu MenuPassRef)
        {
            AutoClose = false;
            AutoSize = false;
            Margin = Padding.Empty;
            Padding = Padding.Empty;

            Menu = MenuPassRef;
            ListView = new DC_AutocompleteListView();
        }

        private IDC_AutocompleteListView listView;
        public IDC_AutocompleteListView ListView
        {
            get { return listView; }
            set
            {

                if (listView != null)
                {
                    (listView as Control).LostFocus -= new EventHandler(ListView_LostFocus);
                }


                if (value == null)
                {
                    listView = new DC_AutocompleteListView();
                }
                else
                {
                    if (!(value is Control))
                    {
                        throw new Exception("ListView must be derived from Control class");
                    }
                    listView = value;
                }

                Host = new ToolStripControlHost(ListView as Control)
                {
                    Margin = new Padding(2, 2, 2, 2),
                    Padding = Padding.Empty,
                    AutoSize = false,
                    AutoToolTip = false
                };

                (ListView as Control).MaximumSize = Menu.MaximumSize;
                (ListView as Control).Size = Menu.MaximumSize;
                (ListView as Control).LostFocus += new EventHandler(ListView_LostFocus);

                CalcSize();
                base.Items.Clear();
                base.Items.Add(Host);
                (ListView as Control).Parent = this;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(listView.Colors.BackColor))
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        internal void CalcSize()
        {
            Host.Size = (ListView as Control).Size;
            Size = new Size((ListView as Control).Size.Width + 4, (ListView as Control).Size.Height + 4);
        }

        public override RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
                (ListView as Control).RightToLeft = value;
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!(ListView as Control).Focused)
            {
                Close();
            }
        }

        void ListView_LostFocus(object sender, EventArgs e)
        {
            if (!Focused)
            {
                Close();
            }
        }
    }

    public enum CompareResult
    {
        Hidden,
        Visible,
        VisibleAndSelected
    }

    public interface IDC_AutocompleteListView
    {
        /// <summary>
        /// Index of currently selected item
        /// </summary>
        int SelectedItemIndex { get; set; }

        /// <summary>
        /// Index of currently selected item
        /// </summary>
        int HighlightedItemIndex { get; set; }

        /// <summary>
        /// List of visible elements
        /// </summary>
        IList<DC_AutocompleteItem> VisibleItems { get; set; }

        /// <summary>
        /// Tooltip show duration (in ms)
        /// </summary>
        int ToolTipDuration { get; set; }

        /// <summary>
        /// Occurs when user selects an item
        /// </summary>
        event EventHandler ItemSelected;

        /// <summary>
        /// Occurs when hovered item index is changed
        /// </summary>
        event EventHandler<HoveredEventArgs> ItemHovered;

        /// <summary>
        /// Shows tooltip
        /// </summary>
        /// <param name="autocompleteItem"></param>
        /// <param name="control"></param>
        void ShowToolTip(DC_AutocompleteItem autocompleteItem, Control control = null);

        /// <summary>
        /// Returns rectangle of item
        /// </summary>
        Rectangle GetItemRectangle(int itemIndex);

        /// <summary>
        /// Colors
        /// </summary>
        DC_Colors Colors { get; set; }
    }

    [ToolboxItem(false)]
    public class DC_AutocompleteListView : UserControl, IDC_AutocompleteListView
    {
        private readonly ToolTip toolTip = new ToolTip();
        public int HighlightedItemIndex { get; set; }
        private int PreviousItemCount;

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
        public DC_Colors Colors { get; set; }

        public DC_AutocompleteListView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.Font = new Font(FontFamily.GenericSansSerif, 11F, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            ItemHeight = Font.Height + 2;
            VerticalScroll.SmallChange = ItemHeight;
            BackColor = Color.White;
            LeftPadding = 0;
            ToolTipDuration = 3000;
            Colors = new DC_Colors();
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
            set
            {
                itemHeight = value;
                VerticalScroll.SmallChange = value;
                PreviousItemCount = -1;
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


        private IList<DC_AutocompleteItem> visibleItems;
        public IList<DC_AutocompleteItem> VisibleItems
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

        private int selectedItemIndex = -1;
        public int SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set
            {
                DC_AutocompleteItem item = null;
                if (value >= 0 && value < VisibleItems.Count)
                {
                    item = VisibleItems[value];
                }
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
            ItemHovered?.Invoke(this, e);
        }

        private void AdjustScroll()
        {
            if (VisibleItems == null)
            {
                return;
            }

            if (PreviousItemCount == VisibleItems.Count)
            {
                return;
            }

            int needHeight = ItemHeight * VisibleItems.Count + 1;
            Height = Math.Min(needHeight, MaximumSize.Height);
            AutoScrollMinSize = new Size(0, needHeight);
            PreviousItemCount = VisibleItems.Count;
        }


        private void ScrollToSelected()
        {
            int y = SelectedItemIndex * ItemHeight - VerticalScroll.Value;
            if (y < 0)
            {
                VerticalScroll.Value = SelectedItemIndex * ItemHeight;
            }

            if (y > ClientSize.Height - ItemHeight)
            {
                VerticalScroll.Value = Math.Min(VerticalScroll.Maximum, SelectedItemIndex * ItemHeight - ClientSize.Height + ItemHeight);
            }
            //some magic for update scrolls
            AutoScrollMinSize -= new Size(1, 0);
            AutoScrollMinSize += new Size(1, 0);
        }

        public Rectangle GetItemRectangle(int itemIndex)
        {
            int y = itemIndex * ItemHeight - VerticalScroll.Value;
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
            int startI = VerticalScroll.Value / ItemHeight - 1;
            int finishI = (VerticalScroll.Value + ClientSize.Height) / ItemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, VisibleItems.Count);
            for (int i = startI; i < finishI; i++)
            {
                int y = i * ItemHeight - VerticalScroll.Value;

                var textRect = new Rectangle(LeftPadding, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);
                if (rtl)
                {
                    textRect = new Rectangle(1, y, ClientSize.Width - 1 - LeftPadding, ItemHeight - 1);
                }

                if (i == SelectedItemIndex)
                {
                    Brush selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + ItemHeight), Colors.SelectedBackColor2, Colors.SelectedBackColor);
                    e.Graphics.FillRectangle(selectedBrush, textRect);
                    using (var pen = new Pen(Colors.SelectedBackColor2))
                    {
                        e.Graphics.DrawRectangle(pen, textRect);
                    }
                }

                if (i == HighlightedItemIndex)
                {
                    using (var pen = new Pen(Colors.HighlightingColor))
                    {
                        e.Graphics.DrawRectangle(pen, textRect);
                    }
                }


                StringFormat sf = new StringFormat();
                if (rtl)
                {
                    sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                }

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
            mouseEnterPoint = MousePosition;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseEnterPoint != MousePosition)
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
            {
                ItemSelected(this, EventArgs.Empty);
            }
        }

        private int PointToItemIndex(Point p)
        {
            return (p.Y + VerticalScroll.Value) / ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            DC_AutocompleteMenuHost host = (DC_AutocompleteMenuHost)Parent;
            if (host != null)
            {
                if (host.Menu.ProcessKey((char)keyData, Keys.None))
                {
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SelectItem(int itemIndex)
        {
            SelectedItemIndex = itemIndex;
            ScrollToSelected();
            Invalidate();
        }

        public void SetItems(List<DC_AutocompleteItem> ItemsPassRef)
        {
            VisibleItems = ItemsPassRef;
            SelectedItemIndex = -1;
            AdjustScroll();
            Invalidate();
        }

        public void ShowToolTip(DC_AutocompleteItem AutocompleteItemPassReff, Control control = null)
        {
            string title = AutocompleteItemPassReff.ToolTipTitle;
            string text = AutocompleteItemPassReff.ToolTipText;
            if (control == null)
            {
                control = this;
            }

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

    public class DC_AutocompleteItem
    {
        public object Tag;
        string toolTipTitle;
        string toolTipText;
        string menuText;
        protected readonly string[] AlternateSearch;

        /// <summary>
        /// Parent AutocompleteMenu
        /// </summary>
        public DC_AutocompleteMenu Parent { get; internal set; }

        /// <summary>
        /// Text for inserting into textbox
        /// </summary>
        public string Text { get; set; }

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

        public DC_AutocompleteItem()
        {
        }

        public DC_AutocompleteItem(string text) : this()
        {
            Text = text;
        }

        public DC_AutocompleteItem(string text, string[] alternateSearch) : this(text)
        {
            AlternateSearch = alternateSearch;
        }

        public DC_AutocompleteItem(string text, string menuText) : this(text)
        {
            this.menuText = menuText;
        }

        public DC_AutocompleteItem(string text, string menuText, string toolTipTitle, string toolTipText) : this(text, menuText)
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
            if (Text.StartsWith(fragmentText, StringComparison.OrdinalIgnoreCase))
            {
                if (Text.ToLower().Equals(fragmentText))
                {
                    return CompareResult.VisibleAndSelected;
                }
                else
                {
                    return CompareResult.Visible;
                }
            }

            if (AlternateSearch != null)
            {
                foreach (string StringTemp in AlternateSearch)
                {
                    if (StringTemp.StartsWith(fragmentText, StringComparison.OrdinalIgnoreCase))
                    {
                        if (StringTemp.ToLower().Equals(fragmentText))
                        {
                            return CompareResult.VisibleAndSelected;
                        }
                        else
                        {
                            return CompareResult.Visible;
                        }
                    }
                }
            }

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
            {
                e.Graphics.DrawString(ToString(), e.Font, brush, e.TextRect, e.StringFormat);
            }
        }
    }

    public class DC_MultiCollumnAutocompleteItem : DC_AutocompleteItem
    {
        protected readonly bool CompareBySubstring = true; //{ get; set; }
        public string[] MenuTextByCollumns { get; set; }

        protected readonly string lowercaseText;

        public int[] CollumnWidth { get; set; }

        public DC_MultiCollumnAutocompleteItem(string[] menuTextByCollumns, string insertingText, string[] alternateSearch = null) : base(insertingText, alternateSearch)
        {
            MenuTextByCollumns = menuTextByCollumns;
            lowercaseText = insertingText.ToLower();
        }

        public override CompareResult Compare(string fragmentText)
        {
            //if (lowercaseText.StartsWith(fragmentText, StringComparison.OrdinalIgnoreCase))
            //{
            //    if (lowercaseText.Equals(fragmentText))
            //    {
            //        return CompareResult.VisibleAndSelected;
            //    }
            //    else
            //    {
            //        return CompareResult.Visible;
            //    }
            //}

            //comic id, but exclude "pool:" or any startwith version
            if (Regex.IsMatch(fragmentText, @"pool:\d+"))
            {
                if (lowercaseText.Equals(fragmentText))
                {
                    return CompareResult.VisibleAndSelected;
                }

                if (lowercaseText.Contains(fragmentText))
                {
                    return CompareResult.Visible;
                }
            }

            if (AlternateSearch != null)
            {
                foreach (string StringTemp in AlternateSearch)
                {
                    if (StringTemp.Contains(fragmentText))
                    {
                        if (StringTemp.Equals(fragmentText))
                        {
                            return CompareResult.VisibleAndSelected;
                        }
                        else
                        {
                            return CompareResult.Visible;
                        }
                    }
                }
            }

            return CompareResult.Hidden;
        }

        public override void OnPaint(PaintItemEventArgs e)
        {
            if (CollumnWidth != null && CollumnWidth.Length != MenuTextByCollumns.Length)
            {
                throw new Exception("CollumnWidth.Length != MenuTextByCollumns.Length");
            }

            int[] collumnWidth = CollumnWidth;
            if (collumnWidth == null)
            {
                collumnWidth = new int[MenuTextByCollumns.Length];
                int step = (int)((float)e.TextRect.Width / MenuTextByCollumns.Length);
                for (int i = 0; i < MenuTextByCollumns.Length; i++)
                {
                    collumnWidth[i] = step;
                }
            }

            Pen pen = Pens.Silver;
            float x = e.TextRect.X;
            e.StringFormat.FormatFlags = e.StringFormat.FormatFlags | StringFormatFlags.NoWrap;

            using (var brush = new SolidBrush(e.IsSelected ? e.Colors.SelectedForeColor : e.Colors.ForeColor))
            {
                for (int i = 0; i < MenuTextByCollumns.Length; i++)
                {
                    int width = collumnWidth[i];
                    RectangleF rect = new RectangleF(x, e.TextRect.Top, width, e.TextRect.Height);
                    e.Graphics.DrawLine(pen, new PointF(x, e.TextRect.Top), new PointF(x, e.TextRect.Bottom));
                    e.Graphics.DrawString(MenuTextByCollumns[i], e.Font, brush, rect, e.StringFormat);
                    x += width;
                }
            }
        }
    }


    public class Range
    {
        public IDC_TextBoxWrapper TargetWrapper { get; private set; }
        public int Start { get; set; }
        public int End { get; set; }

        public Range(IDC_TextBoxWrapper targetWrapper)
        {
            TargetWrapper = targetWrapper;
        }

        public string Text
        {
            get
            {
                string text = TargetWrapper.Text;
                if (string.IsNullOrEmpty(text))
                {
                    return "";
                }
                if (Start >= text.Length)
                {
                    return "";
                }
                if (End > text.Length)
                {
                    return "";
                }
                return TargetWrapper.Text.Substring(Start, End - Start);
            }

            set
            {
                TargetWrapper.SelectionStart = Start;
                TargetWrapper.SelectionLength = End - Start;
                TargetWrapper.SelectedText = value;
            }
        }
    }

    public class DC_Colors
    {
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public Color SelectedForeColor { get; set; }
        public Color SelectedBackColor { get; set; }
        public Color SelectedBackColor2 { get; set; }
        public Color HighlightingColor { get; set; }

        public DC_Colors()
        {
            ForeColor = Color.Black;
            BackColor = Color.White;
            SelectedForeColor = Color.Black;
            SelectedBackColor = Color.RoyalBlue; //Bottom color
            SelectedBackColor2 = Color.LightSteelBlue; //top color
            HighlightingColor = Color.Orange;
        }
    }

    public class SelectingEventArgs : EventArgs
    {
        public DC_AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    public class SelectedEventArgs : EventArgs
    {
        public DC_AutocompleteItem Item { get; internal set; }
        public Control Control { get; set; }
    }

    public class HoveredEventArgs : EventArgs
    {
        public DC_AutocompleteItem Item { get; internal set; }
    }

    public class PaintItemEventArgs : PaintEventArgs
    {
        public RectangleF TextRect { get; internal set; }
        public StringFormat StringFormat { get; internal set; }
        public Font Font { get; internal set; }
        public bool IsSelected { get; internal set; }
        public bool IsHovered { get; internal set; }
        public DC_Colors Colors { get; internal set; }

        public PaintItemEventArgs(Graphics graphics, Rectangle clipRect) : base(graphics, clipRect)
        {
        }
    }

    public class WrapperNeededEventArgs : EventArgs
    {
        public Control TargetControl { get; private set; }
        public IDC_TextBoxWrapper Wrapper { get; set; }

        public WrapperNeededEventArgs(Control targetControl)
        {
            TargetControl = targetControl;
        }
    }

    public interface IDC_TextBoxWrapper
    {
        Control TargetControl { get; }
        string Text { get; }
        string SelectedText { get; set; }
        int SelectionLength { get; set; }
        int SelectionStart { get; set; }
        Point GetPositionFromCharIndex(int pos);
        bool Readonly { get; }
        event EventHandler LostFocus;
        event ScrollEventHandler Scroll;
        event KeyEventHandler KeyDown;
        event MouseEventHandler MouseDown;
    }

    public class TextBoxWrapper : IDC_TextBoxWrapper
    {
        private Control target;
        private PropertyInfo selectionStart;
        private PropertyInfo selectionLength;
        private PropertyInfo selectedText;
        private PropertyInfo readonlyProperty;
        private MethodInfo getPositionFromCharIndex;
        private event ScrollEventHandler RTBScroll;

        private TextBoxWrapper(Control targetControl)
        {
            target = targetControl;
            Init();
        }

        protected virtual void Init()
        {
            var t = target.GetType();
            selectedText = t.GetProperty("SelectedText");
            selectionLength = t.GetProperty("SelectionLength");
            selectionStart = t.GetProperty("SelectionStart");
            readonlyProperty = t.GetProperty("ReadOnly");
            getPositionFromCharIndex = t.GetMethod("GetPositionFromCharIndex") ?? t.GetMethod("PositionToPoint");

            if (target is RichTextBox)
            {
                (target as RichTextBox).VScroll += new EventHandler(TextBoxWrapper_VScroll);
            }
        }

        void TextBoxWrapper_VScroll(object sender, EventArgs e)
        {
            if (RTBScroll != null)
            {
                RTBScroll(sender, new ScrollEventArgs(ScrollEventType.EndScroll, 0, 1));
            }
        }

        public static TextBoxWrapper Create(Control targetControl)
        {
            TextBoxWrapper result = new TextBoxWrapper(targetControl);

            if (result.selectedText == null || result.selectionLength == null || result.selectionStart == null || result.getPositionFromCharIndex == null)
            {
                return null;
            }
            return result;
        }

        public virtual string Text
        {
            get { return target.Text; }
            set { target.Text = value; }
        }

        public virtual string SelectedText
        {
            get { return (string)selectedText.GetValue(target, null); }
            set { selectedText.SetValue(target, value, null); }
        }

        public virtual int SelectionLength
        {
            get { return (int)selectionLength.GetValue(target, null); }
            set { selectionLength.SetValue(target, value, null); }
        }

        public virtual int SelectionStart
        {
            get { return (int)selectionStart.GetValue(target, null); }
            set { selectionStart.SetValue(target, value, null); }
        }

        public virtual Point GetPositionFromCharIndex(int pos)
        {
            return (Point)getPositionFromCharIndex.Invoke(target, new object[] { pos });
        }


        public virtual Form FindForm()
        {
            return target.FindForm();
        }

        public virtual event EventHandler LostFocus
        {
            add { target.LostFocus += value; }
            remove { target.LostFocus -= value; }
        }

        public virtual event ScrollEventHandler Scroll
        {
            add
            {
                if (target is RichTextBox)
                {
                    RTBScroll += value;
                }
                else
                {
                    if (target is ScrollableControl)
                    {
                        (target as ScrollableControl).Scroll += value;
                    }
                }
            }
            remove
            {
                if (target is RichTextBox)
                {
                    RTBScroll -= value;
                }
                else
                {
                    if (target is ScrollableControl)
                    {
                        (target as ScrollableControl).Scroll -= value;
                    }
                }
            }
        }

        public virtual event KeyEventHandler KeyDown
        {
            add { target.KeyDown += value; }
            remove { target.KeyDown -= value; }
        }

        public virtual event MouseEventHandler MouseDown
        {
            add { target.MouseDown += value; }
            remove { target.MouseDown -= value; }
        }

        public virtual Control TargetControl
        {
            get { return target; }
        }

        public bool Readonly
        {
            get { return (bool)readonlyProperty.GetValue(target, null); }
        }
    }
}