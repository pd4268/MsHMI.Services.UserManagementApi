#!/usr/bin/env bash
set -euo pipefail

IMAGE_NAME=${1:-mshmi-user-management-api:local}

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
cd "$repo_root"

# Publish on the host (uses host NuGet/network config)
dotnet publish src/MsHMI.UserManagement.Api/MsHMI.UserManagement.Api.csproj -c Release -o publish

# Build a runtime-only image using the publish/ output
docker build -t "$IMAGE_NAME" .

echo "Built $IMAGE_NAME"
