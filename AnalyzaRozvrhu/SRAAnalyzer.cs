using AnalyzaRozvrhu.STAG_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region Privatni globalni promenne

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
        private Dictionary<int, int> roakIdnoRepetition;

        /// <summary>
        /// SRA, kterou prave Analyzujeme
        /// </summary>
        private SRA sra;

        /// <summary>
        /// Studeti navstevujici konkretni rozvrhovou akci s roakIdno.
        /// </summary>
        private Dictionary<int, List<Student>> studentsOnRoakIdno;

        #endregion

        #region Verejne metody

        /// <summary>
        /// Konstruktor vnitrne nastavi tridu, do ktere budeme akumulovat vypocty 
        /// zateze pro vyuku vsech studentu.
        /// </summary>
        /// <param name="onusDistribution">Trida pro akumulaci.</param>
        public SRAAnalyzer(ZatezNaStudenta onusDistribution, Dictionary<int, List<Student>> studentsOnRoakIdno)
        {
            this.onusDistribution = onusDistribution;
            this.studentsOnRoakIdno = studentsOnRoakIdno;
        }

        /// <summary>
        /// Provede analyzu zateze na vyuku zadane SRA.
        /// Vysledek akumulujeme do onusDistribution.
        /// </summary>
        /// <param name="sra">SRA, kterou chceme analyzovat.</param>
        public void AnalyzeSRA(SRA sra)
        {
            CleanInit(sra);    // Pro analyzu kazde SRA je treba si vycistit promenne

            if (sra.Predmety.Any(predmet => predmet.IsAtypical))
            {
                if (sra.Predmety.Count > 1)
                {
                    // Pocitame zatez pro atypipicke i radne predmety, ktere jsou sdruzene do jedne SRA
                    // Budeme resit tak, ze si vsechny RA v SRA projedeme a zpracujeme jako samostatne rozvrhove akce,
                    // tj. bez sdileni zateze.
                    // TODO: Tady muze vzniknout chyba, ktera ovlivni vypocet zateze!

                    if (sra.Predmety.Any(predmet => !predmet.IsAtypical))
                    {
                        // Pocitame zatez v pripade, ze v SRA jsou alespon dva predmety, 
                        // kde alespon jeden je atypicky a alespon jeden je radny
                        // TODO: Jak resit vypocet zateze v tomto pripade? Zatim zatez nezapocitavam!

                        // Zatim si jen vypisu, ktere to jsou
                        foreach (Predmet p in sra.Predmety)
                            if (!p.IsAtypical)
                                Debug.WriteLine(string.Format("Predmet {0}/{1} je radny predmet a je v SRA uveden s atypickymi predmety.", p.Katedra, p.Zkratka));
                    }
                    else
                    {
                        // Pocitame zatez v pripade, ze v SRA jsou alespon dva predmety a vsechny jsou atypicke
                        foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
                        {
                            // musim si vycistit globalni promene
                            // ComputeSimpleATYPOnusDistribution do nich totiz opakovane zapisuje a potrebuje je "prazdne"
                            CleanInit(sra);
                            ComputeSimpleATYPOnusDistribution(ra);  // Spoctu zatez na vyuku kazde akce zvlast
                        }
                    }
                }
                else
                {
                    // Pocitam zatez pro atypicky predmet, ktery je v SRA sam
                    ComputeSimpleATYPOnusDistribution(sra.VnoreneAkce[0]);
                }
            }
            else
            {
                // Pocitam zatez pro radny predmet

                SetRoakIdnoToStartTimes();
                SetNoStudentsOnSRAHour();
                SetRoakIdnoRepetition();
                SetRoakIdnoSharedUtility();

                ComputeOnusDistribution();
            }
        }

        #endregion

        #region Privatni metody

        /// <summary>
        /// Vypocte a ulozi do vystupni tridy zatez na vyuku analyzovane SRA, ktera obsahuje jeden atypicky predmet. 
        /// </summary>
        private void ComputeSimpleATYPOnusDistribution(RozvrhovaAkce ra)
        {
            SetRoakIdnoSharedUtility(ra); // Podobne, jako u radnych predmetu, si nastavim rozdeleni zateze mezi katedry
            
            // Kvuli sdilene zatezi na vyuku predmetu budu pridavat zatez pro kazdou katedru 
            foreach (string dept in roakIdnoSharedUtility[ra.RoakIdno].Keys)
            {
                int groupMaxSize = GetGroupMaxSizeForATYP(ra).GetValueOrDefault();

                if (groupMaxSize == 0)
                {
                    // Pokud je groupMaxSize, potom pocitame s daty pro cely predmet - Pouze pro jistotu, nemolo by byt pouzito

                    // ke kazdemu studentovi
                    foreach (Student student in studentsOnRoakIdno[ra.RoakIdno])
                    {
                        // Spocteme zatez na vyuku jednoho studenta, kde vsichni zapsani studenti jsou v jedne skupine
                        double onus = ComputeATYPOnus(ra.Obsazeni, roakIdnoSharedUtility[ra.RoakIdno][dept], GetNoOfHoursForATYP(ra));

                        onusDistribution.Pridat(student, dept, onus);   // zapis do vystupni tridy
                    }
                }
                else
                {
                    // Pokud groupMaxSize neni 0, musime spocitat zatez na vyuku KAZDE skupiny

                    // ke kazdemu studentovi
                    foreach (Student student in studentsOnRoakIdno[ra.RoakIdno])
                    {
                        //TODO: Pri vypoctu zanedbavam, ze posledni skupina nemusi mit maximalni pocet studentu

                        // Spocteme si zatez na vyuku jednoho studenta ve skupine maximalni velikosti
                        double onus = ComputeATYPOnus(groupMaxSize, roakIdnoSharedUtility[ra.RoakIdno][dept], GetNoOfHoursForATYP(ra));

                        onusDistribution.Pridat(student, dept, onus);   // zapis do vystupni tridy
                    }
                }
            }
        }

        /// <summary>
        /// Spocte zatez na vyuku jednoho studenta ve skupine o zadane velikosti se zadanym sdilenim vyukove zateze mezi katedry
        /// a s poctem zadanych oducenych hodin.
        /// Pouze pro ATYP predmety, ktere se neopakuji! Tj. Pocet oducenych hodin uz je konecny!
        /// </summary>
        /// <param name="noStudents"></param>
        /// <param name="sharedUtility"></param>
        /// <param name="noHours"></param>
        /// <returns></returns>
        private double ComputeATYPOnus(int noStudents, double sharedUtility, int noHours)
        {
            double onus = 1 / (double)noStudents;  // zatez na vyuku jedne hodiny vsech zapsanych studentu
            onus *= sharedUtility;   // sdilena zatez
            onus *= noHours;    // pocet skutecne oducenych hodin (nesedi se STAGem)

            return onus;
        }

        /// <summary>
        /// Vrati maximalni velikost skupiny ATYP predmetu. 
        /// Vhodne pouze pro ATYPicke predmety!
        /// </summary>
        /// <param name="ra">Rozvrhova akce, ktera spada pod ATYP predmet.</param>
        /// <returns>Maximalni velikost skupiny dane akce. Muze byt i null!</returns>
        private int? GetGroupMaxSizeForATYP(RozvrhovaAkce ra)
        {
            switch (ra.TypAkceZkr)
            {
                case "Př":
                    return ra.PredmetRef.VelikostSkupinyPr;
                case "Cv":
                    return ra.PredmetRef.VelikostSkupinyCv;
                case "Se":
                    return ra.PredmetRef.VelikostSkupinySe;
                default:
                    Debug.WriteLine(string.Format("Nepodarilo se ziskat maximalni velikost skupiny ATYPu {0}/{1}", ra.Katedra, ra.Predmet));
                    return null;
            }
        }

        /// <summary>
        /// Vrati pocet skutecnych hodin vyucovanych na danem predmetu a tedy i rozvrhove akci, ktera pod nej spada. 
        /// Vhodne pouze pro ATYPicke predmety!
        /// </summary>
        /// <param name="ra">Rozvrhova akce jejiz pocet hodin chceme zjistit.</param>
        /// <returns>Pocet hodin.</returns>
        private int GetNoOfHoursForATYP(RozvrhovaAkce ra)
        {
            switch (ra.TypAkceZkr)
            {
                case "Př":
                    return ra.PredmetRef.HodinZaSemestrPr;
                case "Cv":
                    return ra.PredmetRef.HodinZaSemestrCv;
                case "Se":
                    return ra.PredmetRef.HodinZaSemestrSe;
                default:
                    Debug.WriteLine(string.Format("Nepodarilo se ziskat pocet hodin ATYPu {0}/{1}", ra.Katedra, ra.Predmet));
                    return 0;
            }
        }

        /// <summary>
        /// Vypocte a ulozi do vystupni tridy zatez na vyuku analyzovane SRA. 
        /// </summary>
        private void ComputeOnusDistribution()
        {
            // Budeme si prochazet vsechny rozvrhove akce v dane SRA
            foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
            {
                // Pro kazdou zapocatou hodinu vyuky
                foreach (int hour in roakIdnoToStartTimes[ra.RoakIdno])
                {
                    // se budeme divat, ktera katedra ma na vyuce podil
                    foreach (string dept in roakIdnoSharedUtility[ra.RoakIdno].Keys)
                    {
                        // a kazdemu studentovi zapiseme do zateze katedry na jeho vyuku tuto zatez
                        foreach (Student student in studentsOnRoakIdno[ra.RoakIdno])
                        {
                            int repetition = roakIdnoRepetition[ra.RoakIdno];   // kolikrat se behem semestru dana RA opakuje

                            double onus = 1 / (double)noStudentsOnSRAHour[hour];    // spocteme zatez na hodinu bez jakychkoli koeficientu
                            onus *= roakIdnoSharedUtility[ra.RoakIdno][dept];  // vynasobime podilem katedry (proto foreach pres vsechny katedry)
                            onus *= repetition;  // vynasobime poctem opakovani RA

                            onusDistribution.Pridat(student, dept, onus);   // a ulozime zateze
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Provede novou inicializaci vsech globalnich promennych v teto tride, ktere slouzi pro vypocet zateze 
        /// na vyuku konkretni SRA a to vcetne "zameny" samotne SRA, kterou budeme analyzovat.
        /// Netyka se tridy pro vypocet celkove zateze!
        /// </summary>
        /// <param name="sra">SRA, kterou chceme analyzovat.</param>
        private void CleanInit(SRA sra)
        {
            roakIdnoToStartTimes = new Dictionary<int, List<int>>();
            noStudentsOnSRAHour = new Dictionary<int, int>();
            roakIdnoSharedUtility = new Dictionary<int, Dictionary<string, double>>();
            roakIdnoRepetition = new Dictionary<int, int>();
            this.sra = sra;
        }

        /// <summary>
        /// Nastavi do globalni promenne pro kazde roakIdno seznam vyucovanych hodin.
        /// Pouziva se pritom poradove oznaceni hodiny, ktere je ve STAGu v zahlavi rozvrhu,
        /// tj. 1,2,3,...,15
        /// </summary>
        private void SetRoakIdnoToStartTimes()
        {
            //TODO: Muze se ve VnorenychAkcich objevit vicekrat RA se stejnym roakIdno?

            // Budu pouzivat hodnoty z VnoreneAkce
            foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
            {
                List<int> startTimes = new List<int>(); // seznam zacatku hodinove vyuky
                roakIdnoToStartTimes.Add(ra.RoakIdno, startTimes);  // priradime prazdny seznam k roakIdno

                // Naplnime seznam zacatku hodinove vyuky
                // Pokud neni rozvrhova akce definovana hodinami od a do, zaradim do slovniku pouze prazdny seznam pocatecnich hodin vyuky
                // Pozn.: Melo by se dit jen u ATYP akci
                if (ra.HodinaOd == null || ra.HodinaDo == null)
                {
                    Debug.WriteLine(string.Format("Rozvrhova akce s roakIdno={0} (predmet {1}/{2}) ma hodinuOd nebo hodinuDo null", ra.RoakIdno, ra.Katedra, ra.Predmet));
                    continue;   //TODO: Je takovato reakce dostatecna?
                }
                else
                {
                    for (int i = ra.HodinaOd.GetValueOrDefault(); i <= ra.HodinaDo.GetValueOrDefault(); i++)
                        startTimes.Add(i);
                }
            }
        }

        /// <summary>
        /// Nastavi do globalni promenne pro kazdou hodinu vyuky SRA, tj. i pro prekryvajici se vyuku,
        /// pocet studentu, kteri na ni chodi. Pouziva poradove hodiny ze zahlavi rozvrhu ze STAGu (1,2,..,15).
        /// </summary>
        private void SetNoStudentsOnSRAHour()
        {
            foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
            {
                List<int> startTimes = roakIdnoToStartTimes[ra.RoakIdno];
                foreach (int hour in startTimes)
                {
                    if (!noStudentsOnSRAHour.ContainsKey(hour)) // pokud jsme danou hodinu jeste nezapisovali, zapisem ji jako novy prvek
                    {
                        noStudentsOnSRAHour.Add(hour, ra.Obsazeni);
                    }
                    else
                    {
                        noStudentsOnSRAHour[hour] = noStudentsOnSRAHour[hour] + ra.Obsazeni; // jinak jen pricteme k puvodni hodnote
                    }
                }
            }
        }

        /// <summary>
        /// Nastavi do globalni promenne pro kazdou rozvrhovou akci v SRA rozdeleni zateze na vyuku mezi jednotlive katedry
        /// </summary>
        private void SetRoakIdnoSharedUtility()
        {
            // Pro kazdou rozvrhovou akci si zjistim jaky maji na vyuce podil jednotlive katedry (staci jen prekopirovat).
            foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
            {
                SetRoakIdnoSharedUtility(ra);
            }
        }

        /// <summary>
        /// Nastavi do globalni promenne pro zadanou rozvrhovou akci rozdeleni zateze na vyuku mezi jednotlive katedry.
        /// </summary>
        private void SetRoakIdnoSharedUtility(RozvrhovaAkce ra)
        {
            // Pro rozvrhovou akci si vytahnu jeji typ (Pr, Cv, Se) a podle toho zjistim
            // z predmetu, ke kteremu patri, jaky maji na vyuce podil jednotlive katedry (staci jen prekopirovat).
            switch (ra.TypAkceZkr)
            {
                case "Př":
                    roakIdnoSharedUtility.Add(ra.RoakIdno, ra.PredmetRef.PodilKatedryPrednaska);
                    break;
                case "Cv":
                    roakIdnoSharedUtility.Add(ra.RoakIdno, ra.PredmetRef.PodilKatedryCviceni);
                    break;
                case "Se":
                    roakIdnoSharedUtility.Add(ra.RoakIdno, ra.PredmetRef.PodilKatedrySeminar);
                    break;
                default:
                    //TODO: Predmety typu SZZ maji toto pole prazdne! Jak osetrit?
                    break;
            }

        }


        /// <summary>
        /// Nastavi do globalni promenne pro kazdou rozvrhovou akci v SRA pocet opakovani vyuky.
        /// </summary>
        private void SetRoakIdnoRepetition()
        {
            foreach (RozvrhovaAkce ra in sra.VnoreneAkce)
            {
                // spoctu pocet opakovani a vlozi do slovniku pod odpovidajici roakIdno
                int repetition = ra.TydenDo - ra.TydenOd + 1;   //TODO: Co delat v pripade, ze jsou obe hodnoty v RA nastaveny na 0?
                roakIdnoRepetition.Add(ra.RoakIdno, repetition);
            }
        }

        #endregion
    }
}
