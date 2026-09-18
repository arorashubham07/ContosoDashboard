# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-management/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [document contract](contracts/document-contract.md), and [quickstart.md](quickstart.md)

**Tests**: Automated tests are mandatory because the specification requires repeatable verification of service rules, authorization, persistence, and stream contracts. Write test tasks before their corresponding implementation and confirm they initially fail.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare test infrastructure, configuration, fixtures, and offline prerequisites.

- [X] T001 Create xUnit test project and reference the web project in `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`
- [X] T002 [P] Add temporary file-backed SQLite and private-storage test-host helpers in `ContosoDashboard.Tests/DocumentTestHost.cs`
- [X] T003 [P] Add fictional clean, macro-bearing, malformed, and encrypted Office fixture documentation in `ContosoDashboard.Tests/Fixtures/Office/README.md`
- [X] T004 [P] Document Microsoft Defender initial setup, disconnected execution, and Windows ARM64 readiness checks in `specs/001-document-management/quickstart.md`
- [X] T005 Add private storage root and Defender scanner settings without secrets in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish persistence, authorization, storage, inspection, scanning, audit, recovery, and stream infrastructure shared by all stories.

**Critical**: Complete this phase before user-story work. It enforces offline operation and audit-before-action safety.

- [X] T006 Add Document, DocumentShare, TaskDocument, DocumentActivity, and DocumentRecoveryRecord entities using all constraints in `specs/001-document-management/data-model.md` in `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentShare.cs`, `ContosoDashboard/Models/TaskDocument.cs`, `ContosoDashboard/Models/DocumentActivity.cs`, and `ContosoDashboard/Models/DocumentRecoveryRecord.cs`
- [X] T007 Extend document/task/user relationships and document notification types/destinations in `ContosoDashboard/Models/User.cs`, `ContosoDashboard/Models/TaskItem.cs`, and `ContosoDashboard/Models/Notification.cs`
- [X] T008 Configure DbSets, relationships, retained audit history, active-share/task-link/path indexes, concurrency tokens, and recovery records in `ContosoDashboard/Data/ApplicationDbContext.cs`
- [X] T009 Create the EF Core migration and document existing-data upgrade/reset effects in `ContosoDashboard/Migrations/` and `specs/001-document-management/quickstart.md`
- [X] T010 [P] Define document requests/results and injected clock, storage, scanner, macro, audit, recovery, and document-service interfaces in `ContosoDashboard/Services/DocumentContracts.cs`
- [X] T011 [P] Implement current role, department, project membership, ownership, manager, Administrator, share-recipient, and former-member authorization in `ContosoDashboard/Services/DocumentAuthorizationService.cs`
- [X] T012 [P] Implement private-root validation, GUID staging/final paths, atomic moves, and deletion in `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T013 [P] Implement fail-closed allowed-content/type and OOXML/legacy Office macro inspection in `ContosoDashboard/Services/OfficeMacroInspector.cs`
- [X] T014 [P] Implement Windows Defender `MpCmdRun.exe` custom-file scanning with timeout and clean/threat/indeterminate results in `ContosoDashboard/Services/WindowsDefenderMalwareScanner.cs`
- [X] T015 Implement durable activity creation and transaction-aware audit persistence in `ContosoDashboard/Services/DocumentAuditService.cs`
- [X] T016 Implement startup and pre-operation storage reconciliation from durable recovery records in `ContosoDashboard/Services/DocumentRecoveryService.cs`
- [X] T017 Register document services, safely run recovery, and map authenticated private download/preview endpoint shells in `ContosoDashboard/Program.cs`
- [ ] T018 [P] Add entity configuration and migration integration tests in `ContosoDashboard.Tests/DocumentPersistenceTests.cs`
- [ ] T019 [P] Add complete access-matrix and former-uploader authorization tests in `ContosoDashboard.Tests/DocumentAuthorizationTests.cs`
- [ ] T020 [P] Add private storage path traversal, GUID, atomic-move, and cleanup tests in `ContosoDashboard.Tests/DocumentStorageRecoveryTests.cs`
- [ ] T021 [P] Add OOXML/legacy macro fixture and content-type inspection tests in `ContosoDashboard.Tests/OfficeMacroInspectorTests.cs`
- [ ] T022 [P] Add scanner result-mapping tests and conditional Windows ARM64 Defender readiness suite in `ContosoDashboard.Tests/WindowsDefenderMalwareScannerTests.cs`

**Checkpoint**: No file can be publicly served before authorization, scan/inspection, durable audit, and accepted metadata are established.

---

## Phase 3: User Story 1 - Upload and Retrieve Work Documents (Priority: P1) MVP

**Goal**: Let authenticated users upload one to ten validated work files and retrieve only currently authorized accepted files.

**Independent Test**: Upload a clean file as one training user, compare downloaded bytes and metadata, and verify a second unauthorized user receives no metadata or content. Verify partial batch failures and unsafe inputs without sharing or dashboard dependencies.

- [ ] T023 [P] [US1] Write upload tests for 1-10 files, whole-batch 11-file rejection, partial results, failed-file retry, title/category validation, zero-byte, 25,000,000-byte, and 25,000,001-byte files in `ContosoDashboard.Tests/DocumentServiceTests.cs`
- [ ] T024 [P] [US1] Write tests for unsupported extensions, type disagreement, disguised executables, scanner/macro unsafe or indeterminate results, and unauthorized project uploads in `ContosoDashboard.Tests/DocumentUploadValidationTests.cs`
- [ ] T025 [P] [US1] Write download contract tests for exact accepted bytes, safe filename, audit-before-content, generic unauthorized denial, and unavailable/rejected content in `ContosoDashboard.Tests/DocumentStreamContractTests.cs`
- [ ] T026 [P] [US1] Write stage, inspect, scan, journal, move, metadata/audit persistence, cleanup, and restart recovery fault tests in `ContosoDashboard.Tests/DocumentUploadRecoveryTests.cs`
- [ ] T027 [US1] Implement per-file upload validation in `ContosoDashboard/Services/DocumentService.cs`, enforcing the 1-10 limit, nonblank title, six text categories, 25,000,000-byte limit, allowed types, current upload authority, and retry-safe results
- [ ] T028 [US1] Implement authorize-stage-inspect-scan-journal-move-transactional-metadata-and-audit workflow with accepted-only visibility and cleanup in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T029 [US1] Implement authorized download preparation and durable download activity before returning an accepted private stream in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T030 [US1] Complete authenticated download endpoint response and retryable audit-failure mapping in `ContosoDashboard/Program.cs`
- [ ] T031 [US1] Build `/documents` upload controls with labels, per-file title/category metadata, progress, outcomes, failed-file retry, keyboard support, three-action flow, and InputFile reset in `ContosoDashboard/Pages/Documents.razor`
- [ ] T032 [US1] Add Documents navigation and responsive upload styles in `ContosoDashboard/Shared/NavMenu.razor` and `ContosoDashboard/wwwroot/css/site.css`

**Checkpoint**: Secure upload and retrieval are independently demonstrable, including all validation, scan, macro, authorization, audit, and recovery failures.

---

## Phase 4: User Story 2 - Browse and Find Documents (Priority: P1)

**Goal**: Let users find only documents currently available to them through personal, project, shared, filter, search, and preview workflows.

**Independent Test**: Seed accessible and inaccessible accepted documents; verify all list types, sorting, filters, metadata search, empty state, and PDF/image preview without uploading.

- [ ] T033 [P] [US2] Write My Documents, Project Documents, Shared with Me, sorting, combined inclusive date/category/project filters, stable default order, and empty-state tests in `ContosoDashboard.Tests/DocumentBrowseTests.cs`
- [ ] T034 [P] [US2] Write case-insensitive title, description, tags, uploader, and project search tests with former-member exclusion and zero leaked result counts in `ContosoDashboard.Tests/DocumentSearchTests.cs`
- [ ] T035 [P] [US2] Write preview contract tests for PDF/JPEG/PNG inline content, download-only unsupported types, audit-before-content, and direct authorization denial in `ContosoDashboard.Tests/DocumentPreviewContractTests.cs`
- [ ] T036 [US2] Implement accepted-document lists, filters, sorts, project/shared queries, and authorized metadata search in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T037 [US2] Implement authorized PDF/JPEG/PNG preview preparation and durable preview activity before streaming in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T038 [US2] Complete authenticated preview endpoint behavior in `ContosoDashboard/Program.cs`
- [ ] T039 [US2] Add My Documents, Shared with Me, search, sort/filter table, preview/download actions, and empty states in `ContosoDashboard/Pages/Documents.razor`
- [ ] T040 [US2] Add authorized project-document list with preview/download actions in `ContosoDashboard/Pages/ProjectDetails.razor`

**Checkpoint**: Browsing, search, and preview expose no inaccessible metadata, row, count, or file content.

---

## Phase 5: User Story 3 - Maintain and Share Documents (Priority: P2)

**Goal**: Let authorized owners/managers update, replace, permanently delete, and share documents while preserving current authorization and audit guarantees.

**Independent Test**: Seed documents, users, departments, and memberships; verify updates, conflict behavior, replacement preservation, delete confirmation, share idempotence/revocation, notifications, and all role boundaries.

- [ ] T041 [P] [US3] Write metadata edit, concurrency conflict, scoped manager, and read-only recipient denial tests in `ContosoDashboard.Tests/DocumentManagementTests.cs`
- [ ] T042 [P] [US3] Write replacement/deletion fault and audit tests for preservation on failure, retained audit, active share/link/content removal, and audit-failure blocking in `ContosoDashboard.Tests/DocumentLifecycleTests.cs`
- [ ] T043 [P] [US3] Write individual/team eligibility, project-member restriction, active-user validation, idempotence, revocation, notification, and permission-matrix tests in `ContosoDashboard.Tests/DocumentShareTests.cs`
- [ ] T044 [US3] Implement authorized metadata edits with concurrency checks and durable audit evidence in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T045 [US3] Implement validated replacement using private staging, macro/scanner/recovery checks, accepted swap, old-file cleanup, and audit-before-commit in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T046 [US3] Implement confirmation-gated permanent deletion with recovery, share/task-link removal, link invalidation, and retained audit history in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T047 [US3] Implement individual/department share and revocation eligibility, idempotency, independent permission preservation, audit, and retry-recorded notification failures in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T048 [US3] Add document notification types, idempotency persistence, and safe notification destination resolution in `ContosoDashboard/Services/NotificationService.cs` and `ContosoDashboard/Models/Notification.cs`
- [ ] T049 [US3] Add accessible edit, replace, share/revoke, conflict/retry, and permanent-delete confirmation controls in `ContosoDashboard/Pages/Documents.razor`

**Checkpoint**: Management maintains document integrity and current access rules while no unaudited operation is reported as successful.

---

## Phase 6: User Story 4 - Work with Documents in Context (Priority: P2)

**Goal**: Connect authorized documents to tasks, project activity, navigation, and dashboard context without widening access.

**Independent Test**: Seed tasks and documents; verify legal attachment/task upload, reject cross-project attachment, validate recent/count, and validate project notifications independently.

- [ ] T050 [P] [US4] Write task attachment and task-context upload tests for task-update authority, project match, personal-to-project confirmation, document authorization, and failed-upload cleanup in `ContosoDashboard.Tests/TaskDocumentTests.cs`
- [ ] T051 [P] [US4] Write dashboard recent/count and project-notification idempotence tests including former-uploader exclusion and retry behavior in `ContosoDashboard.Tests/DocumentDashboardTests.cs`
- [ ] T052 [US4] Extend task services for authorized task-document attachment, task-context upload association, and cross-project/personal visibility enforcement in `ContosoDashboard/Services/TaskService.cs` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T053 [US4] Implement up-to-five newest currently authorized uploader documents and matching count in `ContosoDashboard/Services/DashboardService.cs` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T054 [US4] Notify current project members except uploader only after new accepted project uploads, with idempotent retry handling, in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T055 [US4] Add task attachment list, project-document picker, task upload, and visibility confirmation UI in `ContosoDashboard/Pages/Tasks.razor`
- [ ] T056 [US4] Add document summary count and Recent Documents actions in `ContosoDashboard/Pages/Index.razor`

**Checkpoint**: Task and dashboard context does not create any new document permission.

---

## Phase 7: User Story 5 - Review Document Activity (Priority: P3)

**Goal**: Give Administrators accurate, date-filtered activity reports and prove audited operations cannot bypass durable evidence.

**Independent Test**: Generate known actions and compare reports, including deleted-document history; deny non-Administrators and fault-inject audit persistence across every audited action.

- [ ] T057 [P] [US5] Write report aggregation tests for successful uploads by type, uploader ranks, date/user/document/download/preview patterns, deleted history, and failed/blocked exclusion in `ContosoDashboard.Tests/DocumentReportTests.cs`
- [ ] T058 [P] [US5] Write Administrator-only direct access and audit-failure matrix tests for upload, replace, download, preview, delete, grant, and revoke in `ContosoDashboard.Tests/DocumentAuditFailureTests.cs`
- [ ] T059 [US5] Implement Administrator-authorized date-filtered report queries that retain deleted evidence and exclude failed/blocked actions in `ContosoDashboard/Services/DocumentAuditService.cs`
- [ ] T060 [US5] Add Administrator-only report filters, type totals, uploader rankings, and activity tables in `ContosoDashboard/Pages/DocumentReports.razor`
- [ ] T061 [US5] Add Administrator-only report navigation while retaining direct server-side authorization in `ContosoDashboard/Shared/NavMenu.razor`

**Checkpoint**: Reports are accurate and access-controlled; audit persistence failures never produce a false success.

---

## Phase 8: Polish and Cross-Cutting Validation

**Purpose**: Complete security, performance, accessibility, offline, and release evidence across stories.

- [ ] T062 [P] Add 500-accessible/500-inaccessible document fixture seeding and timing tests in `ContosoDashboard.Tests/DocumentPerformanceTests.cs`
- [ ] T063 [P] Record keyboard, labels, action-count, desktop, and mobile validation evidence in `specs/001-document-management/quickstart.md`
- [ ] T064 [P] Record offline scanner, definition maintenance, macro-fixture, and no-cloud-runtime validation evidence in `specs/001-document-management/quickstart.md`
- [ ] T065 Exclude upload paths, SQLite databases, scanner output, and training artifacts in `./.gitignore`
- [ ] T066 Run build and tests and record results in `specs/001-document-management/quickstart.md` using `dotnet build ContosoDashboard/ContosoDashboard.csproj` and `dotnet test ContosoDashboard.Tests/ContosoDashboard.Tests.csproj`
- [ ] T067 Execute and record the complete acceptance, fault-injection, authorization, offline, and 20-trial performance workflow in `specs/001-document-management/quickstart.md`

---

## Dependencies and Execution Order

```text
Setup -> Foundational -> US1 (MVP) -> US2 -> US3 -> US4 -> US5 -> Polish
```

- US1 depends on Phase 2.
- US2 depends on US1 accepted-document persistence and protected streams.
- US3 depends on US1 upload/recovery/audit and US2 authorized discovery.
- US4 depends on US1 document creation, US2 discovery, and US3 notification contracts.
- US5 depends on audit events emitted by US1 and US3.

## Parallel Execution Examples

**Foundation**: T010-T014 and T018-T022 can be worked in parallel after T006-T009 establish shared types/configuration.

**US1 tests**: T023, T024, T025, and T026 can run in parallel before T027-T032.

**US2 tests**: T033, T034, and T035 can run in parallel before T036-T040.

**US3 tests**: T041, T042, and T043 can run in parallel before T044-T049.

**US4 tests**: T050 and T051 can run in parallel before T052-T056.

**US5 tests**: T057 and T058 can run in parallel before T059-T061.

## Implementation Strategy

**Phases**: Setup -> Foundation -> US1 only
**Tasks**: T001 - T045 (45 tasks)
**Estimated Time**: 6-8 hours for a developer familiar with ASP.NET Core/Blazor
**Deliverable**: Users can upload and view their documents

### MVP First

1. Complete setup and foundational tasks T001-T022.
2. Complete US1 tasks T023-T032.
3. Run US1 automated tests and the clean/threat/indeterminate Defender readiness checks.
4. Demonstrate secure upload/retrieval before browse, sharing, dashboard, or reporting work.

### Incremental Delivery

1. Add US2 for discovery and preview.
2. Add US3 for safe management and controlled sharing.
3. Add US4 for task/dashboard integration.
4. Add US5 for Administrator reporting.
5. Complete Phase 8 after all desired stories pass their independent checks.

## Format Validation

All 67 tasks use the required checkbox, sequential task ID, optional parallel marker only when applicable, user-story labels in story phases, and at least one exact repository-relative file path.