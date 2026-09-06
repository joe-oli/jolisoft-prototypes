# Jolisoft.Demo.WebAPI

The first .NET 10 backend slice for the Jolisoft prototype collection.

It demonstrates:

- ASP.NET Core controller boundaries
- A minimal audited workflow model
- Middleware request timing and structured logging
- An `IWorkflowStore` persistence abstraction
- Offline in-memory execution by default
- Optional EF Core SQL Server/LocalDB persistence
- A fake ACME platform boundary

See `../doco/201-full-system-slice.md` and `../doco/301-first-slice-run.md` for design and run instructions.