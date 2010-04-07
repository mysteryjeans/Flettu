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
        public Thread WorkerThread { get; private set; }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            this.WorkerThread = Thread.CurrentThread;
            base.OnDoWork(e);
        }

        public void Interrupt()
        {
            if (this.WorkerThread != null)
                this.WorkerThread.Interrupt();
        }

        public void Join()
        {
            if (this.WorkerThread != null)
                this.WorkerThread.Join();
        }

        public void Join(int milisecondsTimeout)
        {
            if (this.WorkerThread != null)
                this.WorkerThread.Join(milisecondsTimeout);
        }    

        public void Stop()
        {
            if (this.IsBusy)
                this.WorkerThread.Abort();
        }
    }
}
