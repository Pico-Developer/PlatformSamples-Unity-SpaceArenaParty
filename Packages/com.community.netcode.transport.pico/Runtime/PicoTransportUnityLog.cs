using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Netcode.Transports.Pico
{
    public partial class PicoTransport
    {
        enum LogLevel
        {
            Debug = 0,
            Info,
            Warn,
            Error,
            Fatal
        };

        static LogLevel IntToLogLevel(int logLevel)
        {
            if (logLevel <= 3)
            {
                return LogLevel.Debug;
            }
            if (logLevel <= 4)
            {
                return LogLevel.Info;
            }
            if (logLevel <= 5)
            {
                return LogLevel.Warn;
            }
            if (logLevel <= 6)
            {
                return LogLevel.Error;
            }
            return LogLevel.Fatal;
        }

        static void MyLog(int level, IntPtr nativeLog)
        {
            var curtime = DateTime.Now.ToString("hh.mm.ss.ffffff");
            LogLevel logLev = IntToLogLevel(level);
            var logCont = Marshal.PtrToStringAnsi(nativeLog);
            switch (logLev)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log($"[{curtime}]PicoLibLog: {logCont}");
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning($"[{curtime}]PicoLibLog: {logCont}");
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                default:
                    Debug.LogError($"[{curtime}]PicoLibLog: {logCont}");
                    break;
            }
        }

        public static void Log(object logCont)
        {
            var curtime = DateTime.Now.ToString("hh.mm.ss.ffffff");
            Debug.Log($"[{curtime}]PicoLibLog: {logCont}");
        }

        public static void LogWarning(object logCont)
        {
            var curtime = DateTime.Now.ToString("hh.mm.ss.ffffff");
            Debug.LogWarning($"[{curtime}]PicoLibLog: {logCont}");
        }

        public static void LogError(object logCont)
        {
            var curtime = DateTime.Now.ToString("hh.mm.ss.ffffff");
            Debug.LogError($"[{curtime}]PicoLibLog: {logCont}");
        }
    }
}
