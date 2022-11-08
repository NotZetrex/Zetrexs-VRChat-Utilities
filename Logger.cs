using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum InfoType : int
{
    Loading = 0,
    Exception = 1,
    Complete = 2,
    Debug = 3,
}

namespace start_protected_game
{
    public class Logger
    {
        public static bool isDebug = false;

        string instanceName;
        public void Msg(string Message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{Time.GetTime()}][{instanceName}]: {Message}");
            Console.ResetColor();
        }

        public void Warn(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{Time.GetTime()}][{instanceName}](WARNING): {Message}");
            Console.ResetColor();
        }

        public void Error(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{Time.GetTime()}][{instanceName}](ERROR): {Message}");
            Console.ResetColor();
        }

        public void Info(string Message, InfoType infoType)
        {
            if (infoType == InfoType.Debug && isDebug == true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{Time.GetTime()}][{instanceName}](Debug): {Message}");
                Console.ResetColor();
            } else if (infoType != InfoType.Debug)
            {
                ConsoleColor color = ConsoleColor.White;

                if (infoType == InfoType.Loading)
                    color = ConsoleColor.Yellow;
                else if (infoType == InfoType.Exception)
                    color = ConsoleColor.Red;
                else if (infoType == InfoType.Complete)
                    color = ConsoleColor.Green;

                Console.ForegroundColor = color;
                Console.WriteLine($"[{Time.GetTime()}][{instanceName}](Info): {Message}{(infoType == InfoType.Complete ? "\n" : "")}");
                Console.ResetColor();
            }
            
        }

        public static Logger Instance(string name)
        {
            Logger logger = new Logger();
            logger.instanceName = name;
            return logger;
        }
    }

    public class Time
    {
        public static string GetTime()
        {
            return DateTime.Now.ToString("hh:mm:ss.fff");
        }
    }
}
