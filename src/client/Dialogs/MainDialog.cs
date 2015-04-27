using System;
using System.Drawing;
using System.Windows.Forms;
using Sdm.Core.Messages;

namespace Sdm.Client
{
    internal partial class MainDialog : Form
    {
        public MainDialog()
        {
            InitializeComponent();
        }

        private void TrySendMessage()
        {
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
            if (e.KeyCode == Keys.Return)
                TrySendMessage();
        }

        private void cmConnection_Popup(object sender, EventArgs e)
        {
            // XXX: show login/logout according to connection state
        }

        private void cmiLogin_Click(object sender, EventArgs e)
        {
            // XXX: show login window or logout
            AppController.Instance.ShowLoginDialog();
        }

        public void UpdateUserList(SvUserlistRespond msg)
        {
            lvUsers.Items.Clear();
            foreach (var username in msg.Usernames)
                lvUsers.Items.Add(username);
        }
    }
}
