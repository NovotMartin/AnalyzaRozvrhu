using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalyzaRozvrhu.STAG_Classes
{

    [XmlRoot(ElementName = "predmetKatedryFullInfo")]
    public class PredmetKatedryFullInfo
    {
        [XmlElement(ElementName = "katedra")]
        public string Katedra { get; set; }
        [XmlElement(ElementName = "zkratka")]
        public string Zkratka { get; set; }





        [XmlElement(ElementName = "jednotekPrednasek")]
        public string JednotekPrednasek { get; set; }
        [XmlElement(ElementName = "jednotkaPrednasky")]

        public string JednotkaPrednasky { get; set; }
        [XmlElement(ElementName = "jednotekCviceni")]
        public string JednotekCviceni { get; set; }
        [XmlElement(ElementName = "jednotkaCviceni")]

        public string JednotkaCviceni { get; set; }
        [XmlElement(ElementName = "jednotekSeminare")]
        public string JednotekSeminare { get; set; }
        [XmlElement(ElementName = "jednotkaSeminare")]
        public string JednotkaSeminare { get; set; }



        #region Nepouzivane atributy
        /*           
            
            [XmlElement(ElementName = "rok")]
            public string Rok { get; set; }
            [XmlElement(ElementName = "nazev")]
            public string Nazev { get; set; }
            [XmlElement(ElementName = "nazevDlouhy")]
            public string NazevDlouhy { get; set; }
            [XmlElement(ElementName = "vyukaZS")]
            public string VyukaZS { get; set; }
            [XmlElement(ElementName = "vyukaLS")]
            public string VyukaLS { get; set; }
            [XmlElement(ElementName = "kreditu")]
            public string Kreditu { get; set; }
            [XmlElement(ElementName = "garanti")]
            public string Garanti { get; set; }
            [XmlElement(ElementName = "garantiUcitIdno")]
            public string GarantiUcitIdno { get; set; }
            [XmlElement(ElementName = "prednasejici")]
            public string Prednasejici { get; set; }
            [XmlElement(ElementName = "prednasejiciUcitIdno")]
            public string PrednasejiciUcitIdno { get; set; }
            [XmlElement(ElementName = "cvicici")]
            public string Cvicici { get; set; }
            [XmlElement(ElementName = "cviciciUcitIdno")]
            public string CviciciUcitIdno { get; set; }
            [XmlElement(ElementName = "seminarici")]
            public string Seminarici { get; set; }
            [XmlElement(ElementName = "seminariciUcitIdno")]
            public string SeminariciUcitIdno { get; set; }
            [XmlElement(ElementName = "podminujiciPredmety")]
            public string PodminujiciPredmety { get; set; }
            [XmlElement(ElementName = "vylucujiciPredmety")]
            public string VylucujiciPredmety { get; set; }
            [XmlElement(ElementName = "podminujePredmety")]
            public string PodminujePredmety { get; set; }
            [XmlElement(ElementName = "literatura")]
            public string Literatura { get; set; }
            [XmlElement(ElementName = "nahrazPredmety")]
            public string NahrazPredmety { get; set; }
            [XmlElement(ElementName = "metodyVyucovaci")]
            public string MetodyVyucovaci { get; set; }
            [XmlElement(ElementName = "metodyHodnotici")]
            public string MetodyHodnotici { get; set; }
            [XmlElement(ElementName = "akreditovan")]
            public string Akreditovan { get; set; }
            */

        /*
        
        [XmlElement(ElementName = "anotace")]
        public string Anotace { get; set; }
        [XmlElement(ElementName = "typZkousky")]
        public string TypZkousky { get; set; }
        [XmlElement(ElementName = "maZapocetPredZk")]
        public string MaZapocetPredZk { get; set; }
        [XmlElement(ElementName = "formaZkousky")]
        public string FormaZkousky { get; set; }
        [XmlElement(ElementName = "prehledLatky")]
        public string PrehledLatky { get; set; }
        [XmlElement(ElementName = "casovaNarocnost")]
        public string CasovaNarocnost { get; set; }
        [XmlElement(ElementName = "vyucovaciJazyky")]
        public string VyucovaciJazyky { get; set; }
        [XmlElement(ElementName = "ectsZobrazit")]
        public string EctsZobrazit { get; set; }
        [XmlElement(ElementName = "ectsAkreditace")]
        public string EctsAkreditace { get; set; }
        [XmlElement(ElementName = "ectsNabizetUPrijezdu")]
        public string EctsNabizetUPrijezdu { get; set; }
        [XmlElement(ElementName = "zarazenDoKombinovanehoStudia")]
        public string ZarazenDoKombinovanehoStudia { get; set; }
        [XmlElement(ElementName = "semestr")]
        public string Semestr { get; set; }
        [XmlElement(ElementName = "pocetStudentu")]
        public string PocetStudentu { get; set; }
        [XmlElement(ElementName = "aSkut")]
        public string ASkut { get; set; }
        [XmlElement(ElementName = "bSkut")]
        public string BSkut { get; set; }
        [XmlElement(ElementName = "cSkut")]
        public string CSkut { get; set; }
        [XmlElement(ElementName = "pozadavky")]
        public string Pozadavky { get; set; }
        [XmlElement(ElementName = "cMax")]
        public string CMax { get; set; }
        [XmlElement(ElementName = "predpoklady")]
        public string Predpoklady { get; set; }
        [XmlElement(ElementName = "poznamka")]
        public string Poznamka { get; set; }
        [XmlElement(ElementName = "bMax")]
        public string BMax { get; set; }
        [XmlElement(ElementName = "ziskaneZpusobilosti")]
        public string ZiskaneZpusobilosti { get; set; }
        [XmlElement(ElementName = "skupinaAkreditace")]
        public string SkupinaAkreditace { get; set; }
        */
        #endregion

    }




    [XmlRoot(ElementName = "predmetyKatedryFullInfo")]
    public class PredmetyKatedryFullInfo
    {
        [XmlElement(ElementName = "predmetKatedryFullInfo")]
        public List<PredmetKatedryFullInfo> PredmetKatedryFullInfo { get; set; }
    }

    [XmlRoot(ElementName = "getPredmetyByKatedraFullInfoResponse", Namespace = "http://stag-ws.zcu.cz/")]
    public class GetPredmetyByKatedraFullInfoResponse
    {
        [XmlElement(ElementName = "predmetyKatedryFullInfo")]
        public PredmetyKatedryFullInfo PredmetyKatedryFullInfo { get; set; }
        [XmlAttribute(AttributeName = "ns1", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Ns1 { get; set; }
    }

    

}
