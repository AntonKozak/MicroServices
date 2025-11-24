FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 5001

# Copy solution and project files
COPY MicroServices.sln MicroServices.sln
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# Restore as distinct layers
RUN dotnet restore src/IdentityService/IdentityService.csproj

# Copy everything else and build
COPY src/IdentityService src/IdentityService
WORKDIR /app/src/IdentityService
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "IdentityService.dll"]
