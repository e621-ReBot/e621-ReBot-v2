using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace e621_ReBot_v2.CustomControls
{
    public partial class Form_IDForm : Form
    {
        public static Form_IDForm _FormReference;
        public Form_IDForm(in Form OwnerForm, Point PointPass)
        {
            InitializeComponent();
            _FormReference = this;
            SetURLWhitelist();
            Location = new Point(PointPass.X - 7, PointPass.Y - 2);
            Owner = OwnerForm;
            ID_TextBox.ContextMenu = new ContextMenu(); //Disable right click menu
        }

        private void SetURLWhitelist()
        {
            WhitelistedURLs.Add("https://e621.net/posts/");
            WhitelistedURLs.Add("https://e621.net/pools/");
        }

        private void ID_TextBox_Enter(object sender, EventArgs e)
        {
            ID_TextBox.Text = null;
        }

        private void ID_TextBox_Leave(object sender, EventArgs e)
        {
            ID_TextBox.Text = "Enter ID";
        }

        private void ID_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Don't allow anything that isn't a control or number
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
            // Don't allow 0 as first value
            if (ID_TextBox.Text.Length == 0 && e.KeyChar == '0')
            {
                e.Handled = true;
                return;
            }
        }

        private readonly List<string> WhitelistedURLs = new List<string>();
        private void ID_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.V:
                    {
                        if (ModifierKeys.HasFlag(Keys.Control))
                        {
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.StringFormat))
                            {
                                string ClipboardText = (string)Clipboard.GetDataObject().GetData(DataFormats.StringFormat);
                                if (WhitelistedURLs.Any(s => ClipboardText.Contains(s)))
                                {
                                    ClipboardText = ClipboardText.Replace("https://e621.net/", "");
                                    if (ClipboardText.Contains("?"))
                                    {
                                        ClipboardText = ClipboardText.Substring(0, ClipboardText.IndexOf("?"));
                                    }

                                    string ClipboardID = ClipboardText.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                                    if (int.TryParse(ClipboardID, out _))
                                    {
                                        ID_TextBox.Text = ClipboardID;
                                        ID_TextBox.SelectionStart = ID_TextBox.Text.Length;
                                    }
                                    return;
                                }

                                if (ClipboardText.Length < 8 && int.TryParse(ClipboardText, out _))
                                {
                                    ID_TextBox.Text = ClipboardText;
                                    ID_TextBox.SelectionStart = ID_TextBox.Text.Length;
                                    return;
                                }
                            }
                        }
                        break;
                    }

                case Keys.Enter:
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                        break;
                    }
            }
        }

        private void Form_e6Post_FormClosed(object sender, FormClosedEventArgs e)
        {
            Owner.Activate();
            Owner.Focus();
            _FormReference = null;
            Dispose();
        }

        // = = = = =

        public static string Show(in Form FormPass, Point StartingLocation, string Title, Color? BGColor = null)
        {
            Form_IDForm e6_IDFormTemp = new Form_IDForm(FormPass, StartingLocation)
            {
                Location = new Point(StartingLocation.X - 7, StartingLocation.Y - 2),
                Text = Title
            };
            if (BGColor != null)
            {
                e6_IDFormTemp.BackColor = (Color)BGColor;
            }
            e6_IDFormTemp.ShowDialog();

            string IDEntered = null;
            if (e6_IDFormTemp.DialogResult == DialogResult.OK)
            {
                IDEntered = string.IsNullOrEmpty(e6_IDFormTemp.ID_TextBox.Text) ? null : e6_IDFormTemp.ID_TextBox.Text;
            }

            e6_IDFormTemp.Dispose();

            return IDEntered;
        }
    }
}