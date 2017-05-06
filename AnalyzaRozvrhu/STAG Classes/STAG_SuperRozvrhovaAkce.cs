using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu.STAG_Classes
{
    /// <summary>
    /// Obal pro rozvrhové akce
    /// </summary>
    /// <remarks>Tento objekt zapouzdřuje rozvrhové akce, buď tam budou samotné a nebo společně se společně rozvrhovanými</remarks>
    public class SRA
    {
        /// <summary>
        /// Pocet studentu na teto SRA (soucet vsech vnorenych akci)
        /// </summary>
        public int PocetStudentuSRA { get; set; }

        /// <summary>
        /// Předměty vyučované na SRA (reference na databazi předmětu v Database.PredmetyPodleKateder)
        /// </summary>
        public List<Predmet> Predmety { get; set; }

        /// <summary>
        /// Kolekce vnorenych akci... Zmenit typ podle potreby, zalezi jak se to bude zjistovat (napr. rozdeleni STAGovských akcí po hodinách a pod....)
        /// </summary>
        public List<RozvrhovaAkce> VnoreneAkce { get; set; }

        /// <summary>
        /// Vytvoří jednu SRA pro jeden předmět
        /// </summary>
        /// <param name="akce">Rozvrhová akce</param>
        public SRA(RozvrhovaAkce akce)
        {
            Inicializuj();

            VnoreneAkce.Add(akce);
            Predmety.Add(akce.PredmetRef);
            PocetStudentuSRA = akce.Obsazeni;
        }

       
        /// <summary>
        /// Vytvoří jednu SRA pro list předmětů
        /// </summary>
        /// <param name="listAkci">List rozvrhových akcí</param>
        public SRA(List<RozvrhovaAkce> listAkci)
        {
            Inicializuj();

            foreach (var akce in listAkci)
            {
                VnoreneAkce.Add(akce);
                Predmety.Add(akce.PredmetRef);
                PocetStudentuSRA += akce.Obsazeni;
            }
        }

        private void Inicializuj()
        {
            VnoreneAkce = new List<RozvrhovaAkce>();
            Predmety = new List<Predmet>();
            PocetStudentuSRA = 0;
        }


}
}
