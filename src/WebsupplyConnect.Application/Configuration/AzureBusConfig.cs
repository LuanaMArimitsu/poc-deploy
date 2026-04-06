namespace WebsupplyConnect.Application.Configuration
{
    public record AzureBusConfig
    {
        public string EndpointAzureBus { get; set; }
        public Dictionary<string, string> Queues { get; set; }
    }
}
