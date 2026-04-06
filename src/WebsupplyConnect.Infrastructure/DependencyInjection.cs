using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comum;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Configuracao;
using WebsupplyConnect.Domain.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Domain.Interfaces.Empresa;
using WebsupplyConnect.Domain.Interfaces.Equipe;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Domain.Interfaces.Notificacao;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Permissao;
using WebsupplyConnect.Domain.Interfaces.OLAP.Controle;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Produto;
using WebsupplyConnect.Domain.Interfaces.Usuario;
using WebsupplyConnect.Infrastructure.Authorization.Handlers;
using WebsupplyConnect.Infrastructure.Authorization.Requirement;
using WebsupplyConnect.Infrastructure.Configuration;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Data.Repositories.Comum;
using WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Configuracao;
using WebsupplyConnect.Infrastructure.Data.Repositories.ControleSistemasExternos;
using WebsupplyConnect.Infrastructure.Data.Repositories.Empresa;
using WebsupplyConnect.Infrastructure.Data.Repositories.Equipe;
using WebsupplyConnect.Infrastructure.Data.Repositories.Lead;
using WebsupplyConnect.Infrastructure.Data.Repositories.Notificacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Controle;
using WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data.Repositories.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.Repositories.Perfil;
using WebsupplyConnect.Infrastructure.Data.Repositories.Permissao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Produto;
using WebsupplyConnect.Infrastructure.Data.Repositories.Usuarios;
using WebsupplyConnect.Infrastructure.ExternalServices.AzureAd;
using WebsupplyConnect.Infrastructure.ExternalServices.AzureBus;
using WebsupplyConnect.Infrastructure.ExternalServices.Cache;
using WebsupplyConnect.Infrastructure.ExternalServices.Chatbot;
using WebsupplyConnect.Infrastructure.ExternalServices.ConnectIntegrador;
using WebsupplyConnect.Infrastructure.ExternalServices.Firebase;
using WebsupplyConnect.Infrastructure.ExternalServices.OLX;
using WebsupplyConnect.Infrastructure.ExternalServices.OpenAi;
using WebsupplyConnect.Infrastructure.ExternalServices.SendGrid;
using WebsupplyConnect.Infrastructure.ExternalServices.SignalR;
using WebsupplyConnect.Infrastructure.ExternalServices.Storage;
using WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp;
using WebsupplyConnect.Infrastructure.Identity;
using WebsupplyConnect.Domain.Interfaces.VersaoApp;
using WebsupplyConnect.Infrastructure.Data.Repositories.VersaoApp;

namespace WebsupplyConnect.Infrastructure
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Adiciona os serviços da camada de infraestrutura ao contêiner de serviços
        /// </summary>
        /// <param name="services">Contêiner de serviços</param>
        /// <param name="configuration">Configurações da aplicação</param>
        /// <param name="useSqlServerSplitQuery">
        /// Quando true, aplica split query no SQL Server (recomendado no host ETL para evitar cartesian explosion em consultas com várias coleções incluídas).
        /// </param>
        /// <returns>IServiceCollection com os serviços configurados</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool useSqlServerSplitQuery = false)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration["DefaultConnection"];

            services.AddDbContext<WebsupplyConnectDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    connectionString,
                    b =>
                    {
                        b.MigrationsAssembly(typeof(WebsupplyConnectDbContext).Assembly.FullName);
                        b.CommandTimeout(120);
                        if (useSqlServerSplitQuery)
                            b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });
            });

            services.AddHttpContextAccessor();
            services.AddScoped<IAuthorizationHandler, HorarioDeTrabalhoHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HorarioTrabalho", policy =>
                    policy.Requirements.Add(new HorarioDeTrabalhoRequirement()));
            });

            // Registra o Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registra os repositórios
            services.AddSingleton<ServiceBusClient>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<AzureBusConfig>>().Value;
                return new ServiceBusClient(config.EndpointAzureBus);
            });

            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<RedisConfiguration>>().Value;
                return ConnectionMultiplexer.Connect(config.EndpointRedisCache);
            });

            // Configurations
            services.Configure<APIsConnectConfig>(configuration.GetSection("APIsConnect"));

            // SignalR
            services.AddSignalR().AddAzureSignalR(configuration["SignalR:ConnectionString"]);
            services.AddScoped<INotificacaoDispatcher, NotificacaoDispatcher>();
            services.AddSingleton<ISignalRConnection, SignalRConnection>();

            services.AddSingleton<IPushNotificationService, PushNotificationService>();

            // Services
            services.AddSingleton<IBusPublisherService, BusPublisherService>();
            services.AddScoped<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IMessageProcessingInboundService, MessageProcessingInboundService>();
            services.AddScoped<IAzureAdService, AzureAdService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IMidiaProcessingService, MidiaProcessingService>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();
            services.AddScoped<IMessageProcessingOutboundService, MessageProcessingOutboundService>();
            services.AddScoped<IMailSenderService, MailSenderService>();
            services.AddScoped<IOpenAiService, OpenAiService>();
            services.AddScoped<IChatBotClient, ChatBotClient>();
            services.AddScoped<IConnectIntegradorService, ConnectIntegradorService>();
            services.AddScoped<IWhatsAppInatividadeService, WhatsAppInatividadeService>();
            services.AddScoped<IEscalonamentoAutomaticoService,EscalonamentoAutomaticoService>();
            services.AddScoped<IOlxIntegracaoService, OlxIntegracaoService>();

            //clients
            services.AddScoped<IWhatsAppMediaClient, WhatsAppMediaClient>();
            services.AddScoped<IWhatsAppClient, WhatsAppClient>();
            services.AddScoped<INotificacaoClient,  NotificacaoClient>();

            // Repositorys
            services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IWebhookMetaRepository, WebhookMetaRepository>();
            services.AddScoped<IConversaRepository, ConversaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ICanalRepository, CanalRepository>();
            services.AddScoped<IConversaRepository, ConversaRepository>();
            services.AddScoped<IMensagemRepository, MensagemRepository>();
            services.AddScoped<ILeadRepository, LeadRepository>();
            services.AddScoped<ITipoOrigemRepository, TipoOrigemRepository>();
            services.AddScoped<IMidiaRepository, MidiaRepository>();
            services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddScoped<IWhatsAppClient, WhatsAppClient>();
            services.AddScoped<IEmpresaRepository, EmpresaRepository>();
            services.AddScoped<IDispositivosRepository, DispositivosRepository>();
            services.AddScoped<IHorariosRepository, HorariosRepository>();
            services.AddScoped<IEnderecoRepository, EnderecoRepository>();
            services.AddScoped<IOrigemRepository, OrigemRepository>();
            services.AddScoped<IOportunidadeRepository, OportunidadeRepository>();
            services.AddScoped<IProdutoRepository, ProdutoRepository>();
            services.AddScoped<IProdutoHistoricoRepository, ProdutoHistoricoRepository>();
            services.AddScoped<IUsuarioEmpresaRepository, UsuarioEmpresaRepository>();
            services.AddScoped<IEtapaRepository, EtapaRepository>();
            services.AddScoped<IFunilRepository, FunilRepository>();
            services.AddScoped<IPromptEmpresaRepository, PromptEmpresasRepository>();
            services.AddScoped<IPromptConfiguracaoRepository, PromptConfiguracaoRepository>();
            services.AddScoped<IEquipeRepository, EquipeRepository>();
            services.AddScoped<ITipoEquipeRepository, TipoEquipeRepository>();
            services.AddScoped<IMembroEquipeRepository, MembroEquipeRepository>();
            services.AddScoped<IStatusMembroEquipeRepository, StatusMembroEquipeRepository>();
            services.AddScoped<IPermissaoRepository, PermissaoRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<ICampanhaRepository, CampanhaRepository>();
            services.AddScoped<ISistemaExternoRepository, SistemaExternoRepository>();
            services.AddScoped<IEventoIntegracaoRepository, EventoIntegracaoRepository>();

            //Repositórios de Eventos de Lead
            services.AddScoped<ILeadEventoRepository, LeadEventoRepository>();

            // Repositórios de Distribuição
            services.AddDistribuicaoServices();

            // Repositórios de Comum
            services.AddScoped<IFeriadoRepository, FeriadoRepository>();

            // Repositórios OLAP
            services.AddScoped<IDimensaoRepository, DimensaoRepository>();
            services.AddScoped<IFatoOportunidadeMetricaRepository, FatoOportunidadeMetricaRepository>();
            services.AddScoped<IFatoLeadAgregadoRepository, FatoLeadAgregadoRepository>();
            services.AddScoped<IFatoEventoAgregadoRepository, FatoEventoAgregadoRepository>();
            services.AddScoped<IETLControleProcessamentoRepository, ETLControleProcessamentoRepository>();

            // Repositório de Versão do App
            services.AddScoped<IVersaoAppRepository, VersaoAppRepository>();

            return services;
        }

    }
}
