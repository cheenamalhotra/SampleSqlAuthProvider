//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace SampleSqlAuthProvider.Authentication
{
    /// <summary>
    /// Configuration used by <see cref="Authenticator"/> to perform AAD authentication using MSAL.NET
    /// </summary>
    public class AuthenticatorConfiguration
    {
        /// <summary>
        /// Application Client ID to be used.
        /// </summary>
        public string AppClientId { get; set; } 

        /// <summary>
        /// Application name used for public client application instantiation.
        /// </summary>
        public string AppName { get; set; }

        public AuthenticatorConfiguration(string appClientId, string appName) {
            AppClientId = appClientId;
            AppName = appName;
        }
    }
}
