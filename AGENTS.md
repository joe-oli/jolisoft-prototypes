# Jolisoft Prototypes Agent Guide

This is a private personal archive of Jolisoft prototypes. Read [doco/999-handoff.md](doco/999-handoff.md) first for the current state and next steps.

## Required Local Source Archive

Before doing any repository work, verify that `./temp/` exists directly under the repository root, alongside this `AGENTS.md` file. This is the required local source archive for this workspace. It is intentionally untracked and is not included in repository history; a fresh clone or a new computer will not contain it automatically.

If `./temp/` does not exist at that repository-root location, stop and tell the user: "I cannot continue making changes on this computer because the required local `./temp/` source archive is missing." Do not continue by guessing, reconstructing, or treating the repository as complete without it. This prerequisite gate does not prevent updating this instruction file to establish or clarify the gate itself.

## Repository Purpose

Extract useful techniques from the historical projects under `./temp/` and preserve them as a small collection of buildable Jolisoft prototypes, full mini-systems, generated database-first layers, and focused demonstrations. The `./temp/` directory, located at the repository root beside `AGENTS.md`, is the source archive; the new repository is the curated result. The owner may delete `./temp/` manually after salvage is complete.

## Working Rules

- Do not modify or delete anything under `./temp/`. It is historical source material and may be manually deleted later by the owner.
- Do not introduce the retired historical project name into new code, namespaces, documentation identity, or project names. Use `Jolisoft` for real project identity and `ACME` for fictional external services.
- Keep the showcase local-first and private. Use fakes/mocks for cloud, CRM, Dataverse, SharePoint, Blob Storage, and observability integrations by default.
- Prefer complete, buildable mini-systems over isolated snippets. Keep schemas intentionally small and technique-focused.
- Use React + TypeScript + Vite for the catalog shell, Bootstrap for global styling, and CSS Modules for local overrides.
- New active .NET projects target .NET 10. Do not change historical frameworks under `./temp/`.
- Keep the API controller-based. Do not replace controllers with Minimal APIs unless explicitly requested.
- Keep database ownership database-first: the SDK-style SQL project owns the schema, and EF classes are generated outputs.
- Keep generated EF code in `Jolisoft.Demo.EFLayer`; the Web API owns controllers, middleware, DTOs, and application services.
- Do not hand-edit generated EF files. Regenerate them from the deployed schema when the SQL project changes.
- Use Data Annotations during EF scaffolding. Generated `OnModelCreating` code may remain when EF requires it.
- `/secrets/` is the intentional private home for credentials, connection details, and local-environment reference material. The private local launcher may contain the agreed demo credential when convenience is explicitly preferred. Do not move, sanitize, or delete the owner's secret reference files unless explicitly asked.
- Use the local ports defined by `Start-Local-Stack.ps1`: API `6041`, UI `6173`.

## Validation

- Stop the API before EF scaffolding so its output assembly is not locked.
- Build with `./Build-Local-Backend.ps1` or `dotnet build ./Jolisoft.Prototypes.slnx`.
- Build the catalog with `npm run build` from `ShowcaseShell`.
- Use `doco/302-ef-database-first-scaffold.md` for the known-good scaffold command. It requires `Command Timeout=180` and `--verbose`.
- Do not run duplicate long-running commands from chat while a visible terminal command is active.
