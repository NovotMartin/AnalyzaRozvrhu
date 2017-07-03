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
    public class STAG_Database
    {
        /// <summary>
        /// Fakulta pro kterou stahujeme a načítáme všechny další informace. Obsahuje zkratku (např. PRF, PF ad.) 
        /// </summary>
        /// <remarks>Může obsahovat například <c>PRF</c></remarks>
        public string Fakulta { get; private set; }

        /// <summary>
        /// Seznam všech studujících studentů na fakultě.
        /// </summary>
        public List<Student> Students { get; set; }

        /// <summary>
        /// HashTable všech rozvrhovaných akcí od studentů.
        /// </summary>
        /// <remarks>Obsahuje všechny rozvrhové akce získáne a načtené z rozvrhů studentů. Klíčem je roakID. Používá se pro rychlejší přiřazování a vyhledávání akcí.</remarks>
        /// <example>
        /// Rozvrhovou akci podle id získáme následujícím způsobem
        /// <code>
        /// var roak = Database.Students[295151] // vrátí rozvrhovou akci cvičení z předmětu AVD
        /// </code>
        /// </example>
        public Dictionary<int, RozvrhovaAkce> Akce { get; set; }

        /// <summary>
        /// HashTable všech učitelů, kteří se vyskytují v rozvrhových akcích
        /// </summary>
        /// <remarks>Obsahuje všechny učitele, na které se během načítání dat narazí. Klíčem je ucitID.</remarks>
        /// <example>
        /// Učitele podle id získáme následujícím způsobem
        /// <code>
        /// var ucit = Database.Ucitele[2220] // vrátí objekt učitele Jiří Škvor
        /// </code>
        /// </example>
        public Dictionary<int, Ucitel> Ucitele { get; set; }

        /// <summary>
        /// Predmety z fakulty
        /// Katedra - Kod - Predmet
        /// </summary>
        /// <remarks>
        /// Obsahuje všechny předměty které se vyskytly během načítání. Doporučují při přístupu kontrolovat, jestli v Dictionary existují klíče apod.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Takhle projdeme všechny předměty katedri KI
        /// foreach(var katedra in Database.PredmetyPodleKateder["KI"])
        ///     foreach (var predmet in katedra.Value)
        ///     {
        ///         //ted je v predmet ulozen odkaz na nejaky predmet z katedry
        ///     }
        ///     
        /// // Takhle zase jednoduše zíkáme predmet AVD z tohoto slovníku
        /// var avd = Database.PredmetyPodleKateder["KI"]["AVD"];
        /// </code>
        /// </example>
        public Dictionary<string, Dictionary<string, Predmet>> PredmetyPodleKateder { get; set; }

        /// <summary>
        /// Všechny pracoviště v hiearchii
        /// </summary>
        /// <remarks>V prvni urovni je lvl 2(fakulty, knihovna, rektorat...) dal lvl 3 (katedry, oddeleni...)</remarks>
        public Dictionary<string, Dictionary<string, Pracoviste>> HiearchiePracovist { get; set; }

        /// <summary>
        /// Všechny SRA vygenerované v rámci preprocessingu
        /// </summary>
        public List<STAG_Classes.SRA> SuperRozvrhoveAkce { get; set; }

        /// <summary>
        /// Zatez kateder na vyuku studentu.
        /// Pozn.: Zatez neni normalizovana.
        /// </summary>
        public ZatezNaStudenta zatezNaStudenta;

        /// <summary>
        /// Studenti navstevujici zadanou rozvrhovou akci.
        /// </summary>
        public Dictionary<int, List<Student>> studentsOnRoakIdno;

        /// <summary>
        /// Seznam studentu obsahujici vsechny akce, na ktere jsou zapsani
        /// Konkretne dvojici (RoakIdno, SRA)
        /// </summary>
        public Dictionary<Student, List<Tuple<int, SRAOverlay>>> studentsSRA;

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
            this.SuperRozvrhoveAkce = new List<SRA>();
            this.zatezNaStudenta = new ZatezNaStudenta();
            this.studentsOnRoakIdno = new Dictionary<int, List<Student>>();
            this.studentsSRA = new Dictionary<Student, List<Tuple<int, SRAOverlay>>>();
        }



    }


}
