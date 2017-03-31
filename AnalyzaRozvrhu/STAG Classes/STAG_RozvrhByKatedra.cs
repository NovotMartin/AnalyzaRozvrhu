using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalyzaRozvrhu.STAG_Classes
{


    [XmlRoot(ElementName = "rozvrhovaAkce")]
    public class RozvrhovaAkce
    {
        [XmlElement(ElementName = "roakIdno")]
        public string RoakIdno { get; set; }




        [XmlElement(ElementName = "katedra")]
        public string Katedra { get; set; }
        [XmlElement(ElementName = "predmet")]
        public string Predmet { get; set; }


        

        [XmlElement(ElementName = "budova")]
        public string Budova { get; set; }
        [XmlElement(ElementName = "mistnost")]
        public string Mistnost { get; set; }


       

        [XmlElement(ElementName = "obsazeni")]
        public string Obsazeni { get; set; }

        


        [XmlElement(ElementName = "typAkceZkr")]
        public string TypAkceZkr { get; set; }

        [XmlElement(ElementName = "semestr")]
        public string Semestr { get; set; }

       


        [XmlElement(ElementName = "denZkr")]
        public string DenZkr { get; set; }

        [XmlElement(ElementName = "hodinaOd")]
        public string HodinaOd { get; set; }

        [XmlElement(ElementName = "hodinaDo")]
        public string HodinaDo { get; set; }

        [XmlElement(ElementName = "hodinaSkutOd")]
        public string HodinaSkutOd { get; set; }

        [XmlElement(ElementName = "hodinaSkutDo")]
        public string HodinaSkutDo { get; set; }

        [XmlElement(ElementName = "tydenOd")]
        public string TydenOd { get; set; }

        [XmlElement(ElementName = "tydenDo")]
        public string TydenDo { get; set; }

       

        [XmlElement(ElementName = "tydenZkr")]
        public string TydenZkr { get; set; }

        [XmlElement(ElementName = "grupIdno")]
        public string GrupIdno { get; set; }

      

        [XmlElement(ElementName = "druhAkce")]
        public string DruhAkce { get; set; }

        [XmlElement(ElementName = "vsichniUciteleUcitIdno")]
        public string VsichniUciteleUcitIdno { get; set; }


        #region Nepouzivane Atributy
        /*
        [XmlElement(ElementName = "nazev")]
        public string Nazev { get; set; }
        */
        /*
        [XmlElement(ElementName = "ucitIdno")]
        public string UcitIdno { get; set; }

        [XmlElement(ElementName = "rok")]
        public string Rok { get; set; }
        */
        /*
       [XmlElement(ElementName = "kapacitaMistnosti")]
       public string KapacitaMistnosti { get; set; }
       [XmlElement(ElementName = "planObsazeni")]
       public string PlanObsazeni { get; set; }
       */
        /*
        [XmlElement(ElementName = "typAkce")]
        public string TypAkce { get; set; }
        */
        /*
       [XmlElement(ElementName = "platnost")]
       public string Platnost { get; set; }
       [XmlElement(ElementName = "den")]
       public string Den { get; set; }
       */
        /*
        [XmlElement(ElementName = "tyden")]
        public string Tyden { get; set; }
        */
        /*
        [XmlElement(ElementName = "jeNadrazena")]
        public string JeNadrazena { get; set; }
        [XmlElement(ElementName = "maNadrazenou")]
        public string MaNadrazenou { get; set; }
        [XmlElement(ElementName = "kontakt")]
        public string Kontakt { get; set; }
        [XmlElement(ElementName = "krouzky")]
        public string Krouzky { get; set; }
        [XmlElement(ElementName = "casovaRada")]
        public string CasovaRada { get; set; }
        [XmlElement(ElementName = "datumOd")]
        public string DatumOd { get; set; }
        [XmlElement(ElementName = "datumDo")]
        public string DatumDo { get; set; }
          */
        /*
        [XmlElement(ElementName = "vsichniUciteleJmenaTituly")]
        public string VsichniUciteleJmenaTituly { get; set; }
        [XmlElement(ElementName = "vsichniUcitelePrijmeni")]
        public string VsichniUcitelePrijmeni { get; set; }
        [XmlElement(ElementName = "referencedIdno")]
        public string ReferencedIdno { get; set; }
        [XmlElement(ElementName = "owner")]
        public string Owner { get; set; }
        [XmlElement(ElementName = "zakazaneAkce")]
        public string ZakazaneAkce { get; set; }
        [XmlElement(ElementName = "datum")]
        public string Datum { get; set; }
        */ 
        #endregion
    }

    [XmlRoot(ElementName = "rozvrh")]
    public class Rozvrh
    {
        [XmlElement(ElementName = "rozvrhovaAkce")]
        public List<RozvrhovaAkce> RozvrhovaAkce { get; set; }
    }

    [XmlRoot(ElementName = "getRozvrhByKatedraResponse", Namespace = "http://stag-ws.zcu.cz/")]
    public class GetRozvrhByKatedraResponse
    {
        [XmlElement(ElementName = "rozvrh")]
        public Rozvrh Rozvrh { get; set; }

    }
}
