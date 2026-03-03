#!/bin/bash

set -e

NAMESPACE="workflow360"
DB_USER="postgres"
TARGET_DB="postgres_timecard"
DB_FOLDER="./database"

echo "🚀 Starting Database Initialization..."

POSTGRES_POD=$(kubectl get pod -n $NAMESPACE -l app=postgres -o jsonpath="{.items[0].metadata.name}")

if [ -z "$POSTGRES_POD" ]; then
  echo "❌ Postgres pod not found!"
  exit 1
fi

echo "📦 Waiting for Postgres pod to be ready..."
kubectl wait --for=condition=ready pod/$POSTGRES_POD -n $NAMESPACE --timeout=120s

echo "🧱 Step 1: Create database (if not exists)"
kubectl exec -i -n $NAMESPACE $POSTGRES_POD -- \
  psql -U $DB_USER -d postgres < $DB_FOLDER/01-create-databases.sql

echo "🧩 Step 2: Enable extensions"
kubectl exec -i -n $NAMESPACE $POSTGRES_POD -- \
  psql -U $DB_USER -d $TARGET_DB < $DB_FOLDER/00-enable-extensions.psql.sql

echo "📋 Step 3: Create tables"
kubectl exec -i -n $NAMESPACE $POSTGRES_POD -- \
  psql -U $DB_USER -d $TARGET_DB < $DB_FOLDER/02-create-all-tables.psql.sql

echo "📦 Step 4: Insert seed data"
kubectl exec -i -n $NAMESPACE $POSTGRES_POD -- \
  psql -U $DB_USER -d $TARGET_DB < $DB_FOLDER/03-insert-final-data.psql.sql

echo "📤 Step 5: Create event_outbox table"
kubectl exec -i -n $NAMESPACE $POSTGRES_POD -- \
  psql -U $DB_USER -d $TARGET_DB < $DB_FOLDER/04-create-event_outbox.psql.sql

echo "✅ Database initialization completed successfully."