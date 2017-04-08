using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu.STAG_Classes
{
    /// <summary>
    /// Hlavni datovy zdroj pro analyzu
    /// </summary>
    class STAG_Database
    {
        /// <summary>
        /// Fakulta pro kterou stahujeme a načítáme všechny další informace. Obsahuje zkratku (např. PRF, PF ad.) 
        /// </summary>
        public string Fakulta { get;  private set;}
        
        /// <summary>
        /// Seznam všech studujících studentů na fakultě.
        /// </summary>
        public List<Student> Students { get; set; }
        
        /// <summary>
        /// HashTable všech rozvrhovaných akcí od studentů.
        /// </summary>
        public Dictionary<int,RozvrhovaAkce> Akce { get; set; }
        
        /// <summary>
        /// HashTable všech učitelů, kteří se vyskytují v rozvrhových akcích
        /// </summary>
        public Dictionary<int, Ucitel> Ucitele { get; set; }

        /// <summary>
        /// Predmety z fakulty
        /// Katedra - Kod - Predmet
        /// POZOR: Mezi ostatnimi predmety budou pravdepodobne i jine katedry nez z vybrane fakulty
        /// </summary>
        public Dictionary<string, Dictionary<string, Predmet>> PredmetyPodleKateder { get; set; }

        /// <summary>
        /// V prvni urovni je lvl 2(fakulty, knihovna, rektorat...) dal lvl 3 (katedry, oddeleni...)
        /// </summary>
        public Dictionary<string, Dictionary<string, Pracoviste>> HiearchiePracovist { get; set; }







        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="Fakulta">Zkratka fakulty pro ktereho analyzujeme</param>
        public STAG_Database(string Fakulta)
        {
            this.Fakulta = Fakulta;
            this.Akce = new Dictionary<int, RozvrhovaAkce>(1000);
            this.Ucitele = new Dictionary<int, Ucitel>(600);
            this.PredmetyPodleKateder = new Dictionary<string, Dictionary<string, Predmet>>();
            this.HiearchiePracovist = new Dictionary<string, Dictionary<string, Pracoviste>>();
        }



    }

    
}
