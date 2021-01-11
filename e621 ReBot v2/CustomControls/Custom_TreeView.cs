using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//https://stackoverflow.com/questions/10362988/treeview-flickering/10364283#10364283
namespace e621_ReBot_v2.CustomControls
{
    public class Custom_TreeView : TreeView
    {

        //Doublebuffer
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        //Focus bug
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x203)
            {
                var LocalPos = PointToClient(Cursor.Position);
                var hitTestInfo = HitTest(LocalPos);
                if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
                {
                    m.Result = (IntPtr)0;
                }
                else
                {
                    base.WndProc(ref m);
                }

            }
            else
            {
                base.WndProc(ref m);
            }
        }

        //"remove" highlight color

        public Custom_TreeView()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }


        public Color NodeBackColor { get; set; } = Color.FromArgb(0, 45, 90);
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            using (SolidBrush TempBrush = new SolidBrush(NodeBackColor))
            {
                e.Graphics.FillRectangle(TempBrush, e.Bounds);
            }
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            Color foreColor = e.Node.ForeColor;
            if (foreColor == Color.Empty)
            {
                foreColor = e.Node.TreeView.ForeColor;
            }
            TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, foreColor, TextFormatFlags.GlyphOverhangPadding);
        }
    }
}
