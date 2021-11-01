using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_e6Pool : Form
    {

        public static Form_e6Pool _FormReference;

        public Form_e6Pool(Point PointPass, Form OwnerForm)
        {
            InitializeComponent();
            _FormReference = this;
            Location = new Point(PointPass.X - 7, PointPass.Y - 2);
            Owner = OwnerForm;
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

        private void ID_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control) && e.KeyCode == Keys.V)
            {
                e.Handled = true;
                if (Clipboard.GetDataObject().GetDataPresent(DataFormats.StringFormat))
                {
                    string ClipboardText = (string)Clipboard.GetDataObject().GetData(DataFormats.StringFormat);
                    if (ClipboardText.Contains("https://e621.net/pools/"))
                    {
                        ClipboardText = ClipboardText.Replace("https://e621.net/pools/", "");
                        if (ClipboardText.Contains("?"))
                        {
                            ClipboardText = ClipboardText.Substring(0, ClipboardText.IndexOf("?"));
                        }
                        if (int.TryParse(ClipboardText, out _))
                        {
                            ID_TextBox.Text = ClipboardText;
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
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                GetPool(ID_TextBox.Text);
                Close();
            }
        }

        private void GetPool(string PoolID)
        {
            if (Form_PoolWatcher._FormReference.TreeView_PoolWatcher.Nodes.ContainsKey(PoolID))
            {
                MessageBox.Show(string.Format("Pool with ID#{0} is already being watched.", PoolID), "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }

            string e6JSONResult = Module_e621Info.e621InfoDownload("https://e621.net/pools/" + PoolID + ".json");
            if (e6JSONResult == null)
            {
                MessageBox.Show($"Pool with ID#{PoolID} does not exist.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JObject PoolJSON = JObject.Parse(e6JSONResult);
            string PoolName = PoolJSON["name"].Value<string>().Replace("_", " ");

            JObject JObjectTemp = new JObject
                {
                    { "id", PoolJSON["id"].Value<int>() },
                    { "name", PoolJSON["name"].Value<string>() },
                    { "post_ids", PoolJSON["post_ids"].Value<JToken>() },
                    { "post_count",  PoolJSON["post_count"].Value<int>() }
                };
            TreeNode PoolNode = new TreeNode()
            {
                Text = string.Format("{0} | Posts: {1}", PoolName, PoolJSON["post_count"]),
                Name = PoolID,
                ToolTipText = "Pool ID#" + PoolID,
                Tag = JObjectTemp
            };
            Form_PoolWatcher._FormReference.TreeView_PoolWatcher.Nodes.Add(PoolNode);
            BackgroundWorker BGW = new BackgroundWorker();
            BGW.DoWork += Module_Downloader.GraBPoolInBG;
            BGW.RunWorkerCompleted += Module_Downloader.E6APIDL_BGW_Done;
            BGW.RunWorkerAsync(PoolID);
            Close();
        }

        private void Form_e6Pool_FormClosed(object sender, FormClosedEventArgs e)
        {
            Module_Downloader.timer_Download.Start();
            Owner.Activate();
            Owner.Focus();
            _FormReference = null;
            Dispose();
        }
    }
}
