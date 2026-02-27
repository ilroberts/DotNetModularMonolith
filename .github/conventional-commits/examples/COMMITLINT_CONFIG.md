# Commitlint Configuration Example

Example commitlint configuration for enforcing Conventional Commits in the Drive repository.

## Installation

```bash
npm install --save-dev @commitlint/cli @commitlint/config-conventional
```

## Configuration File

Create `commitlint.config.js` in repository root:

```javascript
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    // Type enum - allowed commit types
    'type-enum': [
      2,
      'always',
      [
        'feat',      // New feature
        'fix',       // Bug fix
        'docs',      // Documentation only
        'style',     // Code style (formatting, semicolons)
        'refactor',  // Code restructuring (no behavior change)
        'perf',      // Performance improvement
        'test',      // Adding/updating tests
        'build',     // Build system or dependencies
        'ci',        // CI configuration
        'chore',     // Maintenance tasks
        'revert',    // Revert previous commit
      ],
    ],
    
    // Scope enum - Drive-specific scopes (optional)
    'scope-enum': [
      1, // warning
      'always',
      [
        // Containers
        'driveapi',
        'drive.portal',
        'rmv3adapter',
        'datasync',
        'notification',
        
        // Architecture artifacts
        'dsl',
        'arc42',
        'adr',
        'diagrams',
        
        // Cross-cutting
        'docs',
        'tests',
        'infra',
        'ci',
        'deps',
        
        // Components (examples)
        'auth',
        'cache',
        'validation',
        'payment',
        'workflow',
      ],
    ],
    
    // Scope case - must be lowercase
    'scope-case': [2, 'always', 'lower-case'],
    
    // Subject (description) case - must be lowercase
    'subject-case': [2, 'always', 'lower-case'],
    
    // Subject must not be empty
    'subject-empty': [2, 'never'],
    
    // Subject must not end with period
    'subject-full-stop': [2, 'never', '.'],
    
    // Header (first line) max length
    'header-max-length': [2, 'always', 72],
    
    // Body leading blank - must have blank line before body
    'body-leading-blank': [2, 'always'],
    
    // Footer leading blank - must have blank line before footer
    'footer-leading-blank': [2, 'always'],
  },
};
```

## Simplified Configuration

For less strict validation:

```javascript
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'scope-enum': [0], // Disable scope validation
    'subject-case': [1, 'always', 'lower-case'], // Warning only
  },
};
```

## Pre-commit Hook

Add to `.git/hooks/commit-msg`:

```bash
#!/bin/sh

# Run commitlint on commit message
npx commitlint --edit $1
```

Make it executable:

```bash
chmod +x .git/hooks/commit-msg
```

## GitHub Action

Create `.github/workflows/commitlint.yml`:

```yaml
name: Lint Commit Messages

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  commitlint:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v3
        with:
          fetch-depth: 0
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm install --save-dev @commitlint/cli @commitlint/config-conventional
      
      - name: Validate commit messages
        uses: wagoid/commitlint-github-action@v5
        with:
          configFile: commitlint.config.js
```

## Testing Configuration

Test a commit message:

```bash
echo "feat(cache): add Redis support" | npx commitlint
```

Test with invalid message:

```bash
echo "Added new feature" | npx commitlint
# Should fail validation
```

## Custom Rules

### Require Issue Reference

```javascript
rules: {
  'footer-max-line-length': [0], // Disable for long URLs
  'references-empty': [2, 'never'], // Require issue reference
}
```

### Restrict Scopes to Architecture Artifacts

For a documentation-focused repository:

```javascript
rules: {
  'scope-enum': [
    2,
    'always',
    ['dsl', 'arc42', 'adr', 'diagrams', 'docs'],
  ],
}
```

### Allow Uppercase for Specific Cases

```javascript
rules: {
  'subject-case': [
    2,
    'never',
    ['start-case', 'pascal-case', 'upper-case'],
  ],
}
```

## Drive-Specific Configuration

Recommended configuration for Drive architecture repository:

```javascript
module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'type-enum': [
      2,
      'always',
      [
        'feat', 'fix', 'docs', 'style', 'refactor',
        'perf', 'test', 'build', 'ci', 'chore', 'revert',
      ],
    ],
    'scope-case': [2, 'always', 'lower-case'],
    'subject-case': [2, 'always', 'lower-case'],
    'subject-empty': [2, 'never'],
    'subject-full-stop': [2, 'never', '.'],
    'header-max-length': [2, 'always', 72],
    'body-leading-blank': [2, 'always'],
    'body-max-line-length': [1, 'always', 80], // Warning only
  },
};
```

## Validation Levels

- `0` - Disable rule
- `1` - Warning (doesn't fail commit)
- `2` - Error (fails commit)

## Resources

- **Commitlint Documentation**: https://commitlint.js.org/
- **Conventional Commits**: https://www.conventionalcommits.org/
- **Configuration Reference**: https://commitlint.js.org/#/reference-rules
- **Skill Documentation**: `../SKILL.md`

## Troubleshooting

### Hook Not Running

Ensure hook is executable:
```bash
chmod +x .git/hooks/commit-msg
```

### Commitlint Not Found

Install dependencies:
```bash
npm install
```

### Custom Scope Not Recognized

Add to `scope-enum` in configuration:
```javascript
'scope-enum': [2, 'always', ['your-scope']],
```

### Too Strict

Change error level from `2` to `1` (warning) or `0` (disabled):
```javascript
'header-max-length': [1, 'always', 72], // Warning instead of error
```
