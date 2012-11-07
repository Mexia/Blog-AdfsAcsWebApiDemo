// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   A sample consumer of a REST service.
//    1. Pulls a SAML token from ADFS using WIF.
//    2. Presents the SAML token to ACS, and gets a SWT token in return.
//    3. Passes the SWT token to the REST call.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Client
{
    using System;
    using System.Collections.Specialized;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Web;

    using Thinktecture.IdentityModel.Constants;
    using Thinktecture.IdentityModel.Extensions;
    using Thinktecture.IdentityModel.WSTrust;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        // REST service URI
        #region Static Fields

        /// <summary>
        /// The _base address.
        /// </summary>
        private static readonly Uri _baseAddress = new Uri("http://localhost:51388");

        // ACS base URI

        // Windows ADFS Services URI
        /// <summary>
        /// The _idp endpoint.
        /// </summary>
        private static readonly EndpointAddress _idpEndpoint =
            new EndpointAddress("https://dc2012.dev.bencode.net/adfs/services/trust/13/windowsmixed");

        /// <summary>
        /// The _acs realm.
        /// </summary>
        private static string _acsRealm = "http://localhost:51388/api";

        /// <summary>
        /// The _acs url.
        /// </summary>
        private static string _acsUrl = "https://theboss.accesscontrol.windows.net/";

        #endregion

        #region Methods

        /// <summary>
        /// The call service.
        /// </summary>
        /// <param name="swtToken">
        /// The swt token.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string CallService(string swtToken)
        {
            var client = new HttpClient { BaseAddress = _baseAddress };
            string headerValue = string.Format("WRAP access_token=\"{0}\"", swtToken);
            client.DefaultRequestHeaders.Add("Authorization", headerValue);

            HttpResponseMessage response = client.GetAsync("api/values").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// The get saml token from adfs.
        /// </summary>
        /// <param name="domain">
        /// The domain.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetSamlTokenFromAdfs(string domain, string username, string password)
        {
            "Requesting identity token".ConsoleYellow();

            var factory =
                new WSTrustChannelFactory(
                    new WindowsWSTrustBinding(SecurityMode.TransportWithMessageCredential), _idpEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;
            factory.Credentials.Windows.ClientCredential.Domain = domain;
            factory.Credentials.Windows.ClientCredential.UserName = username;
            factory.Credentials.Windows.ClientCredential.Password = password;

            var rst = new RequestSecurityToken
                {
                    RequestType = RequestTypes.Issue, 
                    KeyType = KeyTypes.Bearer, 
                    TokenType = TokenTypes.Saml2TokenProfile11, 
                    AppliesTo = new EndpointReference(_acsUrl)
                };

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;
            return token.TokenXml.OuterXml;
        }

        /// <summary>
        /// The get swt token.
        /// </summary>
        /// <param name="samlToken">
        /// The saml token.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private static string GetSwtToken(string samlToken)
        {
            string acsStsUrl = _acsUrl + "/WRAPv0.9";

            try
            {
                var client = new WebClient();
                client.BaseAddress = acsStsUrl;

                var parameters = new NameValueCollection();
                parameters.Add("wrap_scope", _acsRealm);
                parameters.Add("wrap_assertion_format", "SAML");
                parameters.Add("wrap_assertion", samlToken);

                byte[] responseBytes = client.UploadValues(string.Empty, parameters);
                string response = Encoding.UTF8.GetString(responseBytes);

                return
                    HttpUtility.UrlDecode(
                        response.Split('&').Single(
                            value => value.StartsWith("wrap_access_token=", StringComparison.OrdinalIgnoreCase)).Split(
                                '=')[1]);
            }
            catch (WebException wex)
            {
                string value = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                throw;
            }
        }

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            Console.Clear();
            string samlToken = GetSamlTokenFromAdfs("dev.bencode.net", "ben", "j4{S0>4fDs5oE+C");
            samlToken.ConsoleGreen();
            string swtToken = GetSwtToken(samlToken);
            swtToken.ConsoleYellow();
            CallService(swtToken).ColoredWriteLine(ConsoleColor.Magenta);
            Console.ReadLine();
        }

        #endregion
    }
}