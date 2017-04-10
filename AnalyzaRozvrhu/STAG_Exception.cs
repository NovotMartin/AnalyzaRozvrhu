using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AnalyzaRozvrhu
{

    
    // Sem pište všechny vyjímky které napíšete, ať v tom nemáme bordel
    
    


    

    /// <summary>
    /// Vyjimka, která vznikne v pripadě že ve vstupní souboru PredmetyATYP.xlsx je u predmetu spatne vyplnena rozvrhova akce (nebo tam neni vyplnena vubec)
    /// </summary>
    public class STAG_Exception_InvalidTypeOfCourses : System.Exception
    {

       /// <summary>
       /// Pri volani vyjimky predame vyijimce seznam predmetu ktere maji zadany spatny typ rozvrhove akce (nebo zadny)
       /// </summary>
       /// <param name="Courses">List tuplu kde item1 == zkratka katedry, item2 == kod predmetu </param>
       public STAG_Exception_InvalidTypeOfCourses(List<Tuple<string, string>> Courses) : base()
       {
           
            foreach (var rozvrhovaAkce in Courses)
                Debug.WriteLine("Spatny typ rozvrhove akce u predmetu {0}/{1}", rozvrhovaAkce.Item1, rozvrhovaAkce.Item2);
       }

        /// <summary>
        /// Pri volani vyjimky predame vyijimce  predmet ktery ma zadany spatny typ rozvrhove akce (nebo zadny)
        /// </summary>
        /// <param name="Courses">Tuple kde item1 == zkratka katedry, item2 == kod predmetu </param>
        public STAG_Exception_InvalidTypeOfCourses(Tuple<string, string> Courses) : base()
       {
           
            Debug.WriteLine("Spatny typ rozvrhove akce u predmetu {0}/{1}", Courses.Item1,Courses.Item2);
        }

    }
}
