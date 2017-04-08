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
        // Pripravim HttpClienta        
        static Auth auth = null;
        static RestPuller puller = null;

        // Nazvy adredaru se soubory pro stazeni
        const string FolderPath_rootFolder       = @"STAG_DATA";
        const string FolderPath_rozvrhyByStudent = FolderPath_rootFolder + @"\rozvrhyByStudent";
        const string FolderPath_uciteleByID      = FolderPath_rootFolder + @"\uciteleBySTAGid";
        const string FolderPath_PredmetCizi      = FolderPath_rootFolder + @"\predmetyCizi";

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

            // Stahne a poresi hiearchii pracovist
            Handle_Hiearchie(data, serializer);
        

            // Stahne seznam studentu pro @fakulta            
            Handle_StudentByFakulta(fakulta, data, serializer);

            // Stahne predmety fakulty + deserializuje (pokud existuje, nestahuje)
            Handle_PredmetyByFakulta(fakulta, data, serializer);

            // Stahne rozvrh kazdeho studenta fakulty + deserializuje (pokud existuje, nestahuje) + poresi reference + stahne chybejici predmety
            Handle_StudentsRoak(data, serializer);

            // Stahne podrobnosti ke vsem ucitelum + deserializuje (pokud existuje, nestahuje) + poresi reference
            Handle_UcitelByRoak(data, serializer);

            
            
        }

        private static void PrepareFolders()
        {
            // Poresim slozky kam se budou ukladat všechny soubory

            System.IO.Directory.CreateDirectory(FolderPath_rootFolder);
            System.IO.Directory.CreateDirectory(FolderPath_PredmetCizi);
            System.IO.Directory.CreateDirectory(FolderPath_rozvrhyByStudent);
            System.IO.Directory.CreateDirectory(FolderPath_uciteleByID);
        }

        private static void Handle_PredmetyByFakulta(string fakulta, STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            // Stahnuti json souboru
            string pathpredmety = FolderPath_rootFolder + @"\" + "predmetyFakulta" + fakulta + ".json";
            Download_PredmetyByFakultaFullInfo(fakulta, pathpredmety);

            // Deserializace
            STAG_Classes.PredmetResponse tmppr = null;
            using (StreamReader file = File.OpenText(pathpredmety))
                tmppr = ((List<STAG_Classes.PredmetResponse>)serializer.Deserialize(file, typeof(List<STAG_Classes.PredmetResponse>)))[0];

            // Kazdy predmet si ulozim
            foreach (var pr in tmppr.PredmetInfo)
            {
                // Existuje uz katedra s timhle kodem? Kdyz ne, pridam
                if (!data.PredmetyPodleKateder.ContainsKey(pr.Katedra))
                {
                    data.PredmetyPodleKateder.Add(pr.Katedra, new Dictionary<string, STAG_Classes.Predmet>());
                }

                // Pridam predmet
                if (!data.PredmetyPodleKateder[pr.Katedra].ContainsKey(pr.Zkratka))
                {
                    data.PredmetyPodleKateder[pr.Katedra].Add(pr.Zkratka, pr);
                }
            }          
        }
        private static void Handle_UcitelByRoak(STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            foreach (var akce in data.Akce)
            {
                foreach (var ucitelID in akce.Value.VsichniUciteleUcitIdno)
                {
                    if (!data.Ucitele.ContainsKey(ucitelID))
                    {
                        // Stazeni ucitele
                        string tmppath = FolderPath_uciteleByID + @"\" + ucitelID.ToString() + ".json";
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
        private static void Handle_StudentByFakulta(string fakulta, STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            string studentspath = FolderPath_rootFolder + @"\StudentiByFakulta" + fakulta + ".json";
            Download_StudentsByFakulta(fakulta, studentspath);
            using (StreamReader file = File.OpenText(studentspath)) //Deserializace
                data.Students = ((List<STAG_Classes.StudentiResponse>)serializer.Deserialize(file, typeof(List<STAG_Classes.StudentiResponse>)))[0].Student.ToList();
        }
        private static void Handle_StudentsRoak(STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            foreach (var student in data.Students)
            {
                // Stahnu rozvrh studenta
                string tmppath = FolderPath_rozvrhyByStudent + @"\" + student.OsCislo + ".json";
                Download_RozvrhByStudent(student.OsCislo, tmppath);

                // Sem si ulozim rozvrhove akce studenta
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
                    Handle_Roak(data, student, roak,serializer);
                }

            }
        }
        private static void Handle_Roak(STAG_Classes.STAG_Database data, STAG_Classes.Student student, STAG_Classes.RozvrhovaAkce roak, JsonSerializer serializer)
        {
            // Zaradi rozvrhovou akci a prida vsemozne reference

            // Zkontroluji, jestli uz zavedenou katedru
            if (!data.PredmetyPodleKateder.ContainsKey(roak.Katedra))
            { 
                // Zatim neznam ~ neni z me fakultu... musim stahnout
                Handle_PredmetInfo(roak.Katedra, roak.Predmet, data, serializer);              
            }
            if (!data.PredmetyPodleKateder[roak.Katedra].ContainsKey(roak.Predmet))
            {
                // Nezman predmet... musim stahnout
                Handle_PredmetInfo(roak.Katedra, roak.Predmet, data, serializer);
            }

            // vvvvv   REFERENCE   vvvvvv

            // Pridam reference roak <=> predmet
            data.PredmetyPodleKateder[roak.Katedra][roak.Predmet].VsechnyAkce.Add(roak);
            roak.PredmetRef = data.PredmetyPodleKateder[roak.Katedra][roak.Predmet];
            
            // Ulozim si akci a ridam reference student => roak
            if (!data.Akce.ContainsKey(roak.RoakIdno))
                data.Akce.Add(roak.RoakIdno, roak);
            student.Rozvrh.Add(data.Akce[roak.RoakIdno]);
            data.Akce[roak.RoakIdno].referenceCount++;
        }
        private static void Handle_PredmetInfo(string katedra, string zkratka, STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            // Stahnu predmet kdy jeste neni
            var path = FolderPath_PredmetCizi + @"\" + string.Format("{0}{1}.json", katedra, zkratka);
            Download_PredmetyByZkratka(katedra, zkratka, path);
            
            // Deserializace
            STAG_Classes.Predmet tmppr = null;
            using (StreamReader file = File.OpenText(path))
                tmppr = ((List<STAG_Classes.Predmet>)serializer.Deserialize(file, typeof(List<STAG_Classes.Predmet>)))[0];

            // Predmet si ulozim
            // Existuje uz katedra s timhle kodem? Kdyz ne, pridam
            if (!data.PredmetyPodleKateder.ContainsKey(tmppr.Katedra))
            {
                data.PredmetyPodleKateder.Add(tmppr.Katedra, new Dictionary<string, STAG_Classes.Predmet>());
            }
            // Pridam predmet
            if (!data.PredmetyPodleKateder[tmppr.Katedra].ContainsKey(tmppr.Zkratka))
            {
                data.PredmetyPodleKateder[tmppr.Katedra].Add(tmppr.Zkratka, tmppr);
            }           
        }
        private static void Handle_Hiearchie(STAG_Classes.STAG_Database data, JsonSerializer serializer)
        {
            // Stahnuti souboru
            var path = FolderPath_rootFolder + @"\hiearchie.json";
            Download_HierarchiePracovist(path);

            // Deserializace
            List<STAG_Classes.Pracoviste> hiearchie = null;
            using (StreamReader file = File.OpenText(path))
                hiearchie = ((List<STAG_Classes.Hierarchie>)serializer.Deserialize(file, typeof(List<STAG_Classes.Hierarchie>)))[0].Pracoviste.ToList();

            // Poresim hiearchii do slovniku

            foreach (var pracoviste in from lvl2 in hiearchie where lvl2.Level == 2 select lvl2)
            {
                // Podrzim si jmeno lvl2 pracoviste
                var nadrazene = pracoviste.Zkratka;

                // Vytvorim slovnik pr jeho podradne pracoviste
                var tmp = new Dictionary<string, STAG_Classes.Pracoviste>();
                
                // Pridam vsechny pracoviste, ktery ho maji jako nadradne (hledam lvl3)
                foreach (var podradne in from lvl3 in hiearchie where lvl3.NadrazenePracoviste == nadrazene select lvl3 )
                    tmp.Add(podradne.Zkratka, podradne);
                
                // Pridam cele pracoviste s jeho podrazenymi pracovisti
                data.HiearchiePracovist.Add(pracoviste.Zkratka, tmp);
            }
        }

        

        private static void Download_StudentsByFakulta(string fakulta, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetStudentiByFakulta;
                request.SetToken("fakulta", fakulta);
                request.AddToken("stav", "S");
                request.AddToken("outputFormat", "json");

                File.WriteAllText(path, puller.GetResponseContent(request));
                Debug.WriteLine(string.Format("[Stazeno] StudentsByFakulta({0})",fakulta));
            }

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
        private static void Download_PredmetyByZkratka(string katedra,string zkratka, string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetPredmetInfo;
                request.SetToken("katedra", katedra);
                request.SetToken("zkratka", zkratka);
                request.AddToken("outputFormat", "json");


                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] PredmetyByZkratka({0})", zkratka));
            }

        }
        private static void Download_HierarchiePracovist(string path)
        {
            if (!File.Exists(path))
            {
                var request = STAGRequests.GetHierarchiePracovist;

                request.AddToken("outputFormat", "json");


                var content = puller.GetResponseContent(request);
                File.WriteAllText(path, content);
                Debug.WriteLine(string.Format("[Stazeno] HierarchiePracovist({0})", "UJEP"));
            }

        }
    }
}
