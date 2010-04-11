using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace CoreSystem.ComponentModel
{
    public class BackgroundWorkerExtended : BackgroundWorker
    {
        public void Sleep(int millisecondTimeout)
        {
            lock (this)
            {
                Monitor.Wait(this, millisecondTimeout);
            }
        }

        public void Sleep(TimeSpan timeout)
        {
            lock (this)
            {
                Monitor.Wait(this, timeout);
            }
        }

        public void Interrupt()
        {
            lock (this)
            {
                Monitor.Pulse(this);
            }
        }
        
        public void Stop()
        {
            this.CancelAsync();
            this.Interrupt();
        }
    }
}
