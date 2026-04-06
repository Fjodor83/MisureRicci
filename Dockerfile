# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo il progetto per sfruttare la cache dei layer
COPY ["MisureRicci.csproj", "./"]
RUN dotnet restore "MisureRicci.csproj"

# Copia tutto il sorgente
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

# Crea directory per upload (nota: Railway ha filesystem efimero)
RUN mkdir -p SecureUploads

# Copia il publish
COPY --from=build /app/publish .

# Railway usa $PORT (default 8080)
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
# Riduce il footprint di memoria del GC per free tier (512MB RAM)
ENV DOTNET_GCConserveMemory=7
ENV DOTNET_GCHeapHardLimit=419430400

ENTRYPOINT ["dotnet", "MisureRicci.dll"]
