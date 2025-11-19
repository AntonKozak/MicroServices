# MicroServices System Architecture Overview

This repository contains a sample microservices system used for learning. The architecture follows a microservices pattern with dedicated services for identity, auctions, bidding, search, notifications, and a gateway. Services communicate asynchronously through RabbitMQ.

## Contents

- `frontend/` — Optional frontend app for consuming the microservices.
- `src/` — Source code for services and libraries:
  - `src/AuctionService/` — ASP.NET Core Web API for auction functionality.
  - `src/Contracts/` — Shared DTOs/interfaces between services (optional).

## Mermaid sequence diagrams

### Login / Getting JWT
```mermaid
sequenceDiagram
    actor User as User (Browser)
    participant FE as Frontend
    participant GW as GatewayService
    participant ID as IdentityService

    User->>FE: Submit login form (email, password)
    FE->>GW: POST /auth/login
    GW->>ID: ValidateUser(email, password)
    ID-->>GW: AuthResult (success, userId, roles)
    GW-->>FE: JWT token
    FE-->>User: Store token (localStorage/cookie)
```

### Create Auction
```mermaid
sequenceDiagram
    actor User as User
    participant FE as Frontend
    participant GW as GatewayService
    participant AU as AuctionService
    participant BUS as MessageBroker
    participant SR as SearchService
    participant NT as NotificationService

    User->>FE: Create auction form
    FE->>GW: POST /auctions (JWT + CreateAuction)
    GW->>AU: CreateAuctionCommand(userId, data)
    AU->>AU: Validate + Save to DB
    AU->>BUS: Publish AuctionCreated
    BUS-->>SR: AuctionCreated
    SR->>SR: Update search index
    BUS-->>NT: AuctionCreated
    NT->>NT: Send "auction created" notification (optional)
    AU-->>GW: AuctionDto
    GW-->>FE: AuctionDto
```

### Search Auctions
```mermaid
sequenceDiagram
    actor User as User
    participant FE as Frontend
    participant GW as GatewayService
    participant SR as SearchService

    User->>FE: Type search query
    FE->>GW: GET /search?query=...
    GW->>SR: SearchAuctions(query, filters)
    SR->>SR: Query search index
    SR-->>GW: SearchResults
    GW-->>FE: SearchResults
    FE-->>User: Render results
```

### Place Bid
```mermaid
sequenceDiagram
    actor User as User
    participant FE as Frontend
    participant GW as GatewayService
    participant BD as BiddingService
    participant AU as AuctionService
    participant BUS as MessageBroker
    participant SR as SearchService
    participant NT as NotificationService

    User->>FE: Place bid (amount)
    FE->>GW: POST /bids (JWT + amount + auctionId)
    GW->>BD: PlaceBidCommand(userId, auctionId, amount)
    BD->>AU: GetAuction(auctionId)
    AU-->>BD: AuctionDetails (status, endTime, minBid, currentPrice)
    BD->>BD: Validate bid (time, amount, user)
    BD->>BD: Save bid to DB
    BD->>BUS: Publish BidPlaced
    alt Previous bidder exists
        BD->>BUS: Publish OutBid(previousBidderId, auctionId)
    end
    BUS-->>SR: BidPlaced
    SR->>SR: Update currentPrice in index
    BUS-->>NT: BidPlaced
    NT->>NT: Notify bidder: "your bid is accepted"
    BUS-->>NT: OutBid
    NT->>NT: Notify previous bidder: "you were outbid"
    BD-->>GW: BidDto/UpdatedAuctionInfo
    GW-->>FE: Response (OK + updated price)
```

### End Auction
```mermaid
sequenceDiagram
    participant SCH as AuctionScheduler (in AuctionService)
    participant AU as AuctionService
    participant BUS as MessageBroker
    participant BD as BiddingService
    participant NT as NotificationService
    participant SR as SearchService

    SCH->>AU: CheckExpiredAuctions()
    AU->>AU: Find auctions where EndTime <= now AND Status = Active
    loop For each expired auction
        AU->>BD: GetHighestBid(auctionId)
        BD-->>AU: HighestBid or null
        AU->>AU: Set status = Ended, WinnerId, FinalPrice
        AU->>BUS: Publish AuctionEnded(auctionId, sellerId, winnerId, finalPrice)
    end

    BUS-->>NT: AuctionEnded
    NT->>NT:
      Notify seller (auction finished)
      Notify winner (you won)
    BUS-->>SR: AuctionEnded
    SR->>SR: Remove/mark as ended in index
```

## Events

- AuctionCreated → SearchService, NotificationService
- BidPlaced → SearchService, NotificationService
- OutBid → NotificationService
- AuctionEnded → SearchService, NotificationService

## High-Level Flow

- Users authenticate via Identity Service and receive a JWT.
- All client requests go through the Gateway Service which validates JWTs and routes requests to the internal services.
- Services perform domain logic, persist state in dedicated databases, and publish domain events to RabbitMQ.
- Other services subscribe to events and update their own read models or send notifications.

## Components

- Identity Service: authentication, JWT tokens, users (Postgres).
- Gateway Service: API gateway, JWT validation, routing.
- Auction Service: create/manage auctions (Postgres), publish AuctionCreated/AuctionEnded.
- Bidding Service: handle bids, validate business rules (MongoDB).
- Search Service: read-optimized index for searching/filtering (MongoDB).
- Notification Service: subscribes to events, sends notifications (email, push, etc.).
- Event Bus: RabbitMQ for asynchronous pub/sub.

## Example Request Flow

1. User login → Identity Service → JWT.
2. Create auction → Gateway → Auction Service → AuctionCreated event → SearchService indexes → NotificationService (optional).
3. Place a bid → Gateway → Bidding Service → BidPlaced and OutBid events → SearchService updates price → NotificationService notifies.
4. Auction ends → Auction Service publishes AuctionEnded → Search/Notification services react.

## Project Structure

Suggested structure:
```
src/
  AuctionService/
    Controllers/
    DTOs/
    Models/
    Services/
    Repositories/
    Data/ (DbContext)
    Program.cs
    AuctionService.csproj
src/
  Contracts/
tests/
  AuctionService.Tests/
```

## How to scaffold the projects

Run these commands from the repository root (Windows):

```powershell
dotnet new webapi -n AuctionService -o src\AuctionService
dotnet new classlib -n Contracts -o src\Contracts
dotnet new sln -n MicroServices
dotnet sln add src\AuctionService\AuctionService.csproj src\Contracts\Contracts.csproj
```

## Build and run

- Build:
```powershell
dotnet build
```

- Run the API:
```powershell
dotnet run --project src\AuctionService\AuctionService.csproj
```

- Use Postman, curl, or a browser to test endpoints once the API is running.

## Adding tests

- Create test project:
```powershell
dotnet new xunit -n AuctionService.Tests -o tests\AuctionService.Tests
dotnet sln add tests\AuctionService.Tests\AuctionService.Tests.csproj
```

## Tips & Next Steps

- Add a Dockerfile and docker-compose for local integration tests.
- Add EF Core migrations for Postgres (AuctionService).
- Add Swagger/OpenAPI, health checks, and structured logging (Serilog).
- Add CI (GitHub Actions) to run tests and builds.

## Contributing

- Create feature branches and open pull requests.
- Add tests for new functionality and follow coding standards.

## License

Choose a license for the repository (MIT, Apache 2.0, etc.).
