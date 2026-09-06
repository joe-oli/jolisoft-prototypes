# Jolisoft.Demo.EFLayer

Database-first EF Core layer for the Jolisoft demo system.

- The SDK-style SQL project owns the schema.
- The deployed SQL Server database is the scaffolding source.
- `Models/Generated` contains generated Data Annotation entities.
- `Data/Generated` contains the generated DbContext.
- The Web API references this project and owns controllers, middleware, and application services.

Regenerate from the repository root using the command in `doco/302-ef-database-first-scaffold.md`.