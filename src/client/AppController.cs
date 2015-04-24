using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Sdm.Core;

namespace Sdm.Client
{
    internal sealed class AppController : ApplicationContext
    {
        private static readonly AppController instance = new AppController();
        private MainDialog mainDialog;
        private LoginDialog loginDialog;

        public static AppController Instance { get { return instance; } }
        public Client Client { get; private set; }
        
        private AppController()
        {
            Application.Idle += OnIdle;
            Client = new Client();
            mainDialog = new MainDialog();
            loginDialog = new LoginDialog();
            MainForm = mainDialog;
            MainForm.Show();
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (Client.ConnectionState != ConnectionState.Disconnected)
            {
                Client.Update();
            }
        }
        
        private IPAddress GetHostByName(string hname, AddressFamily af)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(hname); // XXX: call asynchronously
                foreach (var entry in hostEntry.AddressList)
                {
                    if (entry.AddressFamily == af)
                        return entry;
                }
                return IPAddress.None;
            }
            catch (SocketException)
            {
                return IPAddress.None;
            }
        }

        public static bool ValidateLogin(ref string login, out string msg)
        {
            var tmpLogin = login.Trim().ToLower();
            if (tmpLogin.Length == 0)
            {
                msg = "Login can't be empty";
                return false;
            }
            if (tmpLogin.Length < 2 || tmpLogin.Length > 30)
            {
                msg = "Login should be 2-30 characters long";
                return false;
            }
            foreach (char c in tmpLogin)
            {
                if ('a' <= c && c <= 'z')
                    continue;
                if (Char.IsDigit(c))
                    continue;
                if (c == '.')
                    continue;
                msg = "Login must consist of letters (a-z), numbers and periods";
                return false;
            }
            login = tmpLogin;
            msg = "";
            return true;
        }

        public static bool ValidatePassword(ref string password, out string msg)
        {
            var tmpPass = password.Trim();
            if (tmpPass.Length < 6)
            {
                msg = "Password must have at least 6 characters";
                return false;
            }
            if (tmpPass.Length > 100)
            {
                msg = "Password must have at most 100 characters";
                return false;
            }
            password = tmpPass;
            msg = "";
            return true;
        }

        private void OnClientConnectionResult(ConnectionResult cr, string msg)
        {
            if (cr == ConnectionResult.Rejected)
            {
                Client.ConnectionResult -= OnClientConnectionResult;
                Client.AuthResult -= OnClientAuthResult;
                Action cb = () =>
                {
                    loginDialog.EnableControls(true);
                    loginDialog.ShowError(LoginDialog.Error.Generic, msg);
                };
                if (loginDialog.InvokeRequired)
                    loginDialog.Invoke(cb);
                else
                    cb();
            }
        }

        private void OnClientAuthResult(AuthResult ar, string msg)
        {
            Client.ConnectionResult -= OnClientConnectionResult;
            Client.AuthResult -= OnClientAuthResult;
            // XXX: implement InvokeAsync and use it here
            Action cb = () =>
            {
                loginDialog.EnableControls(true);
                if (ar != AuthResult.Accepted)
                    loginDialog.ShowError(LoginDialog.Error.Generic, msg);
                else
                    loginDialog.Hide();
            };
            if (loginDialog.InvokeRequired)
                loginDialog.Invoke(cb);
            else
                cb();
        }

        public void Login()
        {
            var hostPort = loginDialog.Host.Trim();
            var address = IPAddress.None;
            ushort port = 0;
            var login = loginDialog.Login.Trim();
            var pass = loginDialog.Password.Trim();
            var savePass = loginDialog.SavePassword; // XXX: save pass
            var errMsg = "";
            LoginDialog.Error errType;
            do
            {
                if (hostPort == "")
                {
                    errType = LoginDialog.Error.Host;
                    errMsg = "Enter valid server address";
                    break;
                }
                var spl = hostPort.Split(':');
                if (spl.Length != 2)
                {
                    errType = LoginDialog.Error.Host;
                    errMsg = "Enter valid server address";
                    break;
                }
                if (!UInt16.TryParse(spl[1], out port))
                {
                    errType = LoginDialog.Error.Host;
                    errMsg = "Enter valid port";
                    break;
                }
                address = GetHostByName(spl[0], AddressFamily.InterNetwork);
                if (address.Equals(IPAddress.None))
                {
                    errType = LoginDialog.Error.Host;
                    errMsg = "Can't resolve host name";
                    break;
                }
                if (!ValidateLogin(ref login, out errMsg))
                {
                    errType = LoginDialog.Error.Login;
                    break;
                }
                if (!ValidatePassword(ref pass, out errMsg))
                {
                    errType = LoginDialog.Error.Password;
                    break;
                }
                loginDialog.EnableControls(false);
                Client.ConnectionResult += OnClientConnectionResult;
                Client.AuthResult += OnClientAuthResult;
                Client.Connect(address, port, login, pass);
                return;
            } while (false);
            loginDialog.ShowError(errType, errMsg);
        }

        public void ShowLoginDialog()
        {
            loginDialog.ShowDialog();
        }
    }
}
