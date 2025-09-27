# CloudWatch and Seq Monitoring/Logging Implementation

## Overview

This document describes the comprehensive monitoring and logging implementation for Sterling Auctions using AWS CloudWatch and Seq. The implementation provides structured logging, metrics collection, and real-time monitoring capabilities.

## Architecture

### Components

1. **CloudWatch Metrics Service** - Custom metrics collection and publishing
2. **CloudWatch Logging Service** - Structured logging to CloudWatch Logs
3. **Seq Logging Service** - Structured logging to Seq for analysis
4. **Combined Logging Service** - Unified logging across both platforms
5. **Application Metrics Service** - Business logic metrics
6. **Application Logging Service** - Business logic logging
7. **Monitoring Controller** - API endpoints for testing monitoring

### Data Flow

```
Application Events → Application Services → CloudWatch/Seq → Dashboards/Alerts
```

## Services

### 1. CloudWatch Metrics Service

**Purpose**: Collect and publish custom application metrics to CloudWatch.

**Key Features**:
- Custom metric publishing
- Counter, gauge, and timer metrics
- Dimension support for filtering
- Error handling and fallback

**Usage**:
```csharp
await _metricsService.PublishCounterAsync("ApiRequests", 1, dimensions);
await _metricsService.PublishGaugeAsync("ActiveUsers", userCount);
await _metricsService.PublishTimerAsync("ResponseTime", durationMs);
```

### 2. CloudWatch Logging Service

**Purpose**: Send structured logs to CloudWatch Logs.

**Key Features**:
- Structured JSON logging
- Log level support
- Exception handling
- Automatic log stream management

**Usage**:
```csharp
await _cloudWatchService.LogInfoAsync("User logged in", new { UserId = userId });
await _cloudWatchService.LogErrorAsync("Payment failed", exception);
```

### 3. Seq Logging Service

**Purpose**: Send structured logs to Seq for analysis and querying.

**Key Features**:
- Structured logging with Serilog
- Rich query capabilities
- Event correlation
- Performance analysis

**Usage**:
```csharp
await _seqService.LogUserEventAsync(userId, "Login", data);
await _seqService.LogAuctionEventAsync(auctionId, "BidPlaced", data);
```

### 4. Application Metrics Service

**Purpose**: Collect business-specific metrics.

**Key Features**:
- API request metrics
- Authentication/authorization metrics
- Auction event metrics
- Payment event metrics
- Cache event metrics
- Error tracking
- Performance metrics

**Usage**:
```csharp
await _appMetrics.RecordApiRequestAsync("POST", "/api/auctions", 200, 150);
await _appMetrics.RecordAuctionEventAsync("AuctionCreated", auctionId);
await _appMetrics.RecordBidEventAsync(auctionId, bidAmount);
```

### 5. Application Logging Service

**Purpose**: Log business events and user actions.

**Key Features**:
- User action logging
- Auction event logging
- Payment event logging
- Security event logging
- Performance event logging
- Business event logging

**Usage**:
```csharp
await _appLogging.LogUserActionAsync(userId, "CreateAuction", data);
await _appLogging.LogSecurityEventAsync("FailedLogin", userId, data);
```

## Configuration

### CloudWatch Configuration

```json
{
  "CloudWatch": {
    "Enabled": true,
    "Region": "us-east-1",
    "Namespace": "SterlingAuctions/Application",
    "LogGroup": "/aws/sterling-auctions/application",
    "LogRetentionDays": 14
  }
}
```

### Seq Configuration

```json
{
  "Seq": {
    "Enabled": true,
    "Url": "http://localhost:5341",
    "ApiKey": ""
  }
}
```

### Serilog Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.AwsCloudWatch", "Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/sterling-auctions-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

## API Endpoints

### Monitoring Controller

The `MonitoringController` provides endpoints for testing and demonstrating the monitoring capabilities:

#### Test Metrics
```http
POST /api/monitoring/test-metrics
```
Tests metrics collection and publishing.

#### Test Logging
```http
POST /api/monitoring/test-logging
```
Tests different log levels and structured logging.

#### Test Error Logging
```http
POST /api/monitoring/test-error-logging
```
Tests error and exception logging.

#### Test Auction Events
```http
POST /api/monitoring/test-auction-events
```
Tests auction-specific event logging and metrics.

#### Test Payment Events
```http
POST /api/monitoring/test-payment-events
```
Tests payment-specific event logging and metrics.

#### Test Security Events
```http
POST /api/monitoring/test-security-events
```
Tests security event logging and metrics.

#### Test Performance Metrics
```http
POST /api/monitoring/test-performance-metrics
```
Tests performance metric collection.

#### Get Monitoring Status
```http
GET /api/monitoring/status
```
Returns the current monitoring status and configuration.

## Metrics Collected

### API Metrics
- Request count by method, path, and status code
- Response time by endpoint
- Error rate by endpoint
- Throughput metrics

### Authentication Metrics
- Login attempts by method and success rate
- Failed login attempts
- Authentication method usage

### Authorization Metrics
- Authorization attempts by resource and action
- Success/failure rates
- Permission denials

### Auction Metrics
- Auction creation events
- Auction start/end events
- Bid placement events
- Bid amounts and ranges

### Payment Metrics
- Payment creation events
- Payment success/failure rates
- Payment amounts and ranges
- Refund events

### Cache Metrics
- Cache hit/miss rates
- Cache operations by type
- Cache performance metrics

### Error Metrics
- Error counts by type and context
- Exception tracking
- Error rate trends

### Performance Metrics
- Custom performance metrics
- Duration tracking
- Resource utilization

## Logs Collected

### User Events
- User registration and login
- Profile updates
- Account actions
- User preferences

### Auction Events
- Auction creation and updates
- Auction start/end
- Bid placement and updates
- Auction status changes

### Payment Events
- Payment creation and processing
- Payment completion and failures
- Refund processing
- Payment method updates

### Security Events
- Login attempts and failures
- Unauthorized access attempts
- Permission denials
- Security policy violations

### Performance Events
- API response times
- Database query performance
- Cache performance
- External service calls

### Business Events
- Revenue tracking
- User engagement
- Feature usage
- Business metrics

## CloudWatch Dashboards

### Overview Dashboard
- ECS service metrics (CPU, memory)
- RDS metrics (CPU, connections)
- Redis metrics (CPU, memory usage)
- Load balancer metrics (requests, response time, errors)

### Application Performance Dashboard
- API request metrics
- Response time trends
- Error rates
- Throughput metrics

### Business Metrics Dashboard
- Auction activity
- Payment processing
- User engagement
- Revenue metrics

### Security Dashboard
- Authentication events
- Authorization attempts
- Security violations
- Failed login attempts

## CloudWatch Alarms

### Critical Alarms
- High error rate (>5%)
- High response time (>2 seconds)
- Failed authentication rate (>10%)
- Database connection failures

### Warning Alarms
- Low throughput
- High CPU usage
- Memory usage warnings
- Cache miss rate

### Info Alarms
- Service health checks
- Deployment notifications
- Performance trends

## Seq Queries

### Error Analysis
```sql
@Level = 'Error' | where @Exception is not null
```

### Performance Analysis
```sql
@Message like '%Performance%' | where DurationMs > 1000
```

### User Activity
```sql
@Message like '%User Event%' | where UserId = 'user123'
```

### Auction Activity
```sql
@Message like '%Auction Event%' | where AuctionId = 123
```

### Security Events
```sql
@Message like '%Security Event%' | where EventType = 'FailedLogin'
```

## Integration Points

### Controllers
All controllers can inject and use the monitoring services:

```csharp
public class AuctionController : ControllerBase
{
    private readonly IApplicationMetricsService _metrics;
    private readonly IApplicationLoggingService _logging;

    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Business logic
            var auction = await _auctionService.CreateAsync(dto);
            
            // Log success
            await _logging.LogAuctionEventAsync(auction.Id, "AuctionCreated", new { CreatedBy = UserId });
            await _metrics.RecordAuctionEventAsync("AuctionCreated", auction.Id);
            
            return Ok(auction);
        }
        catch (Exception ex)
        {
            // Log error
            await _logging.LogErrorAsync("Failed to create auction", ex);
            await _metrics.RecordErrorAsync("AuctionCreationFailed", "AuctionController");
            
            throw;
        }
        finally
        {
            stopwatch.Stop();
            await _metrics.RecordApiRequestAsync("POST", "/api/auctions", 200, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Middleware
Custom middleware can be added to automatically collect metrics:

```csharp
public class MetricsMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            await _metrics.RecordApiRequestAsync(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
```

## Deployment Considerations

### AWS Permissions
The application needs the following IAM permissions:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "cloudwatch:PutMetricData",
        "cloudwatch:GetMetricStatistics",
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "logs:DescribeLogGroups",
        "logs:DescribeLogStreams"
      ],
      "Resource": "*"
    }
  ]
}
```

### Environment Variables
Set the following environment variables:

```bash
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
CLOUDWATCH_ENABLED=true
SEQ_ENABLED=true
SEQ_URL=http://localhost:5341
```

### Docker Configuration
Update the Docker configuration to include monitoring:

```dockerfile
# Install AWS CLI for CloudWatch
RUN apt-get update && apt-get install -y awscli

# Set environment variables
ENV AWS_REGION=us-east-1
ENV CLOUDWATCH_ENABLED=true
ENV SEQ_ENABLED=true
```

## Testing

### Unit Tests
Test the monitoring services:

```csharp
[Test]
public async Task CloudWatchMetricsService_ShouldPublishMetric()
{
    // Arrange
    var mockClient = new Mock<IAmazonCloudWatch>();
    var service = new CloudWatchMetricsService(mockClient.Object, _logger, _config);

    // Act
    await service.PublishMetricAsync("TestMetric", 1.0);

    // Assert
    mockClient.Verify(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), default), Times.Once);
}
```

### Integration Tests
Test the monitoring endpoints:

```csharp
[Test]
public async Task TestMetrics_ShouldReturnSuccess()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.PostAsync("/api/monitoring/test-metrics", null);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Monitoring Best Practices

### 1. Structured Logging
- Use consistent log formats
- Include relevant context
- Avoid logging sensitive data
- Use appropriate log levels

### 2. Metrics Collection
- Collect meaningful business metrics
- Use appropriate metric types
- Include relevant dimensions
- Set appropriate retention periods

### 3. Error Handling
- Log errors with context
- Include stack traces
- Track error rates
- Set up alerts for critical errors

### 4. Performance Monitoring
- Track response times
- Monitor resource usage
- Set performance baselines
- Alert on performance degradation

### 5. Security Monitoring
- Log security events
- Track authentication failures
- Monitor authorization attempts
- Alert on suspicious activity

## Troubleshooting

### Common Issues

1. **CloudWatch Permissions**
   - Ensure IAM permissions are correct
   - Check AWS credentials
   - Verify region configuration

2. **Seq Connection**
   - Verify Seq URL is accessible
   - Check API key if required
   - Ensure Seq is running

3. **Log Format Issues**
   - Verify Serilog configuration
   - Check log level settings
   - Ensure proper JSON formatting

4. **Metrics Not Appearing**
   - Check CloudWatch namespace
   - Verify metric dimensions
   - Ensure metrics are being published

### Debugging

Enable debug logging:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

Check CloudWatch logs:

```bash
aws logs describe-log-groups --log-group-name-prefix "/aws/sterling-auctions"
```

## Future Enhancements

### 1. Custom Dashboards
- Create application-specific dashboards
- Add business KPI tracking
- Implement real-time monitoring

### 2. Advanced Alerting
- Implement intelligent alerting
- Add alert correlation
- Create escalation policies

### 3. Performance Optimization
- Implement log sampling
- Add metric aggregation
- Optimize log retention

### 4. Integration Enhancements
- Add more log sinks
- Implement log forwarding
- Add custom metrics

## Conclusion

The CloudWatch and Seq monitoring implementation provides comprehensive observability for the Sterling Auctions application. It enables:

- **Real-time monitoring** of application health and performance
- **Structured logging** for debugging and analysis
- **Business metrics** for tracking KPIs and user behavior
- **Security monitoring** for detecting threats and violations
- **Performance tracking** for optimization and scaling

The implementation is designed to be scalable, maintainable, and cost-effective while providing the visibility needed to operate a production auction platform.
