using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("zadej login:");
            var log = Console.ReadLine();
            Console.WriteLine("zadej heslo:");
            var pass = Console.ReadLine();


            STAG_DataCollector.GetData(STAG_Classes.Fakulta.PRF,log,pass);

            
        }
    }
}
