<p style="margin: 0; padding: 0;">
  <img src="media/RA617.png"
       alt="RA 617 Docker"
       style="width: 100%; height: auto; max-height: 240px; object-fit: cover;" />
</p>

# Docker Deployment Guide

## Quick Start

### Option 1: Using Docker Compose (Recommended)

```bash
# Build and start the service
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the service
docker-compose down
```

The service will be available at `http://localhost:3000/tems/`

### Option 2: Using Docker Directly

```bash
# Build the image
docker build -t cppdas:latest .

# Run the container
docker run -d \
  --name cppdas \
  -p 3000:3000 \
  -v $(pwd)/logs:/app/logs \
  cppdas:latest \
  -dasUrl http://0.0.0.0:3000/tems/ \
  -dasLog /app/logs/das.log

# View logs
docker logs -f cppdas

# Stop the container
docker stop cppdas
docker rm cppdas
```

## Why Docker + Zig is Powerful

### Multi-Stage Build Benefits

Our Dockerfile uses a **multi-stage build**:

1. **Build Stage** (alpine + Zig): ~300 MB
   - Downloads Zig
   - Compiles C++ code
   - Produces the binary

2. **Runtime Stage** (alpine): ~10 MB
   - Only includes the compiled binary
   - Minimal dependencies (just libstdc++)

**Final image size: ~12 MB** (compared to 300+ MB with traditional C++ Docker images!)

### Image Size Comparison

| Approach | Base Image | Final Size | Build Time |
|----------|------------|------------|------------|
| Traditional (gcc:latest) | ~1.2 GB | ~1.3 GB | 5-10 min |
| Zig Multi-stage (alpine) | ~7 MB | ~12 MB | 2-3 min |
| **Reduction** | **99% smaller** | **99% smaller** | **50% faster** |

### Cross-Platform Builds

Build for different architectures from any machine:

```bash
# Build for AMD64 (x86_64)
docker build --platform linux/amd64 -t cppdas:amd64 .

# Build for ARM64 (Apple Silicon, Raspberry Pi)
docker build --platform linux/arm64 -t cppdas:arm64 .

# Build for both platforms
docker buildx build --platform linux/amd64,linux/arm64 -t cppdas:multi .
```

## Configuration

### Environment Variables

- `DAS_URL`: Server URL (default: `http://0.0.0.0:4242/cppdas/`)
- `DAS_LOG`: Log file path (default: `/app/logs/das.log`)

### Custom Port

Edit `docker-compose.yml`:

```yaml
ports:
  - "8080:8080"  # Host:Container
environment:
  - DAS_URL=http://0.0.0.0:8080/tems/
```

### Volume Mounts

Logs are persisted on the host in `./logs/`:

```yaml
volumes:
  - ./logs:/app/logs
```

## Advanced Usage

### Development Mode

For development with live code changes:

```bash
# Build without cache
docker-compose build --no-cache

# Rebuild and restart
docker-compose up -d --build
```

### Production Deployment

```bash
# Build optimized image
docker build --build-arg ZIG_VERSION=0.15.2 -t cppdas:v1.0 .

# Run with resource limits
docker run -d \
  --name cppdas-prod \
  --memory="256m" \
  --cpus="0.5" \
  -p 3000:3000 \
  --restart=unless-stopped \
  cppdas:v1.0
```

### Health Monitoring

```bash
# Check container health
docker ps

# Inspect health status
docker inspect --format='{{.State.Health.Status}}' cppdas

# View health check logs
docker inspect --format='{{range .State.Health.Log}}{{.Output}}{{end}}' cppdas
```

## Kubernetes Deployment

Example Kubernetes manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cppdas
spec:
  replicas: 3
  selector:
    matchLabels:
      app: cppdas
  template:
    metadata:
      labels:
        app: cppdas
    spec:
      containers:
      - name: cppdas
        image: cppdas:latest
        ports:
        - containerPort: 3000
        env:
        - name: DAS_URL
          value: "http://0.0.0.0:3000/tems/"
        resources:
          limits:
            memory: "256Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: cppdas-service
spec:
  selector:
    app: cppdas
  ports:
  - protocol: TCP
    port: 80
    targetPort: 3000
  type: LoadBalancer
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Docker image
      run: docker build -t cppdas:${{ github.sha }} .
    
    - name: Push to registry
      run: |
        docker tag cppdas:${{ github.sha }} myregistry/cppdas:latest
        docker push myregistry/cppdas:latest
```

## Troubleshooting

### Container won't start

```bash
# Check logs
docker logs cppdas

# Check if port is already in use
netstat -an | grep 3000

# Run interactively to debug
docker run -it --rm cppdas:latest /bin/sh
```

### Build fails

```bash
# Clean build
docker system prune -af
docker-compose build --no-cache
```

### Performance issues

```bash
# Check resource usage
docker stats cppdas

# Increase memory limit
docker update --memory="512m" cppdas
```

## Benefits Summary

✅ **Tiny images** - 12 MB vs 1.3 GB traditional  
✅ **Fast builds** - Zig compiles C++ quickly  
✅ **Cross-platform** - Build once, run anywhere  
✅ **Reproducible** - Same binary every time  
✅ **Secure** - Minimal attack surface (alpine base)  
✅ **Easy deployment** - One command to run  

## Next Steps

1. **Test locally**: `docker-compose up`
2. **Deploy to production**: Push image to registry
3. **Scale horizontally**: Use Kubernetes or Docker Swarm
4. **Monitor**: Integrate with Prometheus/Grafana

For more details on Zig benefits, see [ZIG_OVERVIEW.md](ZIG_OVERVIEW.md)
