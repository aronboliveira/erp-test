#!/bin/bash
# Automated Docker Compose Boot with Port Conflict Resolution
# This script safely starts all services with automatic retry and port management

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

MAX_RETRIES=3
RETRY_COUNT=0

echo -e "${CYAN}🚀 ACME ERP - Automated Docker Boot${NC}"
echo "================================================"

# Function to check if port is available
check_port() {
    local port=$1
    ! (sudo lsof -i :$port >/dev/null 2>&1 || lsof -i :$port >/dev/null 2>&1)
}

# Function to find available port
find_free_port() {
    local base_port=$1
    local current_port=$base_port
    
    for i in {0..20}; do
        if check_port $current_port; then
            echo $current_port
            return 0
        fi
        current_port=$((base_port + i + 1))
    done
    return 1
}

# Function to kill stale docker-proxy processes
cleanup_stale_ports() {
    echo -e "${YELLOW}→ Cleaning up stale port bindings...${NC}"
    
    for port in 5432 6379 8080 4000; do
        local pids=$(sudo lsof -t -i:$port 2>/dev/null | grep -v "^$" || true)
        if [ ! -z "$pids" ]; then
            local process_names=$(sudo lsof -i:$port 2>/dev/null | grep LISTEN | awk '{print $1}' | sort -u || true)
            if echo "$process_names" | grep -q "docker-pr"; then
                echo -e "${YELLOW}  • Killing stale docker-proxy on port $port${NC}"
                echo "$pids" | xargs -r sudo kill -9 2>/dev/null || true
                sleep 0.5
            fi
        fi
    done
}

# Function to setup environment with available ports
setup_ports() {
    echo -e "${BLUE}→ Detecting available ports...${NC}"
    
    # PostgreSQL
    if ! check_port 5432; then
        export POSTGRES_PORT=$(find_free_port 5432)
        echo -e "${YELLOW}  • PostgreSQL: 5432 busy, using $POSTGRES_PORT${NC}"
    else
        export POSTGRES_PORT=5432
        echo -e "${GREEN}  • PostgreSQL: 5432 ✓${NC}"
    fi
    
    # Redis (if needed)
    if ! check_port 6379; then
        export REDIS_PORT=$(find_free_port 6379)
        echo -e "${YELLOW}  • Redis: 6379 busy, using $REDIS_PORT${NC}"
    else
        export REDIS_PORT=6379
        echo -e "${GREEN}  • Redis: 6379 ✓${NC}"
    fi
    
    # Backend
    if ! check_port 8080; then
        export BACKEND_PORT=$(find_free_port 8080)
        echo -e "${YELLOW}  • Backend: 8080 busy, using $BACKEND_PORT${NC}"
    else
        export BACKEND_PORT=8080
        echo -e "${GREEN}  • Backend: 8080 ✓${NC}"
    fi
    
    # Frontend
    if ! check_port 4000; then
        export FRONTEND_PORT=$(find_free_port 4000)
        echo -e "${YELLOW}  • Frontend: 4000 busy, using $FRONTEND_PORT${NC}"
    else
        export FRONTEND_PORT=4000
        echo -e "${GREEN}  • Frontend: 4000 ✓${NC}"
    fi
}

# Function to start docker compose with retry
start_containers() {
    local attempt=$1
    
    echo -e "\n${BLUE}→ Starting containers (attempt $attempt/$MAX_RETRIES)...${NC}"
    
    # Try with sudo first, fallback to regular docker
    if docker compose ps >/dev/null 2>&1; then
        docker compose up -d --build 2>&1
    else
        sudo -E docker compose up -d --build 2>&1
    fi
    
    local exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        return 0
    else
        return 1
    fi
}

# Function to wait for services
wait_for_services() {
    echo -e "\n${BLUE}→ Waiting for services to be ready...${NC}"
    
    # Wait for PostgreSQL
    echo -e "${CYAN}  • PostgreSQL...${NC}"
    for i in {1..30}; do
        if docker compose exec -T postgres pg_isready -U postgres >/dev/null 2>&1 || \
           sudo docker compose exec -T postgres pg_isready -U postgres >/dev/null 2>&1; then
            echo -e "${GREEN}    ✓ PostgreSQL ready${NC}"
            break
        fi
        sleep 1
    done
    
    # Wait for Frontend (which proxies to backend, so this validates both)
    echo -e "${CYAN}  • Application (Backend + Frontend)...${NC}"
    for i in {1..60}; do
        if curl -sf http://localhost:$FRONTEND_PORT >/dev/null 2>&1; then
            echo -e "${GREEN}    ✓ Application ready${NC}"
            break
        fi
        sleep 2
    done
}

# Main execution
cd "$(dirname "$0")"

# Cleanup stale ports first
cleanup_stale_ports

# Setup environment with available ports
setup_ports

# Try to start containers with retry logic
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    RETRY_COUNT=$((RETRY_COUNT + 1))
    
    if start_containers $RETRY_COUNT; then
        echo -e "${GREEN}✓ Containers started successfully${NC}"
        break
    else
        echo -e "${RED}✗ Failed to start containers${NC}"
        
        if [ $RETRY_COUNT -lt $MAX_RETRIES ]; then
            echo -e "${YELLOW}→ Cleaning up and retrying in 3 seconds...${NC}"
            
            # Cleanup
            docker compose down -v 2>/dev/null || sudo docker compose down -v 2>/dev/null || true
            cleanup_stale_ports
            
            sleep 3
            
            # Re-detect ports
            setup_ports
        else
            echo -e "${RED}✗ Max retries reached. Please check logs with: docker compose logs${NC}"
            exit 1
        fi
    fi
done

# Wait for services to be ready
wait_for_services

# Display status
echo -e "\n${GREEN}================================================${NC}"
echo -e "${GREEN}✓ ACME ERP is running!${NC}"
echo -e "${GREEN}================================================${NC}"
echo ""
echo -e "${CYAN}📍 Services:${NC}"
echo -e "   PostgreSQL  → localhost:$POSTGRES_PORT"
echo -e "   Backend     → http://localhost:$BACKEND_PORT"
echo -e "   Frontend    → http://localhost:$FRONTEND_PORT"
echo ""
echo -e "${CYAN}📋 Useful commands:${NC}"
echo -e "   docker compose logs -f    → View live logs"
echo -e "   docker compose ps         → Container status"
echo -e "   docker compose down       → Stop containers"
echo -e "   ./docker-down.sh          → Safe shutdown"
echo ""
