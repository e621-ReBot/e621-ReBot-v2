using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace e621_ReBot_v2
{
    static class Program
    {
        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LoadMergedDLLs;
        }
        private static Assembly LoadMergedDLLs(object sender, ResolveEventArgs e)
        {
            AssemblyName currentAssembly = new AssemblyName(e.Name);

            switch (currentAssembly.Name)
            {
                case "System.Data.SQLite":
                    {
                        return Assembly.Load(Properties.Resources.System_Data_SQLite);
                    }

                case "HtmlAgilityPack":
                    {
                        return Assembly.Load(Properties.Resources.HtmlAgilityPack);
                    }

                case "Newtonsoft.Json":
                    {
                        return Assembly.Load(Properties.Resources.Newtonsoft_Json);
                    }

                default:
                    {
                        return null;
                    }
            }
        }

        //https://www.codeproject.com/Articles/32908/C-Single-Instance-App-With-the-Ability-To-Restore
        public static readonly int WM_SHOWFIRSTINSTANCE = WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);
        static Mutex AppMutex;
        [STAThread]
        static void Main()
        {
            //Check updates
            if (File.Exists("update.check"))
            {
                DateTime DateTimeUpdate = DateTime.Parse(File.ReadAllText("update.check")).AddDays(Properties.Settings.Default.UpdateDays);
                if (DateTimeUpdate < DateTime.UtcNow && File.Exists("e621 ReBot Updater.exe"))
                {
                    Process.Start("e621 ReBot Updater.exe");
                    Application.Exit();
                }
            }
            else
            {
                if (File.Exists("e621 ReBot Updater.exe"))
                {
                    Process.Start("e621 ReBot Updater.exe");
                    Application.Exit();
                }
            }

            bool onlyInstance;
            AppMutex = new Mutex(true, $"Local\\e621 ReBot v2 - {ProgramInfo.AssemblyGuid}", out onlyInstance);

            if (!onlyInstance)
            {
                //Show first instance
                WinApi.PostMessage((IntPtr)WinApi.HWND_BROADCAST, WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);
                return;
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form_Loader());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            AppMutex.ReleaseMutex();
        }

    }
    static internal class WinApi
    {
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        public static int RegisterWindowMessage(string format, params object[] args)
        {
            string message = string.Format(format, args);
            return RegisterWindowMessage(message);
        }

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowToFront(IntPtr window)
        {
            ShowWindow(window, SW_SHOWNORMAL);
            SetForegroundWindow(window);
        }
    }

    static public class ProgramInfo
    {
        static public string AssemblyGuid
        {
            get
            {
                object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((GuidAttribute)attributes[0]).Value;
            }
        }
    }
}
