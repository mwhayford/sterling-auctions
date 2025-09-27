# Development Environment Configuration
# terraform.tfvars for development environment

environment = "dev"
owner_email = "dev@sterlingauctions.com"

# Domain Configuration
domain_name = ""
certificate_arn = ""

# Database Configuration
db_instance_class = "db.t3.micro"
db_allocated_storage = 20
db_max_allocated_storage = 100
db_engine_version = "15.4"

# Redis Configuration
redis_node_type = "cache.t3.micro"
redis_num_cache_nodes = 1

# ECS Configuration
ecs_cpu = 512
ecs_memory = 1024
ecs_desired_count = 1
ecs_min_capacity = 1
ecs_max_capacity = 3

# Application Configuration
app_port = 5000
frontend_port = 3000

# Monitoring Configuration
enable_detailed_monitoring = false
log_retention_days = 7

# Security Configuration
allowed_cidr_blocks = ["0.0.0.0/0"]
enable_waf = false

# Backup Configuration
backup_retention_days = 3
enable_automated_backups = true

# Cost Optimization
enable_spot_instances = true
enable_scheduled_scaling = false
