# RJSF Foundations Workbench

## Decision

Preserve the testRepo3 foundations techniques as a separate page at `DynamicQuestions/foundations.html`. It has no CRM iframe or wizard and shares the Vite build and named controls with the hosted assessment. This keeps the examples distinct without maintaining duplicate React/RJSF projects.

## Source lineage

Read-only sources under `temp/testRepo3/dynamic-form-app/src`:

- `SchemaEditor.tsx`: edit serialized JSON schema and UI schema, then explicitly apply them to parent state.
- `App.tsx`: hold schema state outside the rendered form and switch between focused demonstrations.
- `SingleForm.tsx`: maintain form data separately, save drafts without validation, trigger validated submission through a form ref, and add custom validation.
- `DataEntryBS.tsx` and `CustomFields/`: named field/widget registrations and local fixture-driven rendering.

The extraction preserves these techniques rather than copying the old CRA/router/Bootstrap 4 setup. Historical files were not modified.

## New implementation

- `src/foundations/fixtures.ts`: small proposal schema and UI-schema fixture exercising shared controls.
- `src/foundations/main.tsx`: paired editors, atomic Apply schemas, working preview, manual Save draft, Validate & Save, and Load saved draft.
- `src/foundations/Foundations.module.css`: local workbench styling over Bootstrap.

Invalid JSON or an invalid JSON schema leaves the prior preview intact. An error boundary displays unsupported preview errors and lets the user correct the editors. Applying schemas retains current answers for comparison. Restore fixture resets editor text; Apply updates the preview.

The custom rule requires at least ten characters of notes for a High risk proposal. Draft saving permits incomplete data; validated saving runs JSON-schema and custom validation first.

Local storage key: `jolisoft.foundations.v1`. It stores schema, UI schema, answers, and Draft/Validated status together. The workbench supports object-root JSON schemas and trusted local experimentation. It has no platform persistence, completion workflow, or real CRM adapter.

## Run

Start DynamicQuestions on port 6174 as described in `304-dynamic-questions-run.md`, then open `http://localhost:6174/foundations.html`. The catalog and fake host both link to this page.

Build and browser verification are shared with DynamicQuestions:

```powershell
npm run build
npm run test:browser
```
