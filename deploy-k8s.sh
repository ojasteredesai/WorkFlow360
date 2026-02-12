#!/bin/bash

set -e

NAMESPACE="workflow360"

echo "🚀 Starting Kubernetes Deployment..."

echo "📦 Applying namespace..."
kubectl apply -f k8s/namespace/

echo "💾 Applying storage..."
kubectl apply -f k8s/storage/

echo "🔐 Applying secrets..."
kubectl apply -f k8s/secrets/

echo "⚙️ Applying configmaps..."
kubectl apply -f k8s/configmaps/

echo "🐘 Deploying Postgres (StatefulSet)..."
kubectl apply -f k8s/statefulsets/

echo "🐰 Deploying RabbitMQ (StatefulSet)..."
kubectl apply -f k8s/statefulsets/

echo "🌐 Deploying services..."
kubectl apply -f k8s/services/

echo "🧩 Deploying application workloads..."
kubectl apply -f k8s/deployments/

echo "🌍 Applying ingress..."
kubectl apply -f k8s/ingress/

echo "⏳ Waiting for timecard-service rollout..."
kubectl rollout status deployment/timecard-service -n $NAMESPACE

echo "✅ Kubernetes deployment completed successfully."
