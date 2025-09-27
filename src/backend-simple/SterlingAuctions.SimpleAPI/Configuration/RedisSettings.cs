namespace SterlingAuctions.SimpleAPI.Configuration;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "SterlingAuctions";
    public int DefaultDatabase { get; set; } = 0;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortOnConnectFail { get; set; } = false;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}
