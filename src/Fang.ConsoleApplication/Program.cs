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
                    if (cmd == "exit")
                    {
                        break;
                    }
                    else if (cmd == "Zhuanqu")
                    {
                        host.Zhuanqu();
                    }
                    else if (cmd == "Fenxi")
                    {
                        host.Fenxi();
                    }
                        
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.ReadLine();
        }

        
    }
}
