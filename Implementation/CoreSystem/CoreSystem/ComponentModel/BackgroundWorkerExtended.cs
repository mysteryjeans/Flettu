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

        private Thread orignalThread;
        private Thread workerThread;



        protected override void OnDoWork(DoWorkEventArgs e)
        {
            this.orignalThread = Thread.CurrentThread;

            this.workerThread = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        base.OnDoWork(e);
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }
                }));

            this.workerThread.Start();
            this.workerThread.Join();
        }

        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            base.OnRunWorkerCompleted(e);
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
                this.workerThread.Join(3000);
                if (this.workerThread.IsAlive)
                {
                    this.workerThread.Abort();
                    this.orignalThread.Join(3000);

                    if (this.orignalThread.IsAlive)
                        this.orignalThread.Abort();
                }              
            }
        }
    }
}
