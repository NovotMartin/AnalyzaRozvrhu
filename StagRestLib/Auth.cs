using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Authenticators;

namespace StagRestLib
{
    /// <summary>
    /// Třída obaluje HttpBasicAuthentificator.
    /// </summary>
    public class Auth
    {
        // Properties
        public HttpBasicAuthenticator Authenticator
        {
            get
            {
                return authenticator;
            }
        }
        public string Login { get; private set; }

        private HttpBasicAuthenticator authenticator;

        /// <summary>
        /// Vytvoří Auth objekt - obal pro HttpBasicAuthenticator.
        /// </summary>
        /// <param name="login">Login do systému STAG -> nově "st*****".</param>
        /// <param name="password">Heslo k účtu *NEŠIFROVANÉ!*</param>
        public Auth(string login, string password)
        {
            this.Login = login;
            this.authenticator = new HttpBasicAuthenticator(login, password);
        }


    }
}
