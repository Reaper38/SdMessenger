using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sdm.ClientWPF.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public enum Error
        {
            Host,
            Login,
            Password,
            Generic
        }

        private const int TooltipDuration = 2000;
        public LoginPage()
        {
            InitializeComponent();
        }

        public string Host
        {
            get { return tbHost.Text; }
            set { tbHost.Text = value; }
        }

        public string Login
        {
            get { return tbLogin.Text; }
            set { tbLogin.Text = value; }
        }

        public string Password
        {
            get { return tbPassword.Password; }
            set { tbPassword.Password = value; }
        }

        public bool SavePassword
        {
            get { return chkSavePass.IsChecked.Value; }
            set { chkSavePass.IsChecked = value; }
        }

        public void EnableControls(bool enable)
        {
            tbHost.IsEnabled = enable;
            tbLogin.IsEnabled = enable;
            tbPassword.IsEnabled = enable;
            chkSavePass.IsEnabled = enable;
            btnConnect.IsEnabled = enable;
        }

        public void ShowError(Error err, string msg)
        {
            switch (err)
            {
                case Error.Host:
                    ShowError(msg, tbHost);
                    tbHost.Focus();
                    break;
                case Error.Login:
                    ShowError(msg, tbLogin);
                    tbLogin.Focus();
                    break;
                case Error.Password:
                    ShowError(msg, tbPassword);
                    tbPassword.Focus();
                    break;
                case Error.Generic:
                default:
                    ShowError(msg, btnConnect);
                    btnConnect.Focus();
                    break;
            }
        }

        private void ShowError(string msg, Control c)
        {
         /*   if (btnConnect.ToolTip())
                ttAlert.IsOpen = false;
            ttText.Text = msg;
            ttAlert.Content = ttText.Text;*/
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
     //       AppController.Instance.Connect();
        }

        private void OnEnter(KeyEventArgs e)
        {
      /*      if (e.Key != Key.Return)
                return;
            e.Handled = true;
            if (tbHost.Text.Length > 0 && tbLogin.Text.Length > 0 && tbPassword.Password.Length > 0)
                AppController.Instance.Connect();*/
        }

        private void tbHost_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }

        private void tbLogin_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }
    }
}

