using Sdm.ClientWPF.Pages;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Sdm.ClientWPF
{
    internal sealed class AppController : Application
    {
        private static readonly object SyncRoot = 1;
        private static volatile AppController instance;
        private Client client;
        private Thread updaterThread;
        private volatile bool updaterExit;
        private ManualResetEvent evUpdaterThread;
        private LogPage logPage;
        private bool logPageVisible;
        private LogAdapter logAdapter;
        private MainPage mainPage;
        private MainWindow mainWindow;
        private LoginPage loginPage;
        private readonly object syncUiProxies = 1;
        //      private readonly Dictionary<IFileTransfer, FileTransferUiProxy> uiProxies;
        //      private readonly FileTransferManager ftMgr;
        public ClientConfig Config { get; private set; }
        public ConnectionState State { get { return client.ConnectionState; } }
        public string Login { get { return Config.Login; } }
        public static AppController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                            instance = new AppController();
                    }
                }
                return instance;
            }
        }

        private AppController()
        {
            Config = new ClientConfig();
            client = new Client(Config);
            client.ConnectionStateChanged += ClientConnectionStateChanged;
            client.UserMessage += OnMessage;
          /*  ftMgr = new FileTransferManager(client, 8 * 1024);
            ftMgr.TransferRequestReceived += FileTransferRequestReceived;
            ftMgr.TransferStateChanged += FileTransferStateChanged;
            ftMgr.DataSent += FileTransferDataSent;
            ftMgr.DataReceived += FileTransferDataReceived;*/
            logPage = new LogPage();
          //  logAdapter = new LogAdapter(logPage.Console);
            mainPage = new MainPage();
            loginPage = new LoginPage();
          //  uiProxies = new Dictionary<IFileTransfer, FileTransferUiProxy>();
            mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.frMain.Navigate(loginPage);
            mainWindow.Height = 300;
            mainWindow.Width = 200;
            mainWindow.Show();
            evUpdaterThread = new ManualResetEvent(false);
            updaterThread = new Thread(UpdateProc)
            {
                Name = "AppController updater",
                IsBackground = true,
            };
            updaterThread.Start();
        }

        private void OnMessage(IMessage msg)
        {
            switch (msg.Id)
            {
                case MessageId.SvUserlistRespond:
                    OnSvUserlistRespond(msg as SvUserlistRespond);
                    break;
                case MessageId.SvUserlistUpdate:
                    OnSvUserlistUpdate(msg as SvUserlistUpdate);
                    break;
                case MessageId.CsChatMessage:
                    OnCsChatMessage(msg as CsChatMessage);
                    break;
    /*            case MessageId.SvFileTransferRequest:
                case MessageId.SvFileTransferResult:
                case MessageId.CsFileTransferData:
                case MessageId.CsFileTransferVerificationResult:
                case MessageId.CsFileTransferInterruption:
                    ftMgr.OnMessage(msg);*/
                    break;
            }
        }

        private void OnSvUserlistRespond(SvUserlistRespond msg)
        {
            mainWindow.InvokeAsync(() =>
            {
                mainPage.UpdateUserList(msg);
            });
        }

        private void OnSvUserlistUpdate(SvUserlistUpdate msg)
        {
            mainWindow.InvokeAsync(() =>
            {
                mainPage.UpdateUserList(msg);
            });
        }

        private void OnCsChatMessage(CsChatMessage msg)
        {
            mainWindow.InvokeAsync(() =>
            {
                mainPage.AddMessage(msg.Username, msg.Username, msg.Message);
            });
        }

        private void UpdateProc()
        {
            while (!updaterExit)
            {
                evUpdaterThread.WaitOne();
                // XXX: detect connection loss and change connection state
                if (client.ConnectionState != ConnectionState.Disconnected)
                {
                    client.Update();
                    //FTPLUG: Fix me
                   // ftMgr.Update();
                }
                Thread.Sleep(Config.UpdateSleep);
            }
        }

        private void OnClientConnectionResult(ConnectionResult cr, string msg)
        {
            if (cr == ConnectionResult.Rejected)
            {
                client.ConnectionResult -= OnClientConnectionResult;
                client.AuthResult -= OnClientAuthResult;
                mainWindow.InvokeAsync(() =>
                {
                    loginPage.EnableControls(true);
                    loginPage.ShowError(LoginPage.Error.Generic, msg);
                });
            }
        }

        private void OnClientAuthResult(AuthResult ar, string msg)
        {
            client.ConnectionResult -= OnClientConnectionResult;
            client.AuthResult -= OnClientAuthResult;
            mainWindow.InvokeAsync(() =>
            {
                loginPage.EnableControls(true);
                if (ar != AuthResult.Accepted)
                {
                    var desc = ar.GetDescription();
                    msg = msg == "" ? desc : String.Format("{0}: {1}", desc, msg);
                    var err = ar == AuthResult.InvalidLogin ?
                        LoginPage.Error.Password : LoginPage.Error.Generic;
                    loginPage.ShowError(err, msg);
                }
                else
                {
                    mainWindow.frMain.Navigate(mainPage);
                    var request = new ClUserlistRequest();
                    client.Send(request);
                }
            });
        }

        private void ClientConnectionStateChanged()
        {
            if (client.ConnectionState == ConnectionState.Disconnected)
            {
                evUpdaterThread.Reset();
                //FTPLUG: fix me
                //ClearFileTransfers();
                mainWindow.Hide();
            }
            mainWindow.InvokeAsync(() => mainPage.ApplyConnectionState(client.ConnectionState));
        }

        public void Connect()
        {
            var hostPort = loginPage.Host.Trim();
            var address = IPAddress.None;
            ushort port = 0;
            var login = loginPage.Login.Trim();
            var pass = loginPage.Password.Trim();
            var errMsg = "";
            LoginPage.Error errType;
            do
            {
                if (hostPort == "")
                {
                    errType = LoginPage.Error.Host;
                    errMsg = "Enter valid server address";
                    break;
                }
                var spl = hostPort.Split(':');
                if (spl.Length != 2)
                {
                    errType = LoginPage.Error.Host;
                    errMsg = "Enter valid server address";
                    break;
                }
                if (!UInt16.TryParse(spl[1], out port))
                {
                    errType = LoginPage.Error.Host;
                    errMsg = "Enter valid port";
                    break;
                }
                address = NetUtil.GetHostByName(spl[0], AddressFamily.InterNetwork);
                if (address.Equals(IPAddress.None))
                {
                    errType = LoginPage.Error.Host;
                    errMsg = "Can't resolve host name";
                    break;
                }
                if (!NetUtil.ValidateLogin(ref login, out errMsg))
                {
                    errType = LoginPage.Error.Login;
                    break;
                }
                if (!NetUtil.ValidatePassword(ref pass, out errMsg))
                {
                    errType = LoginPage.Error.Password;
                    break;
                }
                loginPage.EnableControls(false);
                Config.Host = spl[0];
                Config.Port = port;
                Config.Remember = loginPage.SavePassword;
                if (Config.Remember)
                {
                    Config.Login = login;
                    Config.Password = pass;
                }
                client.ConnectionResult += OnClientConnectionResult;
                client.AuthResult += OnClientAuthResult;
                client.Connect(address, port, login, pass);
                evUpdaterThread.Set();
                return;
            } while (false);
            loginPage.ShowError(errType, errMsg);
        }

        public void ShowLoginPage()
        {
            if (Config.Host != "")
                loginPage.Host = String.Format("{0}:{1}", Config.Host, Config.Port);
            loginPage.SavePassword = Config.Remember;
            if (Config.Remember)
            {
                loginPage.Login = Config.Login;
                loginPage.Password = Config.Password;
            }
            mainWindow.frMain.Navigate(loginPage);
        }

        public bool SendMessage(string username, string message)
        {
            var msg = new CsChatMessage { Username = username, Message = message };
            if (client.Send(msg))
            {
                mainPage.AddMessage(username, Config.Login, message);
                return true;
            }
            return false;
        }

        public bool LogWindowVisible
        {
            get { return logPageVisible; }
            set { logPageVisible = value; }
        }

        public bool FileTransferWindowVisible
        {
            get { return mainWindow.IsVisible; }
            set { if (value)
                    mainWindow.Show();
                else
                    mainWindow.Hide();
                }
        }

    /*    private void FileTransferStateChanged(IFileTransfer ft)
        {
            FileTransferUiProxy proxy;
            lock (syncUiProxies)
            {
                if (!uiProxies.TryGetValue(ft, out proxy))
                {
                    Root.Log(LogLevel.Error, "AppController: skipping unknown file transfer session [sid={0}]",
                        ft.Id);
                    return;
                }
            }
            mainWindow.InvokeAsync(() =>
            {
                proxy.View.ErrorMessage = ft.ErrorMessage;
                proxy.View.State = ft.State;
            });
        }

        private void FileTransferRequestReceived(IIncomingFileTransfer ft)
        {
            mainWindow.InvokeAsync(() =>
            {
                var fileDesc = String.Format("\"{0}\" ({1} bytes)", ft.Name, ft.BytesTotal);
                mainPage.AddSystemMessage(ft.Sender, "Incoming file transfer", fileDesc);
                var proxy = new FileTransferUiProxy(ft);
                lock (syncUiProxies)
                {
                    uiProxies.Add(proxy.Desc, proxy);
                }
                filePage.View.Items.Add(proxy.View);
                filePage.Show();
            });
        }

        private void FileTransferDataSent(IOutcomingFileTransfer ft)
        { UpdateFileTransferView(ft); }

        private void FileTransferDataReceived(IIncomingFileTransfer ft)
        { UpdateFileTransferView(ft); }

        private void UpdateFileTransferView(IFileTransfer ft)
        {
            lock (syncUiProxies)
            {
                FileTransferUiProxy proxy;
                if (!uiProxies.TryGetValue(ft, out proxy))
                {
                    Root.Log(LogLevel.Error, "AppController: skipping unknown file transfer session [sid={0}]",
                        ft.Id);
                    return;
                }
                if (proxy.ViewUpdateRequired || ft.BytesDone == ft.BytesTotal)
                {
                    mainWindow.InvokeAsync(() =>
                    {
                        proxy.UpdateView();
                    });
                }
            }
        }

        private static void OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                var msg = String.Format("File not found:\r\n'{0}'", path);
                MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Process.Start(path);
        }

        private static void ShowFile(string path)
        {
            // http://stackoverflow.com/questions/13680415/how-to-open-explorer-with-a-specific-file-selected
            Process.Start("explorer.exe", String.Format("/select,\"{0}\"", path));
        }
        private class FileTransferUiProxy
        {
            public FileTransferViewItem View { get; private set; }
            public IFileTransfer Desc { get; private set; }
            private const long UpdateDelta = 1000; // ms
            private DateTime lastUpdate;
            private bool whBound = false;

            public FileTransferUiProxy(IFileTransfer ft)
            {
                View = new FileTransferViewItem(Path.GetFileName(ft.Name), ft.BytesTotal, ft.Direction);
                Desc = ft;
                switch (ft.Direction)
                {
                    case FileTransferDirection.In:
                        // waiting
                        View.Accept += ViewAccept;
                        View.Decline += ViewDecline;
                        whBound = true;
                        // receiving
                        View.Cancel += ViewCancel;
                        // received
                        View.Open += ViewOpen;
                        View.ShowInFolder += ViewShowInFolder;
                        View.Conceal += ViewConceal;
                        break;
                    case FileTransferDirection.Out:
                        View.Cancel += ViewCancel;
                        View.Conceal += ViewConceal;
                        break;
                }
                lastUpdate = DateTime.Now;
            }

            public bool ViewUpdateRequired
            {
                get
                {
                    var ts = DateTime.Now - lastUpdate;
                    return ts.TotalMilliseconds >= UpdateDelta;
                }
            }

            public void UpdateView()
            {
                if (Desc.State == FileTransferState.Working)
                    View.BytesDone = Desc.BytesDone;
                lastUpdate = DateTime.Now;
            }

            private void UnbindIncomingWaitHandlers()
            {
                if (!whBound)
                    return;
                whBound = false;
                View.Accept -= ViewAccept;
                View.Decline -= ViewDecline;
            }

            private void ViewAccept(object sender, EventArgs e)
            {
                var ift = Desc as IIncomingFileTransfer;
                if (ift == null)
                    return;
                using (var sfd = new SaveFilePage())
                {
                    sfd.OverwritePrompt = true;
                    sfd.RestoreDirectory = true;
                    sfd.FileName = Path.GetFileName(Desc.Name);
                    sfd.Filter = String.Format("*{0}|", Path.GetExtension(Desc.Name));
                    if (sfd.ShowPage() != PageResult.OK)
                        return;
                    ift.Accept(sfd.FileName);
                    // preemptively set new state to hide decline/accept buttons
                    View.State = FileTransferState.Working;
                    View.FileName = Path.GetFileName(sfd.FileName);
                    UnbindIncomingWaitHandlers();
                }
            }

            private void ViewDecline(object sender, EventArgs e)
            {
                View.State = FileTransferState.Cancelled;
                Desc.Cancel();
                UnbindIncomingWaitHandlers();
            }

            private void ViewCancel(object sender, EventArgs e)
            {
                View.State = FileTransferState.Cancelled;
                Desc.Cancel();
                UnbindIncomingWaitHandlers();
            }

            private void ViewOpen(object sender, EventArgs e)
            { OpenFile(Desc.Name); }

            private void ViewShowInFolder(object sender, EventArgs e)
            { ShowFile(Desc.Name); }

            private void ViewConceal(object sender, EventArgs e)
            { Instance.RemoveFileTransfer(this); }
        }

        private void AddFileTransfer(FileTransferUiProxy proxy)
        {
            lock (syncUiProxies)
            {
                uiProxies.Add(proxy.Desc, proxy);
            }
            filePage.View.Items.Add(proxy.View);
        }

        private void RemoveFileTransfer(FileTransferUiProxy proxy)
        {
            lock (syncUiProxies)
            {
                uiProxies.Remove(proxy.Desc);
                proxy.Desc.Cancel();
            }
            filePage.View.Items.Remove(proxy.View);
        }

        private void ClearFileTransfers()
        {
            lock (syncUiProxies)
            {
                foreach (var kv in uiProxies)
                    filePage.View.Items.Remove(kv.Value.View);
                uiProxies.Clear();
            }
        }

        public void SendFiles(string username, string[] filenames)
        {
            var fileList = new StringBuilder();
            for (int i = 0; i < filenames.Length; i++)
            {
                var filename = filenames[i];
                var ft = ftMgr.Add(username, filename);
                var proxy = new FileTransferUiProxy(ft);
                AddFileTransfer(proxy);
                var fileDesc = String.Format("\"{0}\" ({1} bytes)", Path.GetFileName(ft.Name), ft.BytesTotal);
                fileList.Append(fileDesc);
                if (i != filenames.Length - 1)
                    fileList.AppendLine();
            }
            mainPage.AddSystemMessage(username, "Outcoming file transfer", fileList.ToString());
            filePage.Show();
        }*/

        public void Disconnect() { client.Disconnect(); }
    }
    internal sealed class LogAdapter : xr.ILogger
    {
        private volatile int lineCount;
        private const string DateTimeFormat = "MMM dd HH:mm:ss";

        public LogAdapter(xr.Console console)
        {
            console.AttachLogger(this);
            SdmCore.Logger.MessageLogged += SdmMessageLogged;
        }

        private string LogLevelToColorPrefix(LogLevel lvl)
        {
            switch (lvl)
            {
                case LogLevel.Trace: return "";
                case LogLevel.Debug: return "- ";
                case LogLevel.Info: return "* ";
                case LogLevel.Warning: return "~ ";
                case LogLevel.Error: return "! ";
                case LogLevel.Fatal: return "! ";
                default: return "";
            }
        }

        private void SdmMessageLogged(LogLevel lvl, DateTime time, string msg)
        {
            lineCount++;
            if (MessageLogged == null)
                return;
            var prefix = LogLevelToColorPrefix(lvl);
            var timeString = time.ToString(DateTimeFormat);
            var lvlString = lvl.ToString().ToLower();
            MessageLogged(String.Format("{0}{1} [{2}] {3}", prefix, timeString, lvlString, msg));
        }

        public event Action LogCleared;
        public event Action<string> MessageLogged;

        public void Log(string message) { /* ignore console output */ }
        public int LineCount { get { return lineCount; } }
        public void Clear() { }

        public void Dispose() { }
    }
}
