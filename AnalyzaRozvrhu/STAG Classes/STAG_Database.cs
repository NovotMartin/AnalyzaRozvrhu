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
        /// Hash Table predmetu na zvolene katedře.
        /// Key = predmet.katedra/predmet.zkratka
        /// </summary>
        public Dictionary<string, Predmet> Predmety { get; set; }
        /// <summary>
        /// Seznam všech katedere získaný z předmětů fakulty
        /// POZOR: Mezi predmety budou pravdepodobne i jine katedry nez z vybrane fakulty
        /// </summary>
        public List<String> Katedry { get; set; }


        public STAG_Database(string Fakulta)
        {
            this.Fakulta = Fakulta;
            this.Akce = new Dictionary<int, RozvrhovaAkce>(1000);
            this.Ucitele = new Dictionary<int, Ucitel>(600);
            this.Predmety = new Dictionary<string, Predmet>(1000);
        }



    }

    
}
