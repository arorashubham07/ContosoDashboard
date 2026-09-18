# Research: Document Upload and Management

## Offline Malware Scanning

**Decision**: Implement `IMalwareScanner` with a Windows Defender adapter that invokes `MpCmdRun.exe` with an argument list to custom-scan a private staging file. Accept only an unambiguously clean result; missing Defender, unavailable service or definitions, timeout, detection, malformed output, and non-clean exit results are indeterminate or unsafe and reject the file.

**Rationale**: Microsoft Defender is integrated with the Windows ARM64 training platform and can operate using locally present definitions. It fulfills the requirement for an actual scan without a cloud account, SDK, or a separately managed native scanner. Staging content outside `wwwroot` keeps it inaccessible before acceptance.

**Alternatives considered**: ClamAV or a third-party command-line scanner introduces unsupported Windows ARM64 packaging and offline-signature risks. AMSI is not a complete uploaded-file scanner. A simulated clean scanner is prohibited by FR-006.

**Validation needed**: On a disconnected Windows ARM64 training device, prove acceptance of clean fixtures and rejection of EICAR, a disabled/missing executable, timeout, scan error, and unavailable definitions. Document initial definition acquisition and offline execution separately.

## Office Macro Inspection

**Decision**: Add `IOfficeMacroInspector` before scanner acceptance. Inspect OOXML `.docx`, `.xlsx`, and `.pptx` ZIP packages with `System.IO.Compression` for VBA parts, relationships, and content types. Inspect legacy `.doc`, `.xls`, and `.ppt` Compound File Binary containers with a vetted managed parser; any parse, encryption, or inspection ambiguity returns indeterminate and rejects the file.

**Rationale**: The approach is offline and works at the service boundary for initial uploads and replacements. Fail-closed behavior prevents a clean malware scan from incorrectly approving macro-containing or unreadable Office content.

**Alternatives considered**: Office COM automation is unsuitable for a server process. LibreOffice headless adds a native deployment dependency. Filename, MIME, and malware-only checks cannot satisfy FR-003.

**Validation needed**: Use fictional macro-free, macro-bearing, malformed, and encrypted fixtures for every supported Office extension. The selected legacy parser is a release gate until it passes these fixtures on Windows ARM64.

## Filesystem, SQLite, and Mandatory Audit Evidence

**Decision**: Use private same-volume staging and final directories, server-generated GUID relative paths, an `Accepted` availability state, and SQLite recovery-journal records. Authorize first; stage and flush; inspect and scan; persist recovery intent; atomically rename to final storage; then commit accepted metadata and successful audit outcome in one SQLite transaction. Stream routes write durable audit evidence before releasing content. Replacements, deletions, grants, and revocations follow the same audit-before-commit rule.

**Rationale**: SQLite cannot join an NTFS rename in a distributed transaction. The recovery journal makes incomplete file operations discoverable, while the `Accepted` metadata gate ensures only committed records can be listed or served. Coupling metadata and audit records in SQLite prevents false successful operations when audit persistence fails.

**Alternatives considered**: Metadata-first writes can expose broken records. File-first writes without recovery state leave unreconciled private files after crashes. Best-effort auditing or a fallback queue violates FR-025.

**Validation needed**: Fault-inject stage, macro, scan, journal, rename, metadata save, audit save, cleanup, and restart recovery. Assert no accessible document/content or false success after failures; preserve prior content for failed replacements and deletions.

## Automated Verification

**Decision**: Add `ContosoDashboard.Tests` with xUnit, file-backed temporary SQLite databases, and temporary private storage roots. Use fakes for scanner, macro inspection, clock, and induced storage/audit failures; use a real SQLite/local-storage integration subset. Keep the Windows Defender readiness suite as a documented manual release check rather than a mock-only test.

**Rationale**: File-backed SQLite exercises relational and transactional behavior that EF InMemory does not. Fakes make the failure paths repeatable while the platform suite proves the required real scanner integration.

**Alternatives considered**: UI-only testing misses direct-object authorization and recovery behavior. EF InMemory does not represent SQLite transactions faithfully. Mock-only malware verification conflicts with the specification.

**Validation needed**: Cover role and membership denials, current-membership revocation, scan/macro outcomes, all audited-operation failures, storage cleanup, direct stream endpoints, notification idempotence, and report retention after deletion.