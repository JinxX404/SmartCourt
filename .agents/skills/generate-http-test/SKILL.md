---
name: generate-http-test
description: Generates comprehensive, exhaustive HTTP PowerShell test scripts for a target vertical slice following the SmartCourt automated testing methodology.
---

# Generate HTTP Test Script

When asked to write or generate HTTP PowerShell test scripts for a specific vertical slice in the SmartCourt project, you MUST strictly follow these requirements:

## 1. Zero Assumption Workflow (End-to-End Integration)
- **Do not assume any existing state in the database.** 
- If the endpoints require authentication, the script MUST start from scratch:
  - Register a new account.
  - Parse the background email logs to extract the verification token.
  - Confirm the email.
  - Log in to obtain a JWT.
  - Complete the user profile (if required).
  - Proceed to test the target endpoints.
- Redundancy is expected and required to ensure the workflow functions end-to-end.

## 2. Exhaustive Code Coverage
- You must test **every single endpoint** inside the target slice. 
- For each endpoint, script multiple scenarios to guarantee every single line of code in the Controller, Service, and Validators is hit:
  - **Valid success paths** (200 OK, 201 Created, 204 No Content).
  - **400 Validation errors** (missing fields, wrong formats, invalid ranges).
  - **404 Not Found errors** (invalid IDs, unauthorized access to resources).
  - **401/403 Security errors** (missing token, invalid role).

## 3. Stressful & Edge Case Testing
- Include aggressive test scenarios designed to break the system:
  - **Malicious payloads**: SQL Injection attempts, XSS tags, null bytes.
  - **Extreme inputs**: Massive string lengths, negative numbers, zeros where inappropriate.
  - **Unicode and unusual characters**: Emojis, Zalgo text, non-Latin scripts.
  - **Type mismatches**: Sending strings for integers, invalid date formats.
  - **Missing or invalid headers**: E.g., dropping `Content-Type`, invalid Accept headers.

## 4. Standardized Formatting & Output
- **Use TestHelpers**: Rely on `TestHelpers.psm1` using functions like `Invoke-Api` and `Extract-EmailConfirmationUrl` where applicable.
- **Authorization**: Ensure all scripts use proper authorization headers automatically obtained from the earlier registration/login steps in the workflow.
- **Markdown Reporting**: The script MUST output a detailed, readable Markdown report (e.g., `[SliceName]_Report.md`) that logs:
  - The endpoint tested.
  - The JSON body sent.
  - The HTTP status code received.
  - The full JSON response received for every single scenario.

## Execution Steps
1. **Analyze the Slice**: Review the Controller, Service, DTOs, and Validators for the target slice to identify all required routes, inputs, and business rules.
2. **Draft the Script**: Create the PowerShell script in `SmartCourt.Tests/HttpTests/` following the constraints above.
3. **Review Against Requirements**: Verify the script includes the zero-assumption setup, edge case tests, and markdown report generation.
