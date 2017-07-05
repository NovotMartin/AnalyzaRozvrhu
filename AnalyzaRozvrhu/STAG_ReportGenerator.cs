using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    public static class STAG_ReportGenerator
    {
        public static void GenerovatPrehledXLS(this STAG_Classes.STAG_Database data, string path)
        {
            // Kramář W.I.P.

            FileInfo file = new FileInfo(path);

            if (file.Exists)
                file.Delete();

            using (var excel = new ExcelPackage(file))
            {
                ExcelWorksheet sheet;
                for (int j = 0; j < 3; j++)
                {
                    if (j == 0)
                        sheet = excel.Workbook.Worksheets.Add("ZS");
                    else
                        if (j == 1)
                        sheet = excel.Workbook.Worksheets.Add("LS");
                    else
                        sheet = excel.Workbook.Worksheets.Add("ZS+LS");

                    sheet.Cells[1, 1].Value = "Osobni cislo";
                    sheet.Cells[1, 2].Value = "Rocnik";
                    sheet.Cells[1, 3].Value = "St. program";
                    sheet.Cells[1, 4].Value = "Forma";


                    var katedry = data.HiearchiePracovist[data.Fakulta];
                    for (int i = 0; i < katedry.Count; i++)
                    {
                        sheet.Cells[1, i + 5].Value = katedry.Keys.ElementAt(i);
                    }
                    sheet.Cells[1, katedry.Count + 5].Value = "jiná";
                    // st.program
                    // forma
                    // foreach (katedry)
                    // debug - vsechny predmety studenta

                    int row = 2;
                    Dictionary<string, double> podiKatedry;
                    foreach (var student in data.Students)
                    {
                        sheet.Cells[row, 1].Value = student.OsCislo;
                        sheet.Cells[row, 2].Value = student.Rocnik;
                        sheet.Cells[row, 3].Value = student.KodSp;
                        sheet.Cells[row, 4].Value = student.FormaSp;
                        if (j == 0)
                            podiKatedry = student.PodilKatedryZS;
                        else
                            if (j == 1)
                                     podiKatedry = student.PodilKatedryLS;
                            else
                                     podiKatedry = student.PodilKatedry;

                        for (int i = 0; i < katedry.Count; i++)
                        {
                            if (podiKatedry.ContainsKey(katedry.Keys.ElementAt(i)))
                                sheet.Cells[row, i + 5].Value = Math.Round(podiKatedry[katedry.Keys.ElementAt(i)], 2);
                            else
                                sheet.Cells[row, i + 5].Value = 0;
                        }
                        sheet.Cells[row, katedry.Count + 5].Value = (from x in podiKatedry where !katedry.Keys.Contains(x.Key) select x.Value).Sum();
                        row++;
                    }
                    for (int i = 1; i <= 4; i++)
                    {
                        ExcelColumn col = sheet.Column(i);
                        col.AutoFit();
                        col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                }
                excel.Save();
            }
        }

        /// <summary>
        /// Vygenerovani prehledu zateze ziskane analyzou SRA.
        /// Vystupem je xlsx soubor.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name">Nazev souboru bez pripony!</param>
        public static void GenerovatSRAPrehledXLS(this STAG_Classes.STAG_Database data, string name)
        {
            FileInfo file;
            int number = 2;
            if ((file = new FileInfo(name + ".xlsx")).Exists)
                while ((file = new FileInfo(name + number + ".xlsx")).Exists)
                    number++;

            using (var excel = new ExcelPackage(file))
            {
                ExcelWorksheet sheet;

                var deprts = data.HiearchiePracovist[data.Fakulta];
                double per;
                int row = 2;
                int col = 5;

                sheet = excel.Workbook.Worksheets.Add("sheet");
                sheet.Cells[1, 1].Value = "Osobni cislo";
                sheet.Cells[1, 2].Value = "Rocnik";
                sheet.Cells[1, 3].Value = "St. program";
                sheet.Cells[1, 4].Value = "Forma";
                foreach (var dep in deprts)
                    sheet.Cells[1, col++].Value = dep.Key;
                sheet.Cells[1, col].Value = "jiná";

                foreach (var student in data.zatezNaStudenta.GetAll())
                {
                    sheet.Cells[row, 1].Value = student.Item1.OsCislo;
                    sheet.Cells[row, 2].Value = student.Item1.Rocnik;
                    sheet.Cells[row, 3].Value = student.Item1.KodSp;
                    sheet.Cells[row, 4].Value = student.Item1.FormaSp;
                    col = 5;
                    foreach (var dep in deprts)
                    {
                        if (student.Item2.TryGetValue(dep.Key, out per))
                            sheet.Cells[row, col++].Value = per;
                        else
                            sheet.Cells[row, col++].Value = 0;
                    }
                    sheet.Cells[row, col].Value = (from cizi in student.Item2 where !deprts.Keys.Contains(cizi.Key) select cizi.Value).Sum();
                    row++;
                }
                for (int i = 1; i <= 4; i++)
                {
                    ExcelColumn coll = sheet.Column(i);
                    coll.AutoFit();
                    coll.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                excel.Save();
            }
        }
    }
}
