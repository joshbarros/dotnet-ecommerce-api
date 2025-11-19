# E-Commerce Platform - .NET 8

A high-performance, enterprise-scale e-commerce platform built with **Domain-Driven Design (DDD)**, **Clean Architecture**, and **CQRS** patterns.

## 🏗️ Architecture

- **Modular Monolith**: Clear module boundaries with future microservices extraction path
- **Clean Architecture**: 4-layer architecture (Domain → Application → Infrastructure → Presentation)
- **Domain-Driven Design**: Rich domain models, aggregates, value objects, domain events
- **CQRS**: Separated read/write models for optimal performance
- **Event-Driven**: Domain events for intra-module, integration events for inter-module communication
- **Test-Driven Development**: Comprehensive testing strategy

See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed architectural documentation.

## 🚀 Features

### ✅ Implemented Modules

#### Catalog Module (Complete)
- ✅ Product & Category aggregates with full business logic
- ✅ 10+ commands (Create, Update, Activate, Discontinue, Delete, etc.)
- ✅ Search & filtering queries with pagination
- ✅ Money and ProductName value objects
- ✅ Domain events (ProductCreated, PriceChanged, etc.)
- ✅ EF Core persistence with PostgreSQL
- ✅ 16 RESTful API endpoints
- ✅ Complete test coverage (Unit, Integration, Architecture)

#### Customers Module (Complete)
- ✅ Customer aggregate with profile management
- ✅ Address management (add/remove)
- ✅ Email, Phone, CustomerName value objects
- ✅ Customer status lifecycle (Active, Suspended, etc.)
- ✅ 5+ commands for customer operations
- ✅ EF Core with snake_case conventions
- ✅ 6 RESTful API endpoints

#### Orders Module (Complete)
- ✅ Order aggregate with complete lifecycle (Draft → Delivered)
- ✅ OrderItem entity with quantity management
- ✅ 8 commands (Create, AddItem, Submit, Confirm, Ship, Cancel, etc.)
- ✅ Cross-module integration (Customers, Catalog)
- ✅ Order status transitions with validation
- ✅ EF Core with owned entities
- ✅ 9 RESTful API endpoints

### 🏗️ Infrastructure & DevOps

- ✅ **API Gateway** - Unified entry point with all modules registered
- ✅ **Health Checks** - Database connectivity monitoring
- ✅ **Logging** - Serilog with Seq integration
- ✅ **CI/CD Pipeline** - GitHub Actions with:
  - Automated testing (Unit, Integration, Architecture)
  - Code coverage reporting
  - Security scanning (CodeQL, dependency review)
  - Docker multi-platform builds
  - Performance testing with k6
- ✅ **Docker Compose** - Complete local development stack
- ✅ **Comprehensive Test Suite** - 110+ tests across all modules
- ✅ **Response Compression** & **Output Caching**
- ✅ **Rate Limiting** - API throttling
- ✅ **Swagger/OpenAPI** - Complete API documentation

### 📋 Planned Features

- 📋 Inventory Module with concurrency control
- 📋 Payments Module (Stripe integration)
- 📋 Shipping Module with tracking
- 📋 Authentication & Authorization (JWT, OAuth)
- 📋 Full-text search with Elasticsearch
- 📋 Outbox pattern for reliable messaging
- 📋 OpenTelemetry distributed tracing
- 📋 CQRS read models with Dapper

## 🛠️ Technology Stack

### Core Framework
- **.NET 8 LTS** - Latest long-term support version
- **C# 12** - Latest language features
- **ASP.NET Core** - Minimal APIs

### Domain & Application
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation

### Data & Caching
- **PostgreSQL 16** - Primary database
- **Entity Framework Core 8** - ORM for write models
- **Dapper** - Micro-ORM for read models (planned)
- **Redis 7** - Distributed caching

### Messaging & Search
- **RabbitMQ** - Async messaging
- **Elasticsearch 8** - Product search (planned)

### Logging & Monitoring
- **Serilog** - Structured logging
- **Seq** - Local log aggregation
- **OpenTelemetry** - Distributed tracing (planned)

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Assertion library
- **NSubstitute** - Mocking
- **Testcontainers** - Integration tests
- **NetArchTest** - Architecture tests

## 📁 Project Structure

```
├── src/
│   ├── Common/                      # Shared Kernel
│   │   ├── Domain/                  # Base classes, interfaces
│   │   ├── Application/             # CQRS, behaviors
│   │   └── Infrastructure/          # Cross-cutting implementations
│   ├── Modules/                     # Business Modules
│   │   ├── Catalog/
│   │   │   ├── Domain/              # Aggregates, entities, value objects
│   │   │   ├── Application/         # Commands, queries, handlers
│   │   │   ├── Infrastructure/      # EF Core, repositories
│   │   │   └── Presentation/        # API endpoints
│   │   ├── Orders/                  # Same structure
│   │   ├── Customers/
│   │   ├── Inventory/
│   │   ├── Payments/
│   │   └── Shipping/
│   └── API/
│       └── Gateway/                 # Main API entry point
├── tests/
│   ├── UnitTests/                   # Domain & Application tests
│   ├── IntegrationTests/            # Infrastructure tests
│   └── ArchitectureTests/           # Enforce architectural rules
├── docs/                            # Additional documentation
├── scripts/                         # Database & deployment scripts
├── docker-compose.yml               # Local development infrastructure
└── ECommerce.sln                    # Solution file
```

## 🚦 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for infrastructure)
- Your favorite IDE:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/)
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
  - [VS Code](https://code.visualstudio.com/)

### 1. Clone the Repository

```bash
git clone <repository-url>
cd dotnet-ecommerce-api
```

### 2. Start Infrastructure

```bash
# Start all infrastructure services
docker-compose up -d

# Check services are running
docker-compose ps

# View logs
docker-compose logs -f
```

**Services Started:**
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
- Seq: `http://localhost:5341`
- Elasticsearch: `localhost:9200`
- RabbitMQ: `localhost:15672` (Management UI)

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Build the Solution

```bash
dotnet build
```

### 5. Run the Application

```bash
cd src/API/Gateway
dotnet run
```

**Automatic migrations** will run on startup in development mode.

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`
- Health: `http://localhost:5000/health`

### 6. Explore the API

**Root Endpoint:**
```bash
curl http://localhost:5000/
```

Returns API information with available modules and endpoints.

**API Endpoints (31 total):**
- **Catalog**: `/api/v1/products`, `/api/v1/categories`
- **Customers**: `/api/v1/customers`
- **Orders**: `/api/v1/orders`

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/UnitTests

# Run integration tests
dotnet test tests/IntegrationTests

# Run architecture tests
dotnet test tests/ArchitectureTests

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Development Tools

### Seq (Log Viewer)
- URL: `http://localhost:5341`
- View structured logs in real-time
- Query logs with SQL-like syntax

### RabbitMQ Management
- URL: `http://localhost:15672`
- Username: `ecommerce`
- Password: `ecommerce_dev_password`

### PostgreSQL
- Host: `localhost:5432`
- Database: `ecommerce`
- Username: `ecommerce`
- Password: `ecommerce_dev_password`

```bash
# Connect via psql
docker exec -it ecommerce-postgres psql -U ecommerce -d ecommerce
```

## 🏛️ Domain Model

### Catalog Module

**Aggregates:**
- **Product**: Core product information, pricing, images, status
- **Category**: Hierarchical product categories

**Value Objects:**
- **Money**: Monetary value with currency
- **ProductName**: Validated product name

**Domain Events:**
- ProductCreated
- ProductPriceChanged
- ProductActivated
- ProductOutOfStock
- ProductDiscontinued
- CategoryCreated
- CategoryUpdated

### Orders Module

**Aggregates:**
- **Order**: Complete order lifecycle with status transitions
- **OrderItem**: Line items with product snapshots

**Value Objects:**
- Shipping Address, Money (reused from Catalog)

**Domain Events:**
- OrderCreated, OrderSubmitted, OrderConfirmed
- OrderShipped, OrderDelivered, OrderCancelled

**Cross-Module Integration:**
- References CustomerID (Customers module)
- References ProductID (Catalog module)
- Uses Address and Money value objects

### Customers Module

**Aggregates:**
- **Customer**: Profile with multiple addresses

**Value Objects:**
- **Email**: RFC-compliant email validation
- **PhoneNumber**: International format support
- **CustomerName**: First and last name
- **Address**: Complete address information

**Domain Events:**
- CustomerCreated, CustomerEmailChanged
- CustomerSuspended, CustomerActivated, CustomerDeleted

## 🎯 Development Guidelines

### Domain Layer Rules
1. No external dependencies (pure .NET)
2. All business logic in domain entities
3. Use value objects for complex types
4. Raise domain events for significant changes
5. Use Result pattern instead of exceptions

### Clean Architecture Rules
1. Domain depends on nothing
2. Application depends only on Domain
3. Infrastructure depends on Domain & Application
4. Presentation depends on Application
5. All dependencies point inward

### Testing Strategy
1. Write tests first (TDD)
2. Unit tests for domain logic (fast, isolated)
3. Integration tests for infrastructure
4. Architecture tests to enforce rules
5. Aim for >80% code coverage

## 📖 Additional Documentation

- [ARCHITECTURE.md](./ARCHITECTURE.md) - Complete architecture documentation
- [API Documentation](./docs/api.md) - API endpoints (coming soon)
- [Domain Model](./docs/domain-model.md) - Detailed domain documentation (coming soon)

## 🤝 Contributing

This is a reference implementation showcasing best practices in:
- Domain-Driven Design
- Clean Architecture
- CQRS & Event Sourcing
- Test-Driven Development
- High-performance .NET

## 📝 License

This project is licensed under the MIT License.

## 🎓 Learning Resources

### Domain-Driven Design
- [Domain-Driven Design by Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Implementing Domain-Driven Design by Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)

### Clean Architecture
- [Clean Architecture by Robert C. Martin](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164)

### .NET Patterns
- [Microsoft .NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [Milan Jovanović's Blog](https://www.milanjovanovic.tech/)

## 📫 Contact

For questions or feedback, please open an issue on GitHub.

---

**Built with ❤️ using .NET 8, Clean Architecture, and Domain-Driven Design**
