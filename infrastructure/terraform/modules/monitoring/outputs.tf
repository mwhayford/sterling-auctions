# Monitoring Module Outputs

output "log_group_names" {
  description = "Names of all CloudWatch log groups"
  value = {
    application = aws_cloudwatch_log_group.application.name
    api         = aws_cloudwatch_log_group.api.name
    frontend    = aws_cloudwatch_log_group.frontend.name
    database    = aws_cloudwatch_log_group.database.name
    redis       = aws_cloudwatch_log_group.redis.name
    security    = aws_cloudwatch_log_group.security.name
    performance = aws_cloudwatch_log_group.performance.name
  }
}

output "log_group_arns" {
  description = "ARNs of all CloudWatch log groups"
  value = {
    application = aws_cloudwatch_log_group.application.arn
    api         = aws_cloudwatch_log_group.api.arn
    frontend    = aws_cloudwatch_log_group.frontend.arn
    database    = aws_cloudwatch_log_group.database.arn
    redis       = aws_cloudwatch_log_group.redis.arn
    security    = aws_cloudwatch_log_group.security.arn
    performance = aws_cloudwatch_log_group.performance.arn
  }
}

output "sns_topic_arns" {
  description = "ARNs of all SNS topics"
  value = {
    critical_alerts = aws_sns_topic.critical_alerts.arn
    warning_alerts  = aws_sns_topic.warning_alerts.arn
    info_alerts     = aws_sns_topic.info_alerts.arn
  }
}

output "sns_topic_names" {
  description = "Names of all SNS topics"
  value = {
    critical_alerts = aws_sns_topic.critical_alerts.name
    warning_alerts  = aws_sns_topic.warning_alerts.name
    info_alerts     = aws_sns_topic.info_alerts.name
  }
}

output "dashboard_urls" {
  description = "URLs of all CloudWatch dashboards"
  value = {
    overview   = "https://${data.aws_region.current.name}.console.aws.amazon.com/cloudwatch/home?region=${data.aws_region.current.name}#dashboards:name=${aws_cloudwatch_dashboard.overview.dashboard_name}"
    performance = "https://${data.aws_region.current.name}.console.aws.amazon.com/cloudwatch/home?region=${data.aws_region.current.name}#dashboards:name=${aws_cloudwatch_dashboard.performance.dashboard_name}"
    security   = "https://${data.aws_region.current.name}.console.aws.amazon.com/cloudwatch/home?region=${data.aws_region.current.name}#dashboards:name=${aws_cloudwatch_dashboard.security.dashboard_name}"
  }
}

output "alarm_names" {
  description = "Names of all CloudWatch alarms"
  value = {
    high_error_rate         = aws_cloudwatch_metric_alarm.high_error_rate.alarm_name
    high_response_time      = aws_cloudwatch_metric_alarm.high_response_time.alarm_name
    low_throughput          = aws_cloudwatch_metric_alarm.low_throughput.alarm_name
    failed_login_attempts   = aws_cloudwatch_metric_alarm.failed_login_attempts.alarm_name
    unauthorized_access     = aws_cloudwatch_metric_alarm.unauthorized_access.alarm_name
    ecs_cpu_high           = aws_cloudwatch_metric_alarm.ecs_cpu_high.alarm_name
    rds_cpu_high           = aws_cloudwatch_metric_alarm.rds_cpu_high.alarm_name
  }
}

output "query_definition_names" {
  description = "Names of CloudWatch Log Insights query definitions"
  value = {
    error_analysis     = aws_cloudwatch_query_definition.error_analysis.name
    performance_analysis = aws_cloudwatch_query_definition.performance_analysis.name
    security_analysis  = aws_cloudwatch_query_definition.security_analysis.name
  }
}

# Legacy outputs for backward compatibility
output "log_group_name" {
  description = "Name of the main CloudWatch log group (legacy)"
  value       = aws_cloudwatch_log_group.application.name
}

output "log_group_arn" {
  description = "ARN of the main CloudWatch log group (legacy)"
  value       = aws_cloudwatch_log_group.application.arn
}

output "sns_topic_arn" {
  description = "ARN of the main SNS topic (legacy)"
  value       = aws_sns_topic.critical_alerts.arn
}

output "dashboard_url" {
  description = "URL of the main CloudWatch dashboard (legacy)"
  value       = "https://${data.aws_region.current.name}.console.aws.amazon.com/cloudwatch/home?region=${data.aws_region.current.name}#dashboards:name=${aws_cloudwatch_dashboard.overview.dashboard_name}"
}
