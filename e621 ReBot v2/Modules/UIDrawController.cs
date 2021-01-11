using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace e621_ReBot.Modules
{
    public class UIDrawController
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(Control ctrl)
        {
            SendMessage(ctrl.Handle, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing(Control ctrl)
        {
            SendMessage(ctrl.Handle, WM_SETREDRAW, true, 0);
            ctrl.Refresh();
        }
    }
}