FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Required for HEALTHCHECK
RUN apt-get update \
  && apt-get install -y --no-install-recommends curl \
  && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://0.0.0.0:80

# This Dockerfile expects a local publish output folder.
# Build it with:
#   dotnet publish src/MsHMI.UserManagement.Api/MsHMI.UserManagement.Api.csproj -c Release -o publish
COPY publish/ .

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -fsS http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MsHMI.UserManagement.Api.dll"]
