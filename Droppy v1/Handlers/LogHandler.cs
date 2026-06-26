using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.IO;

namespace Droppy_v1.Handlers
{
    public class logHandler
    {
        public void Start()
        {
            using (StreamWriter sw = new StreamWriter(Declarations.logPath, true))
            {
                sw.WriteLine("------------------------------------------------------");
                sw.WriteLine($"Bot started at {DateTime.Now}");
            }
        }

        public void WriteLine(string log)
        {
            if(log.Contains("[Info]"))
                Console.ForegroundColor = System.Drawing.Color.FromArgb(0, 255, 0);
            else if(log.Contains("[Command]"))
                Console.ForegroundColor = System.Drawing.Color.FromArgb(55, 177, 227);
            else if (log.Contains("[ERROR]"))
                Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 0, 0);
            else if (log.Contains("[Warning]"))
                Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 255, 0);

            Console.WriteLine(log);
            Console.ForegroundColor = System.Drawing.Color.FromArgb(255, 255, 255);
            using (StreamWriter sw = new StreamWriter(Declarations.logPath, true))
            {
                sw.WriteLine(log);
            }

        }
    }
}
