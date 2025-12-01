FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only the necessary project files
COPY src/NotificationService/NotificationService.csproj src/NotificationService/
COPY src/Contracts/Contracts.csproj src/Contracts/

# Restore dependencies for NotificationService only
RUN dotnet restore src/NotificationService/NotificationService.csproj

# Copy the rest of the source code
COPY src/NotificationService src/NotificationService
COPY src/Contracts src/Contracts

# Build and publish
WORKDIR /app/src/NotificationService
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
