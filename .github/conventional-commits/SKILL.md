---
name: conventional-commits
description: Generates and validates git commit messages following the Conventional Commits specification (v1.0.0). Analyzes staged changes to suggest appropriate commit types, scopes, and descriptions. Creates well-structured commit messages with breaking changes, references, and multi-line bodies. Use when creating commits, validating commit messages, or maintaining consistent git history.
license: See repository root
metadata:
  author: Nova Scotia RMV Modernization (Drive) Team
  version: "1.0"
  technology: GitHub Copilot
  updated: "2026-02-23"
compatibility: Requires GitHub Copilot (Claude Sonnet 4.5 or later)
---

# Conventional Commits Skill

This skill generates and validates git commit messages following the [Conventional Commits specification (v1.0.0)](https://www.conventionalcommits.org/en/v1.0.0/). It helps you create clear, consistent, and semantic commit messages that support automated changelog generation, semantic versioning, and clear project history.

## Purpose

The Conventional Commits skill helps you:
- Create commit messages following Conventional Commits specification
- Analyze staged changes to suggest appropriate commit types and scopes
- Generate multi-line commit messages with body and footer sections
- Document breaking changes properly
- Link commits to issues and pull requests
- Validate existing commit messages against the specification
- Maintain consistent commit history across team members
- Support automated tooling (changelog generation, semantic versioning)

## When to Use This Skill

Use this skill when you need to:
- Create a commit message following Conventional Commits format
- Get suggestions for commit type based on changed files
- Document breaking changes in your commits
- Link commits to GitHub issues or pull requests
- Validate commit messages before pushing
- Enforce commit message standards in your project
- Generate comprehensive commit messages with context
- Teach team members about Conventional Commits

## Prerequisites

Before using this skill, ensure:
1. GitHub Copilot is active in VS Code
2. You have staged changes ready to commit (`git add`)
3. You understand the changes you're committing
4. You know if your changes introduce breaking changes
5. You have issue/PR numbers if creating references

## Conventional Commits Specification

### Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Type

**MUST** be one of the following:

- **feat**: A new feature (correlates with MINOR in Semantic Versioning)
- **fix**: A bug fix (correlates with PATCH in Semantic Versioning)
- **docs**: Documentation only changes
- **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing tests or correcting existing tests
- **build**: Changes that affect the build system or external dependencies (example scopes: npm, maven, gradle)
- **ci**: Changes to CI configuration files and scripts (example scopes: GitHub Actions, Azure Pipelines)
- **chore**: Other changes that don't modify src or test files
- **revert**: Reverts a previous commit

### Scope

**OPTIONAL** contextual information about what part of the codebase is affected:

- Module name (e.g., `feat(parser):`)
- Component name (e.g., `fix(DriveAPI):`)
- File or directory (e.g., `docs(README):`)
- Feature area (e.g., `perf(database):`)
- Layer (e.g., `refactor(ui):`)

### Description

- **MUST** be in lowercase
- **MUST** be a short summary of the code change
- **MUST NOT** end with a period
- **SHOULD** be written in imperative mood (e.g., "add" not "added" or "adds")
- **SHOULD** be 50 characters or less

### Body

**OPTIONAL** longer explanation providing:
- Motivation for the change
- Contrast with previous behavior
- Implementation details
- Impact on other parts of the system

**Format:**
- Separate from description by one blank line
- Can be multiple paragraphs
- Wrap at 72 characters per line

### Footer

**OPTIONAL** metadata including:

**Breaking Changes:**
```
BREAKING CHANGE: <description>
```
or using `!` after type/scope:
```
feat!: <description>
feat(scope)!: <description>
```

**Issue References:**
```
Refs: #123
Fixes: #456
Closes: #789
```

**Co-authored-by:**
```
Co-authored-by: Name <email@example.com>
```

## How to Use

### Basic Commit Message

When you have staged changes and want a commit message:

```
Create a conventional commit message for my staged changes
```

### Analyze and Suggest

Let Copilot analyze your changes and suggest the message:

```
@workspace Analyze my staged changes and suggest a conventional commit message
```

### Specific Type Request

If you know the type of change:

```
Generate a feat commit message for adding Redis caching to the API
```

```
Create a fix commit for resolving the authentication timeout issue
```

### Breaking Change

When introducing breaking changes:

```
Generate a commit message for removing the legacy authentication endpoint (breaking change)
```

### With Body and Footer

For comprehensive commits:

```
Create a conventional commit with body and footer for:
- Type: refactor
- Scope: DriveAPI
- Description: restructure authentication middleware
- Body: Explain why we moved from inline auth to middleware pipeline
- Footer: Refs #342
```

### Validate Existing Message

To check if a commit message follows the specification:

```
Validate this commit message:
"feat(auth): add OAuth2 support"
```

## Examples

### Simple Feature Addition

```
feat(cache): add Redis distributed caching

Implements Redis-based caching layer to improve API response times
for frequently accessed driver license data. Uses StackExchange.Redis
client with connection pooling.

Refs: #234
```

### Bug Fix

```
fix(validation): correct postal code validation regex

The previous regex rejected valid postal codes with lowercase letters.
Updated to accept both upper and lowercase, with normalization to
uppercase before storage.

Fixes: #456
```

### Breaking Change (! notation)

```
feat(api)!: remove deprecated /v1/licenses endpoint

The /v1/licenses endpoint has been fully replaced by /v2/driver-licenses
and is no longer supported. All clients must migrate to the v2 endpoint.

BREAKING CHANGE: /v1/licenses endpoint removed. Use /v2/driver-licenses instead.

Closes: #789
```

### Documentation Update

```
docs(arc42): update chapter 9 with new ADRs

Added ADR-015 through ADR-018 covering caching strategy, authentication
decisions, and deployment model choices.

Refs: #123
```

### Refactoring

```
refactor(components): extract validation logic to shared module

Moved common validation rules from individual components to
ClientValidationModule for reuse across DriveAPI and Drive.Portal.
No functional changes.
```

### Performance Improvement

```
perf(database): optimize license lookup query

Added composite index on (license_number, status) and refactored query
to avoid table scan. Reduces average lookup time from 200ms to 15ms.

Refs: #567
```

### Revert

```
revert: feat(cache): add Redis distributed caching

This reverts commit a1b2c3d4e5f6.

Reason: Redis connection pooling issue causing timeouts in production.
Will revisit after investigating connection management.

Refs: #890
```

### Build/CI Changes

```
build(deps): upgrade Structurizr CLI to 2024.03.03

Updates Structurizr CLI for latest DSL syntax support and bug fixes.
Regenerated all diagrams with new version.
```

```
ci(github-actions): add conventional commit validation

Added commitlint action to enforce Conventional Commits specification
on all pull requests.

Refs: #345
```

## Scope Suggestions for Drive Architecture

Based on the Drive system architecture, common scopes include:

### Containers
- `DriveAPI` - API Gateway container
- `Drive.Portal` - Web Portal container  
- `RMV3Adapter` - Legacy system adapter
- `DataSync` - Data synchronization components
- `Notification` - Notification services

### Components (use with specific containers)
- `auth` - Authentication/authorization
- `cache` - Caching layer
- `validation` - Validation logic
- `workflow` - Business workflow
- `integration` - External integrations

### Cross-Cutting
- `docs` - Documentation
- `tests` - Test code
- `infra` - Infrastructure as code
- `deploy` - Deployment configuration
- `ci` - CI/CD pipelines
- `deps` - Dependencies

### Architecture Artifacts
- `dsl` - Structurizr DSL models
- `arc42` - Arc42 documentation
- `adr` - Architecture Decision Records
- `diagrams` - Architecture diagrams

## Advanced Usage

### Interactive Commit Creation

For step-by-step commit message creation:

```
Help me create a conventional commit interactively
```

Copilot will ask you:
1. What type of change is this?
2. What scope (if any)?
3. Brief description?
4. Need a body?
5. Any breaking changes?
6. Issue references?

### Multiple Changes in One Commit

When committing related changes:

```
Create a conventional commit for these changes:
- Added OAuth2 authentication
- Updated user model
- Added integration tests
All related to authentication feature
```

Result:
```
feat(auth): implement OAuth2 authentication flow

- Added OAuth2 provider configuration
- Extended user model with OAuth2 fields
- Implemented token refresh logic
- Added integration tests for OAuth2 flow

Refs: #234
```

### Validate PR Commits

Check all commits in a PR:

```
@workspace Validate all commits in this branch against Conventional Commits spec
```

## Best Practices

### Writing Effective Commit Messages

1. **Be Specific**: Use appropriate type and scope
2. **Use Imperative Mood**: "add feature" not "added feature"
3. **Keep Description Short**: 50 characters or less
4. **Add Context in Body**: Explain why, not what (code shows what)
5. **Document Breaking Changes**: Always use BREAKING CHANGE footer
6. **Link Issues**: Reference related issues/PRs in footer
7. **One Concern Per Commit**: Split unrelated changes into separate commits

### Type Selection Guidelines

- **feat**: Adds new functionality users will notice
- **fix**: Resolves incorrect behavior users experienced
- **refactor**: Changes code structure without changing behavior
- **perf**: Improves performance measurably
- **docs**: Only touches documentation files
- **test**: Only touches test files
- **build/ci**: Only affects build or CI configuration
- **chore**: Maintenance tasks (update deps, cleanup)

### When to Use Breaking Changes

Use `!` or `BREAKING CHANGE:` when:
- Removing public APIs or endpoints
- Changing API contracts (request/response formats)
- Renaming configuration options
- Changing default behavior that users rely on
- Removing or renaming CLI arguments
- Migrating to incompatible versions (e.g., major dependency upgrades)

**Don't use for:**
- Internal refactoring (even if significant)
- Bug fixes (even if behavior changes)
- Adding new optional features

## Common Pitfalls

### ❌ Incorrect

```
Added new caching feature
```
- Missing type prefix
- Past tense instead of imperative
- Not lowercase

```
feat: Added caching.
```
- Past tense instead of imperative
- Period at end of description

```
feat(Cache): Add Redis caching
```
- Scope should be lowercase

```
BREAKING CHANGE: remove old API
```
- Missing type prefix
- Breaking change should be in footer or use `!` notation

### ✅ Correct

```
feat(cache): add Redis caching
```

```
feat(cache)!: remove legacy caching implementation

BREAKING CHANGE: LegacyCache class removed. Use RedisCache instead.
```

## Integration with Drive Workflow

### With Branch Strategy

When working with feature branches:

```
# Feature work
feat(auth): add OAuth2 client configuration
feat(auth): implement token endpoint
test(auth): add OAuth2 integration tests
docs(auth): update authentication guide

# Bug discovered during feature work
fix(auth): correct token expiration validation

# Final refactoring
refactor(auth): extract OAuth2 provider to separate class
```

### With Architecture Changes

When updating architecture artifacts:

```
docs(dsl): add OAuth2 service to container model

Added OAuth2AuthService container to Drive system model,
including relationships with DriveAPI and MyNSID integration.
Updated container and deployment views.

Refs: #456
```

```
docs(arc42): document OAuth2 integration in chapter 8

Added runtime view for OAuth2 authentication flow and
updated building block view with new authentication components.

Refs: #456
```

### With ADRs

When implementing decisions:

```
feat(cache)!: implement Redis caching per ADR-015

Implements distributed caching strategy documented in ADR-015.
Replaces in-memory caching with Redis cluster for improved
scalability and cache coherency across API instances.

BREAKING CHANGE: ICache interface changed to async methods.
All cache consumers must await cache operations.

Refs: ADR-015, #234
```

## Automation and Tooling

### Commitlint Integration

The Conventional Commits format enables automated validation with tools like commitlint.

**Basic commitlint configuration:**

```json
{
  "extends": ["@commitlint/config-conventional"],
  "rules": {
    "type-enum": [
      2,
      "always",
      [
        "feat",
        "fix",
        "docs",
        "style",
        "refactor",
        "perf",
        "test",
        "build",
        "ci",
        "chore",
        "revert"
      ]
    ],
    "scope-case": [2, "always", "lower-case"],
    "subject-case": [2, "always", "lower-case"],
    "subject-full-stop": [2, "never", "."]
  }
}
```

### Changelog Generation

Conventional Commits enable automated changelog generation:

```bash
# Using standard-version
npx standard-version

# Using semantic-release
npx semantic-release
```

### Semantic Versioning

Commit types map to version bumps:
- `feat`: MINOR version bump (0.x.0)
- `fix`: PATCH version bump (0.0.x)
- `BREAKING CHANGE` or `!`: MAJOR version bump (x.0.0)

## Validation Rules

The skill validates commits against these rules:

1. ✅ **Type Required**: Must start with valid type
2. ✅ **Scope Format**: Scope must be in parentheses, lowercase
3. ✅ **Colon Required**: Type/scope followed by colon
4. ✅ **Space After Colon**: One space between colon and description
5. ✅ **Description Required**: Must have non-empty description
6. ✅ **Description Case**: Description must be lowercase
7. ✅ **No Period**: Description must not end with period
8. ✅ **Imperative Mood**: Description should use imperative mood
9. ✅ **Blank Line Before Body**: Body must be separated by blank line
10. ✅ **Footer Format**: Footer must use correct syntax

## Examples by Scenario

### Adding New Feature

```
feat(driver): add driver license renewal workflow

Implements multi-step renewal workflow with eligibility checking,
fee calculation, and payment processing. Integrates with RMV3
for legacy data synchronization.

Refs: #123, #124
```

### Fixing Production Bug

```
fix(payment): handle payment gateway timeout correctly

Previous implementation treated timeouts as payment failures,
causing duplicate charge attempts. Now properly handles timeouts
with idempotency keys and payment status polling.

Fixes: #456
Refs: ADR-018
```

### Updating Documentation

```
docs(readme): update development setup instructions

Added prerequisites section, clarified Docker requirements,
and updated environment variable configuration examples.
```

### Improving Performance

```
perf(api): implement response compression

Added gzip compression middleware to API Gateway, reducing
average response size by 60% and improving bandwidth usage.

Refs: #789
```

### Restructuring Code

```
refactor(validation): consolidate validation logic

Moved validation rules from controllers to dedicated validation
services. No functional changes, improves testability and reuse.
```

### Updating Dependencies

```
build(deps): upgrade .NET runtime to 8.0.2

Security and performance improvements. No breaking changes
in our codebase.
```

### CI/CD Changes

```
ci(azure-pipelines): add code coverage reporting

Added coverage collection with Coverlet and reporting to
Azure DevOps. Minimum coverage threshold set to 80%.

Refs: #890
```

## Tips for Team Adoption

1. **Start Simple**: Begin with type and description, add body later
2. **Use Examples**: Share real examples from your project
3. **Automate Validation**: Add commitlint to CI pipeline
4. **Provide Templates**: Create commit message templates in IDE
5. **Code Review**: Check commit messages during PR review
6. **Documentation**: Add Conventional Commits guide to contributing docs
7. **Training**: Run team sessions on commit message best practices
8. **Lead by Example**: Maintainers should consistently follow the format

## Troubleshooting

### "Not sure what type to use"

Ask Copilot to analyze your changes:
```
@workspace What conventional commit type should I use for these changes?
```

### "Scope too broad/narrow"

Use scopes that match your architecture:
- Too broad: `feat(system):`
- Too narrow: `feat(LoginController.cs):`
- Just right: `feat(auth):` or `feat(portal/login):`

### "Too many changes for one commit"

Split into multiple commits:
```
git add -p  # Stage changes interactively
```

Then create separate conventional commits for each logical change.

### "Breaking change but not intentional"

If you accidentally broke compatibility:
```
fix(api)!: correct user endpoint response format

Fixed incorrect JSON structure in user response. Previous format
was invalid according to OpenAPI spec.

BREAKING CHANGE: User response now correctly includes 'id' field
at root level instead of nested in 'data'.

Fixes: #456
```

## References

- **Specification**: [conventionalcommits.org](https://www.conventionalcommits.org/)
- **Commitlint**: [commitlint.js.org](https://commitlint.js.org/)
- **Semantic Versioning**: [semver.org](https://semver.org/)
- **Drive Repository**: `.github/adr-generator/` for related architectural decisions
- **Git Best Practices**: `instructions/git-usage.md`

## Related Skills

- **adr-generator**: Document architectural decisions referenced in commits
- **discussion-notes-generator**: Capture design discussions leading to commits
- **view-consistency-checker**: Validate architectural changes committed to DSL

## Support

For questions about this skill:
1. Review this documentation
2. Check existing commit history for examples
3. Ask Copilot: `@workspace Explain Conventional Commits for this repository`
4. Refer to project's CONTRIBUTING.md (if available)

---

**Version**: 1.0  
**Last Updated**: 2026-02-23  
**Specification**: Conventional Commits 1.0.0  
**Maintainer**: Drive Architecture Team
