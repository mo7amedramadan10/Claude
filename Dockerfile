# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/ChatToDashboard.Api/ChatToDashboard.Api.csproj ChatToDashboard.Api/
RUN dotnet restore ChatToDashboard.Api/ChatToDashboard.Api.csproj
COPY backend/ChatToDashboard.Api/ ChatToDashboard.Api/
RUN dotnet publish ChatToDashboard.Api/ChatToDashboard.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime: one container serving the API and the UI ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Seed data baked into the image; mount a volume over /data to use your own files.
# Kept in /seed as well so an init container can copy it into an empty volume.
COPY --chown=app:app data/ /data/
COPY --chown=app:app data/ /seed/
ENV DataFolderPath=/data \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# Run as the non-root 'app' user shipped with the aspnet image.
USER app
ENTRYPOINT ["dotnet", "ChatToDashboard.Api.dll"]
