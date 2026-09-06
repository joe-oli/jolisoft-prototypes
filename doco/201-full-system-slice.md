# First Full-System Slice

Status: API slice complete; backend targets .NET 10

## Purpose

Prove the smallest useful multi-tier pattern before extracting larger historical systems. The first record is intentionally small: it demonstrates validation, audit fields, persistence, middleware, and an external platform boundary without reproducing a complete business schema.

## Current flow

```text
HTTP client -> WorkflowsController -> IWorkflowStore -> in-memory store
                                      |
                                      -> IPlatformGateway -> FakePlatformGateway
```

`RequestTimingMiddleware` surrounds the request and logs method, path, status code, and elapsed time.

## Model

`WorkflowRecord` contains only, with EF mapping expressed through Data Annotation attributes:

- `Id`
- `Title`
- `Status`
- `CreatedBy`
- `CreatedAt`

The model uses `[Table]`, `[Key]`, `[Required]`, and `[MaxLength]`. The DbContext deliberately has no fluent `OnModelCreating` mapping for this small example.

The fields `CreatedBy` and `CreatedAt` remain because audit behavior is part of the technique. The historical business-specific question fields are intentionally absent.

## Persistence boundary

The API uses `IWorkflowStore`:

- No connection string: `InMemoryWorkflowStore` runs locally without infrastructure.
- `ConnectionStrings:DefaultConnection` supplied: `EfWorkflowStore` uses EF Core and SQL Server.

The database-first path is authoritative for the SQL-backed profile: the SDK-style SQL project defines `WorkflowRecords`, publishes the DACPAC, and EF Core reverse scaffolding generates the entity and DbContext. The generated classes are outputs, not the source of the database schema.

## Fake platform boundary

`IPlatformGateway` represents a cloud/CRM-style external service. `FakePlatformGateway` returns an `ACME-LOCAL-*` reference and requires no network access. A later example can add a real adapter without changing the controller contract.

## Next step

Connect the Vite catalog shell to this API with a small create/list workflow view. Keep the API's local fake default and preserve the SQL Server path as an optional configuration profile.