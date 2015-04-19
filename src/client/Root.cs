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
            Application.Run();
            SdmCore.Destroy();
        }
    }
}
