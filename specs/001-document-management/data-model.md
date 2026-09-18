# Data Model: Document Upload and Management

## Document

| Field | Type and rules |
|---|---|
| `DocumentId` | Integer primary key. |
| `Title` | Required, trimmed, nonblank, maximum 255 characters. |
| `Description` | Optional, maximum 2,000 characters. |
| `Category` | Required text value: Project Documents, Team Resources, Personal Files, Reports, Presentations, or Other. |
| `Tags` | Optional normalized text or a related tag table; match search case-insensitively. |
| `ProjectId` | Optional FK to `Project`; association requires current upload authority. |
| `UploaderUserId` | Required FK to `User`; attribution is immutable and does not bypass current membership. |
| `OriginalFileName` | Required display-only filename, never used for storage paths. |
| `FilePath` | Required server-generated, relative private path, unique and never rooted/user-supplied. |
| `FileSizeBytes` | Required, greater than zero and at most 25,000,000. |
| `ContentType` | Required, maximum 255 characters; must agree with inspected content. |
| `UploadedAtUtc` | Required original acceptance timestamp; immutable across replacement. |
| `AvailabilityState` | Pending processing, Accepted, or cleanup/recovery state; only Accepted is visible. |
| `ConcurrencyToken` | Required optimistic-concurrency value for edit/replace/delete conflict detection. |

Indexes: unique `FilePath`; `(ProjectId, AvailabilityState, UploadedAtUtc)`; `(UploaderUserId, AvailabilityState, UploadedAtUtc)`; and searchable normalized metadata fields as supported by SQLite.

## Document Share

| Field | Type and rules |
|---|---|
| `DocumentShareId` | Integer primary key. |
| `DocumentId` | Required FK to an active accepted document. |
| `RecipientUserId` / `RecipientDepartment` | Exactly one target. Team targets use the existing department grouping. |
| `GrantedByUserId` | Required FK to the granting owner or Administrator. |
| `GrantedAtUtc` / `RevokedAtUtc` | Grant and optional revocation timestamps. |

Active grants are unique per document and recipient. A project-document grant is effective only while the recipient is a current project member. Shares grant read-only access and never supersede current authorization checks.

## Task Document

| Field | Type and rules |
|---|---|
| `TaskDocumentId` | Integer primary key. |
| `TaskId` | Required FK to `TaskItem`. |
| `DocumentId` | Required FK to accepted `Document`. |
| `AttachedByUserId` / `AttachedAtUtc` | Required auditability fields. |

Unique `(TaskId, DocumentId)`. The document project must equal the task project; a task link never grants document read access.

## Document Activity

| Field | Type and rules |
|---|---|
| `DocumentActivityId` | Integer primary key. |
| `DocumentId` | Required historical integer reference; retained after document deletion without a cascading FK delete. |
| `ActorUserId` | Required FK to `User`. |
| `Action` | Upload, Replace, Download, Preview, Delete, ShareGrant, or ShareRevoke. |
| `Outcome` | Succeeded, Failed, or Blocked; reports exclude non-successes from success counts. |
| `OccurredAtUtc` | Required event time. |
| `RecipientSummary` | Optional, non-sensitive recipient evidence for share actions. |
| `Details` | Optional reason/error data without file content or protected metadata leakage. |

This entity is append-only. Required successful-operation activities are inserted in the same SQLite transaction as the matching metadata/permission mutation; download/preview activity is committed before releasing bytes.

## Document Recovery Record

| Field | Type and rules |
|---|---|
| `DocumentRecoveryRecordId` | Integer primary key. |
| `OperationId` | Server-generated GUID, unique. |
| `OperationKind` | Upload, Replace, or Delete. |
| `StagingPath` / `FinalPath` | Server-generated relative paths only. |
| `DocumentId` | Optional until metadata exists. |
| `State` | IntentRecorded, FileMoved, MetadataCommitted, CleanupRequired, or Resolved. |
| `CreatedAtUtc` / `ResolvedAtUtc` | Recovery lifecycle timestamps. |
| `FailureDetail` | Operational error details for administrator attention. |

On startup and before affected operations, reconcile non-resolved records. Recovery artifacts are never served.

## Upload Attempt and Notification

An upload attempt is transient per-file UI state: selected metadata, progress, validation/scan result, retryable failure, and final document ID. It is not visible as a `Document` before acceptance. Reuse the existing `Notification` entity with new document-related notification types and a document destination reference that the recipient re-authorizes when opened. Add a uniqueness/idempotency key for share and project-addition notifications.

## Relationships and State Transitions

`User` uploads `Document`; `Document` optionally belongs to `Project`; `Document` has zero or more shares, task links, and activities. `TaskItem` has zero or more document links. Deleting a document removes active shares and task links but retains activities.

Document processing transitions: `Selected -> Validating -> Staged -> Inspected -> Scanned -> Accepted` or `Selected/Validating/Staged/Inspected/Scanned -> Rejected/CleanupRequired -> Removed`. Replacement retains document identity, original uploader/date, shares, and links; it swaps the accepted file only after the new file is accepted and audited. Deletion transitions `Accepted -> DeletionPending -> Removed`, with recovery required on failures.