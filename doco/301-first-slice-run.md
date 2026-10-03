# Run The First API Slice

From the repository root:

```powershell
dotnet build .\Jolisoft.Prototypes.slnx
dotnet run --project .\Jolisoft.Demo.WebAPI\Jolisoft.Demo.WebAPI.csproj --no-launch-profile --urls http://127.0.0.1:6041
```

This direct command without a connection string uses the in-memory store. The normal `Start-Local-Stack.ps1` launcher instead defaults to the Linux SQL-backed profile. Create a record:

```powershell
$body = @{ title = 'Prototype workflow'; createdBy = 'local-user' } | ConvertTo-Json
Invoke-RestMethod -Uri http://127.0.0.1:6041/api/workflows -Method Post -ContentType 'application/json' -Body $body
```

List records:

```powershell
Invoke-RestMethod -Uri http://127.0.0.1:6041/api/workflows -Method Get
```

For the normal SQL-backed catalog workflow, build then run `.\Start-Local-Stack.ps1`. It targets `JolisoftDemoDB` on Linux SQL Server `192.168.1.20,1433` using the private demo login. See `303-sql-backed-workflow-test.md` for verified browser restart persistence. Keep connection details out of portable appsettings.
