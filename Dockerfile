# ---- Frontend build ----
FROM node:22-alpine AS frontend
WORKDIR /src
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---- Backend build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend
WORKDIR /src
COPY backend/ChatToDashboard.Api/ChatToDashboard.Api.csproj ChatToDashboard.Api/
RUN dotnet restore ChatToDashboard.Api/ChatToDashboard.Api.csproj
COPY backend/ChatToDashboard.Api/ ChatToDashboard.Api/
RUN dotnet publish ChatToDashboard.Api/ChatToDashboard.Api.csproj -c Release -o /app/publish

# ---- Runtime: one container serving both the API and the built frontend ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend /app/publish .
COPY --from=frontend /src/dist ./wwwroot
# Seed data baked into the image; mount a volume over /data to use your own files.
COPY data/ /data/
ENV DataFolderPath=/data
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatToDashboard.Api.dll"]
