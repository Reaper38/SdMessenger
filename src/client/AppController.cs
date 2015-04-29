using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using Sdm.Client.Controls;
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
        public ClientConfig Config { get; private set; }
        public ConnectionState State { get { return client.ConnectionState; } }
        public string Login { get { return Config.Login; } }
        public static AppController Instance { get { return instance; } }
        
        private AppController()
        {
            Application.Idle += OnIdle;
            Config = new ClientConfig();
            client = new Client(Config);
            client.ConnectionStateChanged += ClientConnectionStateChanged;
            client.UserMessage += OnMessage;
            mainDialog = new MainDialog();
            loginDialog = new LoginDialog();
            MainForm = mainDialog;
        }

        private void OnMessage(IMessage msg)
        {
            switch (msg.Id)
            {
            case MessageId.SvUserlistRespond:
                mainDialog.UpdateUserList(msg as SvUserlistRespond);
                break;
            case MessageId.SvUserlistUpdate:
                mainDialog.UpdateUserList(msg as SvUserlistUpdate);
                break;
            case MessageId.CsChatMessage:
                OnCsChatMessage(msg as CsChatMessage);
                break;
            }
        }

        private void OnCsChatMessage(CsChatMessage msg)
        { mainDialog.AddMessage(msg.Username, msg.Username, msg.Message); }

        // XXX: update client in separate thread (one can't control OnIdle call frequency)
        private void OnIdle(object sender, EventArgs e)
        {
            // XXX: detect connection loss and change connection state
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
                loginDialog.InvokeAsync(() =>
                {
                    loginDialog.EnableControls(true);
                    loginDialog.ShowError(LoginDialog.Error.Generic, msg);
                });
            }
        }

        private void OnClientAuthResult(AuthResult ar, string msg)
        {
            client.ConnectionResult -= OnClientConnectionResult;
            client.AuthResult -= OnClientAuthResult;
            loginDialog.InvokeAsync(() =>
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
            });
        }

        private void ClientConnectionStateChanged()
        {
            mainDialog.InvokeAsync(() => mainDialog.ApplyConnectionState(client.ConnectionState));
        }

        public void Connect()
        {
            var hostPort = loginDialog.Host.Trim();
            var address = IPAddress.None;
            ushort port = 0;
            var login = loginDialog.Login.Trim();
            var pass = loginDialog.Password.Trim();
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
                Config.Host = spl[0];
                Config.Port = port;
                Config.Remember = loginDialog.SavePassword;
                if (Config.Remember)
                {
                    Config.Login = login;
                    Config.Password = pass;
                }
                client.ConnectionResult += OnClientConnectionResult;
                client.AuthResult += OnClientAuthResult;
                client.Connect(address, port, login, pass);
                return;
            } while (false);
            loginDialog.ShowError(errType, errMsg);
        }

        public void ShowLoginDialog()
        {
            if (Config.Host != "")
                loginDialog.Host = String.Format("{0}:{1}", Config.Host, Config.Port);
            loginDialog.SavePassword = Config.Remember;
            if (Config.Remember)
            {
                loginDialog.Login = Config.Login;
                loginDialog.Password = Config.Password;
            }
            loginDialog.ShowDialog();
        }

        public bool SendMessage(string username, string message)
        {
            var msg = new CsChatMessage {Username = username, Message = message};
            client.Send(msg);
            mainDialog.AddMessage(username, Config.Login, message);
            return true;
        }

        public void Disconnect()
        {
            client.Disconnect();
        }
    }
}
