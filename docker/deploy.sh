#!/bin/bash

# Sterling Auctions Docker Deployment Script
# This script provides easy commands for deploying the Sterling Auctions API with Redis

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="docker/docker-compose.yml"
PROJECT_NAME="sterling-auctions"

# Functions
print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}  Sterling Auctions Deployment  ${NC}"
    echo -e "${BLUE}================================${NC}"
    echo
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker Desktop or Docker Engine."
        exit 1
    fi
    print_success "Docker is running"
}

# Check if Docker Compose is available
check_compose() {
    if ! docker-compose --version > /dev/null 2>&1; then
        print_error "Docker Compose is not available. Please install Docker Compose."
        exit 1
    fi
    print_success "Docker Compose is available"
}

# Start development environment
start_dev() {
    print_info "Starting development environment..."
    docker-compose -f $COMPOSE_FILE up -d
    print_success "Development environment started"
    print_info "API available at: http://localhost:5000"
    print_info "Redis available at: localhost:6379"
}

# Start with tools
start_tools() {
    print_info "Starting with development tools..."
    docker-compose -f $COMPOSE_FILE --profile tools up -d
    print_success "Environment with tools started"
    print_info "API available at: http://localhost:5000"
    print_info "Redis Commander available at: http://localhost:8081"
}

# Start production environment
start_prod() {
    print_info "Starting production environment..."
    docker-compose -f $COMPOSE_FILE --profile production up -d
    print_success "Production environment started"
    print_info "API available at: http://localhost:5000"
    print_info "Nginx proxy available at: http://localhost:80"
}

# Stop all services
stop_all() {
    print_info "Stopping all services..."
    docker-compose -f $COMPOSE_FILE down
    print_success "All services stopped"
}

# Restart services
restart() {
    print_info "Restarting services..."
    docker-compose -f $COMPOSE_FILE restart
    print_success "Services restarted"
}

# View logs
view_logs() {
    print_info "Viewing logs (Press Ctrl+C to exit)..."
    docker-compose -f $COMPOSE_FILE logs -f
}

# Check status
check_status() {
    print_info "Checking service status..."
    docker-compose -f $COMPOSE_FILE ps
}

# Health check
health_check() {
    print_info "Performing health checks..."
    
    # Check API health
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        print_success "API is healthy"
    else
        print_error "API health check failed"
    fi
    
    # Check Redis health
    if docker exec sterling-redis redis-cli ping > /dev/null 2>&1; then
        print_success "Redis is healthy"
    else
        print_error "Redis health check failed"
    fi
}

# Build and start
build_and_start() {
    print_info "Building and starting services..."
    docker-compose -f $COMPOSE_FILE build
    docker-compose -f $COMPOSE_FILE up -d
    print_success "Services built and started"
}

# Clean up
cleanup() {
    print_warning "This will remove all containers, volumes, and images. Are you sure? (y/N)"
    read -r response
    if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
        print_info "Cleaning up..."
        docker-compose -f $COMPOSE_FILE down -v --remove-orphans
        docker system prune -f
        print_success "Cleanup completed"
    else
        print_info "Cleanup cancelled"
    fi
}

# Show help
show_help() {
    echo "Usage: $0 [COMMAND]"
    echo
    echo "Commands:"
    echo "  dev         Start development environment (API + Redis)"
    echo "  tools       Start with development tools (API + Redis + Redis Commander)"
    echo "  prod        Start production environment (API + Redis + Nginx)"
    echo "  stop        Stop all services"
    echo "  restart     Restart all services"
    echo "  logs        View logs from all services"
    echo "  status      Check service status"
    echo "  health      Perform health checks"
    echo "  build       Build and start services"
    echo "  cleanup     Remove all containers and volumes"
    echo "  help        Show this help message"
    echo
    echo "Examples:"
    echo "  $0 dev      # Start development environment"
    echo "  $0 tools    # Start with Redis Commander"
    echo "  $0 prod     # Start production stack"
    echo "  $0 logs     # View logs"
}

# Main script
main() {
    print_header
    
    # Check prerequisites
    check_docker
    check_compose
    
    # Parse command
    case "${1:-help}" in
        dev)
            start_dev
            ;;
        tools)
            start_tools
            ;;
        prod)
            start_prod
            ;;
        stop)
            stop_all
            ;;
        restart)
            restart
            ;;
        logs)
            view_logs
            ;;
        status)
            check_status
            ;;
        health)
            health_check
            ;;
        build)
            build_and_start
            ;;
        cleanup)
            cleanup
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            show_help
            exit 1
            ;;
    esac
}

# Run main function
main "$@"
