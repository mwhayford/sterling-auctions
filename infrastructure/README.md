# Sterling Auctions AWS Infrastructure Documentation

## Overview

This directory contains the complete AWS infrastructure configuration for the Sterling Auctions application using Terraform. The infrastructure is designed to be scalable, secure, and cost-effective across multiple environments.

## Architecture

### Core Components

- **VPC**: Multi-AZ VPC with public, private, and database subnets
- **ECS**: Container orchestration with Fargate for API and frontend
- **RDS**: PostgreSQL database with Multi-AZ support
- **ElastiCache**: Redis cluster for caching and sessions
- **ALB**: Application Load Balancer with SSL termination
- **CloudFront**: CDN for static assets
- **S3**: Object storage for files and static assets
- **Route53**: DNS management and health checks
- **WAF**: Web Application Firewall for security
- **CloudWatch**: Monitoring, logging, and alerting
- **Secrets Manager**: Secure storage for sensitive data

### Network Architecture

```
Internet Gateway
       |
   Public Subnets (ALB, NAT Gateway)
       |
   Private Subnets (ECS Tasks)
       |
   Database Subnets (RDS, Redis)
```

## Directory Structure

```
infrastructure/
├── terraform/
│   ├── main.tf                 # Main Terraform configuration
│   ├── variables.tf           # Variable definitions
│   ├── outputs.tf             # Output definitions
│   ├── infrastructure.tf      # Module orchestration
│   ├── modules/               # Reusable Terraform modules
│   │   ├── vpc/               # VPC module
│   │   ├── rds/               # RDS module
│   │   ├── redis/             # ElastiCache module
│   │   ├── ecs/               # ECS module
│   │   ├── alb/               # ALB module
│   │   ├── s3/                # S3 module
│   │   ├── cloudfront/        # CloudFront module
│   │   ├── route53/           # Route53 module
│   │   ├── monitoring/        # CloudWatch module
│   │   ├── waf/               # WAF module
│   │   └── secrets/           # Secrets Manager module
│   └── environments/           # Environment-specific configurations
│       ├── dev.tfvars         # Development environment
│       ├── staging.tfvars    # Staging environment
│       └── production.tfvars # Production environment
├── deploy.sh                  # Bash deployment script
├── deploy.ps1                 # PowerShell deployment script
└── README.md                  # This documentation
```

## Prerequisites

### Required Tools

1. **AWS CLI** (v2.0+)
   ```bash
   # Install AWS CLI
   curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
   unzip awscliv2.zip
   sudo ./aws/install
   ```

2. **Terraform** (v1.0+)
   ```bash
   # Install Terraform
   wget https://releases.hashicorp.com/terraform/1.6.0/terraform_1.6.0_linux_amd64.zip
   unzip terraform_1.6.0_linux_amd64.zip
   sudo mv terraform /usr/local/bin/
   ```

3. **Docker** (for building images)
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sh get-docker.sh
   ```

### AWS Configuration

1. **Configure AWS Credentials**
   ```bash
   aws configure
   ```

2. **Required AWS Permissions**
   - EC2 (VPC, Security Groups, Instances)
   - ECS (Clusters, Services, Task Definitions)
   - RDS (Instances, Subnet Groups, Parameter Groups)
   - ElastiCache (Clusters, Subnet Groups, Parameter Groups)
   - S3 (Buckets, Objects)
   - CloudFront (Distributions)
   - Route53 (Hosted Zones, Records)
   - WAF (Web ACLs, Rules)
   - CloudWatch (Log Groups, Alarms, Dashboards)
   - Secrets Manager (Secrets)
   - IAM (Roles, Policies)

## Environment Configuration

### Development Environment

- **Purpose**: Local development and testing
- **Resources**: Minimal resources for cost optimization
- **Database**: db.t3.micro (20GB storage)
- **Redis**: cache.t3.micro (1 node)
- **ECS**: 1 task, 512 CPU, 1024 MB memory
- **Monitoring**: Basic monitoring only
- **Security**: Relaxed security for development

### Staging Environment

- **Purpose**: Pre-production testing and validation
- **Resources**: Production-like configuration
- **Database**: db.t3.small (50GB storage)
- **Redis**: cache.t3.small (1 node)
- **ECS**: 2 tasks, 1024 CPU, 2048 MB memory
- **Monitoring**: Enhanced monitoring enabled
- **Security**: Production-like security

### Production Environment

- **Purpose**: Live production environment
- **Resources**: High availability and performance
- **Database**: db.r5.large (100GB storage, Multi-AZ)
- **Redis**: cache.r5.large (2 nodes, Multi-AZ)
- **ECS**: 3+ tasks, 2048 CPU, 4096 MB memory
- **Monitoring**: Full monitoring and alerting
- **Security**: Maximum security with WAF

## Deployment

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/mwhayford/sterling-auctions.git
   cd sterling-auctions/infrastructure
   ```

2. **Initialize infrastructure**
   ```bash
   # Linux/Mac
   ./deploy.sh dev init
   
   # Windows PowerShell
   .\deploy.ps1 -Environment dev -Action init
   ```

3. **Plan deployment**
   ```bash
   # Linux/Mac
   ./deploy.sh dev plan
   
   # Windows PowerShell
   .\deploy.ps1 -Environment dev -Action plan
   ```

4. **Deploy infrastructure**
   ```bash
   # Linux/Mac
   ./deploy.sh dev apply
   
   # Windows PowerShell
   .\deploy.ps1 -Environment dev -Action apply
   ```

### Manual Deployment

1. **Create Terraform state bucket**
   ```bash
   aws s3 mb s3://sterling-auctions-terraform-state-dev --region us-east-1
   ```

2. **Create DynamoDB table for state locking**
   ```bash
   aws dynamodb create-table \
     --table-name sterling-auctions-terraform-locks \
     --attribute-definitions AttributeName=LockID,AttributeType=S \
     --key-schema AttributeName=LockID,KeyType=HASH \
     --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5
   ```

3. **Initialize Terraform**
   ```bash
   cd terraform
   terraform init
   ```

4. **Plan deployment**
   ```bash
   terraform plan -var-file="environments/dev.tfvars"
   ```

5. **Apply deployment**
   ```bash
   terraform apply -var-file="environments/dev.tfvars"
   ```

## Configuration

### Environment Variables

Key configuration variables that can be customized:

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `environment` | Environment name | - | `dev`, `staging`, `production` |
| `owner_email` | Infrastructure owner email | - | `admin@sterlingauctions.com` |
| `domain_name` | Application domain | `""` | `sterlingauctions.com` |
| `db_instance_class` | RDS instance type | `db.t3.micro` | `db.r5.large` |
| `redis_node_type` | Redis node type | `cache.t3.micro` | `cache.r5.large` |
| `ecs_cpu` | ECS task CPU | `512` | `2048` |
| `ecs_memory` | ECS task memory | `1024` | `4096` |

### Customization

1. **Modify environment files** (`environments/*.tfvars`)
2. **Update module configurations** (`modules/*/variables.tf`)
3. **Add new modules** for additional services
4. **Customize monitoring** and alerting rules

## Security

### Network Security

- **VPC**: Isolated network environment
- **Security Groups**: Restrictive ingress/egress rules
- **NAT Gateway**: Private subnet internet access
- **WAF**: Web application firewall protection

### Data Security

- **Encryption**: All data encrypted at rest and in transit
- **Secrets Manager**: Secure storage of sensitive data
- **IAM Roles**: Least privilege access
- **VPC Endpoints**: Private AWS service access

### Compliance

- **SOC 2**: Infrastructure designed for SOC 2 compliance
- **GDPR**: Data protection and privacy controls
- **PCI DSS**: Payment card industry compliance ready

## Monitoring

### CloudWatch Integration

- **Logs**: Centralized logging for all services
- **Metrics**: Performance and health metrics
- **Alarms**: Automated alerting for issues
- **Dashboards**: Visual monitoring interfaces

### Key Metrics

- **ECS**: CPU, memory, task count
- **RDS**: CPU, connections, storage
- **Redis**: CPU, memory, evictions
- **ALB**: Request count, response time, errors

### Alerting

- **Email**: Critical alerts via SNS
- **Slack**: Team notifications
- **PagerDuty**: On-call escalation

## Cost Optimization

### Resource Sizing

- **Development**: Minimal resources for cost
- **Staging**: Production-like for testing
- **Production**: Right-sized for performance

### Cost Controls

- **Spot Instances**: Use spot instances where appropriate
- **Scheduled Scaling**: Scale down during off-hours
- **Reserved Instances**: Long-term cost savings
- **Storage Optimization**: Appropriate storage classes

### Estimated Costs

| Environment | Monthly Cost (USD) | Notes |
|-------------|-------------------|-------|
| Development | $50-100 | Minimal resources |
| Staging | $200-400 | Production-like |
| Production | $500-2000 | High availability |

## Troubleshooting

### Common Issues

1. **Terraform State Lock**
   ```bash
   # Force unlock if needed
   terraform force-unlock <lock-id>
   ```

2. **AWS Credentials**
   ```bash
   # Check credentials
   aws sts get-caller-identity
   ```

3. **Resource Limits**
   ```bash
   # Check service limits
   aws service-quotas get-service-quota --service-code ec2 --quota-code L-F678F1CE
   ```

### Debugging

1. **Enable Terraform Debug**
   ```bash
   export TF_LOG=DEBUG
   terraform plan
   ```

2. **Check CloudWatch Logs**
   ```bash
   aws logs describe-log-groups --log-group-name-prefix "/aws/ecs"
   ```

3. **ECS Task Logs**
   ```bash
   aws ecs describe-tasks --cluster <cluster-name> --tasks <task-arn>
   ```

## Maintenance

### Regular Tasks

1. **Update AMIs**: Keep ECS instances updated
2. **Security Patches**: Apply OS and application patches
3. **Certificate Renewal**: Monitor SSL certificate expiration
4. **Backup Verification**: Test backup and restore procedures

### Scaling

1. **Horizontal Scaling**: Increase ECS task count
2. **Vertical Scaling**: Upgrade instance types
3. **Database Scaling**: RDS read replicas, ElastiCache clusters
4. **CDN Optimization**: CloudFront cache tuning

## Support

### Documentation

- **AWS Documentation**: https://docs.aws.amazon.com/
- **Terraform Documentation**: https://www.terraform.io/docs/
- **ECS Documentation**: https://docs.aws.amazon.com/ecs/

### Contact

- **Infrastructure Team**: infrastructure@sterlingauctions.com
- **DevOps Team**: devops@sterlingauctions.com
- **Emergency**: +1-XXX-XXX-XXXX

## License

This infrastructure configuration is part of the Sterling Auctions project and is licensed under the MIT License.
