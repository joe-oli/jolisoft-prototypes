# DynamicQuestions

A local-first CRM-style dynamic assessment demonstration. The root page is a fake host and `embedded.html` is the separately mounted React web-resource-style app. The two exchange context and the final payload through `window.postMessage`.

## Run and build

From this directory:

```powershell
npm run dev -- --host localhost --port 6174 --strictPort --open
npm run build
```

Open `http://localhost:6174/`. See `../doco/304-dynamic-questions-run.md` for the workflow checks.

## Control library

`src/fields` contains named RJSF registrations for text, textarea, checkbox, radio, select, and date controls, plus an instruction-content field. `src/schemas/assessment.ts` provides the deliberately small JSON-schema/UI-schema fixture that exercises every control while preserving the eligibility-driven wizard.

The instruction field renders plain schema text; it intentionally does not insert arbitrary HTML from a schema.
