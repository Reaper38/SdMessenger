using System;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal class TextBoxEx : TextBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Multiline && ShortcutsEnabled && e.Control && !e.Shift)
            {
                switch (e.KeyCode)
                {
                case Keys.Back:
                {
                    e.SuppressKeyPress = true;
                    if (TextLength == 0)
                        break;
                    if (SelectionLength > 0)
                    {
                        SelectedText = "";
                        break;
                    }
                    if (e.Control)
                        SendKeys.SendWait("^+{LEFT}{BACKSPACE}");
                    break;
                }
                }
            }
            base.OnKeyDown(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.A | Keys.Control))
            {
                SelectionStart = 0;
                SelectionLength = Text.Length;
                Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
