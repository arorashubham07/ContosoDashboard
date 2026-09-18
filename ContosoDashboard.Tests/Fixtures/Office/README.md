# Office Test Fixtures

Use fictional, non-sensitive files only. Keep binary fixtures under version control only when they are small and licensed for this repository.

| Fixture | Expected result | Purpose |
|---|---|---|
| `clean.docx`, `clean.xlsx`, `clean.pptx` | Accepted | Valid macro-free OOXML documents. |
| `macro.docm`, `macro.xlsm`, `macro.pptm` | Rejected | OOXML packages containing a VBA project. |
| `legacy-macro.doc`, `legacy-macro.xls`, `legacy-macro.ppt` | Rejected | Legacy Compound File Binary documents containing VBA storage. |
| `malformed.docx` | Rejected | Invalid or truncated OOXML archive. |
| `encrypted.docx` | Rejected | Password-protected or encrypted Office container. |

Do not use real company documents, passwords, or malware. Content-based inspection tests create their own minimal inputs until this documented fixture corpus is added.