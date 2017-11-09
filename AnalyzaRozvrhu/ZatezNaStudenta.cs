using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    /// <summary>
    /// Zatez kateder na vyuku studentu.
    /// </summary>
    public class ZatezNaStudenta
    {
        private Dictionary<STAG_Classes.Student, Dictionary<string, double>> students;

        private int TestInt = 9;

        /// <summary>
        /// Inicializace.
        /// </summary>
        public ZatezNaStudenta()
        {
            students = new Dictionary<STAG_Classes.Student, Dictionary<string, double>>();
        }

        /// <summary>
        /// Přidání zátěže studenta na katedru
        /// </summary>
        /// <param name="student">Osobní číslo studenta FXXYYY</param>
        /// <param name="katedra">Zkratka katedry</param>
        /// <param name="zatez">Zatížení studenta</param>
        public void Pridat(STAG_Classes.Student student, string katedra, double zatez)
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
        /// Zmeni zates studenta na konkretni katedru
        /// </summary>
        /// <param name="student"></param>
        /// <param name="katedra"></param>
        /// <param name="zatez"></param>
        /// <returns></returns>
        public bool Zmenit(STAG_Classes.Student student, string katedra, double zatez)
        {
            Smazat(student, katedra);
            Pridat(student, katedra, zatez);
            return true;
        }

        /// <summary>
        /// Vyčiští všechny uložené zátěže
        /// </summary>
        public void Smazat()
        {
            students = new Dictionary<STAG_Classes.Student, Dictionary<string, double>>();
        }

        /// <summary>
        /// Smazat zatizeni studenta
        /// </summary>
        /// <param name="student"></param>
        public bool Smazat(STAG_Classes.Student student)
        {
            if (students.ContainsKey(student))
                return students.Remove(student);
            return false;
        }

        /// <summary>
        /// Zmeni zatizeni studenta kontretni katedry
        /// </summary>
        /// <param name="student"></param>
        /// /// <param name="katedra"></param>
        public bool Smazat(STAG_Classes.Student student, string katedra)
        {
            if (students.ContainsKey(student))
                if (students[student].ContainsKey(katedra))
                    return students[student].Remove(katedra);
            return false;
        }

        /// <summary>
        /// Ziskani zateze konkretniho studenta
        /// </summary>
        /// <returns>Slovnik Katedra -> zatez</returns>
        public Dictionary<string, double> ZiskatZatezStudenta(STAG_Classes.Student student)
        {
            Dictionary<string, double> ret;
            if (students.TryGetValue(student, out ret))
                return Normalize(ret);
            else
                return null;
        }

        /// <summary>
        /// Postupne ziskani vsech studentu, kterym byla pridana zatez
        /// </summary>
        /// <returns>Vrací pár: objekt studenta a slovnik katedra -> zatez</returns>
        public IEnumerable<Tuple<STAG_Classes.Student, Dictionary<string, double>>> GetAll()
        {
            foreach (var student in students)
                yield return new Tuple<STAG_Classes.Student, Dictionary<string, double>>(student.Key, ZiskatZatezStudenta(student.Key));
        }

        private Dictionary<string, double> Normalize(Dictionary<string, double> percents)
        {
            double sum = (from dep in percents select dep.Value).Sum();
            for (int i = 0; i < percents.Count; i++)
                percents[percents.ElementAt(i).Key] /= sum;
            return percents;
        }
    }
}
