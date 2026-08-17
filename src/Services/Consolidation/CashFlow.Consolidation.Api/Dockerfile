# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files for layer caching
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/CashFlow.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/CashFlow.BuildingBlocks.Messaging/CashFlow.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/CashFlow.BuildingBlocks.Messaging/"]
COPY ["src/Services/Consolidation/CashFlow.Consolidation.Domain/CashFlow.Consolidation.Domain.csproj", "src/Services/Consolidation/CashFlow.Consolidation.Domain/"]
COPY ["src/Services/Consolidation/CashFlow.Consolidation.Application/CashFlow.Consolidation.Application.csproj", "src/Services/Consolidation/CashFlow.Consolidation.Application/"]
COPY ["src/Services/Consolidation/CashFlow.Consolidation.Infrastructure/CashFlow.Consolidation.Infrastructure.csproj", "src/Services/Consolidation/CashFlow.Consolidation.Infrastructure/"]
COPY ["src/Services/Consolidation/CashFlow.Consolidation.Api/CashFlow.Consolidation.Api.csproj", "src/Services/Consolidation/CashFlow.Consolidation.Api/"]

RUN dotnet restore "src/Services/Consolidation/CashFlow.Consolidation.Api/CashFlow.Consolidation.Api.csproj"

# Copy source files
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Consolidation/ src/Services/Consolidation/

WORKDIR "/src/src/Services/Consolidation/CashFlow.Consolidation.Api"
RUN dotnet publish "CashFlow.Consolidation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.Consolidation.Api.dll"]
