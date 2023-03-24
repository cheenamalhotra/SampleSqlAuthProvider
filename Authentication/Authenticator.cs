//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SampleSqlAuthProvider.Authentication
{
    /// <summary>
    /// Provides APIs to acquire access token using MSAL.NET v4 with provided <see cref="AuthenticationParams"/>.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        private readonly string appClientId = string.Empty;
        private readonly string appName = string.Empty;

        private static readonly ConcurrentDictionary<string, IPublicClientApplication> PublicClientAppMap = new();

        #region Public APIs
        public Authenticator(string appClientId, string appName)
        {
            this.appClientId = appClientId;
            this.appName = appName;
        }

        /// <summary>
        /// Acquires access token synchronously.
        /// </summary>
        /// <param name="params">Authentication parameters to be used for access token request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Access Token with expiry date</returns>
        public AccessToken GetToken(AuthenticationParams @params, CancellationToken cancellationToken) =>
            GetTokenAsync(@params, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Acquires access token asynchronously.
        /// </summary>
        /// <param name="params">Authentication parameters to be used for access token request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Access Token with expiry date</returns>
        /// <exception cref="Exception"></exception>
        public async Task<AccessToken> GetTokenAsync(AuthenticationParams @params, CancellationToken cancellationToken)
        {
            IPublicClientApplication publicClientApplication = GetPublicClientAppInstance(@params.Authority, @params.Audience);

            AccessToken accessToken;
            if (@params.AuthenticationMethod == AuthenticationMethod.ActiveDirectoryInteractive)
            {
                // Find account
                IEnumerator<IAccount> accounts = (await publicClientApplication.GetAccountsAsync().ConfigureAwait(false)).GetEnumerator();
                IAccount account = default;

                if (!string.IsNullOrEmpty(@params.UserName) && accounts.MoveNext())
                {
                    string username = @params.UserName;

                    do
                    {
                        IAccount currentVal = accounts.Current;
                        if (string.Compare(username, currentVal.Username, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            account = currentVal;
                            break;
                        }
                    }
                    while (accounts.MoveNext());

                    if (null != account)
                    {
                        try
                        {
                            // Fetch token silently
                            var result = await publicClientApplication.AcquireTokenSilent(@params.Scopes, account)
                                .ExecuteAsync(cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                            accessToken = new AccessToken(result!.AccessToken, result!.ExpiresOn);
                        }
                        catch (MsalUiRequiredException)
                        {
                            // Fetch token interactively
                            accessToken = await GetAccessTokenInteractively(publicClientApplication, @params.Scopes, username).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // Fetch token interactively.
                        accessToken = await GetAccessTokenInteractively(publicClientApplication, @params.Scopes, username).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Fetch token interactively.
                    accessToken = await GetAccessTokenInteractively(publicClientApplication, @params.Scopes).ConfigureAwait(false);
                }
            }
            else
            {
                throw new Exception($"Authentication Method ${@params.AuthenticationMethod} is not supported.");
            }

            return accessToken;
        }

        private async Task<AccessToken> GetAccessTokenInteractively(IPublicClientApplication publicClientApplication, IEnumerable<string> scopes, string username = null)
        {
            AuthenticationResult result;
            if (username == null)
            {
                result = await publicClientApplication.AcquireTokenInteractive(scopes)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                result = await publicClientApplication.AcquireTokenInteractive(scopes)
                    .WithLoginHint(username)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            return new AccessToken(result!.AccessToken, result!.ExpiresOn);
        }

        #endregion

        #region Private methods
        private IPublicClientApplication GetPublicClientAppInstance(string authority, string audience)
        {
            string authorityUrl = authority + '/' + audience;
            if (!PublicClientAppMap.TryGetValue(authorityUrl, out IPublicClientApplication clientApplicationInstance))
            {
                clientApplicationInstance = CreatePublicClientAppInstance(authority, audience);
                // TODO register clientApplicationInstance.UserTokenCache with MSAL extensions for cache persistence if needed.
                PublicClientAppMap.TryAdd(authorityUrl, clientApplicationInstance);
            }
            return clientApplicationInstance;
        }

        private IPublicClientApplication CreatePublicClientAppInstance(string authority, string audience) =>
            PublicClientApplicationBuilder.Create(this.appClientId)
                .WithAuthority(authority, audience)
                .WithClientName(this.appName)
                .WithDefaultRedirectUri()
                .Build();

        #endregion
    }
}
