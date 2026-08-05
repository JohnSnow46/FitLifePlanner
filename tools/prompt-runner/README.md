# prompt-runner

Runs a queue of prompts through Claude Code's headless mode (`claude -p`), one at a
time, each in a fresh session — waits for one to finish before starting the next.
Replaces manually pasting each prompt into a new window and waiting.

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

2. Run the script (git bash / WSL / any POSIX shell with the `claude` CLI on PATH):

   ```bash
   bash tools/prompt-runner/run.sh
   ```

3. Each prompt's full output is written to `logs/output_<timestamp>_<n>.md`. The
   original queue is archived to `logs/queue_<timestamp>.txt` and `queue.txt` is
   cleared so you can paste the next batch without re-running old prompts.

## Notes

- Prompts run strictly in order; the script waits for each `claude -p` call to exit
  before starting the next one.
- If a prompt fails or produces output you don't like, check its `output_*.md` file —
  the rest of the batch still runs.
- `prompt-engineer` (`.claude/agents/prompt-engineer.md`) wraps its generated prompt in
  `---` delimiters by default, so its output can be pasted straight into `queue.txt`.
