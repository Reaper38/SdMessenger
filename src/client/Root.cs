using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sdm.Client
{
    internal static class Root
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
        }
    }
}
