# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["StockMind.sln", "./"]

# Copy project files
COPY ["src/StockMind.Domain/StockMind.Domain.csproj", "src/StockMind.Domain/"]
COPY ["src/StockMind.Application/StockMind.Application.csproj", "src/StockMind.Application/"]
COPY ["src/StockMind.Infrastructure/StockMind.Infrastructure.csproj", "src/StockMind.Infrastructure/"]
COPY ["src/StockMind.API/StockMind.API.csproj", "src/StockMind.API/"]
COPY ["src/StockMind.Shared/StockMind.Shared.csproj", "src/StockMind.Shared/"]

# Restore dependencies
RUN dotnet restore "src/StockMind.API/StockMind.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/StockMind.API"
RUN dotnet build "StockMind.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "StockMind.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Create a non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "StockMind.API.dll"]
