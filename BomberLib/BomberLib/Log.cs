using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BomberLib
{
    class Log
    {
        private Boolean usefile, useconsole;
        private Object logLock = new Object();
        private string filename = null;
        private StreamWriter file;
        private Boolean outputRedirected;

        /// <summary>
        ///Print the Log to Console
        /// </summary>
        /// <param name="useconsole">true = print Log to Console</param>
        /// <param name="usefile">true = print Log to File</param>
        public Log(Boolean useconsole, Boolean usefile)
        {
            this.useconsole = useconsole;
            this.usefile = usefile;
            outputRedirected = ConsoleEx.IsOutputRedirected;
        }

        private void open()
        {
            string datenow = DateTime.Now.ToString("yyyy-MM-dd");
            string checkfile = Environment.CurrentDirectory + @"\Log\" + datenow + ".log";
            if (!Directory.Exists(Environment.CurrentDirectory + @"\Log\")) Directory.CreateDirectory(Environment.CurrentDirectory + @"\Log\");
            if (filename == null)
            {
                filename = checkfile;
                if (!File.Exists(filename))
                {
                    file = new StreamWriter(File.Create(filename));
                }
                else
                {
                    file = new StreamWriter(filename, true);
                }
            }
            else if (filename != checkfile)
            {
                close();
                filename = null;
                open();
            }
        }

        private void close()
        {
            file.Close();
        }

        private int INFO = 0;
        private int WARN = 1;
        private int ERROR = 2;
        private int FATAL = 3;

        /// <summary>
        /// Prints the Log into the Console
        /// </summary>
        /// <param name="message">The Message to print</param>
        /// <param name="level">0 = Info; 1 = Warning; 2 = Error; 3 = Fatal</param>
        /// <returns></returns>
        private Boolean print(string message, int level)
        {
            if (usefile) open();
            lock (logLock)
            {
                string timenow = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
                //Info
                if (level == INFO)
                {
                    if (usefile) file.WriteLine(timenow + "[INFO]  " + message);
                    if (useconsole) Console.WriteLine(timenow + "[INFO]  " + message);
                }
                //Warning
                if (level == WARN)
                {
                    if (usefile) file.WriteLine(timenow + "[WARN]  " + message);
                    if (useconsole)
                    {
                        if(!outputRedirected) Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(timenow + "[WARN]  " + message);
                        if (!outputRedirected) Console.ResetColor();
                    }
                }
                //Error
                if (level == ERROR)
                {
                    if (usefile) file.WriteLine(timenow + "[ERROR] " + message);
                    if (useconsole)
                    {
                        if (!outputRedirected) Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(timenow + "[ERROR] " + message);
                        if (!outputRedirected) Console.ResetColor();
                    }
                }
                //Fatal
                if (level == FATAL)
                {
                    if (usefile) file.WriteLine(timenow + "[FATAL] " + message);
                    if (useconsole)
                    {
                        if (!outputRedirected) Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(timenow + "[FATAL] " + message);
                        if (!outputRedirected) Console.ResetColor();
                    }
                }

                if (usefile) file.Flush();
            }

            return true;
        }

        public void info(String message)
        {
            print(message, this.INFO);
        }

        public void warn(String message)
        {
            print(message, this.WARN);
        }

        public void error(String message)
        {
            print(message, this.ERROR);
        }

        public void fatal(String message)
        {
            print(message, this.FATAL);
        }

    }
}
