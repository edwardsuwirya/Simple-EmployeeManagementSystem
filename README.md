# Employee Management System

The purpose of this repository is dot net core 8 hands on only

## How to run

1. Please make sure, you already hace a running SQL Server database

2. In Server project folder > appsettings.json, change connection string for IP address and database name of your database server
3. Change directory to Server Project folder, and then Create user secret
   ```
   dotnet user-secrets set "JwtSection:Key" "<Your JWT Secret Key>"
   dotnet user-secrets set "JwtSection:Issuer" "<The issuer>"
   dotnet user-secrets set "DbCon:user" "<Database user id>"
   dotnet user-secrets set "DbCon:password" "<Database user password>"
   ```
4. Change directory to Solution Project Folder
    ```
    dotnet ef database update --project ServerLibrary --startup-project Server
    ```
