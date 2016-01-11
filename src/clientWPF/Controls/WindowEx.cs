using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Sdm.ClientWPF.Controls
{
    public partial class WindowEx : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private readonly ConcurrentQueue<Action> messageQueue = new ConcurrentQueue<Action>();
        private IntPtr handle;

        /// <summary>
        ///     Gets a value indicating whether the window is active.
        /// </summary>
        public bool isActive { get; private set; }

        protected override void OnSourceInitialized(EventArgs e)
        {            
            handle = new WindowInteropHelper(this).Handle;
            HwndSource src = HwndSource.FromHwnd(handle);
            src.AddHook(new HwndSourceHook(WndProc));
            base.OnSourceInitialized(e);
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const uint WM_NCACTIVATE = 0x0086;
            switch ((uint)msg)
            {
                case WM_NCACTIVATE:
                    isActive = wParam != IntPtr.Zero;
                    break;
            }
            Action callback;
            while (messageQueue.Count > 0 && messageQueue.TryDequeue(out callback))
                callback();
            return IntPtr.Zero;
        }

        /// <summary>
        /// Same as Invoke, but closes created handle.
        /// </summary>
        public void InvokeSync(Action callback)
        {
            var result = Dispatcher.BeginInvoke(callback);
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
