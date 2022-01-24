using System;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_Notes : Form
    {
        public static Form_Notes _FormReference;
        public Form_Notes()
        {
            InitializeComponent();
            _FormReference = this;
            Owner = Form_Loader._FormReference;
        }

        private void Form_Notes_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Note))
            {
                Note_TextBox.Text = Properties.Settings.Default.Note;
                Note_TextBox.ReadOnly = true;
            }
            if (Delete_Note_btn.Tag.ToString().Equals("Save"))
            {
                Delete_Note_btn.Text = "Save Note";
                Note_TextBox.ReadOnly = false;
                toolTip_Display.SetToolTip(Delete_Note_btn, "Saves the note, note will appear next time you run the app");
            }
            else
            {
                toolTip_Display.SetToolTip(Delete_Note_btn, "Deletes the note, close this instead if you want it to appear next time");
            }
        }

        private void Form_Notes_Shown(object sender, EventArgs e)
        {
            if (Delete_Note_btn.Tag.ToString().Equals("Save") && Note_TextBox.Text.Length > 0)
            {
                Note_TextBox.SelectAll();
            }
        }

        private void Delete_Note_btn_Click(object sender, EventArgs e)
        {
            panel_Holder.Focus();
            if (Delete_Note_btn.Tag.ToString().Equals("Save"))
            {
                Properties.Settings.Default.Note = Note_TextBox.Text;
                Form_Loader._FormReference.bU_NoteAdd.Text = "Edit Note";
                Form_Loader._FormReference.toolTip_Display.SetToolTip(Form_Loader._FormReference.bU_NoteAdd, "Edit existing note.");
                Form_Loader._FormReference.bU_NoteRemove.Enabled = true;
            }
            else
            {
                Properties.Settings.Default.Note = "";
                Form_Loader._FormReference.bU_NoteAdd.Text = "Add Note";
                Form_Loader._FormReference.toolTip_Display.SetToolTip(Form_Loader._FormReference.bU_NoteAdd, "Leave a note for yourself that will appear when application starts.");
                Form_Loader._FormReference.bU_NoteRemove.Enabled = false;
            }
            Properties.Settings.Default.Save();
            Close();
        }

        private void Note_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    {
                        Close();
                        break;
                    }
            }
        }

        private void Form_Notes_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Form_Menu._FormReference != null)
            {
                Form_Menu._FormReference.Enabled = true;
            }
            Form_Loader._FormReference.Activate();
            Form_Loader._FormReference.Focus();
            Dispose();
            _FormReference = null;
        }
    }
}