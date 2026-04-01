#!/bin/bash

# dev.sh - Development script for easy docker-compose management

COMMAND=$1
SERVICE=$2

print_usage() {
    echo "Usage: ./dev.sh [command] [service]"
    echo ""
    echo "Commands:"
    echo "  up            Start all services (detached)"
    echo "  down          Stop all services"
    echo "  restart       Restart all services or a specific service"
    echo "  build         Rebuild all services or a specific service and restart it"
    echo "  logs          Tail logs for all services or a specific service"
    echo "  clean         Stop services and remove containers, networks, and volumes"
    echo ""
    echo "Examples:"
    echo "  ./dev.sh up                  # Start everything"
    echo "  ./dev.sh build               # Rebuild and start everything"
    echo "  ./dev.sh build gateway       # Rebuild and restart only the gateway service"
    echo "  ./dev.sh build identity      # Rebuild and restart only the identity service"
    echo "  ./dev.sh build syncing       # Rebuild and restart only the syncing service"
    echo "  ./dev.sh logs repository     # Tail logs for the repository service"
    echo "  ./dev.sh logs syncing        # Tail logs for the syncing service"
}

if [ -z "$COMMAND" ]; then
    print_usage
    exit 1
fi

case $COMMAND in
    "up")
        echo "Starting all services..."
        docker compose up -d
        ;;
    "down")
        echo "Stopping all services..."
        docker compose down
        ;;
    "restart")
        if [ -n "$SERVICE" ]; then
            echo "Restarting service: $SERVICE..."
            docker compose restart "$SERVICE"
        else
            echo "Restarting all services..."
            docker compose restart
        fi
        ;;
    "build")
        if [ -n "$SERVICE" ]; then
            echo "Rebuilding and restarting service: $SERVICE..."
            docker compose up -d --build "$SERVICE"
        else
            echo "Rebuilding and restarting all services..."
            docker compose up -d --build
        fi
        ;;
    "logs")
        if [ -n "$SERVICE" ]; then
            docker compose logs -f "$SERVICE"
        else
            docker compose logs -f
        fi
        ;;
    "clean")
        echo "Cleaning up containers, networks, and volumes..."
        docker compose down -v
        ;;
    *)
        echo "Unknown command: $COMMAND"
        print_usage
        exit 1
        ;;
esac
