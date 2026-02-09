# Docker Boot System

Automated, safe Docker container management with automatic port conflict resolution and retry logic.

## Quick Start

### Start All Services

```bash
./docker-up.sh
```

This will:

- 🔍 Detect and resolve port conflicts automatically
- 🧹 Clean up stale docker-proxy processes
- 🔄 Retry up to 3 times if startup fails
- ⏳ Wait for all services to be ready
- 📊 Display service URLs with actual ports

### Stop All Services

```bash
./docker-down.sh
```

Options:

- Removes volumes if confirmed (database data will be lost)
- Cleans up port bindings

## Features

### Automatic Port Assignment

If default ports are busy, the system automatically finds alternatives:

- PostgreSQL: 5432 → 5433, 5434, ... (up to +20)
- Redis: 6379 → 6380, 6381, ...
- Backend: 8080 → 8081, 8082, ...
- Frontend: 4000 → 4001, 4002, ...

### Retry Logic

- 3 automatic retry attempts
- Automatic cleanup between retries
- Detailed error reporting

### Enhanced Docker Compose

- Restart policies: `unless-stopped`
- Health checks for all services
- Resource limits (CPU/Memory)
- Conditional dependencies with restart

## Services

| Service     | Default Port | Container Port |
| ----------- | ------------ | -------------- |
| PostgreSQL  | 5434         | 5432           |
| Backend API | 8080         | 8080           |
| Frontend    | 4000         | 4000           |

## Manual Control

If you prefer manual control:

```bash
# Start with specific ports
POSTGRES_PORT=5433 BACKEND_PORT=8081 sudo -E docker compose up -d

# Stop
sudo docker compose down

# View logs
sudo docker compose logs -f

# Restart a service
sudo docker compose restart app
```

## Troubleshooting

### Port conflicts persist

```bash
# Clean up all docker-proxy processes
sudo killall docker-proxy

# Then restart
./docker-up.sh
```

### Build fails

```bash
# Clean rebuild
sudo docker compose down -v
sudo docker system prune -f
./docker-up.sh
```

### Check logs

```bash
sudo docker compose logs app
sudo docker compose logs postgres
```
