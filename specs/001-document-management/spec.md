# Feature Specification: Document Upload and Management

**Feature Branch**: `main` (existing branch; no branch-creation hook configured)
**Created**: 2026-09-14
**Status**: Draft
**Input**: User description: `--file StakeholderDocs/document-upload-and-management-feature.md`
**Source**: [Stakeholder requirements](../../StakeholderDocs/document-upload-and-management-feature.md)

## Clarifications

### Session 2026-09-14

- Q: When an employee leaves a project, should they retain access to documents they uploaded to that project? → A: Revoke uploader-based access; require a separate current managerial or administrative permission.
- Q: If the system cannot durably record an audit event, should it allow the associated document operation to proceed? → A: Block audited operations unless their audit evidence can be durably saved; return a retryable error.
- Q: What is the maximum number of files a user may submit in one upload batch? → A: Maximum 10 files per batch.

### Session 2026-09-15

- Q: Should otherwise-supported Office documents be rejected when they contain embedded macros? → A: Reject embedded macros in all supported Office files; reject files when macro presence cannot be determined.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload and Retrieve Work Documents (Priority: P1)

An employee uploads one or more work documents, supplies descriptive metadata, and retrieves an accepted document without exposing it to unauthorized users.

**Why this priority**: Secure upload and retrieval deliver the core value of a central document collection.

**Independent Test**: Using two existing training users, upload allowed files as one user and verify exact downloads, metadata, and denial of access for the other user without requiring sharing or dashboard widgets.

**Acceptance Scenarios**:

1. **Given** a signed-in employee and several supported files within the limit, **When** the employee supplies each file's title and category and submits them, **Then** progress and a separate outcome appear for every file, and only files with a successful malware scan become available.
2. **Given** a batch containing an oversized file, an unsupported file, and a valid file, **When** it is uploaded, **Then** the invalid files receive specific errors without preventing the valid file from succeeding.
3. **Given** a detected threat, scanner failure, or interrupted upload, **When** processing ends, **Then** the file is not available through any document view, download, preview, or attachment and the employee receives a clear failure or retry message.
4. **Given** an accepted document, **When** its owner downloads it, **Then** its contents match the submitted file and the activity is recorded; an unauthorized user attempting the same direct access is denied without receiving metadata or content.
5. **Given** a user not assigned to a project, **When** that user attempts to upload into that project, **Then** the operation is denied unless the user manages that project as a Project Manager or has the explicit administrative permission.
6. **Given** a selection of exactly 10 files, **When** the user submits it, **Then** the batch is allowed subject to per-file validation; **Given** a selection of 11 or more files, **When** submission is attempted, **Then** no file in that batch is uploaded, the 10-file limit is explained, and the user can reduce the selection and retry without silent truncation.
7. **Given** an otherwise-supported Office file containing embedded macros or whose macro presence cannot be determined, **When** it is uploaded or used as a replacement, **Then** it is rejected with a specific explanation even if malware scanning reports it clean; a rejected replacement leaves the previous accepted file usable. Macro-free legacy `.doc`, `.xls`, and `.ppt` files remain eligible subject to all other checks.

---

### User Story 2 - Browse and Find Documents (Priority: P1)

An employee finds documents through My Documents, Project Documents, Shared with Me, or a search limited to accessible documents.

**Why this priority**: Documents must be discoverable for the feature to reduce time spent locating work information.

**Independent Test**: Seed accessible and inaccessible documents with distinct metadata, then verify each view, sort, filter, search field, and PDF/image preview without uploading new files.

**Acceptance Scenarios**:

1. **Given** an employee's uploaded documents that they are currently authorized to access, **When** My Documents opens, **Then** title, category, original upload date, size, and project are visible and sorting and category/project/date filtering return the expected subset.
2. **Given** a project member, **When** Project Documents opens, **Then** all accepted documents in that project are visible and downloadable; a nonmember sees only documents independently authorized by the access rules.
3. **Given** matching terms in titles, descriptions, tags, uploader names, and project names, **When** a user searches, **Then** matches across each field are returned within the specified performance target without exposing inaccessible documents or their counts.
4. **Given** an accessible PDF or image, **When** preview is requested, **Then** it opens within the preview target without a separate download action; other supported formats offer download without promising a preview.
5. **Given** no documents or no matching results, **When** a view or search opens, **Then** it shows an accurate empty state instead of an error.

---

### User Story 3 - Maintain and Share Documents (Priority: P2)

Owners and authorized managers keep documents current, remove obsolete files, and grant colleagues read access with in-app notifications.

**Why this priority**: Maintenance and controlled sharing prevent stale documents and uncontrolled distribution.

**Independent Test**: Use seeded documents, team relationships, and project assignments to test editing, replacement, permanent deletion, individual/team sharing, and the permission matrix.

**Acceptance Scenarios**:

1. **Given** an owner or authorized manager, **When** title, description, category, or tags are edited, **Then** the changes appear in authorized views and searches; a read-only recipient cannot make the same changes.
2. **Given** an existing document, **When** an authorized user replaces it, **Then** the same size, type, and malware checks apply; success preserves its identity, shares, and task links and exposes only the new file, while failure leaves the previous file usable.
3. **Given** deletion permission, **When** the user cancels confirmation, **Then** nothing changes; **When** the user confirms and deletion succeeds, **Then** the file and document entry are permanently removed, existing links stop granting access, and the audit record remains.
4. **Given** an owner sharing with an eligible user or team, **When** sharing succeeds, **Then** each newly entitled recipient has read access in Shared with Me and receives one notification per newly granted entitlement; repeating the same share does not duplicate it.
5. **Given** an owner revoking a share or a recipient losing a required membership, **When** the recipient next accesses the document, **Then** that grant no longer provides access, while any independent valid permission remains effective.
6. **Given** a Team Lead, Project Manager, and Administrator, **When** each attempts read and management actions inside and outside their scope, **Then** the permission matrix is enforced consistently, including direct document requests.
7. **Given** an uploader removed from a project with no separate current managerial or administrative permission, **When** they next browse, search, download, preview, edit, replace, delete, share, or follow a task/notification link to their uploads in that project, **Then** access is denied and those documents are excluded from My Documents, Recent Documents, and the dashboard count; original uploader attribution and documents remain intact for authorized users.
8. **Given** a former project member with a separate current managerial or administrative permission, **When** they access a document they uploaded to that project, **Then** only actions authorized by that separate permission are allowed; uploader identity grants no additional rights.

---

### User Story 4 - Work with Documents in Context (Priority: P2)

Employees attach documents to tasks and use project notifications and dashboard summaries to return to relevant work.

**Why this priority**: Existing task, project, and dashboard workflows provide the context absent from scattered file storage.

**Independent Test**: With seeded task and project documents, verify attachments, task-context upload, recent items, counts, and project notifications separately from general search.

**Acceptance Scenarios**:

1. **Given** access to a task and an eligible document, **When** the user attaches it, **Then** the task shows the link and the document belongs to that task's project; unauthorized or cross-project attachments are rejected.
2. **Given** a task in an assigned project, **When** the user uploads from its detail view, **Then** successful upload associates the document with the task's project and task; failed upload creates neither a document entry nor an attachment.
3. **Given** an employee with at least six accepted uploads they are currently authorized to access, **When** the dashboard opens, **Then** Recent Documents shows the five latest surviving accessible uploads by that employee and the summary count matches their authorized My Documents collection.
4. **Given** a newly accepted project document, **When** it becomes available, **Then** current project members other than the uploader receive an in-app notification; failed uploads, edits, and replacements do not generate a new-document notification.

---

### User Story 5 - Review Document Activity (Priority: P3)

An administrator reviews document activity to understand document use and investigate access concerns.

**Why this priority**: Audit evidence and reports support accountable use; recording activity itself is required from the first release of upload and access.

**Independent Test**: Generate known uploads, downloads, shares, and deletions and compare administrator reports to the recorded events, including events for subsequently deleted documents.

**Acceptance Scenarios**:

1. **Given** document activity and available audit persistence, **When** an operation succeeds or fails, **Then** its actor, timestamp, action, document reference, and outcome are recorded; sharing records identify the intended recipients. When audit persistence itself fails, the operation is blocked as specified in scenario 4, without claiming that an audit record was saved.
2. **Given** known activity in a selected date range, **When** an administrator generates reports, **Then** upload totals by type, uploader rankings, and access activity by user/document/date match those events.
3. **Given** a non-administrator, **When** they attempt to open audit reports or access their contents directly, **Then** access is denied.
4. **Given** audit evidence cannot be durably saved, **When** a user attempts an upload, replacement, download, preview, deletion, share, or share revocation, **Then** the operation is blocked with a retryable error before releasing file content or committing the requested document or permission change; no success is reported.
5. **Given** audit recording has recovered after a blocked attempt, **When** the user retries, **Then** the operation may proceed with durable audit evidence and its actual outcome is distinguished from the earlier blocked or incomplete attempt; no blocked attempt is reported as successful.

### Edge Cases

- A file exactly at the size limit is accepted if otherwise valid; one byte over is rejected. Empty files are rejected with a specific explanation.
- A batch may contain at most 10 files. A selection exceeding that limit is rejected as a whole before upload begins, not silently truncated; the user can reduce the selection and retry. This per-batch limit does not impose a total document-storage quota.
- Extension/content disagreement, a disguised executable, and unreadable or encrypted content that cannot be scanned are rejected rather than marked clean.
- Embedded macros in any supported Office file cause rejection even after a clean malware scan. If macro inspection cannot determine whether macros are present, the file is rejected rather than treated as macro-free. This applies to initial uploads and replacements, including legacy Office formats.
- Duplicate titles and original filenames are allowed as distinct documents; one upload never overwrites another implicitly.
- A connection interruption or unavailable scanner must not expose an unfinished document or report a false success. Retrying a failed file must not duplicate already successful files in the batch.
- Failed storage or metadata persistence leaves no visible broken document and triggers cleanup of incomplete data; failed cleanup is recorded for administrator attention.
- A failed replacement preserves the previous accepted file; a failed deletion is not reported as successful and can be retried without restoring a completed deletion.
- Audit persistence failure blocks audited operations before content release or committed changes, including share revocation. The existing document and permissions remain unchanged, and the caller receives a retryable error rather than a false success. Temporary upload content remains unavailable and is cleaned up. An incomplete attempt MUST NOT be counted as a successful operation merely because an attempt record exists.
- Membership and permissions are checked at the time of each action, including after a view has loaded. Revocation cannot recall files already downloaded outside the application.
- Leaving a project revokes all uploader-based rights to its documents, including sharing management. Only a separate current managerial or administrative permission can authorize subsequent access while the uploader remains a nonmember; original uploader attribution is retained.
- A deleted document disappears from shares, attachments, search, counts, and recent lists, but historical audit evidence remains.
- Inactive recipients, empty teams, stale notification links, and invalid date ranges receive clear outcomes without disclosing inaccessible document details.
- Conflicting edits, replacements, or deletions do not silently overwrite a newer change; the later conflicting attempt requires refresh and retry.
- Project reassignment is not an ordinary metadata edit. Attaching a personal document to a project must warn its owner that project members gain access and require confirmation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The feature MUST be available to signed-in dashboard users through existing navigation and enforce the access matrix for every document action, view, search, preview, notification destination, and report.
- **FR-002**: Users MUST be able to select 1 to 10 files per upload batch, with per-file progress, validation outcomes, success/failure messages, and retry of failed files without resubmitting successful files. A selection exceeding 10 files MUST be rejected as a whole before upload begins, with a clear limit message and an opportunity to reduce the selection and retry; the system MUST NOT silently truncate the batch. This limit MUST also be enforced on submitted batches, not only by the file picker.
- **FR-003**: Each file MUST be nonempty and at most 25 MB (25,000,000 bytes). Allowed extensions are `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.txt`, `.jpg`, `.jpeg`, and `.png`; file contents MUST agree with the claimed type. Macro-enabled Office formats and archives are excluded. All otherwise-supported Office files MUST also be inspected for embedded macros on initial upload and replacement. Files containing macros or whose macro presence cannot be determined MUST be rejected with a specific explanation, even if malware scanning reports them clean. Macro-free legacy `.doc`, `.xls`, and `.ppt` files remain supported subject to all other validation.
- **FR-004**: Upload MUST require a nonblank title and one category per file: Project Documents, Team Resources, Personal Files, Reports, Presentations, or Other. Description, an authorized associated project, and custom tags MUST be optional. Selecting a category alone MUST NOT grant access.
- **FR-005**: The system MUST capture uploader identity/name, original upload timestamp, file size, file type, and original filename. File-type labels up to 255 characters MUST be retained without truncation.
- **FR-006**: Every initial upload and replacement MUST receive an actual malware scan before acceptance into the document collection. Threat detection, indeterminate results, and scanner unavailability MUST block availability; a simulated clean result does not satisfy acceptance.
- **FR-007**: Unaccepted content MUST never be served to users. Failed processing MUST not leave a visible document entry, inaccessible attachment, or orphaned content without recorded cleanup handling. Successful uploads MUST not overwrite unrelated content.
- **FR-008**: My Documents MUST show all surviving documents uploaded by the current user that they are currently authorized to access, including title, category, upload date, size, and project. It MUST support ascending/descending title, date, category, and size sorting and combined category, project, and inclusive upload-date-range filters.
- **FR-009**: Project Documents MUST list all accepted documents associated with that project for its authorized members and managers. Shared with Me MUST list documents granted through an individual or current team share, without duplicate rows.
- **FR-010**: Search MUST match title, description, tags, uploader name, and project name, be case-insensitive, and return only authorized documents. Search and filters MUST combine consistently; no body-content search is required.
- **FR-011**: Authorized readers MUST be able to download accepted files and preview PDF, JPEG, and PNG documents in-browser. Unsupported preview formats MUST retain a working download option.
- **FR-012**: Authorized managers MUST be able to edit title, description, category, and tags and replace the current file. Replacement MUST preserve original uploader/date, document identity, shares, and attachments, update current file attributes, and remove the superseded file after successful replacement without exposing version history.
- **FR-013**: Authorized managers MUST explicitly confirm permanent deletion. Successful deletion MUST remove content and active document metadata, shares, and task links, and invalidate access through old links while retaining audit evidence. No recovery view is provided.
- **FR-014**: Owners and Administrators MUST be able to grant or revoke read access for active users or teams. Team shares MUST follow current membership. Project-document sharing MUST NOT grant nonmembers access; a team share MUST be limited to recipients who are also project members. Invalid recipients MUST be reported before confirmation.
- **FR-015**: New individual/team share entitlements MUST generate in-app notifications and appear in Shared with Me. Repeated identical shares MUST be idempotent. Revoking one grant MUST not revoke independent project, ownership, management, or other sharing permissions.
- **FR-016**: Users authorized to update a task MUST be able to attach accessible documents from its project or upload directly from its detail view. Only owners or Administrators may move a personal document into that project through attachment, with explicit confirmation of expanded visibility. Cross-project attachments MUST be rejected; attaching MUST not bypass document permissions.
- **FR-017**: Dashboard Recent Documents MUST show up to five surviving accepted documents uploaded by the current user that they are currently authorized to access, newest original upload first. The document summary MUST count only that user's surviving accessible uploads, not uploads they can no longer access, other users' files, or historical files.
- **FR-018**: Each newly accepted project document MUST notify current project members other than the uploader. Replacement, editing, failed processing, and retrying notification delivery MUST not produce duplicate new-document notifications. Notification failure MUST not discard a successfully accepted document and MUST be recorded for retry.
- **FR-019**: Uploads (including file replacements), downloads, deletions, and sharing/grant-revocation attempts MUST be auditable with actor, timestamp, action, document reference, and outcome. Successful events MUST be distinguishable from failures and incomplete attempts. Preview access MUST also be recorded as access activity without counting it as a download. Operations blocked by audit persistence failure MUST return the error defined in FR-025; the system MUST NOT claim a durable audit record exists when persistence itself failed.
- **FR-020**: Only Administrators MUST be able to generate date-filtered reports for uploads by document type, active uploaders by successful upload count, and access patterns by document, user, date, and download/preview action. Reports MUST retain evidence for deleted documents and MUST not count failed attempts as successes.
- **FR-021**: Missing, expired, or insufficient permissions MUST deny access without leaking document content or protected metadata. Team Leads MUST be limited to their current team's uploaders, Project Managers to projects they manage, and Administrators MUST have the explicit global access described below.
- **FR-022**: Core workflows MUST operate without internet or cloud services after documented setup, use existing training sign-in, preserve existing dashboard behavior, and remain clearly identified as training functionality. The feature MUST not require a production identity or external document service.
- **FR-023**: Views and upload feedback MUST be usable with keyboard navigation and labeled controls and remain readable without overlapping content on desktop and mobile browsers. The upload flow MUST meet the action-count definition in SC-005.
- **FR-024**: Removing an uploader from a project MUST revoke all uploader-based rights to documents in that project at the next authorization check. While they remain a nonmember, only a separate current managerial or administrative permission MAY authorize an action, within that permission's scope. Upload attribution MUST remain unchanged and MUST NOT itself authorize access. Personal documents without a project association are unaffected.
- **FR-025**: An operation requiring audit under FR-019 MUST NOT release file content or commit document or sharing changes unless its audit evidence can be durably saved. Audit persistence failure MUST block the operation with a retryable error, preserve existing document content and permissions, and prevent a success response. Audit evidence MUST distinguish the attempt from its actual outcome; an incomplete operation MUST NOT be reported as successful. Best-effort logging or a fallback queue MUST NOT bypass this requirement.

### Access Rules

Permissions are cumulative, evaluated against current memberships and assignments. Uploader-based permissions for a project document require current project membership; while the uploader is a nonmember, only a separate current managerial or administrative permission can authorize actions within its scope. All references to owners and own documents below are subject to this condition. "Manage" means edit metadata, replace, and permanently delete; it does not automatically mean permission to reshare.

| Actor or relationship | Read/download/preview | Manage | Grant/revoke sharing |
|-----------------------|-----------------------|--------|----------------------|
| Document uploader | Own personal documents; own project documents while a current member | Same ownership scope | Same ownership scope, subject to project-sharing eligibility |
| Project member | Documents in that project | Only through another applicable permission | Only through another applicable permission |
| Individual or team-share recipient | Granted documents while eligible | No | No |
| Team Lead | Documents uploaded by current members of their team | Same team scope, including personal-category documents | Only own documents |
| Project Manager | Documents belonging to projects they manage | Same project scope | Only own documents |
| Administrator | All documents, explicit membership bypass | All documents | All documents, subject to project-sharing eligibility |

All users may upload personal documents and upload into projects they belong to; Project Managers may upload into projects they manage and Administrators into any project. Private means restricted to this matrix, not hidden from authorized managers. Task links never grant additional read rights.

### Key Entities *(include if feature involves data)*

- **Document**: One current accepted work file with title, description, predefined category, tags, optional project, uploader, original upload time, original filename, size, type, and availability state.
- **Document Category**: One of the six named classifications; independent of authorization.
- **Document Share**: A document's read grant to an individual or team, with grantor and grant/revocation time; distinct from project and managerial access.
- **Task Document Association**: A relationship between a task and an accepted document belonging to the task's project.
- **User, Team, and Project Membership**: Existing identities, roles, team relationships, and project assignments that determine current access.
- **Upload Attempt**: Per-file progress, validation and scan outcome, and completion/failure status; distinct from an accepted document.
- **Document Activity**: Historical actor, document reference, action, timestamp, outcome, and sharing recipients supporting administrator reports after document deletion.
- **Document Notification**: A sharing or project-addition notice for a recipient, whose destination still requires current authorization.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 95% of valid single-file uploads up to 25 MB become available within 30 seconds from submitting a fully specified upload through completed malware scanning, under the agreed reference conditions below. For multi-file batches, per-file transfer/processing time and total batch time are measured separately; this target does not promise an entire batch in 30 seconds.
- **SC-002**: At least 95% of document list requests finish showing usable results within 2 seconds for collections of up to 500 authorized documents.
- **SC-003**: At least 95% of document searches finish showing authorized results within 2 seconds for the reference collection, with zero unauthorized matches or leaked counts across the access-matrix tests.
- **SC-004**: At least 95% of supported PDF/image preview requests display usable content within 3 seconds, including files at the upload-size limit.
- **SC-005**: Users can submit an upload with no more than three in-application button activations: open upload, choose files, and submit. Typing metadata, choosing field values, and operating the system file picker are excluded; no extra mandatory navigation or confirmation is required for an ordinary upload.
- **SC-006**: Within three months of training rollout, at least 70% of active participating dashboard users have successfully uploaded at least one document. Active means at least one dashboard session during that period; seed accounts and automated test users are excluded.
- **SC-007**: In a representative find-document exercise, participants locate and open an authorized target document in an average of less than 30 seconds, and at least 90% complete the exercise without assistance.
- **SC-008**: Within three months, at least 90% of a reviewed sample of at least 100 uploaded documents (or all documents if fewer exist) have a category consistent with the agreed category descriptions; mere presence of a required value does not count as correct categorization.
- **SC-009**: All authorization, malware-blocking, and Office macro-rejection acceptance tests pass before rollout, including rejection when macro presence cannot be determined and when macros are present despite a clean malware scan. No confirmed unauthorized document disclosure or modification incidents are observed during the first three months. Incident-free observation is a release metric, not a guarantee of absolute security.
- **SC-010**: All tested uploads (including replacements), downloads, previews, deletions, and sharing actions that proceed are represented accurately in administrator activity reports; deleting documents does not erase these records, and non-administrators cannot obtain reports. In injected audit-persistence failure tests, 100% of audited operations are blocked before content release or committed changes, return a retryable error, and report no false success. Blocked attempts whose evidence could not be saved are not claimed to appear in reports.
- **SC-011**: All primary upload, browse, search, download, preview, sharing, task, and reporting acceptance scenarios can be demonstrated without an internet connection after initial setup.

## Assumptions and Dependencies

- The supplied file is the authoritative feature brief for this invocation. Its offline training requirements replace the earlier cloud-only chat summary; the earlier 5,000-employee figure is not treated as a simultaneous-user or production-deployment commitment.
- Constitution 1.0.0 governs this feature. The source phrase "production-ready within 8-10 weeks" conflicts with its training-only rule: the delivery target is interpreted as a verified training release in 8-10 weeks, not production certification. Enterprise deployment requires a separate approved amendment and scope. The timeline is a planning constraint, not evidence of feasibility.
- Implementation notes remain in the linked source for planning review, not as solution design in this specification. Planning must reconcile its storage and authentication guidance with the constitution, preserve applicable metadata constraints, and review its outdated database-reset advice. No reset, migration, cloud integration, or source-document edit is authorized by this specification.
- A real offline-capable malware scanner with documented installation, supported file formats, and definition-update procedures is a delivery dependency. Core operation cannot require an online scan; scanner absence or inability to inspect content blocks upload. Scanner feasibility must be demonstrated before promising the performance targets; there is no silent mock-scanner substitution.
- Offline inspection MUST determine macro presence across all supported Office formats, including legacy files. Planning MUST verify this capability alongside malware scanning; a clean malware result alone does not establish that a file is macro-free. Unavailable or inconclusive macro inspection blocks acceptance, and its processing time is included in the upload target.
- Team means the existing departmental team grouping; Team Lead scope is their current department. Membership is evaluated dynamically. Broad Team Lead access to team-uploaded documents and global Administrator access are taken from the source role definitions and must be visible to users selecting Personal Files.
- Sharing grants read-only access and can be revoked. Restricting project shares to project members, allowing only owners/Administrators to share, and requiring confirmation before personal-to-project association are conservative access defaults. Moving a document between projects is excluded from this release.
- Search uses a case-insensitive substring match within any specified metadata field for the entered search text, then intersects selected filters. Date filters apply to original upload dates in the user's displayed local date. Default list order is newest upload first, with a stable document identity tie-breaker.
- Reference performance conditions are a supported desktop browser, one actively uploading user, at least 20 Mbps effective transfer capacity, round-trip latency at most 100 ms, 500 accessible documents plus 500 inaccessible documents, and a ready scanner with current definitions. At least 20 trials per operation include initial and repeated access. Batch timing is per file from transfer start; queue time and total batch duration are reported separately. No high-concurrency service-level commitment is inferred.
- Definitions of the six categories are agreed before categorization measurement. Activity needed for reports is retained throughout the training exercise and at least the three-month measurement period; archival policy and regulated retention are not part of this release. Explicit training-data reset procedures may clear local data outside normal document deletion.
- Automated verification requires fictional users across all four roles, distinct teams and projects, safe scanner test fixtures, and both positive and negative permission cases. Real employee documents or malicious payloads are not needed for acceptance testing.

## Out of Scope

- Collaborative editing; version history, rollback, or access to superseded files.
- Approval/routing workflows; document templates or generation.
- External document-system integrations or cloud deployment; production identity migration.
- Native mobile applications; the web experience remains responsive.
- Storage quotas or quota management; trash, soft deletion, and recovery.
- Document body-content search, previews other than PDF/JPEG/PNG, bulk cross-project moves, and performance guarantees for thousands of concurrent users.