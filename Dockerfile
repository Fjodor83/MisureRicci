# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo il progetto per sfruttare la cache dei layer
COPY ["MisureRicci.csproj", "./"]
RUN dotnet restore "MisureRicci.csproj"

# Copia tutto il sorgente (ora sicuro grazie a .dockerignore)
COPY . .

# Pubblica in Release con ottimizzazioni per dimensione
RUN dotnet publish "MisureRicci.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:PublishTrimmed=false \
    -p:PublishSingleFile=false

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Installa curl per health check (opzionale ma utile)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Crea un utente non privilegiato (appuser)
RUN adduser --disabled-password --gecos "" appuser

# Crea directory per upload e assegna i permessi all'utente
RUN mkdir -p SecureUploads && chown -R appuser:appuser /app

# Copia il publish
COPY --from=build /app/publish .

# Passa all'utente non root
USER appuser

# Railway usa $PORT (default 8080)
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
# Riduce il footprint di memoria del GC per free tier (512MB RAM)
ENV DOTNET_GCConserveMemory=7
ENV DOTNET_GCHeapHardLimit=536870912

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MisureRicci.dll"]