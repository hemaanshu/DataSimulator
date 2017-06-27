using System;

namespace DataSimulator
{
    public class Trace
    {
        private int logLevel;

        private static Trace instance;

        private Trace()
        {
            this.logLevel = Config.Instance.LogLevel;
        }

        public static Trace Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new Trace();
                }

                return instance;
            }
        }

        public void WriteInfo(string log, params object[] args)
        {
            Console.WriteLine(log, args);
        }

        public void WriteVerbose(string log, params object[] args)
        {
            if (2 == this.logLevel)
            {
                var current = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                WriteInfo(log, args);
                Console.ForegroundColor = current;
            }
        }

        public void WriteWarning(string log, params object[] args)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteInfo(log, args);
            Console.ForegroundColor = current;
        }

        public void WriteError(string log, params object[] args)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteInfo(log, args);
            Console.ForegroundColor = current;
        }
    }
}
