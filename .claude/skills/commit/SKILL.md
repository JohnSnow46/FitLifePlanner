---
name: commit
description: Stage and commit the current changes with a descriptive message. Invoke this once reviewer/reviewer-lite gives a ready-to-merge verdict — builder doesn't run git commands on its own.
disable-model-invocation: true
allowed-tools: Bash(git status *), Bash(git diff *), Bash(git add *), Bash(git commit *), Bash(git log *)
---

Commit the current changes:

1. Run `git status` and `git diff` to see what changed.
2. Group related changes; if unrelated changes are mixed together, ask before committing them together.
3. Write a commit message: `<type>: <short imperative summary>`, then a body explaining
   *why* if it's not obvious from the diff. `<type>` is one of `feat`, `fix`, `refactor`,
   `docs`, `test`, `chore` (see `docs/development.md`) — pick the one matching the actual
   change, not just `chore` by default.
4. Stage only the relevant files — never `git add -A` blindly.
5. Commit. Do not push unless asked.
