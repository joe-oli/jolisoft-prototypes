# Jolisoft Showcase Plan

Status: implementation started; first API slice complete; active backend targets .NET 10
Last revised: 2026-09-06

Proposed repository name: `jolisoft-prototypes`

## Purpose

Create a private, local-first showcase of the useful techniques accumulated across the historical source projects. The result is a working personal archive with an online catalog, several buildable examples, and offline documentation for future reference.

The original `temp` folder is source material during this work. It must not be modified or deleted by the implementation. It may be manually deleted later after salvage is complete.

## Agreed direction

- The showcase is private and personal, not intended for publication.
- The repository name is `jolisoft-prototypes`.
- The showcase is a family of related systems and independent demonstrations, not one forced production application.
- The outer catalog is a modern Vite + React + TypeScript application.
- Bootstrap CSS, or `react-bootstrap` where useful, provides global styling.
- CSS Modules provide local component overrides.
- Related full systems use consistent namespaces such as `Jolisoft.Demo.WebAPI` and `Jolisoft.Demo.WebUI`.
- Active new prototype projects target .NET 10 LTS. Historical projects under `temp` retain their original frameworks and are not changed.
- Independent demonstrations have their own folders, initially including `DynamicQuestions` and `WPFTools`.
- `doco` is repository-wide offline Markdown for decisions, historical explanations, and setup notes. It is not required to be rendered by the catalog.
- `secrets` is repository-wide private reference material for credentials, connection details, local settings, and environment-specific values. It remains local to this personal archive.
- `temp` remains untouched during curation.

## Demonstration shape

The first milestone is a thin vertical slice proving that the layers can work together:

```text
Vite catalog shell -> React UI -> ASP.NET Core API -> middleware -> SQL Server/LocalDB -> fake external service
```

The showcase should contain two or three complete mini-systems that build and compile across tiers. Examples are allowed to be simplified; they do not need to reproduce every field in the historical business applications.

## Minimal schemas

Database schemas are technique-oriented rather than full business reconstructions. Keep fields needed to demonstrate the workflow, relationships, validation, and audit behavior. Fields such as `created_at` and `created_by` are generally useful. Omit irrelevant business-specific fields and large collections of fields that do not teach a technique.

The default relational target is SQL Server/LocalDB. Each system may use a smaller schema than its historical source, provided the README explains what was intentionally omitted.

## Fake and local service boundaries

Cloud and platform dependencies should be replaceable by local fakes or mocks by default, including:

- Dataverse/Dynamics
- Azure services
- SharePoint
- Blob Storage
- Application Insights

The blob abstraction should expose an S3-like API. Begin with a fake, in-memory, or filesystem implementation. A future SeaweedFS adapter may be added later; do not assume MinIO.

## Candidate systems

### Jolisoft full system

A related ASP.NET Core and React system demonstrating database, API, middleware, UI, validation, document or file handling, and fake external integrations. Use a deliberately reduced schema and preserve the architectural techniques rather than every historical field.

Suggested related namespaces:

- `Jolisoft.Demo.WebAPI`
- `Jolisoft.Demo.WebUI`
- shared contracts and infrastructure as needed

### DynamicQuestions

A React/TypeScript demonstration based on the RJSF work. Compare the implementations from `temp/testRepo3`, `temp/testRepo4`, and `temp/testRepo6`.

Keep both only when they demonstrate materially different techniques. Otherwise combine the strongest parts, including schema/UI-schema handling, validation, custom fields/widgets, multi-step behavior, and the CRM/controller boundary.

### WPFTools

A focused WPF/Dataverse tooling demonstration covering useful parts of the later experiments, such as generated entities, filtered model generation, metadata inspection, service-context choices, and retry behavior. Cloud access should be optional and a local fake should be available for the demonstration path.

### Observability

Preserve the strongest request/response logging and Application Insights-style middleware as part of a full system or as an independent demonstration, depending on which boundary keeps the example clearest.

## Documentation numbering

Use numbered Markdown files so the archive remains easy to scan and extend:

- `101-*.md`: overall plan and repository decisions
- `102-*.md`: source inventory and lineage
- `201-*.md`: full-system design notes
- `202-*.md`: dynamic-question comparison and decision
- `301-*.md`: implementation and run instructions
- `401-*.md`: historical technique notes

## Implementation sequence

1. Create the Vite React TypeScript catalog shell with Bootstrap and CSS Modules. **Complete.**
2. Add catalog entries and navigation for the planned systems. **Complete.**
3. Add one thin vertical full-system slice with a minimal schema, API, middleware, UI, and fake service boundary. **Complete for the in-memory local profile; SQL Server/LocalDB validation remains next.**
4. Compare the dynamic-question implementations and document whether they remain separate or are merged.
5. Add the remaining full-system and WPF/Dataverse examples incrementally, keeping each buildable.
6. Keep configuration and private local references in `secrets`.
7. Update this document as decisions change.

## Scope boundary

Do not modify or delete source material under `temp` during curation. Other than that, the useful techniques, integrations, mocks, local services, full-system examples, and supporting tooling are all candidates for inclusion in the personal archive.
