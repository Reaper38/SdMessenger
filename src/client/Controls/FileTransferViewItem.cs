using System;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    internal partial class FileTransferViewItem : UserControl
    {
        private FileTransferState state = FileTransferState.Waiting;
        private FileSizeUnit unit = FileSizeUnit.KB;
        private long bytesTotal, bytesDone;
        private const string DecFmt = "0.##";

        public event EventHandler Accept;
        public event EventHandler Decline;
        public event EventHandler Cancel;
        public event EventHandler Open;
        public event EventHandler ShowInFolder;

        public FileTransferViewItem(string fileName, long fileSize, FileTransferDirection direction)
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Margin = new Padding(1, 1, 1, 0);
            Direction = direction;
            FileName = fileName;
            bytesDone = 0;
            bytesTotal = fileSize;
            UpdateBytesDoneText();
            UpdateBytesTotalText();
            UpdateProgressBar();
            ApplyState();
        }

        // for designer support
        private FileTransferViewItem() :
            this("file_name.ext", 42 * 1024, FileTransferDirection.In)
        {}

        private void OnAccept()
        {
            if (Accept != null)
                Accept(this, EventArgs.Empty);
        }

        private void OnDecline()
        {
            if (Decline != null)
                Decline(this, EventArgs.Empty);
        }

        private void OnCancel()
        {
            if (Cancel != null)
                Cancel(this, EventArgs.Empty);
        }

        private void OnOpen()
        {
            if (Open != null)
                Open(this, EventArgs.Empty);
        }

        private void OnShowInFolder()
        {
            if (ShowInFolder != null)
                ShowInFolder(this, EventArgs.Empty);
        }

        private void UpdateProgressBar()
        {
            var min = pbProgress.Minimum;
            var max = pbProgress.Maximum;
            var rh = max - min;
            var ph = bytesTotal;
            var ratio = ph == 0 ? 0 : rh * 1M / ph; // bar / real_size
            pbProgress.Value = min + (int)Math.Floor(bytesDone * ratio);
        }

        private void UpdateBytesTotalText()
        { lTotal.Text = ((decimal)bytesTotal / unit).ToString(DecFmt); }

        private void UpdateBytesDoneText()
        { lDone.Text = ((decimal)bytesDone / unit).ToString(DecFmt); }

        private void ApplyState()
        {
            switch (Direction)
            {
            case FileTransferDirection.In:
                switch (state)
                {
                case FileTransferState.Waiting: // [save as...] [accept]
                    btn1.Visible = true;
                    btn1.Text = "Cancel";
                    btn2.Visible = true;
                    btn2.Text = "Save as...";
                    pbProgress.Visible = true;
                    pbProgress.Style = ProgressBarStyle.Marquee;
                    lStatus.Visible = false;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = true;
                    lSizeUnit.Visible = true;
                    break;
                case FileTransferState.Working: // <hidden> [cancel]
                    btn1.Visible = false;
                    btn2.Visible = true;
                    btn2.Text = "Cancel";
                    pbProgress.Visible = true;
                    pbProgress.Style = ProgressBarStyle.Continuous;
                    lStatus.Visible = false;
                    lDone.Visible = true;
                    lSlash.Visible = true;
                    lTotal.Visible = true;
                    lSizeUnit.Visible = true;
                    break;
                case FileTransferState.Success: // [open] [show in folder]
                    btn1.Visible = true;
                    btn1.Text = "Open";
                    btn2.Visible = true;
                    btn2.Text = "Show";
                    pbProgress.Visible = false;
                    lStatus.Text = "File received.";
                    lStatus.Visible = true;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = false;
                    lSizeUnit.Visible = false;
                    break;
                case FileTransferState.Failure: // <hidden> <hidden>
                    btn1.Visible = false;
                    btn2.Visible = false;
                    pbProgress.Visible = false;
                    lStatus.Text = ErrorMessage;
                    lStatus.Visible = true;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = false;
                    lSizeUnit.Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
                }
                break;
            case FileTransferDirection.Out:
                switch (state)
                {
                case FileTransferState.Waiting: // <hidden> [cancel]
                    btn1.Visible = false;
                    btn2.Visible = true;
                    btn2.Text = "Cancel";
                    pbProgress.Visible = true;
                    pbProgress.Style = ProgressBarStyle.Marquee;
                    lStatus.Text = "Waiting";
                    lStatus.Visible = true;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = false;
                    lSizeUnit.Visible = false;
                    break;
                case FileTransferState.Working: // <hidden> [cancel]
                    btn1.Visible = false;
                    btn2.Visible = true;
                    btn2.Text = "Cancel";
                    pbProgress.Visible = true;
                    pbProgress.Style = ProgressBarStyle.Continuous;
                    lStatus.Visible = false;
                    lDone.Visible = true;
                    lSlash.Visible = true;
                    lTotal.Visible = true;
                    lSizeUnit.Visible = true;
                    break;
                case FileTransferState.Success: // <hidden> <hidden>
                    btn1.Visible = false;
                    btn2.Visible = false;
                    pbProgress.Visible = false;
                    lStatus.Text = "File sent.";
                    lStatus.Visible = true;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = false;
                    lSizeUnit.Visible = false;
                    break;
                case FileTransferState.Failure: // <hidden> <hidden>
                    btn1.Visible = false;
                    btn2.Visible = false;
                    pbProgress.Visible = false;
                    lStatus.Text = ErrorMessage;
                    lStatus.Visible = true;
                    lDone.Visible = false;
                    lSlash.Visible = false;
                    lTotal.Visible = false;
                    lSizeUnit.Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        public string FileName
        {
            get { return tbFileName.Text; }
            set { tbFileName.Text = value; }
        }
        
        public FileSizeUnit Unit
        {
            get { return unit; }
            set
            {
                if (unit == value)
                    return;
                unit = value;
                UpdateBytesTotalText();
                UpdateBytesDoneText();
                lSizeUnit.Text = unit.ToString();
            }
        }

        public long BytesTotal
        {
            get { return bytesTotal; }
            set
            {
                if (value == bytesTotal)
                    return;
                if (value < bytesDone || value < 0)
                    throw new ArgumentOutOfRangeException();
                bytesTotal = value;
                UpdateBytesTotalText();
                UpdateProgressBar();
            }
        }

        public long BytesDone
        {
            get { return bytesDone; }
            set
            {
                if (bytesDone == value)
                    return;
                if (value > bytesTotal || value < 0)
                    throw new ArgumentOutOfRangeException();
                bytesDone = value;
                UpdateBytesDoneText();
                UpdateProgressBar();
            }
        }

        public string ErrorMessage { get; set; }

        public FileTransferDirection Direction { get; private set; }

        public FileTransferState State
        {
            get { return state; }
            set
            {
                if (value == state)
                    return;
                state = value;
                ApplyState();
            }
        }

        /*
         *   state  |  btn1   |  btn2  |
         * in:
         *  waiting: [decline] [accept]
         *  working: < hidden> [cancel]
         *  success: [  open ] [ show ]
         *  failure: < hidden> <hidden>
         * out:
         *  waiting: < hidden> [cancel]
         *  working: < hidden> [cancel]
         *  success: < hidden> <hidden>
         *  failure: < hidden> <hidden>
         */

        private void btn1_Click(object sender, EventArgs e)
        {
            switch (Direction)
            {
            case FileTransferDirection.In:
                switch (state)
                {
                case FileTransferState.Waiting: 
                    OnDecline();
                    break;
                case FileTransferState.Success:
                    OnOpen();
                    break;
                }
                break;
            }
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            switch (Direction)
            {
            case FileTransferDirection.In:
                switch (state)
                {
                case FileTransferState.Waiting:
                    OnAccept();
                    break;
                case FileTransferState.Working:
                    OnCancel();
                    break;
                case FileTransferState.Success:
                    OnShowInFolder();
                    break;
                }
                break;
            case FileTransferDirection.Out:
                switch (state)
                {
                case FileTransferState.Waiting:
                    OnCancel();
                    break;
                case FileTransferState.Working:
                    OnCancel();
                    break;
                }
                break;
            }
        }
    }

    internal enum FileTransferDirection
    {
        In,
        Out,
    }

    internal enum FileTransferState
    {
        Waiting,
        Working,
        Success,
        Failure,
    }

    internal struct FileSizeUnit
    {
        private readonly int value;

        private FileSizeUnit(int value) { this.value = value; }
        
        public static readonly FileSizeUnit B = new FileSizeUnit(1);
        public static readonly FileSizeUnit KB = new FileSizeUnit(1024);
        public static readonly FileSizeUnit MB = new FileSizeUnit(1024 * 1024);
        public static readonly FileSizeUnit GB = new FileSizeUnit(1024 * 1024 * 1024);

        public static implicit operator int(FileSizeUnit su) { return su.value; }

        public override string ToString()
        {
            switch (value)
            {
            case 1: return "byte(s)";
            case 1024: return "KB";
            case 1024 * 1024: return "MB";
            case 1024 * 1024 * 1024: return "GB";
            default: return "";
            }
        }
    }
}
