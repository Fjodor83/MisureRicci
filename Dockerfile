# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo i file di progetto per sfruttare la cache dei layer
COPY ["MisureRicci.csproj", "./"]
RUN dotnet restore "MisureRicci.csproj" --locked-mode 2>/dev/null || dotnet restore "MisureRicci.csproj"

# Copia il resto del codice sorgente
COPY . .

# Pubblica in modalità Release
RUN dotnet publish "MisureRicci.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crea la cartella per i log e gli upload (filesystem efimero su Railway)
RUN mkdir -p logs SecureUploads

# Copia i file pubblicati
COPY --from=build /app/publish .

# Railway gestisce la porta tramite la variabile PORT
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "MisureRicci.dll"]
