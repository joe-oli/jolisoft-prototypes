# How to run Jolisoft Prototypes

These are separate local demonstrations. DynamicQuestions and its foundations page do not require the API/catalog stack.

## 1. Initial setup and builds

```powershell
Set-Location C:\PROJECTS\EMDG
.\Build-Local-Backend.ps1
Set-Location .\ShowcaseShell
npm ci
npm run build
Set-Location ..\DynamicQuestions
npm ci
npm run build
```

The required historical `temp/` folder must exist at the repository root before repository work. Do not modify it.

## 2. DynamicQuestions and RJSF foundations

```powershell
Set-Location C:\PROJECTS\EMDG\DynamicQuestions
npm run dev -- --host localhost --port 6174 --strictPort
```

Open `http://localhost:6174/` for the fake CRM host and iframe assessment. Open `http://localhost:6174/foundations.html` for the separate schema/UI-schema editor and manual validation workbench.

Save & Next persists a validated step through the host. Save draft permits incomplete answers. Refresh reloads saved answers. Submission validates all applicable steps and produces a Validated record; Mark completed in the host makes it read-only. Viewer access is also read-only. Choose No in a fresh demo record to see the assessment tab skipped. The host owns browser-local storage; it does not use SQL for assessments.

For full walkthroughs see `doco/304-dynamic-questions-run.md` and `doco/203-rjsf-foundations.md`.

Stop Vite with Ctrl+C. Browser checks start their own production preview, so stop any existing server on port 6174 before running:

```powershell
npm run build
npm run test:browser
```

These checks use installed Edge in headless mode and isolated browser storage.

## 3. API and catalog stack

```powershell
Set-Location C:\PROJECTS\EMDG
.\Start-Local-Stack.ps1
```

The launcher opens visible API/UI terminals and defaults to Linux SQL Server `192.168.1.20,1433`, database `JolisoftDemoDB`, using the private demo login.

- API: `http://localhost:6041/api/workflows`
- Catalog: `http://localhost:6173`

The catalog links to DynamicQuestions and the foundations workbench. Start DynamicQuestions separately before following those links.

For an explicit in-memory API demonstration:

```powershell
.\Start-Local-Stack.ps1 -UseInMemory
```

SQL-backed workflows survive API/UI restarts; in-memory workflows last only for the API process. See `doco/303-sql-backed-workflow-test.md` for the repeatable browser restart script.

## 4. Database deployment plan

Supply the private Linux connection string as described in `doco/305-sqlpackage-publish.md`. If needed, install SQLPackage into the ignored workspace tool folder:

```powershell
Set-Location C:\PROJECTS\EMDG
dotnet tool install Microsoft.SqlPackage --tool-path .\artifacts\tools
$env:PATH = (Join-Path (Get-Location) 'artifacts/tools') + ';' + $env:PATH
.\Publish-Local-Database.ps1
```

Review the generated `artifacts/database/*.sql` plan. The helper preserves server-managed database options and changes nothing without explicit publish:

```powershell
.\Publish-Local-Database.ps1 -Publish
```

The Linux plan reviewed on 2026-10-03 requires no schema changes, so there is nothing to publish for that checkpoint. If a future schema change is deployed, stop the API and regenerate the EF layer using `doco/302-ef-database-first-scaffold.md`.

## 5. WPFTools desktop query example

From the repository root:

```powershell
dotnet run --project .\WPFTools\Jolisoft.WPFTools\Jolisoft.WPFTools.csproj
```

This independent Windows desktop demo needs no API, SQL, or Dataverse service. Toggle the active and expiry filters and Join active organisation, then click Run SDK query. With the default filters, joined mode returns three assessments across two pages and displays Organisation names; turning the join off returns six assessments across three pages. Turning off all three checkboxes returns eight rows across four pages. The status reports the retrieve-all loop's requests, using two rows per page. It evaluates real SDK query objects over fictional records. See `WPFTools/README.md` for current limitations and headless checks.

Paging is automatic: Run SDK query retrieves all pages through SDK service calls and displays every returned row together in the grid. There are no Next/Previous controls. The status's page count refers to retrieval requests, not UI pages.

The window automatically shows the fixed assessment state choices inline: `0 = Active; 1 = Inactive`. These definitions are independent of the selected filters and supply the grid's State labels. They are retrieved from local fixture metadata through the SDK request boundary when the query runs; no separate button or Dataverse connection is needed.

Click Retry scenarios… at the top right to open a separate window. Select a scenario and click Run scenario: temporary failures recover on attempt 3, repeated temporary failures stop at attempt 3, and an unsupported query stops at attempt 1. The visible trace shows attempts and waits (200 ms, then 400 ms). Every run starts fresh; closing the retry window cancels pending waits. These are local simulations, not real Dataverse fault/throttling handling.

Click SDK LINQ… to open a separate typed-query window. All three checkboxes start checked: the active/current query joined to an active organisation with registration ACME-LOCAL-001 returns three assessments and Organisation names. Turn off Join active organisation to see six. With the join enabled, either assessment filter alone returns four rows; neither returns five. Without the join, either filter alone returns seven; neither returns eight. Click Run SDK LINQ after changing checkboxes. The trace shows actual SDK-generated predicates, join alias and link details. Models are handwritten for local fixtures.

The root solution and `Build-Local-Backend.ps1` include all three WPFTools projects. In the catalog, the WPFTools card takes you to these desktop run instructions. Run the command above in a terminal to open the WPF window.
