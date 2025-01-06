using System;

namespace SimpleUDP
{
    public static class UdpLog
    {
        public delegate void Log(object mes);

        public static Log Info = Console.Write;
        public static Log Warning = Console.Write;
        public static Log Error = Console.Write;

        public static void Initialize(Log logInfo, Log logWarning = null, Log logError = null)
        {
            Info = logInfo;
            Warning = logWarning;
            Error = logError;
        }
    }
}