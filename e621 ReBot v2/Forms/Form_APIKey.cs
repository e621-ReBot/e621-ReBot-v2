using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using e621_ReBot_v2.Modules;
using Newtonsoft.Json.Linq;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_APIKey : Form
    {
        public static Form_APIKey _FormReference;
        public Form_APIKey()
        {
            InitializeComponent();
            _FormReference = this;
            Owner = Form_Loader._FormReference;
        }

        private void APIKey_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox _sender = (TextBox)sender;
                if (_sender.Text.Length < 24)
                {
                    MessageBox.Show("API Key must be 24 characters long.", "e621 ReBot");
                    return;
                }
                if (ValidateAPIKey())
                {
                    Properties.Settings.Default.API_Key = Module_Cryptor.Encrypt(APIKey_TextBox.Text);
                    Form_Loader._FormReference.bU_APIKey.Text = "Remove API Key";
                    Properties.Settings.Default.Save();
                    Module_APIControler.APIEnabled = true;
                    Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = true;
                    Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = true;
                }
                else
                {
                    MessageBox.Show("API Key is not valid.", "e621 ReBot");
                    return;
                }
                e.SuppressKeyPress = true; // to disable sound
                Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true; // to disable sound
                Close();
            }
        }

        public bool ValidateAPIKey()
        {
            HttpWebRequest e6APICheck = (HttpWebRequest)WebRequest.Create($"https://e621.net/users/{Properties.Settings.Default.UserID}.json");
            e6APICheck.UserAgent = Form_Loader._FormReference.AppName_Label.Text;
            e6APICheck.Timeout = 5000;
            e6APICheck.Headers.Add(HttpRequestHeader.Authorization, $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Properties.Settings.Default.UserName}:{APIKey_TextBox.Text}"))}");
            try
            {
                HttpWebResponse HttpWebResponseTemp = (HttpWebResponse)e6APICheck.GetResponse();
                if (HttpWebResponseTemp.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream StreamTemp = HttpWebResponseTemp.GetResponseStream())
                    {
                        using (StreamReader StreamReaderTemp = new StreamReader(StreamTemp))
                        {
                            string JSONReponse = StreamReaderTemp.ReadToEnd();
                            JObject JObjectTemp = JObject.Parse(JSONReponse);
                            if (JObjectTemp.Count > 32)
                            {
                                Properties.Settings.Default.UserLevel = JObjectTemp["level"].Value<string>();
                                Properties.Settings.Default.AppName = $"e621 ReBot ({Properties.Settings.Default.UserName})";
                                Form_Loader._FormReference.AppName_Label.Text = Properties.Settings.Default.AppName;
                                return true;
                            }
                            else
                            {
                                MessageBox.Show(this, "API key is not valid.", "e621 ReBot");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(this, $"Error!\n\nStatus Code: {HttpWebResponseTemp.StatusCode}", "e621 ReBot");
                }
            }
            catch (WebException ex)
            {
                using (HttpWebResponse HttpWebResponseTemp = (HttpWebResponse)ex.Response)
                {
                    MessageBox.Show(this, $"{ex.Message}\n\nStatus Code: {HttpWebResponseTemp.StatusCode}", "e621 ReBot");
                }
            }
            return false;
        }

        private void Form_APIKey_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Loader._FormReference.Invoke(new Action(() =>
            {
                if (Properties.Settings.Default.FirstRun && !string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
                {
                    MessageBox.Show("You can change or remove your API Key at any time in settings.", "e621 ReBot");
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("You can now go visit your favorite artists and grab your favorite images.", "e621 ReBot");
                    Form_Loader._FormReference.QuickButtonPanel.Visible = true;
                }
                if (!string.IsNullOrEmpty(Properties.Settings.Default.API_Key))
                {
                    new Thread(Module_Credits.Check_Credit_All).Start();
                }
            }));
            Owner = null;
            _FormReference = null;
            Dispose();
        }
    }
}