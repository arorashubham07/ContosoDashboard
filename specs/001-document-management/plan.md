# Implementation Plan: Document Upload and Management

**Branch**: `001-document-management` | **Date**: 2026-09-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-management/spec.md`

## Summary

Deliver secure, offline document upload, browsing, sharing, task attachment, dashboard, and audit workflows for signed-in training users. Extend the ASP.NET Core Blazor Server application with EF Core SQLite metadata, private local filesystem storage, service-layer authorization, a Windows Defender-backed malware scanner, a fail-closed Office macro inspector, and authorized file streaming endpoints. Keep accepted-document metadata and successful audit records transactionally coupled; use a durable recovery journal to reconcile filesystem failures that SQLite cannot transact atomically.

## Technical Context

**Language/Version**: C# on .NET 10.0
**Primary Dependencies**: ASP.NET Core Blazor Server, EF Core 8 SQLite, cookie-based mock authentication, Bootstrap 5.3/Bootstrap Icons; add xUnit test packages and a managed Compound File Binary parser only after Windows ARM64 fixture validation
**Storage**: SQLite metadata plus a local, private filesystem root outside `wwwroot`; recovery-journal records in SQLite
**Testing**: New xUnit test project using temporary file-backed SQLite databases and storage roots; manual Windows ARM64 Defender readiness/performance checks
**Target Platform**: Offline-capable Windows ARM64 training workstation using a supported desktop browser
**Project Type**: Existing single-project ASP.NET Core web application
**Performance Goals**: 95% of valid 25 MB single-file uploads within 30 seconds; list/search within 2 seconds for the defined 500 accessible plus 500 inaccessible document fixture; preview within 3 seconds
**Constraints**: No cloud runtime dependency; 1-10 files/batch, 25,000,000 bytes/file; allowed content/type and macro validation; actual offline scan; current authorization at every service and streaming boundary; audit failure blocks operations
**Scale/Scope**: Five prioritized user stories, four existing training roles, up to 500 accessible documents per reference test user

## Constitution Check

*GATE: Passed before Phase 0 research and re-checked after Phase 1 design.*

| Principle or constraint | Plan response | Status |
|---|---|---|
| I. Specification-Driven Delivery | Design maps every document workflow to the approved specification, artifacts, and future verification tasks. | Pass |
| II. Offline Training by Default | Use SQLite, private local storage, Windows Defender with locally available definitions, and no Azure SDK/runtime service. Azure migration remains interface-only guidance. | Pass, subject to machine readiness check |
| III. Authorization at Every Data Boundary | `IDocumentService` evaluates caller identity, roles, current project membership, team department, and explicit grants for lists, mutations, search, reports, and stream endpoints. | Pass |
| IV. Simple, Separated Responsibilities | Pages/components present state; document, scanner, macro, storage, audit, and authorization concerns use focused injected interfaces. The scanner abstraction is required by the offline/real-scan constraint; the recovery journal is required because SQLite and NTFS lack a distributed transaction. | Pass |
| V. Verifiable Changes | Add automated success/denial/failure-path tests and documented manual Defender, macro-fixture, performance, accessibility, and responsive checks. | Pass |
| File and database safeguards | Generate server-controlled relative paths before metadata insertion, stage outside `wwwroot`, reject unsafe content, clean failures, preserve deleted audit evidence, and document explicit development-data reset only. | Pass |
| Existing noncompliance | Current `EnsureCreated()` initialization and mock-auth revalidation require migration/testing design attention but are not changed by this plan without scoped tasks. | Recorded |

Post-design result: no constitution violation is introduced. Implementation cannot declare the scanner or macro-inspection performance requirements satisfied until the specified Windows ARM64 readiness suite and fixture corpus pass.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs          # DbSets, relationships, indexes, recovery state
├── Models/
│   ├── Document.cs
│   ├── DocumentShare.cs
│   ├── TaskDocument.cs
│   ├── DocumentActivity.cs
│   └── DocumentRecoveryRecord.cs
├── Services/
│   ├── DocumentService.cs               # authorization and workflow orchestration
│   ├── DocumentAuthorizationService.cs
│   ├── LocalFileStorageService.cs
│   ├── WindowsDefenderMalwareScanner.cs
│   ├── OfficeMacroInspector.cs
│   └── DocumentAuditService.cs
├── Pages/
│   ├── Documents.razor
│   ├── ProjectDetails.razor             # project document section
│   ├── Tasks.razor                      # document attachment/upload controls
│   └── Index.razor                      # count and recent documents
├── Shared/
│   └── NavMenu.razor
├── Program.cs                           # DI, private-stream endpoints, configuration
└── appsettings*.json                    # private storage/scanner configuration, no secrets

ContosoDashboard.Tests/
├── DocumentServiceTests.cs
├── DocumentAuthorizationTests.cs
├── DocumentStorageRecoveryTests.cs
├── DocumentStreamContractTests.cs
└── Fixtures/
  └── Office/                          # fictional clean/macro/malformed fixtures
```

**Structure Decision**: Retain the existing single web project. Add domain models and injected services in the present `ContosoDashboard` folders, protected streaming routes in `Program.cs`, and a sibling xUnit project for service, storage, authorization, and HTTP contract checks.
