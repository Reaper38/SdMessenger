using Sdm.ClientWPF.Util;
using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace Sdm.ClientWPF.Pages
{
    /// <summary>
    /// Interaction logic for ConversationPage.xaml
    /// </summary>
    public partial class ConversationPage : Page
    {
        ConversationPage()
        {
            InitializeComponent();
        }

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
            rtbHistory.Document.Blocks.Add(new Paragraph(new Run(sb.ToString())));
            if (rtbHistory.VerticalOffset + rtbHistory.ViewportHeight == rtbHistory.ExtentHeight)
                    rtbHistory.ScrollToEnd();
        }

        public void ClearHistory()
        { rtbHistory.Document.Blocks.Clear(); }

        public void ClearMessage()
        { rtbNewMsg.Document.Blocks.Clear(); }

        public int MessageLength { get { return rtbNewMsg.Document.Blocks.Count; } }

        public string MessageText { get { return new TextRange(rtbNewMsg.Document.ContentStart, rtbNewMsg.Document.ContentEnd).Text; } }

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
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void tbNewMsg_DragDrop(object sender, DragEventArgs e)
        {
            Attachments = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            OnSendFile();
        }
    }

    public delegate void SendMessageHandler();

    public enum MsgType
    {
        Outcoming,
        Incoming,
        System
    }
}
