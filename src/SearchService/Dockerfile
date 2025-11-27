FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 7002

# Copy solution and project files
COPY MicroServices.sln MicroServices.sln
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore as distinct layers
RUN dotnet restore src/SearchService/SearchService.csproj

# Copy everything else and build
COPY src/SearchService src/SearchService
COPY src/Contracts src/Contracts
WORKDIR /app/src/SearchService
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SearchService.dll"]
