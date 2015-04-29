using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sdm.Client.Util;

namespace Sdm.Client.Controls
{
    internal enum MsgType
    {
        Outcoming,
        Incoming,
        System,
    }

    internal sealed class ConversationTab : TabPage
    {
        private RichTextBoxEx rtbHistory;

        public ConversationTab(string username)
        {
            rtbHistory = new RichTextBoxEx();
            rtbHistory.SuspendLayout();
            SuspendLayout();
            rtbHistory.HideSelection = false;
            rtbHistory.ReadOnly = true;
            rtbHistory.BorderStyle = BorderStyle.None;
            rtbHistory.BackColor = SystemColors.Window;
            rtbHistory.Dock = DockStyle.Fill;
            rtbHistory.ScrollBars = RichTextBoxScrollBars.Vertical;
            Text = username;
            Controls.Add(rtbHistory);
            rtbHistory.ResumeLayout(false);
            ResumeLayout(false);
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
            rtbHistory.AppendRtf(sb.ToString());
        }
    }
}
