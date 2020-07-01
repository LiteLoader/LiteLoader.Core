﻿using System;

namespace LiteLoader.Logging
{
    public abstract class CoreLogger : ILogger
    {
        protected struct LogMessage
        {
            public string Message { get; }

            public LogLevel Level { get; }

            public DateTimeOffset Timestamp { get; }

            public LogMessage(string msg, LogLevel level)
            {
                Message = msg;
                Level = level;
                Timestamp = DateTimeOffset.Now;
            }
        }

        public LogLevel Level { get; }

        protected CoreLogger()
        {
            Level = Interface.CoreModule.LogLevel;
        }

        public void Log(object msg, LogLevel level)
        {
            if (level.HasFlag(LogLevel.None) || !Level.HasFlag(level))
            {
                return;
            }

            if (msg == null || (msg is string str && string.IsNullOrEmpty(str)))
            {
                return;
            }

            LogMessage message = new LogMessage(msg.ToString(), level);
            OnNewLogMessage(message);
        }

        protected abstract void OnNewLogMessage(LogMessage message);
    }
}
