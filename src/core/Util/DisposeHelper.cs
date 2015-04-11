using System;
using System.Diagnostics;

namespace Sdm.Core.Util
{
    public static class DisposeHelper
    {
        [Conditional("DEBUG")]
        public static void OnDispose<T>(bool disposing)
        {
            if (disposing)
                return;
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
                Debug.Fail("Non-disposed object finalization: " + typeof(T).FullName);
        }
    }
}
