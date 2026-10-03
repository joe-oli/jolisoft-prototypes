# Jolisoft database project: SQL source to DACPAC

This project owns the intended database schema. Make schema changes in its SQL source files, then build them into a DACPAC: a package describing the schema to deploy.

## 1. The high-level deployment model

```text
SQL project changes
    -> CI builds a DACPAC
    -> deploy to UAT and validate
    -> promote the same DACPAC to PROD
```

At each deployment, SQLPackage compares the DACPAC's desired schema with the target database's existing schema. It determines the changes needed to bring that target into line with the package, subject to the deployment options.

The DACPAC describes the complete intended schema rather than a sequence of incremental migration scripts. UAT and PROD can therefore require different deployment scripts even when receiving the same DACPAC, because their starting schemas may differ.

The target database supplies the existing state for comparison. Changes made directly on a server are not automatically captured in the SQL project or its next DACPAC; bring deliberate changes back into the SQL source so the project remains authoritative.

## 2. What deployment still needs

Each environment supplies its own connection details and deployment permissions. A pipeline can generate a deployment script for review before publishing, with approval before production where required.

Schema changes also need a plan for existing data. For example, adding a required column to a populated table may need a default or a staged backfill. A DACPAC does not automatically decide what values those existing rows should receive.

Deployment options matter: they control behavior such as dropping objects absent from the project, blocking possible data loss, and changing database settings. Review the generated changes alongside those options.

## 3. What this repository demonstrates

- [Jolisoft.Demo.Database.sqlproj](Jolisoft.Demo.Database.sqlproj) defines the SDK-style SQL project.
- [Tables/WorkflowRecords.sql](Tables/WorkflowRecords.sql) defines the deliberately small workflow table.
- Building produces `bin/Debug/Jolisoft.Demo.Database.dacpac`; this generated output is excluded from Git.
- [Publish-Local-Database.ps1](../Publish-Local-Database.ps1) builds the package and generates a comparison script against the configured database. It publishes only when run with `-Publish`.

The local helper blocks possible data loss, preserves objects absent from the project, and preserves server-managed database options. Those choices mean deployment is not intended to remove every difference between the target and the package.

This repository currently demonstrates the local build, comparison, and optional publish mechanism. It does not contain a UAT/PROD CI pipeline. The reviewed Linux database plan on 2026-10-03 required no schema changes, so nothing was published for that checkpoint.

For commands and connection setup, see [SQLPackage database publish](../doco/305-sqlpackage-publish.md). After deploying a schema change, regenerate the EF output in `Jolisoft.Demo.EFLayer` using the [database-first scaffold guide](../doco/302-ef-database-first-scaffold.md). The SQL project remains the schema authority.
