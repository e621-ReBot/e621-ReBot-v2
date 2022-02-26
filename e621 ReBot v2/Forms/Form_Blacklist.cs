using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_Blacklist : Form
    {
        public static Form_Blacklist _FormReference;
        public Form_Blacklist(Point LocationPass)
        {
            InitializeComponent();
            _FormReference = this;
            Location = new Point(LocationPass.X - 8, LocationPass.Y);
        }

        private void Form_Blacklist_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Blacklist == null)
            {
                Properties.Settings.Default.Blacklist = new StringCollection();
            }
            if (Properties.Settings.Default.Blacklist.Count > 0)
            {
                foreach (var BlacklistString in Properties.Settings.Default.Blacklist)
                {
                    textBox_Blacklist.AppendText(BlacklistString + Environment.NewLine);
                    textBox_Blacklist.Text = textBox_Blacklist.Text.Remove(textBox_Blacklist.Text.Length - 1);
                }
            }
            textBox_Blacklist.SelectionStart = textBox_Blacklist.Text.Length;
        }

        private void TextBox_Blacklist_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        Close();
                        break;
                    }

                case Keys.Escape:
                    {
                        Close();
                        break;
                    }
            }
        }

        private void Form_Blacklist_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Blacklist = new StringCollection();
            List<string> GetBlacklist = new List<string>();
            textBox_Blacklist.Text = textBox_Blacklist.Text.ToLower();
            foreach (var TBLine in textBox_Blacklist.Lines)
            {
                string TempLine = TBLine.Trim();
                if (TempLine.Length > 0)
                {
                    GetBlacklist.Add(TempLine);
                }
            }
            Form_Loader._FormReference.Blacklist = GetBlacklist;
            Properties.Settings.Default.Blacklist.AddRange(GetBlacklist.ToArray());
            Properties.Settings.Default.Save();

            Form_Loader._FormReference.Activate();
            Form_Loader._FormReference.Focus();

            _FormReference = null;
            Dispose();
        }
    }
}