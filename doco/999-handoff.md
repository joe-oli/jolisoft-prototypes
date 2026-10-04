# Jolisoft Prototypes Handoff

Last updated: 2026-10-04

## Read this first

Resume point agreed with the owner on 2026-10-04: the WPFTools and selected RJSF/AJV chapters are closed. The next authorized direction is [local HTTP diagnostics](205-local-http-diagnostics-plan.md), starting with section 3, slice 1: the bounded collector, controller access and fictional JSON echo endpoint. Implementation has not started. The owner stopped for now; begin that work when they next ask to resume, in passes of about five minutes.

Permanent technique references are [Dataverse examples](206-dataverse-archive-examples.md) and [RJSF/AJV examples](207-rjsf-archive-techniques.md). These preserve their selected examples independently of `temp/`, which the owner intends to delete after curation is finished. Do not reopen the completed chapters without a new request or a concrete defect.

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

WPFTools selected scope is complete and visually verified by the owner, including the typed SDK LINQ organisation join. Its latest implementation checkpoint passed the full solution build with zero warnings/errors and all 55 headless checks. The archive survey and remaining candidates are in `102-source-inventory.md`; completion limits and model-builder lineage are in `204-wpf-dataverse-query-plan.md` sections 6–7. See `WPFTools/README.md` and HOW_TO_RUN section 5 for the runnable examples. The owner requests passes of about five minutes, then wind down and report.

Validation on 2026-10-03: backend build, SQL project build, both frontend builds, both frontend lint checks, all six production-build browser tests, and the SQL-backed API/UI restart check passed. Temporary verification servers were stopped.

1. Agreed next on resume: bounded local HTTP diagnostics in the controller API and a separate catalog view; follow `205-local-http-diagnostics-plan.md`, section 3 in order. Start with the collector/controller/echo slice; implementation has not started.
2. Then extract local blob/document storage, followed by a fictional XML serialization workbench.
3. Continue selective archive review; ZIP contents and authentication/embedded-content experiments remain unreviewed in depth. Clipboard folder has notes only, not located source. Do not treat salvage as complete or delete `temp/`.

There is no pending database deployment or EF regeneration for this checkpoint: the SQL schema did not change.


WPFTools validation updated on 2026-10-04: the owner visually verified the window and checkbox filters. System.Security.Cryptography.Xml was updated from 10.0.9 to patched 10.0.10 after checking the five advisories. The desktop build passed with zero warnings/errors, all seven headless query checks passed, and NuGet's direct/transitive vulnerability audit reported no vulnerable packages for the WPF project using current sources. Related builds must run sequentially to avoid shared-core output locks.

WPFTools is now integrated into the root solution under a WPFTools folder (core, desktop, and console checks). The catalog card reports the implemented local SDK query slice and links to an on-page desktop run guide. Root solution build: zero warnings/errors. Catalog production build and lint: passed. All seven checks passed from the solution-built output. The next feature pass is an inner-join query with reviewable fixtures and negative cases; paging, metadata, retry, and SDK LINQ-context work remain later.

Organisation join checkpoint (2026-10-04): implemented one assessment-to-organisation inner join with LinkCriteria, EntityReference matching, and AliasedValue projections. The desktop has Join active organisation checked by default; default joined results are three assessments, versus six without the join. Full solution build passed with zero warnings/errors; all fourteen checks and catalog build/lint passed. The new desktop checkbox awaits owner visual review. Next slice: deterministic paging and the retrieve-all loop. Nested/outer joins and metadata remain unsupported.

Local paging checkpoint (2026-10-04): the owner visually verified the organisation join. Implemented stable assessment-ID ordering, bounded page sizes, local continuation cookies, and QueryPaging.RetrieveAll. The desktop uses two-row pages: default joined results are three rows across two pages; removing the join gives six across three; removing all filters gives eight across four. The helper restores caller paging settings on success and failure. Local tokens track page number/size, not query identity or snapshots; query and fixtures must remain unchanged during retrieval. Custom ordering and total-count requests remain unsupported.

Validation: root solution build passed with zero warnings/errors; all 23 headless checks passed, including partial/exact page boundaries, empty results, fixture-order independence, invalid tokens and restoration after failure. Catalog production build and lint passed. The owner subsequently visually verified the window. Clarified in WPFTools README section 1 and HOW_TO_RUN section 5 that paging is automatic SDK retrieval: all rows appear together, with no Next/Previous UI controls. Next slice: selected fixture choice metadata and desktop inspection. No database schema change or deployment is required.

Fixture choice metadata checkpoint (2026-10-04): added RetrieveAttributeRequest dispatch for published acme_assessment.statecode by logical name. Each response contains fresh StateAttributeMetadata with English (1033) labels 0 = Active and 1 = Inactive. ChoiceExamples extracts values/labels; the desktop grid uses them and a new Inspect state choices button displays them independently of filters. Other targets, metadata-ID lookup and unpublished retrieval are explicitly rejected. This is a bounded fixture definition, not live schema discovery or full metadata emulation. Source lineage: WpfDotnetCore/MainWindow.xaml.cs metadata request and option iteration, adapted from a picklist to the existing fixture state field.

Validation: root solution build passed with zero warnings/errors; all 32 headless checks passed; catalog build and lint passed. The checks caught missing UserLocalizedLabel initialization, which was corrected before completion. Next slice: deterministic retry/failure scenarios, followed by investigating the real SDK LINQ context. No database changes or deployment required.

Owner feedback on the metadata UI: repeated clicks on Inspect state choices offered no useful visual change. Removed that button and its handler. The fixed choice values/labels and a short explanation now appear automatically inline when the query runs, using the same metadata response as the State column. Run guide, README and catalog guidance reflect this behavior.

Retry checkpoint (2026-10-04): owner requested separate windows for additional demos to avoid crowding the query form. MainWindow now launches/activates one owned RetryWindow via Retry scenarios… at top right. It offers recover-after-two-failures, exhaust-retries, and unsupported-query scenarios, each with a visible attempt/wait trace and final outcome. Every run resets its fixture service and counter. LocalRetryPolicy retries only SimulatedTransientException, uses three total attempts and asynchronous 200/400 ms waits, and supports cancellation on window close. Unsupported queries and unclassified exceptions are not retried. This is a new supporting exercise, not archived retry code or authentic Dataverse throttling/fault-code handling. Normal query/paging calls retain their existing behavior.

Validation: full root solution build passed with zero warnings/errors; all 40 headless checks passed. Tests inject completed waits to verify delay values without sleeping and cover recovery, exhaustion, permanent errors, traces, fresh runs, cancellation and unclassified exceptions. Retry window visual review remains outstanding. Run guide: HOW_TO_RUN section 5 and WPFTools README section 1. Next slice: real SDK LINQ-context investigation. Preserve owner's preference for separate demo windows and meaningful controls. No database deployment is needed.

SDK LINQ checkpoint (2026-10-04): owner visually verified retry window PASS. Added separate SDK LINQ window using real OrganizationServiceContext.CreateQuery<FixtureAssessment>(), SDK proxy registration and a handwritten read-only fixture model. Probe identified typed response requirement; optional service materialization now uses ToEntity<FixtureAssessment>() after ordinary query evaluation. Actual generated QueryExpression requests are observed and displayed, with tests verifying equality/null/date predicates. No LINQ-to-Objects query substitute. Default active/current returns six rows; either filter alone seven; neither eight. Typed joins remain next.

Validation: root solution build passed with zero warnings/errors; all 47 checks passed, covering SDK request path, row identity versus direct QueryExpression, typed nullable data, checkbox combinations, translated predicates and unsupported string-operator rejection. SDK LINQ window awaits owner visual review. Run guide: HOW_TO_RUN section 5 and WPFTools README section 1. No catalog source or database changes in this slice. Next: selected typed LINQ organisation join.

Typed SDK LINQ join checkpoint (2026-10-04): owner visually verified the filter window. Added a handwritten fixture organisation model, assessment lookup property and real SDK Join-before-Where query with linked active/registration predicates. Probe established that the provider emits aliased predicates in root Criteria. The bounded evaluator now validates root/linked aliases and evaluates criteria on joined row pairs, preserving nested AND/OR. Unknown aliases and unsupported joins remain rejected. SDK LINQ window defaults to joined mode (3 rows versus 6 unjoined with assessment filters); it displays Organisation names, join details and aliases. No client-side join fallback.

Validation: full solution build passed with zero warnings/errors; all 55 checks passed. New checks verify typed related names, identity against direct QueryExpression, exclusions, actual translated lookup inner join, aliased predicates, filter combinations, alternate registration, empty matches and unknown aliases. Run guide and README updated. Join visual verification remains outstanding. No catalog source or database changes. The selected WPFTools query/filter/join/paging/metadata/retry/LINQ scope is now implemented; next review completion and remaining archive inventory rather than adding unneeded demos automatically.

Completion and inventory review (2026-10-04): owner confirmed typed LINQ join visual PASS. Closed the selected WPFTools scope, documented delivered/deferred features and model-builder settings lineage, and refreshed catalog status/run guidance. Catalog build and lint passed after those text changes. No backend changes or database deployment in this review; the previous 55-check/clean-solution result remains the implementation evidence.

Compared request/response middleware from testRepo3 and testRepo6: both pairs are byte-identical by SHA256. Active API has timing only, making bounded local HTTP diagnostics the recommended next extraction. Reviewed archived blob/library boundaries and XML serialization handlers for subsequent candidates. Corrected clipboard inventory: WPFClipboard contains 38 top-level text notes and no located project/source/archive files recursively. New plan: doco/205-local-http-diagnostics-plan.md. No new demo implementation has started and temp remains untouched.

Dataverse archive sanity check (2026-10-04): traversed all seven temp families and reviewed representative code beyond the selected WPFTools scope. Results and fictional examples are in doco/206-dataverse-archive-examples.md, with source paths, evidence classifications and scan limits. Confirmed additional metadata discovery, live identity, retrieve-by-ID, ordering/TopCount/filters, nested joins, FetchXML/outer joins, create/update reconciliation, ExecuteMultiple batches, state/status transitions, browser record/action APIs and CRM form behavior. Distinguished generated proxies, unreachable experiments and unimplemented stubs from actual call sites. No WPFTools or temp files changed; no live service calls. Continue with the previously recommended local HTTP diagnostics plan when requested.

Permanent-reference revision (2026-10-04): owner clarified that temp will be deleted permanently after curation. Reworked doco/206-dataverse-archive-examples.md to stand alone, removing historical source-location dependencies and preserving complete adapted SDK helpers, differential comparison/reconciliation/batch logic, browser functions, schema prerequisites, expected outcomes and model-builder settings. Chose documentation rather than expanding the desktop emulator for live/server/browser techniques. WPFTools source unchanged. Extracted Markdown C# examples compile with zero warnings/errors; browser examples pass node syntax checking; JSON settings parse successfully. No live Dataverse call was made. Treat historical paths elsewhere as provenance only; selected techniques must be preserved independently before considering their source salvaged.

RJSF preservation review (2026-10-04): new self-contained doco/207-rjsf-archive-techniques.md preserves structured answers/notes, array and consent semantics, numeric handling, custom error summaries and immutable schema/UI transformations not positively demonstrated by the curated runtime. Includes package roles, installed-version caveat, permanent curated-code locations and scan limits. Strict TypeScript check and seven AJV semantic cases passed. No DynamicQuestions runtime or temp source changes. Documentation requires no historical source paths.
