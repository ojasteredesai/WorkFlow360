#!/bin/bash

NAMESPACE=workflow360

echo "Starting tunnels..."

kubectl port-forward -n $NAMESPACE deployment/timecard-service 5003:8080 &
kubectl port-forward -n $NAMESPACE pod/rabbitmq-0 15672:15672 &
kubectl port-forward -n $NAMESPACE pod/postgres-0 15432:5432 &

echo "Tunnels running:"
echo "API        -> http://localhost:5003"
echo "RabbitMQ   -> http://localhost:15672"
echo "Postgres   -> localhost:15432"
echo ""
echo "Press Ctrl+C to stop all tunnels"

wait