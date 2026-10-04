# Source inventory and next extraction

Surveyed: 2026-10-03; selected candidates reviewed again on 2026-10-04. This is a working inventory of project families and selected source techniques, not a claim that every archived file has been reviewed or salvaged. Everything under root `temp/` remains read-only.

## 1. Archive families

| Archive area | Observed material | Curation direction |
| --- | --- | --- |
| `temp/testRepo` | Route experiments, reference material, images, and ZIP archives | Review individual assets when a selected example needs them; ZIP contents are not inventoried yet. |
| `temp/testRepo2` | Earlier multi-tier API/UI, CRM/shared layers, and test projects | Compare with later full-system versions before copying techniques; avoid maintaining duplicate systems. |
| `temp/testRepo3` | RJSF foundations, request/response logging, blob abstractions, MVC examples, and clipboard tooling | Foundations already extracted; logging, blob handling, and clipboard utilities remain candidates. |
| `temp/testRepo4` | Focused CRM DynamicQuestions, working variants, CLI notes | Focused form techniques already extracted; retain historical packaging/model-generation references. |
| `temp/testRepo5` | Later full systems, desktop integration tooling, API/EF examples, Node backend, and authentication experiments | Candidate document/service boundaries, serialization, and local integration tools; compare overlapping versions first. |
| `temp/testRepo6` | WPF/CRM workbench, generated models, DynamicQuestions evolution, MVC/API examples, TypeScript and embedded-content experiments | Selected WPF/Dataverse scope extracted and visually verified. Broader CRM modules remain separate historical references. |
| `temp/testRepo7` | Operational notes, query notes, screenshots, and reference documents | Supporting evidence; inspect selectively and avoid copying private operational details into demos. |

## 2. Technique priorities

| Technique | Source evidence | Current state / next action |
| --- | --- | --- |
| Hosted schema-driven forms and wizard | testRepo4 and testRepo6 DynamicQuestions sources | Extracted into DynamicQuestions; see `202-dynamic-questions-comparison.md`. |
| Schema/UI-schema editor and manual validation | testRepo3 `dynamic-form-app/src/SchemaEditor.tsx` and `SingleForm.tsx` | Extracted into the foundations page; see `203-rjsf-foundations.md`. |
| Database-first API/EF layering | testRepo5 API/EF and full-system project families | Curated workflow slice already demonstrates SQL source, DACPAC, generated EF, controllers, and local platform fake. |
| Dataverse queries, metadata, model filters | testRepo6 `WpfSolution/WpfApp/MainWindow.xaml.cs`, `WpfDotnetCore/MainWindow.xaml.cs`, and model-builder settings | Selected queries, choice metadata, retries and typed SDK LINQ filters/join implemented. Model-filter configuration documented in `204`; live generation deferred. |
| Paged record retrieval | testRepo6 CRM-layer `Extentions/OrganizationServiceExtentions.cs` | Extracted automatic retrieve-all loop with local cookies; no Next/Previous UI. |
| Request/response body logging | testRepo3 `LogToAppInsightsSolution/LogToAppInsightsWebAPI/Middlewares/` and testRepo6 `TryoutWebApi_v2/Middlewares/` | Recommended next. The two request/response middleware pairs are byte-identical by SHA256; no duplicate extraction needed. Active API currently logs timing only. See `205-local-http-diagnostics-plan.md`. |
| Blob/document service boundaries | testRepo3 blob interface/implementation and testRepo6 shared `IDocumentLibrary`, `IDocumentManagement`, `DocumentLibrary` | Reviewed upload/list/get/delete and library add/download/classification boundaries. Recommend second: local file/object store behind an S3-like interface. Historical library implementation couples SharePoint and CRM adapters; extract the boundary rather than those dependencies. |
| Serialization and integration tools | testRepo5 `CIX.TestIntegrationTool/SerializeTest.cs` and selected `MainWindow.xaml.cs` handlers | Reviewed XML namespace/order annotations and SOAP typed-message conversion. Recommend third: fictional XML round-trip workbench; avoid porting the desktop tool's DB/WCF/email/SignalR dependencies wholesale. |
| Clipboard utility | testRepo3 `WPFClipboard` | Correction: this directory contains 38 top-level .txt link-note files and no .cs/.xaml/.csproj/.sln/.zip files found recursively. No buildable clipboard source was found here; reference-only until source is located elsewhere. |
| Authentication and embedded-content examples | testRepo5/testRepo6 authentication projects and export-readiness experiments | Located; defer until their distinct technique and local demonstration path are clear. |

## 3. WPFTools completion and next extraction

WPFTools preserves selected Dataverse query construction and result handling with fictional ACME entities, deterministic records and real SDK types. The owner visually verified the query workbench, retry window, typed SDK LINQ filters and organisation join. All 55 headless checks passed at the latest implementation checkpoint.

The local service deliberately supports a limited query subset and throws for unsupported features. This completes the chosen runnable scope, not every archived WPF feature. Custom ordering, nested/outer joins, broad metadata and live generation remain deferred. See `204-wpf-dataverse-query-plan.md` and `WPFTools/README.md`.

Recommended sequence: bounded local HTTP diagnostics, local blob/document storage, then a focused XML serialization workbench. Logging is the smallest distinct addition to the working controller API and has directly compared source evidence. ZIP archives and authentication/embedded-content experiments still require selective review; this inventory does not authorize deleting `temp/`.

