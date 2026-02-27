# Commit Message Validation Guide

This guide shows how to validate commit messages against the Conventional Commits specification.

## Validation Rules

### 1. Type Validation

**Rule**: Commit must start with a valid type

**Valid Types**:
- `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`

**Examples**:

✅ **Valid**:
```
feat(auth): add OAuth2 support
fix: correct validation bug
docs: update README
```

❌ **Invalid**:
```
feature: add OAuth2 support       # Wrong type name
Add OAuth2 support                # Missing type
```

### 2. Scope Validation

**Rule**: Scope must be lowercase and in parentheses (if provided)

**Examples**:

✅ **Valid**:
```
feat(auth): add OAuth2 support
feat(drive-api): optimize query
fix(payment/gateway): handle timeout
```

❌ **Invalid**:
```
feat(Auth): add OAuth2 support         # Scope not lowercase
feat [auth]: add OAuth2 support        # Wrong brackets
feat auth: add OAuth2 support          # Missing parentheses
```

### 3. Description Validation

**Rule**: Description must be:
- Lowercase
- Present tense / imperative mood
- Not end with a period
- Start with lowercase letter
- One space after colon

**Examples**:

✅ **Valid**:
```
feat(cache): add Redis support
fix(auth): prevent token leak
docs: update installation guide
```

❌ **Invalid**:
```
feat(cache): Add Redis support.        # Capital letter, period
feat(cache):add Redis support          # Missing space after colon
feat(cache): added Redis support       # Past tense
feat(cache): Redis support added       # Not imperative
```

### 4. Breaking Change Validation

**Rule**: Breaking changes must be documented with:
- `!` after type/scope, OR
- `BREAKING CHANGE:` in footer, OR
- Both

**Examples**:

✅ **Valid**:
```
feat!: remove v1 endpoints

feat(api)!: remove v1 endpoints

feat(api): remove v1 endpoints

BREAKING CHANGE: v1 endpoints removed
```

❌ **Invalid**:
```
feat: remove v1 endpoints
# Breaking change but not marked

BREAKING CHANGE: v1 endpoints removed
# Missing type prefix
```

### 5. Body Validation

**Rule**: If present, body must:
- Be separated from description by blank line
- Wrap at 72 characters (recommended)
- Provide context and rationale

**Examples**:

✅ **Valid**:
```
feat(cache): add Redis support

Implements Redis-based caching to improve API response times.
Uses connection pooling and 5-minute TTL for hot data.
```

❌ **Invalid**:
```
feat(cache): add Redis support
Implements Redis-based caching...
# Missing blank line before body
```

### 6. Footer Validation

**Rule**: Footer must use correct syntax:
- `Refs: #123` for references
- `Fixes: #123` for bug fixes  
- `Closes: #123` for closing issues
- `BREAKING CHANGE: description` for breaking changes
- `Co-authored-by: Name <email>` for co-authors

**Examples**:

✅ **Valid**:
```
feat(cache): add Redis support

Refs: #123
Fixes: #456
Closes: #789

BREAKING CHANGE: Old cache interface removed

Co-authored-by: John Doe <john@example.com>
```

❌ **Invalid**:
```
Refs #123                    # Missing colon
Fixed: #456                  # Wrong keyword (should be "Fixes")
Breaking Change: removed     # Wrong format (needs colon, uppercase)
```

## Validation Checklist

Use this checklist to validate commit messages:

- [ ] **Type**: Valid type from allowed list
- [ ] **Scope**: Lowercase, in parentheses (if present)
- [ ] **Separator**: Colon with space after type/scope
- [ ] **Description**: Lowercase, imperative, no period
- [ ] **Length**: Description ≤ 50 characters (recommended)
- [ ] **Body**: Blank line before body (if present)
- [ ] **Line Length**: Body lines ≤ 72 characters (recommended)
- [ ] **Breaking Change**: Marked with `!` or `BREAKING CHANGE:` (if applicable)
- [ ] **Footer**: Correct syntax for references (if present)
- [ ] **Overall**: Clear, concise, follows specification

## Validation Scenarios

### Scenario 1: Simple Feature Commit

**Message**:
```
feat(auth): add OAuth2 support
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `auth` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `add OAuth2 support` (lowercase, imperative, no period)
- ✅ Length: 23 characters
- ✅ **VALID**

### Scenario 2: Bug Fix with Details

**Message**:
```
fix(payment): handle gateway timeout

The payment gateway occasionally times out under high load.
Added retry logic with exponential backoff and circuit breaker.

Fixes: #456
```

**Validation**:
- ✅ Type: `fix` (valid)
- ✅ Scope: `payment` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `handle gateway timeout` (lowercase, imperative)
- ✅ Body: Separated by blank line, provides context
- ✅ Footer: Correct `Fixes:` syntax
- ✅ **VALID**

### Scenario 3: Breaking Change

**Message**:
```
feat(api)!: remove v1 endpoints

All v1 endpoints have been removed as planned.

BREAKING CHANGE: v1 API endpoints removed. Clients must migrate
to v2 endpoints.

Closes: #789
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `api` (lowercase, in parentheses)
- ✅ Breaking: Marked with `!` after scope
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `remove v1 endpoints` (lowercase, imperative)
- ✅ Body: Separated by blank line
- ✅ Footer: `BREAKING CHANGE:` documented, correct `Closes:` syntax
- ✅ **VALID**

### Scenario 4: Invalid - Wrong Capitalization

**Message**:
```
feat(API): Add OAuth2 Support
```

**Validation**:
- ✅ Type: `feat` (valid)
- ❌ Scope: `API` (should be lowercase: `api`)
- ✅ Separator: `: ` (colon with space)
- ❌ Description: `Add OAuth2 Support` (should be lowercase)
- ❌ **INVALID**

**Correction**:
```
feat(api): add OAuth2 support
```

### Scenario 5: Invalid - Past Tense

**Message**:
```
feat(cache): added Redis support
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `cache` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ❌ Description: `added Redis support` (should be imperative: "add")
- ❌ **INVALID**

**Correction**:
```
feat(cache): add Redis support
```

### Scenario 6: Invalid - Missing Blank Line

**Message**:
```
feat(cache): add Redis support
Implements Redis-based caching for improved performance.
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `cache` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `add Redis support` (lowercase, imperative)
- ❌ Body: Missing blank line separator
- ❌ **INVALID**

**Correction**:
```
feat(cache): add Redis support

Implements Redis-based caching for improved performance.
```

### Scenario 7: Invalid - Breaking Change Not Documented

**Message**:
```
feat(api): remove v1 endpoints
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `api` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `remove v1 endpoints` (lowercase, imperative)
- ❌ Breaking: Removing endpoints is a breaking change but not marked
- ❌ **INVALID**

**Correction**:
```
feat(api)!: remove v1 endpoints

BREAKING CHANGE: v1 API endpoints removed. Clients must migrate
to v2 endpoints.
```

### Scenario 8: Invalid - Wrong Type

**Message**:
```
feature(auth): add OAuth2 support
```

**Validation**:
- ❌ Type: `feature` (should be `feat`)
- ✅ Scope: `auth` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `add OAuth2 support` (lowercase, imperative)
- ❌ **INVALID**

**Correction**:
```
feat(auth): add OAuth2 support
```

### Scenario 9: Valid - Multiple Footers

**Message**:
```
feat(payment): add refund processing

Implements refund API endpoint and processing logic.
Integrates with payment gateway refund API.

Refs: #123, #124
Closes: #125
Co-authored-by: Jane Smith <jane@example.com>
```

**Validation**:
- ✅ Type: `feat` (valid)
- ✅ Scope: `payment` (lowercase, in parentheses)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `add refund processing` (lowercase, imperative)
- ✅ Body: Separated by blank line
- ✅ Footer: Multiple footers with correct syntax
- ✅ **VALID**

### Scenario 10: Valid - No Scope

**Message**:
```
docs: update contributing guidelines
```

**Validation**:
- ✅ Type: `docs` (valid)
- ✅ Scope: (none - optional)
- ✅ Separator: `: ` (colon with space)
- ✅ Description: `update contributing guidelines` (lowercase, imperative)
- ✅ **VALID**

## Common Validation Errors

### Error: Capital Letter in Description

**Problem**:
```
feat(cache): Add Redis support
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Period at End of Description

**Problem**:
```
feat(cache): add Redis support.
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Past Tense

**Problem**:
```
feat(cache): added Redis support
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Capital Scope

**Problem**:
```
feat(Cache): add Redis support
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Missing Space After Colon

**Problem**:
```
feat(cache):add Redis support
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Wrong Type Name

**Problem**:
```
feature(cache): add Redis support
```

**Fix**:
```
feat(cache): add Redis support
```

### Error: Missing Blank Line Before Body

**Problem**:
```
feat(cache): add Redis support
This implements Redis caching.
```

**Fix**:
```
feat(cache): add Redis support

This implements Redis caching.
```

### Error: Breaking Change Not Marked

**Problem**:
```
feat(api): remove v1 endpoints
```

**Fix**:
```
feat(api)!: remove v1 endpoints

BREAKING CHANGE: v1 API endpoints removed.
```

## Automated Validation

### Using Commitlint

Install and configure commitlint:

```bash
npm install --save-dev @commitlint/cli @commitlint/config-conventional
```

**commitlint.config.js**:
```javascript
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat',
        'fix',
        'docs',
        'style',
        'refactor',
        'perf',
        'test',
        'build',
        'ci',
        'chore',
        'revert',
      ],
    ],
    'scope-case': [2, 'always', 'lower-case'],
    'subject-case': [2, 'always', 'lower-case'],
    'subject-empty': [2, 'never'],
    'subject-full-stop': [2, 'never', '.'],
    'header-max-length': [2, 'always', 72],
  },
};
```

**Validate a message**:
```bash
echo "feat(cache): add Redis support" | commitlint
```

### Using Pre-commit Hook

**`.git/hooks/commit-msg`**:
```bash
#!/bin/sh
npx commitlint --edit $1
```

Make executable:
```bash
chmod +x .git/hooks/commit-msg
```

### Using GitHub Action

**.github/workflows/commitlint.yml**:
```yaml
name: Lint Commit Messages

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  commitlint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - uses: wagoid/commitlint-github-action@v5
        with:
          configFile: commitlint.config.js
```

## Manual Validation Checklist

When reviewing commit messages manually:

1. **Read the first line**
   - Does it start with a valid type?
   - Is scope lowercase and in parentheses (if present)?
   - Is there a colon with space after type/scope?
   - Is description lowercase?
   - Does description use imperative mood?
   - Is description under 50 characters?
   - Does description avoid ending with period?

2. **Check for breaking changes**
   - If removing features: Is `!` present or `BREAKING CHANGE:` in footer?
   - If changing APIs: Is breaking change documented?
   - If changing configuration: Is migration path provided?

3. **Review body (if present)**
   - Is there a blank line after description?
   - Does body explain why (not what)?
   - Does body provide context?
   - Are lines wrapped at ~72 characters?

4. **Check footer (if present)**
   - Are issue references correct (`Refs:`, `Fixes:`, `Closes:`)?
   - Is breaking change documented?
   - Are co-authors listed properly?

5. **Overall quality**
   - Is commit message clear and understandable?
   - Does it follow project conventions?
   - Would it make sense in git log?

## Using Copilot for Validation

### Validate a Message

```
@workspace Validate this commit message:
"feat(cache): add Redis support"
```

### Validate Current Branch Commits

```
@workspace Validate all commits in this branch against Conventional Commits
```

### Get Suggestions for Improvement

```
@workspace How can I improve this commit message?
"Added new caching feature"
```

### Check Before Committing

```
@workspace Analyze my staged changes and suggest a valid conventional commit message
```

## Drive-Specific Validation Considerations

When validating commits for the Drive architecture repository:

1. **Scope Validation**
   - Use lowercase container names: `driveapi`, `drive.portal`, `rmv3adapter`
   - Use lowercase architecture artifact names: `dsl`, `arc42`, `adr`
   - Use lowercase cross-cutting concerns: `docs`, `tests`, `ci`

2. **Type Selection**
   - DSL changes: Usually `docs(dsl):`
   - Arc42 changes: Usually `docs(arc42):`
   - ADR creation: Usually `docs(adr):`
   - Code implementation: `feat:`, `fix:`, `refactor:`

3. **References**
   - Link to ADRs: `Refs: ADR-015`
   - Link to issues: `Refs: #123`
   - Link to PRs: `Refs: #PR456`

4. **Breaking Changes**
   - Architecture changes that affect implementation: Mark as breaking
   - DSL changes that invalidate diagrams: Consider as breaking
   - API removals or changes: Always mark as breaking

## Resources

- **Conventional Commits**: https://www.conventionalcommits.org/
- **Commitlint**: https://commitlint.js.org/
- **Git Commit Guidelines**: https://git-scm.com/book/en/v2/Distributed-Git-Contributing-to-a-Project
- **Drive Git Usage**: `instructions/git-usage.md`

## Getting Help

Ask Copilot:

```
@workspace Validate my commit message against Conventional Commits
@workspace What's wrong with this commit message: "..."
@workspace How should I write a commit for [your changes]
```

Or see the [main skill documentation](../SKILL.md) for comprehensive guidance.
