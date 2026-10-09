#!/usr/bin/env bash
# Starts a local SQL Server instance in Docker for SmartPantry development.
# Usage: ./scripts/start-sqlserver.sh
# Override defaults with env vars: SA_PASSWORD, SQL_PORT, CONTAINER_NAME.
set -euo pipefail

CONTAINER_NAME="${CONTAINER_NAME:-smartpantry-sqlserver}"
SA_PASSWORD="${SA_PASSWORD:-SmartPantry_Dev123!}"
SQL_PORT="${SQL_PORT:-1433}"
VOLUME_NAME="${CONTAINER_NAME}-data"
IMAGE="mcr.microsoft.com/mssql/server:2022-latest"

if ! docker info >/dev/null 2>&1; then
  echo "Docker is not running. Start Docker Desktop and try again." >&2
  exit 1
fi

if docker ps --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
  echo "Container '$CONTAINER_NAME' is already running."
elif docker ps -a --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
  echo "Starting existing container '$CONTAINER_NAME'..."
  docker start "$CONTAINER_NAME" >/dev/null
else
  echo "Creating container '$CONTAINER_NAME'..."
  # SQL Server only ships amd64 images; Apple Silicon runs it through Rosetta.
  docker run -d \
    --name "$CONTAINER_NAME" \
    --platform linux/amd64 \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
    -p "$SQL_PORT:1433" \
    -v "$VOLUME_NAME:/var/opt/mssql" \
    "$IMAGE" >/dev/null
fi

echo "Waiting for SQL Server to accept connections..."
for _ in $(seq 1 60); do
  if docker exec "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
      -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server is ready on localhost,$SQL_PORT"
    echo
    echo "Connection string:"
    echo "Server=localhost,$SQL_PORT;Database=SmartPantry;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=true"
    exit 0
  fi
  sleep 2
done

echo "SQL Server did not become ready in time. Check logs: docker logs $CONTAINER_NAME" >&2
exit 1
