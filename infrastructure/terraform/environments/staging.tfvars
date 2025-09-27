# Staging Environment Configuration
# terraform.tfvars for staging environment

environment = "staging"
owner_email = "staging@sterlingauctions.com"

# Domain Configuration
domain_name = "staging.sterlingauctions.com"
certificate_arn = "arn:aws:acm:us-east-1:123456789012:certificate/your-cert-id"

# Database Configuration
db_instance_class = "db.t3.small"
db_allocated_storage = 50
db_max_allocated_storage = 200
db_engine_version = "15.4"

# Redis Configuration
redis_node_type = "cache.t3.small"
redis_num_cache_nodes = 1

# ECS Configuration
ecs_cpu = 1024
ecs_memory = 2048
ecs_desired_count = 2
ecs_min_capacity = 1
ecs_max_capacity = 5

# Application Configuration
app_port = 5000
frontend_port = 3000

# Monitoring Configuration
enable_detailed_monitoring = true
log_retention_days = 14

# Security Configuration
allowed_cidr_blocks = ["0.0.0.0/0"]
enable_waf = true

# Backup Configuration
backup_retention_days = 7
enable_automated_backups = true

# Cost Optimization
enable_spot_instances = false
enable_scheduled_scaling = true
