using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace StagRestLib
{

    //test

    public enum STAGRequestResponseType
    {
        XML,
        JSON,
        XLS,
        CSV
    }
    /// <summary>
    /// Statická metoda, která má v sobě všechny naprogramovaný requesty.
    /// </summary>
    public static class STAGRequests
    {
        // Vsechny predprogramovane requesty
        public static STAGRequest GetRozvrhByStudent { get { return new STAGRequest("/ws/services/rest/rozvrhy/getRozvrhByStudent", Method.GET, new STAGRequestToken("osCislo")); } }
        public static STAGRequest GetPredmetyByKatedraFullInfo { get { return new STAGRequest("/ws/services/rest/predmety/getPredmetyByKatedraFullInfo", Method.GET, new STAGRequestToken("katedra")); } }
        public static STAGRequest GetRozvrhByKatedra { get{ return new STAGRequest("/ws/services/rest/rozvrhy/getRozvrhByKatedra",Method.GET,new STAGRequestToken[] { new STAGRequestToken("katedra"), new STAGRequestToken("semestr")}); } }
        public static STAGRequest GetStudentiByFakulta { get { return new STAGRequest("/ws/services/rest/student/getStudentiByFakulta", Method.GET,new STAGRequestToken("fakulta")); } }
        public static STAGRequest GetPredmetyByFakultaFullInfo { get { return new STAGRequest("/ws/services/rest/predmety/getPredmetyByFakultaFullInfo",Method.GET, new STAGRequestToken("fakulta")); } }
        public static STAGRequest GetUcitelInfo { get { return new STAGRequest("/ws/services/rest/ucitel/getUcitelInfo", Method.GET, new STAGRequestToken("ucitIdno")); } }
        public static STAGRequest GetPredmetInfo { get { return new STAGRequest("/ws/services/rest/predmety/getPredmetInfo", Method.GET, new STAGRequestToken[] { new STAGRequestToken("katedra"), new STAGRequestToken("zkratka") }); } }
        public static STAGRequest GetHierarchiePracovist { get { return new STAGRequest("/ws/services/rest/ciselniky/getHierarchiePracovist",Method.GET); } }


    }

    /// <summary>
    /// Nas request do STAGu. Obal pro RestRequest z RestSharp knihovny.
    /// </summary>
    public class STAGRequest
    {
        private const string ID_IDENTITY = "IDVal";

        private string requestUrl;
        private Method method;
        private List<STAGRequestToken> tokens;
        private STAGRequestResponseType responseType;

        public STAGRequest(string url, Method method)
        {
            this.requestUrl = url;
            this.method = method;
            this.tokens = new List<STAGRequestToken>();
            this.responseType = STAGRequestResponseType.XML;
        }

        public STAGRequest(string url, Method method, params STAGRequestToken[] tokens)
        {
            this.requestUrl = url;
            this.method = method;
            this.tokens = tokens.ToList();
            this.responseType = STAGRequestResponseType.XML;
        }

        public void AddToken(string name, string value)
        {
            this.tokens.Add(new STAGRequestToken(name, value));
        }

        public bool SetToken(string name, string value)
        {
            foreach (STAGRequestToken token in tokens)
            {
                if (name == token.Name)
                {
                    token.SetValue(value);
                    return true;
                }
            }
            return false;
        }

        public RestRequest GetRestRequest()
        {
            string fullURL = GenerateUrlString();
            RestRequest request = new RestRequest(fullURL, method);

            if (tokens.Count > 0)
            {
                foreach (var token in tokens)
                {
                    request.AddUrlSegment(token.Name + ID_IDENTITY, token.Value);
                }
            }

            //request.AddParameter("outputFormat", responseType.ToString());

            return request;
        }

        private string GenerateUrlString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(requestUrl);
            
            if (tokens.Count > 0)
            {
                sb.Append("?");

                for (int i = 0; i < tokens.Count; i++)
                {
                    sb.Append(tokens[i].Name + "=" + "{" + tokens[i].Name + ID_IDENTITY + "}");
                    if (i != tokens.Count - 1)
                    {
                        sb.Append("&");
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class STAGRequestToken
    {
        public string Name { get { return name; } }
        public string Value { get { return value; } }

        private string name;
        private string value;

        public STAGRequestToken(string name)
        {
            this.name = name;
            this.value = "";
        }

        public STAGRequestToken(string name, string value)
        {
            this.name = name;
            this.value = value;                
        }

        public void SetValue(string value)
        {
            this.value = value;
        }
    }
}
