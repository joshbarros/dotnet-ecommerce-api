# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution and project files
COPY ["ECommerce.sln", "./"]
COPY ["src/Common/Domain/Common.Domain.csproj", "src/Common/Domain/"]
COPY ["src/Common/Application/Common.Application.csproj", "src/Common/Application/"]
COPY ["src/Modules/Catalog/Domain/Modules.Catalog.Domain.csproj", "src/Modules/Catalog/Domain/"]
COPY ["src/Modules/Catalog/Application/Modules.Catalog.Application.csproj", "src/Modules/Catalog/Application/"]
COPY ["src/Modules/Catalog/Infrastructure/Modules.Catalog.Infrastructure.csproj", "src/Modules/Catalog/Infrastructure/"]
COPY ["src/Modules/Catalog/Presentation/Modules.Catalog.Presentation.csproj", "src/Modules/Catalog/Presentation/"]
COPY ["src/API/Gateway/Gateway.csproj", "src/API/Gateway/"]

# Restore dependencies
RUN dotnet restore "src/API/Gateway/Gateway.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/API/Gateway"
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser

# Install curl for health checks
RUN apk add --no-cache curl

# Copy published app
COPY --from=publish /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Gateway.dll"]
