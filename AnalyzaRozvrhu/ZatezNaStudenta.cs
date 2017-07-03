using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    /// <summary>
    /// Zatez kateder na vyuku studentu.
    /// Pozn.: Zatez neni normalizovana.
    /// </summary>
    public class ZatezNaStudenta
    {
        private Dictionary<string, Dictionary<string, double>> students;

        /// <summary>
        /// Inicializace.
        /// </summary>
        public ZatezNaStudenta()
        {
            students = new Dictionary<string, Dictionary<string, double>>();
        }

        /// <summary>
        /// Přidání zátěže studenta na katedru
        /// </summary>
        /// <param name="student">Osobní číslo studenta FXXYYY</param>
        /// <param name="katedra">Zkratka katedry</param>
        /// <param name="zatez">Zatížení studenta</param>
        public void Pridat(string student, string katedra, double zatez)
        {
            if (students.ContainsKey(student))
                if (students[student].ContainsKey(katedra))
                    students[student][katedra] += zatez;
                else
                    students[student].Add(katedra, zatez);
            else
                students.Add(student, new Dictionary<string, double> { { katedra, zatez } });
        }

        /// <summary>
        /// Vyčiští všechny uložené zátěže
        /// </summary>
        public void clean()
        {
            students = new Dictionary<string, Dictionary<string, double>>();
        }

    }
}
