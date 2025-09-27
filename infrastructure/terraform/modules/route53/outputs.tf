# Route53 Module Outputs

output "zone_id" {
  description = "Hosted zone ID"
  value       = var.domain_name != "" ? aws_route53_zone.main[0].zone_id : null
}

output "name_servers" {
  description = "Name servers for the hosted zone"
  value       = var.domain_name != "" ? aws_route53_zone.main[0].name_servers : null
}
