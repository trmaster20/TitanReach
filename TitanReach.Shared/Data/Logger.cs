using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TRShared
{
    public class Logger
    {
        public static event EventHandler<string> OnSharedLog;

        /// <summary>
        /// Client & Server handle logging differently. Instead of referencing unity libraries to do Debug.Log
        /// we get both projects to subscribe to logs and print them however they please.
        /// </summary>
        public static void Log(string message)
        {
            message = "[SharedLibrary]: " + message;
            OnSharedLog(null, message);
        }

        public static void Log(string msg, Exception e) => Log(msg + e.Message + "\n" + e.StackTrace);
        public static void Log(int num) => Log(num.ToString());
    }
}
