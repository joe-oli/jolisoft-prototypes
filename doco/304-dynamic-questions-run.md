# DynamicQuestions Iframe Demo

## Run and build

From the repository root:

```powershell
Set-Location .\DynamicQuestions
npm ci
npm run build
npm run dev -- --host localhost --port 6174 --strictPort
```

Open `http://localhost:6174/`. The fake CRM host loads `/embedded.html` in an iframe. The separate foundations workbench is `/foundations.html`. The production build explicitly emits all three HTML entries and their static assets.

## Lifecycle walkthrough

1. Choose Yes and use Save & Next. The host saves the eligibility draft before Assessment opens.
2. Fill the project name and risk level; exercise checkbox, date, and notes controls. Save draft also allows incomplete answers.
3. Refresh or use Reload saved assessment. Saved answers are loaded through host context; unsaved host reloads require discard confirmation.
4. Save & Next to Review, then Submit JSON to host. Whole-assessment validation prevents direct Review navigation from bypassing required answers.
5. The host displays the persisted Validated payload, including submission metadata. Mark completed changes it to Completed and reloads a read-only assessment.
6. Access = Read-only viewer disables editing even on a Draft or Validated record. The host rejects saves in viewer mode or for Completed records.
7. In a fresh browser profile or after removing only this demo's local storage entry, repeat with No. Assessment is disabled/skipped; the submitted answers contain only the eligibility answer.

The local record storage key is `jolisoft.assessment.v1.ACME-LOCAL-APPLICATION-001`. Storage belongs to the fake host and persists within the same browser and origin. The app does not use the SQL database for these assessments.

## Adapter and message boundary

`src/adapters/contracts.ts` defines typed host context, record, and save messages. `LocalAssessmentStore` implements a persistence interface using host localStorage. `IframeAssessmentAdapter` sends a save request with a request ID and waits for the host acknowledgement, with a timeout.

The host and iframe both check source window and origin. The host checks the request record, answer types, validation, and editability before storage. Save failure leaves the embedded answers dirty. These are local demonstration controls; the fake access selector is not real CRM authorization.

## Control library and fixtures

`src/fields` registers text, textarea, checkbox, radio, select, date, and instruction controls. `src/schemas/assessment.ts` defines the deliberately small schema/UI-schema fixture. Instructions render plain text rather than arbitrary HTML.

## Browser checks

```powershell
npm run build
npm run test:browser
```

The Playwright suite uses installed Edge in headless mode and starts a production preview on port 6174. Stop an existing server on that port first. Tests use isolated browser contexts and do not alter the owner's saved demo answers.

The suite covers eligibility paths, refresh persistence, submission metadata, completed/viewer controls, validation bypass, discarded assessment answers, unsaved-change cancellation, storage failure, message-origin checks, host save rejection, and the foundations editor/custom validation.

All six browser tests passed against the production build on 2026-10-03. Backend and both frontend builds passed, and both frontend lint checks passed.
