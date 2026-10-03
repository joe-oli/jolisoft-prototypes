# DynamicQuestions

A local-first fake CRM host and iframe assessment, plus a separate RJSF foundations workbench. No real CRM or backend is needed.

## Run

```powershell
npm ci
npm run build
npm run dev -- --host localhost --port 6174 --strictPort
```

- Host: `http://localhost:6174/`
- Embedded assessment: `/embedded.html` (opened by the host)
- Schema workbench: `/foundations.html`

The production build emits all three entries. Browser checks run against that build with installed Edge:

```powershell
npm run test:browser
```

## Techniques

The assessment demonstrates schema-driven custom controls, eligibility-based tabs, draft saving/reloading, dirty tracking, validated submission, and completed/viewer read-only states. The host owns localStorage behind a persistence interface; the iframe saves through an asynchronous postMessage adapter with acknowledgements. Both ends validate message source and origin.

The foundations page preserves schema/UI-schema editing, local fixtures, manual draft saving, and custom validation from the historical RJSF experiments. It shares controls and dependencies, but runs separately from the host/assessment workflow.

Instructions render plain text. Local access controls are demonstrations, not real authentication. Browser storage is scoped to the current origin.

See `../doco/304-dynamic-questions-run.md` for lifecycle checks and `../doco/203-rjsf-foundations.md` for extraction lineage.
