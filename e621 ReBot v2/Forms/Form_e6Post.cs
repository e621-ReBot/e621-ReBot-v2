using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_e6Post : Form
    {
        public Form_e6Post(Point PointPass, Form OwnerForm)
        {
            InitializeComponent();
            _FormReference = this;
            Location = new Point(PointPass.X - 7, PointPass.Y - 2);
            Owner = OwnerForm;
        }

        public static Form_e6Post _FormReference;

        public string PuzzlePostID;

        private void Form_e6Post_FormClosed(object sender, FormClosedEventArgs e)
        {
            _FormReference = null;
            Dispose();
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
                    if (ClipboardText.Contains("https://e621.net/posts/"))
                    {
                        ClipboardText = ClipboardText.Replace("https://e621.net/posts/", "");
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
                if (Tag == null)
                {
                    SuperiorSub(ID_TextBox.Text, Form_Preview._FormReference.Preview_RowHolder);
                }
                else
                {
                    if (Tag.Equals("Inferior"))
                    {
                        InferiorSub(ID_TextBox.Text, Form_Preview._FormReference.Preview_RowHolder);
                    }
                    else // (Tag.Equals("Puzzle"))
                    {
                        PuzzlePostID = ID_TextBox.Text;
                    }
                }
                Close();
            }
        }

        public static void SuperiorSub(string PostID, DataRow RowRefference)
        {
            string PostTest = Module_e621Info.e621InfoDownload(string.Format("https://e621.net/posts/{0}.json", PostID), true);
            if (PostTest == null || PostTest.Length < 10)
            {
                if (_FormReference != null)  _FormReference.ID_TextBox.Text = null;
                MessageBox.Show(string.Format("Post with ID#{0} does not exist.", PostID), "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JToken PostData = JObject.Parse(PostTest)["post"];
            RowRefference["Upload_Rating"] = PostData["rating"].Value<string>().ToUpper();
            List<string> SortTags = new List<string>();
            foreach (JProperty pTag in PostData["tags"].Children())
            {
                foreach (JToken cTag in pTag.First)
                {
                    SortTags.Add(cTag.Value<string>());
                }
            }
            SortTags.Sort();

            if (!PostData["pools"].ToString().Equals("[]"))
            {
                foreach (JToken pPool in PostData["pools"].Children())
                {
                    SortTags.Add("pool:" + pPool.Value<string>());
                }
            }

            string InferiorParentID = PostData["relationships"]["parent_id"].Value<string>();
            if (InferiorParentID != null)
            {
                SortTags.Add("parent:" + InferiorParentID);
                RowRefference["Inferior_ParentID"] = InferiorParentID;
            }
            RowRefference["Upload_Tags"] = string.Join(" ", SortTags);
            RowRefference["Inferior_ID"] = PostID;
            string InferiorDescription = PostData["description"].Value<string>();
            string CurrentDescriptionConstruct = string.Format("[section{0}={1}]\n{2}\n[/section]", Properties.Settings.Default.ExpandedDescription ? ",expanded" : "", (string)RowRefference["Grab_Title"], (string)RowRefference["Grab_TextBody"]);
            if (!InferiorDescription.Equals("") && InferiorDescription != CurrentDescriptionConstruct)
            {
                RowRefference["Inferior_Description"] = InferiorDescription;
            }

            if (PostData["sources"].Children().Count() > 0)
            {
                List<string> SourceList = new List<string>();
                foreach (JToken cChild in PostData["sources"])
                {
                    SourceList.Add(cChild.Value<string>());
                }
                RowRefference["Inferior_Sources"] = SourceList;
            }

            if (PostData["relationships"]["has_children"].Value<bool>())
            {
                List<string> ChildList = new List<string>();
                foreach (JToken cChild in PostData["relationships"]["children"])
                {
                    ChildList.Add(cChild.Value<string>());
                }
                RowRefference["Inferior_Children"] = ChildList;
            }

            if (PostData["has_notes"].Value<bool>())
            {
                // when they fix api this should no longer maker 2 requests to get notes
                PostTest = Module_e621Info.e621InfoDownload("https://e621.net/notes.json?search[post_id]=" + PostID, true);
                if (!PostTest.StartsWith("{")) // no notes then
                {
                    RowRefference["Inferior_HasNotes"] = true;
                    double NewNoteSizeRatio = Math.Max((int)RowRefference["Info_MediaWidth"], (int)RowRefference["Info_MediaHeight"]) / (double)Math.Max(PostData["file"]["width"].Value<int>(), PostData["file"]["height"].Value<int>());
                    RowRefference["Inferior_NotesSizeRatio"] = NewNoteSizeRatio;
                }
            }
            if (Properties.Settings.Default.RemoveBVAS)
            {
                RowRefference["Upload_Tags"] = ((string)RowRefference["Upload_Tags"]).Replace("better_version_at_source", "");
            }
            Form_Preview._FormReference.Label_Tags.Text = (string)RowRefference["Upload_Tags"];

            DataRow DataRowTemp = Form_Preview._FormReference.Preview_RowHolder;
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref DataRowTemp);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp._Rating = (string)RowRefference["Upload_Rating"];
                int TagCounter = ((string)RowRefference["Upload_Tags"]).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Count();
                e6_GridItemTemp.cLabel_TagWarning.Visible = TagCounter < 5;
                e6_GridItemTemp.cCheckBox_UPDL.Checked = true;
                e6_GridItemTemp.toolTip_Display.SetToolTip(e6_GridItemTemp.cLabel_isSuperior, string.Format("Media will be uploaded as superior of #{0}\n{1}", PostID, e6_GridItemTemp.cLabel_isSuperior.Tag)); ;
                e6_GridItemTemp.cLabel_isSuperior.Visible = true;
            }
            else
            {

                if (DataRowTemp["Info_TooBig"] != DBNull.Value && Module_Uploader.Media2Big4User(DataRowTemp, true))
                {
                    // e6_GridItemTemp.cCheckBox_UPDL.Checked = false;
                }
                else
                {
                    Form_Loader._FormReference.UploadCounter += (bool)RowRefference["UPDL_Queued"] ? 0 : 1;
                    Form_Loader._FormReference.DownloadCounter += (bool)RowRefference["UPDL_Queued"] ? 0 : 1;
                    RowRefference["UPDL_Queued"] = true;
                    if (!Properties.Settings.Default.API_Key.Equals(""))
                    {
                        Form_Loader._FormReference.GB_Upload.Enabled = Form_Loader._FormReference.UploadCounter > 0;
                    }
                    Form_Loader._FormReference.GB_Download.Enabled = Form_Loader._FormReference.DownloadCounter != 0;
                }
            }
            Form_Preview._FormReference.UpdateButtons();
        }

        public static void InferiorSub(string PostID, DataRow RowRefference)
        {
            string PostTest = Module_e621Info.e621InfoDownload(string.Format("https://e621.net/posts/{0}.json", PostID), true);
            if (PostTest == null || PostTest.Length < 10)
            {
                if (_FormReference != null) _FormReference.ID_TextBox.Text = null;
                MessageBox.Show(string.Format("Post with ID#{0} does not exist.", PostID), "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            JToken PostData = JObject.Parse(PostTest)["post"];
            RowRefference["Upload_Rating"] = PostData["rating"].Value<string>().ToUpper();
            RowRefference["Uploaded_As"] = PostID;
            e6_GridItem e6_GridItemTemp = Form_Loader._FormReference.IsE6PicVisibleInGrid(ref RowRefference);
            if (e6_GridItemTemp != null)
            {
                e6_GridItemTemp._Rating = (string)RowRefference["Upload_Rating"];
                e6_GridItemTemp.cLabel_isUploaded.Text = PostID;
            }
            Form_Preview._FormReference.Label_AlreadyUploaded.Text = string.Format("Already uploaded as #{0}", PostID);
            if (Properties.Settings.Default.ManualInferiorSave)
            {
                Module_DB.DB_CreateMediaRecord(ref RowRefference);
            }
            Form_Preview._FormReference.UpdateButtons();
        }
    }
}
