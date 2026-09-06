# Jolisoft Prototypes Handoff

Last updated: 2026-09-06

## Read This First

This is a private personal prototype archive. The root `AGENTS.md` contains the durable working rules. This document records the current implementation state so a future session can continue without reconstructing the conversation.

The overall purpose is to extract useful techniques from the historical projects under `/temp/` and turn them into a curated collection of buildable Jolisoft prototypes, full mini-systems, and focused demonstrations. `/temp/` is the source archive; this repository is the salvage result.

Do not modify or delete `/temp/`. It is historical source material and will be manually deleted by the owner only after salvage work is complete. Keep credentials and local connection references in `/secrets/`, which is the owner's intentional private reference location.

## Current Repository Shape

```text
AGENTS.md
Build-Local-Backend.ps1
Start-Local-Stack.ps1
Jolisoft.Prototypes.slnx
Jolisoft.Demo.Database/
Jolisoft.Demo.EFLayer/
Jolisoft.Demo.WebAPI/
ShowcaseShell/
doco/
secrets/
temp/
```

The repository identity is `jolisoft-prototypes`. The old physical workspace folder name is irrelevant and may be relocated later.

## Technology Decisions

- Active backend projects target `.NET 10`.
- EF Core packages and `dotnet-ef` are on the `10.0.8` line.
- API style is ASP.NET Core MVC with controllers. Do not convert it to Minimal APIs.
- Frontend is React + TypeScript + Vite.
- Bootstrap is the global frontend styling layer.
- CSS Modules are used for local component styling.
- Active local ports are API `6041` and UI `6173`.
- Database schema is database-first/schema-first.
- `Microsoft.Build.Sql` SDK-style SQL project owns the schema.
- EF classes are generated outputs and must not become the schema authority.
- Generated EF code belongs in `Jolisoft.Demo.EFLayer`, not the Web API.
- Data Annotations are requested during scaffolding.
- Fake platform references use the fictional `ACME` name.

## Completed

### Catalog shell

`ShowcaseShell` is a Vite React TypeScript catalog with:

- Catalog cards for the Jolisoft full system, DynamicQuestions, and WPFTools.
- Bootstrap CSS.
- Local CSS styling.
- A workflow panel that calls `/api/workflows` through the Vite proxy.
- Proxy target `http://127.0.0.1:6041`.

The frontend production build has passed with:

```powershell
Set-Location .\ShowcaseShell
npm run build
```

### First API slice

`Jolisoft.Demo.WebAPI` contains:

- `WorkflowsController` with `GET /api/workflows` and `POST /api/workflows`.
- Request timing middleware.
- `IWorkflowStore` with an in-memory local implementation and an EF-backed implementation.
- Fake `IPlatformGateway` returning `ACME-LOCAL-*` references.
- Minimal workflow fields: `Id`, `Title`, `Status`, `CreatedBy`, and `CreatedAt`.
- CORS for the UI port `6173`.

The local in-memory workflow was tested through the browser and survived refresh in that profile.

### SQL database project

`Jolisoft.Demo.Database` is an SDK-style SQL project using:

```xml
<Project Sdk="Microsoft.Build.Sql/2.3.0-preview.2">
```

It contains `Tables/WorkflowRecords.sql` and builds successfully to a DACPAC. The project uses the SQL Server 2022 schema provider because the installed SDK requires it, while the target server is SQL Server 2025.

The table definition is:

- `dbo.WorkflowRecords`
- `Id UNIQUEIDENTIFIER NOT NULL` primary key
- `Title NVARCHAR(160) NOT NULL`
- `Status NVARCHAR(40) NOT NULL`
- `CreatedBy NVARCHAR(160) NOT NULL`
- `CreatedAt DATETIMEOFFSET(7) NOT NULL`

The DACPAC publish through SQLPackage stalled during initialization on this machine. The authoritative table was then created directly in `JolisoftDemoDB` from the same SQL definition using `dev_user1`. Revisit SQLPackage publishing later; do not silently replace the SQL project as the schema authority.

### SQL Server database

```text
Server:              192.168.1.80\SQLEXPRESS
Database:            JolisoftDemoDB
Server engine:       SQL Server 2025 Express
Database owner:      dev_user1
Application login:   dev_user1
Database user seen:  dbo
Compatibility level: 160
```

The password is intentionally not recorded here. See the private local reference under `secrets/`.

Compatibility level `160` was verified and intentionally left in place after the EF metadata investigation. It is reversible to `170` with:

```sql
ALTER DATABASE [JolisoftDemoDB] SET COMPATIBILITY_LEVEL = 170;
```

### EF layer extraction

`Jolisoft.Demo.EFLayer` now owns the generated EF code:

```text
Jolisoft.Demo.EFLayer/
  Data/Generated/JolisoftDemoDbContext.cs
  Models/Generated/WorkflowRecords.cs
```

The Web API references the EFLayer. The API no longer owns a DbContext or generated entities.

The cross-project scaffold succeeded with the following important settings:

- `--project .\Jolisoft.Demo.EFLayer`
- `--startup-project .\Jolisoft.Demo.WebAPI`
- `Command Timeout=180`
- `--data-annotations`
- `--verbose`
- `--schema dbo`
- `--table dbo.WorkflowRecords`

The startup API retains `Microsoft.EntityFrameworkCore.Design` as a private design-time dependency because EF tools require it for cross-project scaffolding. Runtime EF provider ownership remains in the EFLayer.

### SQL-backed API profile

The API now defaults to the SQL-backed profile when launched by `Start-Local-Stack.ps1`. The launcher constructs `ConnectionStrings__DefaultConnection` in the process environment and passes it only to the child API process. The in-memory profile is retained only as an explicit `-UseInMemory` test option.

The SQL-backed profile was smoke-tested successfully: the API listed existing rows, created a workflow through `POST /api/workflows`, returned an `ACME-LOCAL-*` reference, and read the created record back through `GET /api/workflows`. EF logs confirmed SQL `SELECT` and `INSERT` commands against `WorkflowRecords` with a 180-second command timeout.

### DynamicQuestions iframe slice

The independent `DynamicQuestions` Vite project has been created and builds successfully. Its root page is a fake CRM host; `embedded.html` is the iframe-loaded React app. The slice currently demonstrates readiness/context messages, RJSF schema-driven fields, `react-tabs`, eligibility-based tab skipping, review, and final JSON submission back to the host with `window.postMessage`.

Run it on its own port with the command in `doco/304-dynamic-questions-run.md`.

The browser test passed on both paths:

- Normal path: `eligible = Yes`, assessment tab available, review reached, JSON submitted.
- Skippable path: `eligible = No`, assessment skipped/disabled, review reached, JSON submitted.

Verified normal-path payload shape:

```json
{
  "recordId": "ACME-LOCAL-APPLICATION-001",
  "status": "Validated",
  "answers": {
    "eligible": "Yes",
    "projectName": "Test project",
    "hasRisk": true,
    "notes": "Haha ha this is working well"
  },
  "submittedBy": "local-user",
  "submittedAt": "2026-09-06T12:53:24.338Z"
}
```

## Known-Good Commands

### Build backend

```powershell
.\Build-Local-Backend.ps1
```

or:

```powershell
dotnet build .\Jolisoft.Prototypes.slnx
```

### Build frontend

```powershell
Set-Location .\ShowcaseShell
npm run build
```

### Start visible local stack

```powershell
.\Start-Local-Stack.ps1
```

This opens separate PowerShell windows:

```text
API: http://localhost:6041
UI:  http://localhost:6173
```

The normal local workflow is database-backed. This private local launcher contains the agreed demo credential for convenience; it is inherited by the child API process and is not written to appsettings:

```powershell
.\Start-Local-Stack.ps1
```

For a deliberately small in-memory test only:

```powershell
.\Start-Local-Stack.ps1 -UseInMemory
```

### EF scaffold

Stop the API first. Use the complete command in `doco/302-ef-database-first-scaffold.md`. The essential pattern is:

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

Do not use `sa` for normal scaffolding, application access, or schema work. `sa` is reserved for one-time server administration.

## Immediate Next Steps

1. Run the browser workflow with `Start-Local-Stack.ps1` and confirm the SQL-backed row survives an API/UI restart.
2. Add a repeatable SQLPackage publish command or script and diagnose why SQLPackage initialization stalled.
3. Commit the current extraction as a coherent checkpoint before starting DynamicQuestions comparison.
4. **Resume here:** add named custom RJSF widgets/fields in `DynamicQuestions/src/fields` for the historical control techniques: text, textarea, checkbox, radio, select, date, and notes/instruction content. Keep the host/iframe/postMessage architecture unchanged.
5. Add small schema/UI-schema fixtures demonstrating those custom controls, then run `npm run build` from `DynamicQuestions`.
6. Repeat the browser test from `doco/304-dynamic-questions-run.md`: verify both `eligible = Yes` and `eligible = No`, then verify the final JSON still crosses the iframe boundary.
7. Preserve selected testRepo3 schema-editor techniques and decide whether they belong in the DynamicQuestions project or a separate foundations example.

## Do Not Repeat Earlier Mistakes

- Do not use the default EF command timeout for this SQL Server metadata query.
- Do not omit `--verbose` during scaffolding diagnostics.
- Do not scaffold while the API process is running and locking its build output.
- Do not run duplicate SQLPackage or EF commands while another copy is active.
- Do not use `sa` when `dev_user1` is the intended database owner and working login.
- Do not move generated EF code back into the API project.
- Do not modify `/temp/`.
