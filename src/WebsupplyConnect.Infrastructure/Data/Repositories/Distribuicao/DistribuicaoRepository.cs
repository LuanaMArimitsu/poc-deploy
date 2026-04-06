using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório de distribuição - refatorado para conter apenas operações de acesso a dados
    /// </summary>
    internal class DistribuicaoRepository : BaseRepository, IDistribuicaoRepository
    {
        private readonly ILogger<DistribuicaoRepository> _logger;

        /// <summary>
        /// Construtor do repositório
        /// </summary>
        public DistribuicaoRepository(
            WebsupplyConnectDbContext dbContext,
            IUnitOfWork unitOfWork,
            ILogger<DistribuicaoRepository> logger)
            : base(dbContext, unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Salva o histórico de uma distribuição de leads
        /// </summary>
        public async Task<HistoricoDistribuicao> SalvarHistoricoDistribuicaoAsync(HistoricoDistribuicao historico)
        {
            _logger.LogDebug("Salvando histórico de distribuição. ConfigId: {ConfigId}", 
                historico.ConfiguracaoDistribuicaoId);
                
            await _context.Set<HistoricoDistribuicao>().AddAsync(historico);
            await _context.SaveChangesAsync();
            
            return historico;
        }

        /// <summary>
        /// Lista o histórico de distribuição para uma empresa
        /// </summary>
        public async Task<List<HistoricoDistribuicao>> ListHistoricoDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null, 
            int pagina = 1, 
            int tamanhoPagina = 20)
        {
            _logger.LogDebug("Listando histórico de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}, Página: {Pagina}", 
                empresaId, dataInicio, dataFim, pagina);

            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Where(h => h.ConfiguracaoDistribuicao.EmpresaId == empresaId && !h.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(h => h.DataExecucao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(h => h.DataExecucao <= dataFim.Value);

            return await query
                .OrderByDescending(h => h.DataExecucao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();
        }

        /// <summary>
        /// Conta o total de históricos de distribuição para uma empresa
        /// </summary>
        public async Task<int> CountHistoricoDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            _logger.LogDebug("Contando histórico de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}", 
                empresaId, dataInicio, dataFim);

            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Where(h => h.ConfiguracaoDistribuicao.EmpresaId == empresaId && !h.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(h => h.DataExecucao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(h => h.DataExecucao <= dataFim.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Obtém um histórico de distribuição pelo ID
        /// </summary>
        public async Task<HistoricoDistribuicao?> GetHistoricoByIdAsync(int id)
        {
            _logger.LogDebug("Obtendo histórico de distribuição por ID: {Id}", id);

            if (id <= 0)
                throw new InfraException("ID do histórico deve ser maior que zero");

            return await _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Include(h => h.UsuarioExecutou)
                .FirstOrDefaultAsync(h => h.Id == id && !h.Excluido);
        }

        /// <summary>
        /// Obtém o tempo médio de distribuição para uma empresa
        /// </summary>
        public async Task<decimal> GetTempoMedioDistribuicaoAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            _logger.LogDebug("Calculando tempo médio de distribuição. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}", 
                empresaId, dataInicio, dataFim);

            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Where(h => h.ConfiguracaoDistribuicao.EmpresaId == empresaId && 
                          !h.Excluido && 
                          h.TempoExecucaoSegundos > 0);  // Ignorar registros com tempo zero

            if (dataInicio.HasValue)
                query = query.Where(h => h.DataExecucao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(h => h.DataExecucao <= dataFim.Value);

            // Verifica se existem registros antes de calcular a média
            if (await query.AnyAsync())
            {
                return await query.AverageAsync(h => (decimal)h.TempoExecucaoSegundos);
            }
            
            return 0;
        }

        /// <summary>
        /// Obtém a última distribuição para uma empresa
        /// </summary>
        public async Task<HistoricoDistribuicao?> GetUltimaDistribuicaoAsync(int empresaId)
        {
            _logger.LogDebug("Obtendo última distribuição. Empresa: {EmpresaId}", empresaId);

            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            return await _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Where(h => h.ConfiguracaoDistribuicao.EmpresaId == empresaId && !h.Excluido)
                .OrderByDescending(h => h.DataExecucao)
                .FirstOrDefaultAsync();
        }
    }
}