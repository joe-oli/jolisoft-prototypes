# Jolisoft Prototypes Handoff

Last updated: 2026-10-03

## Read this first

This is a private, local-first archive of Jolisoft prototypes. Follow root `AGENTS.md` and verify the required root `temp/` archive before repository work. Do not modify that historical source material. The owner's references under `secrets/` remain intentional private material.

## Current shape and decisions

- Active backend: .NET 10, controller-based ASP.NET Core Web API, EF Core 10.0.8.
- `Jolisoft.Demo.Database` owns the schema through its Microsoft.Build.Sql project.
- `Jolisoft.Demo.EFLayer` owns generated entities and context. Regenerate; do not hand-edit.
- `ShowcaseShell` is the React/TypeScript/Vite catalog, with Bootstrap and CSS Modules for new local overrides.
- `DynamicQuestions` builds three static entries: fake CRM host, embedded assessment, and a separate RJSF foundations workbench.
- External platform references are local `ACME` fakes. No real CRM/cloud connection is required.
- API port: 6041. Catalog port: 6173. DynamicQuestions port: 6174.

## Linux SQL Server is the active target

The owner's 2026-10-03 update in `secrets/sqlserver.md` supersedes the old Windows instance. New work targets `192.168.1.20,1433`, server `ubuntu24-sqlserver`, database `JolisoftDemoDB`.

Connectivity was verified using `dev_user1`. It maps to database user `dev_user1`; compatibility level is 160. `dbo.WorkflowRecords` exists. Use the demo login for application, scaffolding, and schema work. Reserve `sa` for necessary one-time administration.

`Start-Local-Stack.ps1` now defaults to Linux SQL Server and supplies the private demo connection to the child API process. `-UseInMemory` is an explicit test profile. The credential remains in the agreed private launcher for convenience.

### Workflow persistence verified

A headless Edge browser created a workflow through the catalog UI and its Vite proxy. Both API and UI were stopped and restarted, then the same record ID and title were read back through the API and browser.

Verification record: `9dad8e03-33bc-494c-9c0d-0df6ed668c27`. This intentional demo row remains in the Linux database. Screenshots and server logs are under ignored `artifacts/verification/`.

The repeatable check is `scripts/verify-workflow-restart.mjs`; it requires the private connection in `ConnectionStrings__DefaultConnection`, installed frontend dependencies, the built API DLL, and Edge. It starts hidden servers and stops only the processes it created. It refuses to run over an existing stack on the same ports.

### Deployment plan verified

`Publish-Local-Database.ps1` built the DACPAC and generated a deployment plan against Linux successfully. The prior SQLPackage initialization stall did not recur.

The helper now sets `ScriptDatabaseOptions=False` alongside `BlockOnPossibleDataLoss=True` and `DropObjectsNotInSource=False`. This prevents SQL project defaults from changing server-managed recovery, page verification, and query-store settings.

Reviewed plan: `artifacts/database/JolisoftDemoDB-20261003-215132.sql`. It contains no schema or database-option changes. Nothing was published because the deployed table already matches the SQL project. SQLPackage is installed locally at `artifacts/tools/sqlpackage.exe`; add that folder to PATH when using the helper. The SQL SDK emits an available-version warning for its existing preview SDK; no SDK upgrade was made.

## DynamicQuestions lifecycle

The hosted assessment now demonstrates:

- Host readiness/context handshake with source and origin checks.
- Named RJSF controls for text, textarea, checkbox, radio, select, date, and plain-text instructions.
- Eligibility-driven tabs: `No` skips Assessment and removes its obsolete answers.
- Host-owned localStorage persistence keyed by record ID, behind `AssessmentStore`.
- An asynchronous iframe adapter with request IDs, save acknowledgements, and timeout handling.
- Save draft without complete validation; Save & Next validates the current step and persists before moving.
- Dirty-state tracking, an unload warning, and discard confirmation when the fake host reloads or changes access.
- Whole-assessment validation on submission, including when users navigate directly to Review.
- Draft and Validated records remain editable. Editing/saving a validated record returns it to Draft; submission metadata is recreated only on validation.
- The host can mark a saved Validated record Completed. Completed records and viewer access disable editing; the host also rejects save requests for those states.
- A visible saved JSON payload, with audit and submission fields.

The host is a demonstration boundary, not real authentication. Persistence is browser-local and scoped to its origin. Changing host/port/browser changes the local storage context.

## RJSF foundations extraction

`DynamicQuestions/foundations.html` is a separate page without the CRM host or wizard. It preserves selected techniques from `temp/testRepo3/dynamic-form-app`: schema/UI-schema editors, local fixtures, named control registration, draft saving without validation, and explicit validated saving with a custom rule.

It shares the DynamicQuestions build and controls to avoid duplicating dependencies. Invalid editor JSON does not replace the last working preview; rendering errors can be recovered by applying corrected schemas. Local saved schema, UI schema, and answers can be reloaded. See `doco/203-rjsf-foundations.md` for lineage and boundaries.

The catalog now links to both the hosted assessment and foundations workbench. Start DynamicQuestions separately to follow those links.

## Commands

```powershell
# Backend
.\Build-Local-Backend.ps1

# Catalog
Set-Location .\ShowcaseShell
npm ci
npm run build

# Hosted assessment and foundations
Set-Location ..\DynamicQuestions
npm ci
npm run build
npm run test:browser
npm run dev -- --host localhost --port 6174 --strictPort
```

Launch the normal visible API/catalog stack from the repository root with `.\Start-Local-Stack.ps1`. Do not run duplicate long-running commands while a visible terminal command is active.

Stop the API before EF scaffolding. Follow `doco/302-ef-database-first-scaffold.md` and retain `Command Timeout=180`, `--data-annotations`, and `--verbose`.

## Next work after this checkpoint

Validation on 2026-10-03: backend build, SQL project build, both frontend builds, both frontend lint checks, all six production-build browser tests, and the SQL-backed API/UI restart check passed. Temporary verification servers were stopped.

1. Review and commit this verified Linux/lifecycle/foundations checkpoint, keeping the owner's pre-existing `secrets/sqlserver.md` edit distinct when reviewing the diff.
2. Inventory remaining historical techniques into `doco/102-*.md` before selecting the next mini-system.
3. Prioritize a local fake-backed WPFTools example or document/blob handling slice according to which historical techniques are most useful to preserve.

There is no pending database deployment or EF regeneration for this checkpoint: the SQL schema did not change.
