﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnalyzaRozvrhu.STAG_Classes
{


    public class PlatnostDo
    {

        [JsonProperty("value")]
        public string DateTime { get; set; }
    }
    /// <summary>
    /// Objektová reprezentace souboru pro pracoviště získané ze služby getHierarchiePracovist
    /// </summary>
    /// <remarks> Určeno pro návrat z této služby https://ws.ujep.cz/ws/service-help/rest/ciselniky/getHierarchiePracovist . Pro deserializaci je nutný json. Pro lepší pochopení dat v této třídě doporučuji nahlédnout do staženého souboru.
    /// Skutečné názvy soupců (nebo json atributů chceteli) jsou vždy obsaženy v tagu [JsonProperty("nazev")] 
    /// </remarks>
    public class Pracoviste
    {
        /// <summary>
        /// Úroveň pracoviště (1 - 3).
        /// </summary>
        /// <remarks>
        /// lvl 1 = univerzit, rektorat 
        /// lvl 2 = fakulta, knihovna...
        /// lvl 3 = katedra, oddeleni, studijni...
        /// </remarks>
        [JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// Zřejmě ID pracoviště.
        /// </summary>
        [JsonProperty("cisloPracoviste")]
        public int CisloPracoviste { get; set; }

        /// <summary>
        /// Zkratka pracoviště (např. PRF, KI, KCH ....).
        /// </summary>
        /// <remarks>Zkratka může obsahovat i znaky z diakritikou, ale není pak úplně jasné co se stane když to použijeme jako parametr do webové služby STAGu</remarks>
        [JsonProperty("zkratka")]
        public string Zkratka { get; set; }

        /// <summary>
        /// Celý dlouhý název pracoviště (např. Přírodovědecká fakulta).
        /// </summary>
        [JsonProperty("nazev")]
        public string Nazev { get; set; }

        /// <summary>
        /// Typ pracoviště K(atedra) nebo F(akulta).
        /// </summary>
        /// <remarks> Pozor, to že je něco typu K neznamená, že je to katedra. Furt to může být třeba oddělení nebo knihovna... :D</remarks>
        [JsonProperty("typPracoviste")]
        public string TypPracoviste { get; set; }

        /// <summary>
        /// Typ vedoucího
        /// </summary>
        /// <remarks>Podle tohohle se dá zjistit jakého typu je pracoviště</remarks>
        [JsonProperty("typVedouciho")]
        public string TypVedouciho { get; set; }

        /// <summary>
        /// Zkratka nadřazeného pracoviště 
        /// </summary>
        [JsonProperty("nadrazenePracoviste")]
        public string NadrazenePracoviste { get; set; }

        /// <summary>
        /// Pokud je null nebo prazdna, je to platne, pokud je tam nejake starsi datum, pracoviste je jiz zrusenne
        /// </summary>
        [JsonProperty("platnostDo")]
        public PlatnostDo PlatnostDo { get; set; }
    }

    public class Hierarchie
    {

        [JsonProperty("pracoviste")]
        public Pracoviste[] Pracoviste { get; set; }
    }

}
