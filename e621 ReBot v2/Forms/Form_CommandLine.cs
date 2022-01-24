using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_CommandLine : Form
    {
        public Form_CommandLine()
        {
            InitializeComponent();
        }

        private void Form_CommandLine_Load(object sender, EventArgs e)
        {
            CommandLine_LoadCommands();
        }

        private Dictionary<string, string> CommandList = new Dictionary<string, string>();
        public void CommandLine_LoadCommands()
        {
            CommandList.Clear();
            if (Properties.Settings.Default.CommandLineCommands != null)
            {
                foreach (string Command in Properties.Settings.Default.CommandLineCommands)
                {
                    string[] SplitString = Command.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    CommandList.Add(SplitString[0], SplitString[1]);
                }

                if (Tag.ToString().Equals("Execute"))
                {
                    AutoCompleteStringCollection TempSource = new AutoCompleteStringCollection();
                    TempSource.AddRange(CommandList.Keys.ToArray());
                    CommandLine_Textbox.AutoCompleteCustomSource = TempSource;
                }
            }
        }

        private string NewCommand;
        private void CommandLine_Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                //case Keys.Return:
                    {
                        e.SuppressKeyPress = true;
                        if (string.IsNullOrEmpty(CommandLine_Textbox.Text))
                        {
                            // Invoke to delay MessageBox so SuppressKeyPress works
                            BeginInvoke(new Action(() => MessageBox.Show("Command string can not be blank.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                        }
                        else
                        {
                            NewCommand = CommandLine_Textbox.Text;
                            if (Tag.ToString().Equals("Add"))
                            {
                                if (CommandList.Keys.Contains(NewCommand))
                                {
                                    // Invoke to delay MessageBox so SuppressKeyPress works
                                    BeginInvoke(new Action(() => { MessageBox.Show("Command string already exists.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning); }));
                                    return;
                                }

                                Text = "Now enter tags";
                                Size = new Size(180, 140);
                                CommandLine_Textbox.Text = null;
                                CommandLine_Textbox.Visible = false;
                                TagLine_Textbox.Visible = true;
                                TagLine_Textbox.Focus();
                                Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(TagLine_Textbox, Form_Loader._FormReference.AutoTags);
                            }
                            else if (Tag.ToString().Equals("Edit"))
                            {
                                string TagItem = Properties.Settings.Default.CommandLineCommands[(int)CommandLine_Textbox.Tag];
                                var SplitString = TagItem.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                Properties.Settings.Default.CommandLineCommands[(int)CommandLine_Textbox.Tag] = NewCommand + "," + SplitString[1];
                                Properties.Settings.Default.Save();
                                Close();
                            }
                            else if (!CommandList.Keys.Contains(NewCommand))
                            {
                                // Invoke to delay MessageBox so SuppressKeyPress works
                                BeginInvoke(new Action(() => { MessageBox.Show("Unknown command string.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning); }));
                                return;
                            }
                            else
                            {
                                Form_Tagger._FormReference.textBox_Tags.AppendText(" " + CommandList[NewCommand]);
                                Close();
                            }
                        }
                        break;
                    }

                case Keys.Escape:
                    {
                        e.SuppressKeyPress = true;
                        Close();
                        break;
                    }

                case Keys.Space:
                case Keys.Oemcomma:
                case Keys.OemPeriod:
                    {
                        e.SuppressKeyPress = true;
                        break;
                    }
            }
        }

        private void TagLine_Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                //case Keys.Return:
                    {
                        e.SuppressKeyPress = true;
                        if (!Form_Loader._FormReference.AutoTags.Visible)
                        {
                            TagLine_Textbox.Text = TagLine_Textbox.Text.Trim();
                            if (string.IsNullOrEmpty(TagLine_Textbox.Text))
                            {
                                // Invoke to delay MessageBox so SuppressKeyPress works
                                BeginInvoke(new Action(() => { MessageBox.Show("Command tags can not be blank.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning); }));
                            }
                            else
                            {
                                if (Tag.ToString().Equals("Edit"))
                                {
                                    string TagItem = Properties.Settings.Default.CommandLineCommands[(int)TagLine_Textbox.Tag];
                                    var SplitString = TagItem.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                    Properties.Settings.Default.CommandLineCommands[(int)TagLine_Textbox.Tag] = SplitString[0] + "," + TagLine_Textbox.Text;
                                }
                                else
                                {
                                    CommandList.Add(NewCommand, TagLine_Textbox.Text);
                                    NewCommand += "," + TagLine_Textbox.Text;
                                    if (Properties.Settings.Default.CommandLineCommands == null)
                                    {
                                        Properties.Settings.Default.CommandLineCommands = new StringCollection();
                                    }

                                    Properties.Settings.Default.CommandLineCommands.Add(NewCommand);
                                    Form_Tagger._FormReference.TB_CommandLine.ForeColor = Color.RoyalBlue;
                                }
                                Properties.Settings.Default.Save();
                                Close();
                            }
                        }

                        break;
                    }

                case Keys.Escape:
                    {
                        e.SuppressKeyPress = true;
                        if (!Form_Loader._FormReference.AutoTags.Visible)
                        {
                            Close();
                        }
                        break;
                    }

                case Keys.Oemcomma:
                case Keys.OemPeriod:
                    {
                        e.SuppressKeyPress = true;
                        break;
                    }
            }
        }

        private void Form_CommandLine_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(TagLine_Textbox, null); // got to remove menu or error about control not existing happens due to closed form, since esc closes menu and form
        }
    }
}