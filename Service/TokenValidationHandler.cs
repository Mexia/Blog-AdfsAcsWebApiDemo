// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenValidationHandler.cs" company="">
//   
// </copyright>
// <summary>
//   Class that handles the Token Validation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Service
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AccessControl2.SDK;

    /// <summary>
    /// Class that handles the Token Validation
    /// </summary>
    public class TokenValidationHandler : DelegatingHandler
    {
        // This code sample is based on - How to: Authenticate to a REST WCF Service Deployed to Windows Azure Using ACS - tutorial
        // http://msdn.microsoft.com/en-us/library/hh289317.aspx
        // It is recommended that you read the whole tutorial carefuly, especially 
        // Step 4 – Implement a Client That Requests The SWT Token From ACS and Forwards It To The REST WCF Service
        #region Fields

        /// <summary>
        /// The acs host name.
        /// </summary>
        private readonly string acsHostName = ConfigurationManager.AppSettings["Name"];

        /// <summary>
        /// The service namespace.
        /// </summary>
        private readonly string serviceNamespace = ConfigurationManager.AppSettings["ServiceNamespace"];

        /// <summary>
        /// The trusted audience.
        /// </summary>
        private readonly string trustedAudience = ConfigurationManager.AppSettings["TrustedAudience"];

        /// <summary>
        /// The trusted token policy key.
        /// </summary>
        private readonly string trustedTokenPolicyKey = ConfigurationManager.AppSettings["TrustedTokenPolicyKey"];

        #endregion

        #region Methods

        /// <summary>
        /// The send async.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The System.Threading.Tasks.Task`1[TResult -&gt; System.Net.Http.HttpResponseMessage].
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null)
            {
                // HANDLE SWT TOKEN VALIDATION
                // Get the authorization header
                string headerValue = request.Headers.GetValues("Authorization").First();

                // Check that a value is there
                if (string.IsNullOrEmpty(headerValue))
                {
                    return
                        Task.Factory.StartNew(
                            () =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                                        {
                                           Content = new StringContent("Authorization header is empty") 
                                        };
                                });
                }

                // Check that it starts with 'WRAP'
                if (!headerValue.StartsWith("WRAP "))
                {
                    return
                        Task.Factory.StartNew(
                            () =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                                        {
                                           Content = new StringContent("Invalid token") 
                                        };
                                });
                }

                string[] nameValuePair = headerValue.Substring("WRAP ".Length).Split(new[] { '=' }, 2);

                if (nameValuePair.Length != 2 || nameValuePair[0] != "access_token"
                    || !nameValuePair[1].StartsWith("\"") || !nameValuePair[1].EndsWith("\""))
                {
                    throw new ApplicationException("unauthorized");
                }

                // Trim off the leading and trailing double-quotes
                string token = nameValuePair[1].Substring(1, nameValuePair[1].Length - 2);

                // Create a token validator
                var validator = new TokenValidator(
                    this.acsHostName, this.serviceNamespace, this.trustedAudience, this.trustedTokenPolicyKey);

                // Validate the token
                if (!validator.Validate(token))
                {
                    return
                        Task.Factory.StartNew(
                            () =>
                                {
                                    return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                                        {
                                           Content = new StringContent("Invalid token") 
                                        };
                                });
                }
            }
            else
            {
                return
                    Task.Factory.StartNew(
                        () =>
                            {
                                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                                    {
                                       Content = new StringContent("The authorization header was not sent") 
                                    };
                            });
            }

            return base.SendAsync(request, cancellationToken);
            
        }

        #endregion
    }
}