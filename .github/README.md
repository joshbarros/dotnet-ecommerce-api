# CI/CD Pipeline Documentation

## Overview

This project uses GitHub Actions for continuous integration and deployment. The pipeline includes multiple workflows to ensure code quality, security, and reliability.

## Workflows

### 1. CI/CD Pipeline (`ci.yml`)

Main pipeline that runs on every push and pull request to `main` and `develop` branches.

**Jobs:**

- **Build and Test**
  - Builds the .NET solution
  - Runs unit tests with code coverage
  - Runs architecture tests
  - Runs integration tests with Testcontainers
  - Publishes test results and coverage reports

- **Code Analysis**
  - Runs static code analysis
  - Checks code formatting with `dotnet format`
  - Treats warnings as errors

- **Security Scan**
  - Scans for security vulnerabilities
  - Checks NuGet packages for known vulnerabilities
  - Continues even if vulnerabilities are found (warning only)

- **Docker Build**
  - Builds multi-platform Docker images (amd64, arm64)
  - Pushes to Docker Hub on successful builds
  - Uses layer caching for faster builds

- **Performance Tests** (main branch only)
  - Runs k6 load tests
  - Validates API performance under load
  - Tests response times and error rates

- **Deploy to Staging** (develop branch)
  - Deploys to staging environment
  - Runs after successful build and tests

- **Deploy to Production** (main branch)
  - Deploys to production environment
  - Requires manual approval (configured in GitHub)

### 2. Dependency Review (`dependency-review.yml`)

Runs on pull requests to review dependency changes.

**Features:**
- Identifies new dependencies
- Checks for security vulnerabilities
- Blocks prohibited licenses (GPL-3.0, AGPL-3.0)
- Posts summary comment on PRs

### 3. CodeQL Analysis (`codeql-analysis.yml`)

Advanced security scanning that runs on:
- Every push to main/develop
- Pull requests
- Weekly schedule (Mondays at 6 AM UTC)

**Features:**
- Identifies security vulnerabilities
- Detects code quality issues
- Scans for common coding mistakes
- Reports findings to GitHub Security tab

## Required Secrets

Configure these secrets in GitHub repository settings:

```
DOCKER_USERNAME       # Docker Hub username
DOCKER_PASSWORD       # Docker Hub password/token
```

## Environment Configuration

### Staging Environment
- **Name:** staging
- **URL:** https://staging.ecommerce-api.example.com
- **Trigger:** Push to `develop` branch

### Production Environment
- **Name:** production
- **URL:** https://api.ecommerce.example.com
- **Trigger:** Push to `main` branch
- **Protection:** Requires manual approval

## Running Tests Locally

### Unit Tests
```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

### Integration Tests
```bash
# Requires Docker for Testcontainers
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

### Architecture Tests
```bash
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj
```

### All Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Performance Tests
```bash
# Start infrastructure
docker-compose up -d

# Run API
dotnet run --project src/API/Gateway/Gateway.csproj

# Run k6 tests (in another terminal)
k6 run tests/PerformanceTests/load-test.js
```

## Code Quality Standards

### Formatting
The pipeline enforces consistent code formatting using `dotnet format`:

```bash
# Check formatting
dotnet format --verify-no-changes

# Auto-format code
dotnet format
```

### EditorConfig
The project includes an `.editorconfig` file that defines:
- Indentation (4 spaces for C#)
- Line endings (LF)
- Naming conventions
- Code style preferences

Configure your IDE to respect EditorConfig settings.

## Performance Benchmarks

The performance tests validate:
- **P95 Response Time:** < 500ms
- **P99 Response Time:** < 1000ms
- **Error Rate:** < 1%
- **Load Capacity:** 100 concurrent users

## Docker Build

### Build Locally
```bash
docker build -f src/API/Gateway/Dockerfile -t ecommerce-api:latest .
```

### Run Container
```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__Database="Host=postgres;Database=ecommerce;Username=admin;Password=admin123" \
  ecommerce-api:latest
```

### Multi-Platform Build
```bash
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -f src/API/Gateway/Dockerfile \
  -t username/ecommerce-api:latest \
  --push .
```

## Troubleshooting

### Tests Failing Locally
1. Ensure Docker is running (for integration tests)
2. Check .NET SDK version (requires 8.0)
3. Restore packages: `dotnet restore`

### Docker Build Issues
1. Check Dockerfile paths are correct
2. Verify all referenced projects exist
3. Ensure .dockerignore is not excluding required files

### CI/CD Pipeline Failures
1. Check GitHub Actions logs
2. Verify all secrets are configured
3. Ensure branch protection rules allow workflows

## Best Practices

1. **Always run tests locally** before pushing
2. **Keep test coverage above 80%** for critical paths
3. **Fix security vulnerabilities** immediately
4. **Review dependency changes** in PRs
5. **Monitor performance metrics** after deployments

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [Docker Multi-Stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [k6 Performance Testing](https://k6.io/docs/)
