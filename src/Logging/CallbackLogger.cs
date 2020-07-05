using System;
using System.Linq;

namespace LiteLoader.Logging
{
    public class CallbackLogger : BaseLogger
    {
        private readonly Action<object> logAction;
        private readonly string format;

        public CallbackLogger(Action<string> callback, string format = "({DATETIME|HH:mm:ss})[{LEVEL}]: {MESSAGE}")
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentNullException(nameof(format));
            }

            this.format = format;
            logAction = new Action<object>(o => callback(o.ToString()));
        }

        public CallbackLogger(Action<object> callback, string format = "({DATETIME|HH:mm:ss})[{LEVEL}]: {MESSAGE}")
        {
            if (string.IsNullOrEmpty(format))
            {
                throw new ArgumentNullException(nameof(format));
            }

            this.format = format;
            logAction = callback;
        }

        protected override void OnNewLogMessage(LogMessage message)
        {
            logAction(message.Message);
        }

        private string Format(LogMessage message)
        {
            string msg = format;

            while (msg.Contains('}'))
            {
                int startPos = msg.IndexOf('{');
                int endPos = msg.IndexOf('}');

                string replacement = msg.Substring(startPos, endPos);

                if (replacement.StartsWith("{datetime", StringComparison.InvariantCultureIgnoreCase))
                {
                    string format = "YYYY-mm-dd HH:mm:ss";

                    if (replacement.Contains('|'))
                    {
                        format = replacement.Substring(replacement.IndexOf('|') + 1).TrimEnd('}');
                    }

                    msg = msg.Replace(replacement, message.Timestamp.ToString(format));
                }
                else if (replacement.StartsWith("{message", StringComparison.InvariantCultureIgnoreCase))
                {
                    msg = msg.Replace(replacement, message.Message);
                }
                else if (replacement.StartsWith("{level", StringComparison.InvariantCultureIgnoreCase))
                {
                    msg = msg.Replace(replacement, message.Level.ToString());
                }
            }

            return msg;
        }
    }
}
