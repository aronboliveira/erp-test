#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://0.0.0.0:8080}"

if [[ -z "${ConnectionStrings__Default:-}" ]]; then
  export ConnectionStrings__Default="Host=localhost;Port=5434;Database=acmedb;Username=postgres;Password=postgres"
fi

exec dotnet run --project Acme.Admin.Api.csproj
