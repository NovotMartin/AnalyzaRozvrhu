using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu
{
    public static class STAG_DataFixer
    {
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

        public static void NacistDotaznikKatedramXLS(this STAG_Classes.STAG_Database data, string path)
        {
            // TODO
            // Nacte dotaznik, zkontroluje, upravi data
        }


        // Dodelat metody pro generovani dotazniku kvuli ATYP předmětům
    }
}
