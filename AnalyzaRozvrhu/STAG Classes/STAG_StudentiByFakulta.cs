using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalyzaRozvrhu.STAG_Classes
{
    [XmlRoot(ElementName = "student")]
    public class Student
    {
        [XmlElement(ElementName = "osCislo")]
        public string OsCislo { get; set; }


        [XmlElement(ElementName = "stav")]
        public string Stav { get; set; }
        [XmlElement(ElementName = "kodSp")]
        public string KodSp { get; set; }

        [XmlElement(ElementName = "formaSp")]
        public string FormaSp { get; set; }


        [XmlElement(ElementName = "rocnik")]
        public string Rocnik { get; set; }
        [XmlElement(ElementName = "oborKomb")]
        public string OborKomb { get; set; }
        [XmlElement(ElementName = "oborIdnos")]
        public string OborIdnos { get; set; }


        #region Nepouzivane atributy
        /*
          [XmlElement(ElementName = "jmeno")]
          public string Jmeno { get; set; }
          [XmlElement(ElementName = "prijmeni")]
          public string Prijmeni { get; set; }
          */

        /*
        [XmlElement(ElementName = "userName")]
        public string UserName { get; set; }
        [XmlElement(ElementName = "stprIdno")]
        public string StprIdno { get; set; }
        [XmlElement(ElementName = "nazevSp")]
        public string NazevSp { get; set; }
        [XmlElement(ElementName = "fakultaSp")]
        public string FakultaSp { get; set; }
        */
        /*
        [XmlElement(ElementName = "typSp")]
        public string TypSp { get; set; }
        [XmlElement(ElementName = "czvSp")]
        public string CzvSp { get; set; }
        [XmlElement(ElementName = "mistoVyuky")]
        public string MistoVyuky { get; set; }
        */

        /*
        [XmlElement(ElementName = "evidovanBankovniUcet")]
        public string EvidovanBankovniUcet { get; set; }
        [XmlElement(ElementName = "titulPred")]
        public string TitulPred { get; set; }
        [XmlElement(ElementName = "rozvrhovyKrouzek")]
        public string RozvrhovyKrouzek { get; set; }
        [XmlElement(ElementName = "titulZa")]
        public string TitulZa { get; set; }
        [XmlElement(ElementName = "financovani")]
        public string Financovani { get; set; }
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }
        [XmlElement(ElementName = "cisloKarty")]
        public string CisloKarty { get; set; }
        [XmlElement(ElementName = "pohlavi")]
        public string Pohlavi { get; set; }
        */ 
        #endregion
    }

    [XmlRoot(ElementName = "studenti")]
    public class Studenti
    {
        [XmlElement(ElementName = "student")]
        public List<Student> Student { get; set; }
    }

    [XmlRoot(ElementName = "getStudentiByFakultaResponse", Namespace = "http://stag-ws.zcu.cz/")]
    public class GetStudentiByFakultaResponse
    {
        [XmlElement(ElementName = "studenti")]
        public Studenti Studenti { get; set; }

    }
}
