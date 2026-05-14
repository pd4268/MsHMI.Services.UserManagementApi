# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything so `docker build .` from repo root works.
COPY . .

# If a local publish/ folder exists, use it (helps when NuGet is blocked inside Docker).
# Otherwise, publish in the container.
RUN set -e; \
  if [ -d publish ] && [ -f publish/MsHMI.UserManagement.Api.dll ]; then \
    echo "Using existing ./publish output"; \
    mkdir -p /app/publish; \
    cp -a publish/. /app/publish/; \
  else \
    dotnet publish src/MsHMI.UserManagement.Api/MsHMI.UserManagement.Api.csproj -c Release -o /app/publish; \
  fi

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Required for HEALTHCHECK
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:80

COPY --from=build /app/publish .

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -fsS http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MsHMI.UserManagement.Api.dll"]
