#!/bin/bash
# Sterling Auctions AWS Infrastructure Deployment Script

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if required tools are installed
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check AWS CLI
    if ! command -v aws &> /dev/null; then
        print_error "AWS CLI is not installed. Please install it first."
        exit 1
    fi
    
    # Check Terraform
    if ! command -v terraform &> /dev/null; then
        print_error "Terraform is not installed. Please install it first."
        exit 1
    fi
    
    # Check AWS credentials
    if ! aws sts get-caller-identity &> /dev/null; then
        print_error "AWS credentials not configured. Please run 'aws configure' first."
        exit 1
    fi
    
    print_success "All prerequisites are met!"
}

# Function to validate environment
validate_environment() {
    local env=$1
    
    if [[ ! "$env" =~ ^(dev|staging|production)$ ]]; then
        print_error "Invalid environment: $env. Must be one of: dev, staging, production"
        exit 1
    fi
    
    print_success "Environment '$env' is valid"
}

# Function to create S3 bucket for Terraform state
create_terraform_state_bucket() {
    local env=$1
    local bucket_name="sterling-auctions-terraform-state-${env}"
    
    print_status "Creating S3 bucket for Terraform state: $bucket_name"
    
    # Check if bucket already exists
    if aws s3 ls "s3://$bucket_name" 2>&1 | grep -q 'NoSuchBucket'; then
        aws s3 mb "s3://$bucket_name" --region us-east-1
        aws s3api put-bucket-versioning --bucket "$bucket_name" --versioning-configuration Status=Enabled
        aws s3api put-bucket-encryption --bucket "$bucket_name" --server-side-encryption-configuration '{
            "Rules": [{
                "ApplyServerSideEncryptionByDefault": {
                    "SSEAlgorithm": "AES256"
                }
            }]
        }'
        print_success "S3 bucket '$bucket_name' created successfully"
    else
        print_warning "S3 bucket '$bucket_name' already exists"
    fi
}

# Function to create DynamoDB table for Terraform state locking
create_terraform_lock_table() {
    local table_name="sterling-auctions-terraform-locks"
    
    print_status "Creating DynamoDB table for Terraform state locking: $table_name"
    
    # Check if table already exists
    if ! aws dynamodb describe-table --table-name "$table_name" &> /dev/null; then
        aws dynamodb create-table \
            --table-name "$table_name" \
            --attribute-definitions AttributeName=LockID,AttributeType=S \
            --key-schema AttributeName=LockID,KeyType=HASH \
            --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5
        
        print_success "DynamoDB table '$table_name' created successfully"
    else
        print_warning "DynamoDB table '$table_name' already exists"
    fi
}

# Function to initialize Terraform
init_terraform() {
    local env=$1
    
    print_status "Initializing Terraform..."
    
    terraform init \
        -backend-config="bucket=sterling-auctions-terraform-state-${env}" \
        -backend-config="key=infrastructure/terraform.tfstate" \
        -backend-config="region=us-east-1" \
        -backend-config="dynamodb_table=sterling-auctions-terraform-locks"
    
    print_success "Terraform initialized successfully"
}

# Function to plan Terraform deployment
plan_terraform() {
    local env=$1
    
    print_status "Planning Terraform deployment for environment: $env"
    
    terraform plan \
        -var-file="environments/${env}.tfvars" \
        -out="terraform-${env}.plan"
    
    print_success "Terraform plan completed successfully"
}

# Function to apply Terraform deployment
apply_terraform() {
    local env=$1
    
    print_status "Applying Terraform deployment for environment: $env"
    
    terraform apply "terraform-${env}.plan"
    
    print_success "Terraform deployment completed successfully"
}

# Function to show deployment outputs
show_outputs() {
    print_status "Deployment outputs:"
    terraform output
}

# Function to cleanup
cleanup() {
    local env=$1
    
    print_status "Cleaning up temporary files..."
    rm -f "terraform-${env}.plan"
    print_success "Cleanup completed"
}

# Main deployment function
deploy() {
    local env=$1
    local action=${2:-"deploy"}
    
    print_status "Starting Sterling Auctions AWS infrastructure deployment"
    print_status "Environment: $env"
    print_status "Action: $action"
    
    # Change to terraform directory
    cd "$(dirname "$0")/terraform"
    
    case $action in
        "init")
            check_prerequisites
            validate_environment "$env"
            create_terraform_state_bucket "$env"
            create_terraform_lock_table
            init_terraform "$env"
            ;;
        "plan")
            check_prerequisites
            validate_environment "$env"
            init_terraform "$env"
            plan_terraform "$env"
            ;;
        "apply")
            check_prerequisites
            validate_environment "$env"
            init_terraform "$env"
            plan_terraform "$env"
            apply_terraform "$env"
            show_outputs
            cleanup "$env"
            ;;
        "destroy")
            print_warning "This will destroy all infrastructure for environment: $env"
            read -p "Are you sure? (yes/no): " confirm
            if [[ $confirm == "yes" ]]; then
                terraform destroy -var-file="environments/${env}.tfvars"
                print_success "Infrastructure destroyed successfully"
            else
                print_status "Destroy cancelled"
            fi
            ;;
        *)
            print_error "Invalid action: $action. Must be one of: init, plan, apply, destroy"
            exit 1
            ;;
    esac
    
    print_success "Operation completed successfully!"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 <environment> <action>"
    echo ""
    echo "Environments:"
    echo "  dev        - Development environment"
    echo "  staging    - Staging environment"
    echo "  production - Production environment"
    echo ""
    echo "Actions:"
    echo "  init       - Initialize Terraform and create state bucket"
    echo "  plan       - Plan Terraform deployment"
    echo "  apply      - Apply Terraform deployment"
    echo "  destroy    - Destroy infrastructure"
    echo ""
    echo "Examples:"
    echo "  $0 dev init"
    echo "  $0 staging plan"
    echo "  $0 production apply"
}

# Main script logic
if [[ $# -lt 2 ]]; then
    show_usage
    exit 1
fi

deploy "$1" "$2"
