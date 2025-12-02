# Use the official .NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 8.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["kgadi-ya-code-api/kgadi-ya-code-api.csproj", "kgadi-ya-code-api/"]
RUN dotnet restore "kgadi-ya-code-api/kgadi-ya-code-api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/kgadi-ya-code-api"
RUN dotnet build "kgadi-ya-code-api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "kgadi-ya-code-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - create runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for offline data
RUN mkdir -p /app/OfflineData

ENTRYPOINT ["dotnet", "kgadi-ya-code-api.dll"]