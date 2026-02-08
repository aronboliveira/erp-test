#!/bin/bash
set -e

BOLD='\033[1m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${BOLD}${CYAN}"
echo "═══════════════════════════════════════════════════"
echo "  ACME ERP - Unified Stack Launcher"
echo "═══════════════════════════════════════════════════"
echo -e "${NC}"

cd "$(dirname "$0")/.."

echo -e "${BLUE}[1/5]${NC} ${BOLD}Checking prerequisites...${NC}"
if ! command -v docker &> /dev/null; then
    echo -e "${YELLOW}⚠ Docker not found. Please install Docker Desktop.${NC}"
    exit 1
fi
if ! command -v docker compose &> /dev/null; then
    echo -e "${YELLOW}⚠ Docker Compose not found. Please install Docker Compose.${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Docker and Docker Compose are available${NC}\n"

echo -e "${BLUE}[2/5]${NC} ${BOLD}Checking backend dependencies...${NC}"
if [ ! -d "acme-admin/target" ] || [ ! -f "acme-admin/target/*.jar" ]; then
    echo -e "${YELLOW}→ Backend not built yet. Maven will build during Docker image creation.${NC}"
else
    echo -e "${GREEN}✓ Backend artifacts found${NC}"
fi
echo ""

echo -e "${BLUE}[3/5]${NC} ${BOLD}Checking frontend dependencies...${NC}"
if [ ! -d "admin-dashboard/node_modules" ]; then
    echo -e "${YELLOW}→ Frontend dependencies not installed. npm will install during Docker image creation.${NC}"
else
    echo -e "${GREEN}✓ Frontend dependencies found${NC}"
fi
echo ""

echo -e "${BLUE}[4/5]${NC} ${BOLD}Starting Docker containers...${NC}"
echo -e "${CYAN}→ Building and starting: PostgreSQL + Backend (Spring Boot) + Frontend (Angular SSR)${NC}"
cd acme-admin
docker compose up -d --build
echo -e "${GREEN}✓ Containers started successfully${NC}\n"

echo -e "${BLUE}[5/5]${NC} ${BOLD}Waiting for services to be ready...${NC}"
echo -e "${CYAN}→ Checking database...${NC}"
for i in {1..30}; do
    if docker compose exec -T postgres pg_isready -U postgres &> /dev/null; then
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
    if curl -sf http://localhost:8080/actuator/health > /dev/null 2>&1; then
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
    if curl -sf http://localhost:4000 > /dev/null 2>&1; then
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
echo -e "  ${CYAN}🗄  PostgreSQL${NC}      → ${GREEN}localhost:5432${NC}  (acmedb / postgres / postgres)"
echo -e "  ${CYAN}☕ Backend API${NC}      → ${GREEN}http://localhost:8080${NC}"
echo -e "  ${CYAN}🌐 Admin Dashboard${NC}  → ${GREEN}http://localhost:4000${NC}"
echo ""
echo -e "${BOLD}Useful commands:${NC}"
echo -e "  ${YELLOW}docker compose -f acme-admin/docker-compose.yml logs -f${NC}    → View live logs"
echo -e "  ${YELLOW}docker compose -f acme-admin/docker-compose.yml ps${NC}         → Check container status"
echo -e "  ${YELLOW}docker compose -f acme-admin/docker-compose.yml down${NC}       → Stop all containers"
echo ""
echo -e "${BOLD}API Documentation:${NC}"
echo -e "  ${CYAN}Health Check${NC}        → ${GREEN}http://localhost:8080/actuator/health${NC}"
echo -e "  ${CYAN}Swagger UI${NC}          → ${GREEN}http://localhost:8080/swagger-ui.html${NC} ${YELLOW}(if enabled)${NC}"
echo ""
