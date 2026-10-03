# SQL-Backed Workflow Test

Verified on 2026-10-03 against Linux SQL Server `192.168.1.20,1433`, database `JolisoftDemoDB`, using `dev_user1`.

## Visible local stack

```powershell
.\Build-Local-Backend.ps1
.\Start-Local-Stack.ps1
```

Open `http://localhost:6173/`, create a workflow, and confirm it appears in the list. Stop and restart the API/UI stack, then refresh. The record remains because it is stored in `dbo.WorkflowRecords` through the generated EFLayer.

`-UseInMemory` is the explicit alternative. Its records are lost when the API restarts.

## Repeatable browser check

After installing dependencies in ShowcaseShell and DynamicQuestions and building the backend, supply the private Linux connection string in `ConnectionStrings__DefaultConnection`, then run from the repository root:

```powershell
node .\scripts\verify-workflow-restart.mjs
```

The script uses Playwright from DynamicQuestions and the installed Edge browser. It refuses to run if ports 6041 or 6173 are occupied. It starts temporary hidden API/UI processes, creates one demo record through the browser, restarts both processes, and verifies the same record ID through the API and UI. It stops its own processes on completion or failure. Each successful run leaves its created demo row in the database.

Screenshots and server logs are saved under ignored `artifacts/verification/`.

The 2026-10-03 run passed with workflow ID `9dad8e03-33bc-494c-9c0d-0df6ed668c27`.
