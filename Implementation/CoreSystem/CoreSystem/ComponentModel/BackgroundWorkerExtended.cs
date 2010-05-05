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
        private bool isInterrupted = false;
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
                if (!this.CancellationPending)
                    Monitor.Wait(this, timeout);
            }
        }

        public bool IsInterrupted()
        {
            lock (this)
            {
                bool lastInterrupt = isInterrupted;
                isInterrupted = false;
                return lastInterrupt;
            }
        }

        public void Interrupt()
        {
            //lock (this)
            //{
                isInterrupted = true;
                Monitor.Pulse(this);
            //}
        }
        
        public void Stop()
        {
            this.CancelAsync();
            this.Interrupt();
        }
    }
}
