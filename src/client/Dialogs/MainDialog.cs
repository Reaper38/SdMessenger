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
        private sealed class ConversationDesc
        {
            public TabPage Container { get; private set; }
            public ConversationTab Content { get; private set; }

            public ConversationDesc(string username)
            {
                Container = new TabPage {Text = username, Name = username};
                Content = new ConversationTab();
                Container.Controls.Add(Content);
            }
        }

        private AppController Controller { get { return AppController.Instance; } }

        private Dictionary<string, ConversationDesc> convs;

        public MainDialog()
        {
            InitializeComponent();
            convs = new Dictionary<string, ConversationDesc>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyConnectionState(ConnectionState.Disconnected);
        }
        
        private void TrySendMessage()
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            var tab = tabConvs.SelectedTab;
            if (tab == null)
                return;
            var username = tab.Text;
            var conv = convs[username].Content;
            if (conv.MessageLength == 0)
                return;
            if (Controller.SendMessage(username, conv.MessageText))
                conv.ClearMessage();
        }

        private void TrySendFile()
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            var tab = tabConvs.SelectedTab;
            if (tab == null)
                return;
            var username = tab.Text;
            var conv = convs[username].Content;
            var files = conv.Attachments;
            if (files == null)
                return;
            Controller.SendFiles(username, files);
            conv.ClearAttachments();
        }

        private void ToggleFileTransferWindow(object sender, EventArgs e)
        { Controller.ToggleFileTransferWindow(); }

        private ConversationDesc GetConversation(string username)
        {
            if (!convs.ContainsKey(username))
            {
                var conv = new ConversationDesc(username);
                convs.Add(username, conv);
                tabConvs.TabPages.Add(conv.Container);
                conv.Content.SendMessage += TrySendMessage;
                conv.Content.SendFile += TrySendFile;
                conv.Content.ToggleFileTransferWindow += ToggleFileTransferWindow;
                return conv;
            }
            return convs[username];
        }

        private void OpenConversation(string username)
        {
            var conv = GetConversation(username);
            tabConvs.SelectTab(conv.Container);
        }

        private void lvUsers_DoubleClick(object sender, EventArgs e)
        {
            if (Controller.State != ConnectionState.Connected)
                return;
            var username = lvUsers.SelectedItems[0].SubItems[0].Text;
            if (username == Controller.Config.Login)
                return;
            OpenConversation(username);
        }

        private void miLogin_Click(object sender, EventArgs e)
        {
            if (Controller.State == ConnectionState.Disconnected)
                Controller.ShowLoginDialog();
            else
                Controller.Disconnect();
        }

        private void ClearUserList()
        { lvUsers.Items.Clear(); }

        private ListViewItem CreateUserlistItem(string username)
        { return new ListViewItem { Name = username, Text = username }; }

        public void UpdateUserList(SvUserlistRespond msg)
        {
            ClearUserList();
            foreach (var username in msg.Usernames)
                lvUsers.Items.Add(CreateUserlistItem(username));
        }

        public void UpdateUserList(SvUserlistUpdate msg)
        {
            foreach (var username in msg.Disconnected)
                lvUsers.Items.RemoveByKey(username);
            foreach (var username in msg.Connected)
                lvUsers.Items.Add(CreateUserlistItem(username));
        }
        
        public void AddMessage(string convWith, string sender, string message)
        {
            var conv = GetConversation(convWith);
            var type = convWith == sender ? MsgType.Incoming : MsgType.Outcoming;
            conv.Content.AddMessage(DateTime.Now, type, sender, message);
        }

        public void AddSystemMessage(string convWith, string desc, string message)
        {
            var conv = GetConversation(convWith);
            conv.Content.AddMessage(DateTime.Now, MsgType.System, desc, message);
        }

        private void UpdateHeader(ConnectionState state)
        {
            const string appName = "SdmClient";
            var login = state == ConnectionState.Connected ? Controller.Login : "";
            var s = login == "" ? "" : " - ";
            Text = String.Format("{0}{1}{2}", appName, s, login);
        }

        public void ApplyConnectionState(ConnectionState newState)
        {
            switch (newState)
            {
            case ConnectionState.Disconnected:
                tbHost.Text = "Disconnected";
                miLogin.Text = "Connect";
                ClearUserList();
                UpdateHeader(newState);
                break;
            case ConnectionState.Waiting:
                tbHost.Text = "Waiting...";
                miLogin.Text = "Disconnect";
                break;
            case ConnectionState.Connected:
                tbHost.Text = String.Format("{0}:{1}", Controller.Config.Host, Controller.Config.Port);
                miLogin.Text = "Disconnect";
                UpdateHeader(newState);
                break;
            }
        }
        
        private void miLog_Click(object sender, EventArgs e)
        { Controller.LogWindowVisible = !miLog.Checked; }

        private void miView_Popup(object sender, EventArgs e)
        { miLog.Checked = Controller.LogWindowVisible; }
    }
}
