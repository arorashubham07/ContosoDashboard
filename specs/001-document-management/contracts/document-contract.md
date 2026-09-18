# Document UI and Streaming Contract

## Authentication and Authorization

All document UI routes require the existing cookie-authenticated user. Every action passes the authenticated caller ID to `IDocumentService`; route, form, query, and hidden field identifiers are inputs only, never authority. A missing, inaccessible, or stale document result is returned as a generic not-found/forbidden experience without protected metadata or bytes.

## UI Routes

| Route or surface | Required behavior |
|---|---|
| `/documents` | My Documents, Shared with Me, accessible search results, filters, sorting, upload form, and permitted management actions. |
| `/projects/{projectId:int}` | Authorized project documents and upload entry point for that project. |
| Existing task detail surface | Accessible project-document attachments and task-context upload. |
| `/document-reports` | Administrator-only date-filtered activity reports. |
| Dashboard and navigation | Document navigation, count, and up to five currently authorized recent uploads. |

The upload form permits 1-10 file selections, requires title/category per file, exposes progress and a separate result per file, prevents a submitted batch over 10, and permits retrying only failed files. After successful upload, reset `InputFile` with `@key` and clear the browser-file reference after copying its stream into memory.

## Private File Routes

Protected endpoints are implemented in the existing application pipeline and are never mapped to the filesystem path.

| Method and route | Success | Failure |
|---|---|---|
| `GET /documents/{documentId:int}/download` | `200` attachment response with original safe display filename and accepted bytes. | Generic `404`/`403`; audit persistence failure returns retryable `503` before headers/body. |
| `GET /documents/{documentId:int}/preview` | `200` inline response only for PDF, JPEG, or PNG with safe content type. | `400` for unsupported preview format; generic `404`/`403`; audit failure is retryable `503` before content. |

Both routes must: authenticate, re-evaluate current document read authorization, verify `Accepted` state, resolve only a server-generated path under the configured storage root, durably record the activity, then stream bytes. Download and preview actions are distinguished in activity reports.

## Service Requests and Results

`IDocumentService` owns request validation, authorization, scanning/inspection coordination, persistence, recovery, notifications, and audit rules. Its request DTOs contain caller ID, document/project/task ID, metadata, selected-file stream metadata, and optimistic concurrency token as applicable. Its result DTOs expose only authorized metadata and one of: success, validation failure, forbidden/not-found, conflict/reload required, retryable operational failure, or notification queued for retry. No result exposes physical paths, scanner command output, or private recipient details to unauthorized callers.

## Error and Idempotency Rules

- Validation errors identify the affected file without accepting any invalid content.
- A batch over 10 is rejected before any file begins.
- Identical active share grants and notification deliveries are idempotent.
- Replacement/deletion/edit conflicts require refresh; they never overwrite a newer change.
- Audit-persistence failure blocks upload, replacement, download, preview, deletion, sharing, and revocation before bytes are exposed or change is committed.
- Notification delivery failure after accepted project upload is recorded for retry and does not roll back the accepted document.