using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyzaRozvrhu.STAG_Classes
{
    /*
     * Tyhle vycty se daji ziskat ze STAGU
     * https://ws.ujep.cz/ws/web?pp_locale=cs&selectedTyp=REST&pp_reqType=render&pp_page=serviceList&addr=%2Fws%2Fservices%2Frest%2Fciselniky
     * 
     */



    /// <summary>
    /// Výběrový typ pro zvolení fakulty. Chybí FŽP kvůli nejasnému zacházení s diakritikou
    /// </summary>
    public enum Fakulta
    {
        PRF,
        FF,
        FSC,
        FSE,
        FUU,
        FVT,
        FZS,
        PF,
        PFC,
    }
}
