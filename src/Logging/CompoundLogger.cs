using System;
using System.Collections.Generic;

namespace LiteLoader.Logging
{
    public class CompoundLogger : ILogger
    {
        public LogLevel Level { get; }

        protected List<ILogger> SubLoggers { get; }

        public CompoundLogger()
        {
            Level = Interface.CoreModule.LogLevel;
            SubLoggers = new List<ILogger>();
        }

        public void Log(object msg, LogLevel level)
        {
            lock (SubLoggers)
            {
                for (int i = 0; i < SubLoggers.Count; i++)
                {
                    ILogger logger = SubLoggers[i];
                    try
                    {
                        logger.Log(msg, level);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void AddLogger(ILogger logger)
        {
            if (logger == null || logger == this)
            {
                return;
            }

            lock (SubLoggers)
            {
                if (!SubLoggers.Contains(logger))
                {
                    SubLoggers.Add(logger);
                }
            }
        }

        public void RemoveLogger(ILogger logger)
        {
            if (logger == null || logger == this)
            {
                return;
            }

            lock (SubLoggers)
            {
                if (SubLoggers.Contains(logger))
                {
                    SubLoggers.Remove(logger);
                }
            }
        }
    }
}
