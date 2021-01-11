using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public partial class Custom_CheckGroupBox : Custom_GroupBoxColored
    {

        /// <summary>
        /// CheckGroupBox public constructor.
        /// </summary>
        public Custom_CheckGroupBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            InitializeComponent();

            ControlAdded += CheckGroupBox_ControlAdded;
            Paint += CheckGroupBox_Paint;

            m_bDisableChildrenIfUnchecked = true;
            m_checkBox.Parent = this;
            m_checkBox.Location = new Point(CHECKBOX_X_OFFSET, CHECKBOX_Y_OFFSET);
            Checked = true;
            ToolTip_Show.SetToolTip(m_checkBox, TooltipTextOnCheckbox);
        }

        private const int CHECKBOX_X_OFFSET = 16;
        private const int CHECKBOX_Y_OFFSET = -2;
        private CheckBox _m_checkBox;

        private CheckBox m_checkBox
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _m_checkBox;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_m_checkBox != null)
                {
                    _m_checkBox.CheckedChanged -= checkBox_CheckedChanged;
                    _m_checkBox.CheckStateChanged -= checkBox_CheckStateChanged;
                }

                _m_checkBox = value;
                if (_m_checkBox != null)
                {
                    _m_checkBox.CheckedChanged += checkBox_CheckedChanged;
                    _m_checkBox.CheckStateChanged += checkBox_CheckStateChanged;
                }
            }
        }

        internal ToolTip ToolTip_Show;
        private IContainer components;
        private bool m_bDisableChildrenIfUnchecked;

        public string TooltipTextOnCheckbox
        {
            get
            {
                return ToolTip_Show.GetToolTip(m_checkBox);
            }

            set
            {
                ToolTip_Show.SetToolTip(m_checkBox, value);
            }
        }

        /// <summary>
        /// The text associated with the control.
        /// </summary>
        public override string Text
        {
            get
            {
                if (Site is object && Site.DesignMode == true)
                {
                    // Design-time mode
                    return m_checkBox.Text;
                }
                else
                {
                    // Run-time
                    return "     ";
                } // Set the text of the GroupBox to a space, so the gap appears before the CheckBox.
            }

            set
            {
                base.Text = "     "; // / Set the text of the GroupBox to a space, so the gap appears before the CheckBox.
                m_checkBox.Text = value;
            }
        }

        /// <summary>
        /// Indicates whether the component is checked or not.
        /// </summary>
        [Description("Indicates whether the component is checked or not.")]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool Checked
        {
            get
            {
                return m_checkBox.Checked;
            }

            set
            {
                if (m_checkBox.Checked != value)
                {
                    m_checkBox.Checked = value;
                }
            }
        }

        /// <summary>
        /// Indicates the state of the component.
        /// </summary>
        [Description("Indicates the state of the component.")]
        [Category("Appearance")]
        [DefaultValue(CheckState.Checked)]
        public CheckState CheckState
        {
            get
            {
                return m_checkBox.CheckState;
            }

            set
            {
                if (m_checkBox.CheckState != value)
                {
                    m_checkBox.CheckState = value;
                }
            }
        }

        /// <summary>
        /// Determines if child controls of the GroupBox are disabled when the CheckBox is unchecked.
        /// </summary>
        [Description("Determines if child controls of the GroupBox are disabled when the CheckBox is unchecked.")] // <Category("Appearance")>
        [DefaultValue(true)]
        public bool DisableChildrenIfUnchecked
        {
            get
            {
                return m_bDisableChildrenIfUnchecked;
            }

            set
            {
                if (m_bDisableChildrenIfUnchecked != value)
                {
                    m_bDisableChildrenIfUnchecked = value;
                }
            }
        }

        /// <summary>
        /// Occurs whenever the Checked property of the CheckBox is changed.
        /// </summary>
        [Description("Occurs whenever the Checked property of the CheckBox is changed.")]
        public event EventHandler CheckedChanged;

        /// <summary>
        /// Occurs whenever the CheckState property of the CheckBox is changed.
        /// </summary>
        [Description("Occurs whenever the CheckState property of the CheckBox is changed.")]
        public event EventHandler CheckStateChanged;

        /// <summary>
        /// Raises the System.Windows.Forms.CheckBox.checkBox_CheckedChanged event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected virtual void OnCheckedChanged(EventArgs e)
        {
        }

        /// <summary>
        /// Raises the System.Windows.Forms.CheckBox.CheckStateChanged event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected virtual void OnCheckStateChanged(EventArgs e)
        {
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bDisableChildrenIfUnchecked == true)
            {
                bool bEnabled = m_checkBox.Checked;
                foreach (Control control in Controls)
                {
                    if (!ReferenceEquals(control, m_checkBox))
                    {
                        control.Enabled = bEnabled;
                    }
                }
            }

            CheckedChanged?.Invoke(sender, e);
        }

        private void checkBox_CheckStateChanged(object sender, EventArgs e)
        {
            CheckStateChanged?.Invoke(sender, e);
        }

        private void CheckGroupBox_ControlAdded(object sender, ControlEventArgs e)
        {
            if (m_bDisableChildrenIfUnchecked == true)
            {
                e.Control.Enabled = Checked;
            }
        }

        private void InitializeComponent()
        {
            components = new Container();
            _m_checkBox = new CheckBox();
            _m_checkBox.CheckedChanged += new EventHandler(checkBox_CheckedChanged);
            _m_checkBox.CheckStateChanged += new EventHandler(checkBox_CheckStateChanged);
            ToolTip_Show = new ToolTip(components);
            SuspendLayout();
            // 
            // m_checkBox
            // 
            _m_checkBox.AutoSize = true;
            _m_checkBox.Checked = true;
            _m_checkBox.CheckState = CheckState.Checked;
            _m_checkBox.Location = new Point(0, 0);
            _m_checkBox.Name = "_m_checkBox";
            _m_checkBox.Size = new Size(104, 16);
            _m_checkBox.TabIndex = 0;
            _m_checkBox.TabStop = false;
            _m_checkBox.Text = "checkBox";
            ToolTip_Show.SetToolTip(_m_checkBox, "Toggle this (On) to retry failed jobs");
            _m_checkBox.UseVisualStyleBackColor = true;
            // 
            // ToolTip_Show
            // 
            ToolTip_Show.AutomaticDelay = 100;
            ToolTip_Show.AutoPopDelay = 0;
            ToolTip_Show.InitialDelay = 100;
            ToolTip_Show.ReshowDelay = 20;
            // 
            // Custom_CheckGroupBox
            // 
            ResumeLayout(false);
        }

        private void CheckGroupBox_Paint(object sender, PaintEventArgs e)
        {
            m_checkBox.ForeColor = ForeColor;
        }
    }
}
