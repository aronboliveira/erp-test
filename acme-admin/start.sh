#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Load .env file if it exists
if [ -f .env ]; then
    echo "Loading environment from .env..."
    set -a
    source .env
    set +a
fi

# Override profile if passed as argument
if [ -n "$1" ]; then
    export SPRING_PROFILES_ACTIVE="$1"
fi

echo "Starting acme-admin with profile: ${SPRING_PROFILES_ACTIVE:-default}"
echo "Database: ${DATABASE_URL:-H2 in-memory}"

./mvnw spring-boot:run
