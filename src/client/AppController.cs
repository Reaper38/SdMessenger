using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal sealed class AppController : ApplicationContext
    {
        private static readonly AppController instance = new AppController();
        private Client client;
        private MainDialog mainDialog;
        private LoginDialog loginDialog;
        public string Host { get; private set; }
        public ushort Port { get; private set; }
        public ConnectionState State { get { return client.ConnectionState; } }
        public static AppController Instance { get { return instance; } }
        
        private AppController()
        {
            Application.Idle += OnIdle;
            client = new Client();
            client.ConnectionStateChanged += ClientConnectionStateChanged;
            client.UserMessage += OnMessage;
            mainDialog = new MainDialog();
            loginDialog = new LoginDialog();
            MainForm = mainDialog;
            MainForm.Show();
        }

        private void OnMessage(IMessage msg)
        {
            switch (msg.Id)
            {
            case MessageId.SvUserlistRespond:
                mainDialog.UpdateUserList(msg as SvUserlistRespond);
                break;
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (client.ConnectionState != ConnectionState.Disconnected)
            {
                client.Update();
            }
        }
        
        private void OnClientConnectionResult(ConnectionResult cr, string msg)
        {
            if (cr == ConnectionResult.Rejected)
            {
                client.ConnectionResult -= OnClientConnectionResult;
                client.AuthResult -= OnClientAuthResult;
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
            client.ConnectionResult -= OnClientConnectionResult;
            client.AuthResult -= OnClientAuthResult;
            // XXX: implement InvokeAsync and use it here
            Action cb = () =>
            {
                loginDialog.EnableControls(true);
                if (ar != AuthResult.Accepted)
                {
                    var desc = ar.GetDescription();
                    msg = msg == "" ? desc : String.Format("{0}: {1}", desc, msg);
                    loginDialog.ShowError(LoginDialog.Error.Generic, msg);
                }
                else
                {
                    loginDialog.Hide();
                    var request = new ClUserlistRequest();
                    client.Send(request);
                }
            };
            if (loginDialog.InvokeRequired)
                loginDialog.Invoke(cb);
            else
                cb();
        }

        private void ClientConnectionStateChanged()
        {
            Action cb = () => mainDialog.ApplyConnectionState(client.ConnectionState);
            if (mainDialog.InvokeRequired)
                mainDialog.Invoke(cb);
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
                Host = spl[0];
                Port = port;
                client.ConnectionResult += OnClientConnectionResult;
                client.AuthResult += OnClientAuthResult;
                client.Connect(address, port, login, pass);
                return;
            } while (false);
            loginDialog.ShowError(errType, errMsg);
        }

        public void ShowLoginDialog()
        {
            loginDialog.ShowDialog();
        }

        public void Disconnect()
        {
            client.Disconnect();
        }
    }
}
