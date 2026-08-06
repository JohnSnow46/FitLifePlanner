#!/usr/bin/env bash
# Runs each prompt in queue.txt as a fresh, fully interactive `claude` session,
# one at a time. You watch and interact with each session normally (approve
# tools, chat, interrupt); when you exit that session, the next queued prompt
# starts automatically in a new session.
set -uo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if grep -qi microsoft /proc/version 2>/dev/null; then
  echo "Uwaga: to wyglada na WSL bash (sciezka projektu: $DIR)." >&2
  echo "Jesli sesje claude beda zglaszac 'workspace not trusted' mimo ze w Windows" >&2
  echo "juz go zaakceptowales, to dlatego ze WSL widzi ten projekt pod innym" >&2
  echo "identyfikatorem sciezki (/mnt/c/... zamiast C:/...). Uzyj Git Bash zamiast" >&2
  echo "WSL, albo zaakceptuj trust dialog osobno w ramach WSL." >&2
  echo "" >&2
fi

QUEUE_FILE="$DIR/queue.txt"
LOG_DIR="$DIR/logs"
mkdir -p "$LOG_DIR"

if [ ! -s "$QUEUE_FILE" ]; then
  echo "Queue is empty: $QUEUE_FILE"
  exit 0
fi

TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
ARCHIVE_FILE="$LOG_DIR/queue_${TIMESTAMP}.txt"
cp "$QUEUE_FILE" "$ARCHIVE_FILE"
: > "$QUEUE_FILE"

count=0

run_prompt() {
  local prompt="$1"
  # skip blocks that are empty/whitespace-only
  if [ -z "$(printf '%s' "$prompt" | tr -d '[:space:]')" ]; then
    return
  fi
  count=$((count + 1))
  echo ""
  echo "=================================================="
  echo "  Prompt $count/$total — starting interactive session"
  echo "=================================================="
  claude "$prompt"
  echo ""
  echo "  Prompt $count finished (session exited)."
}

total="$(grep -c -- '^---$' "$ARCHIVE_FILE" 2>/dev/null || true)"
total=$((total + 1))

current=""
while IFS= read -r line <&3 || [ -n "$line" ]; do
  if [ "$line" == "---" ]; then
    run_prompt "$current"
    current=""
  else
    current+="$line"$'\n'
  fi
done 3< "$ARCHIVE_FILE"
run_prompt "$current"

echo ""
echo "All $count prompt(s) processed. Queue archived at: $ARCHIVE_FILE"
