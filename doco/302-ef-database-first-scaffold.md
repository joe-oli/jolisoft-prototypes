# EF Database-First Scaffolding

## Successful pattern

The SQL Server metadata queries used by EF scaffolding may exceed the default command timeout. The working pattern uses the normal named SQL Server instance plus an explicit `Command Timeout=180` in the connection string. Keep `--verbose` enabled so the next run shows where EF is working.

Stop the API before running the command so its output assembly is not locked during the EF build step.

```powershell
$env:DB_PASSWORD = 'dev_user1'
$connection = "Server=192.168.1.80\SQLEXPRESS;Database=JolisoftDemoDB;User Id=dev_user1;Password=$env:DB_PASSWORD;TrustServerCertificate=True;Encrypt=False;Application Name=JolisoftEfScaffold;Command Timeout=180"
Set-Location C:\PROJECTS\EMDG\Jolisoft.Demo.WebAPI
dotnet ef dbcontext scaffold $connection Microsoft.EntityFrameworkCore.SqlServer `
  --context JolisoftDemoDbContext `
  --context-dir Data\Generated `
  --output-dir Models\Generated `
  --namespace Jolisoft.Demo.WebAPI.Models.Generated `
  --context-namespace Jolisoft.Demo.WebAPI.Data.Generated `
  --data-annotations `
  --no-onconfiguring `
  --schema dbo `
  --table dbo.WorkflowRecords `
  --no-pluralize `
  --force `
  --verbose
Remove-Item Env:DB_PASSWORD
```

## Result

The command generates:

- `Models/Generated/WorkflowRecords.cs`
- `Data/Generated/JolisoftDemoDbContext.cs`

The SQL project and deployed database remain authoritative. Regenerate these classes after a schema change rather than hand-editing generated files.

## Database ownership

`dev_user1` is the owner of `JolisoftDemoDB` and connects as `dbo`. `sa` is reserved for one-time server/database administration; it is not the application or scaffolding login.