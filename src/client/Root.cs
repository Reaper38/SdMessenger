using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sdm.Core;

namespace Sdm.Client
{
    internal static class Root
    {
        [STAThread]
        private static void Main()
        {
            SdmCore.Initialize(AppType.Client);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainDlg = new MainDialog();
            Application.Run(mainDlg);
            SdmCore.Destroy();
        }
    }
}
