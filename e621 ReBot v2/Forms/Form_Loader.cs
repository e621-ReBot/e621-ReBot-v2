﻿using e621_ReBot_v2.CustomControls;
using e621_ReBot_v2.Forms;
using e621_ReBot_v2.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace e621_ReBot_v2
{
    public partial class Form_Loader : Form
    {
        public static Form_Main _FormReference;
        public static FlowLayoutPanel _GridFLPHolder;
        public static int _GridMaxControls;
        public static int _DLHistoryMaxControls;
        public static readonly string GlobalUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0";

        public Form_Loader()
        {
            InitializeComponent();

            Load += Form_Loader_Load;
            Cursor_Default = Module_Cursor.CreateCursorNoResize(Properties.Resources.e621ReBot_CursorDefault, 0, 0);
            Cursor_ReBotNav = Module_Cursor.CreateCursorNoResize(Properties.Resources.e621ReBot_CursorE6, 0, 0);
            Cursor_BrowserNav = Module_Cursor.CreateCursorNoResize(Properties.Resources.e621ReBot_CursorBrowser, 0, 0);
            timer_CursorFix = new Timer
            {
                Interval = 250
            };
            timer_CursorFix.Tick += Timer_CursorFix_Tick;
        }

        private void Form_Loader_Load(object sender, EventArgs e)
        {
            try
            {
                Module_CefSharp.InitializeBrowser("about:blank");
            }
            catch (FileNotFoundException)
            {
                if (MessageBox.Show("CefSharp dependencies not found or they are an older version. Do you want to visit the download page on GitHub now?\nI can not work without those files so you should really download them.", "e621 Rebot v2", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start("https://github.com/e621-ReBot/e621-ReBot-v2/releases");
                }
                else
                {
                    Close();
                    return;
                }
            }
            if (Properties.Settings.Default.LoadBigForm)
            {
                _FormReference = new Form_MainBig();
                _GridMaxControls = 9 * 4;
                _DLHistoryMaxControls = 7 * 4;
                Module_Downloader.DownloadNodeMax = 54;
            }
            else
            {
                _FormReference = new Form_Main();
                _GridMaxControls = 6 * 3;
                _DLHistoryMaxControls = 7 * 2;
                Module_Downloader.DownloadNodeMax = 15;
            }
            _GridFLPHolder = _FormReference.flowLayoutPanel_Grid;

            _FormReference.Show();
            timer_CursorFix.Start();
        }

        public static Cursor Cursor_Default;
        public static Cursor Cursor_ReBotNav;
        public static Cursor Cursor_BrowserNav;

        public static GridItemStyle _customGIStyle;
        public static CustomPBStyle _customPBStyle;

        public static bool LastAltState;
        private static bool LastCtrlState;
        public static Timer timer_CursorFix;
        private void Timer_CursorFix_Tick(object sender, EventArgs e)
        {
            if (GetAsyncKeyState(Keys.F1) == -32767)
            {
                Form FormTemp;
                for (int i = Application.OpenForms.Count - 1; i > 0; i--)
                {
                    FormTemp = Application.OpenForms[i];
                    switch (FormTemp.Name)
                    {
                        case "Form_Main":
                            {
                                FormTemp.WindowState = FormWindowState.Minimized;
                                FormTemp.Hide();
                                _FormReference.notifyIcon_Main.Visible = true;
                                _FormReference.notifyIcon_Main.ShowBalloonTip(1000);
                                _FormReference.ShowInTaskbar = false;
                                break;
                            }

                        default:
                            {
                                FormTemp.Close();
                                break;
                            }
                    }
                }
                return;
            }

            bool AltState = ModifierKeys.HasFlag(Keys.Alt);
            bool CtrlState = ModifierKeys.HasFlag(Keys.Control);
            if (AltState == LastAltState && CtrlState == LastCtrlState)
            {
                return;
            }
            LastAltState = AltState;
            LastCtrlState = CtrlState;
            Cursor GetCurrentCursor = Cursor.Current;

            if (_FormReference.cTabControl_e621ReBot.SelectedIndex == 1)
            {
                foreach (e6_GridItem e6_GridItemTemp in _GridFLPHolder.Controls)
                {
                    Cursor CursorSelector;
                    if (AltState)
                    {
                        CursorSelector = Cursor_BrowserNav;
                        e6_GridItemTemp.cLabel_isSuperior.Cursor = Cursor_BrowserNav;
                        e6_GridItemTemp.cLabel_isUploaded.Cursor = Cursor_BrowserNav;
                    }
                    else
                    {
                        CursorSelector = CtrlState ? Cursor_ReBotNav : Cursor_Default;
                        e6_GridItemTemp.cLabel_isSuperior.Cursor = Cursor_ReBotNav;
                        e6_GridItemTemp.cLabel_isUploaded.Cursor = Cursor_ReBotNav;
                    }
                    e6_GridItemTemp.pictureBox_ImageHolder.Cursor = CursorSelector;
                    e6_GridItemTemp.cLabel_Rating.Cursor = CursorSelector;
                    e6_GridItemTemp.cPanel_Rating.Cursor = CursorSelector;
                    e6_GridItemTemp.cLabel_TagWarning.Cursor = CursorSelector;
                    e6_GridItemTemp.cCheckBox_UPDL.Cursor = Cursors.Hand;
                }

                List<Button_Unfocusable> ButtonHolder = new List<Button_Unfocusable> { _FormReference.GB_Check, _FormReference.GB_Inverse, _FormReference.GB_Uncheck };
                foreach (Button_Unfocusable ButtonChange in ButtonHolder)
                {
                    ButtonChange.Text = ButtonChange.Tag.ToString() + (ModifierKeys.HasFlag(Keys.Control) ? " All" : null);
                }
            }

            if (_FormReference.cTabControl_e621ReBot.SelectedIndex == 6)
            {
                _FormReference.labelPuzzle_SelectedPost.Cursor = AltState ? Cursor_BrowserNav : Cursor_ReBotNav;
            }

            if (Form_Preview._FormReference != null)
            {
                Form_Preview._FormReference.Label_AlreadyUploaded.Cursor = AltState ? Cursor_BrowserNav : Cursor_ReBotNav;
            }

            if (Form_SimilarSearch._FormReference != null)
            {
                Cursor CursorSelector = CtrlState ? Cursor_Default : (AltState ? Cursor_BrowserNav : Cursors.No);
                foreach (GroupBox GB in Form_SimilarSearch._FormReference.FlowLayoutPanel_Holder.Controls.OfType<GroupBox>())
                {
                    ((PictureBox)GB.Controls[0]).Cursor = CursorSelector;
                }
            }

            //Almost a fix for cursor changing in the controls above the ones that have cursor changed.
            Cursor.Current = GetCurrentCursor;
            Cursor.Position = new Point(Cursor.Position.X - 1, Cursor.Position.Y);
            Cursor.Position = new Point(Cursor.Position.X + 1, Cursor.Position.Y);
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
    }

    //http://csharphelper.com/blog/2017/01/convert-a-bitmap-into-a-cursor-in-c/
    //https://stackoverflow.com/questions/550918/change-cursor-hotspot-in-winforms-net
    static class Module_Cursor
    {
        struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        // Create a cursor from a bitmap without resizing and with the specified hot spot
        public static Cursor CreateCursorNoResize(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
    }
}