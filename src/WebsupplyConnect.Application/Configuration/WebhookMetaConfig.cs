namespace WebsupplyConnect.Application.Configuration
{
    public record WebhookMetaConfig
    {
        public string WebhookSecret { get; init; }
        public string VerifyToken { get; init; }
        public string MetaUrl { get; init; }
    }
}
