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
        private const int WAIT_BEFORE_ABORTING = 10;

        private bool isInterrupted;

        private bool isThreadAborted;

        private Thread workerThread;

        public bool CancellationPending
        {
            get { return base.CancellationPending || isThreadAborted; }
        }

        private bool IsStop
        {
            get { return isThreadAborted || !this.IsBusy; }
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            this.workerThread = Thread.CurrentThread;

            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                isThreadAborted = true;
                Thread.ResetAbort();
            }
        }

        protected override void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (this.IsStop) return;

            base.OnProgressChanged(e);
        }      

        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            base.OnRunWorkerCompleted(e);
        }

        public new void ReportProgress(int percentProgress)
        {
            if (this.IsStop) return;

            base.ReportProgress(percentProgress);
        }

        public new void ReportProgress(int percentProgress, object userState)
        {
            if (this.IsStop) return;

            base.ReportProgress(percentProgress, userState);
        }

        public void Sleep(int millisecondTimeout)
        {
            lock (this)
            {
                if (!this.CancellationPending)
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
            lock (this)
            {
                isInterrupted = true;
                Monitor.Pulse(this);
            }
        }

        public void Stop()
        {
            this.CancelAsync();
            this.Interrupt();

            if (this.workerThread != null)
            {              
                this.workerThread.Join(100);
                if (this.workerThread.IsAlive)
                {                   
                    this.workerThread.Abort();
                }
            }
        }
    }
}
