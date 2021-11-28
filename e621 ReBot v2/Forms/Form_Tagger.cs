using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{

    public partial class Form_Tagger : Form
    {
        public static Form_Tagger _FormReference;
        public DataRow Tagger_RowHolder;
        private Dictionary<string, CheckBox> TaggerCBList = new Dictionary<string, CheckBox>();

        public Form_Tagger()
        {
            InitializeComponent();
            _FormReference = this;

            //fix for buttons showing Form's ContextMenu
            TB_Done.ContextMenu = new ContextMenu();
            TB_Description.ContextMenu = new ContextMenu();
            TB_ArtistAlias.ContextMenu = new ContextMenu();
            TB_ParentOffset.ContextMenu = new ContextMenu();
            TB_CommandLine.ContextMenu = new ContextMenu();
        }

        public static void OpenTagger(Form FormOwnerPass, DataRow DataRowPass, Point TaggerLocation)
        {
            if (_FormReference == null)
            {
                new Form_Tagger();
            }
            _FormReference.Tagger_RowHolder = DataRowPass;
            _FormReference.Owner = FormOwnerPass;
            if (Equals(TaggerLocation, new Point(0, 0)))
            {
                _FormReference.CenterToScreen();
            }
            else
            {
                _FormReference.Location = TaggerLocation;
            }
            _FormReference.Show();
            _FormReference.BringToFront();
        }

        private void Form_Tagger_Load(object sender, EventArgs e)
        {
            SuspendLayout();
            textBox_Tags.Text = Tagger_RowHolder["Upload_Tags"] == DBNull.Value ? null : (string)Tagger_RowHolder["Upload_Tags"];

            if (Properties.Settings.Default.Tagger_Save != null)
            {
                foreach (string Group in Properties.Settings.Default.Tagger_Save)
                {
                    string[] GroupString = Group.Split(new string[] { "✄" }, StringSplitOptions.RemoveEmptyEntries);
                    GroupBox TempGroupBox = new GroupBox()
                    {
                        Text = GroupString[0],
                        Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0))),
                        Size = new Size(flowLayoutPanel_Holder.Width, 24),
                        MinimumSize = new Size(flowLayoutPanel_Holder.Width, 0),
                        MaximumSize = new Size(flowLayoutPanel_Holder.Width, 512),
                        Margin = new Padding(0, 0, 0, 0),
                        Padding = new Padding(2, 2, 2, 2),
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        ContextMenuStrip = contextMenuStrip_EditCat
                    };
                    flowLayoutPanel_Holder.Controls.Add(TempGroupBox);
                    FlowLayoutPanel FLP = new FlowLayoutPanel()
                    {
                        MinimumSize = new Size(flowLayoutPanel_Holder.Width - TempGroupBox.Padding.Right * 2, 0),
                        MaximumSize = new Size(flowLayoutPanel_Holder.Width - TempGroupBox.Padding.Right * 2, 512),
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        Dock = DockStyle.Fill
                    };
                    TempGroupBox.Controls.Add(FLP);
                    for (var x = 1; x <= GroupString.Length - 1; x++)
                    {
                        CheckBox ChkBox = new CheckBox()
                        {
                            Margin = new Padding(2, 2, 2, 2),
                            AutoSize = true,
                            Text = GroupString[x],
                            Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0))),
                            ContextMenuStrip = contextMenuStrip_EditItem,
                            TabStop = false
                        };
                        ChkBox.CheckedChanged += CheckboxClicked;
                        FLP.Controls.Add(ChkBox);
                        TaggerCBList.Add(GroupString[x], ChkBox);
                    }
                }
            }

            int RowIndex = Module_TableHolder.Database_Table.Rows.IndexOf(Tagger_RowHolder);
            if (Module_TableHolder.Database_Table.Rows.Count == 1)
            {
                TB_ParentOffset.Enabled = false;
            }
            else
            {
                if (RowIndex == 0)
                {
                    TB_ParentOffset.Enabled = false;
                    for (int y = 1; y <= Module_TableHolder.Database_Table.Rows.Count - 1; y++)
                    {
                        if (Module_TableHolder.Database_Table.Rows[y]["Uploaded_As"] != DBNull.Value)
                        {
                            TB_ParentOffset.Enabled = true;
                            break;
                        }
                    }
                }
                else
                {
                    TB_ParentOffset.Enabled = true;
                }
            }

            if (Tagger_RowHolder["Upload_ParentOffset"] != DBNull.Value)
            {
                TB_ParentOffset.Tag = "Set";
            }

            string ArtistA = Module_DB.DB_AA_CheckRecord(Tagger_RowHolder["Artist"].ToString(), Tagger_RowHolder["Grab_URL"].ToString());
            textBox_Tags.AppendText(" " + ArtistA);
            TB_ArtistAlias.Tag = ArtistA;

            List<string> SortTags = new List<string>();
            SortTags.AddRange(textBox_Tags.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            SortTags = SortTags.Distinct().ToList();
            for (int i = SortTags.Count - 1; i >= 0; i--)
            {
                if (TaggerCBList.ContainsKey(SortTags[i]))
                {
                    TaggerCBList[SortTags[i]].Checked = true;
                    //CheckboxTagCount += 1;
                    //SortTags.RemoveAt(i);
                }
            }
            textBox_Tags.Clear();
            textBox_Tags.Text = string.Join(" ", SortTags) + " ";

            if (Properties.Settings.Default.AutocompleteTags)
            {
                Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(textBox_Tags, Form_Loader._FormReference.AutoTags);
                cGroupBoxColored_AutocompleteSelector.Enabled = true;
            }

            TB_ArtistAlias.ForeColor = ArtistA == null ? SystemColors.ControlText : Color.RoyalBlue;
            TB_CommandLine.ForeColor = Properties.Settings.Default.CommandLineCommands == null ? SystemColors.ControlText : Color.RoyalBlue;

            TagCounter();
        }

        private void Form_Tagger_Shown(object sender, EventArgs e)
        {
            textBox_Tags.Focus();
            textBox_Tags.SelectionStart = textBox_Tags.TextLength;
            textBox_Title.Text = Tagger_RowHolder["Grab_Title"].ToString();
            ResumeLayout();
        }

        private void FlowLayoutPanel_Holder_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (flowLayoutPanel_Holder.Controls.Count == 0)
            {
                Height = MinimumSize.Height;
            }
        }

        private void FlowLayoutPanel_Holder_SizeChanged(object sender, EventArgs e)
        {
            Height = flowLayoutPanel_Holder.Height + MinimumSize.Height + 2;
        }

        private void Form_Tagger_Move(object sender, EventArgs e)
        {
            if (DescriptionForm != null && DescriptionForm.Visible)
            {
                DescriptionForm.Location = new Point(Location.X - DescriptionForm.Width, Location.Y);
            }
            cGroupBoxColored_AutocompleteSelector.Invalidate();
        }

        private void TextBox_Title_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        e.SuppressKeyPress = true;
                        textBox_Tags.Focus();
                        break;
                    }

                case Keys.Escape:
                    {
                        Close();
                        break;
                    }
            }
        }

        private void AddTags(bool SkipSave = false)
        {
            textBox_Tags.Text = textBox_Tags.Text.Trim();
            if (!SkipSave)
            {
                string Line4Save;
                foreach (GroupBox GroupboxHolder in flowLayoutPanel_Holder.Controls)
                {
                    Line4Save = GroupboxHolder.Text;
                    foreach (CheckBox Checkbox in GroupboxHolder.Controls[0].Controls.OfType<CheckBox>())
                    {
                        Line4Save += "✄" + Checkbox.Text;
                        if (Checkbox.Checked)
                        {
                            textBox_Tags.AppendText(" " + Checkbox.Text);
                        }
                    }
                }
            }

            List<string> SortTags = new List<string>();
            SortTags.AddRange(textBox_Tags.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            SortTags.Sort();
            SortTags = SortTags.Distinct().ToList();
            textBox_Tags.Text = string.Join(" ", SortTags).ToLower();

            if (Form_Preview._FormReference != null && Form_Preview._FormReference.Visible)
            {
                Form_Preview._FormReference.Label_Tags.Text = textBox_Tags.Text.Trim();
            }
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref Tagger_RowHolder);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp.cLabel_TagWarning.Visible = SortTags.Count < 6;
            }
            TagsAdded = true;
        }

        private void TextBox_Tags_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        if (textBox_Tags.Text.Substring(textBox_Tags.SelectionStart - 1, 1).Equals(" "))
                        {
                            e.SuppressKeyPress = true;
                        }
                        break;
                    }
                case Keys.Enter:
                    {
                        if (!Form_Loader._FormReference.AutoTags.Visible)
                        {
                            AddTags();
                            Close();
                        }
                        e.SuppressKeyPress = true;
                        break;
                    }

                case Keys.Escape:
                    {
                        if (!Form_Loader._FormReference.AutoTags.Visible)
                        {
                            Close();
                        }
                        e.SuppressKeyPress = true;
                        break;
                    }

                case Keys.Tab:
                    {
                        e.SuppressKeyPress = true;
                        break;
                    }

                case Keys.V:
                    {
                        if (ModifierKeys.HasFlag(Keys.Control))
                        {
                            e.SuppressKeyPress = true;
                            e.Handled = true;
                            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.StringFormat))
                            {
                                List<string> PasteTags = ((string)Clipboard.GetDataObject().GetData(DataFormats.StringFormat)).ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

                                List<string> SortTags = new List<string>();
                                SortTags.AddRange(textBox_Tags.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                                SortTags = SortTags.Distinct().ToList();

                                for (int i = PasteTags.Count - 1; i >=0; i--)
                                {
                                    if (SortTags.Contains(PasteTags[i]))
                                    {
                                        PasteTags.RemoveAt(i);
                                    }
                                }

                                SortTags.AddRange(PasteTags);
                                textBox_Tags.Text = string.Join(" ", SortTags) + " ";
                                textBox_Tags.SelectionStart = textBox_Tags.Text.Length;
                                TagCounter();
                            }
                        }
                        break;
                    }
            }
        }

        private void TextBox_Tags_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //otherwise not detected by KeyDown
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    {
                        e.IsInputKey = true;
                        break;
                    }
            }
        }

        private void TextBox_Tags_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Back:
                    {
                        TagCounter();
                        break;
                    }

                default:
                    {
                        if (timer_TagCount.Enabled)
                        {
                            timer_TagCount.Stop();
                        }
                        timer_TagCount.Start();
                        break;
                    }
            }
        }

        private void TB_Done_Click(object sender, EventArgs e)
        {
            AddTags();
            if (Form_Preview._FormReference != null)
            {
                Form_Preview._FormReference.TaggerLocator = Location;
            }
            Close();
        }




        private Form DescriptionForm;
        private void TB_Description_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                string Description;
                if (Tagger_RowHolder["Grab_TextBody"] == DBNull.Value)
                {
                    Description = Tagger_RowHolder["Grab_Title"].ToString();
                }
                else
                {
                    Description = $"[section{(Properties.Settings.Default.ExpandedDescription ? ",expanded" : null)}={(string)Tagger_RowHolder["Grab_Title"]}]\n{(string)Tagger_RowHolder["Grab_TextBody"]}\n[/section]";
                }
                Clipboard.SetText(Description);
                MessageBox.Show("Descripton has been copied to clipboard.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (TB_Description.Text.StartsWith("<", StringComparison.OrdinalIgnoreCase))
                {
                    TB_Description.Text = TB_Description.Text.Replace("<", ">");
                    if (DescriptionForm == null) //Setting it just once is enough
                    {
                        DescriptionForm = new Form()
                        {
                            Owner = this,
                            FormBorderStyle = FormBorderStyle.None,
                            Size = new Size(320, 320),
                            StartPosition = FormStartPosition.Manual,
                            Location = new Point(Location.X - 320, Location.Y),
                            ControlBox = false,
                            ShowIcon = false,
                            ShowInTaskbar = false
                        };
                        TextBox DescriptionTextbox = new TextBox()
                        {
                            Parent = DescriptionForm,
                            Dock = DockStyle.Fill,
                            ScrollBars = ScrollBars.Vertical,
                            Multiline = true,
                            TabStop = false,
                            Text = Tagger_RowHolder["Grab_TextBody"] != DBNull.Value ? (string)Tagger_RowHolder["Grab_TextBody"] : null
                        };
                        DescriptionForm.Controls.Add(DescriptionTextbox);
                    }
                    DescriptionForm.Show();
                }
                else
                {
                    TB_Description.Text = TB_Description.Text.Replace(">", "<");
                    Tagger_RowHolder["Grab_TextBody"] = DescriptionForm.Controls[0].Text;
                    DescriptionForm.Hide();
                }
            }
            textBox_Tags.Focus();
        }

        private void TB_ArtistAlias_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (TB_ArtistAlias.ForeColor == Color.RoyalBlue)
                {
                    Module_DB.DB_AA_DeleteRecord((string)Tagger_RowHolder["Artist"], (string)Tagger_RowHolder["Grab_URL"]);
                    textBox_Tags.Text = textBox_Tags.Text.Replace(TB_ArtistAlias.Tag.ToString(), "");
                    AddTags(true);
                    TB_ArtistAlias.ForeColor = SystemColors.ControlText;
                    TB_ArtistAlias.Tag = null;
                    MessageBox.Show(string.Format("Alias removed from artist {0}", (string)Tagger_RowHolder["Artist"]), "e621 ReBot");
                }
            }
            else
            {
                string InputText;
                string InputBoxText = string.Format("Chose a new alias for artist: {0}", (string)Tagger_RowHolder["Artist"]);
                if (TB_ArtistAlias.Tag != null)
                {
                    textBox_Tags.Text = textBox_Tags.Text.Replace(TB_ArtistAlias.Tag.ToString(), "");
                    InputBoxText += string.Format("{0}Current alias is: {1}", Environment.NewLine, TB_ArtistAlias.Tag.ToString());
                }
                string InputBoxAliastString = TB_ArtistAlias.Tag == null ? Tagger_RowHolder["Artist"].ToString().ToLower() : TB_ArtistAlias.Tag.ToString();
                do
                {
                    InputText = Custom_InputBox.Show(this, "Create Artist Alias", InputBoxText, TB_ArtistAlias.PointToScreen(Point.Empty), InputBoxAliastString).ToLower();
                    if (InputText.Equals("✄"))
                    {
                        return;
                    }
                    else
                    {
                        if (InputText.Equals(""))
                        {
                            MessageBox.Show("Artist Alias can not be blank.", "e621 ReBot");
                        }
                        else
                        {
                            InputText = InputText.ToLower();
                            if (TB_ArtistAlias.Tag == null)
                            {
                                Module_DB.DB_AA_CreateRecord((string)Tagger_RowHolder["Artist"], (string)Tagger_RowHolder["Grab_URL"], InputText);
                            }
                            else
                            {
                                Module_DB.DB_AA_UpdateRecord((string)Tagger_RowHolder["Artist"], (string)Tagger_RowHolder["Grab_URL"], InputText);
                            }

                            MessageBox.Show(string.Format("{0} is now aliased to {1}", (string)Tagger_RowHolder["Artist"], InputText), "e621 ReBot");
                            TB_ArtistAlias.ForeColor = Color.RoyalBlue;
                            TB_ArtistAlias.Tag = InputText;
                            textBox_Tags.Text = Regex.Replace(textBox_Tags.Text, "[ ]{2,}", " ", RegexOptions.None).Trim(); // replace multiple spaces with one https://codesnippets.fesslersoft.de/how-to-replace-multiple-spaces-with-a-single-space-in-c-or-vb-net/
                            textBox_Tags.AppendText(" " + InputText + " ");
                        }
                    }
                }
                while (InputText.Equals(""));
            }
        }

        private void TB_ParentOffset_Click(object sender, EventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (Tagger_RowHolder["Upload_ParentOffset"] != DBNull.Value)
                {
                    Tagger_RowHolder["Upload_ParentOffset"] = DBNull.Value;
                }
                TB_ParentOffset.Tag = null;
            }
            else
            {
                Form_ParentOffset Form_ParentOffsetTemp = new Form_ParentOffset
                {
                    Location = new Point(Location.X + Width - 14, Location.Y)
                };
                Form_ParentOffsetTemp.ShowDialog(this);
                Form_ParentOffsetTemp.Dispose();
                textBox_Tags.Focus();
            }
            TagCounter();
        }

        private void TB_CommandLine_Click(object sender, EventArgs e)
        {
            Point CLButtonLocation = TB_CommandLine.PointToScreen(Point.Empty);
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                if (Properties.Settings.Default.CommandLineCommands == null)
                {
                    MessageBox.Show("There are no commands created yet.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Form_CommandView Form_CommandViewTemp = new Form_CommandView
                    {
                        Location = new Point(CLButtonLocation.X - 9, CLButtonLocation.Y)
                    };
                    Form_CommandViewTemp.ShowDialog(this);
                }
                return;
            }

            Form_CommandLine Form_CommandLineTemp = new Form_CommandLine();
            if (ModifierKeys.HasFlag(Keys.Control) || Properties.Settings.Default.CommandLineCommands == null)
            {
                Form_CommandLineTemp.Tag = "Add";
                Form_CommandLineTemp.Text = "Add command string";
            }
            else
            {
                Form_CommandLineTemp.Tag = "Execute";
            }
            Form_CommandLineTemp.Location = new Point(CLButtonLocation.X - 9, CLButtonLocation.Y);
            Form_CommandLineTemp.ShowDialog(this);
            Form_CommandLineTemp.Dispose(); // call here becase closing events caused blinking/sent to background bug
            textBox_Tags.Focus();
            GC.Collect();
        }

        private void CMS_AddCategory_Click(object sender, EventArgs e)
        {
            string inputtext = Custom_InputBox.Show(this, "Add Category", "Name new category", Cursor.Position, "").ToLower();
            if (!inputtext.Equals("✄") && !inputtext.Equals(""))
            {
                GroupBox Groupbox = new GroupBox
                {
                    Text = inputtext,
                    Name = inputtext,
                    Size = new Size(flowLayoutPanel_Holder.Width, 24),
                    MinimumSize = new Size(flowLayoutPanel_Holder.Width, 0),
                    MaximumSize = new Size(flowLayoutPanel_Holder.Width, 512),
                    Margin = new Padding(0, 0, 0, 0),
                    Padding = new Padding(2, 2, 2, 2),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ContextMenuStrip = contextMenuStrip_EditCat
                };
                flowLayoutPanel_Holder.Controls.Add(Groupbox);
                FlowLayoutPanel FLP = new FlowLayoutPanel
                {
                    MinimumSize = new Size(flowLayoutPanel_Holder.Width - Groupbox.Padding.Right * 2, 0),
                    MaximumSize = new Size(flowLayoutPanel_Holder.Width - Groupbox.Padding.Right * 2, 512),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Fill
                };
                Groupbox.Controls.Add(FLP);
                FastTagsChanged = true;
            }
        }

        private void CMS_AddTag_Click(object sender, EventArgs e)
        {
            string inputtext = Custom_InputBox.Show(this, "Name new item", "This is your new tag", Cursor.Position, "").ToLower();
            if (!inputtext.Equals("✄") && !inputtext.Equals(""))
            {
                Control WhichGroupboxIsIt = ((ContextMenuStrip)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl.Controls[0]; // First control becase there is FlowLayoutPanel inside Groupbox
                foreach (CheckBox ChkBoxCheck in WhichGroupboxIsIt.Controls)
                {
                    if (ChkBoxCheck.Text == inputtext)
                    {
                        MessageBox.Show("That item already exists.", "Add item error");
                        return;
                    }
                }
                CheckBox ChkBox = new CheckBox
                {
                    AutoSize = true,
                    Text = inputtext,
                    ContextMenuStrip = contextMenuStrip_EditItem
                };
                ChkBox.CheckedChanged += CheckboxClicked;
                WhichGroupboxIsIt.Controls.Add(ChkBox);
                FastTagsChanged = true;
            }
        }

        private void CMS_EditTag_Click(object sender, EventArgs e)
        {
            Control WhichItemIsIt = ((ContextMenuStrip)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
            string inputtext = Custom_InputBox.Show(this, "Rename item", "This is your tag", Cursor.Position, WhichItemIsIt.Text).ToLower();
            if (!inputtext.Equals("✄") && !inputtext.Equals("") && !inputtext.Equals(WhichItemIsIt.Text))
            {
                foreach (CheckBox ChkBoxCheck in WhichItemIsIt.Parent.Controls)
                {
                    if (ChkBoxCheck.Text == inputtext)
                    {
                        MessageBox.Show("That item already exists.", "Edit item error");
                        return;
                    }
                }
                WhichItemIsIt.Text = inputtext;
                FastTagsChanged = true;
            }
        }

        private void CMS_RemoveTag_Click(object sender, EventArgs e)
        {
            Control WhichItemIsIt = ((ContextMenuStrip)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
            WhichItemIsIt.Dispose();
            FastTagsChanged = true;
        }

        private void CMS_RemoveCategory_Click(object sender, EventArgs e)
        {
            Control WhichItemIsIt = ((ContextMenuStrip)((ToolStripMenuItem)sender).GetCurrentParent()).SourceControl;
            WhichItemIsIt.Dispose();
            FastTagsChanged = true;
        }

        private void TB_ACTags_CheckedChanged(object sender, EventArgs e)
        {
            textBox_Tags.Focus();
            Form_Loader._FormReference.AutoTags.MaximumSize = new Size(200, 200);
            Form_Loader._FormReference.AutoTags.SetAutocompleteItems(Form_Loader._FormReference.AutoTagsList_Tags);
        }

        private void TB_ACPools_CheckedChanged(object sender, EventArgs e)
        {
            textBox_Tags.Focus();
            Form_Loader._FormReference.AutoTags.MaximumSize = new Size(320, 200);
            Form_Loader._FormReference.AutoTags.SetAutocompleteItems(Form_Loader._FormReference.AutoTagsList_Pools);
        }



        private void Timer_TagCount_Tick(object sender, EventArgs e)
        {
            timer_TagCount.Stop();
            TagCounter();
        }

        private void TagCounter()
        {
            List<string> SortTags = new List<string>();
            SortTags.AddRange(textBox_Tags.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            SortTags = SortTags.Distinct().ToList();
            Text = $"Tagger - Tags: {SortTags.Count}";
            if (TB_ParentOffset.Tag == null)
            {
                TB_ParentOffset.ForeColor = SystemColors.ControlText;
            }
            else
            {
                TB_ParentOffset.ForeColor = Color.RoyalBlue;
                Text += "   [Parent offset is set]";
            }

            for (int i = SortTags.Count - 1; i >= 0; i--)
            {
                if (TaggerCBList.ContainsKey(SortTags[i]))
                {
                    TaggerCBList[SortTags[i]].Checked = true;
                    //CheckboxTagCount += 1;
                    //SortTags.RemoveAt(i);
                }
            }

            if (textBox_Tags.SelectionStart > 0)
            {
                string SelectedWord = null;
                int TextBoxCursorIndex = textBox_Tags.SelectionStart - 1;
                var test = textBox_Tags.Text.Substring(TextBoxCursorIndex, 1);
                if (textBox_Tags.Text.Substring(TextBoxCursorIndex, 1).Equals(" "))
                {
                    int WordStartIndex = textBox_Tags.Text.Substring(0, TextBoxCursorIndex).LastIndexOf(" ") + 1;
                    int WordEndIndex = textBox_Tags.Text.IndexOf(" ", WordStartIndex);
                    if (WordEndIndex == -1) WordEndIndex = textBox_Tags.Text.Length;
                    SelectedWord = textBox_Tags.Text.Substring(WordStartIndex, WordEndIndex - WordStartIndex);
                    SortTags.Clear();
                    SortTags.AddRange(textBox_Tags.Text.Remove(WordStartIndex, WordEndIndex - WordStartIndex).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                    if (SortTags.Contains(SelectedWord))
                    {
                        textBox_Tags.Text = textBox_Tags.Text.Remove(WordStartIndex, WordEndIndex - WordStartIndex);
                        textBox_Tags.SelectionStart = textBox_Tags.Text.Length;
                    }
                }
            }

            int CursorHolder = textBox_Tags.SelectionStart;
            textBox_Tags.Text = Regex.Replace(textBox_Tags.Text, "[ ]{2,}", " ", RegexOptions.None); // replace multiple spaces with one https://codesnippets.fesslersoft.de/how-to-replace-multiple-spaces-with-a-single-space-in-c-or-vb-net/
            textBox_Tags.Text = textBox_Tags.Text.TrimStart().ToLower();
            textBox_Tags.SelectionStart = CursorHolder;
        }

        private void CheckboxClicked(object sender, EventArgs e)
        {
            CheckBox checkBoxSender = (CheckBox)sender;

            if (checkBoxSender.CheckState == CheckState.Checked)
            {
                if (!textBox_Tags.Text.Contains(checkBoxSender.Text)) textBox_Tags.Text = $"{checkBoxSender.Text} {textBox_Tags.Text}";
            }
            else
            {
                textBox_Tags.Text = textBox_Tags.Text.Replace(checkBoxSender.Text, "");
            }
            textBox_Tags.Focus();
            textBox_Tags.SelectionStart = textBox_Tags.TextLength;
            TagCounter();
        }



        bool TagsAdded = false;
        private void Form_Tagger_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TagsAdded)
            {
                List<string> TagListOnClose = new List<string>(textBox_Tags.Text.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                string DNPArtist = TagListOnClose.Intersect(Form_Loader._FormReference.DNP_Tags).FirstOrDefault();
                if (DNPArtist != null && (MessageBox.Show("Artist: " + DNPArtist + " is on DNP list, are you sure you want to proceed?", "e621 ReBot", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No))
                {
                    textBox_Tags.AppendText(" ");
                    textBox_Tags.SelectionStart = textBox_Tags.Text.Length;
                    TagsAdded = false;
                    e.Cancel = true;
                    return;
                }
                if (!TagListOnClose.Intersect(Form_Loader._FormReference.Gender_Tags).Any() && (MessageBox.Show("You have not added any gender tags, are you sure you want to close?", "e621 ReBot", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No))
                {
                    textBox_Tags.AppendText(" ");
                    textBox_Tags.SelectionStart = textBox_Tags.Text.Length;
                    TagsAdded = false;
                    e.Cancel = true;
                    return;
                }
                Tagger_RowHolder["Upload_Tags"] = textBox_Tags.Text;
            }

            Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(textBox_Tags, null);
            //// Fix bug focus going to wrong form
            Owner.Activate();
            Owner.Focus();
        }

        private static bool FastTagsChanged = false;
        private void Form_Tagger_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (FastTagsChanged)
            {
                if (Properties.Settings.Default.Tagger_Save == null)
                {
                    Properties.Settings.Default.Tagger_Save = new StringCollection();
                }
                else
                {
                    Properties.Settings.Default.Tagger_Save.Clear();
                }

                string Line4Save;
                foreach (GroupBox GroupboxHolder in flowLayoutPanel_Holder.Controls)
                {
                    Line4Save = GroupboxHolder.Text;
                    foreach (CheckBox Checkbox in GroupboxHolder.Controls[0].Controls.OfType<CheckBox>())
                    {
                        Line4Save += "✄" + Checkbox.Text;
                    }
                    Properties.Settings.Default.Tagger_Save.Add(Line4Save);
                }
                Properties.Settings.Default.Save();
            }

            _FormReference = null;
            Form_Loader._FormReference.AutoTags.SetAutocompleteMenu(textBox_Tags, null);
            Form_Loader._FormReference.AutoTags.SetAutocompleteItems(Form_Loader._FormReference.AutoTagsList_Tags);
            Dispose();
            GC.Collect();
        }
    }
}