Coding Rules for Blazor WebAssembly / .NET (Check PRD.MD for app description / Check STEPS.MD for 10 high level steps to complete the app, Check Diagram folder for quick overview of app)
1. Guiding Philosophy & Standards
Enforce Strict Technology & Ports: Use .NET 9.0 exclusively. The API must run on HTTP 5000/HTTPS 5001 and host the Blazor Wasm application.
Prioritize Simplicity & Principles: Enforce SOLID principles and Design Patterns (GoF, etc.) throughout the codebase. Maintain simplicity, conciseness, and ease of understanding via proactive refactoring.
Automate Everything: Automate all operations (build, deploy, etc.) using CLI tools (dotnet, az, gh, azd, etc.).

2. Architecture & Maintenance
Strictly Enforce Architectural Style: Employ Vertical Slice Architecture with Clean Architecture boundaries where complexity dictates.
Single Responsibility: Limit all files to ≤500 lines by refactoring into focused components.
Project Structure & Naming: Use the standard layout (/src, /tests, /docs, /scripts). Prefix all projects with **Po.AppName** (e.g., Po.AppName.Api).
Do not create additional .md files beyond PRD.MD STEPS.MD README.MD or .ps1 files

3. API, Observability, & Error Handling
Mandate API Observability: Enable Swagger/OpenAPI documentation from inception and expose a mandatory /api/health endpoint for readiness/liveness checks.
Fix and Enforce Error Handling: Implement global error-handling using RFC 7807 Problem Details (via Serilog). NEVER return raw exception messages or stack traces to callers.

4. Data Persistence & Frontend
Default Data Persistence & Naming: Default to Azure Table Storage (use Azurite locally); only use Azure SQL/Cosmos DB with tech-lead approval. Name tables using the pattern: PoAppName[TableName].
Frontend Components: Start with built-in Blazor components; adopt Radzen.Blazor only for advanced scenarios.

5. Testing & Workflow
Mandate Test-Driven Development (TDD): Always follow the TDD cycle: Write a failing xUnit test first before implementing code. Maintain separate unit, integration, and functional tests.
Execution and Documentation Directive: Always run the API project for execution. Refer to and mark steps complete in STEPS.MD as development progresses (if the file exists).
Create E2E tests with Playwright MCP as needed to cover the main application functionality


