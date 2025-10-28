1. Core Constraints
SDK Specification: .NET 9.0 (builds must fail if major version differs)
Ports Specification: API must bind to HTTP 5000 and HTTPS 5001 only
Naming Specification: Project and table names must use the Po.AppName.* prefix exactly
Storage Specification: Azure Table Storage by default; use Azurite locally
Testing Specification: xUnit (Unit/Integration); Playwright MCP TypeScript (E2E, manual execution only)
Code Style Specification: Use dotnet format; files ≤ 500 lines
Specification: Automate with one-line CLI commands only
2.  Architecture & Project Layout
Architectural Style: Use Vertical Slice Architecture with Clean Architecture boundaries where complexity requires separation.
Code Philosophy: Enforce SOLID and appropriate GoF patterns; prioritize simple, small, and well-factored code.
Repository layout at root:
/src/PoAppName.Api (PLUS other projects required for the Vertical Slice/Onion Architecture)
/src/PoAppName.Client (Blazor Wasm hosted inside the API project)
/src/PoAppName.Shared (for classes used by the Client and Server)
/tests/PoAppName.UnitTests
/tests/PoAppName.IntegrationTests
/tests/PoAppName.E2ETests
3.  Backend
Error Standard: Implement global exception handling middleware that transforms all errors into RFC 7807 Problem Details responses.
Health Check: Expose the mandatory .NET health check endpoint with readiness and liveness semantics.
API Interface: Expose Swagger/OpenAPI from project start to document endpoints for manual testing and allow them to be called from .http files
Logging: Use Serilog for structured logging and configure sensible local sinks; follow .NET best practices for telemetry.
4.  Frontend
UX Focus: Prioritize an excellent mobile portrait UX and fully responsive layout (fluid grid, touch controls). Test on portrait mobile emulation.
Component Libraries: Start with built-in Blazor components. Adopt Radzen.Blazor only for advanced scenarios explicitly justified by UX necessity.
5.  Testing Workflow
TDD Practice: Follow Test-Driven Development (TDD): write a failing test first, then implement code.
Test Isolation: Maintain separate Unit and Integration and E2E test projects.
Database Isolation: Integration tests must run against Azurite or isolated disposable test tables, including setup and teardown to ensure no lingering data.
E2E Workflow: Playwright MCP tests are run manually by the user and are explicitly excluded from CI/CD.


