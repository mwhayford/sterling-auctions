# Sterling Auctions AWS Infrastructure
# Main configuration that orchestrates all modules

# VPC Module
module "vpc" {
  source = "./modules/vpc"

  name_prefix        = local.name_prefix
  vpc_cidr          = local.vpc_cidr
  public_subnets    = local.public_subnets
  private_subnets   = local.private_subnets
  database_subnets  = local.database_subnets
  availability_zones = data.aws_availability_zones.available.names

  tags = local.common_tags
}

# RDS Module
module "rds" {
  source = "./modules/rds"

  name_prefix              = local.name_prefix
  vpc_id                   = module.vpc.vpc_id
  subnet_ids               = module.vpc.database_subnets
  allowed_security_groups  = [module.ecs.security_group_id]
  instance_class           = var.db_instance_class
  allocated_storage        = var.db_allocated_storage
  max_allocated_storage    = var.db_max_allocated_storage
  engine_version           = var.db_engine_version
  backup_retention_days    = var.backup_retention_days
  enable_detailed_monitoring = var.enable_detailed_monitoring
  log_retention_days       = var.log_retention_days
  environment              = var.environment
  sns_topic_arn           = module.monitoring.sns_topic_arn

  tags = local.common_tags
}

# Redis Module
module "redis" {
  source = "./modules/redis"

  name_prefix              = local.name_prefix
  vpc_id                   = module.vpc.vpc_id
  subnet_ids               = module.vpc.private_subnets
  allowed_security_groups  = [module.ecs.security_group_id]
  node_type                = var.redis_node_type
  num_cache_nodes          = var.redis_num_cache_nodes
  backup_retention_days    = var.backup_retention_days
  log_retention_days       = var.log_retention_days
  environment              = var.environment
  sns_topic_arn           = module.monitoring.sns_topic_arn

  tags = local.common_tags
}

# ECS Module
module "ecs" {
  source = "./modules/ecs"

  name_prefix                = local.name_prefix
  vpc_id                     = module.vpc.vpc_id
  vpc_cidr                   = local.vpc_cidr
  subnet_ids                 = module.vpc.private_subnets
  target_group_arn           = module.alb.target_group_arn
  frontend_target_group_arn  = module.alb.frontend_target_group_arn
  alb_security_group_id      = module.alb.security_group_id
  app_port                   = var.app_port
  frontend_port              = var.frontend_port
  cpu                        = var.ecs_cpu
  memory                     = var.ecs_memory
  desired_count              = var.ecs_desired_count
  min_capacity               = var.ecs_min_capacity
  max_capacity               = var.ecs_max_capacity
  api_image                  = "ghcr.io/mwhayford/sterling-auctions/api:latest"
  frontend_image             = "ghcr.io/mwhayford/sterling-auctions/frontend:latest"
  api_url                    = var.domain_name != "" ? "https://api.${var.domain_name}" : "https://${module.alb.lb_dns_name}/api"
  db_secret_arn              = module.rds.secrets_manager_secret_arn
  redis_secret_arn           = module.redis.secrets_manager_secret_arn
  s3_bucket_arn              = module.s3.bucket_arn
  log_retention_days         = var.log_retention_days
  environment                = var.environment
  sns_topic_arn             = module.monitoring.sns_topic_arn

  tags = local.common_tags
}

# Application Load Balancer Module
module "alb" {
  source = "./modules/alb"

  name_prefix        = local.name_prefix
  vpc_id             = module.vpc.vpc_id
  subnet_ids         = module.vpc.public_subnets
  certificate_arn    = var.certificate_arn
  domain_name        = var.domain_name
  app_port           = var.app_port
  frontend_port      = var.frontend_port
  allowed_cidr_blocks = var.allowed_cidr_blocks
  enable_waf         = var.enable_waf

  tags = local.common_tags
}

# S3 Module
module "s3" {
  source = "./modules/s3"

  name_prefix = local.name_prefix
  environment = var.environment

  tags = local.common_tags
}

# CloudFront Module
module "cloudfront" {
  source = "./modules/cloudfront"

  name_prefix     = local.name_prefix
  domain_name     = var.domain_name
  certificate_arn = var.certificate_arn
  s3_bucket_name  = module.s3.bucket_name
  s3_bucket_arn   = module.s3.bucket_arn

  tags = local.common_tags
}

# Route53 Module
module "route53" {
  source = "./modules/route53"

  domain_name           = var.domain_name
  certificate_arn       = var.certificate_arn
  alb_dns_name         = module.alb.lb_dns_name
  alb_zone_id          = module.alb.lb_zone_id
  cloudfront_domain_name = module.cloudfront.domain_name
  cloudfront_zone_id   = module.cloudfront.hosted_zone_id

  tags = local.common_tags
}

# Monitoring Module
module "monitoring" {
  source = "./modules/monitoring"

  name_prefix        = local.name_prefix
  environment        = var.environment
  log_retention_days = var.log_retention_days
  owner_email        = var.owner_email

  tags = local.common_tags
}

# WAF Module
module "waf" {
  source = "./modules/waf"

  name_prefix = local.name_prefix
  alb_arn     = module.alb.lb_arn
  enable_waf  = var.enable_waf

  tags = local.common_tags
}

# Secrets Manager Module
module "secrets" {
  source = "./modules/secrets"

  name_prefix = local.name_prefix
  environment = var.environment

  tags = local.common_tags
}
