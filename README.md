# ASP.NET Core MVC Template with Enterprise DevSecOps Blueprint

This repository serves as a production-ready, enterprise-grade development template for an **ASP.NET Core 8.0 MVC** application (`MvcMovie`).

The core focus of this blueprint is **automated governance, strict security gates, and container hardening**. It implements a complete DevSecOps pipeline using native GitHub Actions and industry-standard security scanners, serving as an immutable reference template for future .NET architectures.

---

## 1. Directory Structure

The repository organizes source code, unit tests, and pipelines into isolated modules:

```text
.
├── .github/
│   ├── workflows/
│   │   ├── code-style.yml          # Automated C# code formatting gate
│   │   ├── codeql-analysis.yml     # Deep SAST data-flow vulnerability scanner
│   │   ├── dotnet-tests.yml        # Test execution (Unit, Integration, Playwright)
│   │   ├── docker-build-scan.yml   # Multi-stage Docker build & Trivy scanner
│   │   ├── cd-deployment.yml       # Multi-stage Continuous Delivery (CD)
│   │   └── resharper-analysis-pr.yml # PR code smell inline annotator
│   └── dependabot.yml              # Dependency tracking configuration
├── MvcMovie/                       # ASP.NET Core 8.0 MVC Web App
│   ├── Controllers/, Models/, Views/, wwwroot/
│   ├── Program.cs, appsettings.json
│   └── MvcMovie.csproj             # Main project dependencies
├── MvcMovie.Tests/                 # Isolated Automated Testing Module
│   ├── UnitTests/                  # Fast logical unit testing
│   ├── IntegrationTests/           # HTTP/Database integration test suites
│   ├── PlaywrightTests/            # UI End-to-End browser tests
│   └── MvcMovie.Tests.csproj
├── .editorconfig                   # Unified formatting rule definition
├── .gitignore                      # .NET and SQLite ignore lists
├── Dockerfile                      # Hardened multi-stage container manifest
├── .dockerignore                   # Docker build context filter
└── MvcMovie.sln                    # Central solution layout
```

---

## 2. DevSecOps Pipeline & Governance (10-Gate Architecture)

The CI/CD layout implements ten distinct validation gates to prevent security vulnerabilities, code smells, or layout regression from reaching production.

### Gate 1: ReSharper Code Analysis

- **Workflow File:** `resharper-analysis-pr.yml`
- **Mechanism:** Runs the JetBrains `inspectcode` Command Line Tool in the CI runner.
- **Governance:** Injects code smells and styling issues directly as live, interactive inline comments inside Pull Requests, eliminating style debt at the peer-review level.

### Gate 2: Playwright End-to-End Testing

- **Workflow File:** `dotnet-tests.yml`
- **Mechanism:** Installs Playwright browser runtimes and executes black-box UI tests.
- **Governance:** Automates browser user flow verification (CRUD forms, rendering, redirects) in real headless browsers (Chromium, Firefox, WebKit).

### Gate 3: Unit and Integration Testing (xUnit)

- **Workflow File:** `dotnet-tests.yml`
- **Mechanism:** Executes tests using `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) using SQLite.
- **Governance:**
    - **LocalDB Replacement:** Replaces heavy SQL Server LocalDB with SQLite to run tests without database engine overhead on lightweight Linux runners.
    - **Dynamic DB Profiles:** Runs in-memory (`Data Source=:memory:`) in CI for transient, volatile test execution (auto-wiped on completion), and persists locally (`Data Source=MvcMovie.db`) during development.

### Gate 4: Docker Multi-Stage Build & Trivy Scan

- **Workflow File:** `docker-build-scan.yml`
- **Mechanism:** Builds a hardened container image and scans it with **Trivy** (Aqua Security).
- **Governance:**
    - **Hardened Base Image:** Compiles with `.NET 8 SDK`, then copies compiled binaries into the minimal, secure, non-root `8.0-jammy-chiseled` Ubuntu-based ASP.NET Runtime image.
    - **Trivy Scanner:** Analyzes OS and packages. Fails the pipeline if any `CRITICAL` vulnerability with an available patch is found (`ignore-unfixed: true`).

### Gate 5: CodeQL Static Security Scanning (SAST)

- **Workflow File:** `codeql-analysis.yml`
- **Mechanism:** Native GitHub Advanced Security engine that intercepts compilation.
- **Governance:** Scans abstract syntax trees (AST) and data-flows for SQL injection, Cross-Site Scripting (XSS), cryptographic weaknesses, and hardcoded secrets.

### Gate 6: Database Migration Verification

- **Local / CI Command:**
    ```bash
    dotnet ef migrations script --output migration.sql --project MvcMovie/MvcMovie.csproj
    ```
- **Governance:** Ensures EF Core migrations compile successfully, generates SQL deployment scripts, and verifies schema synchronicity (`dotnet ef migrations has-pending-model-changes`) to prevent database drift in production.

### Gate 7: Automated Code Style Verification (`dotnet format`)

- **Workflow File:** `code-style.yml`
- **Mechanism:** Executes a read-only formatting check:
    ```bash
    dotnet format --verify-no-changes
    ```
- **Governance:** Fails the build if code layout, spacing, or namespace import ordering violates the rules declared in `.editorconfig`.

### Gate 8: Continuous Delivery (CD) Environments & Approval Gates

- **Workflow File:** `cd-deployment.yml`
- **Governance:**
    - **Staging Environment:** Pushes the verified container automatically to a testing sandbox upon merge.
    - **Production Environment:** Protected by an environment lock. Deploying requires manual sign-off by authorized administrators in the GitHub UI before release.

### Gate 9: Automated Supply Chain Tracking (Dependabot)

- **Configuration:** `dependabot.yml`
- **Mechanism:** Continuously watches upstream NuGet package registries and GitHub Actions markets.
- **Governance:** Automatically provisions pull requests with release notes to update packages containing security vulnerabilities or outdated code interfaces.

### Gate 10: Enforced Branch Protection

- **Governance:**
    - Requires all status checks (`Analyze Code`, `inspect-code`, `run-tests`, `Trivy Scan`) to be green before merging.
    - Enforces a **Linear History** (Squash/Rebase merging only) to maintain a clean git history tree, allowing quick debugging and rollback capability.

---

## 3. Local Development Guide

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)

### Setup & Run

1. Restore packages and run the application:
    ```bash
    dotnet restore
    dotnet run --project MvcMovie/MvcMovie.csproj
    ```
2. Open `http://localhost:5000` (or the HTTPS port shown in terminal) to access the application.
3. The application will auto-generate a physical local SQLite database file named `MvcMovie.db`.

### Database Migrations

To generate database scripts or apply changes to your SQLite database schema:

- **Add a migration:**
    ```bash
    dotnet ef migrations add <MigrationName> --project MvcMovie/MvcMovie.csproj
    ```
- **Generate SQL scripts:**
    ```bash
    dotnet ef migrations script --output migration.sql --project MvcMovie/MvcMovie.csproj
    ```
- **Apply migrations directly:**
    ```bash
    dotnet ef database update --project MvcMovie/MvcMovie.csproj
    ```

### Code Formatting

To clean up your workspace files automatically according to the `.editorconfig` rules:

```bash
dotnet format
```

### Running Tests

To run unit, integration, and E2E tests concurrently:

```bash
dotnet test
```

---

## 4. Building the Production Container Locally

To test the multi-stage, secure container configuration:

1. Build the Docker image using the root context:
    ```bash
    docker build -t mvcmovie:latest .
    ```
2. Run the container:
    ```bash
    docker run -d -p 8080:8080 --name mvcmovie-app mvcmovie:latest
    ```
3. Test connectivity:
    ```bash
    curl http://localhost:8080
    ```

_Note: The container executes inside a chiseled environment without a shell or default root credentials, following the principle of least privilege._
