using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebsupplyConnect.Application;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddInfrastructure(configuration, useSqlServerSplitQuery: true);
        services.AddApplication();

        services.Configure<ETLConfig>(configuration.GetSection("ETL"));

        services.Configure<WebhookMetaConfig>(configuration.GetSection("WhatsApp"));

        services.Configure<AzureBlobStorageConfig>(configuration.GetSection("AzureBlobStorageConnection"));

        services.Configure<AzureBusConfig>(configuration.GetSection("AzureBusConnection"));

        services.Configure<RedisConfiguration>(configuration.GetSection("RedisConnection"));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
