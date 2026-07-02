# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

# Copy project files
COPY ["PedidosApi/PedidosApi.csproj", "PedidosApi/"]
RUN dotnet restore "PedidosApi/PedidosApi.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "PedidosApi/PedidosApi.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

# Publish stage
FROM builder AS publish
RUN dotnet publish "PedidosApi/PedidosApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Set environment
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "PedidosApi.dll"]
