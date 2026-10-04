# Jolisoft WPFTools

A .NET 10 WPF workbench executes real SDK QueryExpression and typed SDK LINQ queries through a read-only local IOrganizationService. Selected scope is complete and visually verified as of 4 October 2026. No credentials, SQL Server, or Dataverse environment are needed.

## 1. Run

From the repository root:

All three WPFTools projects are included in `Jolisoft.Prototypes.slnx`, under the WPFTools solution folder. `Build-Local-Backend.ps1` builds them alongside the API and EF layer. The catalog's WPFTools card points to its desktop run instructions; run the command below to open the native window.

```powershell
dotnet run --project .\WPFTools\Jolisoft.WPFTools\Jolisoft.WPFTools.csproj
```

The window starts with active, unexpired-or-no-expiry assessments joined to an active organisation with registration `ACME-LOCAL-001`. Toggle the filters or Join active organisation, then click Run SDK query. The Organisation column displays an SDK `AliasedValue` returned by the join. The fixture clock is fixed at 3 October 2026 UTC so boundary and expiry examples are repeatable.

With the default filters, joined mode returns three assessments across two pages. Turn off Join active organisation to see six across three pages, including assessments linked to inactive, unrelated, and missing organisations. Turning off both remaining filters returns all eight assessment fixtures across four pages; organisation fixture rows are never returned as root assessment rows. The status reports the number of requests made by the retrieve-all loop, using two rows per page.

Paging happens automatically through the SDK service calls. Clicking Run SDK query retrieves every page and displays all returned rows together in the grid. There are no Next/Previous controls. The status's page count describes retrieval requests, not separate screens of results; for example, three rows across two pages means requests returned two rows and then one row.

The window automatically displays the fixed `acme_assessment.statecode` choices inline: `0 = Active; 1 = Inactive`. Each query run executes a real SDK `RetrieveAttributeRequest` against the local service; both this explanation and the grid's State names use the returned option labels. These field definitions are independent of the query filters. There is no separate metadata button because repeated requests return the same fixture definitions.

Click **Retry scenarios…** at the top right to open a separate demo window. Select a scenario, then click **Run scenario**. The trace is cleared for every run and shows each attempt and scheduled delay:

- Two temporary failures, then success: returns three assessment rows on attempt 3.
- Temporary failures exhaust the retry limit: stops after attempt 3.
- Unsupported query: stops after attempt 1 without any retry.

The retry window uses real asynchronous waits of 200 ms and then 400 ms, so the UI remains responsive. Closing it cancels pending waits. Reopening it starts a fresh window; opening it while already open brings the same window forward.

Click **SDK LINQ…** to open a separate typed-query window. All three checkboxes start checked: active/current assessments joined to an active organisation with registration `ACME-LOCAL-001` return three rows and related names. With the join enabled, either assessment filter alone returns four rows and neither returns five. Turn off the join to get six with both filters, seven with either alone and eight with neither. Click **Run SDK LINQ** after changing checkboxes. The trace displays actual SDK-generated predicates, join alias and link details. These controls are independent of the main window's filters.

The assessment and organisation models are handwritten for fixtures, with SDK mapping annotations and assembly proxy registration. The service optionally materializes typed responses using `ToEntity<FixtureAssessment>()`; filtering and joining still evaluate the SDK-translated QueryExpression. The provider requires Join before Where and emits organisation filters as aliased root conditions. The evaluator validates those aliases and applies predicates to matched row pairs. No fixture-list `AsQueryable()` or client-side join is used; only display projection happens after materialization. This verifies the selected filters and lookup inner join, not arbitrary LINQ or SDK LINQ paging. See Microsoft's [SDK LINQ examples](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/linq-query-examples).

## 2. Verify without opening a window

```powershell
dotnet run --project .\WPFTools\Jolisoft.WPFTools.Checks\Jolisoft.WPFTools.Checks.csproj
```

Checks cover excluded inactive/expired rows, null expiry, the inclusive boundary date, changed filter results, column projection, SDK request dispatch, and explicit rejection of unsupported operators/FetchXML. Join checks cover inactive/unrelated/missing organisations, changing linked filters, alias metadata and column selection, and explicit rejection of an unsupported outer join even when the root filter matches nothing.

## 3. Supported scope

This checkpoint supports fictional assessment records, selected columns, nested AND/OR filters, equality, null/not-null tests, UTC date comparisons, and one lookup inner join to fixture organisations. The join requires an explicit alias, projects SDK AliasedValue columns, and evaluates LinkCriteria. String equality uses the local .NET ordinal behavior; live Dataverse collation is not emulated.

Paging supports page sizes from 1 to 5000, `MoreRecords`, and local continuation cookies. Results are always sorted by assessment ID before page boundaries are applied. The retrieve-all helper forwards the cookie, increments the page number, and restores the caller's paging settings even on failure. Count 0 means an unpaged local query. Total-record-count requests and custom ordering remain unsupported.

Cookies are local page-number/page-size markers, not authentic Dataverse server cookies. They are not bound to query identity or a snapshot: keep the query and fixtures unchanged during retrieval. The fake uses in-memory filtering and sorting, so this demonstrates the SDK request loop rather than server-side performance.

Metadata support is limited to published `acme_assessment.statecode` requested by logical name. The response contains a fresh SDK `StateAttributeMetadata` with two local English (LCID 1033) option labels. Other attributes/entities, metadata-ID lookup, and `RetrieveAsIfPublished = true` are explicitly rejected. There is no live metadata discovery, localization negotiation, publication lifecycle, or complete entity schema.

The archive's `WpfDotnetCore/MainWindow.xaml.cs` retrieves picklist metadata and iterates option values/labels. This extraction preserves that request/response technique using the existing fixture state field. State fields use [StateAttributeMetadata](https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.metadata.stateattributemetadata?view=dataverse-sdk-latest); the request boundary is documented by Microsoft in [RetrieveAttributeRequest](https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.retrieveattributerequest?view=dataverse-sdk-latest).

Retry behavior is a separate local exercise around a read-only SDK query. It injects a `SimulatedTransientException` before the service call and retries only that explicit exception type, up to three total attempts. The successful call executes the joined fixture query; the permanent-failure scenario actually submits an unsupported Like condition to the evaluator. Other exceptions propagate without retry. The policy accepts an injected delay function so console checks can verify timings without sleeping. This new exercise supports the extraction plan; it is not a copy of an archive retry implementation.

The retry exercise does not classify real Dataverse fault codes, implement Retry-After, emulate server throttling, retry writes, or add retries to normal query/paging calls. It demonstrates bounded attempts, increasing waits, failure classification and cancellation using local fixtures.

Nested/multiple joins, outer joins, custom ordering, broader metadata and live generated models remain unsupported or planned. The real SDK LINQ context is verified for the selected filters and organisation lookup inner join only. The evaluator is deliberately bounded, not a Dataverse emulator.

The core and checks target net10.0. The desktop project targets net10.0-windows. The Microsoft SDK types come from Microsoft.PowerPlatform.Dataverse.Client 1.2.27; see https://www.nuget.org/packages/Microsoft.PowerPlatform.Dataverse.Client/1.2.27.

See ../doco/204-wpf-dataverse-query-plan.md for source lineage and the wider extraction plan.

## 4. Validation and follow-up

The WPF project builds and all seven console checks pass. A concurrent build initially locked the shared core output; rerunning sequentially passed. Build these related projects sequentially.

On 2026-10-04, the owner visually verified the WPF window and checkbox filters. The XML dependency was updated from 10.0.9 to 10.0.10, the patched version identified by the [Microsoft advisory](https://github.com/advisories/GHSA-23rf-6693-g89p). The WPF build then passed with zero warnings and zero errors; all seven console checks passed. NuGet's direct/transitive vulnerability audit reported no vulnerable packages for the WPF project using the current sources. Audit results are time-dependent.

Solution/catalog integration was completed on 2026-10-04. The root solution build passed with zero warnings/errors, the catalog production build and lint check passed, and all seven checks passed using the solution-built output.

The organisation inner-join, local paging, fixture state-choice metadata, deterministic retry and selected typed SDK LINQ filter/join checkpoints are implemented. Broader metadata and SDK LINQ paging remain unverified.

Organisation join validation on 2026-10-04: full solution build passed with zero warnings/errors; all fourteen console checks passed. Catalog build and lint passed. The owner subsequently visually verified the join control.

Paging validation on 2026-10-04: full solution build passed with zero warnings/errors; all 23 console checks passed. Catalog build and lint passed. Paging checks cover row identity across pages, fixture-order independence, partial and exact final pages, empty results, invalid cookies, and restoration of caller settings on success and failure. The owner subsequently visually verified the window; the automatic retrieval behavior is explained in section 1.

Choice metadata validation on 2026-10-04: full solution build passed with zero warnings/errors; all 32 checks passed. Catalog build and lint passed. New checks verify SDK metadata type, option values/labels and language, fixture-state coverage, isolation from caller mutation, and rejection of unsupported targets/retrieval modes. Headless checks caught that constructing a Label does not populate UserLocalizedLabel; fixtures now explicitly set both label forms. Following owner feedback, the metadata button was replaced with an automatic inline explanation. Next: deterministic retry/failure scenarios.

Retry validation on 2026-10-04: full solution build passed with zero warnings/errors; all 40 console checks passed. New checks verify successful recovery and query rows, 200/400 ms waits, attempt traces, retry exhaustion, immediate unsupported-query failure, fresh runs, cancellation during waits, and propagation of unclassified exceptions. The separate retry window awaits owner visual verification. Next: investigate the real SDK LINQ context against the bounded service.

SDK LINQ validation: root solution build passed with zero warnings/errors; all 47 console checks passed. Verified real provider requests, row identity, typed values and null expiry, filter combinations, translated predicates, and rejection of unsupported string operators. The new window awaits owner visual verification. Next: typed LINQ organisation join.

Typed LINQ join validation: full solution build passed with zero warnings/errors; all 55 checks passed. Verified related names, identity against direct QueryExpression, exclusions, actual SDK join/alias translation, filter combinations, alternate registration, empty matches and unknown aliases. Owner previously visually verified the LINQ filter window; the join control awaits visual verification.

Completion review (2026-10-04): owner visually verified the typed LINQ join. Selected WPFTools scope is complete and all demo windows have been visually verified. Latest implementation validation remains 55 passing checks and a clean solution build; no core or desktop code changed in this review. Explicit ordering, nested/outer joins, broader metadata, LINQ paging and live model generation remain deferred. See doco/204-wpf-dataverse-query-plan.md sections 6–7. The next recommended extraction is bounded local HTTP diagnostics, followed by local object/document storage and XML serialization.
