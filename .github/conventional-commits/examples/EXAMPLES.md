# Conventional Commits Examples

Real-world examples of Conventional Commits for the Drive Architecture repository.

## Basic Examples

### Simple Feature Addition

**Scenario**: Added Redis caching to API

```
feat(cache): add Redis distributed caching
```

**With body**:
```
feat(cache): add Redis distributed caching

Implements Redis-based caching layer to improve API response times
for frequently accessed driver license data. Uses StackExchange.Redis
client with connection pooling and 5-minute TTL for hot data.

Refs: #234
```

### Bug Fix

**Scenario**: Fixed authentication bug

```
fix(auth): prevent token expiration race condition
```

**With details**:
```
fix(auth): prevent token expiration race condition

The token refresh logic had a race condition where concurrent
requests could simultaneously attempt to refresh the same token,
causing authentication failures. Added semaphore-based locking
to ensure only one refresh operation executes at a time.

Fixes: #456
```

### Documentation Update

**Scenario**: Updated Arc42 documentation

```
docs(arc42): add runtime view for payment processing
```

**With context**:
```
docs(arc42): add runtime view for payment processing

Added sequence diagram and textual description of payment processing
flow through Drive.Portal, DriveAPI, and PaymentGateway. Includes
error handling and retry logic documentation.

Refs: #789
```

## Architecture-Specific Examples

### DSL Model Changes

**Adding new container**:
```
docs(dsl): add notification service container

Added NotificationService container to Drive system model with
relationships to DriveAPI and Azure Service Bus. Includes email
and SMS notification capabilities.

Updated views:
- System context diagram
- Container diagram
- Azure deployment diagram

Refs: #123
```

**Modifying relationships**:
```
docs(dsl): update RMV3 integration relationships

Changed DriveAPI -> RMV3 relationship from direct HTTP to
asynchronous message queue via Azure Service Bus, reflecting
ADR-016 decision.

Refs: ADR-016, #234
```

**Adding perspectives**:
```
docs(dsl): add security perspectives to containers

Tagged all containers with security classification (public, internal,
confidential) and data retention policies. Supports Cyber Essentials
compliance assessment.

Refs: #345
```

### Arc42 Documentation

**Adding new chapter content**:
```
docs(arc42): document OAuth2 integration in chapter 8

Added runtime view showing OAuth2 authentication flow between
Drive.Portal, DriveAPI, and MyNSID. Includes token acquisition,
refresh, and revocation sequences.

Refs: #456
```

**Updating architectural decisions**:
```
docs(arc42): update chapter 9 with new ADRs

Added ADR-015 (Caching Strategy), ADR-016 (Async Integration),
and ADR-017 (API Versioning) to Architecture Decisions chapter.

Refs: ADR-015, ADR-016, ADR-017
```

### ADR (Architecture Decision Records)

**Creating new ADR**:
```
docs(adr): add ADR-018 for circuit breaker pattern

Documented decision to implement circuit breaker pattern using
Polly library for external service calls. Covers options evaluated,
pros/cons, and implementation guidance.

Refs: #567
```

**Superseding ADR**:
```
docs(adr): supersede ADR-012 with ADR-018

Updated ADR-012 status to superseded and linked to ADR-018
which provides improved resilience strategy.

Refs: ADR-012, ADR-018
```

## Breaking Changes

### API Removal

```
feat(api)!: remove deprecated v1 endpoints

All v1 API endpoints have been removed as planned. Removed endpoints:
- GET /v1/licenses
- POST /v1/licenses/renew
- GET /v1/vehicles

BREAKING CHANGE: v1 API endpoints removed. All clients must migrate
to v2 endpoints. See migration guide at docs/api-migration.md.

Closes: #789
```

### Interface Change

```
refactor(cache)!: convert cache interface to async

Changed ICache interface to use async/await pattern for all methods.
All cache implementations and consumers updated accordingly.

BREAKING CHANGE: ICache methods are now async. All callers must
await cache operations. Example:
  Before: var value = cache.Get(key);
  After:  var value = await cache.GetAsync(key);

Refs: #890
```

### Configuration Schema Change

```
feat(config)!: restructure application configuration

Reorganized configuration schema to group related settings and
improve clarity. Updated all configuration consumers and deployment
manifests.

BREAKING CHANGE: Configuration schema restructured. Required changes:
- "Redis.ConnectionString" moved to "Cache.Redis.ConnectionString"  
- "Azure.ServiceBus" moved to "Integrations.ServiceBus"
- "RMV3.BaseUrl" moved to "Integrations.RMV3.BaseUrl"

Migration script available at scripts/migrate-config.sh

Refs: #901
```

## Multi-Part Features

### Complete Feature Implementation

When implementing a complete feature across multiple commits:

**Step 1: Core logic**
```
feat(payment): add payment processing service

Implements payment service with PaymentGateway integration,
including charge creation, status checking, and refund processing.
No UI integration yet.

Refs: #123
```

**Step 2: API integration**
```
feat(api): add payment endpoints

Added REST endpoints for payment operations:
- POST /api/payments
- GET /api/payments/{id}
- POST /api/payments/{id}/refund

Refs: #123
```

**Step 3: UI integration**
```
feat(portal): add payment UI workflow

Implements payment form, confirmation page, and receipt display
in Drive.Portal. Integrates with payment API endpoints.

Refs: #123
```

**Step 4: Tests**
```
test(payment): add integration tests

Added integration tests covering:
- Successful payment flow
- Payment failure scenarios  
- Refund processing
- Idempotency

Refs: #123
```

**Step 5: Documentation**
```
docs(payment): document payment processing architecture

Updated Arc42 chapter 8 with payment processing flow, added
sequence diagrams, and documented error handling strategy.

Refs: #123
```

## Refactoring Examples

### Extract Module

```
refactor(validation): extract validation to shared module

Moved validation logic from individual controllers to
ClientValidationModule for reuse across DriveAPI and Drive.Portal.

Changes:
- Created ClientValidationModule project
- Moved validation classes and tests
- Updated project references
- No functional changes

Refs: #234
```

### Performance Optimization

```
perf(database): optimize driver license query

Added composite index on (license_number, status, expiry_date)
and refactored query to eliminate table scan. Reduces average
query time from 200ms to 15ms.

Migration: 20260223_add_license_index.sql

Refs: #345
```

### Dependency Update

```
build(deps): upgrade .NET to 8.0.2

Updated .NET runtime and SDK to 8.0.2 for security fixes
and performance improvements. All tests pass, no breaking
changes in codebase.

Refs: #456
```

## CI/CD Examples

### GitHub Actions

```
ci(github): add conventional commit validation

Added commitlint GitHub Action to validate commit messages
on all pull requests. Prevents merging of non-compliant commits.

Configuration:
- Uses @commitlint/config-conventional
- Validates all commits in PR
- Fails PR if any commit invalid

Refs: #567
```

### Azure Pipelines

```
ci(azure): add automated diagram generation

Updated Azure Pipeline to automatically generate PNG diagrams
from Structurizr DSL on every commit to main branch. Generated
diagrams committed back to repository.

Refs: #678
```

### Build Configuration

```
build(docker): optimize container image size

Reduced Drive.Portal Docker image from 850MB to 320MB by:
- Using multi-stage build
- Switching to Alpine base image
- Removing unnecessary runtime dependencies

Refs: #789
```

## Testing Examples

### Adding Tests

```
test(auth): add OAuth2 token validation tests

Added unit tests for OAuth2TokenValidator covering:
- Valid token scenarios
- Expired token handling
- Invalid signature detection
- Missing claims validation

Refs: #890
```

### Fixing Tests

```
test(integration): fix flaky payment test

Payment integration test was failing intermittently due to
timing issue with mock gateway. Added proper async/await
and increased timeout for gateway response.

Fixes: #901
```

## Complex Scenarios

### Feature with Architecture Change and Breaking Change

```
feat(integration)!: implement async RMV3 integration

Replaced synchronous HTTP calls to RMV3 with asynchronous
message-based integration using Azure Service Bus. Improves
reliability and enables retry logic.

Architecture changes:
- Added DataSyncFunction for RMV3 message processing  
- Updated Structurizr DSL model with new container
- Added deployment configuration for Azure Functions
- Updated Arc42 runtime views

BREAKING CHANGE: RMV3Adapter.SyncDriver() removed. Use
RMV3MessagePublisher.PublishDriverSync() instead. Synchronous
sync no longer available; all syncs are now asynchronous.

Refs: ADR-016, #1234
```

### Revert with Explanation

```
revert: feat(cache): implement Redis clustering

This reverts commit a1b2c3d4e5f6789.

Reverted due to Redis cluster connection issues in production
causing intermittent cache failures. Issue tracked for resolution,
will re-implement after connection pooling issue resolved.

Investigation shows connection pool exhaustion under high load.
Requires Redis configuration tuning and connection management
improvements before re-deployment.

Refs: #1345
```

## Style and Formatting

### Code Style Changes

```
style(api): format code with dotnet format

Applied automated code formatting using dotnet format tool
across entire DriveAPI solution. No functional changes.
```

### Linting Fixes

```
style(dsl): fix Structurizr DSL formatting

Applied consistent indentation and line breaks to all DSL files
following Structurizr style guide. No model changes.
```

## Chore Examples

### Dependency Management

```
chore(deps): update npm dependencies

Updated all npm packages to latest compatible versions.
No breaking changes, all tests pass.
```

### Code Cleanup

```
chore: remove unused imports and variables

Cleaned up unused imports across C# projects and removed
dead code identified by code analysis. No functional changes.
```

### Configuration

```
chore(config): update development environment settings

Updated local development configuration for new team members.
Added default values and improved documentation.

Refs: #1456
```

## Multi-Scope Commits

When changes affect multiple scopes:

```
feat(api,portal): implement password reset flow

Added password reset functionality to both API and Portal:
- API: Added /api/auth/reset-password endpoint
- Portal: Added reset password UI and workflow

Both components tested and integrated.

Refs: #1567
```

## Commit Message Anti-Patterns (Don't Do This)

### ❌ Too Vague
```
fix: bug fix
```

### ❌ Too Detailed (belongs in body)
```
fix(auth): fixed the authentication bug where the token validator was not properly checking expiration timestamps causing tokens to be accepted even after expiry which led to security issues
```

### ❌ Wrong Tense
```
feat(cache): added Redis caching
```

### ❌ Wrong Case
```
Feat(Cache): Add Redis Caching.
```

### ❌ Not Imperative
```
feat(cache): adding Redis caching
```

### ❌ Missing Context (breaking change not documented)
```
feat(api): remove old endpoints
```

## Tips for Using in Drive Project

1. **Reference ADRs**: Link related architecture decisions
   ```
   Refs: ADR-015
   ```

2. **Link Issues**: Always reference GitHub issues
   ```
   Fixes: #123
   Closes: #456
   Refs: #789
   ```

3. **Architecture Artifacts**: Be specific about what changed
   ```
   docs(dsl): update container diagram (added NotificationService)
   docs(arc42): chapter 8 - added payment flow sequence
   docs(adr): add ADR-019 for API versioning strategy
   ```

4. **Multi-File Changes**: Group related changes logically
   ```
   feat(auth): implement OAuth2 across API and Portal
   
   - API: OAuth2 token endpoint and middleware
   - Portal: OAuth2 login flow and callback handling
   - Shared: OAuth2 configuration models
   ```

5. **Testing**: Separate test commits from implementation
   ```
   feat(payment): add payment service
   test(payment): add payment service tests
   ```

6. **Documentation**: Update docs in separate commit or with feature
   ```
   feat(payment): add payment service
   docs(arc42): document payment processing flow
   ```
