#!/usr/bin/env bash
# Usage: dropin-report.sh <upstream-ref> <base-ref>
# Prints a markdown report of upstream changes that could break the fork's
# "drop-in replacement for aptabase" promise. Read it before merging a sync PR.
set -euo pipefail
UP="${1:-upstream/main}"; BASE="${2:-main}"
RANGE="$BASE...$UP"

echo "## Upstream sync: drop-in report"
echo
echo "Commits from upstream: $(git rev-list --count "$BASE..$UP")"
echo
echo "### New upstream migrations (fork migrations live in the 9xxx range)"
git diff --name-status "$RANGE" -- src/Data/Migrations | grep -E '^A' | sed 's/^/- /' || echo "- none"
echo
echo "### ClickHouse schema changes (check the 365d-TTL-style overrides still apply)"
git diff --stat "$RANGE" -- etc/clickhouse | tail -1 | sed 's/^/- /' || true
[ -z "$(git diff --name-only "$RANGE" -- etc/clickhouse)" ] && echo "- none"
echo
echo "### New or changed /api/v0/ routes (fork claims /api/v0/apps, /api/v0/api-keys)"
git diff -U0 "$RANGE" -- src | grep -E '^\+.*Http(Get|Post|Put|Delete|Patch)\("/api/v0' | sed 's/^+ */- /' || true
git diff -U0 "$RANGE" -- src | grep -qE '^\+.*Http(Get|Post|Put|Delete|Patch)\("/api/v0' || echo "- none"
echo
echo "### New environment variables"
git diff -U0 "$RANGE" -- src/Features/EnvSettings.cs | grep -E '^\+.*(Get|MustGet|GetInt|GetBool)\("' | sed 's/^+ */- /' || true
git diff -U0 "$RANGE" -- src/Features/EnvSettings.cs | grep -qE '^\+.*(Get|MustGet|GetInt|GetBool)\("' || echo "- none"
echo
echo "### Fork features still wired"
for pat in "SIGNUP_ALLOWLIST" "AddApiKeys"; do
  if git grep -q "$pat" HEAD -- src 2>/dev/null; then echo "- $pat: present"; else echo "- **$pat: MISSING after merge**"; fi
done
