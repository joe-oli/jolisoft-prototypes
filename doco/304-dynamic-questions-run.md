# DynamicQuestions Iframe Demo

## Run

From the repository root:

```powershell
Set-Location .\DynamicQuestions
npm install
npm run dev -- --host localhost --port 6174 --strictPort --open
```

Open the Vite host page at `http://localhost:6174/`.

The root page is the fake CRM host. It loads `/embedded.html` in an iframe, sends fake record context with `window.postMessage`, and displays the final JSON payload received from the embedded app.

## Demonstrated behavior

- The embedded React app announces readiness to the host.
- The host sends record ID, mode, and user context.
- RJSF renders schema-driven question controls.
- `react-tabs` provides the wizard tabs.
- An ineligible answer disables the assessment tab and sends the user directly to review.
- The final submit sends a JSON payload back to the host with `window.postMessage`.

## Verified browser result

Both the normal and skippable eligibility paths have been tested successfully. The normal path produced a validated payload containing `recordId`, `status`, `answers`, `submittedBy`, and `submittedAt`; the host displayed the payload received from the iframe.

## Resume point

The next implementation step is to add named custom RJSF widgets/fields under `DynamicQuestions/src/fields` for text, textarea, checkbox, radio, select, date, and notes/instruction content. Do not redesign the iframe host boundary. After the widgets are added, rebuild and repeat both eligibility browser paths.

## Build

```powershell
npm run build
```

The Vite build includes the host entry and the `embedded.html` entry so the embedded app remains deployable as a separate static web-resource-style asset.