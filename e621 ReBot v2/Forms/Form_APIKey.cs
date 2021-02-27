using e621_ReBot_v2.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace e621_ReBot_v2.Forms
{
    public partial class Form_APIKey : Form
    {
        public static Form_APIKey _FormReference;
        public Form_APIKey()
        {
            InitializeComponent();
            _FormReference = this;
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
            Dictionary<string, string> POST_RequestData = new Dictionary<string, string>()
            {
                { "login", Properties.Settings.Default.UserName },
                { "api_key", APIKey_TextBox.Text },
                { "score", "1" }
            };

            FormUrlEncodedContent EncodedContent = new FormUrlEncodedContent(POST_RequestData);
            Stream EncodedContentStream = EncodedContent.ReadAsStreamAsync().Result;
            //Debug.Print(New FormUrlEncodedContent(POST_RequestData).ReadAsStringAsync.Result)

            string APIURL = string.Format("https://e621.net/posts/{0}/votes.json", "109434"); //https://e621.net/post/show/109434

            HttpWebRequest e621Request = (HttpWebRequest)WebRequest.Create(APIURL);
            e621Request.UserAgent = Form_Loader._FormReference.AppName_Label.Text;
            e621Request.Method = "POST";
            e621Request.ContentType = "application/x-www-form-urlencoded";
            e621Request.Timeout = 5000;
            EncodedContentStream.CopyTo(e621Request.GetRequestStream());
            EncodedContentStream.Position = 0; // Return stream to beginning
            try
            {
                e621Request.GetResponse().Close();

                // Undo score change on successful validation, just repeat same request
                EncodedContent = new FormUrlEncodedContent(POST_RequestData);
                EncodedContentStream = EncodedContent.ReadAsStreamAsync().Result;

                HttpWebRequest e621Request2 = (HttpWebRequest)WebRequest.Create(APIURL);
                e621Request2.UserAgent = Form_Loader._FormReference.AppName_Label.Text;
                e621Request2.Method = "POST";
                e621Request2.ContentType = "application/x-www-form-urlencoded";
                EncodedContentStream.CopyTo(e621Request2.GetRequestStream());
                e621Request2.GetResponse().Close();
                EncodedContentStream.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                HttpWebResponse TempException = (HttpWebResponse)ex.Response;
                if (TempException.StatusCode == HttpStatusCode.Forbidden) //403
                {
                    //Wrong API Key, e621 denies connection
                }
                e621Request.Abort();
                TempException.Dispose();
                EncodedContentStream.Dispose();
                return false;
            }
            finally
            {
                EncodedContent.Dispose();
            }
        }

        private void Form_APIKey_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form_Loader._FormReference.Invoke(new Action(() => 
            {
                if (Properties.Settings.Default.FirstRun && !Properties.Settings.Default.API_Key.Equals(""))
                {
                    MessageBox.Show("You can change or remove your API Key at any time in settings.", "e621 ReBot");
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("You can now go visit your favorite artists and grab your favorite images.", "e621 ReBot");
                    Form_Loader._FormReference.QuickButtonPanel.Visible = true;
                }
                if (!Properties.Settings.Default.API_Key.Equals(""))
                {
                    Thread ThreadTemp = new Thread(Module_Credits.Check_Credit_All);
                    ThreadTemp.Start();
                }
            }));
            _FormReference = null;
            Dispose();
        }
    }
}
