using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal sealed class ConversationTab : TabPage
    {
        public RichTextBoxEx HistoryBox { get; private set; }

        public ConversationTab(string username)
        {
            HistoryBox = new RichTextBoxEx();
            HistoryBox.SuspendLayout();
            SuspendLayout();
            HistoryBox.ReadOnly = true;
            HistoryBox.BorderStyle = BorderStyle.None;
            HistoryBox.BackColor = SystemColors.Window;
            HistoryBox.Dock = DockStyle.Fill;
            HistoryBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            Text = username;
            Controls.Add(HistoryBox);
            HistoryBox.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
