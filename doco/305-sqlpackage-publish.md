# SQLPackage Database Publish

`Jolisoft.Demo.Database` is the schema authority. `Publish-Local-Database.ps1` builds its DACPAC, generates a SQL deployment plan, and only publishes when explicitly requested.

For the high-level SQL project -> DACPAC -> UAT/PROD deployment model, read the [database project README](../Jolisoft.Demo.Database/README.md).

## Prepare the private connection string

Create the untracked private file `secrets/jolisoft-demo.connection-string.txt` with a single connection-string line, or set `ConnectionStrings__DefaultConnection` for the PowerShell session. Do not commit a connection string.

Example private-file content:

```text
Server=192.168.1.20,1433;Database=JolisoftDemoDB;User Id=dev_user1;Password=<private>;TrustServerCertificate=True;Encrypt=False;Application Name=JolisoftSqlPackage;Command Timeout=180
```

## Generate and review a deployment plan

From the repository root:

```powershell
.\Publish-Local-Database.ps1
```

The helper builds the SQL project, then writes the SQLPackage plan into `artifacts/database/`. It does not change the database without `-Publish`. It enables `BlockOnPossibleDataLoss`, disables `DropObjectsNotInSource`, and disables `ScriptDatabaseOptions` so schema deployment preserves the server-managed database settings.

On this workspace SQLPackage is installed at `artifacts/tools/sqlpackage.exe`. In the current PowerShell session, add it to PATH before running the helper:

```powershell
$env:PATH = (Join-Path (Get-Location) 'artifacts/tools') + ';' + $env:PATH
```

The Linux plan generated and reviewed on 2026-10-03 contains no schema or database-option changes. The prior initialization stall did not recur. Nothing was published, and no EF regeneration is needed for that checkpoint.

## Publish

After reviewing the generated script:

```powershell
.\Publish-Local-Database.ps1 -Publish
```

The helper generates a new deployment plan immediately before it runs the publish action. It stops if the build or plan generation fails, so it cannot deploy a stale DACPAC.

## Follow-on action

If the SQL schema changed, stop the API and regenerate the EF layer using `doco/302-ef-database-first-scaffold.md`.
