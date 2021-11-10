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

        public Form_PoolWatcher(Point LocationPass, Form OwnerPass)
        {
            InitializeComponent();
            _FormReference = this;
            Location = new Point(LocationPass.X - 8, LocationPass.Y);
            Owner = OwnerPass;
        }

        private void Form_PoolWatcher_Load(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.PoolWatcher.Equals(""))
            {
                TreeView_PoolWatcher.BeginUpdate();
                foreach (JToken PoolToken in JArray.Parse(Properties.Settings.Default.PoolWatcher))
                {
                    string PoolName = PoolToken["name"].Value<string>().Replace("_", " ");
                    string PoolID = PoolToken["id"].Value<string>();
                    TreeNode PoolNode = new TreeNode()
                    {
                        Text = string.Format("{0} | Posts: {1}", PoolName, PoolToken["post_count"].Value<int>()),
                        Name = PoolID,
                        ToolTipText = "Pool ID#" + PoolID,
                        Tag = PoolToken
                    };
                    TreeView_PoolWatcher.Nodes.Add(PoolNode);
                }
                TreeView_PoolWatcher.EndUpdate();
            }
            this.Text = $"Pool Watcher - Watching {TreeView_PoolWatcher.Nodes.Count} pool{((TreeView_PoolWatcher.Nodes.Count > 0 && TreeView_PoolWatcher.Nodes.Count < 2) ? null : "s")}";
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
            if (Form_e6Pool._FormReference == null)
            {
                new Form_e6Pool(Cursor.Position, this);
            }
            Form_e6Pool._FormReference.BringToFront();
            Form_e6Pool._FormReference.ShowDialog();
            this.Text = $"Pool Watcher - Watching {TreeView_PoolWatcher.Nodes.Count} pool{((TreeView_PoolWatcher.Nodes.Count > 0 && TreeView_PoolWatcher.Nodes.Count < 2) ? null : "s")}";
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
            if (!Properties.Settings.Default.PoolWatcher.Equals(""))
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
                            JObject JObjectTemp = new JObject
                        {
                            { "id", CurrentPoolData["id"].Value<int>() },
                            { "name", CurrentPoolData["name"].Value<string>() },
                            { "post_ids", CurrentPoolData["post_ids"].Value<JToken>() },
                            { "post_count",  CurrentPoolData["post_count"].Value<int>() }
                        };
                            PoolWatcherSave.Add(JObjectTemp);

                            string PoolID = CurrentPoolData["id"].Value<string>();
                            List<string> NewPostsIfAny = CurrentPoolData["post_ids"].Values<string>().ToList().Except(PoolWatherDictionary[PoolID]).ToList();
                            if (NewPostsIfAny.Count > 0)
                            {
                                PoolPosts2Get.Add(CurrentPoolData, NewPostsIfAny);
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        return;
                    }
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
            foreach (KeyValuePair<JObject, List<string>> Pool2Get in PoolPosts2Get)
            {
                JObject PoolData_JSON = Pool2Get.Key;

                string PostsString = string.Join(",", Pool2Get.Value);
                string e6JSONResult = Module_e621Info.e621InfoDownload("https://e621.net/posts.json?tags=id:" + PostsString, true);
                if (e6JSONResult != null && e6JSONResult.Length > 24)
                {
                    JToken CurrentPoolPosts_Unsorted = JObject.Parse(e6JSONResult)["posts"];
                    Dictionary<string, JToken> CurrentPoolPosts_Sorted = new Dictionary<string, JToken>();
                    foreach (JObject Post in CurrentPoolPosts_Unsorted)
                    {
                        CurrentPoolPosts_Sorted.Add(Post["id"].Value<string>(), Post);
                    }
                    CurrentPoolPosts_Unsorted = null;

                    int ItemsAddedCount = 0;
                    foreach (string PoolPostID in Pool2Get.Value)
                    {
                        string PoolName = PoolData_JSON["name"].Value<string>().Replace("_", " ");
                        PoolName = string.Join("", PoolName.Split(Path.GetInvalidFileNameChars()));

                        Module_Downloader.AddDownloadQueueItem(
                            DataRowRef: null,
                            URL: "https://e621.net/posts/" + PoolPostID,
                            Media_URL: CurrentPoolPosts_Sorted[PoolPostID]["file"]["url"].Value<string>(),
                            e6_PostID: PoolPostID,
                            e6_PoolName: PoolName,
                            e6_PoolPostIndex: CurrentPoolPosts_Sorted.Keys.ToList().IndexOf(PoolPostID).ToString()
                            );

                        ItemsAddedCount += 1;
                    }

                    Form_Loader._FormReference.BeginInvoke(new Action(() =>
                    {
                        if (ItemsAddedCount > 0)
                        {
                            Form_Loader._FormReference.textBox_Info.Text = string.Format("{0} Pool Watcher >>> Started download of {1} image{2}\n{3}", DateTime.Now.ToLongTimeString(), ItemsAddedCount, ItemsAddedCount > 1 ? "s" : "", Form_Loader._FormReference.textBox_Info.Text);
                        }
                        Module_Downloader.timer_Download.Start();
                    }));
                    CurrentPoolPosts_Sorted = null;
                }
                Thread.Sleep(1000);
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
            Owner.Activate();
            Owner.Focus();
            _FormReference = null;
            Dispose();
        }
    }
}
