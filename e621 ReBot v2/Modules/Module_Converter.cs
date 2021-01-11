using e621_ReBot_v2.Forms;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    public class Module_Converter
    {
        static Module_Converter()
        {
            timer_Conversion = new Timer
            {
                Interval = 1000
            };
            timer_Conversion.Tick += Timer_Conversion_Tick;
            Conversion_BGW = new BackgroundWorker();
            Conversion_BGW.DoWork += ConversionBGW_Start;
            Conversion_BGW.RunWorkerCompleted += ConversionBGW_Done;
        }

        public static void Report_Info(string InfoMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.textBox_Info.Text = string.Format("{0} Conversionist >>> {1}\n", DateTime.Now.ToLongTimeString(), InfoMessage) + Form_Loader._FormReference.textBox_Info.Text;
            }
            ));
        }

        public static void Report_Status(string StatusMessage)
        {
            Form_Loader._FormReference.BeginInvoke(new Action(() =>
            {
                Form_Loader._FormReference.label_ConversionStatus.Text = string.Format("Status: {0}", StatusMessage);
            }
            ));
        }

        public static Timer timer_Conversion;
        private static void Timer_Conversion_Tick(object sender, EventArgs e)
        {
            timer_Conversion.Stop();
            if (Form_Loader._FormReference.cCheckGroupBox_Convert.Checked && Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes.Count > 0 && !Conversion_BGW.IsBusy)
            {
                Conversion_BGW.RunWorkerAsync();
            }
        }

        public static BackgroundWorker Conversion_BGW;
        private static DataRow TaskRow;
        private static void ConversionBGW_Start(object sender, DoWorkEventArgs e)
        {
            timer_Conversion.Stop();

            TreeNode ConversionNode = Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes[0];

            string URL = ConversionNode.Name;
            TaskRow = (DataRow)ConversionNode.Tag;
            if (URL.Contains("ugoira"))
            {
                Module_FFmpeg.ConversionQueue_Ugoira2WebM(ref TaskRow);
            }
            else
            {
                if (URL.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || URL.EndsWith(".swf", StringComparison.OrdinalIgnoreCase))
                {
                    Module_FFmpeg.ConversionQueue_Videos2WebM(ref TaskRow);
                }
                else
                {
                    Form_Loader._FormReference.Invoke(new Action(() => { MessageBox.Show("Errored somehow @ CoversionQueue_Run", "e621 ReBot"); }));
                }
            }
        }

        private static void ConversionBGW_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Form_Preview._FormReference != null && ReferenceEquals(Form_Preview._FormReference.Preview_RowHolder, TaskRow))
            {
                Form_Preview._FormReference.Label_Download.Visible = false;
                Form_Preview._FormReference.PB_ViewFile.Visible = true;
                Form_Preview._FormReference.PB_ViewFile.Text = "▶";
            }
            Report_Status("Waiting...");
            Form_Loader._FormReference.cTreeView_ConversionQueue.Nodes[0].Remove();
            timer_Conversion.Start();

            //string ImageName = Module_Downloader.GetMediasFileNameOnly((string)TaskRow["Grab_MediaURL"]);
            Module_Downloader.Download_AlreadyDownloaded.Add((string)TaskRow["Grab_MediaURL"]);
        }

    }
}
