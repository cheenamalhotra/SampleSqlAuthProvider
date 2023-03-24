# Sample Sql Authentication Provider implementation

This repository provides a sample implementation of [SqlAuthenticationProvider]() that can be used for `ActiveDirectoryInteractive` authentication mode with System.Data.SqlClient as well as Microsoft.Data.SqlClient (with namespace toggle).

The `CustomAuthProvider` implements `SqlAuthenticationProvider` and the authentication is performed using MSAL (Microsoft.Identity.Client).

## Configure the sample

Provide details in the sample in the `Program.cs` file:
- `appClientId`: Application Client ID of First Party App to be used for authentication (by default SqlClient's client id is used)
- `appName`: Name of client application
- Connection string details
  - `DataSource`: Target Azure SQL Server endpoint
  - `InitialCatalog`: Name of Azure Database instance
  - `UserID`: Username for silent authentication (applicable when cache is persisted)

## How to build the sample

- Using Visual Studio, Build the solution.
- Using Dotnet CLI, run `dotnet build`

## How to run the sample

- Run the exe: `bin\net48\SampleSqlAuthProvider.exe`
- Using Dotnet CLI, run `dotnet run`

## Porting to Microsoft.Data.SqlClient

Microsoft.Data.SqlClient package is referenced in the sample, and to use that, comment out the `using System.Data.SqlClient` namespaces and uncomment `using Microsoft.Data.SqlClient` in below files:
- Program.cs
- CustomAuthProvider.cs
