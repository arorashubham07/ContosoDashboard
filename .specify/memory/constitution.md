# ContosoDashboard Constitution

## Core Principles

### I. Specification-Driven Delivery

New features MUST begin with a specification defining user outcomes, scope, acceptance
criteria, and access rules before implementation. Plans MUST identify affected layers,
data contracts, verification steps, and constitution compliance. Implementation tasks
MUST be traceable to that specification. Bug fixes MUST identify the failing behavior
and a reproducible check. Changes to agreed behavior MUST update the relevant specification
and tasks. This keeps the repository usable for teaching Spec-Driven Development.

### II. Offline Training by Default

The application MUST remain an educational, local-only system, not a production offering.
After documented dependency installation, training workflows MUST run without cloud accounts,
paid services, network APIs, or externally hosted runtime assets. Persistence MUST use local
SQLite and any file storage MUST use the local filesystem. Mock user-selection authentication
MUST remain explicitly labeled as training-only. Cloud migration examples MUST remain optional
guidance and MUST NOT introduce runtime dependencies into the training application.

### III. Authorization at Every Data Boundary

Protected pages MUST require authentication. Services MUST validate the authenticated caller's
role and applicable ownership or project membership before reading or mutating protected data.
Client-supplied identifiers and hidden UI controls MUST NOT be treated as authorization.
Role privileges and membership restrictions MUST be explicit in acceptance criteria, including
whether an administrator bypass is permitted; no bypass may be inferred from a role label.
Security changes MUST verify both allowed and denied operations, including direct access using
another user's resource identifier. Mock authentication does not waive data-isolation rules.

### IV. Simple, Separated Responsibilities

Pages and shared components MUST handle presentation and delegate business decisions to services.
Models MUST describe domain data; the data layer MUST own persistence configuration. New or changed
infrastructure access MUST use dependency-injected interfaces or existing framework abstractions
so business logic does not depend on a particular storage provider. Changes MUST reuse existing
patterns and remain scoped to the requirement. Additional layers, packages, or abstractions MUST
have a documented need in the plan; hypothetical future features are not sufficient justification.

### V. Verifiable Changes

Every behavioral change MUST include repeatable verification tied to its acceptance criteria.
Bug fixes MUST include a regression check that distinguishes the failure from the corrected behavior.
Service rules, authorization, persistence, and changed contracts MUST have automated checks for
affected success and failure paths. UI checks MAY be manual when their setup, steps, expected
results, and actual results are recorded. Unexecuted checks and known failures MUST be disclosed;
completion MUST NOT imply validation that was not performed.

## Technical Constraints

- The baseline stack MUST remain ASP.NET Core 10.0, Blazor Server, EF Core with SQLite,
	cookie-based mock authentication, and Bootstrap 5.3 with Bootstrap Icons unless an approved
	constitution amendment changes that baseline.
- Dependencies MUST support the documented Windows ARM64 training environment. Setup instructions
	MUST distinguish initial package acquisition from offline application execution.
- Changes MUST preserve the four training roles: Employee, TeamLead, ProjectManager, and
	Administrator. Access-policy changes MUST update both documentation and verification cases.
- Database changes MUST document effects on existing local data and fresh seeded databases.
	Destructive reset procedures MUST be explicit and MUST NOT run silently.
- File-based features MUST generate unique server-controlled paths before inserting metadata,
	validate file size and type against specified limits, prevent path traversal, enforce access
	checks, and define cleanup when either storage or database operations fail.
- Secrets, real personal data, local database files, and uploaded training artifacts MUST NOT
	be committed. Seed data MUST remain fictional.
- User-facing changes MUST preserve established navigation and styling, keyboard access,
	labeled inputs, and layouts without overlapping content at desktop and mobile widths.
- Documentation MUST describe limitations accurately and MUST NOT claim production readiness
	or completed cloud integration.

## Development Workflow and Quality Gates

1. Define the feature specification and resolve material scope or access-policy ambiguities.
	For a bounded bug fix, record the reproduction and expected behavior instead.
2. Prepare a plan and dependency-ordered tasks. Review all five principles and the technical
	constraints before implementation; identify any existing noncompliance in the affected area.
3. Implement the smallest scoped change and add or update the relevant verification cases.
	Preserve unrelated work and avoid opportunistic refactoring.
4. For application changes, run `dotnet build ContosoDashboard/ContosoDashboard.csproj` and
	the affected automated tests. Verify changed training workflows locally, including relevant
	seeded roles, unauthorized access, and persistence failure cases. For documentation-only
	changes, validate structure, references, and consistency instead of building the application.
5. Before merge, a reviewer MUST check specification traceability, constitution compliance,
	verification evidence, and updated setup or limitation documentation. New failures caused
	by the change MUST be resolved. Pre-existing failures and unexecuted checks MUST be recorded
	with their impact and an explicit maintainer decision before merge.

## Governance

This constitution is the authoritative project policy. Specifications, plans, tasks, and review
decisions MUST comply with it. README.md provides operational guidance but does not override these
rules. Existing noncompliance MUST be recorded when encountered; adopting this constitution does
not assert that every existing implementation is compliant or authorize unrelated remediation.

Amendments MUST describe the proposed rule change, rationale, affected workflows, version impact,
and any migration or follow-up work. A project maintainer MUST approve an amendment before merge.
Conflicting work MUST wait for an approved amendment; silent exceptions are not permitted.
Dependent artifacts MUST be reviewed for impact during amendment review and updated separately
when needed, not automatically rewritten by the constitution command.

Constitution versions MUST follow semantic versioning: MAJOR for incompatible principle removals
or redefinitions, MINOR for new principles or materially expanded guidance, and PATCH for
non-semantic clarifications or corrections. The initial completed constitution is version 1.0.0.
Ratification remains the original adoption date; Last Amended MUST reflect the date of the latest
content change, with dates in YYYY-MM-DD format. Each amendment MUST include a temporary Sync Impact
Report for human review, which MUST be removed before committing the amended constitution.

Compliance MUST be reviewed during planning and before merge. Reviews MUST cite applicable rules
and verification evidence rather than merely declaring compliance.

**Version**: 1.0.0 | **Ratified**: 2026-09-14 | **Last Amended**: 2026-09-14
