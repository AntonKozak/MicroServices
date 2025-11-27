# Docker Commands Reference

## Build and Start All Services
```bash
docker-compose up --build
```

## Start All Services (without rebuild)
```bash
docker-compose up
```

## Stop All Services
```bash
docker-compose down
```

## Remove All Containers and Volumes (clean start)
```bash
docker-compose down -v
```

## View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f auction-svc
docker-compose logs -f search-svc
docker-compose logs -f identity-svc
docker-compose logs -f gateway-svc
```

## Service URLs (from host machine)
- **Gateway Service**: http://localhost:6001
- **Identity Service**: http://localhost:5001
- **Auction Service**: http://localhost:7001
- **Search Service**: http://localhost:7002
- **PostgreSQL**: localhost:5432
- **MongoDB**: localhost:27017
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Service URLs (inside Docker network)
- **Gateway Service**: http://gateway-svc:6001
- **Identity Service**: http://identity-svc:5001
- **Auction Service**: http://auction-svc:7001
- **Search Service**: http://search-svc:7002
- **PostgreSQL**: postgres:5432
- **MongoDB**: mongodb:27017
- **RabbitMQ**: rabbitmq:5672

## Testing Flow
1. **Login to get JWT token**:
   ```bash
   curl -X POST http://localhost:5001/api/account/login \
     -H "Content-Type: application/json" \
     -d '{"email":"bob@test.com","password":"Pass123$"}'
   ```

2. **Use Gateway to access services**:
   ```bash
   # Public - Get all auctions
   curl http://localhost:6001/auctions

   # Protected - Create auction (needs token)
   curl -X POST http://localhost:6001/auctions \
     -H "Authorization: Bearer YOUR_TOKEN_HERE" \
     -H "Content-Type: application/json" \
     -d '{...}'
   ```

## Rebuild Specific Service
```bash
docker-compose up --build auction-svc
docker-compose up --build search-svc
docker-compose up --build identity-svc
docker-compose up --build gateway-svc
```
