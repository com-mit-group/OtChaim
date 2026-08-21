#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${NUGET_AUTH_TOKEN:-}" ]]; then
  echo "NUGET_AUTH_TOKEN is required to restore Yaref92.Events from GitHub Packages." >&2
  exit 2
fi

dotnet nuget remove source github >/dev/null 2>&1 || true
dotnet nuget add source https://nuget.pkg.github.com/yaron-E92/index.json \
  --name github \
  --username yaron-E92 \
  --password "$NUGET_AUTH_TOKEN" \
  --store-password-in-clear-text
