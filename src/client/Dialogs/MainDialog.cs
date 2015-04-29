using System;
using System.Collections.Generic;
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
        private Dictionary<string, ConversationTab> convs;

        public MainDialog()
        {
            InitializeComponent();
            convs = new Dictionary<string, ConversationTab>();
            ApplyConnectionState(ConnectionState.Disconnected);
        }

        private void TrySendMessage()
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            if (tbNewMsg.TextLength == 0)
                return;
            var username = tabConversations.SelectedTab.Text;
            if (Controller.SendMessage(username, tbNewMsg.Text))
                tbNewMsg.Clear();
        }

        private ConversationTab GetConversation(string username)
        {
            if (!convs.ContainsKey(username))
            {
                var tab = new ConversationTab(username);
                convs.Add(username, tab);
                tabConversations.TabPages.Add(convs[username]);
                return tab;
            }
            return convs[username];
        }

        private void OpenConversation(string username)
        {
            var conv = GetConversation(username);
            tabConversations.SelectTab(conv);
        }

        private void btnSend_Click(object sender, EventArgs e)
        { TrySendMessage(); }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            // XXX: show OpenFile dialog
        }

        private void btnSrv_Click(object sender, EventArgs e)
        { cmConnection.Show(btnSrv, new Point(0, btnSrv.Height)); }

        private void lvUsers_DoubleClick(object sender, EventArgs e)
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            var username = lvUsers.SelectedItems[0].SubItems[0].Text;
            if (username == Controller.Config.Login)
                return;
            OpenConversation(username);
        }

        private void tbNewMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !e.Shift)
            {
                TrySendMessage();
                e.SuppressKeyPress = true;
            }
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

        public void AddMessage(string username, string message, MsgType type)
        {
            var conv = GetConversation(username);
            conv.AddMessage(DateTime.Now, type, username, message);
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
                tbHost.Text = String.Format("{0}:{1}", Controller.Config.Host, Controller.Config.Port);
                cmiLogin.Text = "Disconnect";
                break;
            }
        }
    }
}
