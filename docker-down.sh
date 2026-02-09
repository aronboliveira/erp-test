#!/bin/bash
# Safe Docker Compose Shutdown Script

set -e

CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${CYAN}🛑 Stopping ACME ERP containers...${NC}"

# Try regular docker first, fallback to sudo
if docker compose ps >/dev/null 2>&1; then
    docker compose down
else
    sudo docker compose down
fi

echo -e "${GREEN}✓ All containers stopped${NC}"

# Optionally clean up volumes
read -p "Remove volumes (database data will be lost)? [y/N] " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if docker compose ps >/dev/null 2>&1; then
        docker compose down -v
    else
        sudo docker compose down -v
    fi
    echo -e "${YELLOW}✓ Volumes removed${NC}"
fi

# Clean up stale port bindings
echo -e "${CYAN}→ Cleaning up port bindings...${NC}"
for port in 5432 6379 8080 4000; do
    pids=$(sudo lsof -t -i:$port 2>/dev/null | grep -v "^$" || true)
    if [ ! -z "$pids" ]; then
        process_names=$(sudo lsof -i:$port 2>/dev/null | grep LISTEN | awk '{print $1}' | sort -u || true)
        if echo "$process_names" | grep -q "docker-pr"; then
            echo "$pids" | xargs -r sudo kill -9 2>/dev/null || true
        fi
    fi
done

echo -e "${GREEN}✓ Cleanup complete${NC}"
