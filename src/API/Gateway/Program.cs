using Common.Application;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Infrastructure.Persistence;
using Modules.Catalog.Presentation;
using Modules.Customers.Infrastructure.Persistence;
using Modules.Customers.Presentation;
using Modules.Orders.Infrastructure.Persistence;
using Modules.Orders.Presentation;
using Serilog;
using Serilog.Formatting.Compact;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ECommerce.API")
    .Enrich.WithMachineName()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ECommerce API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter())
        .WriteTo.Seq(
            context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

    // Add services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "ECommerce API",
            Version = "v1",
            Description = @"High-Performance E-Commerce Platform with DDD, Clean Architecture & CQRS

**Modules:**
- **Catalog**: Product and category management
- **Customers**: Customer profile and address management
- **Orders**: Complete order lifecycle management

**Architecture:**
- Clean Architecture with 4 layers (Domain, Application, Infrastructure, Presentation)
- Domain-Driven Design (DDD) with aggregates, value objects, and domain events
- CQRS pattern with MediatR
- Event-Driven Architecture
- Modular Monolith design

**Technology Stack:**
- .NET 8 LTS, C# 12
- PostgreSQL with EF Core 8
- Minimal APIs
- FluentValidation
- Serilog for logging"
        });
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    // Response compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Output caching
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder
            .Expire(TimeSpan.FromMinutes(10))
            .Tag("api-cache"));
    });

    // Rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.PermitLimit = 100;
            limiterOptions.QueueLimit = 10;
        });
    });

    // Add modules
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddCustomersModule(builder.Configuration);
    builder.Services.AddOrdersModule(builder.Configuration);

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("Database")!,
            name: "database",
            tags: new[] { "db", "postgres" })
        .AddDbContextCheck<CatalogDbContext>(
            name: "catalog-db",
            tags: new[] { "db", "catalog" })
        .AddDbContextCheck<CustomersDbContext>(
            name: "customers-db",
            tags: new[] { "db", "customers" })
        .AddDbContextCheck<OrdersDbContext>(
            name: "orders-db",
            tags: new[] { "db", "orders" });

    var app = builder.Build();

    // Configure middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseResponseCompression();
    app.UseOutputCache();
    app.UseRateLimiter();

    // Map endpoints
    app.MapGet("/", () => new
    {
        name = "ECommerce API",
        version = "v1.0.0",
        status = "running",
        modules = new[] { "Catalog", "Customers", "Orders" },
        endpoints = new
        {
            catalog = "/api/v1/products, /api/v1/categories",
            customers = "/api/v1/customers",
            orders = "/api/v1/orders"
        },
        health = new
        {
            all = "/health",
            ready = "/health/ready",
            live = "/health/live"
        },
        documentation = "/swagger",
        timestamp = DateTime.UtcNow
    }).ExcludeFromDescription();

    app.MapGet("/error", () => Results.Problem())
        .ExcludeFromDescription();

    // Health checks
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

    // Map module endpoints
    app.MapCatalogEndpoints();
    app.MapCustomersEndpoints();
    app.MapOrdersEndpoints();

    // Run migrations in development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();

        var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await catalogDb.Database.MigrateAsync();
        Log.Information("Catalog database migrations applied");

        var customersDb = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        await customersDb.Database.MigrateAsync();
        Log.Information("Customers database migrations applied");

        var ordersDb = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        await ordersDb.Database.MigrateAsync();
        Log.Information("Orders database migrations applied");

        Log.Information("All database migrations applied successfully");
    }

    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
