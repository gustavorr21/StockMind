# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["StockMind.sln", "./"]

COPY ["src/StockMind.Domain/StockMind.Domain.csproj", "src/StockMind.Domain/"]
COPY ["src/StockMind.Application/StockMind.Application.csproj", "src/StockMind.Application/"]
COPY ["src/StockMind.Infrastructure/StockMind.Infrastructure.csproj", "src/StockMind.Infrastructure/"]
COPY ["src/StockMind.API/StockMind.API.csproj", "src/StockMind.API/"]
COPY ["src/StockMind.Shared/StockMind.Shared.csproj", "src/StockMind.Shared/"]

RUN dotnet restore "src/StockMind.API/StockMind.API.csproj"

COPY . .

WORKDIR "/src/src/StockMind.API"
RUN dotnet publish "StockMind.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "StockMind.API.dll"]