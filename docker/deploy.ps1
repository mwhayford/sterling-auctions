# Sterling Auctions Docker Deployment Script (PowerShell)
# This script provides easy commands for deploying the Sterling Auctions API with Redis

param(
    [Parameter(Position=0)]
    [string]$Command = "help"
)

# Configuration
$ComposeFile = "docker/docker-compose.yml"
$ProjectName = "sterling-auctions"

# Functions
function Write-Header {
    Write-Host "================================" -ForegroundColor Blue
    Write-Host "  Sterling Auctions Deployment  " -ForegroundColor Blue
    Write-Host "================================" -ForegroundColor Blue
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Blue
}

# Check if Docker is running
function Test-Docker {
    try {
        docker info | Out-Null
        Write-Success "Docker is running"
        return $true
    }
    catch {
        Write-Error "Docker is not running. Please start Docker Desktop."
        return $false
    }
}

# Check if Docker Compose is available
function Test-DockerCompose {
    try {
        docker-compose --version | Out-Null
        Write-Success "Docker Compose is available"
        return $true
    }
    catch {
        Write-Error "Docker Compose is not available. Please install Docker Compose."
        return $false
    }
}

# Start development environment
function Start-Development {
    Write-Info "Starting development environment..."
    docker-compose -f $ComposeFile up -d
    Write-Success "Development environment started"
    Write-Info "API available at: http://localhost:5000"
    Write-Info "Redis available at: localhost:6379"
}

# Start with tools
function Start-Tools {
    Write-Info "Starting with development tools..."
    docker-compose -f $ComposeFile --profile tools up -d
    Write-Success "Environment with tools started"
    Write-Info "API available at: http://localhost:5000"
    Write-Info "Redis Commander available at: http://localhost:8081"
}

# Start production environment
function Start-Production {
    Write-Info "Starting production environment..."
    docker-compose -f $ComposeFile --profile production up -d
    Write-Success "Production environment started"
    Write-Info "API available at: http://localhost:5000"
    Write-Info "Nginx proxy available at: http://localhost:80"
}

# Stop all services
function Stop-All {
    Write-Info "Stopping all services..."
    docker-compose -f $ComposeFile down
    Write-Success "All services stopped"
}

# Restart services
function Restart-Services {
    Write-Info "Restarting services..."
    docker-compose -f $ComposeFile restart
    Write-Success "Services restarted"
}

# View logs
function View-Logs {
    Write-Info "Viewing logs (Press Ctrl+C to exit)..."
    docker-compose -f $ComposeFile logs -f
}

# Check status
function Get-Status {
    Write-Info "Checking service status..."
    docker-compose -f $ComposeFile ps
}

# Health check
function Test-Health {
    Write-Info "Performing health checks..."
    
    # Check API health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Success "API is healthy"
        } else {
            Write-Error "API health check failed"
        }
    }
    catch {
        Write-Error "API health check failed: $($_.Exception.Message)"
    }
    
    # Check Redis health
    try {
        docker exec sterling-redis redis-cli ping | Out-Null
        Write-Success "Redis is healthy"
    }
    catch {
        Write-Error "Redis health check failed"
    }
}

# Build and start
function Build-AndStart {
    Write-Info "Building and starting services..."
    docker-compose -f $ComposeFile build
    docker-compose -f $ComposeFile up -d
    Write-Success "Services built and started"
}

# Clean up
function Remove-All {
    Write-Warning "This will remove all containers, volumes, and images. Are you sure? (y/N)"
    $response = Read-Host
    if ($response -match "^[yY]([eE][sS])?$") {
        Write-Info "Cleaning up..."
        docker-compose -f $ComposeFile down -v --remove-orphans
        docker system prune -f
        Write-Success "Cleanup completed"
    } else {
        Write-Info "Cleanup cancelled"
    }
}

# Show help
function Show-Help {
    Write-Host "Usage: .\deploy.ps1 [COMMAND]"
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  dev         Start development environment (API + Redis)"
    Write-Host "  tools       Start with development tools (API + Redis + Redis Commander)"
    Write-Host "  prod        Start production environment (API + Redis + Nginx)"
    Write-Host "  stop        Stop all services"
    Write-Host "  restart     Restart all services"
    Write-Host "  logs        View logs from all services"
    Write-Host "  status      Check service status"
    Write-Host "  health      Perform health checks"
    Write-Host "  build       Build and start services"
    Write-Host "  cleanup     Remove all containers and volumes"
    Write-Host "  help        Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\deploy.ps1 dev      # Start development environment"
    Write-Host "  .\deploy.ps1 tools    # Start with Redis Commander"
    Write-Host "  .\deploy.ps1 prod     # Start production stack"
    Write-Host "  .\deploy.ps1 logs     # View logs"
}

# Main script
function Main {
    Write-Header
    
    # Check prerequisites
    if (-not (Test-Docker)) { exit 1 }
    if (-not (Test-DockerCompose)) { exit 1 }
    
    # Parse command
    switch ($Command.ToLower()) {
        "dev" {
            Start-Development
        }
        "tools" {
            Start-Tools
        }
        "prod" {
            Start-Production
        }
        "stop" {
            Stop-All
        }
        "restart" {
            Restart-Services
        }
        "logs" {
            View-Logs
        }
        "status" {
            Get-Status
        }
        "health" {
            Test-Health
        }
        "build" {
            Build-AndStart
        }
        "cleanup" {
            Remove-All
        }
        "help" {
            Show-Help
        }
        default {
            Write-Error "Unknown command: $Command"
            Show-Help
            exit 1
        }
    }
}

# Run main function
Main
