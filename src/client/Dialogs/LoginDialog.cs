using System;
using System.Windows.Forms;
using Sdm.Client.Controls;
using Sdm.Core;

namespace Sdm.Client
{
    internal partial class LoginDialog : FormEx
    {
        internal enum Error
        {
            Host,
            Login,
            Password,
            Generic
        }

        private const int TooltipDuration = 2000;

        public LoginDialog()
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
            get { return tbPassword.Text; }
            set { tbPassword.Text = value; }
        }

        public bool SavePassword
        {
            get { return chkSavePass.Checked; }
            set { chkSavePass.Checked = value; }
        }
        
        public void EnableControls(bool enable)
        {
            lHost.Enabled = enable;
            tbHost.Enabled = enable;
            lLogin.Enabled = enable;
            tbLogin.Enabled = enable;
            lPassword.Enabled = enable;
            tbPassword.Enabled = enable;
            chkSavePass.Enabled = enable;
            btnConnect.Enabled = enable;
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
            if (ttAlert.Active)
                ttAlert.Hide(this);
            ttAlert.Show(msg, this, c.Location.X + 64, c.Location.Y - c.Height, TooltipDuration);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            AppController.Instance.Connect();
        }

        private void OnEnter(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
                return;
            e.SuppressKeyPress = true;
            if (tbHost.TextLength > 0 && tbLogin.TextLength > 0 && tbPassword.TextLength > 0)
                AppController.Instance.Connect();
        }

        private void tbHost_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }

        private void tbLogin_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e) { OnEnter(e); }
    }
}
