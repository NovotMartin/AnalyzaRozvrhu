using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Diagnostics;
using System.IO;

namespace AnalyzaRozvrhu
{
    public static class STAG_DataFixer
    {
       /* Dictionary<string, int> podilKatedryPrednaska;
        Dictionary<string, int> podilKatedryCviceni;
        Dictionary<string, int> podilKatedrySeminar;
        /*
         * TODO
         * Generovat dotazniky kvuli podilu katedry
         * Generovat dotazniky kvuli skuktecne rozvrhovanosti
         */

        public static void GenerovatDotaznikKatedramXLS(this STAG_Classes.STAG_Database data, string path)
        {

            // TODO
            // Ulozi dotaznik do Excel dokumentu
        }
#region Nacitani dotazniku
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
                foreach(var worksheet in worksheets)
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
                        if (typ == "př")
                        {
                            // pokud slovnik neexistuje tak ho vytvorim
                            if (predmet.PodilKatedryPrednaska == null)
                                predmet.PodilKatedryPrednaska = new Dictionary<string, double>();
                            // pridani podilu katedry do slovniku
                            predmet.PodilKatedryPrednaska.Add(worksheet.Cells[row, 2].Value.ToString(), Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
                        }
                        else
                        {
                            if (typ == "cv")
                            {
                                if (predmet.PodilKatedryCviceni == null)
                                    predmet.PodilKatedryCviceni = new Dictionary<string, double>();
                                predmet.PodilKatedryCviceni.Add(worksheet.Cells[row, 2].Value.ToString(), Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
                            }
                            else
                            {
                                if (typ == "se")
                                {
                                    if (predmet.PodilKatedrySeminar == null)
                                        predmet.PodilKatedrySeminar = new Dictionary<string, double>();
                                    predmet.PodilKatedrySeminar.Add(worksheet.Cells[row, 2].Value.ToString(), Convert.ToInt32(worksheet.Cells[row, 1].Value) / 100.0);
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
                    DoplnPodilKatedry(ref podil, predmet,ref podilKatedry);
                    predmet.Value.PodilKatedryPrednaska = podilKatedry;

                    // podil katedry na cviceni
                    podilKatedry = predmet.Value.PodilKatedryCviceni;
                    DoplnPodilKatedry(ref podil, predmet,ref podilKatedry);
                    predmet.Value.PodilKatedryCviceni = podilKatedry;
                    // podil katedry na cviceni
                    podilKatedry = predmet.Value.PodilKatedrySeminar;
                    DoplnPodilKatedry(ref podil, predmet,ref podilKatedry);
                    predmet.Value.PodilKatedrySeminar = podilKatedry;

                }
            }
            Debug.WriteLine("Doplneni hotovo");
        }

        /// <summary>
        /// Spocita % podil vyuky predmetu pokud je podil na vyuce mensi nez 1 tak doplni
        /// </summary>
        /// <param name="podil"> asi zbitecna promena v pozdejsi verzi ji odstranim :) </param>
        /// <param name="predmet">predmet </param>
        /// <param name="podilKatedry"> odkaz na slovnik s podily kateder na vyuce predmetu</param>
        private static void DoplnPodilKatedry(ref double podil, KeyValuePair<string, STAG_Classes.Predmet> predmet,ref Dictionary<string, double> podilKatedry)
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
            if(tmpUc.Count() == 3) // pokud je mezi carkou a jmenem mezera tak count == 3
                ucitele.Add(new Tuple<string, string>(tmpUc[1], tmpUc[2]));
            else
                ucitele.Add(new Tuple<string, string>(tmpUc[0], tmpUc[1]));
        }
#endregion
        // Dodelat metody pro generovani dotazniku kvuli ATYP předmětům
        public static void  NacistDotazniAtypPredmety(this STAG_Classes.STAG_Database data, string path)
        {

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
                    while((kodPredmetu = worksheet.Cells[row, 1].Value) != null)
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
                            case "Př": predmet.HodinZaSemestrPr = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                            case "Cv": predmet.HodinZaSemestrCv = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                            case "Se": predmet.HodinZaSemestrSe = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                            default:
                                Debug.Write(string.Format("{0}/{1} =>  pocet akci -> {2}", kp[0], kp[1], predmet.VsechnyAkce.Count()));
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
                                    Debug.WriteLine(" OK");
                                    switch (neco[0])
                                    {
                                        case "Př": predmet.HodinZaSemestrPr = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                                        case "Cv": predmet.HodinZaSemestrCv = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                                        case "Se": predmet.HodinZaSemestrSe = Convert.ToInt32(worksheet.Cells[row, 5].Value); break;
                                        default: throw new Exception("WTF!");
                                    }
                                }
                                else
                                {
                                    // pokud nejsou tak ..............
                                    Debug.WriteLine(" Chyba");
                                }
                                break;
                        }
                        row++;

                    }                                                     
                }
            }
        }
    }
}
