# Monitoring Module for Sterling Auctions
# Creates comprehensive CloudWatch logs, alarms, dashboards, and SNS topics

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# CloudWatch Log Groups for different services
resource "aws_cloudwatch_log_group" "application" {
  name              = "/aws/sterling-auctions/${var.environment}/application"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "api" {
  name              = "/aws/sterling-auctions/${var.environment}/api"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "frontend" {
  name              = "/aws/sterling-auctions/${var.environment}/frontend"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "database" {
  name              = "/aws/sterling-auctions/${var.environment}/database"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "redis" {
  name              = "/aws/sterling-auctions/${var.environment}/redis"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "security" {
  name              = "/aws/sterling-auctions/${var.environment}/security"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

resource "aws_cloudwatch_log_group" "performance" {
  name              = "/aws/sterling-auctions/${var.environment}/performance"
  retention_in_days = var.log_retention_days

  tags = var.tags
}

# SNS Topics for different alert types
resource "aws_sns_topic" "critical_alerts" {
  name = "${var.name_prefix}-critical-alerts"

  tags = var.tags
}

resource "aws_sns_topic" "warning_alerts" {
  name = "${var.name_prefix}-warning-alerts"

  tags = var.tags
}

resource "aws_sns_topic" "info_alerts" {
  name = "${var.name_prefix}-info-alerts"

  tags = var.tags
}

# SNS Topic Subscriptions
resource "aws_sns_topic_subscription" "critical_email" {
  count     = var.owner_email != "" ? 1 : 0
  topic_arn = aws_sns_topic.critical_alerts.arn
  protocol  = "email"
  endpoint  = var.owner_email
}

resource "aws_sns_topic_subscription" "warning_email" {
  count     = var.owner_email != "" ? 1 : 0
  topic_arn = aws_sns_topic.warning_alerts.arn
  protocol  = "email"
  endpoint  = var.owner_email
}

# CloudWatch Dashboard - Overview
resource "aws_cloudwatch_dashboard" "overview" {
  dashboard_name = "${var.name_prefix}-overview"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "metric"
        x      = 0
        y      = 0
        width  = 6
        height = 6

        properties = {
          metrics = [
            ["AWS/ECS", "CPUUtilization", "ServiceName", "${var.name_prefix}-service", "ClusterName", "${var.name_prefix}-cluster"],
            [".", "MemoryUtilization", ".", ".", ".", "."]
          ]
          view    = "timeSeries"
          stacked = false
          region  = data.aws_region.current.name
          title   = "ECS Service Metrics"
          period  = 300
        }
      },
      {
        type   = "metric"
        x      = 6
        y      = 0
        width  = 6
        height = 6

        properties = {
          metrics = [
            ["AWS/RDS", "CPUUtilization", "DBInstanceIdentifier", "${var.name_prefix}-db"],
            [".", "DatabaseConnections", ".", "."]
          ]
          view    = "timeSeries"
          stacked = false
          region  = data.aws_region.current.name
          title   = "RDS Metrics"
          period  = 300
        }
      },
      {
        type   = "metric"
        x      = 0
        y      = 6
        width  = 6
        height = 6

        properties = {
          metrics = [
            ["AWS/ElastiCache", "CPUUtilization", "CacheClusterId", "${var.name_prefix}-redis"],
            [".", "DatabaseMemoryUsagePercentage", ".", "."]
          ]
          view    = "timeSeries"
          stacked = false
          region  = data.aws_region.current.name
          title   = "Redis Metrics"
          period  = 300
        }
      },
      {
        type   = "metric"
        x      = 6
        y      = 6
        width  = 6
        height = 6

        properties = {
          metrics = [
            ["AWS/ApplicationELB", "RequestCount", "LoadBalancer", "${var.name_prefix}-alb"],
            [".", "TargetResponseTime", ".", "."],
            [".", "HTTPCode_Target_5XX_Count", ".", "."]
          ]
          view    = "timeSeries"
          stacked = false
          region  = data.aws_region.current.name
          title   = "Load Balancer Metrics"
          period  = 300
        }
      }
    ]
  })

  tags = var.tags
}

# CloudWatch Dashboard - Application Performance
resource "aws_cloudwatch_dashboard" "performance" {
  dashboard_name = "${var.name_prefix}-performance"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "log"
        x      = 0
        y      = 0
        width  = 24
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/application' | fields @timestamp, @message | filter @message like /ERROR/ | sort @timestamp desc | limit 20"
          region  = data.aws_region.current.name
          title   = "Recent Errors"
          view    = "table"
        }
      },
      {
        type   = "log"
        x      = 0
        y      = 6
        width  = 12
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/api' | fields @timestamp, @message | filter @message like /Request/ | stats count() by bin(5m)"
          region  = data.aws_region.current.name
          title   = "API Request Rate"
          view    = "timeSeries"
        }
      },
      {
        type   = "log"
        x      = 12
        y      = 6
        width  = 12
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/api' | fields @timestamp, @message | filter @message like /Response/ | stats avg(responseTime) by bin(5m)"
          region  = data.aws_region.current.name
          title   = "Average Response Time"
          view    = "timeSeries"
        }
      }
    ]
  })

  tags = var.tags
}

# CloudWatch Dashboard - Security
resource "aws_cloudwatch_dashboard" "security" {
  dashboard_name = "${var.name_prefix}-security"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "log"
        x      = 0
        y      = 0
        width  = 24
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/security' | fields @timestamp, @message | filter @message like /FAILED/ | sort @timestamp desc | limit 20"
          region  = data.aws_region.current.name
          title   = "Security Events"
          view    = "table"
        }
      },
      {
        type   = "log"
        x      = 0
        y      = 6
        width  = 12
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/security' | fields @timestamp, @message | filter @message like /LOGIN/ | stats count() by bin(1h)"
          region  = data.aws_region.current.name
          title   = "Login Attempts"
          view    = "timeSeries"
        }
      },
      {
        type   = "log"
        x      = 12
        y      = 6
        width  = 12
        height = 6

        properties = {
          query   = "SOURCE '/aws/sterling-auctions/${var.environment}/security' | fields @timestamp, @message | filter @message like /UNAUTHORIZED/ | stats count() by bin(1h)"
          region  = data.aws_region.current.name
          title   = "Unauthorized Access Attempts"
          view    = "timeSeries"
        }
      }
    ]
  })

  tags = var.tags
}

# CloudWatch Alarms - Critical
resource "aws_cloudwatch_metric_alarm" "high_error_rate" {
  alarm_name          = "${var.name_prefix}-high-error-rate"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "ErrorRate"
  namespace           = "SterlingAuctions/Application"
  period              = "300"
  statistic           = "Average"
  threshold           = "5"
  alarm_description   = "High error rate detected"
  alarm_actions       = [aws_sns_topic.critical_alerts.arn]
  ok_actions          = [aws_sns_topic.critical_alerts.arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "high_response_time" {
  alarm_name          = "${var.name_prefix}-high-response-time"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "3"
  metric_name         = "ResponseTime"
  namespace           = "SterlingAuctions/Application"
  period              = "300"
  statistic           = "Average"
  threshold           = "2000"
  alarm_description   = "High response time detected"
  alarm_actions       = [aws_sns_topic.warning_alerts.arn]
  ok_actions          = [aws_sns_topic.warning_alerts.arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "low_throughput" {
  alarm_name          = "${var.name_prefix}-low-throughput"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "Throughput"
  namespace           = "SterlingAuctions/Application"
  period              = "300"
  statistic           = "Sum"
  threshold           = "10"
  alarm_description   = "Low throughput detected"
  alarm_actions       = [aws_sns_topic.warning_alerts.arn]
  ok_actions          = [aws_sns_topic.warning_alerts.arn]

  tags = var.tags
}

# CloudWatch Alarms - Security
resource "aws_cloudwatch_metric_alarm" "failed_login_attempts" {
  alarm_name          = "${var.name_prefix}-failed-login-attempts"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "1"
  metric_name         = "FailedLoginAttempts"
  namespace           = "SterlingAuctions/Security"
  period              = "300"
  statistic           = "Sum"
  threshold           = "10"
  alarm_description   = "High number of failed login attempts"
  alarm_actions       = [aws_sns_topic.critical_alerts.arn]
  ok_actions          = [aws_sns_topic.critical_alerts.arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "unauthorized_access" {
  alarm_name          = "${var.name_prefix}-unauthorized-access"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "1"
  metric_name         = "UnauthorizedAccess"
  namespace           = "SterlingAuctions/Security"
  period              = "300"
  statistic           = "Sum"
  threshold           = "5"
  alarm_description   = "Unauthorized access attempts detected"
  alarm_actions       = [aws_sns_topic.critical_alerts.arn]
  ok_actions          = [aws_sns_topic.critical_alerts.arn]

  tags = var.tags
}

# CloudWatch Alarms - Infrastructure
resource "aws_cloudwatch_metric_alarm" "ecs_cpu_high" {
  alarm_name          = "${var.name_prefix}-ecs-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = "300"
  statistic           = "Average"
  threshold           = "80"
  alarm_description   = "ECS CPU utilization is high"
  alarm_actions       = [aws_sns_topic.warning_alerts.arn]
  ok_actions          = [aws_sns_topic.warning_alerts.arn]

  dimensions = {
    ServiceName = "${var.name_prefix}-service"
    ClusterName = "${var.name_prefix}-cluster"
  }

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "rds_cpu_high" {
  alarm_name          = "${var.name_prefix}-rds-cpu-high"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/RDS"
  period              = "300"
  statistic           = "Average"
  threshold           = "80"
  alarm_description   = "RDS CPU utilization is high"
  alarm_actions       = [aws_sns_topic.warning_alerts.arn]
  ok_actions          = [aws_sns_topic.warning_alerts.arn]

  dimensions = {
    DBInstanceIdentifier = "${var.name_prefix}-db"
  }

  tags = var.tags
}

# CloudWatch Log Insights Queries
resource "aws_cloudwatch_query_definition" "error_analysis" {
  name = "${var.name_prefix}-error-analysis"

  log_group_names = [
    aws_cloudwatch_log_group.application.name,
    aws_cloudwatch_log_group.api.name
  ]

  query_string = <<EOF
fields @timestamp, @message, level
| filter level = "ERROR"
| stats count() by bin(5m)
| sort @timestamp desc
EOF
}

resource "aws_cloudwatch_query_definition" "performance_analysis" {
  name = "${var.name_prefix}-performance-analysis"

  log_group_names = [
    aws_cloudwatch_log_group.performance.name,
    aws_cloudwatch_log_group.api.name
  ]

  query_string = <<EOF
fields @timestamp, @message, responseTime
| filter responseTime > 1000
| stats avg(responseTime), max(responseTime), count() by bin(5m)
| sort @timestamp desc
EOF
}

resource "aws_cloudwatch_query_definition" "security_analysis" {
  name = "${var.name_prefix}-security-analysis"

  log_group_names = [
    aws_cloudwatch_log_group.security.name
  ]

  query_string = <<EOF
fields @timestamp, @message, eventType, userId, ipAddress
| filter eventType in ["LOGIN_FAILED", "UNAUTHORIZED_ACCESS", "SUSPICIOUS_ACTIVITY"]
| stats count() by eventType, bin(1h)
| sort @timestamp desc
EOF
}

# Data source for current region
data "aws_region" "current" {}
