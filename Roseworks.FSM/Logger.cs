using System;
using System.Collections.Generic;
using System.Text;

namespace Roseworks
{
    public static class Logger
    {
        public static Action<string> LogLineCallback = (l) =>
        {
            if (LogCallback != null)
                LogCallback(l + "\r\n");
        };
        public static Action<string> LogCallback = null;

        public static StringBuilder Builder = new StringBuilder();

        public static void WriteLine(string line)
        {
            if (LogLineCallback != null)
                LogLineCallback(line);
        }

        public static void Write(string text)
        {
            if (LogCallback != null)
                LogCallback(text);
        }
    }
}