using System;
using System.ComponentModel;
using Sdm.Client.Controls;

namespace Sdm.Client
{
    public partial class LogDialog : FormEx
    {
        public LogDialog()
        {
            InitializeComponent();
            xrConsole.CommandLineEnabled = false;
        }

        public xr.Console Console
        { get { return xrConsole; } }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            Hide();
        }
    }
}
