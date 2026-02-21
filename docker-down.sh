#!/bin/bash
# Safe Docker Compose Shutdown Script

set -e

CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${CYAN}🛑 Stopping ACME ERP containers...${NC}"

# Check whether sudo can run non-interactively
can_sudo_non_interactive() {
    sudo -n true >/dev/null 2>&1
}

if docker compose down; then
    :
elif can_sudo_non_interactive; then
    sudo -n docker compose down
else
    echo -e "${YELLOW}⚠ Could not stop with sudo (non-interactive). Continuing...${NC}"
fi

echo -e "${GREEN}✓ All containers stopped${NC}"

# Optionally clean up volumes
read -p "Remove volumes (database data will be lost)? [y/N] " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    if docker compose down -v; then
        :
    elif can_sudo_non_interactive; then
        sudo -n docker compose down -v
    else
        echo -e "${YELLOW}⚠ Could not remove volumes with sudo (non-interactive).${NC}"
    fi
    echo -e "${YELLOW}✓ Volumes removed${NC}"
fi

# Clean up stale port bindings
echo -e "${CYAN}→ Cleaning up port bindings...${NC}"
for port in 5432 6379 8080 4000; do
    lsof_cmd="lsof"
    if can_sudo_non_interactive; then
        lsof_cmd="sudo -n lsof"
    fi

    pids=$($lsof_cmd -t -iTCP:$port -sTCP:LISTEN 2>/dev/null | grep -v "^$" || true)
    if [ ! -z "$pids" ]; then
        process_names=$($lsof_cmd -iTCP:$port -sTCP:LISTEN 2>/dev/null | awk 'NR>1 {print $1}' | sort -u || true)
        if echo "$process_names" | grep -q "docker-pr"; then
            if can_sudo_non_interactive; then
                echo "$pids" | xargs -r sudo -n kill -9 2>/dev/null || true
            else
                echo "$pids" | xargs -r kill -9 2>/dev/null || true
            fi
        fi
    fi
done

echo -e "${GREEN}✓ Cleanup complete${NC}"
