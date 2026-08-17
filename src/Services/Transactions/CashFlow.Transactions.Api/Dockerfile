# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files for layer caching
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/CashFlow.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Messaging/CashFlow.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Messaging/"]
COPY ["src/Services/Transactions/CashFlow.Transactions.Domain/CashFlow.Transactions.Domain.csproj", "src/Services/Transactions/CashFlow.Transactions.Domain/"]
COPY ["src/Services/Transactions/CashFlow.Transactions.Application/CashFlow.Transactions.Application.csproj", "src/Services/Transactions/CashFlow.Transactions.Application/"]
COPY ["src/Services/Transactions/CashFlow.Transactions.Infrastructure/CashFlow.Transactions.Infrastructure.csproj", "src/Services/Transactions/CashFlow.Transactions.Infrastructure/"]
COPY ["src/Services/Transactions/CashFlow.Transactions.Api/CashFlow.Transactions.Api.csproj", "src/Services/Transactions/CashFlow.Transactions.Api/"]

RUN dotnet restore "src/Services/Transactions/CashFlow.Transactions.Api/CashFlow.Transactions.Api.csproj"

# Copy source files
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Transactions/ src/Services/Transactions/

WORKDIR "/src/src/Services/Transactions/CashFlow.Transactions.Api"
RUN dotnet publish "CashFlow.Transactions.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.Transactions.Api.dll"]
