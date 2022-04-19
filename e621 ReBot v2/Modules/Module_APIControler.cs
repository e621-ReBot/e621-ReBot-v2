namespace e621_ReBot_v2.Modules
{
    public class Module_APIControler
    {
        public static bool APIEnabled = false;

        public static void ToggleStatus()
        {
            APIEnabled = !APIEnabled;
            Form_Loader._FormReference.cCheckGroupBox_Upload.Checked = APIEnabled;
            Form_Loader._FormReference.cCheckGroupBox_Retry.Checked = APIEnabled;
            Form_Loader._FormReference.bU_PoolWatcher.Enabled = APIEnabled;
            Form_Loader._FormReference.bU_RefreshCredit.Enabled = APIEnabled;
            if (APIEnabled)
            {
                Module_Uploader.timer_Upload.Start();
                Module_Retry.timer_Retry.Start();
            }
            else
            {
                Module_Uploader.timer_Upload.Stop();
                Module_Uploader.timer_UploadDisable.Stop();
                Module_Retry.timer_Retry.Stop();
                Module_Retry.timer_RetryDisable.Stop();
            }
        }
    }
}
