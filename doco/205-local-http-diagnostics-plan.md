# Local HTTP diagnostics extraction

Reviewed: 2026-10-04. Status: owner agreed this is the next extraction when work resumes; implementation has not started. WPFTools and the selected RJSF/AJV chapter are closed. Start with section 3, slice 1, keeping passes near five minutes.

## 1. Source and current gap

The request and response body middleware under `temp/testRepo3/LogToAppInsightsSolution/LogToAppInsightsWebAPI/Middlewares/` and `temp/testRepo6/TryoutWebApi_v2/Middlewares/` are byte-for-byte identical, verified by SHA256. Preserve one technique rather than separate versions.

The request middleware enables buffering, reads POST/PUT bodies with a leave-open reader and rewinds before downstream processing. The response middleware swaps in a memory stream, captures the response, copies it to the original stream, and restores that stream in finally. Both attach text to Application Insights request telemetry. Neither implementation bounds captured body length or response buffering.

The active API's `Services/RequestTimingMiddleware.cs` currently logs method, path, status and elapsed time. It does not preserve request/response bodies or offer a visible local diagnostics view.

## 2. Proposed runnable example

Add a bounded local diagnostics collector to the existing controller API and a separate diagnostics view in the catalog. Keep the timing technique, request rewind and guaranteed response-stream restoration. Replace the cloud telemetry destination with an in-memory local sink.

Each entry should show correlation ID, method, path, status, duration, request/response previews and truncation/omission indicators. Use a small fixed capacity, initially 20 entries, and bounded previews, initially 4 KiB each. Capture body previews only for selected fictional JSON demo endpoints; omit credentials/authorization headers, multipart uploads and binary streams. A body exceeding the preview limit must still reach the controller/client in full.

For responses, prefer forwarding bytes to the original stream while retaining a bounded preview, rather than buffering the entire response in memory. Restore the original response stream even when downstream processing throws. Keep failure propagation intact and make omission/truncation visible in diagnostics.

The diagnostics view should have meaningful actions: make a sample request, refresh entries and clear the local collector. Restart clears this collector; it is demonstration telemetry, not persistent audit storage. No new SQL table, EF regeneration, cloud account or API port is required.

## 3. Short implementation slices

1. Implement the collector and controller access with a small fictional JSON echo endpoint. Verify eviction, body limits and controller output independently of the UI.
2. Add request/response capture and stream-preservation checks, including downstream failure, large bodies and excluded binary content.
3. Add the separate visible diagnostics view and run guide; visually verify captured requests, response integrity and clear/refresh actions.

Keep each pass near five minutes and report a buildable checkpoint or any remaining issue. This document proposes the sequence; it does not claim any of these slices are implemented.

## 4. Following candidates

Local object/document storage is next: preserve upload/list/download/delete behind an S3-like interface, initially backed by files, with a controller and small browser view. Compare the archived blob and SharePoint/CRM library boundaries before selecting naming and metadata.

XML serialization follows: extract a fictional round trip showing element order, namespaces and nullable values from the integration-tool experiments. The historical desktop tool's database, WCF, email and SignalR integrations do not need to be ported to demonstrate that technique.
