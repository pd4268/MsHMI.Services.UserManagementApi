# MsHMI.Services.UserManagementApi

ASP.NET Core (.NET 8) API for MsHMI User Management.

## What this is

- Implements endpoints like `/api/auth/login`, `/api/users`, `/api/groups`, `/api/rights`.
- Uses `MsHMI.OracleGateway` (separate service) for Oracle 8i access.

## Container

Build (from repo root):

```bash
docker build -t mshmi-user-management-api:local .
```

If your Docker environment cannot reach NuGet, you can still build by publishing on the host first:

```bash
./build-image.sh mshmi-user-management-api:local
```

Run (example):

```bash
docker run --rm -p 8080:80 \
  -e OracleGateway__BaseUrl=http://ms-oracle-gateway \
  mshmi-user-management-api:local
```

Health check:

- `GET /health`
