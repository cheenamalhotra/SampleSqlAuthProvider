//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using SampleSqlAuthProvider.Authentication;
using System;
using System.Data.SqlClient;
// using Microsoft.Data.SqlClient; // Use MDS in future!
using System.Threading.Tasks;

namespace SampleSqlAuthProvider
{
    internal class Program
    {
        private const string DEFAULT_APP_CLIENT_ID = "2fd908ad-0664-4344-b9be-cd3e8b574c38";

        public static async Task Main()
        {
            SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryInteractive,
                new CustomAuthProvider(new Authenticator(
                    appClientId: DEFAULT_APP_CLIENT_ID, // TODO Provide own application ID if needed.
                    appName: "Sample Sql Auth Provider Implementation" // TODO Provide application name
                )));

            using SqlConnection sqlConnection = new(new SqlConnectionStringBuilder()
            {
                DataSource = "", // TODO Provide server name
                InitialCatalog = "", // TODO Provide database name
                UserID = "", // TODO Provide username (optionally) as 'loginhint'
                Authentication = SqlAuthenticationMethod.ActiveDirectoryInteractive
            }.ConnectionString);

            await sqlConnection.OpenAsync().ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Connected successfully!");
        }
    }
}