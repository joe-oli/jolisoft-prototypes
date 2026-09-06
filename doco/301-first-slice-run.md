# Run The First API Slice

From the repository root:

```powershell
dotnet build .\Jolisoft.Prototypes.slnx
dotnet run --project .\Jolisoft.Demo.WebAPI\Jolisoft.Demo.WebAPI.csproj --no-launch-profile --urls http://127.0.0.1:6041
```

The default configuration uses the in-memory store. Create a record:

```powershell
$body = @{ title = 'Prototype workflow'; createdBy = 'local-user' } | ConvertTo-Json
Invoke-RestMethod -Uri http://127.0.0.1:6041/api/workflows -Method Post -ContentType 'application/json' -Body $body
```

List records:

```powershell
Invoke-RestMethod -Uri http://127.0.0.1:6041/api/workflows -Method Get
```

To use SQL Server/LocalDB, add a private `ConnectionStrings:DefaultConnection` value to a local configuration file or the local environment. Do not put machine-specific connection strings into the portable example configuration.