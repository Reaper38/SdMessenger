using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal class RichTextBoxEx : RichTextBox
    {
        public void AppendRtf(string rtf)
        {
            var s = SelectionStart;
            var l = SelectionLength;
            Select(TextLength, 0);
            SelectedRtf = rtf;
            Select(s, l);
        }
    }
}
