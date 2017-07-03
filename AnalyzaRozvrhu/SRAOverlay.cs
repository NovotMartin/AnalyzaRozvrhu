using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    // Trida maskujici (prekryvajici) SuperRozvrhovouAkci
    public class SRAOverlay
    {
        /// <summary>
        /// Akce, kterou chceme prekryvat.
        /// </summary>
        public STAG_Classes.SRA parentAction;

        /// <summary>
        /// Seznam "startovnich" hodin vyuky akce s danym RoakIdno (id rozvrhove akce)
        /// </summary>        
        public Dictionary<int, List<int>> RoakIdnoToStartTimes;

        /// <summary>
        /// Pocet studentu, kteri jsou na SRA v danou vyucovaci hodinu (tj soucet na vsech spolecnych rozvrhovych akcich).
        /// </summary>
        public Dictionary<int, int> noStudentsOnSRAHour;

        /// <summary>
        /// Zatez na katedru pri sdilene vyuce
        /// </summary>
        public Dictionary<string, double> RoakIdnoSharedUtility;

        /// <summary>
        /// Pocet opakovani SRA
        /// </summary>
        public int repetition;
    }
}
