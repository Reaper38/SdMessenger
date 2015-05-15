using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal class RichTextBoxEx : RichTextBox
    {
        private const int WM_USER = 0x400;
        private const int SB_VERT = 1;
        private const int EM_SETSCROLLPOS = WM_USER + 222;
        private const int EM_GETSCROLLPOS = WM_USER + 221;

        public void AppendRtf(string rtf)
        {
            var s = SelectionStart;
            var l = SelectionLength;
            Select(TextLength, 0);
            SelectedRtf = rtf;
            Select(s, l);
        }

        [DllImport("user32.dll")]
        private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        public Point GetScrollPosition()
        {
            var scrollPos = Point.Empty;
            SendMessage(Handle, EM_GETSCROLLPOS, 0, ref scrollPos);
            return scrollPos;
        }

        public int GetMaxScrollPosition()
        {
            int minScroll, maxScroll;
            GetScrollRange(Handle, SB_VERT, out minScroll, out maxScroll);
            return maxScroll;
        }

        public void ScrollToEnd()
        {
            var scrollPos = GetScrollPosition();
            scrollPos.Y = GetMaxScrollPosition() - ClientSize.Height;
            SendMessage(Handle, EM_SETSCROLLPOS, 0, ref scrollPos);
        }
    }
}
