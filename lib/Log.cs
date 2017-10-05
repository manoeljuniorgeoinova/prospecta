using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib
{
    public class Log
    {
        private static object logLock = new object();

        private static string getFilename()
        {
            return System.IO.Directory.GetCurrentDirectory() + @"\log.txt";
        }

        private static void writeFile(string str)
        {
            lock (logLock)
            {
                System.IO.File.AppendAllText(Log.getFilename(), str);
            }
        }

        public static void Write(string text)
        {
            var str = String.Format("{0} {1}", DateTime.Now, text);

            Console.Write(str);
            Log.writeFile(text);
        }

        public static void WriteLine(string text)
        {
            var str = String.Format("{0} {1}", DateTime.Now, text);

            Console.WriteLine(str);
            Log.writeFile("\r\n" + str);
        }

        public static void WriteLine()
        {
            Console.WriteLine("\r\n");
            Log.writeFile("\r\n");
        }

        public static void begin(string text)
        {

        }

        public static void end()
        {

        }
    }
}
