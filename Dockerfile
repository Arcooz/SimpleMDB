# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and projects
COPY SimpleMDB.sln .
COPY src/Smdb.Api/Smdb.Api.csproj src/Smdb.Api/
COPY src/Smdb.Core/Smdb.Core.csproj src/Smdb.Core/
COPY src/Smdb.Csr/Smdb.Csr.csproj src/Smdb.Csr/
COPY ../SharedLibrary/src/Shared/Shared.csproj ../SharedLibrary/src/Shared/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/src/Smdb.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "Smdb.Api.dll"]