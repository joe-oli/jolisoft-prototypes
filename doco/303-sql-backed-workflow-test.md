# SQL-Backed Workflow Test

The API supports two profiles:

- Default: `JolisoftDemoDB` through the generated EFLayer and `dev_user1`.
- `-UseInMemory`: explicit small in-memory test, no database connection.

Run the normal SQL-backed profile from the repository root. The private launcher contains the local demo credential:

```powershell
.\Start-Local-Stack.ps1
```

Then open `http://localhost:6173/`, create a workflow, and verify it appears in the list. Stop and restart the API/UI stack, then refresh the page. The record should still be present because it is stored in `dbo.WorkflowRecords`, not the in-memory fallback. The API-level create/list test has already passed; the remaining check is the visible browser restart cycle.

The API connection uses the generated `JolisoftDemoDbContext` from `Jolisoft.Demo.EFLayer`. The SQL project and deployed database remain authoritative.