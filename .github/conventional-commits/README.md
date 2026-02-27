# Conventional Commits Skill

GitHub Copilot skill for creating and validating git commit messages following the [Conventional Commits specification](https://www.conventionalcommits.org/en/v1.0.0/).

## Quick Start

### Create a Commit Message

```
Create a conventional commit message for my staged changes
```

### Analyze Changes

```
@workspace Analyze my staged changes and suggest a conventional commit type and message
```

### Validate Message

```
Validate this commit message: "feat(api): add caching layer"
```

## Commit Message Format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

## Quick Reference

### Types

| Type | Description | Version Impact |
|------|-------------|----------------|
| **feat** | New feature | MINOR (0.x.0) |
| **fix** | Bug fix | PATCH (0.0.x) |
| **docs** | Documentation only | - |
| **style** | Code style (formatting, semicolons) | - |
| **refactor** | Code restructuring (no behavior change) | - |
| **perf** | Performance improvement | PATCH (0.0.x) |
| **test** | Adding/updating tests | - |
| **build** | Build system or dependencies | - |
| **ci** | CI configuration | - |
| **chore** | Maintenance tasks | - |
| **revert** | Revert previous commit | - |

### Examples

#### Simple Feature
```
feat(cache): add Redis distributed caching
```

#### Bug Fix with Details
```
fix(auth): prevent token expiration race condition

The token refresh logic had a race condition where concurrent
requests could cause authentication failures. Added locking
mechanism to ensure only one refresh happens at a time.

Fixes: #456
```

#### Breaking Change
```
feat(api)!: remove legacy v1 endpoints

BREAKING CHANGE: All v1 API endpoints removed. Clients must
migrate to v2 API. See migration guide for details.

Refs: #789
```

#### Documentation
```
docs(arc42): add chapter 8 runtime views
```

## Drive-Specific Scopes

Common scopes for this repository:

### Containers
- `DriveAPI`, `Drive.Portal`, `RMV3Adapter`, `DataSync`, `Notification`

### Architecture
- `dsl` - Structurizr DSL models
- `arc42` - Arc42 documentation  
- `adr` - Decision records
- `diagrams` - Architecture diagrams

### Cross-Cutting
- `docs`, `tests`, `infra`, `ci`, `deps`

## Best Practices

1. **Use imperative mood**: "add feature" not "added feature"
2. **Keep description short**: 50 characters or less
3. **Lowercase description**: "add cache" not "Add cache"
4. **No period at end**: "add cache" not "add cache."
5. **Add context in body**: Explain why and impact
6. **Link issues**: Use `Refs: #123` or `Fixes: #456`
7. **Document breaking changes**: Always include `BREAKING CHANGE:` footer

## Validation

The skill validates:
- ✅ Valid type prefix
- ✅ Proper scope format (lowercase, in parentheses)
- ✅ Colon and space after type/scope
- ✅ Lowercase description
- ✅ No period at end of description
- ✅ Imperative mood
- ✅ Proper blank line before body
- ✅ Correct footer syntax

## Common Mistakes

### ❌ Wrong
```
Added new feature          # Missing type, past tense
feat: Added feature.       # Past tense, period
feat(API): add feature     # Scope not lowercase
BREAKING CHANGE: removed   # Missing type
```

### ✅ Correct
```
feat: add new feature
feat(api): add feature
feat(api)!: remove old endpoint
```

## Automation

### Commitlint Configuration

```json
{
  "extends": ["@commitlint/config-conventional"]
}
```

### Pre-commit Hook

```bash
#!/bin/sh
npx commitlint --edit $1
```

### GitHub Action

```yaml
- uses: wagoid/commitlint-github-action@v5
```

## Integration with Drive Workflow

### Feature Development
```
feat(auth): add OAuth2 configuration
feat(auth): implement token endpoint  
test(auth): add OAuth2 integration tests
docs(auth): update authentication guide
```

### Architecture Updates
```
docs(dsl): add new container to system model
docs(arc42): document OAuth2 integration
docs(adr): add ADR-019 for OAuth2 decision
```

### Bug Fixes
```
fix(api): handle timeout in payment processing

Added retry logic with exponential backoff for payment
gateway timeouts. Implements circuit breaker pattern.

Fixes: #456
Refs: ADR-018
```

## Resources

- **Specification**: https://www.conventionalcommits.org/
- **Commitlint**: https://commitlint.js.org/
- **Semantic Versioning**: https://semver.org/
- **Full Skill Documentation**: [SKILL.md](SKILL.md)

## Support

Questions? Ask Copilot:

```
@workspace How do I write a conventional commit for [your changes]?
```

Or see the [full skill documentation](SKILL.md) for comprehensive guidance.
