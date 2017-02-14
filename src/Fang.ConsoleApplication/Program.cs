using AngleSharp;
using Fang.Core;
using Fang.Data;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fang.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            FangHost host = new FangHost();

            while (true)
            {
                string cmd = Console.ReadLine();
                if (string.IsNullOrEmpty(cmd))
                {
                    continue;
                }

                try
                {
                    
                        
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (cmd == "exit")
                {
                    break;
                }
                else if (cmd == "Zhuaqu")
                {
                    host.Zhuaqu();
                }
                else if (cmd == "Fenxi")
                {
                    host.Fenxi();
                }
            }

            Console.ReadLine();
        }

        
    }
}
