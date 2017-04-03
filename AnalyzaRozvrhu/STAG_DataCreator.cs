using Newtonsoft.Json;
using StagRestLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;



namespace AnalyzaRozvrhu
{
    public static class STAG_DataCollector
    {

        /*
         *  TODO:
         *   - Dodelat reference k predmetum z rozvrhovych akci (nebo ne?)
         */
        static Auth auth = null;
        static RestPuller puller = null;

        // Nazvy adredaru se soubory pro stazeni
        const string rootFolderName = @"STAG_DATA";
        //const string rozvrhyByKatedraFolderPath = rootFolderName + @"\rozvrhyByKatedra";
        const string rozvrhyByStudentFolderPath = rootFolderName + @"\rozvrhyByStudent";
        const string uciteleByIDFolderPath = rootFolderName + @"\uciteleBySTAGid";


        public static void GetData(STAG_Classes.Fakulta fakulta, string staguser, string stagpass)
        {
            GetData(fakulta.ToString(), staguser, stagpass);
        }

        public static void GetData(string fakulta, string staguser, string stagpass)
        {
            STAG_Classes.STAG_Database data = new STAG_Classes.STAG_Database(fakulta);
            JsonSerializer serializer = new JsonSerializer();

            // Priprava REST klienta
            auth = new Auth(staguser, stagpass);
            puller = new RestPuller(auth);

            // Priprava adresaru (kdyz existuji, nic se nedeje)
            PrepareFolders();


            // Stahne seznam studentu pro @fakulta            
            StudentByFakultaHandle(fakulta, data, serializer);

            // Stahne rozvrh kazdeho studenta fakulty + deserializuje (pokud existuje, nestahuje) + poresi reference
            StudentsRoakHandle(data, serializer);

            // Stahne podrobnosti ke vsem ucitelum + deserializuje (pokud existuje, nestahuje) + poresi reference
            UcitelByRoakHandle(data, serializer);

            // Stahne predmety fakulty + deserializuje (pokud existuje, nestahuje)
            PredmetyByFakultaHandle(fakulta, data, serializer);

            // Vytvori seznam vsech kateder
            data.Katedry = (from x in data.Predmety where x.Value.Katedra != "" select x.Value.Katedra).Distinct().ToList();
        }

        private static void PredmetyByFakultaHandle(string fakulta, STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            string pathpredmety = rootFolderName + @"\" + "predmetyFakulta" + fakulta + ".json";
            Download_PredmetyByFakultaFullInfo(fakulta, pathpredmety);

            // Deserializace
            STAG_Classes.PredmetResponse tmppr = null;
            using (StreamReader file = File.OpenText(pathpredmety))
                tmppr = ((List<STAG_Classes.PredmetResponse>)serializer.Deserialize(file, typeof(List<STAG_Classes.PredmetResponse>)))[0];

            foreach (var pr in tmppr.PredmetInfo)
            {
                string key = pr.Katedra + "/" + pr.Zkratka;
                if (!data.Predmety.ContainsKey(key))
                    data.Predmety.Add(key, pr);
            }

            // TODO: pridat reference k rozvrhovym akcim
        }

        private static void UcitelByRoakHandle(STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            foreach (var akce in data.Akce)
            {
                foreach (var ucitelID in akce.Value.VsichniUciteleUcitIdno)
                {
                    if (!data.Ucitele.ContainsKey(ucitelID))
                    {
                        // Stazeni ucitele
                        string tmppath = uciteleByIDFolderPath + @"\" + ucitelID.ToString() + ".json";
                        Download_UcitelInfo(ucitelID.ToString(), tmppath);
                        // Deserializace
                        STAG_Classes.Ucitel tmp = null;
                        using (StreamReader file = File.OpenText(tmppath))
                            tmp = ((List<STAG_Classes.Ucitel>)serializer.Deserialize(file, typeof(List<STAG_Classes.Ucitel>)))[0];
                        // Pridani do slovniku
                        data.Ucitele.Add(ucitelID, tmp);
                    }

                    // Pridani reference
                    akce.Value.VsichniUcitele.Add(data.Ucitele[ucitelID]);
                    data.Ucitele[ucitelID].referenceCount++;
                }
            }
        }

        private static void StudentByFakultaHandle(string fakulta, STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            string studentspath = rootFolderName + @"\StudentiByFakulta" + fakulta + ".json";
            Download_StudentsByFakulta(fakulta, studentspath);
            using (StreamReader file = File.OpenText(studentspath)) //Deserializace
                data.Students = ((List<STAG_Classes.StudentiResponse>)serializer.Deserialize(file, typeof(List<STAG_Classes.StudentiResponse>)))[0].Student.ToList();
        }

        private static void StudentsRoakHandle(STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            foreach (var student in data.Students)
            {
                string tmppath = rozvrhyByStudentFolderPath + @"\" + student.OsCislo + ".json";
                Download_RozvrhByStudent(student.OsCislo, tmppath);


                List<STAG_Classes.RozvrhovaAkce> tmp = null;
                // Deserializace
                try
                {
                    using (StreamReader file = File.OpenText(tmppath))
                        tmp = ((List<STAG_Classes.RozvrhByStudentResponse>)serializer.Deserialize(file, typeof(List<STAG_Classes.RozvrhByStudentResponse>)))[0].RozvrhovaAkce.ToList();
                }
                catch (Exception)
                {
                    // Pokud narazim na nejaky divny soubor, preskocim
                    continue;
                }
                // Kazkou akci pridam do tabulky akci a pridam reference
                foreach (var roak in tmp)
                {
                    if (!data.Akce.ContainsKey(roak.RoakIdno))
                        data.Akce.Add(roak.RoakIdno, roak);
                    student.Rozvrh.Add(data.Akce[roak.RoakIdno]);
                    data.Akce[roak.RoakIdno].referenceCount++;
                }

            }
        }

        private static void PrepareFolders()
        {
            System.IO.Directory.CreateDirectory(rootFolderName);
            //System.IO.Directory.CreateDirectory(rozvrhyByKatedraFolderPath);
            System.IO.Directory.CreateDirectory(rozvrhyByStudentFolderPath);
            System.IO.Directory.CreateDirectory(uciteleByIDFolderPath);
        }

        private static void Download_StudentsByFakulta(string fakulta, string path)
        {
            var request = STAGRequests.GetStudentiByFakulta;
            request.SetToken("fakulta", fakulta);
            request.AddToken("stav", "S");
            request.AddToken("outputFormat", "json");

            File.WriteAllText(path, puller.GetResponseContent(request));
            Debug.WriteLine(string.Format("[Stazeno] StudentsByFakulta({0})",fakulta));
        }
        private static void Download_RozvrhByStudent(string osCislo, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetRozvrhByStudent;
                request.SetToken("osCislo", osCislo);
                request.AddToken("outputFormat", "json");
                request.AddToken("jenRozvrhoveAkce", "true");
                request.AddToken("vsechnyAkce", "false");

                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] RozvrhByStudent({0})", osCislo));
            }          

        }
        private static void Download_RozvrhByKatedra(string katedra, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetRozvrhByStudent;
                request.SetToken("katedra", katedra);
                request.AddToken("outputFormat", "json");
                request.AddToken("jenRozvrhoveAkce", "true");
                request.AddToken("vsechnyAkce", "false");

                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] RozvrhByKatedra({0})", katedra));
            }

        }
        private static void Download_UcitelInfo(string ucitIdno, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetUcitelInfo;
                request.SetToken("ucitIdno", ucitIdno);
                request.AddToken("outputFormat", "json");


                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] UcitelInfoByIdno({0})", ucitIdno));
            }

        }
        private static void Download_PredmetyByFakultaFullInfo(string fakulta, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetPredmetyByFakultaFullInfo;
                request.SetToken("fakulta", fakulta);
                request.AddToken("outputFormat", "json");


                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] PredmetyByFakulta({0})", fakulta));
            }

        }
    }
}
