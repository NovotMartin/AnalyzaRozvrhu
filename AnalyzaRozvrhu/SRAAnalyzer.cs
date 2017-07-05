using AnalyzaRozvrhu.STAG_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    /// <summary>
    /// Trida pro analyzu jednotlivych Super Rozvrhovych Akci (SRA).
    /// Vysledky analyzy se ukladaji do tridy predane v konstruktoru.
    /// </summary>
    public class SRAAnalyzer
    {
        /// <summary>
        /// trida, kde kumuluje zatez na vyuku vsech studentu
        /// </summary>
        private ZatezNaStudenta onusDistribution;

        /// <summary>
        /// Seznam "startovnich" hodin vyuky akce s danym RoakIdno (id rozvrhove akce)
        /// </summary>
        private Dictionary<int, List<int>> roakIdnoToStartTimes;

        /// <summary>
        /// Pocet studentu, kteri jsou na SRA v danou vyucovaci hodinu (tj soucet na vsech spolecnych rozvrhovych akcich).
        /// </summary>
        private Dictionary<int, int> noStudentsOnSRAHour;

        /// <summary>
        /// Zatez na katedru pri sdilene vyuce
        /// </summary>
        private Dictionary<int, Dictionary<string, double>> roakIdnoSharedUtility;

        /// <summary>
        /// Pocet opakovani SRA
        /// </summary>
        private Dictionary<int, int> repetition;

        /// <summary>
        /// Konstruktor vnitrne nastavi tridu, do ktere budeme akumulovat vypocty 
        /// zateze pro vyuku vsech studentu.
        /// </summary>
        /// <param name="onusDistribution">Trida pro akumulaci.</param>
        public SRAAnalyzer(ZatezNaStudenta onusDistribution)
        {
            this.onusDistribution = onusDistribution;
        }

        /// <summary>
        /// Provede analyzu zateze na vyuku zadane SRA.
        /// Vysledek akumulujeme do onusDistribution.
        /// </summary>
        /// <param name="sra">SRA, kterou chceme analyzovat.</param>
        public void AnalyzeSRA(SRA sra)
        {
            //TODO: Je treba poresit zachazeni s atypickymi predmety (napr. rozliseni pomoci priznaku)
        }

        ///////////////////// Private metody //////////////////


        private static Dictionary<int, List<int>> getRoakIdnoToStartTimes(SRA sra)
        {
            return null;
        }

        private static Dictionary<int, int> getNoStudentsOnSRAHour(SRA sra)
        {
            return null;
        }

        private static Dictionary<int, Dictionary<string, double>> getRoakIdnoSharedUtility(SRA sra)
        {
            return null;
        }

        private static Dictionary<int, int> getRepetition(SRA sra)
        {
            return null;
        }
    }
}
