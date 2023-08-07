using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Support
{
    /// <inheritdoc/>
    /// <summary>
    /// AccurateTimer
    ///
    /// https://stackoverflow.com/questions/9228313/most-accurate-timer-in-net
    /// 
    /// </summary>
    public class AccurateTimer : IDisposable
    {
        private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        ManualResetEvent resetEvent;
        private int mTimerId;
        private TimerEventDel? mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!

        public AccurateTimer(ManualResetEvent manualResetEvent, int delay)
        {
            resetEvent = manualResetEvent;
            timeBeginPeriod(1);
            mHandler = new TimerEventDel(TimerCallback);
            mTimerId = timeSetEvent(delay, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
        }

        public void Stop()
        {
            mTimerId = 0;
            int err = timeKillEvent(mTimerId);
            timeEndPeriod(1);
            System.Threading.Thread.Sleep(10);// Ensure callbacks are drained
        }

        private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            if (mTimerId != 0)
                resetEvent.Set();
        }

        public void Dispose()
        {
            Stop();
            mHandler = null;
        }
    }
}
