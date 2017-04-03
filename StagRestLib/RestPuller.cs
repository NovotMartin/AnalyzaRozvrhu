using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace StagRestLib
{
    /// <summary>
    /// TODO : Tato trida by mohla byt i staticka!
    /// </summary>
    public class RestPuller
    {
        // WebServices URL
        private const string WS_URL = @"https://ws.ujep.cz";

        private RestClient restClient = new RestClient(WS_URL);
        private Auth auth;

        public RestPuller()
        {
            this.auth = new Auth("Anonymous", "");
            restClient.Authenticator = this.auth.Authenticator;
        }

        public RestPuller(Auth auth)
        {
            this.auth = auth;
            restClient.Authenticator = this.auth.Authenticator;
        }

        public string GetResponseContent(STAGRequest request)
        {
            IRestResponse response = restClient.Execute(request.GetRestRequest());
            string content = "";

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (auth.Login == "Anonymous")
                {
                    content = "ERROR - You need to be logged in!";
                } else
                {
                    content = "ERROR - User \"" + auth.Login + "\" couldn't login! Check your password!";
                }
            } else
            {
                content = response.Content;
            }

            
            return content;
        }
    }
}
