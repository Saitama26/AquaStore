#!/bin/bash

set -e

echo "Waiting for MySQL to be ready..."

DB_HOST="${MYSQL_HOST:-mysql}"
DB_PORT="${MYSQL_PORT:-3306}"
DB_PASSWORD="${MYSQL_ROOT_PASSWORD:-${MYSQL_PASSWORD:-sqlPassword123}}"

for i in {1..30}; do
  if mysqladmin ping -h "$DB_HOST" -P "$DB_PORT" -uroot -p"$DB_PASSWORD" --silent; then
    echo "MySQL is up."
    exit 0
  fi
  echo "MySQL not ready yet... retry $i/30"
  sleep 2
done

echo "MySQL did not become ready in time."
exit 1
