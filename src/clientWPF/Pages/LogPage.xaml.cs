using System;
using System.Windows.Controls;

namespace Sdm.ClientWPF.Pages
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {

        public xr.Console Console
        { get { return (xr.Console)wfHost.Child; } }

        public LogPage()
        {
            InitializeComponent();
            Console.CommandLineEnabled = false;
        }
    }
}
