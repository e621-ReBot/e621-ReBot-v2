
using System;
using System.Drawing;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_CommandView : Form
    {
        public Form_CommandView()
        {
            InitializeComponent();
        }

        private void Form_CommandView_Load(object sender, EventArgs e)
        {
            PopulateList();
        }

        private void PopulateList()
        {
            if (Properties.Settings.Default.CommandLineCommands != null)
            {
                Command_ListView.SuspendLayout();
                Command_ListView.BeginUpdate();
                Command_ListView.Items.Clear();
                foreach (string Command in Properties.Settings.Default.CommandLineCommands)
                {
                    string[] SplitString = Command.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var CommandRow = new ListViewItem() { Text = SplitString[0] };
                    CommandRow.SubItems.Add(SplitString[1]);
                    Command_ListView.Items.Add(CommandRow);
                }
                Command_ListView.EndUpdate();
                Command_ListView.ResumeLayout();
            }
            else
            {
                Form_Tagger._FormReference.TB_CommandLine.ForeColor = SystemColors.ControlText;
                Close();
            }
        }

        private void ContextMenuStrip_ListView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Command_ListView.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }

        }

        private void CommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem SelectedRow = Command_ListView.SelectedItems[0];

            Form_CommandLine Form_CommandLineTemp = new Form_CommandLine
            {
                Tag = "Edit"
            };
            Form_CommandLineTemp.CommandLine_Textbox.Text = SelectedRow.Text;
            Form_CommandLineTemp.CommandLine_Textbox.Tag = SelectedRow.Index;
            Form_CommandLineTemp.Location = new Point(Cursor.Position.X - 8 - Form_CommandLineTemp.Width / 2, Cursor.Position.Y - 8);
            Form_CommandLineTemp.ShowDialog(this);
            Form_CommandLineTemp.Dispose(); // call here becase closing events caused blincking/send to background bug
            PopulateList();
        }

        private void TagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem SelectedRow = Command_ListView.SelectedItems[0];

            Form_CommandLine Form_CommandLineTemp = new Form_CommandLine
            {
                Tag = "Edit",
                Text = "Enter new tags",
                Size = new Size(180, 140)
            };
            Form_CommandLineTemp.CommandLine_Textbox.Text = SelectedRow.Text;
            Form_CommandLineTemp.CommandLine_Textbox.Visible = false;
            Form_CommandLineTemp.TagLine_Textbox.Text = SelectedRow.SubItems[1].Text;
            Form_CommandLineTemp.TagLine_Textbox.Tag = SelectedRow.Index;
            Form_CommandLineTemp.TagLine_Textbox.Visible = true;
            Form_CommandLineTemp.TagLine_Textbox.Focus();
            Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(Form_CommandLineTemp.TagLine_Textbox, Form_Loader._FormReference.AutoTags);

            Form_CommandLineTemp.Location = new Point(Cursor.Position.X - 8 - Form_CommandLineTemp.Width / 2, Cursor.Position.Y - 8);
            Form_CommandLineTemp.ShowDialog(this);
            Form_CommandLineTemp.Dispose(); // call here becase closing events caused blincking/send to background bug
            PopulateList();
        }

        private void RemoveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.CommandLineCommands.RemoveAt(Command_ListView.SelectedIndices[0]);
            if (Properties.Settings.Default.CommandLineCommands.Count == 0)
            {
                Properties.Settings.Default.CommandLineCommands = null;
            }
            Properties.Settings.Default.Save();
            PopulateList();
        }
    }
}
