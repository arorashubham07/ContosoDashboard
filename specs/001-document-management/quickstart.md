# Quickstart: Document Upload and Management Validation

## Prerequisites

- Windows ARM64 training workstation with .NET 10 SDK and previously restored NuGet packages.
- Microsoft Defender enabled with locally available current definitions and the command-line scanner validated for the training image.
- A configured private local storage root outside `wwwroot` with write access for the application identity.
- Fictional users representing Employee, TeamLead, ProjectManager, and Administrator; at least two departments, two projects, current and former members, and task assignments.
- Fictional fixture files covering each allowed type, exactly 25,000,000 bytes, 25,000,001 bytes, empty files, mismatched types, EICAR, macro-free Office files, macro-bearing Office files, and malformed/encrypted Office containers.

See [data-model.md](data-model.md) for storage-state expectations and [document-contract.md](contracts/document-contract.md) for UI and file-stream behavior.

## Setup and Build

From the repository root, restore dependencies while connected if needed, then run offline:

```powershell
dotnet restore ContosoDashboard/ContosoDashboard.csproj
dotnet build ContosoDashboard/ContosoDashboard.csproj
dotnet test ContosoDashboard.Tests/ContosoDashboard.Tests.csproj
dotnet run --project ContosoDashboard/ContosoDashboard.csproj
```

Do not commit the generated SQLite database or uploads. For an intentional fresh training reset, stop the application, delete only the documented local database and private upload root, then restart so seed data is recreated. Never run a reset as part of normal startup or validation.

## Database Migration Transition

This feature introduces the first EF Core migration. Existing local databases were created with `EnsureCreated()` and do not contain migration history, so they cannot be upgraded in place. For the training environment only, stop the application, delete `ContosoDashboard/ContosoDashboard.db` and its `-wal`/`-shm` files, then start the application after the migration-aware startup change. This creates a fresh database with the seeded training data and document tables.

Do not use this reset procedure for production data or as part of normal application startup.

## Microsoft Defender Offline Readiness

1. On the Windows ARM64 training image, open Windows Security and confirm real-time protection is enabled and virus definitions are current before disconnecting from the network.
2. Run `Get-MpComputerStatus` in an elevated PowerShell session and confirm `AMServiceEnabled`, `AntivirusEnabled`, and `RealTimeProtectionEnabled` are `True`.
3. Locate the platform scanner with `Get-ChildItem 'C:\ProgramData\Microsoft\Windows Defender\Platform' -Recurse -Filter MpCmdRun.exe`, then set `MalwareScanner:ExecutablePath` to that executable when the default path differs.
4. Before offline validation, run `& $scanner -Scan -ScanType 3 -File <fictional-test-file>` against a harmless local test file and confirm the scanner exits successfully.
5. Disconnect the training workstation and repeat the harmless scan. Record the Defender platform and definition versions, command result, and ARM64 device model with the validation evidence.

The application treats a missing scanner, timeout, non-zero operational failure, or indeterminate result as unsafe and does not accept the upload.

## Validation Scenarios

1. Upload 1, then 10, valid files as a project member. Verify per-file progress/outcomes, accepted metadata, exact private-file download, current project visibility, and no public static-file URL. Attempt 11 files and verify the whole selection is rejected before transfer.
2. Exercise empty, oversized, unsupported, MIME/extension-mismatched, EICAR, macro-bearing, malformed, encrypted, unavailable-scanner, and scan-timeout inputs. Verify specific errors, no Accepted document, no streamable content, and recorded cleanup handling.
3. With distinct users/roles/memberships, directly request a document stream, preview, search, metadata action, and report using unauthorized IDs. Verify denial reveals neither content nor protected metadata. Remove an uploader from a project and verify immediate loss of uploader-based access while authorized managers retain their scoped access.
4. Verify My Documents sorting and combined category/project/inclusive-date filters; Project Documents; Shared with Me idempotence and revocation; case-insensitive search across each specified metadata field; PDF/JPEG/PNG preview; and accessible empty states.
5. Replace a file and verify identity, original uploader/date, shares, and task links persist while the old private file is removed. Cancel then confirm deletion; verify content, links, shares, dashboard count, and search entries disappear while activity remains reportable.
6. Attach eligible project documents to tasks, reject cross-project attempts, and confirm the personal-to-project visibility warning. Verify project additions and shares create one notification per new entitlement and failed notification delivery is recoverable without undoing acceptance.
7. Confirm dashboard Recent Documents contains the five newest currently authorized uploads by the current user and its count matches My Documents. Verify administrator date-filtered reports, deleted-document history, and direct denial for a non-administrator.
8. Inject each recovery and audit persistence failure described in [research.md](research.md). Verify audited operations report a retryable failure before byte release or mutation and never claim success.
9. Run at least 20 trials under the reference conditions in the feature specification. Record upload, list, search, and preview timings; manually check keyboard traversal, labels, upload action count, and desktop/mobile layouts.