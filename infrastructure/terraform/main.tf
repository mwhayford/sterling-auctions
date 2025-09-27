# Sterling Auctions AWS Infrastructure
# Main Terraform configuration

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
  
  backend "s3" {
    bucket         = "sterling-auctions-terraform-state"
    key            = "infrastructure/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "sterling-auctions-terraform-locks"
  }
}

provider "aws" {
  region = var.aws_region
  
  default_tags {
    tags = {
      Project     = "SterlingAuctions"
      Environment = var.environment
      ManagedBy   = "Terraform"
      Owner       = var.owner_email
    }
  }
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

# Local values
locals {
  common_tags = {
    Project     = "SterlingAuctions"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Owner       = var.owner_email
  }
  
  name_prefix = "${var.project_name}-${var.environment}"
  
  # VPC Configuration
  vpc_cidr = var.environment == "production" ? "10.0.0.0/16" : "10.1.0.0/16"
  
  # Subnet Configuration
  public_subnets = var.environment == "production" ? 
    ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"] : 
    ["10.1.1.0/24", "10.1.2.0/24"]
  
  private_subnets = var.environment == "production" ? 
    ["10.0.10.0/24", "10.0.20.0/24", "10.0.30.0/24"] : 
    ["10.1.10.0/24", "10.1.20.0/24"]
  
  database_subnets = var.environment == "production" ? 
    ["10.0.100.0/24", "10.0.200.0/24", "10.0.300.0/24"] : 
    ["10.1.100.0/24", "10.1.200.0/24"]
}
