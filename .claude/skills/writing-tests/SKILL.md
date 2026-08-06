---
name: writing-tests
description: Use when writing new tests or fixing failing tests in this project
---

# Writing Tests

xUnit, in `tests/FitLifePlanner.Tests`.

## Before writing a test
- Find an existing test for similar code and follow its structure, naming, and assertion style.
- Prefer testing behavior (inputs → outputs) over implementation details.

## Running tests
- Run a single test file/case while iterating, not the full suite: `dotnet test tests/FitLifePlanner.Tests --filter "FullyQualifiedName~ClassName.MethodName"`
- Run the full suite only before considering work done: `dotnet test` (see `CLAUDE.md` → Commands)

## When a test fails
- Read the actual failure output before changing anything — don't guess.
- Confirm whether the test or the code is wrong before "fixing" either.

## Common mistakes
- Mocking so much that the test no longer verifies real behavior.
- Asserting on incidental details (log strings, internal ordering) instead of the actual contract.
