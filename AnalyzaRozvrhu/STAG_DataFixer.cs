﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.IO;
using AnalyzaRozvrhu.STAG_Classes;
using System.Drawing;
using OfficeOpenXml.DataValidation;

namespace AnalyzaRozvrhu
{

    public static class STAG_DataFixer
    {

        /*
         * TODO
         * Generovat dotazniky kvuli podilu katedry
         * Generovat dotazniky kvuli skuktecne rozvrhovanosti
         */
        #region generovani dotazniku podilKateder
        class PredmetVDotazniku
        {
            public STAG_Classes.Predmet predmet;
            //public List<STAG_Classes.Ucitel> ucitele;
            public Dictionary<string, List<STAG_Classes.Ucitel>> ucitele = new Dictionary<string, List<Ucitel>>();
            public string typAkce;
            public void AddUcitel(STAG_Classes.Ucitel ucitel)
            {
                if (ucitele.ContainsKey(ucitel.Katedra))
                    ucitele[ucitel.Katedra].Add(ucitel);
                else
                    ucitele.Add(ucitel.Katedra, new List<Ucitel>() { ucitel });
            }
        }
        /// <summary>
        /// Tato metoda souluží k vygenerování dotazníku s podílem katedry na vyučovaném predmetu
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path">nazev souboru ktery vytvorime</param>
        public static void GenerovatDotaznikKatedramXLS(this STAG_Classes.STAG_Database data, string path)
        {
            Debug.WriteLine("Generovani dotazniku ......");
            Debug.WriteLine("Probiha hledani predmetu");
            List<PredmetVDotazniku> spolecnePredmety = new List<PredmetVDotazniku>();
            List<STAG_Classes.Ucitel> neco = new List<STAG_Classes.Ucitel>();
            // TODO

            // projdeme vsechny katedry
            foreach (var katedra in data.PredmetyPodleKateder)
            {
                // u kazde katedry projdeme vsechny predmety
                foreach (var predmet in katedra.Value)
                {

                    PredmetVDotazniku pr = null;
                    PredmetVDotazniku cv = null;
                    PredmetVDotazniku se = null;
                    // u kazdeho predmetu projdeme vsechny rozvrhove akce
                    // vyucujici zjistujeme z akci a ne z predmetu (zapocitavame pouze ucitele kteri predmet opravdu vyucuji)
                    foreach (var akce in predmet.Value.VsechnyAkce)
                    {


                        //seznam vsech ucitelu dane akce ktery vyhovuji podminkam ( nejsou zamestnanci katedry ktery predmet patri)
                        neco = (from ucitel in akce.VsichniUcitele where !(ucitel.Katedra == predmet.Value.Katedra || (ucitel.PracovisteDalsi != null && ucitel.PracovisteDalsi.ToString().Split(',').Contains(predmet.Value.Katedra))) select ucitel).ToList();

                        if (neco.Count != 0)
                        {
                            //kontrolujeme duplicity
                            switch (akce.TypAkceZkr)
                            {
                                case "Př":
                                    ZkontrolujAkci(neco, predmet, ref pr, akce);
                                    break;
                                case "Cv":
                                    ZkontrolujAkci(neco, predmet, ref cv, akce);
                                    break;
                                case "Se":
                                    ZkontrolujAkci(neco, predmet, ref se, akce);
                                    break;
                                default: throw new Exception("Nenalezen typ akce");
                            }
                            Debug.Write(string.Format("{0}/{1} => {2} \n", predmet.Value.Katedra, predmet.Value.Zkratka, akce.TypAkceZkr));
                        }
                    }
                    //pridani problemovych predmetu do seznamu
                    if (pr != null)
                        spolecnePredmety.Add(pr);
                    if (cv != null)
                        spolecnePredmety.Add(cv);
                    if (se != null)
                        spolecnePredmety.Add(se);
                }
            }
            Debug.WriteLine("Probiha vytvareni souboru");
            // generování excel souboru
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(path);
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Predmety");

                worksheet.Cells[1, 1].Value = "podíl pracoviště učitele \n(v procentech)";
                worksheet.Cells[1, 2].Value = "pracoviště učitelů";
                worksheet.Cells[1, 3].Value = "jména učitelů pracoviště";
                worksheet.Cells[1, 4].Value = "kód";
                worksheet.Cells[1, 5].Value = "název předmětu";
                worksheet.Cells[1, 6].Value = "typ";
                worksheet.Cells[1, 7].Value = "Semestr";
                worksheet.Cells["A1:G1"].AutoFilter = true;

                // seznam vsech kateder na fakulte
                var katedry = data.HiearchiePracovist[data.Fakulta];

                // kontrola validace zadávaných dat 
                var validace = worksheet.DataValidations.AddIntegerValidation("A:A"/*string.Format("A2:A{0}", spolecnePredmety.Count + 2)*/);
                validace.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                validace.PromptTitle = "Sem napiš % podíl na výuce";
                validace.Prompt = "Hodnota musí být od 0 do 100";
                validace.ShowInputMessage = true;
                validace.ErrorTitle = "Vložil/a si neplatnou hodnotu";
                validace.Error = "Hodnota musí být od 0 do 100.\nPokud omylem píšeš do špatné buňky tak vyplň nějaký platný údaj pak klikni na buňku a zmáčkni klávesu delete  ";
                validace.ShowErrorMessage = true;
                validace.Operator = ExcelDataValidationOperator.between;
                validace.Formula.Value = 0;
                validace.Formula2.Value = 100;


                StringBuilder sb;
                int j = 2;
                for (int i = 2; i < spolecnePredmety.Count + 2; i++)
                {
                    foreach (var ucitele in spolecnePredmety[i - 2].ucitele)
                    {
                        sb = new StringBuilder();
                        // vypsani 0 tam kde katedra ucitele nepatri mezi katedry PRF nebo je to UMC
                        if (!katedry.ContainsKey(ucitele.Key) || (ucitele.Key == "UMC" && data.Fakulta == "PRF"))
                            worksheet.Cells[j, 1].Value = 0;
                        //vypsani katedry
                        worksheet.Cells[j, 2].Value = ucitele.Key;
                        foreach (var ucitel in ucitele.Value)
                        {
                            sb.Append(string.Format("{0} {1}, ", ucitel.Prijmeni, ucitel.Jmeno));
                        }
                        worksheet.Cells[j, 3].Value = sb.ToString();
                        worksheet.Cells[j, 4].Value = string.Format("{0}/{1}", spolecnePredmety[i - 2].predmet.Katedra, spolecnePredmety[i - 2].predmet.Zkratka);
                        worksheet.Cells[j, 5].Value = spolecnePredmety[i - 2].predmet.Nazev;
                        worksheet.Cells[j, 6].Value = spolecnePredmety[i - 2].typAkce;
                        worksheet.Cells[j, 7].Value = spolecnePredmety[i - 2].predmet.VyukaZS && spolecnePredmety[i - 2].predmet.VyukaLS ? "ZS/LS" : spolecnePredmety[i - 2].predmet.VyukaZS ? "ZS" : "LS";
                        j++;
                    }
                }
                worksheet.Cells.AutoFitColumns(5);
                package.Save();
            }
            Debug.WriteLine("Dotaznik " + path + " vytvoren!");

        }

        private static void ZkontrolujAkci(List<STAG_Classes.Ucitel> neco, KeyValuePair<string, STAG_Classes.Predmet> predmet, ref PredmetVDotazniku predmetVDotazniku, STAG_Classes.RozvrhovaAkce akce)
        {
            if (predmetVDotazniku == null)
            {
                predmetVDotazniku = new PredmetVDotazniku()
                {
                    predmet = predmet.Value,
                    typAkce = akce.TypAkceZkr,
                    ucitele = new Dictionary<string, List<Ucitel>>()
                };
            }
            foreach (var ucitel in neco)
                if (!(predmetVDotazniku.ucitele.ContainsKey(ucitel.Katedra) && predmetVDotazniku.ucitele[ucitel.Katedra].Contains(ucitel)))
                    predmetVDotazniku.AddUcitel(ucitel);
        }
        #endregion
        
        #region Nacitani dotazniku

        /// <summary>
        /// Nacitani podilu kateder na vyucovanem predmetu z dotazniku
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void NacistDotaznikKatedramXLS(this STAG_Classes.STAG_Database data, string path)
        {
            // TODO
            // Nacte dotaznik, zkontroluje, upravi data         
            Debug.WriteLine("otevírám soubor " + path);
            FileInfo file = new FileInfo(path);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                STAG_Classes.Predmet predmet;
                ExcelWorksheets worksheets = package.Workbook.Worksheets;
                int row = 2; // Soubor zacneme cist od druheho radku. Na prvnim radku jsou popisky sloupců
                object cellData;
                List<Tuple<string, string>> ucitele;
                // projede vsechny listy v  xlsx souboru
                foreach (var worksheet in worksheets)
                {
                    // projde vsechny radky kde hodnota bunky v prvnim sloupci != null
                    while ((cellData = worksheet.Cells[row, 1].Value) != null)
                    {
                        // ziskani jmen ucitelu z dotazniku
                        var jmenaUcitelu = worksheet.Cells[row, 3].Value.ToString();
                        ucitele = ZiskejJmenaUcitelu(jmenaUcitelu);

                        //ziskani kodu predmetu z dotazniku
                        var kodPredmetu = worksheet.Cells[row, 4].Value.ToString().Split('/');
                        // nalezeni predmetu, pokud předmět neexistuje vyhodí vyjímku
                        try
                        {
                            predmet = data.PredmetyPodleKateder[kodPredmetu[0]][kodPredmetu[1]];
                        }
                        catch
                        {
                            throw new Exception(String.Format("Predmet {0}/{1} neexistuje", kodPredmetu[0], kodPredmetu[1]));
                        }
                        // sloupec ve kterem je typ predmetu pr/cv/se
                        string typ = worksheet.Cells[row, 6].Value.ToString().ToLower();

                        var katedraVyucujiciho = worksheet.Cells[row, 2].Value.ToString();
                        if (typ == "př")
                        {
                            // pokud slovnik neexistuje tak ho vytvorim
                            if (predmet.PodilKatedryPrednaska == null)
                                predmet.PodilKatedryPrednaska = new Dictionary<string, double>();
                            // pridani podilu katedry do slovniku

                            if (predmet.PodilKatedryPrednaska.ContainsKey(katedraVyucujiciho))
                                predmet.PodilKatedryPrednaska[katedraVyucujiciho] += Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0;
                            else
                                predmet.PodilKatedryPrednaska.Add(katedraVyucujiciho, Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
                        }
                        else
                        {
                            if (typ == "cv")
                            {
                                if (predmet.PodilKatedryCviceni == null)
                                    predmet.PodilKatedryCviceni = new Dictionary<string, double>();
                                if (predmet.PodilKatedryCviceni.ContainsKey(katedraVyucujiciho))
                                    predmet.PodilKatedryCviceni[katedraVyucujiciho] += Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0;
                                else
                                    predmet.PodilKatedryCviceni.Add(katedraVyucujiciho, Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
                            }
                            else
                            {
                                if (typ == "se")
                                {
                                    if (predmet.PodilKatedrySeminar == null)
                                        predmet.PodilKatedrySeminar = new Dictionary<string, double>();
                                    if (predmet.PodilKatedrySeminar.ContainsKey(katedraVyucujiciho))
                                        predmet.PodilKatedrySeminar[katedraVyucujiciho] += Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0;
                                    else
                                        predmet.PodilKatedrySeminar.Add(katedraVyucujiciho, Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
                                }
                                else
                                    throw new Exception("Neni uvedena informace zda jde o Př/Se/Cv");
                            }
                        }
                        row++;
                    }
                }
                Debug.WriteLine("Nacteni dotazniku hotovo");
            }
            Debug.WriteLine("Doplneni podilu kateder");
            double podil = 0;
            foreach (var katedra in data.PredmetyPodleKateder)
            {
                foreach (var predmet in katedra.Value)
                {
                    podil = 0;
                    // podil katedry na prednasce
                    var podilKatedry = predmet.Value.PodilKatedryPrednaska;
                    DoplnPodilKatedry(ref podil, predmet, ref podilKatedry);
                    predmet.Value.PodilKatedryPrednaska = podilKatedry;

                    // podil katedry na cviceni
                    podilKatedry = predmet.Value.PodilKatedryCviceni;
                    DoplnPodilKatedry(ref podil, predmet, ref podilKatedry);
                    predmet.Value.PodilKatedryCviceni = podilKatedry;
                    // podil katedry na cviceni
                    podilKatedry = predmet.Value.PodilKatedrySeminar;
                    DoplnPodilKatedry(ref podil, predmet, ref podilKatedry);
                    predmet.Value.PodilKatedrySeminar = podilKatedry;

                    //myslim ze nazev metody je dostatecny
                    DoplnPocetHodinZaSemestr(predmet.Value);
                }
            }
            Debug.WriteLine("Doplneni hotovo");
        }


        /// <summary>
        /// Tato metoda vypocita na zeklade jednotek za semestr/tyden pocet vyucovanych hodyn za semestr
        /// </summary>
        /// <param name="predmet">Predmet</param>
        private static void DoplnPocetHodinZaSemestr(Predmet predmet)
        {
            if (predmet.JednotkaPrednasky == "HOD/SEM")
                predmet.HodinZaSemestrPr = predmet.JednotekPrednasek;
            else
               if (predmet.JednotkaPrednasky == "HOD/TYD" && predmet.VsechnyAkce.Count != 0)
                predmet.HodinZaSemestrPr = (predmet.VsechnyAkce[0].TydenDo - predmet.VsechnyAkce[0].TydenOd) * predmet.JednotekPrednasek;

            if (predmet.JednotkaCviceni == "HOD/SEM")
                predmet.HodinZaSemestrCv = predmet.JednotekCviceni;
            else
               if (predmet.JednotkaPrednasky == "HOD/TYD" && predmet.VsechnyAkce.Count != 0)
                predmet.HodinZaSemestrCv = (predmet.VsechnyAkce[0].TydenDo - predmet.VsechnyAkce[0].TydenOd) * predmet.JednotekCviceni;

            if (predmet.JednotkaSeminare == "HOD/SEM")
                predmet.HodinZaSemestrSe = predmet.JednotekSeminare;
            else
               if (predmet.JednotkaPrednasky == "HOD/TYD" && predmet.VsechnyAkce.Count != 0)
                predmet.HodinZaSemestrSe = (predmet.VsechnyAkce[0].TydenDo - predmet.VsechnyAkce[0].TydenOd) * predmet.JednotekSeminare;
        }



        /// <summary>
        /// Spocita % podil vyuky predmetu pokud je podil na vyuce mensi nez 1 tak doplni
        /// </summary>
        /// <param name="podil"> asi zbitecna promena v pozdejsi verzi ji odstranim :) </param>
        /// <param name="predmet">predmet </param>
        /// <param name="podilKatedry"> odkaz na slovnik s podily kateder na vyuce predmetu</param>
        private static void DoplnPodilKatedry(ref double podil, KeyValuePair<string, STAG_Classes.Predmet> predmet, ref Dictionary<string, double> podilKatedry)
        {
            if (podilKatedry == null)
            {
                podilKatedry = new Dictionary<string, double>() { { predmet.Value.Katedra, 1 } };
            }
            else
            {
                podil = podilKatedry.Values.Sum();
                if (podil < 1)
                    podilKatedry.Add(predmet.Value.Katedra, 1 - podil);
                if (podil > 1)
                    throw new Exception("Podil na vyuce predmetu je vetsi nez 100% ");
            }
        }


        /// <summary>
        /// Rozparsruje string na jmena jednotlivych ucitelu a prida je do seznamu
        /// </summary>
        /// <param name="jmenaUcitelu">jmena učitelů může obsahovat libovolný počet učitelů, jména jsou oddělena čárkou. </param>
        /// <returns></returns>
        private static List<Tuple<string, string>> ZiskejJmenaUcitelu(string jmenaUcitelu)
        {
            List<Tuple<string, string>> ucitele = new List<Tuple<string, string>>();
            if (jmenaUcitelu.Contains(','))
            {
                var tmpUcitel = jmenaUcitelu.Split(',');
                foreach (var uc in tmpUcitel)
                {
                    ZiskejJmenoUcitele(ucitele, uc);
                }
            }
            else
            {
                ZiskejJmenoUcitele(ucitele, jmenaUcitelu);
            }

            return ucitele;
        }

        /// <summary>
        /// Rozděli jmeno ucitele na jmeno a prijmeni
        /// </summary>
        /// <param name="ucitele"> seznam do ktereho se jmena maji ukladat</param>
        /// <param name="uc"> string ve kterem je jmeno a prijmeni ucitele (oddělovač je mezera např. "Frantisek Oplt")</param>
        private static void ZiskejJmenoUcitele(List<Tuple<string, string>> ucitele, string uc)
        {
            var tmpUc = uc.Split(' ');
            if (tmpUc.Count() == 1)
            {
                // a zase ta KBI
                ucitele.Add(new Tuple<string, string>(tmpUc[0], ""));
                return;
            }
            if (tmpUc.Count() == 3) // pokud je mezi carkou a jmenem mezera tak count == 3
                ucitele.Add(new Tuple<string, string>(tmpUc[1], tmpUc[2]));
            else
                ucitele.Add(new Tuple<string, string>(tmpUc[0], tmpUc[1]));
        }
        #endregion
        
        // Dodelat metody pro generovani dotazniku kvuli ATYP předmětům
        #region vytvareni dotazniku ATYP
        public static void GenerovatDotaznikAtypXLS(this STAG_Classes.STAG_Database data, string path)
        {
            List<STAG_Classes.Predmet> podezrelePredmety = new List<Predmet>();
            foreach (var katedra in data.PredmetyPodleKateder)
            {
                foreach (var predmet in katedra.Value)
                {

                    if (predmet.Value.VsechnyAkce.Count > 0)
                    {

                        if (predmet.Value.JednotkaCviceni != "HOD/TYD" && predmet.Value.JednotkaCviceni != "HOD/SEM" && predmet.Value.JednotkaCviceni != "DNU/SEM")
                        {
                            if (!podezrelePredmety.Contains(predmet.Value))
                                podezrelePredmety.Add(predmet.Value);
                        }
                        if (predmet.Value.JednotkaPrednasky != "HOD/TYD" && predmet.Value.JednotkaPrednasky != "HOD/SEM" && predmet.Value.JednotkaPrednasky != "DNU/SEM")
                        {
                            if (!podezrelePredmety.Contains(predmet.Value))
                                podezrelePredmety.Add(predmet.Value);
                        }
                        if (predmet.Value.JednotkaSeminare != "HOD/TYD" && predmet.Value.JednotkaSeminare != "HOD/SEM" && predmet.Value.JednotkaSeminare != "DNU/SEM")
                        {
                            if (!podezrelePredmety.Contains(predmet.Value))
                                podezrelePredmety.Add(predmet.Value);
                        }

                    }
                }
            }
            Debug.WriteLine("neco");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(path);
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Predmety");

                worksheet.Cells[1, 1].Value = "kód";
                worksheet.Cells[1, 2].Value = "Název předmětu";
                worksheet.Cells[1, 3].Value = "typ";
                worksheet.Cells[1, 4].Value = "kód";
                worksheet.Cells[1, 5].Value = "Rozdah hodin ve STAGu";
                worksheet.Cells[1, 6].Value = "skutečný rozsah v HOD/SEM";
                worksheet.Cells[1, 7].Value = "maximální velikost skupiny";
                worksheet.Cells["A1:G1"].AutoFilter = true;


                for (int i = 2; i < podezrelePredmety.Count + 2; i++)
                {
                    worksheet.Cells[i, 1].Value = string.Format("{0}/{1}", podezrelePredmety[i - 2].Katedra, podezrelePredmety[i - 2].Zkratka);
                    worksheet.Cells[i, 2].Value = podezrelePredmety[i - 2].Nazev;


                }
                package.Save();
            }

        }
        #endregion


        #region nacitani dotazniku ATYP
        /// <summary>
        /// Nacitani dotazniku s ATYP predmety
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void NacistDotazniAtypPredmety(this STAG_Classes.STAG_Database data, string path)
        {
            List<Tuple<string, string>> chybnePredmety = new List<Tuple<string, string>>();
            Debug.WriteLine("otevírám soubor " + path);
            FileInfo file = new FileInfo(path);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                STAG_Classes.Predmet predmet;
                ExcelWorksheets worksheets = package.Workbook.Worksheets;
                int row = 2; // Soubor zacneme cist od druheho radku. Na prvnim radku jsou popisky sloupců
                object kodPredmetu;
                foreach (var worksheet in worksheets)
                {
                    while ((kodPredmetu = worksheet.Cells[row, 1].Value) != null)
                    {
                        //nacteni kodu predmetu
                        var kp = kodPredmetu.ToString().Split('/');
                        // typ akce pr/cv/se/null  ..... null je jenom v případě expertů z KBI 
                        var typ = worksheet.Cells[row, 3].Value as string;

                        try
                        {
                            // pokus o nalezeni predmetu
                            predmet = data.PredmetyPodleKateder[kp[0]][kp[1]];
                        }
                        catch
                        {
                            throw new Exception(String.Format("Predmet {0}/{1} neexistuje", kp[0], kp[1]));
                        }

                        switch (typ)
                        {
                            case "Př":
                                predmet.HodinZaSemestrPr = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                if (worksheet.Cells[row, 6].Value != null)
                                    predmet.VelikostSkupinyPr = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                predmet.IsAtypical = true;
                                break;
                            case "Cv":
                                predmet.HodinZaSemestrCv = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                if (worksheet.Cells[row, 6].Value != null)
                                    predmet.VelikostSkupinyCv = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                predmet.IsAtypical = true;
                                break;
                            case "Se":
                                predmet.HodinZaSemestrSe = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                if (worksheet.Cells[row, 6].Value != null)
                                    predmet.VelikostSkupinySe = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                predmet.IsAtypical = true;
                                break;
                            default:
                                /*
                                
                                // následující řádky kódu tu jsou hlavně proto, že KBI neumí vyplnit dotazník!!! 


                                */
                                var neco = (from akce in predmet.VsechnyAkce select akce.TypAkceZkr).ToList(); // vytvoreni seznamu vsech typu rozvrhovych akci predmetu
                                bool stejneAkce = true;

                                if (neco.Count > 1)
                                {   // kontrola jestli jsou vsechny akce stejneho typu
                                    for (int i = 1; i < neco.Count; i++)
                                    {
                                        if (neco[i - 1] != neco[i] /*&& neco[i] != "" && neco[i-1] != ""*/)
                                        {
                                            stejneAkce = false;
                                            break;
                                        }
                                    }
                                }
                                // pokud jsou vsechny akce stejneho typu provede se ulozeni dat z dotazniku ....... :( nemám rád KBI
                                if (stejneAkce)
                                {
                                    Debug.Write(string.Format("[Chyba ve vstupnim souboru] {0}/{1} =>  pocet akci -> {2}", kp[0], kp[1], predmet.VsechnyAkce.Count()));
                                    Debug.WriteLine("  Povedlo se opravit");
                                    switch (neco[0])
                                    {
                                        case "Př":
                                            predmet.HodinZaSemestrPr = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                            if (worksheet.Cells[row, 6].Value != null)
                                                predmet.VelikostSkupinyPr = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                            predmet.IsAtypical = true;
                                            break;
                                        case "Cv":
                                            predmet.HodinZaSemestrCv = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                            if (worksheet.Cells[row, 6].Value != null)
                                                predmet.VelikostSkupinyCv = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                            predmet.IsAtypical = true;
                                            break;
                                        case "Se":
                                            predmet.HodinZaSemestrSe = Convert.ToInt32(worksheet.Cells[row, 5].Value);
                                            if (worksheet.Cells[row, 6].Value != null)
                                                predmet.VelikostSkupinySe = Convert.ToInt32(worksheet.Cells[row, 6].Value);
                                            predmet.IsAtypical = true;
                                            break;
                                        default: throw new Exception("WTF!");
                                    }
                                }
                                else
                                {
                                    // pokud nejsou tak ..............
                                    Debug.Write(string.Format("[Chyba ve vstupnim souboru] {0}/{1} =>  pocet akci -> {2}", kp[0], kp[1], predmet.VsechnyAkce.Count()));
                                    Debug.WriteLine(" Nepovedlo se opravit");
                                    chybnePredmety.Add(new Tuple<string, string>(kp[0], kp[1]));
                                }
                                break;
                        }
                        row++;
                    }
                }
            }
            if (chybnePredmety.Count != 0)
                throw new STAG_Exception_InvalidTypeOfCourses(chybnePredmety);
        }
        #endregion
    }
}
