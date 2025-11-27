FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 6001

# Copy solution and project files
COPY MicroServices.sln MicroServices.sln
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj

# Restore as distinct layers
RUN dotnet restore src/GatewayService/GatewayService.csproj

# Copy everything else and build
COPY src/GatewayService src/GatewayService
WORKDIR /app/src/GatewayService
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "GatewayService.dll"]
