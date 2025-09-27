# Docker Deployment Guide for Sterling Auctions

This guide provides comprehensive instructions for deploying the Sterling Auctions API using Docker and Docker Compose with Redis caching.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0+
- Git (to clone the repository)

## Quick Start

### 1. Clone and Navigate
```bash
git clone <repository-url>
cd sterling-auctions
```

### 2. Start the Application
```bash
# Start Redis and API containers
docker-compose -f docker/docker-compose.yml up -d

# View logs
docker-compose -f docker/docker-compose.yml logs -f
```

### 3. Verify Deployment
```bash
# Check container status
docker-compose -f docker/docker-compose.yml ps

# Test API health
curl http://localhost:5000/health

# Test Redis connection
curl http://localhost:5000/api/cache/stats
```

## Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx Proxy   │    │   Sterling API   │    │   Redis Cache   │
│   (Port 80/443) │───▶│   (Port 5000)    │───▶│   (Port 6379)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Container Services

### 1. Sterling Auctions API (`api`)
- **Image**: Built from `docker/Dockerfile.api`
- **Ports**: 5000 (HTTP), 5001 (HTTPS)
- **Environment**: Production-ready configuration
- **Dependencies**: Redis service
- **Health Check**: `/health` endpoint

### 2. Redis Cache (`redis`)
- **Image**: `redis:7.2-alpine`
- **Port**: 6379
- **Configuration**: Custom Redis config
- **Persistence**: Volume-mounted data directory
- **Health Check**: Redis ping command

### 3. Redis Commander (`redis-commander`) - Optional
- **Image**: `rediscommander/redis-commander:latest`
- **Port**: 8081
- **Purpose**: Web UI for Redis management
- **Profile**: `tools` (not started by default)

### 4. Nginx Proxy (`nginx`) - Optional
- **Image**: `nginx:alpine`
- **Ports**: 80 (HTTP), 443 (HTTPS)
- **Purpose**: Reverse proxy and load balancing
- **Profile**: `production` (not started by default)

## Deployment Profiles

### Development Profile (Default)
```bash
docker-compose -f docker/docker-compose.yml up -d
```
- Starts: `api`, `redis`
- Environment: Development
- Logging: Verbose
- Debugging: Enabled

### Tools Profile
```bash
docker-compose -f docker/docker-compose.yml --profile tools up -d
```
- Starts: `api`, `redis`, `redis-commander`
- Additional: Redis web UI
- Access: http://localhost:8081

### Production Profile
```bash
docker-compose -f docker/docker-compose.yml --profile production up -d
```
- Starts: `api`, `redis`, `nginx`
- Environment: Production
- Security: Enhanced
- SSL: Enabled

## Configuration

### Environment Variables

#### Required Variables
- `GOOGLE_CLIENT_ID`: Google OAuth client ID
- `GOOGLE_CLIENT_SECRET`: Google OAuth client secret
- `JWT_SECRET_KEY`: JWT signing key (32+ characters)

#### Optional Variables
- `GOOGLE_REDIRECT_URI`: OAuth redirect URI
- `CORS_ORIGINS`: Allowed CORS origins
- `LOG_LEVEL`: Logging level (Debug, Information, Warning, Error)

### Redis Configuration
- **Memory Limit**: 256MB (configurable)
- **Persistence**: RDB + AOF
- **Eviction Policy**: allkeys-lru
- **Max Clients**: 10,000

### API Configuration
- **Health Checks**: Enabled
- **Metrics**: Enabled
- **Profiling**: Disabled (production)
- **CORS**: Configured for frontend

## Commands Reference

### Basic Operations
```bash
# Start services
docker-compose -f docker/docker-compose.yml up -d

# Stop services
docker-compose -f docker/docker-compose.yml down

# Restart services
docker-compose -f docker/docker-compose.yml restart

# View logs
docker-compose -f docker/docker-compose.yml logs -f

# Scale API instances
docker-compose -f docker/docker-compose.yml up -d --scale api=3
```

### Development Operations
```bash
# Rebuild API image
docker-compose -f docker/docker-compose.yml build api

# Start with tools
docker-compose -f docker/docker-compose.yml --profile tools up -d

# View Redis data
docker exec -it sterling-redis redis-cli
```

### Production Operations
```bash
# Start production stack
docker-compose -f docker/docker-compose.yml --profile production up -d

# Update services
docker-compose -f docker/docker-compose.yml pull
docker-compose -f docker/docker-compose.yml up -d

# Backup Redis data
docker exec sterling-redis redis-cli BGSAVE
docker cp sterling-redis:/data/dump.rdb ./backup/
```

## Monitoring and Health Checks

### Health Check Endpoints
- **API Health**: `GET /health`
- **API Detailed**: `GET /api/health/detailed`
- **Redis Health**: Built into Docker health check

### Monitoring Commands
```bash
# Check container health
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# View resource usage
docker stats

# Check logs for errors
docker-compose -f docker/docker-compose.yml logs --tail=100 api | grep ERROR
```

## Troubleshooting

### Common Issues

#### 1. Redis Connection Timeout
```bash
# Check Redis container
docker logs sterling-redis

# Test Redis connectivity
docker exec -it sterling-redis redis-cli ping
```

#### 2. API Container Won't Start
```bash
# Check API logs
docker logs sterling-api

# Verify build
docker-compose -f docker/docker-compose.yml build api
```

#### 3. Port Conflicts
```bash
# Check port usage
netstat -tulpn | grep :5000

# Use different ports
docker-compose -f docker/docker-compose.yml up -d --scale api=0
# Edit docker-compose.yml ports section
docker-compose -f docker/docker-compose.yml up -d
```

### Performance Optimization

#### 1. Redis Optimization
- Increase memory limit in `redis.conf`
- Adjust eviction policy based on usage
- Enable Redis clustering for high availability

#### 2. API Optimization
- Scale API instances horizontally
- Configure connection pooling
- Enable response compression

#### 3. Nginx Optimization
- Configure caching headers
- Enable gzip compression
- Set up rate limiting

## Security Considerations

### Production Security
1. **Change Default Passwords**: Update JWT secret key
2. **Enable Redis Authentication**: Set `requirepass` in redis.conf
3. **Use HTTPS**: Configure SSL certificates
4. **Network Security**: Use Docker networks
5. **Resource Limits**: Set memory and CPU limits

### Security Commands
```bash
# Generate secure JWT key
openssl rand -base64 32

# Create SSL certificates
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout docker/nginx/ssl/key.pem \
  -out docker/nginx/ssl/cert.pem
```

## Backup and Recovery

### Data Backup
```bash
# Backup Redis data
docker exec sterling-redis redis-cli BGSAVE
docker cp sterling-redis:/data/dump.rdb ./backup/

# Backup application logs
docker cp sterling-api:/app/logs ./backup/logs/
```

### Data Recovery
```bash
# Restore Redis data
docker cp ./backup/dump.rdb sterling-redis:/data/
docker restart sterling-redis
```

## Scaling

### Horizontal Scaling
```bash
# Scale API instances
docker-compose -f docker/docker-compose.yml up -d --scale api=3

# Use load balancer
docker-compose -f docker/docker-compose.yml --profile production up -d
```

### Vertical Scaling
- Increase container memory limits
- Optimize Redis memory usage
- Configure connection pooling

## Development Workflow

### Local Development
1. Make code changes
2. Rebuild container: `docker-compose build api`
3. Restart services: `docker-compose restart api`
4. Test changes: `curl http://localhost:5000/health`

### CI/CD Integration
```yaml
# Example GitHub Actions workflow
- name: Build and Deploy
  run: |
    docker-compose -f docker/docker-compose.yml build
    docker-compose -f docker/docker-compose.yml up -d
```

## Support and Maintenance

### Regular Maintenance
- Monitor container health
- Update base images regularly
- Clean up unused volumes
- Review and rotate logs

### Support Commands
```bash
# Get container information
docker inspect sterling-api

# Access container shell
docker exec -it sterling-api /bin/bash

# View resource usage
docker system df
```

This Docker deployment provides a robust, scalable foundation for the Sterling Auctions API with Redis caching, making it production-ready and easy to maintain.
