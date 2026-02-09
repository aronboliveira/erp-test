#!/bin/bash
set -e

BOLD='\033[1m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
RED='\033[0;31m'
NC='\033[0m'

# Function to check if a port is available
is_port_available() {
    local port=$1
    ! sudo lsof -i :$port >/dev/null 2>&1
}

# Function to find next available port
find_available_port() {
    local base_port=$1
    local max_attempts=${2:-20}
    local current_port=$base_port
    
    for ((i=0; i<max_attempts; i++)); do
        if is_port_available $current_port; then
            echo $current_port
            return 0
        fi
        current_port=$((base_port + i + 1))
    done
    
    echo -e "${RED}✗ Could not find available port starting from $base_port after $max_attempts attempts${NC}" >&2
    return 1
}

echo -e "${BOLD}${CYAN}"
echo "═══════════════════════════════════════════════════"
echo "  ACME ERP - Unified Stack Launcher"
echo "═══════════════════════════════════════════════════"
echo -e "${NC}"

cd "$(dirname "$0")/.."

echo -e "${BLUE}[0/6]${NC} ${BOLD}Checking and assigning ports...${NC}"

# Check and assign PostgreSQL port
POSTGRES_PORT=$(find_available_port 5432)
if [ -z "$POSTGRES_PORT" ]; then
    echo -e "${RED}✗ Failed to find available port for PostgreSQL${NC}"
    exit 1
fi
if [ "$POSTGRES_PORT" != "5432" ]; then
    echo -e "${YELLOW}→ PostgreSQL: Port 5432 busy, using $POSTGRES_PORT${NC}"
else
    echo -e "${GREEN}✓ PostgreSQL: Using default port $POSTGRES_PORT${NC}"
fi
export POSTGRES_PORT

# Check and assign Redis port (if needed)
REDIS_PORT=$(find_available_port 6379)
if [ -z "$REDIS_PORT" ]; then
    echo -e "${RED}✗ Failed to find available port for Redis${NC}"
    exit 1
fi
if [ "$REDIS_PORT" != "6379" ]; then
    echo -e "${YELLOW}→ Redis: Port 6379 busy, using $REDIS_PORT${NC}"
else
    echo -e "${GREEN}✓ Redis: Using default port $REDIS_PORT${NC}"
fi
export REDIS_PORT

# Check and assign Backend port
BACKEND_PORT=$(find_available_port 8080)
if [ -z "$BACKEND_PORT" ]; then
    echo -e "${RED}✗ Failed to find available port for Backend${NC}"
    exit 1
fi
if [ "$BACKEND_PORT" != "8080" ]; then
    echo -e "${YELLOW}→ Backend: Port 8080 busy, using $BACKEND_PORT${NC}"
else
    echo -e "${GREEN}✓ Backend: Using default port $BACKEND_PORT${NC}"
fi
export BACKEND_PORT

# Check and assign Frontend port
FRONTEND_PORT=$(find_available_port 4000)
if [ -z "$FRONTEND_PORT" ]; then
    echo -e "${RED}✗ Failed to find available port for Frontend${NC}"
    exit 1
fi
if [ "$FRONTEND_PORT" != "4000" ]; then
    echo -e "${YELLOW}→ Frontend: Port 4000 busy, using $FRONTEND_PORT${NC}"
else
    echo -e "${GREEN}✓ Frontend: Using default port $FRONTEND_PORT${NC}"
fi
export FRONTEND_PORT

echo ""

echo -e "${BLUE}[1/6]${NC} ${BOLD}Checking prerequisites...${NC}"
if ! command -v docker &> /dev/null; then
    echo -e "${YELLOW}⚠ Docker not found. Please install Docker Desktop.${NC}"
    exit 1
fi
if ! command -v docker compose &> /dev/null; then
    echo -e "${YELLOW}⚠ Docker Compose not found. Please install Docker Compose.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker and Docker Compose are available${NC}\n"

echo -e "${BLUE}[2/6]${NC} ${BOLD}Checking backend dependencies...${NC}"
if [ ! -d "acme-admin/target" ] || [ ! -f "acme-admin/target/*.jar" ]; then
    echo -e "${YELLOW}→ Backend not built yet. Maven will build during Docker image creation.${NC}"
else
    echo -e "${GREEN}✓ Backend artifacts found${NC}"
fi
echo ""

echo -e "${BLUE}[3/6]${NC} ${BOLD}Checking frontend dependencies...${NC}"
if [ ! -d "admin-dashboard/node_modules" ]; then
    echo -e "${YELLOW}→ Frontend dependencies not installed. npm will install during Docker image creation.${NC}"
else
    echo -e "${GREEN}✓ Frontend dependencies found${NC}"
fi
echo ""

echo -e "${BLUE}[4/6]${NC} ${BOLD}Starting Docker containers...${NC}"
echo -e "${CYAN}→ Building and starting: PostgreSQL + Backend (Spring Boot) + Frontend (Angular SSR)${NC}"

# Check if we need sudo for docker
if ! docker ps &> /dev/null; then
    echo -e "${YELLOW}→ Docker requires sudo privileges${NC}"
    sudo docker compose up -d --build
else
    docker compose up -d --build
fi

echo -e "${GREEN}✓ Containers started successfully${NC}\n"

echo -e "${BLUE}[5/6]${NC} ${BOLD}Waiting for services to be ready...${NC}"
echo -e "${CYAN}→ Checking database...${NC}"
for i in {1..30}; do
    if docker compose exec -T postgres pg_isready -U postgres &> /dev/null 2>&1 || sudo docker compose exec -T postgres pg_isready -U postgres &> /dev/null 2>&1; then
        echo -e "${GREEN}✓ PostgreSQL is ready${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${YELLOW}⚠ PostgreSQL is taking longer than expected${NC}"
    fi
    sleep 1
done

echo -e "${CYAN}→ Checking backend API...${NC}"
for i in {1..60}; do
    if curl -sf http://localhost:$BACKEND_PORT/actuator/health > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Backend API is ready${NC}"
        break
    fi
    if [ $i -eq 60 ]; then
        echo -e "${YELLOW}⚠ Backend API is taking longer than expected${NC}"
    fi
    sleep 1
done

echo -e "${CYAN}→ Checking frontend...${NC}"
for i in {1..30}; do
    if curl -sf http://localhost:$FRONTEND_PORT > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Frontend is ready${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${YELLOW}⚠ Frontend is taking longer than expected${NC}"
    fi
    sleep 1
done

echo ""
echo -e "${BOLD}${GREEN}"
echo "═══════════════════════════════════════════════════"
echo "  🚀 ACME ERP is now running!"
echo "═══════════════════════════════════════════════════"
echo -e "${NC}"
echo -e "${BOLD}Services:${NC}"
echo -e "  ${CYAN}🗄  PostgreSQL${NC}      → ${GREEN}localhost:$POSTGRES_PORT${NC}  (acmedb / postgres / postgres)"
echo -e "  ${CYAN}☕ Backend API${NC}      → ${GREEN}http://localhost:$BACKEND_PORT${NC}"
echo -e "  ${CYAN}🌐 Admin Dashboard${NC}  → ${GREEN}http://localhost:$FRONTEND_PORT${NC}"
echo ""
echo -e "${BOLD}Useful commands:${NC}"
echo -e "  ${YELLOW}docker compose logs -f${NC}    → View live logs"
echo -e "  ${YELLOW}docker compose ps${NC}         → Check container status"
echo -e "  ${YELLOW}docker compose down${NC}       → Stop all containers"
echo ""
echo -e "${BOLD}API Documentation:${NC}"
echo -e "  ${CYAN}Health Check${NC}        → ${GREEN}http://localhost:$BACKEND_PORT/actuator/health${NC}"
echo -e "  ${CYAN}Swagger UI${NC}          → ${GREEN}http://localhost:$BACKEND_PORT/swagger-ui.html${NC} ${YELLOW}(if enabled)${NC}"
echo ""
