using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sdm.Client.Controls
{
    public class FormEx : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private readonly ConcurrentQueue<Action> messageQueue;
        private IntPtr handle;

        public FormEx() { messageQueue = new ConcurrentQueue<Action>(); }

        /// <summary>
        ///     Gets a value indicating whether the form is active.
        /// </summary>
        public bool IsActive { get; private set; }

        protected override void OnHandleCreated(EventArgs e)
        {
            handle = Handle;
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            const uint WM_NCACTIVATE = 0x0086;
            switch (unchecked((uint)m.Msg))
            {
            case WM_NCACTIVATE:
                IsActive = m.WParam != IntPtr.Zero;
                break;
            }
            Action callback;
            while (messageQueue.Count > 0 && messageQueue.TryDequeue(out callback))
                callback();
            base.WndProc(ref m);
        }

        /// <summary>
        /// Same as Invoke, but closes created handle.
        /// </summary>
        public void InvokeSync(Action callback)
        {
            var result = BeginInvoke(callback);
            using (result.AsyncWaitHandle)
            { EndInvoke(result); }
        }

        /// <summary>
        ///     Adds a callback to the queue to be invoked from WndProc.
        /// </summary>
        public void InvokeAsync(Action callback)
        {
            messageQueue.Enqueue(callback);
            PostMessage(handle, 0, 0, 0);
        }
    }
}
