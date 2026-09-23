# SQLPackage Database Publish

`Jolisoft.Demo.Database` is the schema authority. `Publish-Local-Database.ps1` builds its DACPAC, generates a SQL deployment plan, and only publishes when explicitly requested.

## Prepare the private connection string

Create the untracked private file `secrets/jolisoft-demo.connection-string.txt` with a single connection-string line, or set `ConnectionStrings__DefaultConnection` for the PowerShell session. Do not commit a connection string.

Example private-file content:

```text
Server=192.168.1.80\SQLEXPRESS;Database=JolisoftDemoDB;User Id=dev_user1;Password=<private>;TrustServerCertificate=True;Encrypt=False;Application Name=JolisoftSqlPackage;Command Timeout=180
```

## Generate and review a deployment plan

From the repository root:

```powershell
.\Publish-Local-Database.ps1
```

The helper builds the SQL project, then writes the SQLPackage plan into `artifacts/database/`. It does not change the database without `-Publish`. It enables `BlockOnPossibleDataLoss` and disables `DropObjectsNotInSource` for the prototype's conservative local deployment posture.

## Publish

After reviewing the generated script:

```powershell
.\Publish-Local-Database.ps1 -Publish
```

The helper generates a new deployment plan immediately before it runs the publish action. It stops if the build or plan generation fails, so it cannot deploy a stale DACPAC.

## Follow-on action

If the SQL schema changed, stop the API and regenerate the EF layer using `doco/302-ef-database-first-scaffold.md`.
