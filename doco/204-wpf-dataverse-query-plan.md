# WPFTools: Dataverse queries without a live service

Status: filter, organisation inner-join, local paging, fixture state-choice metadata, deterministic retry and selected typed SDK LINQ filters/organisation join implemented, updated 2026-10-04. Retry and LINQ demos have separate windows. SDK LINQ paging and arbitrary LINQ compatibility remain unverified. See `WPFTools/README.md` for scope and limitations.

## 1. The service boundary

Dataverse SDK queries are objects describing which entities, columns, filters, joins, and pages to retrieve. The SDK sends a `QueryExpression` through `IOrganizationService.RetrieveMultiple` and returns an `EntityCollection`. See Microsoft's [QueryExpression overview](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/queryexpression/overview).

For the local workbench, preserve that boundary:

```text
WPF query action
    -> real SDK QueryExpression
    -> IOrganizationService
        -> default: local fake evaluates against fictional records
        -> optional future adapter: authenticated Dataverse service
    -> SDK EntityCollection
    -> results grid / JSON / diagnostics
```

The proposed fake will inspect the actual query and compute its result from seeded records. Returning the same hardcoded result regardless of filters would fail to preserve the query technique.

The existing Linux SQL database is independent of this slice. Storing fixture rows in SQL alone would not make it understand Dataverse queries; a query evaluator or translation layer would still be necessary. Start with in-memory fixture records to keep the example small.

## 2. Evidence from the archive

- `temp/testRepo6/WpfSolution/WpfApp/MainWindow.xaml.cs` contains QueryExpression examples with equality, date comparisons, null checks, nested joins, and selected/aliased columns.
- The CRM-layer `Extentions/OrganizationServiceExtentions.cs` contains a retrieve-all loop using page number, `MoreRecords`, and `PagingCookie`.
- `WpfDotnetCore/MainWindow.xaml.cs` contains early-bound LINQ queries with a join, active-record checks, registration-number filtering, and nullable expiry dates. It also retrieves choice-field metadata.
- `WpfDotnetCore/Model/builderSettings.json` and `XModel/builderSettings.json` preserve explicit entity filters, wildcard filters, service-context naming, and generation options.

Preserve these techniques with reduced ACME example entities rather than reproducing the historical business schema or copying all generated models.

## 3. Proposed first runnable scope

Use a .NET 10 WPF app and a small query/service layer that can be verified without opening a window. Seed fictional organisations, assessments, and related configuration records, including active/inactive and expired/unexpired cases.

| Technique | Planned local behavior |
| --- | --- |
| SDK entities and lookups | Use `Entity`, `EntityReference`, and `OptionSetValue` in fixtures and results. |
| QueryExpression filters | Evaluate the selected equality, null, date-comparison, and AND/OR examples. |
| Columns and ordering | Return requested attributes and apply explicit ordering. |
| Joins | Support the selected inner joins and SDK-style aliased result values; add other join forms only when needed by an extracted example. |
| Paging | Return deterministic pages with `MoreRecords` and local paging cookies, allowing the retrieve-all loop to run. |
| Metadata inspection | Serve a small documented set of fixture entity/attribute/choice metadata requests. |
| Failure handling | Offer deterministic transient-failure scenarios to demonstrate retry behavior; these do not measure real service throttling. |
| Unsupported requests | Throw an explicit unsupported-feature error, including for FetchXML unless its evaluator is deliberately added. |

Keep query fixtures and expected results reviewable. Include negative cases proving that inactive, expired, or unrelated records are excluded. The WPF UI should show the scenario, returned rows, and diagnostics.

## 4. LINQ and generated models

`OrganizationServiceContext` provides Dataverse's LINQ provider and accepts an `IOrganizationService` implementation. Microsoft's [context documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/organizationservicecontext) describes that constructor boundary.

For the early-bound query demonstration, investigate feeding the real SDK context the local service, then support the SDK requests emitted by the selected query. Verify those requests explicitly. Replacing it with `List<T>.AsQueryable()` would demonstrate LINQ-to-Objects rather than the Dataverse provider.

Start with QueryExpression execution; add the selected early-bound/context example once its local request path is verified. Do not promise arbitrary LINQ-query compatibility.

The archive's model-builder configuration can be documented without a server. Fresh generation using the normal Dataverse model builder needs access to metadata from a real environment. Checked-in local example models must clearly identify their fixture or historical origin; a custom fixture generator, if added, must not be presented as a live Dataverse scaffold.

## 5. What the local demonstration proves

It proves query construction, the supported filtering/join/paging behavior, typed result handling, fixture metadata display, and the app's response to simulated faults.

It does not prove live Dataverse compatibility, security permissions, plugins, calculated fields, collation, timezone behavior, authentic server paging cookies, or real service limits. Server-specific behavior requires integration checks against a matching Dataverse environment.

The fake and future real service can share the same SDK interface and query code. Matching entity logical names and schema, authentication, and integration verification are still required before using the real adapter.

## 6. Completion review — 4 October 2026

The owner visually verified all selected runnable examples, including the typed LINQ organisation join. The implementation checkpoint passed the full solution build with zero warnings/errors and all 55 headless checks. The selected local scope is complete; further emulator features are optional, not completion blockers.

| Planned technique | Delivered scope / deliberate limit |
| --- | --- |
| SDK entities and lookups | Real SDK Entity, EntityReference, OptionSetValue and AliasedValue fixtures/results. |
| Filters and projection | Selected columns, equality, null tests, date ranges and nested AND/OR. |
| Ordering | Stable assessment-ID ordering for local paging. Explicit custom OrderExpression support deferred. |
| Joins | One lookup inner join; root and linked aliased predicates. Nested/outer joins deferred. |
| Paging | Automatic retrieve-all loop, MoreRecords and local continuation cookies; no UI paging controls. |
| Metadata | Published assessment state choices and English labels only; broad metadata deferred. |
| Failures | Separate deterministic retry window with limits, increasing waits and cancellation; no real server fault classification. |
| SDK LINQ | Actual provider translation, typed filter query and organisation join, verified against direct QueryExpression. LINQ paging remains unverified. |
| Unsupported features | Explicit errors, including unknown aliases and translated unsupported operators. |
| Model generation | Handwritten fixture models clearly identified. Archive settings preserved as reference; live regeneration deferred. |

## 7. Model-builder configuration lineage

The archived `WpfDotnetCore/Model/builderSettings.json` lists explicit entity names. Its `XModel/builderSettings.json` contrasts that with account/contact plus a wildcard entity filter. It also demonstrates SDK-message filters, entity/message/option-set output folders, a namespace and `serviceContextName` configuration. Both are read-only historical references.

For a future Jolisoft live model-builder example, use fictional ACME entities, a small explicit filter and Jolisoft namespace/context identity. Current fixtures do not provide the complete metadata required for that generation workflow. Do not describe the handwritten models or this completion review as a successful live scaffold.

