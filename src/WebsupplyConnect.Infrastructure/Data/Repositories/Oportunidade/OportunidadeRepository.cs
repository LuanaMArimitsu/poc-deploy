using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Oportunidade
{
    public class OportunidadeRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork, ILogger<OportunidadeRepository> logger) : BaseRepository(dbContext, unitOfWork), IOportunidadeRepository
    {
        private readonly ILogger<OportunidadeRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<(List<Domain.Entities.Oportunidade.Oportunidade> itens, int totalItens)> ListarOportunidadesFiltradoAsync(int? leadId, int? produtoId, int? etapaId, decimal? valorMinimo, decimal? valorMaximo, int? responsavelId, int? origemId, int? empresaId, DateTime? dataPrevisaoFechamento, int? pagina, int? tamanho, DateTime? de, DateTime? ate)
        {
            var query = _context.Oportunidades.AsQueryable();

            if (leadId > 0)
                query = query.Where(l => l.LeadId == leadId.Value);

            if (produtoId > 0)
                query = query.Where(l => l.ProdutoId == produtoId.Value);

            if (etapaId > 0)
                query = query.Where(l => l.EtapaId == etapaId.Value);

            if (valorMinimo.HasValue)
                query = query.Where(l => l.Valor >= valorMinimo.Value);

            if (valorMaximo.HasValue)
                query = query.Where(l => l.ValorFinal <= valorMaximo.Value);

            if (responsavelId > 0)
                query = query.Where(l => l.ResponsavelId == responsavelId.Value);

            if (origemId > 0)
                query = query.Where(l => l.OrigemId == origemId.Value);

            if (empresaId > 0)
                query = query.Where(l => l.EmpresaId == empresaId.Value);

            if (dataPrevisaoFechamento.HasValue)
                query = query.Where(l => l.DataPrevisaoFechamento <= dataPrevisaoFechamento.Value);

            if (de.HasValue)
                query = query.Where(l => l.DataCriacao.Date >= de.Value.Date);

            if (ate.HasValue)
                query = query.Where(l => l.DataCriacao.Date <= ate.Value.Date);

            query = query.Where(l => !l.Excluido);

            // Total de itens sem considerar paginação
            var totalItens = await query.CountAsync();

            // Query base com includes e ordenação
            var queryOrdenada = query
                .Where(l => !l.Excluido)
                .OrderByDescending(l => l.DataCriacao)
                .ThenBy(l => l.Id)
                .Include(l => l.Lead)
                .Include(l => l.Produto)
                .Include(l => l.Etapa)
                .Include(l => l.Responsavel)
                .Include(l => l.Origem)
                .Include(l => l.Empresa)
                .Include(l => l.TipoInteresse)
                .Include(o => o.LeadEvento)
                     .ThenInclude(le => le.Campanha)

                .Include(o => o.LeadEvento)
                     .ThenInclude(le => le.Canal);

            IQueryable<Domain.Entities.Oportunidade.Oportunidade> queryFinal = queryOrdenada;

            if (pagina.HasValue && tamanho.HasValue && pagina > 0 && tamanho > 0)
            {
                int paginaSeguro = pagina.Value;
                int tamanhoSeguro = tamanho.Value;

                queryFinal = queryOrdenada
                    .Skip((paginaSeguro - 1) * tamanhoSeguro)
                    .Take(tamanhoSeguro);
            }

            var itens = await queryFinal.ToListAsync();

            return (itens, totalItens);
        }

        public async Task<Domain.Entities.Oportunidade.Oportunidade?> GetDetailsById(int id)
        {
            try
            {

                return await _context.Oportunidades
                    .Include(o => o.Lead)
                    .Include(o => o.Produto)
                    .Include(o => o.Etapa)
                    .Include(o => o.Responsavel)
                    .Include(o => o.Origem)
                    .Include(o => o.Empresa)
                    .Include(o => o.TipoInteresse)

                    .Include(o => o.LeadEvento)
                        .ThenInclude(le => le.Campanha)

                    .Include(o => o.LeadEvento)
                        .ThenInclude(le => le.Canal)

                    .Where(o => !o.Excluido && o.Id == id)
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes da oportunidade por ID.");
                throw;
            }
        }

        public async Task<List<TipoInteresse>> ListarTiposInteresseAsync()
        {
            return await _context.TipoInteresses
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<EtapaHistorico>> ObterHistoricoEtapasPorOportunidadeIdAsync(int oportunidadeId, CancellationToken cancellationToken = default)
        {
            return await _context.EtapasHistorico
                .AsNoTracking()
                .Where(e => e.OportunidadeId == oportunidadeId)
                .OrderByDescending(e => e.DataMudanca)
                .ToListAsync(cancellationToken);
        }
    }
}