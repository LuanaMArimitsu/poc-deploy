using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    internal class CanalRepository : BaseRepository, ICanalRepository
    { /// <summary>
      /// Construtor do repositório
      /// </summary>
      /// <param name="dbContext">Contexto do banco de dados</param>
        public CanalRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Obtém um canal existente a partir do número de WhatsApp informado.
        /// </summary>
        /// <param name="whatsAppNumber">Número de WhatsApp a ser consultado.</param>
        /// <returns>Retorna o canal correspondente ou null se não for encontrado.</returns>
        public async Task<Canal?> GetCanalByWhatsAppNumberAsync(string whatsAppNumber)
        {
            if (string.IsNullOrWhiteSpace(whatsAppNumber))
                throw new DomainException("O número de WhatsApp não pode ser vazio.", nameof(Canal));

            return await _context.Set<Canal>()
                .FirstOrDefaultAsync(c => c.WhatsAppNumero == whatsAppNumber);
        }

        public async Task<List<Canal>> GetListCanaisByWhatsAppNumber(string whatsAppNumber)
        {
            if (string.IsNullOrWhiteSpace(whatsAppNumber))
                throw new DomainException("O número de WhatsApp não pode ser vazio.", nameof(Canal));

            return await _context.Set<Canal>()
                .Where(c => c.WhatsAppNumero == whatsAppNumber && c.Ativo)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se já existe um canal com o nome informado.
        /// </summary>
        /// <param name="channelName">Nome do canal a ser verificado.</param>
        /// <returns>Retorna true se já existir um canal com esse nome; caso contrário, false.</returns>
        public async Task<bool> CanalNameExistsAsync(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                throw new DomainException("O nome do canal não pode ser vazio.", nameof(Canal));

            return await _context.Set<Canal>()
                .AnyAsync(c => c.Nome == channelName);
        }

        /// <summary>
        /// Lista canais do sistema com filtros opcionais de empresa e status ativo
        /// </summary>
        /// <param name="empresaId">ID da empresa para filtrar os canais (opcional - se não informado, retorna canais de todas as empresas)</param>
        /// <param name="ativo">Status ativo do canal para filtrar (opcional - true=apenas ativos, false=apenas inativos, null=todos os status)</param>
        /// <returns>Lista de canais ordenada por nome que atendem aos critérios especificados</returns>
        public async Task<List<Canal>> ListCanaisAsync(int? empresaId = null, bool? ativo = null)
        {
            var query = _context.Set<Canal>().AsQueryable();

            if (empresaId.HasValue)
            {
                query = query.Where(x => x.EmpresaId == empresaId.Value);
            }

            if (ativo.HasValue)
            {
                query = query.Where(x => x.Ativo == ativo.Value);
            }

            return await query
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }


        /// <summary>
        /// Busca um Canal pelo Número WhatsApp
        /// </summary>
        /// <param name="whatsNumero">Número do WhatsApp</param>
        /// <returns>Canal encontrado ou null</returns>
        public async Task<Canal?> GetCanalByNumeroAsync(string whatsNumero)
        {
            if (string.IsNullOrWhiteSpace(whatsNumero))
                return null;

            return await _context.Canal.AsNoTracking().FirstOrDefaultAsync(c => c.WhatsAppNumero == whatsNumero);
        }

        /// <summary>
        /// Retorna o canal.
        /// </summary>
        /// <param name="canalId">Id do canal</param>
        /// <returns>Canal encontrado</returns>
        public async Task<Canal?> GetCanalAsync(int canalId)
        {
            return await _context.Canal
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == canalId);
        }

        public async Task<bool> ExistemCanaisAsync(List<int> canalIds)
        {
            var total = await _context.Canal
                .CountAsync(c => canalIds.Contains(c.Id));

            return total == canalIds.Distinct().Count();
        }

        public async Task<List<Canal>> ListarCanaisPorEmpresasAsync(List<int> empresaIds)
        {
            return await _context.Canal
                .Where(c => empresaIds.Contains(c.EmpresaId) && c.Ativo)
                .ToListAsync();
        }

        public async Task<List<Canal>> ObterCanaisPorIdsAsync(List<int> canalIds)
        {
            return await _context.Canal
                .Where(c => canalIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<List<string>> GetConfiguracaoIntegracao()
        {
            return await _context.Canal.
                Where(e => e.Ativo && !string.IsNullOrEmpty(e.ConfiguracaoIntegracao))
                .Select(e => e.ConfiguracaoIntegracao!)
                .ToListAsync();
        }

        public async Task<Canal?> GetCanalByEmpresaId(int empresaId)
        {
            return await _context.Canal
                .Where(c => c.EmpresaId == empresaId && c.Ativo)
                .FirstOrDefaultAsync();
        }
    }
}
