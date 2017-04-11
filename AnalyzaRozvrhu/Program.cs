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
            var pass = GetPass();

            // Vytvoreni databaze
            var data = STAG_DataCollector.GetData(STAG_Classes.Fakulta.PRF,log,pass);

            // Zjisteni chybějících informací od kateder

            // Podili ucitelu    
            data.GenerovatDotaznikKatedramXLS("example.xlsx");
            // pouzivejte soubor PodilUciteleKatedry.xlsx z http://physics.ujep.cz/~jskvor/AVD/AktualizovanaPodobaPodkladuZKateder/
            data.NacistDotaznikKatedramXLS(@"STAG_DATA\PodilUciteleKatedry.xlsx");
            // Atyp předměty

            //Nacteni dotazniku s Atyp predmety
            try
            {
                data.NacistDotazniAtypPredmety(@"STAG_DATA\PredmetyATYP.xlsx");
            }
            catch(STAG_Exception_InvalidTypeOfCourses e)
            {
                //TODO
                // tady bude reakce na vyvolanou vyjimku
            }
            
                //todo

            // Spojeni společně vyučovaných předmětů apod.
            data.Preprocess();

            // Vypocet zateze
            data.Analyzuj();

            // Vygenerovani vystupu
            data.GenerovatPrehledXLS("hlavnivystup.xlsx");

        }

        /// <summary>
        /// Skryti hesla pri psani, abychom se nemuseli bat to pouzivat na verejnosti.
        /// Ja jsem se bal - Jiri Kramar
        /// </summary>
        /// <returns>Heslo</returns>
        static string GetPass()
        {
            StringBuilder sb = new StringBuilder();
            ConsoleKeyInfo key = Console.ReadKey(true);

            while (key.Key != ConsoleKey.Enter)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (sb.Length > 0)
                        {
                            Console.CursorLeft--;
                            Console.Write(" ");
                            Console.CursorLeft--;
                            sb.Remove(sb.Length - 1, 1);
                        }                        
                        break;
                    case ConsoleKey.Escape:
                        // nuthing
                        break;
                    default:
                        Console.Write("*");
                        sb.Append(key.KeyChar);
                        break;
                }

                key = Console.ReadKey(true);
            }

            return sb.ToString();
        }
    }
}
