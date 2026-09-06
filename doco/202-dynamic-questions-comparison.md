# DynamicQuestions Comparison

## Decision

Keep both concepts, but not both duplicate implementations:

- Preserve the `testRepo3` CRA project as an RJSF foundations reference. Its schema editor, local fixtures, custom-field experiments, and manual validation examples are materially different.
- Use `testRepo4/CrmDynamicQuestions` as the canonical focused DynamicQuestions extraction. It is self-contained and demonstrates the clearest CRM-oriented form workflow.
- Preserve the broader `testRepo6/POC-dynamic-questions-v2/Main` modules separately as historical CRM integration material. Do not merge those modules wholesale into the focused form demo.
- Do not keep testRepo4 and testRepo6 as duplicate standalone DynamicQuestions projects. Their focused `src` implementations and build configuration are substantially duplicated.

## Evolution

```text
testRepo3/dynamic-form-app
  -> RJSF experiments, schema editor, custom controls, local fixtures

testRepo4/CrmDynamicQuestions
  -> focused CRM assessment, lifecycle states, tabs, dirty state, XRM adapter

testRepo6/POC-dynamic-questions-v2
  -> same focused assessment plus shared Main CRM modules and neighboring checks
```

## Techniques to preserve

The local-first extraction should retain the actual hosted-assessment shape:

- A Dynamics CRM-style host page containing or embedding the React page.
- For the local showcase, prefer a fake CRM host page containing an `<iframe>` that loads the embedded React app. This makes the host boundary visible and reproducible without Dynamics CRM.
- Separately built React assets that can be mounted by a host page as static web-resource files.
- A fake host page for local development that supplies the host context and platform adapter without Dynamics or Dataverse.
- A wizard/stepper assessment flow using `react-tabs`.
- Conditional tab navigation: earlier eligibility answers determine whether tabs are enabled, skipped, or opened directly at the last applicable tab.
- A final submit action that creates the expected JSON payload and sends it through the host/platform boundary.
- Runtime JSON schema and UI-schema driven rendering.
- Custom RJSF fields and widgets that render different question types as appropriate HTML controls, including text, textarea, checkbox, radio button, select, date, and notes/instruction content.
- A visible local host-to-embedded-app boundary so the integration technique can be understood and tested.

The application internals should additionally retain:

- Runtime JSON schema and UI-schema driven rendering.
- Custom controls for radio, checkbox, dropdown, date, notes, and instruction links.
- Eligibility-driven tab navigation.
- Dirty-state tracking and explicit Save & Next behavior.
- Draft, validated, completed, and read-only state handling.
- Read-only UI-schema transformation for completed or unauthorized records.
- A persistence interface replacing direct `parent.Xrm.WebApi` calls.
- Local in-memory behavior by default, with an optional Dataverse adapter boundary.

## Host and build shape

Vite is appropriate for the local development experience and for producing the React build assets. It is not intended to pretend to be Dynamics CRM. The demonstration should have two clear pieces:

```text
Fake CRM Host Page
  -> iframe loads the built DynamicQuestions React asset
  -> provides fake parent/XRM-like context and host callbacks
  -> receives the submitted JSON payload

DynamicQuestions React App
  -> mounts inside the iframe document
  -> renders the RJSF wizard and react-tabs flow
  -> calls the host/platform adapter rather than Dynamics directly
```

During development, Vite can serve the fake host page and provide fast reloads. A production build must also emit static assets that can be copied into a CRM web-resource or another host page. The host boundary should therefore be explicit in code rather than hidden inside Vite-only behavior.

The iframe communication contract should use `window.postMessage` for host-to-app initialization and app-to-host JSON submission. The implementation should validate message origins in the real host shape, while the local fake host can use the configured local origin.

The first practical project shape is:

```text
DynamicQuestions/
  host/
    index.html                 # fake CRM host page
    fakeHost.ts                # host context and submitted-payload display
  src/
    app/                       # embedded React wizard
    fields/                    # RJSF custom fields/widgets
    schemas/                   # small JSON schema and UI-schema fixtures
    adapters/                  # fake host adapter and optional CRM-shaped adapter
  public/                      # static build assets if required by host packaging
```

## Deliberate simplification

The showcase version does not reproduce every CRM field or every historical payload. It will keep a small schema and answer model sufficient to demonstrate the techniques. The local implementation runs without Dynamics or Dataverse by using a fake CRM host, but it deliberately preserves the embedded-host boundary and the shape of the eventual XRM integration.

## Next extraction

Create an independent `DynamicQuestions` React + TypeScript + Vite project using the showcase conventions. Build the fake host and embedded React asset boundary first, then add the RJSF custom fields, `react-tabs` wizard, eligibility-driven navigation, draft/validated state, and final JSON submission. Keep the historical Webpack/XRM packaging decisions documented, but do not make them prerequisites for the local demo.