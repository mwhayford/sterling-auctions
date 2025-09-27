# Sterling Auctions AWS Infrastructure Deployment Script (PowerShell)

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("dev", "staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("init", "plan", "apply", "destroy")]
    [string]$Action
)

# Colors for output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-Status "Checking prerequisites..."
    
    # Check AWS CLI
    try {
        aws --version | Out-Null
    }
    catch {
        Write-Error "AWS CLI is not installed. Please install it first."
        exit 1
    }
    
    # Check Terraform
    try {
        terraform --version | Out-Null
    }
    catch {
        Write-Error "Terraform is not installed. Please install it first."
        exit 1
    }
    
    # Check AWS credentials
    try {
        aws sts get-caller-identity | Out-Null
    }
    catch {
        Write-Error "AWS credentials not configured. Please run 'aws configure' first."
        exit 1
    }
    
    Write-Success "All prerequisites are met!"
}

# Function to create S3 bucket for Terraform state
function New-TerraformStateBucket {
    param([string]$Env)
    
    $BucketName = "sterling-auctions-terraform-state-$Env"
    
    Write-Status "Creating S3 bucket for Terraform state: $BucketName"
    
    # Check if bucket already exists
    try {
        aws s3 ls "s3://$BucketName" | Out-Null
        Write-Warning "S3 bucket '$BucketName' already exists"
    }
    catch {
        aws s3 mb "s3://$BucketName" --region us-east-1
        aws s3api put-bucket-versioning --bucket "$BucketName" --versioning-configuration Status=Enabled
        aws s3api put-bucket-encryption --bucket "$BucketName" --server-side-encryption-configuration '{
            "Rules": [{
                "ApplyServerSideEncryptionByDefault": {
                    "SSEAlgorithm": "AES256"
                }
            }]
        }'
        Write-Success "S3 bucket '$BucketName' created successfully"
    }
}

# Function to create DynamoDB table for Terraform state locking
function New-TerraformLockTable {
    $TableName = "sterling-auctions-terraform-locks"
    
    Write-Status "Creating DynamoDB table for Terraform state locking: $TableName"
    
    # Check if table already exists
    try {
        aws dynamodb describe-table --table-name "$TableName" | Out-Null
        Write-Warning "DynamoDB table '$TableName' already exists"
    }
    catch {
        aws dynamodb create-table `
            --table-name "$TableName" `
            --attribute-definitions AttributeName=LockID,AttributeType=S `
            --key-schema AttributeName=LockID,KeyType=HASH `
            --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5
        
        Write-Success "DynamoDB table '$TableName' created successfully"
    }
}

# Function to initialize Terraform
function Initialize-Terraform {
    param([string]$Env)
    
    Write-Status "Initializing Terraform..."
    
    terraform init `
        -backend-config="bucket=sterling-auctions-terraform-state-$Env" `
        -backend-config="key=infrastructure/terraform.tfstate" `
        -backend-config="region=us-east-1" `
        -backend-config="dynamodb_table=sterling-auctions-terraform-locks"
    
    Write-Success "Terraform initialized successfully"
}

# Function to plan Terraform deployment
function Plan-Terraform {
    param([string]$Env)
    
    Write-Status "Planning Terraform deployment for environment: $Env"
    
    terraform plan `
        -var-file="environments/$Env.tfvars" `
        -out="terraform-$Env.plan"
    
    Write-Success "Terraform plan completed successfully"
}

# Function to apply Terraform deployment
function Apply-Terraform {
    param([string]$Env)
    
    Write-Status "Applying Terraform deployment for environment: $Env"
    
    terraform apply "terraform-$Env.plan"
    
    Write-Success "Terraform deployment completed successfully"
}

# Function to show deployment outputs
function Show-Outputs {
    Write-Status "Deployment outputs:"
    terraform output
}

# Function to cleanup
function Remove-TempFiles {
    param([string]$Env)
    
    Write-Status "Cleaning up temporary files..."
    Remove-Item "terraform-$Env.plan" -ErrorAction SilentlyContinue
    Write-Success "Cleanup completed"
}

# Main deployment function
function Deploy-Infrastructure {
    param([string]$Env, [string]$Action)
    
    Write-Status "Starting Sterling Auctions AWS infrastructure deployment"
    Write-Status "Environment: $Env"
    Write-Status "Action: $Action"
    
    # Change to terraform directory
    Set-Location "$PSScriptRoot/terraform"
    
    switch ($Action) {
        "init" {
            Test-Prerequisites
            New-TerraformStateBucket $Env
            New-TerraformLockTable
            Initialize-Terraform $Env
        }
        "plan" {
            Test-Prerequisites
            Initialize-Terraform $Env
            Plan-Terraform $Env
        }
        "apply" {
            Test-Prerequisites
            Initialize-Terraform $Env
            Plan-Terraform $Env
            Apply-Terraform $Env
            Show-Outputs
            Remove-TempFiles $Env
        }
        "destroy" {
            Write-Warning "This will destroy all infrastructure for environment: $Env"
            $Confirm = Read-Host "Are you sure? (yes/no)"
            if ($Confirm -eq "yes") {
                terraform destroy -var-file="environments/$Env.tfvars"
                Write-Success "Infrastructure destroyed successfully"
            } else {
                Write-Status "Destroy cancelled"
            }
        }
    }
    
    Write-Success "Operation completed successfully!"
}

# Show usage information
function Show-Usage {
    Write-Host "Usage: .\deploy.ps1 -Environment <environment> -Action <action>"
    Write-Host ""
    Write-Host "Environments:"
    Write-Host "  dev        - Development environment"
    Write-Host "  staging    - Staging environment"
    Write-Host "  production - Production environment"
    Write-Host ""
    Write-Host "Actions:"
    Write-Host "  init       - Initialize Terraform and create state bucket"
    Write-Host "  plan       - Plan Terraform deployment"
    Write-Host "  apply      - Apply Terraform deployment"
    Write-Host "  destroy    - Destroy infrastructure"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\deploy.ps1 -Environment dev -Action init"
    Write-Host "  .\deploy.ps1 -Environment staging -Action plan"
    Write-Host "  .\deploy.ps1 -Environment production -Action apply"
}

# Main script execution
try {
    Deploy-Infrastructure $Environment $Action
}
catch {
    Write-Error "Deployment failed: $($_.Exception.Message)"
    exit 1
}
