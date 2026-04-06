using Microsoft.Extensions.DependencyInjection;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao.Strategy;
using WebsupplyConnect.Application.Services.Distribuicao;
using WebsupplyConnect.Application.Services.Distribuicao.Strategy;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao;

namespace WebsupplyConnect.Infrastructure.Configuration
{
    /// <summary>
    /// Classe de configuração para registrar serviços de distribuição
    /// </summary>
    public static class DistribuicaoServiceRegistration
    {
        /// <summary>
        /// Adiciona serviços de distribuição ao contêiner de injeção de dependência
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <returns>A mesma coleção para encadeamento</returns>
        public static IServiceCollection AddDistribuicaoServices(this IServiceCollection services)
        {
            // Registrar repositories
            services.AddScoped<IDistribuicaoRepository, DistribuicaoRepository>();
            services.AddScoped<IAtribuicaoLeadRepository, AtribuicaoLeadRepository>();
            services.AddScoped<IConfiguracaoDistribuicaoRepository, ConfiguracaoDistribuicaoRepository>();
            services.AddScoped<IFilaDistribuicaoRepository, FilaDistribuicaoRepository>();
            services.AddScoped<IMetricaVendedorRepository, MetricaVendedorRepository>();
            services.AddScoped<IRegraDistribuicaoRepository, RegraDistribuicaoRepository>();

            // Registrar services especializados
            services.AddScoped<IRegraDistribuicaoService, RegraDistribuicaoService>();
            services.AddScoped<IVendedorEstatisticasService, VendedorEstatisticasService>();
            services.AddScoped<IMetricaCacheService, MetricaCacheService>();
            
            // Registrar commands
            services.AddScoped<ITransferenciaLeadCommand, TransferenciaLeadCommand>();
            
            // Services de dados removidos - simplificação DIP
            
            // Registrar services agregadores (orquestradores)
            services.AddScoped<IDistribuicaoConfiguracaoReaderService, DistribuicaoConfiguracaoReaderService>();
            services.AddScoped<IDistribuicaoContextoReaderService, DistribuicaoContextoReaderService>();
            services.AddScoped<IHistoricoDistribuicaoReaderService, HistoricoDistribuicaoReaderService>();
            services.AddScoped<IScoreCalculationService, ScoreCalculationService>();
            services.AddScoped<IMetricaVendedorService, MetricaVendedorService>();
            services.AddScoped<IRedistribuicaoService, RedistribuicaoService>();
            
            // Registrar outros services
            services.AddScoped<IDistribuicaoWriterService, DistribuicaoWriterService>();
            services.AddScoped<IDistribuicaoReaderService, DistribuicaoReaderService>();

            services.AddScoped<IFilaDistribuicaoService, FilaDistribuicaoService>();
            services.AddScoped<IFilaDistribuicaoReaderService, FilaDistribuicaoReaderService>();
            services.AddScoped<IAtribuicaoLeadService, AtribuicaoLeadService>();

            // Registrar estratégias de distribuição
            services.AddScoped<IRegraDistribuicaoStrategy, RegraDistribuicaoMeritoStrategy>();
            services.AddScoped<IRegraDistribuicaoStrategy, RegraDistribuicaoFilaStrategy>();
            services.AddScoped<IRegraDistribuicaoStrategy, RegraDistribuicaoTempoStrategy>();
            
            // Registrar provider de estratégias
            services.AddScoped<IRegraDistribuicaoProvider, RegraDistribuicaoProvider>();
            
            // Adicionar cache de memória para métricas
            services.AddMemoryCache();
            
            return services;
        }
    }
}