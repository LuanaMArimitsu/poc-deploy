using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório de atribuições de leads
    /// </summary>
    internal class AtribuicaoLeadRepository : BaseRepository, IAtribuicaoLeadRepository
    {
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="dbContext">Contexto do banco de dados</param>
        /// <param name="unitOfWork">Unidade de trabalho para controle de transações</param>
        public AtribuicaoLeadRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Cria um novo registro de atribuição de lead
        /// </summary>
        /// <param name="atribuicao">Atribuição a ser criada</param>
        /// <returns>Atribuição criada com ID atualizado</returns>
        public async Task<AtribuicaoLead> CriarAtribuicaoAsync(AtribuicaoLead atribuicao)
        {
            if (atribuicao == null)
                throw new InfraException("Atribuição não pode ser nula");

            await _context.Set<AtribuicaoLead>().AddAsync(atribuicao);
            await _context.SaveChangesAsync();
            
            // Recarregar a entidade com as propriedades de navegação
            return await _context.Set<AtribuicaoLead>()
                .Include(a => a.MembroAtribuido)
                .Include(a => a.MembroAtribuiu)
                .Include(a => a.TipoAtribuicao)
                .Include(a => a.ConfiguracaoDistribuicao)
                .Include(a => a.RegraDistribuicao)
                .FirstOrDefaultAsync(a => a.Id == atribuicao.Id);
        }
        
        /// <summary>
        /// Atualiza um registro de atribuição existente
        /// </summary>
        /// <param name="atribuicao">Atribuição a ser atualizada</param>
        /// <returns>Atribuição atualizada</returns>
        public async Task<AtribuicaoLead> UpdateAsync(AtribuicaoLead atribuicao)
        {
            if (atribuicao == null)
                throw new InfraException("Atribuição não pode ser nula");

            _context.Set<AtribuicaoLead>().Update(atribuicao);
            await _context.SaveChangesAsync();
            
            // Recarregar a entidade com as propriedades de navegação
            return await _context.Set<AtribuicaoLead>()
                .Include(a => a.MembroAtribuido)
                .Include(a => a.MembroAtribuiu)
                .Include(a => a.TipoAtribuicao)
                .Include(a => a.ConfiguracaoDistribuicao)
                .Include(a => a.RegraDistribuicao)
                .FirstOrDefaultAsync(a => a.Id == atribuicao.Id);
        }
        
        /// <summary>
        /// Obtém a última atribuição para um lead
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>Última atribuição ou null se não existir</returns>
        public async Task<AtribuicaoLead?> ObterUltimaAtribuicaoLeadAsync(int leadId)
        {
            if (leadId <= 0)
                throw new InfraException("ID do lead deve ser maior que zero");

            var query = _context.Set<AtribuicaoLead>().AsQueryable();
            
            // Filtrar por lead e ordenar pela data mais recente
            query = query.Where(a => a.LeadId == leadId && !a.Excluido)
                        .OrderByDescending(a => a.DataAtribuicao);
                        
            // Incluir entidades relacionadas para evitar múltiplas consultas
            query = query.Include(a => a.MembroAtribuido)
                        .Include(a => a.MembroAtribuiu)
                        .Include(a => a.TipoAtribuicao);
                        
            return await query.FirstOrDefaultAsync();
        }
        
        /// <summary>
        /// Obtém o histórico completo de atribuições de um lead
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>Lista de atribuições ordenadas por data</returns>
        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorLeadAsync(int leadId)
        {
            if (leadId <= 0)
                throw new InfraException("ID do lead deve ser maior que zero");

            return await _context.Set<AtribuicaoLead>()
                .Where(a => a.LeadId == leadId && !a.Excluido)
                .Include(a => a.MembroAtribuido)
                .Include(a => a.MembroAtribuiu)
                .Include(a => a.TipoAtribuicao)
                .Include(a => a.RegraDistribuicao)
                .OrderByDescending(a => a.DataAtribuicao)
                .ToListAsync();
        }
        
        /// <summary>
        /// Verifica se um lead já tem um responsável
        /// </summary>
        /// <param name="leadId">ID do lead</param>
        /// <returns>True se o lead já tem responsável, false caso contrário</returns>
        public async Task<bool> LeadPossuiResponsavelAsync(int leadId)
        {
            if (leadId <= 0)
                throw new InfraException("ID do lead deve ser maior que zero");

            var lead = await _context.Set<Domain.Entities.Lead.Lead>()
                .FirstOrDefaultAsync(l => l.Id == leadId && !l.Excluido);

            return lead?.ResponsavelId != null;
        }

        /// <summary>
        /// Lista as atribuições para um vendedor específico
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <param name="pagina">Número da página</param>
        /// <param name="tamanhoPagina">Tamanho da página</param>
        /// <returns>Lista de atribuições do vendedor</returns>
        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorVendedorAsync(
            int vendedorId, 
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null, 
            int pagina = 1, 
            int tamanhoPagina = 20)
        {
            if (vendedorId <= 0)
                throw new InfraException("ID do vendedor deve ser maior que zero");
                
            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");
                
            if (pagina <= 0)
                pagina = 1;
                
            if (tamanhoPagina <= 0)
                tamanhoPagina = 20;

            var query = _context.Set<AtribuicaoLead>()
                .Include(a => a.Lead)
                .Include(a => a.TipoAtribuicao)
                .Include(a => a.RegraDistribuicao)
                .Where(a => a.MembroAtribuidoId == vendedorId && 
                           a.Lead.EmpresaId == empresaId && 
                           !a.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataAtribuicao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataAtribuicao <= dataFim.Value);

            return await query
                .OrderByDescending(a => a.DataAtribuicao)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();
        }

        /// <summary>
        /// Conta o total de atribuições para um vendedor
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Total de atribuições</returns>
        public async Task<int> CountAtribuicoesPorVendedorAsync(
            int vendedorId, 
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            if (vendedorId <= 0)
                throw new InfraException("ID do vendedor deve ser maior que zero");
                
            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<AtribuicaoLead>()
                .Include(a => a.Lead)
                .Where(a => a.MembroAtribuidoId == vendedorId && 
                           a.Lead.EmpresaId == empresaId && 
                           !a.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataAtribuicao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataAtribuicao <= dataFim.Value);

            return await query.CountAsync();
        }

        /// <summary>
        /// Lista as atribuições para uma empresa específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Lista de atribuições da empresa</returns>
        public async Task<List<AtribuicaoLead>> ListAtribuicoesPorEmpresaAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<AtribuicaoLead>()
                .Include(a => a.Lead)
                .Include(a => a.MembroAtribuido)
                .Include(a => a.TipoAtribuicao)
                .Include(a => a.RegraDistribuicao)
                .Where(a => a.Lead.EmpresaId == empresaId && !a.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataAtribuicao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataAtribuicao <= dataFim.Value);

            return await query
                .OrderByDescending(a => a.DataAtribuicao)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém estatísticas de distribuição por vendedor
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início (opcional)</param>
        /// <param name="dataFim">Data de fim (opcional)</param>
        /// <returns>Lista de estatísticas por vendedor</returns>
        public async Task<List<object>> GetDistribuicoesPorVendedorAsync(
            int empresaId, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<AtribuicaoLead>()
                .Include(a => a.Lead)
                .Include(a => a.MembroAtribuido)
                .Where(a => a.Lead.EmpresaId == empresaId && !a.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataAtribuicao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataAtribuicao <= dataFim.Value);

            var resultado = await query
                .GroupBy(a => new { a.MembroAtribuidoId, a.MembroAtribuido.Usuario.Nome })
                .Select(g => new
                {
                    VendedorId = g.Key.MembroAtribuidoId,
                    VendedorNome = g.Key.Nome,
                    TotalLeads = g.Count(),
                    UltimaAtribuicao = g.Max(a => a.DataAtribuicao)
                })
                .OrderByDescending(r => r.TotalLeads)
                .ToListAsync();

            return resultado.Cast<object>().ToList();
        }
    }
}