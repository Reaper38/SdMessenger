using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;

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
            Client = new Client(OnMessage);
            mainDialog = new MainDialog();
            loginDialog = new LoginDialog();
            MainForm = mainDialog;
            MainForm.Show();
        }

        private void OnMessage(IMessage msg)
        {
            // XXX: handle messages
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (Client.ConnectionState != ConnectionState.Disconnected)
            {
                Client.Update();
            }
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
                {
                    loginDialog.Hide();
                    var request = new ClUserlistRequest();
                    Client.Send(request);
                }
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
                address = NetUtil.GetHostByName(spl[0], AddressFamily.InterNetwork);
                if (address.Equals(IPAddress.None))
                {
                    errType = LoginDialog.Error.Host;
                    errMsg = "Can't resolve host name";
                    break;
                }
                if (!NetUtil.ValidateLogin(ref login, out errMsg))
                {
                    errType = LoginDialog.Error.Login;
                    break;
                }
                if (!NetUtil.ValidatePassword(ref pass, out errMsg))
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
