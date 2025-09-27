# Production Environment Configuration
# terraform.tfvars for production environment

environment = "production"
owner_email = "production@sterlingauctions.com"

# Domain Configuration
domain_name = "sterlingauctions.com"
certificate_arn = "arn:aws:acm:us-east-1:123456789012:certificate/your-cert-id"

# Database Configuration
db_instance_class = "db.r5.large"
db_allocated_storage = 100
db_max_allocated_storage = 1000
db_engine_version = "15.4"

# Redis Configuration
redis_node_type = "cache.r5.large"
redis_num_cache_nodes = 2

# ECS Configuration
ecs_cpu = 2048
ecs_memory = 4096
ecs_desired_count = 3
ecs_min_capacity = 2
ecs_max_capacity = 20

# Application Configuration
app_port = 5000
frontend_port = 3000

# Monitoring Configuration
enable_detailed_monitoring = true
log_retention_days = 30

# Security Configuration
allowed_cidr_blocks = ["0.0.0.0/0"]
enable_waf = true

# Backup Configuration
backup_retention_days = 30
enable_automated_backups = true

# Cost Optimization
enable_spot_instances = false
enable_scheduled_scaling = true
