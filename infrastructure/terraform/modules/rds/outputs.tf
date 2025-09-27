# RDS Module Outputs

output "db_instance_id" {
  description = "RDS instance ID"
  value       = aws_db_instance.main.id
}

output "db_instance_arn" {
  description = "RDS instance ARN"
  value       = aws_db_instance.main.arn
}

output "db_instance_endpoint" {
  description = "RDS instance endpoint"
  value       = aws_db_instance.main.endpoint
}

output "db_instance_port" {
  description = "RDS instance port"
  value       = aws_db_instance.main.port
}

output "db_instance_name" {
  description = "RDS instance database name"
  value       = aws_db_instance.main.db_name
}

output "db_instance_username" {
  description = "RDS instance root username"
  value       = aws_db_instance.main.username
}

output "db_instance_address" {
  description = "RDS instance hostname"
  value       = aws_db_instance.main.address
}

output "db_instance_availability_zone" {
  description = "RDS instance availability zone"
  value       = aws_db_instance.main.availability_zone
}

output "db_instance_backup_retention_period" {
  description = "RDS instance backup retention period"
  value       = aws_db_instance.main.backup_retention_period
}

output "db_instance_backup_window" {
  description = "RDS instance backup window"
  value       = aws_db_instance.main.backup_window
}

output "db_instance_maintenance_window" {
  description = "RDS instance maintenance window"
  value       = aws_db_instance.main.maintenance_window
}

output "db_instance_multi_az" {
  description = "RDS instance multi AZ configuration"
  value       = aws_db_instance.main.multi_az
}

output "db_instance_storage_encrypted" {
  description = "RDS instance storage encryption status"
  value       = aws_db_instance.main.storage_encrypted
}

output "db_instance_performance_insights_enabled" {
  description = "RDS instance Performance Insights status"
  value       = aws_db_instance.main.performance_insights_enabled
}

output "security_group_id" {
  description = "Security group ID for RDS"
  value       = aws_security_group.rds.id
}

output "security_group_arn" {
  description = "Security group ARN for RDS"
  value       = aws_security_group.rds.arn
}

output "db_subnet_group_name" {
  description = "DB subnet group name"
  value       = aws_db_subnet_group.main.name
}

output "db_subnet_group_arn" {
  description = "DB subnet group ARN"
  value       = aws_db_subnet_group.main.arn
}

output "db_parameter_group_name" {
  description = "DB parameter group name"
  value       = aws_db_parameter_group.main.name
}

output "db_parameter_group_arn" {
  description = "DB parameter group ARN"
  value       = aws_db_parameter_group.main.arn
}

output "db_option_group_name" {
  description = "DB option group name"
  value       = aws_db_option_group.main.name
}

output "db_option_group_arn" {
  description = "DB option group ARN"
  value       = aws_db_option_group.main.arn
}

output "secrets_manager_secret_arn" {
  description = "Secrets Manager secret ARN for database password"
  value       = aws_secretsmanager_secret.db_password.arn
}

output "secrets_manager_secret_name" {
  description = "Secrets Manager secret name for database password"
  value       = aws_secretsmanager_secret.db_password.name
}

output "cloudwatch_log_group_name" {
  description = "CloudWatch log group name for PostgreSQL"
  value       = aws_cloudwatch_log_group.postgresql.name
}

output "cloudwatch_log_group_arn" {
  description = "CloudWatch log group ARN for PostgreSQL"
  value       = aws_cloudwatch_log_group.postgresql.arn
}

output "enhanced_monitoring_iam_role_arn" {
  description = "IAM role ARN for enhanced monitoring"
  value       = var.enable_detailed_monitoring ? aws_iam_role.rds_enhanced_monitoring[0].arn : null
}

output "enhanced_monitoring_iam_role_name" {
  description = "IAM role name for enhanced monitoring"
  value       = var.enable_detailed_monitoring ? aws_iam_role.rds_enhanced_monitoring[0].name : null
}
