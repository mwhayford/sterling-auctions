# ElastiCache Redis Module Outputs

output "replication_group_id" {
  description = "Redis replication group ID"
  value       = aws_elasticache_replication_group.main.id
}

output "replication_group_arn" {
  description = "Redis replication group ARN"
  value       = aws_elasticache_replication_group.main.arn
}

output "primary_endpoint_address" {
  description = "Redis primary endpoint address"
  value       = aws_elasticache_replication_group.main.primary_endpoint_address
}

output "reader_endpoint_address" {
  description = "Redis reader endpoint address"
  value       = aws_elasticache_replication_group.main.reader_endpoint_address
}

output "port" {
  description = "Redis port"
  value       = aws_elasticache_replication_group.main.port
}

output "configuration_endpoint_address" {
  description = "Redis configuration endpoint address"
  value       = aws_elasticache_replication_group.main.configuration_endpoint_address
}

output "cluster_enabled" {
  description = "Redis cluster mode enabled"
  value       = aws_elasticache_replication_group.main.cluster_enabled
}

output "automatic_failover_enabled" {
  description = "Redis automatic failover enabled"
  value       = aws_elasticache_replication_group.main.automatic_failover_enabled
}

output "multi_az_enabled" {
  description = "Redis multi-AZ enabled"
  value       = aws_elasticache_replication_group.main.multi_az_enabled
}

output "at_rest_encryption_enabled" {
  description = "Redis at-rest encryption enabled"
  value       = aws_elasticache_replication_group.main.at_rest_encryption_enabled
}

output "transit_encryption_enabled" {
  description = "Redis transit encryption enabled"
  value       = aws_elasticache_replication_group.main.transit_encryption_enabled
}

output "auth_token_enabled" {
  description = "Redis auth token enabled"
  value       = aws_elasticache_replication_group.main.auth_token_enabled
}

output "snapshot_retention_limit" {
  description = "Redis snapshot retention limit"
  value       = aws_elasticache_replication_group.main.snapshot_retention_limit
}

output "snapshot_window" {
  description = "Redis snapshot window"
  value       = aws_elasticache_replication_group.main.snapshot_window
}

output "maintenance_window" {
  description = "Redis maintenance window"
  value       = aws_elasticache_replication_group.main.maintenance_window
}

output "security_group_id" {
  description = "Security group ID for Redis"
  value       = aws_security_group.redis.id
}

output "security_group_arn" {
  description = "Security group ARN for Redis"
  value       = aws_security_group.redis.arn
}

output "subnet_group_name" {
  description = "Redis subnet group name"
  value       = aws_elasticache_subnet_group.main.name
}

output "parameter_group_name" {
  description = "Redis parameter group name"
  value       = aws_elasticache_parameter_group.main.name
}

output "secrets_manager_secret_arn" {
  description = "Secrets Manager secret ARN for Redis auth token"
  value       = var.environment == "production" ? aws_secretsmanager_secret.redis_auth_token[0].arn : null
}

output "secrets_manager_secret_name" {
  description = "Secrets Manager secret name for Redis auth token"
  value       = var.environment == "production" ? aws_secretsmanager_secret.redis_auth_token[0].name : null
}

output "cloudwatch_log_group_name" {
  description = "CloudWatch log group name for Redis"
  value       = aws_cloudwatch_log_group.redis.name
}

output "cloudwatch_log_group_arn" {
  description = "CloudWatch log group ARN for Redis"
  value       = aws_cloudwatch_log_group.redis.arn
}
