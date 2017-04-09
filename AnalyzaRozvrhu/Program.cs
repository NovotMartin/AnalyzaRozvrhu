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

            // Přihlášení
            Console.WriteLine("zadej login:");
            var log = Console.ReadLine();
            Console.WriteLine("zadej heslo:");
            var pass = Console.ReadLine();

            // Vytvoreni databaze
            var data = STAG_DataCollector.GetData(STAG_Classes.Fakulta.PRF,log,pass);

            // Zjisteni chybějících informací od kateder

            // Podili ucitelu    
            data.GenerovatDotaznikKatedramXLS("example");
            // pouzivejte soubor PodilUciteleKatedry.xlsx z http://physics.ujep.cz/~jskvor/AVD/AktualizovanaPodobaPodkladuZKateder/
            data.NacistDotaznikKatedramXLS(@"STAG_DATA\PodilUciteleKatedry.xlsx");
            // Atyp předměty

            //Nacteni dotazniku s Atyp predmety
            data.NacistDotazniAtypPredmety(@"STAG_DATA\PredmetyATYP.xlsx");
                //todo

            // Spojeni společně vyučovaných předmětů apod.
            data.Preprocess();

            // Vypocet zateze
            data.Analyzuj();

            // Vygenerovani vystupu
            data.GenerovatPrehledXLS("hlavnivystup");

        }
    }
}
