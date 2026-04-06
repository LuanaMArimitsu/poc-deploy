namespace WebsupplyConnect.Application.Configuration
{
    public record RedisConfiguration
    {
        public required string EndpointRedisCache { get; set; }
        public int CacheExpirationInDays { get; set; }
    }
}
