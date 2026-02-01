#!/bin/bash
set -e

echo "Starting QA deployment..."

cd docker

echo "Pulling images..."
docker compose pull

echo "Starting containers..."
docker compose up -d

echo "Deployment completed"