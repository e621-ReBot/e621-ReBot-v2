using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace e621_ReBot_v2.Modules
{
    //https://www.jitbit.com/alexblog/283-webclient-async--with-timeout/
    //https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
    class Custom_WebClient : WebClient
    {
        public Custom_WebClient()
        {
            DLAsyncTimeoutTimer.Interval = 1000 * 30;
            DLAsyncTimeoutTimer.Tick += TimeoutTick;
        }

        private Timer DLAsyncTimeoutTimer = new Timer();
        private void TimeoutTick(object sender, EventArgs e)
        {
            DLAsyncTimeoutTimer.Stop();
            CancelAsync();
        }

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            DLAsyncTimeoutTimer.Stop();
            base.OnDownloadProgressChanged(e);
            DLAsyncTimeoutTimer.Start();
        }

        protected override void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
        {
            DLAsyncTimeoutTimer.Stop();
            base.OnDownloadFileCompleted(e);
        }
    }
}
