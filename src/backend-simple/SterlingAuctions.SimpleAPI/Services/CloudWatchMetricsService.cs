using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SterlingAuctions.SimpleAPI.Services;

public interface ICloudWatchMetricsService
{
    Task PublishMetricAsync(string metricName, double value, string unit = "Count", Dictionary<string, string> dimensions = null);
}

public class CloudWatchMetricsService : ICloudWatchMetricsService
{
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly ILogger<CloudWatchMetricsService> _logger;
    private readonly string _defaultNamespace;
    private readonly bool _enabled;

    public CloudWatchMetricsService(
        IAmazonCloudWatch cloudWatchClient,
        ILogger<CloudWatchMetricsService> logger,
        IConfiguration configuration)
    {
        _cloudWatchClient = cloudWatchClient;
        _logger = logger;
        _defaultNamespace = configuration.GetValue<string>("CloudWatch:Namespace") ?? "SterlingAuctions/Application";
        _enabled = configuration.GetValue<bool>("CloudWatch:Enabled") && !string.IsNullOrEmpty(_defaultNamespace);
    }

    public async Task PublishMetricAsync(string metricName, double value, string unit = "Count", Dictionary<string, string> dimensions = null)
    {
        if (!_enabled)
        {
            Log.Debug("CloudWatch metrics disabled, skipping metric: {MetricName} = {Value}", metricName, value);
            return;
        }

        try
        {
            var request = new PutMetricDataRequest
            {
                Namespace = _defaultNamespace,
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = metricName,
                        Value = value,
                        Unit = unit,
                        Timestamp = DateTime.UtcNow,
                        Dimensions = ConvertDimensions(dimensions)
                    }
                }
            };

            await _cloudWatchClient.PutMetricDataAsync(request);
            
            Log.Debug("Published CloudWatch metric: {MetricName} = {Value} {Unit}", metricName, value, unit);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to publish CloudWatch metric: {MetricName} = {Value}", metricName, value);
        }
    }

    private List<Dimension> ConvertDimensions(Dictionary<string, string> dimensions)
    {
        if (dimensions == null || dimensions.Count == 0)
            return new List<Dimension>();

        return dimensions.Select(d => new Dimension
        {
            Name = d.Key,
            Value = d.Value
        }).ToList();
    }
}
