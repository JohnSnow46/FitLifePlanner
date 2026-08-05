# Development workflow

## Branches

- `main` — release branch. Always deployable/tagged. No direct commits.
- `develop` — integration branch. Feature branches merge here first.
- `feature/<short-description>` — one branch per task, cut from `develop`.

```
feature/*  →  Pull Request  →  Code Review  →  merge into develop  →  release into main
```

`develop` is the repo's default branch on GitHub, so PRs target it unless a release PR is
explicitly targeting `main`.

## Commit convention

[Conventional Commits](https://www.conventionalcommits.org/), type-only (no scopes
required):

| Prefix | Use for |
|---|---|
| `feat:` | New feature or capability |
| `fix:` | Bug fix |
| `refactor:` | Code change that doesn't change behavior |
| `docs:` | Documentation only |
| `test:` | Adding or fixing tests |
| `chore:` | Tooling, deps, config, repo maintenance |

Format: short imperative summary line (`feat: add workout log entity`), optional body
explaining *why* when it's not obvious from the diff. The `commit` skill
(`.claude/skills/commit/SKILL.md`) applies this automatically.

## Pull requests

- One PR per feature branch, targeting `develop`.
- PR description: what changed and why; link the relevant ADR if one exists.
- Release PRs (`develop` → `main`) are cut once a set of merged features is ready to
  ship.

## Local setup

[Fill in during ETAP 1 once the stack is chosen: install steps, environment variables,
how to run the app locally, how to run tests.]

## Testing

[Fill in during ETAP 1: test framework and how to run the suite. See
`.claude/skills/writing-tests/SKILL.md` for the conventions Claude follows once this is
filled in.]
