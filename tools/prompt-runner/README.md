# prompt-runner

Runs a queue of prompts through Claude Code, one at a time, each in its own **fully
interactive session** — you watch it work, answer permission prompts, and can chat back
or interrupt, exactly like you would if you'd opened the window and pasted the prompt
yourself. When you exit a session, the runner automatically starts the next queued
prompt in a new session. Automates the "open new window, paste, wait" cycle — not the
supervision.

## Opening Git Bash

You need a Git Bash terminal open in the repo root before "Usage" below works.

- **In VS Code (recommended):** open a terminal (`` Ctrl+` ``), click the small `v`
  dropdown next to the `+` in the terminal panel's top-right corner, pick **Git Bash**
  from the list. If it's not listed, `Ctrl+Shift+P` → "Terminal: Select Default Profile"
  → Git Bash → open a new terminal.
- **Standalone:** right-click the repo folder in Windows Explorer → "Git Bash Here". Or
  Start menu → "Git Bash" → then `cd /c/Users/<you>/Desktop/TrainingApp`.
- **Sanity check** you're in Git Bash, not WSL or PowerShell: run `pwd` — it must print
  `/c/...`, not `/mnt/c/...` (that's WSL) or `C:\...` (that's PowerShell).
- **First time only** in a given terminal: run `claude` with no arguments, accept the
  workspace trust dialog if one appears, then exit (`Ctrl+D`) — this avoids the
  "workspace not trusted" issue explained below.

## Usage

1. Paste your prompts into `queue.txt`, in the order you want them to run, separated by
   a line containing exactly `---`:

   ```
   First prompt text,
   can span multiple lines.
   ---
   Second prompt text.
   ---
   Third prompt text.
   ```

2. Run the script from **Git Bash**, not WSL:

   ```bash
   bash tools/prompt-runner/run.sh
   ```

   WSL sees this project under a different path (`/mnt/c/...` instead of `C:/...`), so
   Claude Code treats it as an untrusted, separate workspace and ignores
   `permissions.allow` — every tool call then needs approval that a nested session may
   not be able to surface. Git Bash keeps the same `C:/...` path identity as the
   trusted VS Code session, so this doesn't happen there.

3. Each prompt opens as a normal interactive `claude` session in your terminal. Work in
   it as you normally would (approve tools, reply, correct it). When you're satisfied,
   type **`/exit`** to close it — the runner then starts the next prompt automatically.
   Prefer `/exit` over `Ctrl+D`: pressing Ctrl+D more than once (e.g. out of habit, or
   if the first press didn't seem to register) can leave a stray EOF sitting in the
   terminal that gets delivered to the *next* prompt's session instead, closing it
   instantly before it does any work. `/exit` doesn't have that failure mode.

4. The original queue is archived to `logs/queue_<timestamp>.txt` and `queue.txt` is
   cleared before the run starts, so you can paste the next batch without re-running old
   prompts.

## Notes

- No permission bypass is needed — each session is fully interactive, so you approve
  tool use live, same as today. `.claude/settings.json`'s `permissions.allow` still
  pre-approves the routine stuff on that list (read-only tools, common `dotnet`
  commands, file writes/edits) so you're only asked about things that aren't already
  trusted.
- Prompts run strictly in order; the runner waits for one session to exit before
  starting the next.
- To stop the whole batch partway through, exit the current session and then `Ctrl+C`
  the script before the next one starts (the remaining prompts stay in the archived
  queue file if you need to re-paste them).
- `prompt-engineer` (`.claude/agents/prompt-engineer.md`) writes every batch it
  generates straight to `queue.txt` automatically (in addition to showing it in its
  response) — no manual copy-paste needed for prompts that come from that agent. Each
  new batch **replaces** whatever was in `queue.txt` — it's a single working batch, not
  an accumulating backlog.
- `history.md` (tracked in git, unlike the gitignored `logs/`) is the permanent record:
  `prompt-engineer` appends every batch there too, so nothing is lost when the next
  batch overwrites `queue.txt` or `run.sh` clears it.
