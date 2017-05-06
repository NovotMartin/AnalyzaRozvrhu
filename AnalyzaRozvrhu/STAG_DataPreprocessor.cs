using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.IO;
using AnalyzaRozvrhu.STAG_Classes;

namespace AnalyzaRozvrhu
{
    /// <summary>
    /// Třída, která zpracuje rozvrhové akce do SRA
    /// </summary>
    public static class STAG_DataPreprocessor
    {
        /// <summary>
        /// Hlavní funkce, která zpracuje rozvrhové akce do SRA
        /// </summary>
        /// <param name="data">STAG databáze</param>
        public static void Preprocess(this STAG_Database data)
        {
            Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>> zimakRozdelene =
                new Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>>();
            Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>> letnakRozdelene =
                new Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>>();

            Dictionary<string, List<string>> zimakBudovyMistnosti = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> letnakBudovyMistnosti = new Dictionary<string, List<string>>();

            Debug.WriteLine("---- Zacatek Preprocess debug vypisu ----");
            foreach (var dvojce in data.Akce)
            {
                var akce = dvojce.Value;
                switch (akce.Semestr)
                {
                    case ("ZS"):
                        if (akce.DenZkr != null && akce.Budova != null && akce.Mistnost != null)
                        {
                            RozdelAkce(akce, ref zimakRozdelene, ref zimakBudovyMistnosti);
                        }
                        else
                        {
                            //Debug.WriteLine("Akce bez dne/budovy/mistnosti (ZS): "+akce.Katedra+"/"+ akce.Predmet);
                            PridejSRADoDatabaze(akce, ref data);
                        }
                        break;
                    case ("LS"):
                        if (akce.DenZkr != null && akce.Budova != null && akce.Mistnost != null)
                        {
                            RozdelAkce(akce, ref letnakRozdelene, ref letnakBudovyMistnosti);
                        }
                        else
                        {
                            //Debug.WriteLine("Akce bez dne/budovy/mistnosti (LS): " + akce.Katedra + "/" + akce.Predmet);
                            PridejSRADoDatabaze(akce, ref data);
                        }
                        break;
                    default:
                        Debug.WriteLine("!!! Akce bez semestru (0S): " + akce.Katedra + "/" + akce.Predmet);
                        //TODO by asi nemělo nastat takže jinak asi vyhodit nějakou chybu
                        break;
                }
            }

            VytvorSuperRozvrhoveAkce(zimakRozdelene, zimakBudovyMistnosti, ref data);
            VytvorSuperRozvrhoveAkce(letnakRozdelene, letnakBudovyMistnosti, ref data);


            Debug.WriteLine("---- Konec Preprocess debug vypisu ----");
        }

        /// <summary>
        /// Funkce, rozdělí rozvrhovou akci do slovníku podle budovy,místnosti a dne ve kterém se koná
        /// </summary>
        private static void RozdelAkce(
            RozvrhovaAkce akce,
            ref Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>> rozdelene,
            ref Dictionary<string, List<string>> budovyMistnosti)
        {
            var denBudMist = new Tuple<string, string, string>(akce.DenZkr, akce.Budova, akce.Mistnost);
            if (!rozdelene.ContainsKey(denBudMist))
            {
                rozdelene.Add(denBudMist, new List<NormalizovanaAkce>());
            }
            rozdelene[denBudMist].Add(new NormalizovanaAkce(akce));

            if (!budovyMistnosti.ContainsKey(akce.Budova))
            {
                budovyMistnosti.Add(akce.Budova, new List<string>());
            }
            if (!budovyMistnosti[akce.Budova].Contains(akce.Mistnost))
                budovyMistnosti[akce.Budova].Add(akce.Mistnost);
        }

        /// <summary>
        /// Funkce, která zpracuje všechny a propojí akce, které probíhají společně
        /// </summary>
        private static void VytvorSuperRozvrhoveAkce(
            Dictionary<Tuple<string, string, string>, List<NormalizovanaAkce>> rozdeleneAkce,
            Dictionary<string, List<string>> budovyMistnosti,
            ref STAG_Database data)
        {
            List<string> dny = new List<string>() { "Po", "Út", "St", "Čt", "Pá", "So", "Ne" }; //vytvoření Listu pro projetí dnů

            //TextWriter tw = new StreamWriter("kontrola_pakci.txt"); 

            foreach (var budova in budovyMistnosti.Keys) //budovy
            {
                foreach (var mistnost in budovyMistnosti[budova]) //místnosti v budově
                {
                    foreach (var den in dny) //den v týdnu
                    {
                        var denBudovaMistnost = new Tuple<string, string, string>(den, budova, mistnost); //vytvoření trojce - budova-místnost-den
                        if (!rozdeleneAkce.ContainsKey(denBudovaMistnost)) break; //zjištení jestli existuje toto spojení
                        var normAkce = rozdeleneAkce[denBudovaMistnost]; //získání akcí v dané budově, místnosti a dnu
                        List<List<NormalizovanaAkce>> listPodezdrelichAkci = new List<List<NormalizovanaAkce>>() { }; //vyvoření listu pro spol rozvrhové akce pro pozdější zpracování
                        //tw.WriteLine(budova + " " + mistnost + " - " + den);
                        //tw.WriteLine("--- Všechny akce ---");
                        foreach (var nAkce in normAkce) //všechny akce v dané budově-místnosti-dni
                        {
                            //tw.WriteLine(nAkce.ToString());
                            bool jePouzita = false;
                            foreach (var podezdreleAkce in listPodezdrelichAkci)
                            {
                                foreach (var pAkce in podezdreleAkce)
                                {
                                    if (nAkce.NormZacatek >= pAkce.NormZacatek && nAkce.NormKonec <= pAkce.NormKonec 
                                        && nAkce.Tyden == pAkce.Tyden && nAkce.TydenOd >= pAkce.TydenOd && nAkce.TydenDo <= pAkce.TydenDo) //kontrola, zda li patří k již roztříděné akci
                                    {
                                        podezdreleAkce.Add(nAkce);
                                        jePouzita = true;
                                        break;
                                    }
                                }
                                if (jePouzita) break;
                            }
                            if (!jePouzita)
                            {
                                listPodezdrelichAkci.Add(new List<NormalizovanaAkce>() { nAkce }); //nepatřila k žádné jiné akci tak dostane vlastní list
                            }
                        }
                        //tw.WriteLine("--- Podezdřelé akce ---");
                        //int pocet = 1;
                        foreach (var podezdeleAkce in listPodezdrelichAkci) //zpracování roztříděný akcí do SRA
                        {
                            PridejSRADoDatabaze(podezdeleAkce, ref data);
                            /*
                            tw.WriteLine("--- List " + pocet + " ---");
                            foreach (var pAkce in podezdeleAkce)
                            {
                                tw.WriteLine(pAkce.ToString());
                            }
                            pocet++;
                            */
                        }
                        //tw.WriteLine("--------------------------------------");
                        //tw.WriteLine();
                    }
                }
            }
            //tw.Flush();
            //tw.Close();
        }

        /// <summary>
        /// Přidá jednu akci jako jednu SRA
        /// </summary>
        private static void PridejSRADoDatabaze(RozvrhovaAkce akce, ref STAG_Database data)
        {
            data.SuperRozvrhoveAkce.Add(new SRA(akce));
        }

        /// <summary>
        /// Přidá list akcí jako jednu SRA
        /// </summary>
        private static void PridejSRADoDatabaze(List<NormalizovanaAkce> listAkci, ref STAG_Database data)
        {
            List<RozvrhovaAkce> rAkce = new List<RozvrhovaAkce>();
            foreach (var akce in listAkci)
            {
                rAkce.Add(akce.Akce);
            }
            data.SuperRozvrhoveAkce.Add(new SRA(rAkce));
        }
    }

    /// <summary>
    /// Pomocná třída pro zjednodušení hledání průniků
    /// </summary>
    public class NormalizovanaAkce
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zacatek"></param>
        /// <param name="konec"></param>
        public NormalizovanaAkce(int zacatek, int konec) //pouze pro testování
        {
            NormZacatek = zacatek;
            NormKonec = konec;
            Akce = null;
        }

        /// <summary>
        /// Vytvoří z rozvrhové akce normalizovanou akci
        /// </summary>
        /// <param name="akce">Rozvrhová akce</param>
        public NormalizovanaAkce(RozvrhovaAkce akce)
        {
            this.Akce = akce;
            Tyden = akce.TydenZkr;
            TydenOd = akce.TydenOd;
            TydenDo = akce.TydenDo;
            if (!(akce.HodinaOd == null || akce.HodinaDo == null))
            {
                NormZacatek = akce.HodinaOd.GetValueOrDefault();
                NormKonec = akce.HodinaDo.GetValueOrDefault();
            }
            else
            {
                var hodinaZac = akce.HodinaSkutOd.Hour;
                var minutaZac = akce.HodinaSkutOd.Minute;
                var hodinaKon = akce.HodinaSkutDo.Hour;
                var minutaKon = akce.HodinaSkutDo.Minute;

                if (minutaZac == 0 && minutaKon == 50) //když je jedná o normální akci začátek v celou a konec v 50 ale nemá to vyplněné
                {
                    NormZacatek = hodinaZac - 6;
                    NormKonec = hodinaKon - 6;
                    akce.HodinaOd = NormZacatek;
                    akce.HodinaDo = NormKonec;
                }
                else
                {
                    if (minutaZac != 0)
                    {
                        hodinaZac++;
                    }

                    if (minutaKon == 0)
                    {
                        hodinaKon--;
                    }
                    if (minutaKon > 50 && minutaKon <= 59)
                    {
                        hodinaKon++;
                    }
                    
                    NormZacatek = hodinaZac - 6;
                    NormKonec = hodinaKon - 6;
                    //Debug.WriteLine("["+akce.Katedra + "/" + akce.Predmet+"] "+akce.HodinaSkutOd.Hour +":"+akce.HodinaSkutOd.Minute+" - "+akce.HodinaSkutDo.Hour+":"+akce.HodinaSkutDo.Minute+" >> "+normZacatek+" - "+normKonec);
                }

            }
        }

        /// <summary>
        /// Vrátí normalizovaný začátek podle STAG rozvrhu (např. 8:00 je 2 hodina)
        /// </summary>
        public int NormZacatek { get; }

        /// <summary>
        /// Vrátí normalizovaný konec podle STAG rozvrhu (např. 8:50 je 2 hodina)
        /// </summary>
        public int NormKonec { get; }

        /// <summary>
        /// Vrátí v jakém týdnu výuka probíhá ("K"aždý,"S"udý,"L"ichý,"Jiný")
        /// </summary>
        public string Tyden { get; }

        /// <summary>
        /// Vrátí od jakého týdnu výuka probíhá
        /// </summary>
        public int TydenOd { get; }

        /// <summary>
        /// Vrátí do jakého týdnu výuka probíhá
        /// </summary>
        public int TydenDo { get; }

        /// <summary>
        /// Vrátí rozvrhovou akci ze které vychází
        /// </summary>
        public RozvrhovaAkce Akce { get; }

        /// <summary>
        /// Výpis pro testování
        /// </summary>
        public override string ToString()
        {
            return "[" + Akce.Budova + " " + Akce.Mistnost + "] [" + Akce.DenZkr + " " + NormZacatek + " - " + NormKonec + " || " + Akce.TydenZkr + " " + Akce.TydenOd + " - " + Akce.TydenDo + "] "+Akce.Katedra+"/"+ Akce.Predmet;
        }
    }
}
