# StockMind - Inventory Management System

## Architecture Overview

This project follows **Clean Architecture** principles combined with **Hexagonal Architecture** (Ports and Adapters), designed for scalability and future evolution to event-driven microservices.

## Technology Stack

### Backend
- **.NET 8** - Framework
- **ASP.NET Core Web API** - REST API
- **MediatR** - CQRS pattern implementation
- **Entity Framework Core** - Data access
- **FluentValidation** - Input validation
- **RabbitMQ** - Message broker for events
- **Redis** - Distributed caching
- **Serilog** - Structured logging

### Testing
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library

## Project Structure

```
StockMind/
??? src/
?   ??? StockMind.Domain/              # Domain Layer (Core Business Logic)
?   ?   ??? Common/                    # Base classes and domain primitives
?   ?   ?   ??? BaseEntity.cs
?   ?   ?   ??? AggregateRoot.cs
?   ?   ?   ??? ValueObject.cs
?   ?   ?   ??? IDomainEvent.cs
?   ?   ??? Repositories/              # Repository interfaces
?   ?   ?   ??? IRepository.cs
?   ?   ?   ??? IUnitOfWork.cs
?   ?   ??? Services/                  # Domain services
?   ?
?   ??? StockMind.Application/         # Application Layer (Use Cases)
?   ?   ??? Commands/                  # Command handlers (CQRS)
?   ?   ??? Queries/                   # Query handlers (CQRS)
?   ?   ??? DTOs/                      # Data Transfer Objects
?   ?   ??? Validators/                # FluentValidation validators
?   ?   ??? Mappings/                  # AutoMapper profiles
?   ?   ??? Behaviors/                 # MediatR pipeline behaviors
?   ?   ?   ??? ValidationBehavior.cs
?   ?   ??? Interfaces/                # Application service interfaces
?   ?   ??? Common/                    # Common application abstractions
?   ?       ??? ICommand.cs
?   ?       ??? IQuery.cs
?   ?       ??? ICommandHandler.cs
?   ?       ??? IQueryHandler.cs
?   ?       ??? Result.cs
?   ?
?   ??? StockMind.Infrastructure/      # Infrastructure Layer (External Concerns)
?   ?   ??? Persistence/               # Database implementation
?   ?   ?   ??? ApplicationDbContext.cs
?   ?   ?   ??? UnitOfWork.cs
?   ?   ?   ??? Configurations/        # EF Core entity configurations
?   ?   ?   ??? Repositories/          # Repository implementations
?   ?   ?       ??? Repository.cs
?   ?   ??? Messaging/                 # Event bus implementation
?   ?   ?   ??? IEventBus.cs
?   ?   ?   ??? RabbitMQ/
?   ?   ?       ??? RabbitMQEventBus.cs
?   ?   ??? Caching/                   # Cache implementation
?   ?   ?   ??? ICacheService.cs
?   ?   ?   ??? RedisCacheService.cs
?   ?   ??? Logging/                   # Logging configurations
?   ?   ??? External/                  # External API integrations (AI, etc.)
?   ?
?   ??? StockMind.API/                 # Presentation Layer (API)
?   ?   ??? Controllers/               # API Controllers
?   ?   ?   ??? BaseController.cs
?   ?   ??? Middlewares/               # Custom middlewares
?   ?   ?   ??? ExceptionHandlingMiddleware.cs
?   ?   ??? Filters/                   # Action filters
?   ?   ??? Extensions/                # Service registration extensions
?   ?   ??? Program.cs                 # Application entry point
?   ?   ??? appsettings.json           # Configuration
?   ?
?   ??? StockMind.Shared/              # Shared Kernel (Cross-cutting)
?       ??? Constants/                 # Application constants
?       ?   ??? ErrorMessages.cs
?       ??? Extensions/                # Extension methods
?       ??? Helpers/                   # Helper classes
?       ??? Attributes/                # Custom attributes
?
??? tests/
    ??? StockMind.UnitTests/           # Unit Tests
    ?   ??? Domain/                    # Domain layer tests
    ?   ??? Application/               # Application layer tests
    ?
    ??? StockMind.IntegrationTests/    # Integration Tests
        ??? API/                       # API endpoint tests
        ??? Infrastructure/            # Infrastructure tests

```

## Architecture Principles

### Clean Architecture Layers

1. **Domain Layer** (Inner Circle)
   - Contains business entities, value objects, and domain events
   - No dependencies on other layers
   - Pure business logic
   - Repository interfaces defined here

2. **Application Layer** (Use Cases)
   - Implements CQRS pattern using MediatR
   - Contains commands, queries, and their handlers
   - Orchestrates domain logic
   - Depends only on Domain layer

3. **Infrastructure Layer** (External Services)
   - Implements repository interfaces
   - Database access (EF Core)
   - External integrations (RabbitMQ, Redis)
   - Depends on Domain and Application layers

4. **API Layer** (Presentation)
   - REST API endpoints
   - Request/Response handling
   - Depends on Application and Infrastructure layers

### Key Design Patterns

- **CQRS** (Command Query Responsibility Segregation) via MediatR
- **Repository Pattern** for data access abstraction
- **Unit of Work** for transaction management
- **Domain Events** for decoupled communication
- **Result Pattern** for operation outcomes
- **Pipeline Behavior** for cross-cutting concerns (validation)

### Dependency Flow

```
API ? Application ? Domain
Infrastructure ? Application ? Domain
```

**Rule**: Dependencies flow inward. The Domain layer has no dependencies.

## Configuration

### Database Connection
Edit `src/StockMind.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StockMindDb;..."
}
```

### RabbitMQ
```json
"RabbitMQ": {
  "HostName": "localhost"
}
```

### Redis
```json
"Redis": {
  "Configuration": "localhost:6379"
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server
- RabbitMQ (optional for messaging)
- Redis (optional for caching)

### Build
```bash
dotnet restore
dotnet build
```

### Run API
```bash
cd src/StockMind.API
dotnet run
```

### Run Tests
```bash
dotnet test
```

## Next Steps

1. Implement domain entities and value objects
2. Create use cases (commands and queries)
3. Add API endpoints
4. Configure database migrations
5. Implement event handlers
6. Add AI integration as external service

## Architecture Guidelines

### DO:
- Keep domain logic in Domain layer
- Use CQRS for clear separation of read/write operations
- Publish domain events for important business actions
- Use value objects for domain concepts
- Validate at application layer boundaries

### DON'T:
- Let Domain depend on Infrastructure
- Put business logic in controllers
- Mix AI logic with domain rules
- Skip validation in command handlers
- Use anemic domain models
