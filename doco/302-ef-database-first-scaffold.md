# EF Database-First Scaffolding

## Successful pattern

The SQL Server metadata queries used by EF scaffolding may exceed the default command timeout. The working pattern uses the normal named SQL Server instance plus an explicit `Command Timeout=180` in the connection string. Keep `--verbose` enabled so the next run shows where EF is working.

Stop the API before running the command so its output assembly is not locked during the EF build step.

```powershell
$env:DB_PASSWORD = 'dev_user1'
$connection = "Server=192.168.1.80\SQLEXPRESS;Database=JolisoftDemoDB;User Id=dev_user1;Password=$env:DB_PASSWORD;TrustServerCertificate=True;Encrypt=False;Application Name=JolisoftEfScaffold;Command Timeout=180"
Set-Location C:\PROJECTS\EMDG
dotnet ef dbcontext scaffold $connection Microsoft.EntityFrameworkCore.SqlServer `
  --project .\Jolisoft.Demo.EFLayer `
  --startup-project .\Jolisoft.Demo.WebAPI `
  --context JolisoftDemoDbContext `
  --context-dir Data\Generated `
  --output-dir Models\Generated `
  --namespace Jolisoft.Demo.EFLayer.Models.Generated `
  --context-namespace Jolisoft.Demo.EFLayer.Data.Generated `
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

- `Jolisoft.Demo.EFLayer/Models/Generated/WorkflowRecords.cs`
- `Jolisoft.Demo.EFLayer/Data/Generated/JolisoftDemoDbContext.cs`

The SQL project and deployed database remain authoritative. Regenerate these classes after a schema change rather than hand-editing generated files. The API references the EFLayer; it does not own generated persistence code.

## Database ownership

`dev_user1` is the owner of `JolisoftDemoDB` and connects as `dbo`. `sa` is reserved for one-time server/database administration; it is not the application or scaffolding login.

## Database compatibility

`JolisoftDemoDB` is intentionally left at compatibility level `160`, verified on 2026-09-06. The SQL Server engine remains SQL Server 2025; this setting controls database behavior and query compatibility only. It is a practical prototype setting that was tested during the EF metadata investigation and is reversible.

Check the current value:

```powershell
$env:DB_PASSWORD = 'dev_user1'
sqlcmd -S '192.168.1.80\SQLEXPRESS' -d master -U 'dev_user1' -P $env:DB_PASSWORD -Q "SELECT name, compatibility_level FROM sys.databases WHERE name = N'JolisoftDemoDB';"
Remove-Item Env:DB_PASSWORD
```

Restore SQL Server 2025 compatibility later if desired:

```sql
ALTER DATABASE [JolisoftDemoDB] SET COMPATIBILITY_LEVEL = 170;
```