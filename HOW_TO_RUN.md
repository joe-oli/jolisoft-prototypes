# How to run Jolisoft Prototypes

This repository contains separate local demonstrations. Start the one you want to inspect; the DynamicQuestions demo does not require the API or ShowcaseShell stack.

## DynamicQuestions

This is a fake CRM host page that loads the React assessment in an iframe. The host and embedded assessment exchange context and the submitted JSON through `window.postMessage`.

```powershell
Set-Location C:\PROJECTS\EMDG\DynamicQuestions
npm run build
npm run dev -- --host localhost --port 6174 --strictPort --open
```

Open `http://localhost:6174/` if the browser does not open automatically.

Verify both flows:

1. Confirm the host shows **Embedded React app connected**.
2. Choose **Yes**, proceed to Assessment, fill the project name and risk level, exercise the checkbox, select, date, and notes controls, then submit. Confirm the host displays the JSON payload.
3. Refresh, choose **No**, and proceed. Confirm Assessment is disabled/skipped, Review is reachable, and submission again displays a JSON payload in the host.

Stop the Vite server with `Ctrl+C` in its terminal.

## API and ShowcaseShell stack

This starts the database-backed workflow API and the catalog shell in separate PowerShell windows.

```powershell
Set-Location C:\PROJECTS\EMDG
.\Start-Local-Stack.ps1
```

Open:

- API: `http://localhost:6041`
- ShowcaseShell: `http://localhost:6173`

For an intentionally small, in-memory API test instead of the normal SQL-backed profile:

```powershell
.\Start-Local-Stack.ps1 -UseInMemory
```

## Build checks

```powershell
Set-Location C:\PROJECTS\EMDG
.\Build-Local-Backend.ps1

Set-Location .\ShowcaseShell
npm run build

Set-Location ..\DynamicQuestions
npm run build
```

## Database deployment plan

Generate a reviewable SQLPackage deployment script without changing the database:

```powershell
Set-Location C:\PROJECTS\EMDG
.\Publish-Local-Database.ps1
```

After reviewing the generated `artifacts/database/*.sql` file, apply a freshly built DACPAC with:

```powershell
.\Publish-Local-Database.ps1 -Publish
```

The connection string must be supplied privately as described in `doco/305-sqlpackage-publish.md`.

Further detail for the DynamicQuestions test is in `doco/304-dynamic-questions-run.md`. Database-first EF guidance is in `doco/302-ef-database-first-scaffold.md`.
