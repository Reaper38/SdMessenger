using System;
using System.Text;
using System.Windows.Forms;
using Sdm.Client.Util;

namespace Sdm.Client.Controls
{
    internal partial class ConversationTab : UserControl
    {
        public ConversationTab()
        { InitializeComponent(); }

        public event SendMessageHandler SendMessage;
        public event SendMessageHandler SendFile;

        private void OnSendMessage()
        {
            if (SendMessage != null)
                SendMessage();
        }

        private void OnSendFile()
        {
            if (SendFile != null)
                SendFile();
        }
        
        public void AddMessage(DateTime dt, MsgType type, string from, string msg)
        {
            string col;
            switch (type)
            {
            case MsgType.Incoming:
                col = @"{\colortbl ;\red192\green80\blue77;}"; // Color.FromArgb(192, 080, 077);
                break;
            case MsgType.Outcoming:
                col = @"{\colortbl ;\red79\green129\blue189;}"; // Color.FromArgb(079, 129, 189);
                break;
            case MsgType.System:
                col = @"{\colortbl ;\red135\green157\blue059;}"; // Color.FromArgb(135, 157, 059);
                break;
            default:
                col = ""; //Color.Gray;
                break;
            }
            var sdt = dt.ToString("[HH:mm:ss]"); // [HH:mm:ss dd/MM/yy]
            var sb = new StringBuilder();
            sb.Append(@"{\rtf\ansi ");
            sb.Append(col);
            sb.Append(@"{\b");
            {
                sb.Append(@"{\cf1");
                {
                    RtfUtil.EscapeString(sb, from);
                    sb.Append(' ');
                    RtfUtil.EscapeString(sb, sdt);
                }
                sb.Append('}');
            }
            sb.Append(@"}\line ");
            RtfUtil.EscapeString(sb, msg);
            sb.Append(@"\line\line}");
            var scrollPos = rtbHistory.GetScrollPosition().Y + rtbHistory.ClientSize.Height;
            var maxScrollPos = rtbHistory.GetMaxScrollPosition();
            rtbHistory.AppendRtf(sb.ToString());
            if (scrollPos >= maxScrollPos)
                rtbHistory.ScrollToEnd();
        }
        
        public void ClearHistory()
        { rtbHistory.Clear(); }

        public void ClearMessage()
        { tbNewMsg.Clear(); }
        
        public int MessageLength { get { return tbNewMsg.TextLength; } }

        public string MessageText { get { return tbNewMsg.Text; } }

        public string[] Attachments { get; private set; }

        public void ClearAttachments() { Attachments = null; }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Multiselect = true;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                Attachments = ofd.FileNames;
                OnSendFile();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        { OnSendMessage(); }

        private void tbNewMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && !e.Shift)
            {
                OnSendMessage();
                e.SuppressKeyPress = true;
            }
        }

        private void tbNewMsg_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void tbNewMsg_DragDrop(object sender, DragEventArgs e)
        {
            Attachments = (string[])e.Data.GetData(DataFormats.FileDrop);
            OnSendFile();
        }
    }

    internal delegate void SendMessageHandler();

    internal enum MsgType
    {
        Outcoming,
        Incoming,
        System
    }
}
