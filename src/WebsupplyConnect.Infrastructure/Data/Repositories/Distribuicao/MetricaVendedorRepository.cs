using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório de métricas de vendedor
    /// </summary>
    public class MetricaVendedorRepository : BaseRepository, IMetricaVendedorRepository
    {
        private readonly ILogger<MetricaVendedorRepository> _logger;

        /// <summary>
        /// Construtor do repositório
        /// </summary>
        public MetricaVendedorRepository(
            WebsupplyConnectDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<MetricaVendedorRepository> logger)
            : base(dbContext, unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém a métrica de um vendedor para uma empresa
        /// </summary>
        public async Task<MetricaVendedor?> GetMetricaVendedorAsync(int usuarioId, int empresaId)
        {
            try
            {
                _logger.LogDebug("Buscando métrica do vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    usuarioId, empresaId);

                return await _context.Set<MetricaVendedor>()
                    .Where(m => m.UsuarioId == usuarioId && 
                              m.EmpresaId == empresaId && 
                              !m.Excluido)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar métrica do vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    usuarioId, empresaId);
                throw new InfraException($"Erro ao buscar métrica do vendedor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inicializa uma nova métrica para um vendedor
        /// </summary>
        public async Task<MetricaVendedor> InicializarMetricaVendedorAsync(int usuarioId, int empresaId)
        {
            try
            {
                _logger.LogInformation("Inicializando métricas para vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    usuarioId, empresaId);

                // Verificar se já existe
                var metricaExistente = await GetMetricaVendedorAsync(usuarioId, empresaId);
                if (metricaExistente != null)
                {
                    return metricaExistente;
                }

                // Criar nova métrica inicializada com valores padrão
                var novaMetrica = new MetricaVendedor(
                    usuarioId: usuarioId,
                    empresaId: empresaId,
                    dataInicioMedicao: TimeHelper.GetBrasiliaTime()
                    // Os demais parâmetros usarão os valores padrão definidos no construtor
                );

                // Adicionar ao contexto e salvar
                await _context.Set<MetricaVendedor>().AddAsync(novaMetrica);
                await _context.SaveChangesAsync();

                return novaMetrica;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar métrica do vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    usuarioId, empresaId);
                throw new InfraException($"Erro ao inicializar métrica do vendedor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza a métrica de um vendedor
        /// </summary>
        public async Task<MetricaVendedor> UpdateMetricaAsync(MetricaVendedor metrica)
        {
            try
            {
                _logger.LogDebug("Atualizando métrica do vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    metrica.UsuarioId, metrica.EmpresaId);

                // Atualizar no contexto
                _context.Set<MetricaVendedor>().Update(metrica);
                await _context.SaveChangesAsync();

                return metrica;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar métrica do vendedor. UsuarioId: {UsuarioId}, EmpresaId: {EmpresaId}",
                    metrica.UsuarioId, metrica.EmpresaId);
                throw new InfraException($"Erro ao atualizar métrica do vendedor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista as métricas de todos os vendedores de uma empresa
        /// </summary>
        public async Task<List<MetricaVendedor>> ListMetricasPorEmpresaAsync(int empresaId)
        {
            try
            {
                _logger.LogDebug("Listando métricas de vendedores. EmpresaId: {EmpresaId}", empresaId);

                return await _context.Set<MetricaVendedor>()
                    .Where(m => m.EmpresaId == empresaId && !m.Excluido)
                    .OrderByDescending(m => m.ScoreGeral)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar métricas de vendedores. EmpresaId: {EmpresaId}", empresaId);
                throw new InfraException($"Erro ao listar métricas de vendedores: {ex.Message}", ex);
            }
        }
    }
}