using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_PoolWatcher : Form
    {
        public static Form_PoolWatcher _FormReference;

        public Form_PoolWatcher(Point LocationPass)
        {
            InitializeComponent();
            _FormReference = this;
            Location = new Point(LocationPass.X - 8, LocationPass.Y);
        }

        private void Form_PoolWatcher_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PoolWatcher))
            {
                TreeView_PoolWatcher.BeginUpdate();
                foreach (JToken PoolToken in JArray.Parse(Properties.Settings.Default.PoolWatcher))
                {
                    string PoolName = PoolToken["name"].Value<string>().Replace("_", " ").Trim();
                    string PoolID = PoolToken["id"].Value<string>();
                    TreeNode PoolNode = new TreeNode()
                    {
                        Text = $"{PoolName} | Posts: {PoolToken["post_ids"].Count()}",
                        Name = PoolID,
                        ToolTipText = $"Pool ID#{PoolID}",
                        Tag = PoolToken
                    };
                    TreeView_PoolWatcher.Nodes.Add(PoolNode);
                }
                TreeView_PoolWatcher.EndUpdate();
            }
            Text = $"Pool Watcher - Watching {TreeView_PoolWatcher.Nodes.Count} pool{((TreeView_PoolWatcher.Nodes.Count > 0 && TreeView_PoolWatcher.Nodes.Count < 2) ? null : "s")}";
        }

        private void TreeView_PoolWatcher_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private TreeNode WhichNodeIsIt;

        private void TreeView_PoolWatcher_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            WhichNodeIsIt = e.Node;
        }

        private void ToolStripMenuItem_AddNode_Click(object sender, EventArgs e)
        {
            string AddPoolID = Form_IDForm.Show(this, Cursor.Position, "Enter Pool ID");

            if (AddPoolID != null)
            {
                if (TreeView_PoolWatcher.Nodes.ContainsKey(AddPoolID))
                {
                    MessageBox.Show($"Pool with ID#{AddPoolID} is already being watched.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string e6JSONResult = Module_e621Info.e621InfoDownload($"https://e621.net/pools/{AddPoolID}.json");
                if (e6JSONResult == null)
                {
                    MessageBox.Show($"Pool with ID#{AddPoolID} does not exist.", "e621 ReBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                JObject PoolJSON = JObject.Parse(e6JSONResult);

                string PoolName = PoolJSON["name"].Value<string>().Replace("_", " ").Trim();
                JObject JObjectTemp = new JObject
                {
                    { "id", PoolJSON["id"].Value<int>() },
                    { "name", PoolName },
                    { "post_ids", PoolJSON["post_ids"].Value<JToken>() }
                    //{ "post_count",  PoolJSON["post_count"].Value<int>() }
                };
                TreeNode PoolNode = new TreeNode()
                {
                    Text = $"{PoolName} | Posts: {PoolJSON["post_count"].Value<int>()}",
                    Name = AddPoolID,
                    ToolTipText = $"Pool ID#{AddPoolID}",
                    Tag = JObjectTemp
                };
                TreeView_PoolWatcher.Nodes.Add(PoolNode);
                PoolJSON = null;

                Module_e621APIMinion.AddWork2Queue("Pool Watcher", Module_e621APIMinion.GraBPoolInBG, AddPoolID);

                Text = $"Pool Watcher - Watching {TreeView_PoolWatcher.Nodes.Count} pool{((TreeView_PoolWatcher.Nodes.Count > 0 && TreeView_PoolWatcher.Nodes.Count < 2) ? null : "s")}";
            }
        }

        private void ToolStripMenuItem_RemoveNode_Click(object sender, EventArgs e)
        {
            TreeView_PoolWatcher.Nodes.Remove(WhichNodeIsIt);
            this.Text = $"Pool Watcher - Watching {TreeView_PoolWatcher.Nodes.Count} pool{((TreeView_PoolWatcher.Nodes.Count > 0 && TreeView_PoolWatcher.Nodes.Count < 2) ? null : "s")}";
        }

        private void TreeView_PoolWatcher_MouseDown(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo NodeHitTest = TreeView_PoolWatcher.HitTest(e.Location);
            if (NodeHitTest.Node != null)
            {
                TreeView_PoolWatcher.SelectedNode = null;
                NodeHitTest.Node.ForeColor = TreeView_PoolWatcher.ForeColor;
                NodeHitTest.Node.BackColor = TreeView_PoolWatcher.BackColor;
                if (NodeHitTest.Location == TreeViewHitTestLocations.Label)
                {
                    NodeHitTest.Node.ContextMenuStrip = contextMenuStrip_Remove;
                }
                else
                {
                    NodeHitTest.Node.ContextMenuStrip = contextMenuStrip_Add;
                }
            }
        }

        private void TreeView_PoolWatcher_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }




        public static void PoolWatcher_Check4New()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PoolWatcher))
            {
                Dictionary<string, List<string>> PoolWatherDictionary = new Dictionary<string, List<string>>();

                foreach (JToken PoolToken in JArray.Parse(Properties.Settings.Default.PoolWatcher))
                {
                    PoolWatherDictionary.Add(PoolToken["id"].Value<string>(), PoolToken["post_ids"].Values<string>().ToList());
                }

                JArray PoolWatcherSave = new JArray();
                Dictionary<JObject, List<string>> PoolPosts2Get = new Dictionary<JObject, List<string>>();
                int PageSize = 75; //new API limit
                for (int i = 0; i < Math.Ceiling(PoolWatherDictionary.Keys.Count / (double)PageSize); i++)
                {
                    string ListSlice = string.Join(",", PoolWatherDictionary.Keys.ToList().Skip(i * PageSize).Take(PageSize));
                    string e6JSONResult = Module_e621Info.e621InfoDownload($"https://e621.net/pools.json?search[id]={ListSlice}", true);
                    if (e6JSONResult != null && e6JSONResult.Length > 24)
                    {
                        foreach (JObject CurrentPoolData in JArray.Parse(e6JSONResult))
                        {
                            string PoolName = CurrentPoolData["name"].Value<string>().Replace("_", " ").Trim();
                            JObject JObjectTemp = new JObject
                            {
                                { "id", CurrentPoolData["id"].Value<int>() },
                                { "name", PoolName },
                                { "post_ids", CurrentPoolData["post_ids"].Value<JToken>() }
                                //{ "post_count",  CurrentPoolData["post_count"].Value<int>() }
                            };
                            PoolWatcherSave.Add(JObjectTemp);

                            string PoolID = CurrentPoolData["id"].Value<string>();
                            List<string> NewPostsIfAny = CurrentPoolData["post_ids"].Values<string>().ToList().Except(PoolWatherDictionary[PoolID]).ToList();
                            if (NewPostsIfAny.Count > 0)
                            {
                                PoolPosts2Get.Add(CurrentPoolData, NewPostsIfAny);
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                    Thread.Sleep(1000);
                }
                Properties.Settings.Default.PoolWatcher = JsonConvert.SerializeObject(PoolWatcherSave);
                Properties.Settings.Default.Save();
                if (PoolPosts2Get.Keys.Count > 0)
                {
                    GetNewImages(PoolPosts2Get);
                }
            }
        }

        private static void GetNewImages(Dictionary<JObject, List<string>> PoolPosts2Get)
        {
            Dictionary<string, JObject> PoolReferer = new Dictionary<string, JObject>();
            foreach (KeyValuePair<JObject, List<string>> Pool2Get in PoolPosts2Get)
            {
                foreach (string PostID2Pool in Pool2Get.Value)
                {
                    PoolReferer.Add(PostID2Pool, Pool2Get.Key);
                }
            }

            int ItemsAddedCount = 0;
            int PageSize = 75; //new API limit
            for (int i = 0; i < Math.Ceiling(PoolReferer.Keys.Count / (double)PageSize); i++)
            {
                string ListSlice = string.Join(",", PoolReferer.Keys.ToList().Skip(i * PageSize).Take(PageSize));
                string e6JSONResult = Module_e621Info.e621InfoDownload($"https://e621.net/posts.json?tags=id:{ListSlice}", true);
                if (e6JSONResult != null && e6JSONResult.Length > 24)
                {
                    JToken PostData = JObject.Parse(e6JSONResult)["posts"];
                    string PoolPostID;
                    string PoolName;
                    foreach (JObject PostDataDetailed in PostData)
                    {
                        PoolPostID = PostDataDetailed["id"].Value<string>();
                        PoolName = PoolReferer[PoolPostID]["name"].Value<string>().Replace("_", " ").Trim();
                        PoolName = string.Join("", PoolName.Split(Path.GetInvalidFileNameChars()));

                        Module_Downloader.AddDownloadQueueItem(
                            DataRowRef: null,
                            URL: $"https://e621.net/posts/{PoolPostID}",
                            Media_URL: PostDataDetailed["file"]["url"].Value<string>(),
                            e6_PostID: PoolPostID,
                            e6_PoolName: PoolName,
                            e6_PoolPostIndex: Array.IndexOf(PoolReferer[PoolPostID]["post_ids"].ToObject<string[]>(), PoolPostID).ToString()
                            );
                        ItemsAddedCount += 1;
                    }
                }
                else
                {
                    return;
                }
                Thread.Sleep(1000);
            }
            if (ItemsAddedCount > 0)
            {
                Form_Loader._FormReference.BeginInvoke(new Action(() =>
                {
                    Form_Loader._FormReference.textBox_Info.Text = $"{DateTime.Now.ToLongTimeString()}, Pool Watcher >>> Started download of {ItemsAddedCount} image{(ItemsAddedCount > 1 ? "s" : null)}\n{Form_Loader._FormReference.textBox_Info.Text}";
                    Module_Downloader.timer_Download.Start();
                }));
            }
        }

        private void SavePoolWatcherSettings()
        {
            if (_FormReference != null)
            {
                if (TreeView_PoolWatcher.Nodes.Count > 0)
                {
                    JArray JArrayTemp = new JArray();
                    foreach (TreeNode PoolWatchedPool in TreeView_PoolWatcher.Nodes)
                    {
                        JArrayTemp.Add((JToken)PoolWatchedPool.Tag);
                    }
                    Properties.Settings.Default.PoolWatcher = JsonConvert.SerializeObject(JArrayTemp);
                }
                else
                {
                    Properties.Settings.Default.PoolWatcher = "";
                }
                Properties.Settings.Default.Save();
            }
        }

        private void Form_PoolWatcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePoolWatcherSettings();
        }

        private void Form_PoolWatcher_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Loader._FormReference.Activate();
            Form_Loader._FormReference.Focus();
            _FormReference = null;
            Dispose();
        }
    }
}