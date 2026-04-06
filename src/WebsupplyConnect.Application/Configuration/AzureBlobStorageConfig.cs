namespace WebsupplyConnect.Application.Configuration
{
    public class AzureBlobStorageConfig
    {
        public required string AzureBlobStorageConnectionString { get; set; }
        public required  string ContainerNameMidiasMeta { get; set; }
    }
}
