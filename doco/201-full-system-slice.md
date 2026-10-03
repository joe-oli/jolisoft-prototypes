# First Full-System Slice

Status: SQL-backed browser create/list/restart verified on Linux, 2026-10-03.

## Purpose and flow

The deliberately small slice demonstrates validation, audit fields, persistence, request timing, and a fake external platform boundary.

```text
ShowcaseShell -> Vite /api proxy -> WorkflowsController -> IWorkflowStore
                                                          -> EfWorkflowStore -> Linux SQL Server
                                                          -> IPlatformGateway -> FakePlatformGateway
```

`RequestTimingMiddleware` logs method, path, status, and elapsed time. `WorkflowsController` supports GET and POST at `/api/workflows`. The fake gateway returns `ACME-LOCAL-*` references without external network access.

## Database-first ownership

`Jolisoft.Demo.Database/Tables/WorkflowRecords.sql` owns `Id`, `Title`, `Status`, `CreatedBy`, and `CreatedAt`. Generated entity and context files belong in `Jolisoft.Demo.EFLayer`. Data Annotations are requested during scaffolding; generated fluent mapping can remain when required by EF.

The API owns controllers, DTOs, middleware, and application services. Do not hand-edit generated EF output or use EF migrations as the schema authority.

## Profiles

`Start-Local-Stack.ps1` defaults to `JolisoftDemoDB` on Linux SQL Server at `192.168.1.20,1433`. The explicit `-UseInMemory` launcher option uses an in-memory store. Direct API startup without a connection string also selects the in-memory store.

The in-memory profile persists only for the lifetime of its API process. SQL-backed records survive restarts. The browser/API/UI restart cycle was verified on 2026-10-03; see `303-sql-backed-workflow-test.md`.

## Follow-on work

The first vertical slice is complete. Keep its schema small when adding selected historical techniques such as document/blob handling or richer observability.
