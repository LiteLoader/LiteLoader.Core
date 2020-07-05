using System;
using System.Collections.Generic;
using System.Threading;

namespace LiteLoader.Logging
{
    public abstract class ThreadedLogger : BaseLogger
    {
        private readonly Queue<LogMessage> _messageQueue;

        private readonly object _syncRoot;

        private readonly Thread _workerThread;

        private readonly AutoResetEvent _reset;

        private volatile bool _disposed = false;

        protected ThreadedLogger() 
        {
            _messageQueue = new Queue<LogMessage>();
            _syncRoot = new object();
            _reset = new AutoResetEvent(false);
            _workerThread = new Thread(LongRunningProcess)
            {
                IsBackground = true,
                Name = GetType().Name + "-" + Guid.NewGuid().ToString("D"),
            };
            _workerThread.Start();
        }

        ~ThreadedLogger()
        {
            _disposed = true;
            _reset.Set();
            _workerThread.Join();
        }

        #region Logging

        protected override void OnNewLogMessage(LogMessage message)
        {
            lock (_syncRoot)
            {
                _messageQueue.Enqueue(message);
            }
            _reset.Set();
        }

        protected abstract void Write(LogMessage logMessage);

        #endregion

        #region Threading

        private void LongRunningProcess()
        {
            while (!_disposed)
            {
                _reset.WaitOne();

                lock (_syncRoot)
                {
                    try
                    {
                        BeginBatchProcess();
                        while (_messageQueue.Count != 0)
                        {
                            LogMessage msg = _messageQueue.Dequeue();
                            Write(msg);
                        }
                    }
                    finally
                    {
                        EndBatchProcess();
                    }
                }
            }
        }

        protected abstract void BeginBatchProcess();

        protected abstract void EndBatchProcess();

        #endregion
    }
}
