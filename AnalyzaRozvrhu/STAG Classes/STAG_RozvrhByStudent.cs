using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalyzaRozvrhu.STAG_Classes
{
    // STAG_RozvrhByKatedra již obsahuje definice tříd pro rozvrh a rozvrhové akce...    
    

    [XmlRoot(ElementName = "getRozvrhByStudentResponse", Namespace = "http://stag-ws.zcu.cz/")]
    public class GetRozvrhByStudentResponse
    {
        [XmlElement(ElementName = "rozvrh")]
        public Rozvrh Rozvrh { get; set; }
    }
}
