# E-Commerce Platform Architecture
## High-Performance Modular Monolith with DDD, TDD & Clean Architecture

> **Enterprise-Scale E-Commerce Platform** inspired by Sephora, commercetools, and leading brands
> Built with .NET 8, Domain-Driven Design, Test-Driven Development, and Clean Architecture principles

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Architectural Principles](#architectural-principles)
3. [System Architecture Overview](#system-architecture-overview)
4. [Clean Architecture Layers](#clean-architecture-layers)
5. [Domain-Driven Design (DDD)](#domain-driven-design-ddd)
6. [Bounded Contexts & Modules](#bounded-contexts--modules)
7. [Test-Driven Development (TDD)](#test-driven-development-tdd)
8. [High-Performance Patterns](#high-performance-patterns)
9. [Technology Stack](#technology-stack)
10. [Database Architecture](#database-architecture)
11. [API Design](#api-design)
12. [Security Architecture](#security-architecture)
13. [DevOps & Observability](#devops--observability)
14. [Scalability Strategy](#scalability-strategy)

---

## Executive Summary

This architecture defines a **high-performance modular monolith** e-commerce platform using:

- **Modular Monolith Architecture**: Clear module boundaries with ability to extract to microservices
- **Clean Architecture**: Dependency inversion with Domain at the center
- **Domain-Driven Design (DDD)**: Rich domain models, aggregates, domain events
- **Vertical Slice Architecture**: Feature-focused organization within modules
- **CQRS + Event Sourcing**: Separated read/write models for performance
- **Test-Driven Development**: Comprehensive testing at all levels

### Key Characteristics
- **Performance**: Sub-100ms response times, handles 10,000+ concurrent users
- **Scalability**: Horizontal scaling capability, database sharding ready
- **Maintainability**: Clear boundaries, SOLID principles, high cohesion
- **Reliability**: 99.99% uptime, circuit breakers, graceful degradation
- **Security**: OWASP compliant, PCI-DSS ready for payments

---

## Architectural Principles

### 1. Separation of Concerns
- Each module/layer has a single, well-defined responsibility
- Business logic isolated from infrastructure concerns
- Presentation separated from domain logic

### 2. Dependency Inversion
- High-level modules don't depend on low-level modules
- Both depend on abstractions (interfaces)
- Core domain has zero external dependencies

### 3. Explicit Architecture
- Clear module boundaries enforced at compile time
- Public APIs between modules only
- Internal implementation details hidden

### 4. Testability First
- TDD approach: Write tests before implementation
- High test coverage (>80%) with meaningful tests
- Fast unit tests, comprehensive integration tests

### 5. Performance by Design
- Caching at multiple levels
- Asynchronous operations where beneficial
- Database query optimization
- Bulk operations for high-volume scenarios

### 6. Eventual Consistency
- Domain events for cross-aggregate communication
- Integration events for cross-module communication
- Outbox pattern for reliable messaging

---

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Gateway Layer                        │
│  (Rate Limiting, Authentication, API Versioning, CORS)          │
└─────────────────────────────────────────────────────────────────┘
                                 │
        ┌────────────────────────┼────────────────────────┐
        │                        │                        │
┌───────▼───────┐      ┌────────▼────────┐      ┌───────▼────────┐
│   Catalog     │      │     Orders      │      │    Customers   │
│    Module     │      │     Module      │      │     Module     │
└───────┬───────┘      └────────┬────────┘      └────────┬───────┘
        │                       │                         │
┌───────▼───────┐      ┌────────▼────────┐      ┌────────▼───────┐
│   Inventory   │      │    Payments     │      │   Shipping     │
│    Module     │      │     Module      │      │    Module      │
└───────────────┘      └─────────────────┘      └────────────────┘
        │                       │                         │
        └───────────────────────┼─────────────────────────┘
                                │
                    ┌───────────▼──────────┐
                    │  Shared Kernel       │
                    │  - Domain Events     │
                    │  - Common Types      │
                    │  - Cross-cutting     │
                    └──────────────────────┘
```

### Module Communication
- **In-Process**: Via domain/integration events (MediatR)
- **Async**: Message broker (RabbitMQ/Azure Service Bus) for eventual consistency
- **Sync**: Direct method calls via public module APIs (limited use)

---

## Clean Architecture Layers

Each module follows the Clean Architecture pattern with 4 layers:

### 1. Domain Layer (Core)
**Location**: `Modules.<ModuleName>.Domain`

**Responsibilities**:
- Entities with business logic
- Aggregate roots
- Value objects
- Domain events
- Domain services
- Repository interfaces
- Specification interfaces
- Domain exceptions

**Dependencies**: NONE (pure .NET)

**Example Structure**:
```
Modules.Catalog.Domain/
├── Products/
│   ├── Product.cs (Aggregate Root)
│   ├── ProductId.cs (Value Object)
│   ├── ProductCreatedEvent.cs
│   ├── IProductRepository.cs
│   └── Specifications/
│       └── ProductByPriceRangeSpec.cs
├── Categories/
│   ├── Category.cs
│   ├── CategoryId.cs
│   └── ICategoryRepository.cs
└── Common/
    ├── DomainException.cs
    └── AggregateRoot.cs
```

### 2. Application Layer
**Location**: `Modules.<ModuleName>.Application`

**Responsibilities**:
- Use cases (Commands & Queries)
- DTOs (Data Transfer Objects)
- Command/Query handlers (MediatR)
- Validators (FluentValidation)
- Application services
- Pipeline behaviors (validation, logging, transactions)
- Integration event handlers

**Dependencies**: Domain Layer only

**Example Structure**:
```
Modules.Catalog.Application/
├── Products/
│   ├── Commands/
│   │   ├── CreateProduct/
│   │   │   ├── CreateProductCommand.cs
│   │   │   ├── CreateProductCommandHandler.cs
│   │   │   ├── CreateProductCommandValidator.cs
│   │   │   └── ProductDto.cs
│   │   └── UpdateProduct/
│   ├── Queries/
│   │   ├── GetProduct/
│   │   │   ├── GetProductQuery.cs
│   │   │   └── GetProductQueryHandler.cs
│   │   └── SearchProducts/
│   └── Events/
│       └── ProductCreatedEventHandler.cs
├── Behaviors/
│   ├── ValidationBehavior.cs
│   ├── LoggingBehavior.cs
│   └── TransactionBehavior.cs
└── Abstractions/
    └── ICatalogModule.cs (Public API)
```

### 3. Infrastructure Layer
**Location**: `Modules.<ModuleName>.Infrastructure`

**Responsibilities**:
- Repository implementations
- Database context (EF Core)
- External service integrations
- Caching implementations
- File storage
- Message broker implementations
- Specification implementations

**Dependencies**: Domain + Application

**Example Structure**:
```
Modules.Catalog.Infrastructure/
├── Persistence/
│   ├── CatalogDbContext.cs
│   ├── Configurations/
│   │   ├── ProductConfiguration.cs
│   │   └── CategoryConfiguration.cs
│   ├── Repositories/
│   │   ├── ProductRepository.cs
│   │   └── CategoryRepository.cs
│   └── Migrations/
├── Caching/
│   └── RedisCatalogCache.cs
├── Search/
│   └── ElasticsearchProductSearch.cs
└── Configuration/
    └── CatalogModuleStartup.cs
```

### 4. Presentation Layer
**Location**: `Modules.<ModuleName>.Presentation`

**Responsibilities**:
- API endpoints (Minimal APIs / Controllers)
- Request/Response models
- API documentation
- Endpoint filters
- HTTP concerns

**Dependencies**: Application Layer

**Example Structure**:
```
Modules.Catalog.Presentation/
├── Endpoints/
│   ├── ProductEndpoints.cs
│   └── CategoryEndpoints.cs
├── Filters/
│   └── ValidationFilter.cs
└── Models/
    ├── Requests/
    │   ├── CreateProductRequest.cs
    │   └── SearchProductsRequest.cs
    └── Responses/
        └── ProductResponse.cs
```

---

## Domain-Driven Design (DDD)

### Aggregate Patterns

#### 1. Product Aggregate (Catalog)
```csharp
// Aggregate Root
public sealed class Product : AggregateRoot<ProductId>
{
    public ProductName Name { get; private set; }
    public Money Price { get; private set; }
    public ProductDescription Description { get; private set; }
    public CategoryId CategoryId { get; private set; }

    private readonly List<ProductVariant> _variants = new();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();

    private readonly List<ProductImage> _images = new();
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    public InventoryStatus Status { get; private set; }

    // Business logic in domain
    public void UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount <= 0)
            throw new DomainException("Price must be positive");

        Price = newPrice;
        RaiseDomainEvent(new ProductPriceChangedEvent(Id, newPrice));
    }

    public void AddVariant(ProductVariant variant)
    {
        if (_variants.Any(v => v.Sku == variant.Sku))
            throw new DomainException("Variant SKU must be unique");

        _variants.Add(variant);
        RaiseDomainEvent(new ProductVariantAddedEvent(Id, variant.Id));
    }

    public Result<ProductId> MarkAsOutOfStock()
    {
        if (Status == InventoryStatus.Discontinued)
            return Result.Failure<ProductId>("Cannot mark discontinued product as out of stock");

        Status = InventoryStatus.OutOfStock;
        RaiseDomainEvent(new ProductOutOfStockEvent(Id));
        return Result.Success(Id);
    }
}
```

#### 2. Order Aggregate (Orders)
```csharp
public sealed class Order : AggregateRoot<OrderId>
{
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public ShippingAddress ShippingAddress { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }

    // Factory method
    public static Order Create(CustomerId customerId, ShippingAddress address)
    {
        var order = new Order
        {
            Id = new OrderId(Guid.NewGuid()),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            ShippingAddress = address,
            CreatedAt = DateTime.UtcNow
        };

        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public Result AddItem(ProductId productId, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Cannot modify non-draft order");

        if (quantity <= 0)
            return Result.Failure("Quantity must be positive");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, quantity, unitPrice));
        }

        RecalculateTotal();
        return Result.Success();
    }

    public Result Submit()
    {
        if (Status != OrderStatus.Draft)
            return Result.Failure("Order already submitted");

        if (!_items.Any())
            return Result.Failure("Cannot submit empty order");

        Status = OrderStatus.Pending;
        RaiseDomainEvent(new OrderSubmittedEvent(Id, CustomerId, TotalAmount));
        return Result.Success();
    }

    private void RecalculateTotal()
    {
        var total = _items.Sum(i => i.Subtotal.Amount);
        TotalAmount = new Money(total, "USD");
    }
}
```

### Value Objects

```csharp
// Money Value Object
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator *(Money money, decimal multiplier)
        => new Money(money.Amount * multiplier, money.Currency);
}

// ProductName Value Object
public sealed record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name is required");

        if (value.Length > 200)
            throw new ArgumentException("Product name too long");

        Value = value.Trim();
    }

    public static implicit operator string(ProductName name) => name.Value;
}
```

### Domain Events

```csharp
// Base Domain Event
public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Product Domain Events
public sealed record ProductCreatedEvent(ProductId ProductId, string Name) : DomainEvent;
public sealed record ProductPriceChangedEvent(ProductId ProductId, Money NewPrice) : DomainEvent;
public sealed record ProductOutOfStockEvent(ProductId ProductId) : DomainEvent;

// Order Domain Events
public sealed record OrderCreatedEvent(OrderId OrderId, CustomerId CustomerId) : DomainEvent;
public sealed record OrderSubmittedEvent(OrderId OrderId, CustomerId CustomerId, Money TotalAmount) : DomainEvent;
public sealed record OrderPaidEvent(OrderId OrderId, PaymentId PaymentId) : DomainEvent;
```

### Specification Pattern

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public Specification<T> And(Specification<T> other)
        => new AndSpecification<T>(this, other);

    public Specification<T> Or(Specification<T> other)
        => new OrSpecification<T>(this, other);
}

// Product Specifications
public sealed class ProductInPriceRangeSpec : Specification<Product>
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;

    public ProductInPriceRangeSpec(decimal minPrice, decimal maxPrice)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
    }

    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.Price.Amount >= _minPrice
                   && product.Price.Amount <= _maxPrice;
}

public sealed class ProductInStockSpec : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
        => product => product.Status == InventoryStatus.InStock;
}
```

---

## Bounded Contexts & Modules

### Strategic Design - Bounded Contexts

#### 1. Catalog Context (Core Domain)
**Responsibility**: Product information, categories, search, recommendations

**Aggregates**:
- Product (Root): SKU, name, description, price, images, variants, attributes
- Category (Root): Name, hierarchy, metadata
- Brand (Root): Name, description, logo

**Key Operations**:
- Product catalog management
- Category organization
- Product search & filtering
- Price management
- Inventory status

**Public Events**:
- ProductCreated
- ProductUpdated
- ProductPriceChanged
- ProductDiscontinued

---

#### 2. Orders Context (Core Domain)
**Responsibility**: Order processing, order lifecycle management

**Aggregates**:
- Order (Root): Items, totals, status, shipping address
- OrderItem (Entity): Product reference, quantity, price

**Key Operations**:
- Create order (shopping cart conversion)
- Add/remove items
- Apply discounts
- Submit order
- Cancel order
- Track order status

**Public Events**:
- OrderCreated
- OrderSubmitted
- OrderPaid
- OrderShipped
- OrderDelivered
- OrderCancelled

---

#### 3. Customers Context (Supporting)
**Responsibility**: Customer profiles, preferences, addresses

**Aggregates**:
- Customer (Root): Profile, email, addresses, preferences
- CustomerAddress (Entity): Shipping/billing addresses

**Key Operations**:
- Customer registration
- Profile management
- Address management
- Wishlist management
- Order history viewing

**Public Events**:
- CustomerRegistered
- CustomerProfileUpdated
- CustomerAddressAdded

---

#### 4. Inventory Context (Core Domain)
**Responsibility**: Stock management, reservations, allocation

**Aggregates**:
- InventoryItem (Root): Product reference, quantity, location
- StockReservation (Entity): Temporary holds on inventory

**Key Operations**:
- Stock tracking
- Reserve inventory (time-limited)
- Release reservation
- Restock notifications
- Low stock alerts

**Concurrency Control**:
- Optimistic locking with row versioning
- 15-minute reservation timeout
- Validation at checkout

**Public Events**:
- StockReserved
- StockReleased
- StockReplenished
- LowStockAlert

---

#### 5. Payments Context (Supporting)
**Responsibility**: Payment processing, payment methods, refunds

**Aggregates**:
- Payment (Root): Order reference, amount, status, method
- PaymentMethod (Value Object): Type, details (tokenized)

**Key Operations**:
- Process payment
- Refund payment
- Capture authorization
- Payment method management

**External Integrations**:
- Stripe API
- PayPal API
- PCI-compliant tokenization

**Public Events**:
- PaymentInitiated
- PaymentSucceeded
- PaymentFailed
- PaymentRefunded

---

#### 6. Shipping Context (Supporting)
**Responsibility**: Shipping options, tracking, fulfillment

**Aggregates**:
- Shipment (Root): Order reference, carrier, tracking, status
- ShippingOption (Entity): Carrier, cost, estimated delivery

**Key Operations**:
- Calculate shipping costs
- Create shipment
- Track shipment
- Update delivery status

**External Integrations**:
- Carrier APIs (FedEx, UPS, DHL)

**Public Events**:
- ShipmentCreated
- ShipmentDispatched
- ShipmentInTransit
- ShipmentDelivered

---

### Module Organization

```
src/
├── Modules/
│   ├── Catalog/
│   │   ├── Modules.Catalog.Domain/
│   │   ├── Modules.Catalog.Application/
│   │   ├── Modules.Catalog.Infrastructure/
│   │   └── Modules.Catalog.Presentation/
│   ├── Orders/
│   │   ├── Modules.Orders.Domain/
│   │   ├── Modules.Orders.Application/
│   │   ├── Modules.Orders.Infrastructure/
│   │   └── Modules.Orders.Presentation/
│   ├── Customers/
│   ├── Inventory/
│   ├── Payments/
│   └── Shipping/
├── Common/
│   ├── Common.Domain/
│   │   ├── AggregateRoot.cs
│   │   ├── Entity.cs
│   │   ├── ValueObject.cs
│   │   ├── DomainEvent.cs
│   │   ├── Result.cs
│   │   └── IRepository.cs
│   ├── Common.Application/
│   │   ├── ICommand.cs
│   │   ├── IQuery.cs
│   │   ├── IEventBus.cs
│   │   └── Behaviors/
│   └── Common.Infrastructure/
│       ├── EventBus/
│       ├── Caching/
│       └── Persistence/
├── API/
│   └── API.Gateway/
│       ├── Program.cs
│       ├── Middleware/
│       └── Configuration/
└── Tests/
    ├── UnitTests/
    ├── IntegrationTests/
    └── ArchitectureTests/
```

---

## Test-Driven Development (TDD)

### Testing Strategy

#### 1. Unit Tests (Fast - Run on every build)
**Target**: Domain logic, business rules, value objects

**Framework**: xUnit + FluentAssertions + NSubstitute

**Example**:
```csharp
public class OrderTests
{
    [Fact]
    public void AddItem_WhenOrderIsDraft_ShouldAddItemSuccessfully()
    {
        // Arrange
        var order = Order.Create(
            new CustomerId(Guid.NewGuid()),
            TestData.CreateShippingAddress()
        );
        var productId = new ProductId(Guid.NewGuid());
        var quantity = 2;
        var unitPrice = new Money(29.99m, "USD");

        // Act
        var result = order.AddItem(productId, quantity, unitPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(productId);
        order.Items.First().Quantity.Should().Be(quantity);
        order.TotalAmount.Amount.Should().Be(59.98m);
    }

    [Fact]
    public void AddItem_WhenOrderIsNotDraft_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(
            new CustomerId(Guid.NewGuid()),
            TestData.CreateShippingAddress()
        );
        order.Submit(); // Change status

        // Act
        var result = order.AddItem(
            new ProductId(Guid.NewGuid()),
            1,
            new Money(10, "USD")
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("non-draft");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AddItem_WhenQuantityIsZeroOrNegative_ShouldReturnFailure(int quantity)
    {
        // Arrange
        var order = Order.Create(
            new CustomerId(Guid.NewGuid()),
            TestData.CreateShippingAddress()
        );

        // Act
        var result = order.AddItem(
            new ProductId(Guid.NewGuid()),
            quantity,
            new Money(10, "USD")
        );

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
```

#### 2. Integration Tests
**Target**: Database operations, external services, module interactions

**Framework**: xUnit + Testcontainers + WebApplicationFactory

**Example**:
```csharp
public class ProductRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer;
    private CatalogDbContext _context;
    private ProductRepository _repository;

    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("catalog_test")
            .Build();

        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _context = new CatalogDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = TestData.CreateProduct();
        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(product.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Id.Should().Be(product.Id);
        retrieved.Name.Value.Should().Be(product.Name.Value);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
```

#### 3. API Tests (E2E)
**Target**: API endpoints, request/response flows

**Example**:
```csharp
public class ProductEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 29.99m,
            Currency = "USD",
            CategoryId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product.Name.Should().Be(request.Name);
    }
}
```

#### 4. Architecture Tests
**Target**: Enforce architectural rules

**Framework**: NetArchTest.Rules

**Example**:
```csharp
public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommand).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(CatalogDbContext).Assembly;

    [Fact]
    public void Domain_Should_NotHaveAnyDependencies()
    {
        // Arrange & Act
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Newtonsoft.Json",
                "MediatR"
            )
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_OnlyDependOnDomain()
    {
        // Arrange & Act
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Infrastructure", "Presentation")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_Should_HaveApiControllerAttribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .HaveCustomAttribute(typeof(ApiControllerAttribute))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
```

### TDD Workflow

1. **Red**: Write a failing test
2. **Green**: Write minimal code to pass
3. **Refactor**: Improve code while keeping tests green
4. **Commit**: Commit working code with tests

---

## High-Performance Patterns

### 1. Caching Strategy

#### Multi-Level Caching
```
┌─────────────┐    ┌────────────┐    ┌──────────────┐
│   L1: In-   │ -> │ L2: Redis  │ -> │ L3: Database │
│   Memory    │    │ Distributed│    │              │
└─────────────┘    └────────────┘    └──────────────┘
```

**Implementation**:
```csharp
// Product catalog caching
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IMemoryCache _l1Cache;
    private readonly IDistributedCache _l2Cache;
    private readonly TimeSpan _l1Duration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _l2Duration = TimeSpan.FromHours(1);

    public async Task<Product?> GetByIdAsync(ProductId id)
    {
        // L1: Memory cache
        if (_l1Cache.TryGetValue(id, out Product? product))
            return product;

        // L2: Redis cache
        var cached = await _l2Cache.GetStringAsync($"product:{id}");
        if (cached != null)
        {
            product = JsonSerializer.Deserialize<Product>(cached);
            _l1Cache.Set(id, product, _l1Duration);
            return product;
        }

        // L3: Database
        product = await _inner.GetByIdAsync(id);
        if (product != null)
        {
            await _l2Cache.SetStringAsync(
                $"product:{id}",
                JsonSerializer.Serialize(product),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _l2Duration
                }
            );
            _l1Cache.Set(id, product, _l1Duration);
        }

        return product;
    }
}
```

**Cache Patterns**:
- **Cache-Aside**: Load on demand, cache miss goes to DB
- **Read-Through**: Cache loads data transparently
- **Write-Through**: Writes go to cache and DB synchronously
- **Write-Behind**: Writes go to cache, async to DB

**Cache Invalidation**:
```csharp
// Domain event handler for cache invalidation
public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedEvent>
{
    private readonly IDistributedCache _cache;

    public async Task Handle(ProductUpdatedEvent @event, CancellationToken ct)
    {
        await _cache.RemoveAsync($"product:{@event.ProductId}");
        await _cache.RemoveAsync($"products:search:*"); // Pattern-based invalidation
    }
}
```

---

### 2. CQRS (Command Query Responsibility Segregation)

**Write Model (Commands)**: Domain-rich, normalized, transactional
**Read Model (Queries)**: Denormalized, optimized for reads, eventual consistency

```csharp
// Command Side - Write Model
public record CreateProductCommand(
    string Name,
    decimal Price,
    string Currency,
    Guid CategoryId
) : ICommand<ProductId>;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductId>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<ProductId>> Handle(
        CreateProductCommand command,
        CancellationToken ct)
    {
        var product = Product.Create(
            new ProductName(command.Name),
            new Money(command.Price, command.Currency),
            new CategoryId(command.CategoryId)
        );

        await _repository.AddAsync(product);
        await _unitOfWork.CommitAsync(ct);

        return Result.Success(product.Id);
    }
}

// Query Side - Read Model
public record GetProductQuery(Guid ProductId) : IQuery<ProductDto>;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    private readonly IDbConnection _connection; // Dapper for performance

    public async Task<ProductDto?> Handle(GetProductQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT
                p.id, p.name, p.price, p.currency,
                c.id as CategoryId, c.name as CategoryName,
                (SELECT json_agg(json_build_object('id', pi.id, 'url', pi.url))
                 FROM product_images pi WHERE pi.product_id = p.id) as Images
            FROM products p
            INNER JOIN categories c ON p.category_id = c.id
            WHERE p.id = @ProductId";

        return await _connection.QuerySingleOrDefaultAsync<ProductDto>(
            sql,
            new { query.ProductId }
        );
    }
}
```

---

### 3. Bulk Operations

**EF Core ExecuteUpdate/ExecuteDelete (EF Core 7+)**:
```csharp
// Bulk price update
public async Task UpdatePricesForCategoryAsync(
    CategoryId categoryId,
    decimal percentageIncrease)
{
    await _context.Products
        .Where(p => p.CategoryId == categoryId)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.Price.Amount,
                p => p.Price.Amount * (1 + percentageIncrease / 100))
        );
}
```

**EFCore.BulkExtensions for complex operations**:
```csharp
// Bulk insert for data migrations
public async Task ImportProductsAsync(List<Product> products)
{
    await _context.BulkInsertAsync(products, options =>
    {
        options.BatchSize = 1000;
        options.BulkCopyTimeout = 300;
        options.EnableStreaming = true;
    });
}
```

---

### 4. Asynchronous Processing

**Background Jobs with Hangfire**:
```csharp
// Send order confirmation emails asynchronously
public class OrderSubmittedEventHandler : INotificationHandler<OrderSubmittedEvent>
{
    private readonly IBackgroundJobClient _jobClient;

    public Task Handle(OrderSubmittedEvent @event, CancellationToken ct)
    {
        _jobClient.Enqueue<IEmailService>(x =>
            x.SendOrderConfirmationAsync(@event.OrderId)
        );

        return Task.CompletedTask;
    }
}

// Recurring job for abandoned cart reminders
RecurringJob.AddOrUpdate<IAbandonedCartService>(
    "abandoned-cart-reminders",
    x => x.SendRemindersAsync(),
    Cron.Hourly
);
```

---

### 5. Database Optimization

**Indexing Strategy**:
```csharp
// EF Core index configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        // Frequently queried fields
        entity.HasIndex(p => p.CategoryId);
        entity.HasIndex(p => p.Price);
        entity.HasIndex(p => p.Status);

        // Composite index for common queries
        entity.HasIndex(p => new { p.CategoryId, p.Status, p.Price });

        // Full-text search
        entity.HasIndex(p => p.Name).HasMethod("gin");
    });
}
```

**Query Optimization**:
```csharp
// Efficient pagination with keyset pagination (cursor-based)
public async Task<PagedResult<Product>> GetProductsAsync(
    ProductId? cursor,
    int pageSize = 20)
{
    var query = _context.Products.AsNoTracking();

    if (cursor != null)
    {
        query = query.Where(p => p.Id > cursor);
    }

    var products = await query
        .OrderBy(p => p.Id)
        .Take(pageSize + 1)
        .ToListAsync();

    var hasMore = products.Count > pageSize;
    if (hasMore) products.RemoveAt(pageSize);

    return new PagedResult<Product>(
        products,
        hasMore,
        products.LastOrDefault()?.Id
    );
}
```

---

### 6. API Performance

**Response Compression**:
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

**Output Caching (.NET 8)**:
```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .Tag("api-cache"));

    options.AddPolicy("products", builder => builder
        .Expire(TimeSpan.FromMinutes(30))
        .Tag("products"));
});

// Usage
app.MapGet("/api/v1/products", GetProducts)
    .CacheOutput("products");
```

**Rate Limiting**:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 100;
        limiterOptions.QueueLimit = 10;
    });
});
```

---

## Technology Stack

### Core Framework
- **.NET 8 LTS**: Latest long-term support version
- **C# 12**: Latest language features
- **ASP.NET Core**: Web framework
- **Minimal APIs**: High-performance endpoints

### Domain & Application
- **MediatR**: CQRS, mediator pattern
- **FluentValidation**: Request validation
- **Ardalis.Result**: Result pattern
- **Ardalis.Specification**: Specification pattern

### Data Access
- **Entity Framework Core 8**: ORM for write models
- **Dapper**: Micro-ORM for read models (performance)
- **PostgreSQL 16**: Primary database
- **EFCore.BulkExtensions**: Bulk operations

### Caching
- **Redis**: Distributed caching, session storage
- **Microsoft.Extensions.Caching.Memory**: In-memory L1 cache

### Messaging
- **RabbitMQ** OR **Azure Service Bus**: Async messaging
- **Outbox pattern**: Reliable message publishing

### Search
- **Elasticsearch 8**: Product catalog search
- **NEST**: .NET Elasticsearch client

### Background Jobs
- **Hangfire**: Background job processing
- **Quartz.NET**: Complex scheduling scenarios

### Security
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT auth
- **ASP.NET Core Identity**: User management
- **IdentityServer** OR **Azure AD B2C**: OAuth/OIDC

### Payments
- **Stripe.net**: Payment processing
- **PayPal SDK**: Alternative payment method

### Logging & Monitoring
- **Serilog**: Structured logging
- **Seq**: Local log aggregation
- **OpenTelemetry**: Distributed tracing
- **Prometheus**: Metrics
- **Grafana**: Dashboards

### Testing
- **xUnit**: Test framework
- **FluentAssertions**: Assertion library
- **NSubstitute**: Mocking
- **Testcontainers**: Integration tests
- **NetArchTest.Rules**: Architecture tests
- **Bogus**: Test data generation

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Local development
- **GitHub Actions**: CI/CD
- **Azure Container Apps** OR **Kubernetes**: Production hosting

---

## Database Architecture

### Database Per Module Pattern

Each bounded context has its own database schema for isolation:

```sql
-- Catalog Schema
CREATE SCHEMA catalog;

CREATE TABLE catalog.products (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    price_amount DECIMAL(18,2) NOT NULL,
    price_currency VARCHAR(3) NOT NULL,
    category_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    row_version BIGINT NOT NULL
);

CREATE INDEX idx_products_category ON catalog.products(category_id);
CREATE INDEX idx_products_status_price ON catalog.products(status, price_amount);

CREATE TABLE catalog.categories (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    parent_id UUID NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    FOREIGN KEY (parent_id) REFERENCES catalog.categories(id)
);

CREATE TABLE catalog.product_images (
    id UUID PRIMARY KEY,
    product_id UUID NOT NULL,
    url VARCHAR(500) NOT NULL,
    display_order INT NOT NULL,
    FOREIGN KEY (product_id) REFERENCES catalog.products(id) ON DELETE CASCADE
);

-- Orders Schema
CREATE SCHEMA orders;

CREATE TABLE orders.orders (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    total_currency VARCHAR(3) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    row_version BIGINT NOT NULL
);

CREATE INDEX idx_orders_customer ON orders.orders(customer_id);
CREATE INDEX idx_orders_status ON orders.orders(status);

CREATE TABLE orders.order_items (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    product_id UUID NOT NULL, -- Reference only, not FK
    quantity INT NOT NULL,
    unit_price_amount DECIMAL(18,2) NOT NULL,
    unit_price_currency VARCHAR(3) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders.orders(id) ON DELETE CASCADE
);

-- Inventory Schema
CREATE SCHEMA inventory;

CREATE TABLE inventory.stock_items (
    id UUID PRIMARY KEY,
    product_id UUID NOT NULL UNIQUE,
    quantity INT NOT NULL,
    reserved_quantity INT NOT NULL DEFAULT 0,
    warehouse_location VARCHAR(100),
    row_version BIGINT NOT NULL -- Optimistic locking
);

CREATE TABLE inventory.reservations (
    id UUID PRIMARY KEY,
    stock_item_id UUID NOT NULL,
    order_id UUID NOT NULL,
    quantity INT NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    FOREIGN KEY (stock_item_id) REFERENCES inventory.stock_items(id)
);

CREATE INDEX idx_reservations_expires ON inventory.reservations(expires_at);
```

### Read Models (CQRS)

Denormalized views for query performance:

```sql
-- Materialized view for product search
CREATE MATERIALIZED VIEW catalog.product_search_view AS
SELECT
    p.id,
    p.name,
    p.description,
    p.price_amount,
    p.price_currency,
    p.status,
    c.name as category_name,
    c.slug as category_slug,
    array_agg(pi.url) as image_urls,
    to_tsvector('english', p.name || ' ' || COALESCE(p.description, '')) as search_vector
FROM catalog.products p
INNER JOIN catalog.categories c ON p.category_id = c.id
LEFT JOIN catalog.product_images pi ON p.id = pi.product_id
GROUP BY p.id, c.name, c.slug;

CREATE INDEX idx_product_search_vector ON catalog.product_search_view USING GIN(search_vector);

-- Refresh strategy
CREATE OR REPLACE FUNCTION refresh_product_search_view()
RETURNS TRIGGER AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY catalog.product_search_view;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_refresh_product_search
AFTER INSERT OR UPDATE OR DELETE ON catalog.products
FOR EACH STATEMENT
EXECUTE FUNCTION refresh_product_search_view();
```

### Outbox Pattern Implementation

```sql
-- Outbox table for reliable messaging
CREATE TABLE common.outbox_messages (
    id UUID PRIMARY KEY,
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMP NULL,
    error TEXT NULL
);

CREATE INDEX idx_outbox_unprocessed ON common.outbox_messages(created_at)
WHERE processed_at IS NULL;
```

---

## API Design

### RESTful Endpoints

**URL Versioning Strategy**:
```
/api/v1/products
/api/v1/categories
/api/v1/orders
```

### Catalog Module Endpoints

```http
# Products
GET    /api/v1/products                    # List products (paginated)
GET    /api/v1/products/{id}                # Get product by ID
GET    /api/v1/products/search              # Search products
POST   /api/v1/products                     # Create product
PUT    /api/v1/products/{id}                # Update product
DELETE /api/v1/products/{id}                # Delete product
PATCH  /api/v1/products/{id}/price          # Update price

# Categories
GET    /api/v1/categories                   # List categories
GET    /api/v1/categories/{id}              # Get category
GET    /api/v1/categories/{slug}            # Get by slug
POST   /api/v1/categories                   # Create category
```

### Orders Module Endpoints

```http
# Orders
GET    /api/v1/orders                       # List user orders
GET    /api/v1/orders/{id}                  # Get order details
POST   /api/v1/orders                       # Create order
POST   /api/v1/orders/{id}/submit           # Submit order
POST   /api/v1/orders/{id}/cancel           # Cancel order
GET    /api/v1/orders/{id}/status           # Track order
```

### Minimal API Implementation

```csharp
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .CacheOutput("products")
            .RequireRateLimiting("api");

        group.MapGet("/{id:guid}", GetProduct)
            .WithName("GetProduct")
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(30)));

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .RequireAuthorization("Admin");

        return group;
    }

    private static async Task<Results<Ok<PagedResult<ProductDto>>, ProblemHttpResult>>
        GetProducts(
            [AsParameters] ProductSearchParams searchParams,
            ISender sender,
            CancellationToken ct)
    {
        var query = new SearchProductsQuery(
            searchParams.Query,
            searchParams.CategoryId,
            searchParams.MinPrice,
            searchParams.MaxPrice,
            searchParams.PageSize,
            searchParams.Cursor
        );

        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error);
    }

    private static async Task<Results<Ok<ProductDto>, NotFound, ProblemHttpResult>>
        GetProduct(
            Guid id,
            ISender sender,
            CancellationToken ct)
    {
        var query = new GetProductQuery(id);
        var result = await sender.Send(query, ct);

        return result switch
        {
            { IsSuccess: true } => TypedResults.Ok(result.Value),
            { Error: "Product not found" } => TypedResults.NotFound(),
            _ => TypedResults.Problem(result.Error)
        };
    }

    private static async Task<Results<Created<ProductDto>, ValidationProblem, ProblemHttpResult>>
        CreateProduct(
            CreateProductRequest request,
            ISender sender,
            CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Price,
            request.Currency,
            request.CategoryId
        );

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return TypedResults.Problem(result.Error);

        return TypedResults.Created(
            $"/api/v1/products/{result.Value}",
            new ProductDto { Id = result.Value }
        );
    }
}
```

### Request/Response Models

```csharp
// Request
public record CreateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [Range(0.01, 999999.99)]
    public decimal Price { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = "USD";

    [Required]
    public Guid CategoryId { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }
}

// Response
public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? Description { get; init; }
    public CategoryDto Category { get; init; } = null!;
    public List<ProductImageDto> Images { get; init; } = new();
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

// Paged result
public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public bool HasMore { get; init; }
    public string? NextCursor { get; init; }
    public int TotalCount { get; init; }
}
```

---

## Security Architecture

### Authentication & Authorization

**JWT Bearer Authentication**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Administrator"));

    options.AddPolicy("Customer", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("ManageProducts", policy =>
        policy.RequireClaim("permission", "products.manage"));
});
```

**Policies**:
- **Admin**: Full access to all resources
- **Customer**: Access to own orders, profile, wishlist
- **Vendor**: Manage own products (future multi-tenant)

### Data Protection

**Sensitive Data**:
- Payment information: PCI-compliant tokenization (Stripe)
- Personal data: Encryption at rest (PostgreSQL TDE)
- Passwords: ASP.NET Core Identity (PBKDF2)

**HTTPS Only**:
```csharp
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

### CORS Policy

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
    {
        policy.WithOrigins("https://www.example.com", "https://admin.example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

### Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; img-src 'self' https://cdn.example.com"
    );

    await next();
});
```

### Idempotency for Payment Operations

```csharp
// Idempotency middleware
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var cached = await _cache.GetStringAsync($"idempotency:{idempotencyKey}");
                if (cached != null)
                {
                    // Return cached response
                    var response = JsonSerializer.Deserialize<CachedResponse>(cached);
                    context.Response.StatusCode = response.StatusCode;
                    await context.Response.WriteAsJsonAsync(response.Body);
                    return;
                }
            }
        }

        await _next(context);
    }
}
```

---

## DevOps & Observability

### Structured Logging with Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ECommerce.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"]!)
    .CreateLogger();

builder.Host.UseSerilog();

// Correlation ID middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    context.Response.Headers.Add("X-Correlation-ID", correlationId);

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Database")!,
        name: "database",
        tags: new[] { "db", "postgres" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: new[] { "cache", "redis" })
    .AddRabbitMQ(
        builder.Configuration.GetConnectionString("RabbitMQ")!,
        name: "rabbitmq",
        tags: new[] { "messaging" })
    .AddElasticsearch(
        builder.Configuration.GetConnectionString("Elasticsearch")!,
        name: "elasticsearch",
        tags: new[] { "search" });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

### Distributed Tracing with OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation()
            .AddSource("ECommerce.Catalog")
            .AddSource("ECommerce.Orders")
            .AddJaegerExporter(options =>
            {
                options.AgentHost = builder.Configuration["Jaeger:Host"];
                options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"]!);
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });
```

### CI/CD Pipeline

**GitHub Actions Workflow**:
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore --configuration Release

    - name: Run Unit Tests
      run: dotnet test tests/UnitTests --no-build --configuration Release --logger trx

    - name: Run Integration Tests
      run: dotnet test tests/IntegrationTests --no-build --configuration Release --logger trx

    - name: Run Architecture Tests
      run: dotnet test tests/ArchitectureTests --no-build --configuration Release --logger trx

    - name: Code Coverage
      run: |
        dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:Html

    - name: Build Docker Image
      run: docker build -t ecommerce-api:${{ github.sha }} .

    - name: Push to Container Registry
      if: github.ref == 'refs/heads/main'
      run: |
        echo "${{ secrets.REGISTRY_PASSWORD }}" | docker login -u "${{ secrets.REGISTRY_USERNAME }}" --password-stdin
        docker tag ecommerce-api:${{ github.sha }} registry.example.com/ecommerce-api:${{ github.sha }}
        docker tag ecommerce-api:${{ github.sha }} registry.example.com/ecommerce-api:latest
        docker push registry.example.com/ecommerce-api:${{ github.sha }}
        docker push registry.example.com/ecommerce-api:latest

    - name: Deploy to Production
      if: github.ref == 'refs/heads/main'
      run: |
        # Deploy script here (Azure Container Apps / Kubernetes / etc.)
```

### Monitoring Dashboard (Grafana)

**Key Metrics**:
- **Request Rate**: Requests/second per endpoint
- **Response Time**: P50, P95, P99 latencies
- **Error Rate**: 4xx, 5xx errors
- **Database Performance**: Query time, connection pool
- **Cache Hit Rate**: Redis cache effectiveness
- **Business Metrics**: Orders/hour, revenue, conversion rate

---

## Scalability Strategy

### Horizontal Scaling

**Stateless API Servers**:
- All session state in Redis
- No in-process caching beyond L1 (short TTL)
- Load balancer distributes traffic

**Database Scaling**:
- Read replicas for read-heavy operations
- Connection pooling (PgBouncer)
- Table partitioning for large tables (orders by date)

### Migration Path to Microservices

**When to Extract**:
- Module becomes too large (>100K LOC)
- Different scaling requirements
- Different technology needs
- Separate team ownership

**How to Extract**:
1. Ensure module boundaries are clean
2. Extract database schema
3. Deploy as separate service
4. Switch to HTTP/gRPC for inter-module communication
5. Update API gateway routing

**Candidate Modules** (in order):
1. Payments (external integrations)
2. Shipping (external integrations)
3. Catalog (read-heavy, can scale independently)
4. Orders (core, extract last)

---

## Conclusion

This architecture provides:

✅ **High Performance**: Multi-level caching, CQRS, optimized queries
✅ **Scalability**: Horizontal scaling, modular design, microservices path
✅ **Maintainability**: Clean Architecture, DDD, clear boundaries
✅ **Testability**: TDD approach, comprehensive test coverage
✅ **Reliability**: Event-driven, outbox pattern, circuit breakers
✅ **Security**: JWT auth, PCI compliance ready, OWASP best practices
✅ **Observability**: Structured logging, tracing, metrics

**Next Steps**:
1. Set up project structure
2. Implement Shared Kernel (common types)
3. Build Catalog module (first vertical slice)
4. Add comprehensive tests
5. Implement CI/CD pipeline
6. Deploy to staging environment
7. Load testing and optimization
8. Production deployment

---

**Version**: 1.0
**Last Updated**: 2025-01-19
**Author**: Architecture Team
