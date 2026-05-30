FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY MonitoringPlatform.sln .

# Copy project files
COPY src/MonitoringPlatform.Domain/MonitoringPlatform.Domain.csproj src/MonitoringPlatform.Domain/
COPY src/MonitoringPlatform.Application/MonitoringPlatform.Application.csproj src/MonitoringPlatform.Application/
COPY src/MonitoringPlatform.Infrastructure/MonitoringPlatform.Infrastructure.csproj src/MonitoringPlatform.Infrastructure/
COPY src/MonitoringPlatform.API/MonitoringPlatform.API.csproj src/MonitoringPlatform.API/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/MonitoringPlatform.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MonitoringPlatform.API.dll"]