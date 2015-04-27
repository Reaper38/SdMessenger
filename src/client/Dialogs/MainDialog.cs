using System;
using System.Drawing;
using System.Windows.Forms;
using Sdm.Client.Controls;
using Sdm.Core;
using Sdm.Core.Messages;

namespace Sdm.Client
{
    internal partial class MainDialog : FormEx
    {
        private AppController Controller { get { return AppController.Instance; } }

        public MainDialog()
        {
            InitializeComponent();
            ApplyConnectionState(ConnectionState.Disconnected);
        }

        private void TrySendMessage()
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            if (tbNewMsg.TextLength == 0)
                return;
            // XXX: send message
            tbNewMsg.Clear();
        }

        private void OpenConversation(string username)
        {
            // XXX: ensure that corresponding tab exists
            // load history if needed
            // select tab
        }

        private void btnSend_Click(object sender, EventArgs e)
        { TrySendMessage(); }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            // XXX: show OpenFile dialog
        }

        private void btnSrv_Click(object sender, EventArgs e)
        {
            // XXX: show context menu (connect/disconnect)
            cmConnection.Show(btnSrv, new Point(0, btnSrv.Height));
        }

        private void lvUsers_DoubleClick(object sender, EventArgs e)
        {
            // XXX: get selected user and show conversation tab
        }

        private void tbNewMsg_KeyDown(object sender, KeyEventArgs e)
        {
            // XXX: skip if shift+return
            if (e.KeyCode == Keys.Return)
                TrySendMessage();
        }
        
        private void cmiLogin_Click(object sender, EventArgs e)
        {
            // XXX: show login window or logout
            if (Controller.State == ConnectionState.Disconnected)
                Controller.ShowLoginDialog();
            else
                Controller.Disconnect();
        }

        private void ClearUserList()
        { lvUsers.Items.Clear(); }

        public void UpdateUserList(SvUserlistRespond msg)
        {
            ClearUserList();
            foreach (var username in msg.Usernames)
                lvUsers.Items.Add(username);
        }

        public void ApplyConnectionState(ConnectionState newState)
        {
            switch (newState)
            {
            case ConnectionState.Disconnected:
                tbHost.Text = "Disconnected";
                cmiLogin.Text = "Connect";
                ClearUserList();
                break;
            case ConnectionState.Waiting:
                tbHost.Text = "Waiting...";
                cmiLogin.Text = "Disconnect";
                break;
            case ConnectionState.Connected:
                tbHost.Text = String.Format("{0}:{1}", Controller.Host, Controller.Port);
                cmiLogin.Text = "Disconnect";
                break;
            }
        }
    }
}
