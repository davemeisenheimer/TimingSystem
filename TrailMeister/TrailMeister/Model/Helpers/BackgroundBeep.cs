using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrailMeister.Model.Helpers
{
    public class BackgroundBeep
    {
        static Thread _beepThread;
        static AutoResetEvent _signalBeep;

        static BackgroundBeep() 
        {
            _signalBeep = new AutoResetEvent(false);
            _beepThread = new Thread(() =>
            {
                for (; ; )
                {
                    _signalBeep.WaitOne();
                    Console.Beep(800, 300);
                }
            }, 1)
            {
                IsBackground = true
            };
            _beepThread.Start();
        }

        public static void Beep(int freq)
        {
            _signalBeep.Set();
        }

        public static void Beep()
        {
            _signalBeep.Set();
        }
    }
}
