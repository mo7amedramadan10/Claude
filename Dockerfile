# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/ChatToDashboard.Api/ChatToDashboard.Api.csproj ChatToDashboard.Api/
RUN dotnet restore ChatToDashboard.Api/ChatToDashboard.Api.csproj
COPY backend/ChatToDashboard.Api/ ChatToDashboard.Api/
# ChatToDashboard.Api.csproj embeds IntegrationConnector's source as resources (for
# ConnectorPackager to zip up at download time) without building it as a project — it must
# still be present as a sibling directory for that embed to resolve.
COPY backend/IntegrationConnector/ IntegrationConnector/
RUN dotnet publish ChatToDashboard.Api/ChatToDashboard.Api.csproj -c Release -o /app/publish

# ---- Runtime: one container serving the API and the UI ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Seed data baked into the image; mount a volume over /data to use your own files.
COPY data/ /data/
ENV DataFolderPath=/data
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatToDashboard.Api.dll"]
