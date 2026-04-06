using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebsupplyConnect.Application;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Infrastructure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.Configure<APIsConnectConfig>(builder.Configuration);

builder.Services.Configure<AzureBlobStorageConfig>(
    builder.Configuration.GetSection("AzureBlobStorageConnection"));

builder.Services.Configure<WebhookMetaConfig>(
    builder.Configuration.GetSection("WhatsApp"));

builder.Services.Configure<AzureBusConfig>(
builder.Configuration.GetSection("AzureBusConnection"));

builder.Services.Configure<RedisConfiguration>(
builder.Configuration.GetSection("RedisConnection"));

builder.Services.Configure<ETLConfig>(
    builder.Configuration.GetSection("ETL"));

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
