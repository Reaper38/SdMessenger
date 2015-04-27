using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    public class ListViewEx : ListView
    {
        [DllImport("UxTheme", CharSet = CharSet.Unicode)]
        private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
        
        protected override void WndProc(ref Message m)
        {
            // 0x0201 = 513 LMB_Down
            // 0x0204 = 516 RMB_Down
            // 0x0203 = 515 LMB_Double
            // 0x0206 = 518 RMB_Double
            if (m.Msg == 513 || m.Msg == 516 || m.Msg == 515 || m.Msg == 518)
            {
                // ignore clicks on empty space
                if (HitTest(PointToClient(MousePosition)).Item == null)
                    return;
            }
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Environment.OSVersion.Version.Major >= 6)
                SetWindowTheme(Handle, "explorer", null);
        }
    }
}
