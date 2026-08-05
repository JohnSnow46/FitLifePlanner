#!/usr/bin/env bash
# Runs each prompt in queue.txt through `claude -p`, one at a time, in a fresh
# session per prompt. Waits for one to finish before starting the next.
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
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
  local out_file="$LOG_DIR/output_${TIMESTAMP}_$(printf '%02d' "$count").md"
  echo "=== [$count] starting $(date +%H:%M:%S) -> $out_file ==="
  printf '%s' "$prompt" | claude -p > "$out_file" 2>&1
  echo "=== [$count] done $(date +%H:%M:%S) ==="
}

current=""
while IFS= read -r line || [ -n "$line" ]; do
  if [ "$line" == "---" ]; then
    run_prompt "$current"
    current=""
  else
    current+="$line"$'\n'
  fi
done < "$ARCHIVE_FILE"
run_prompt "$current"

echo "All $count prompt(s) processed. Queue archived at: $ARCHIVE_FILE"
