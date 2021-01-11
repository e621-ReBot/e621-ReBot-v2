using System;
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


        static Mutex AppMutex = new Mutex(true, "e621 ReBot v2");
        [STAThread]
        static void Main()
        {
            if (AppMutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form_Loader());
                AppMutex.ReleaseMutex();
            }
            else
            {
                // send our Win32 message to make the currently running instance
                // jump on top of all the other windows
                NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            }
        }

    }
    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
