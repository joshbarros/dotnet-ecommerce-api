using Common.Application;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Infrastructure.Persistence;
using Modules.Catalog.Presentation;
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
            Description = "High-Performance E-Commerce Platform with DDD, Clean Architecture & CQRS"
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

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("Database")!,
            name: "database",
            tags: new[] { "db", "postgres" })
        .AddDbContextCheck<CatalogDbContext>(
            name: "catalog-db",
            tags: new[] { "db", "catalog" });

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

    // Run migrations in development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await catalogDb.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
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
