namespace SterlingAuctions.SimpleAPI.Configuration;

public class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int AuctionExpirationMinutes { get; set; } = 15;
    public int UserExpirationMinutes { get; set; } = 60;
    public int StatisticsExpirationMinutes { get; set; } = 5;
    public int SearchExpirationMinutes { get; set; } = 10;
    public int NotificationExpirationMinutes { get; set; } = 20;
    public int WatchlistExpirationMinutes { get; set; } = 30;
}
