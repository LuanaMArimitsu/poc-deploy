using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Lead
{
    internal class CampanhaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), ICampanhaRepository
    {
        public async Task<(List<Campanha> Itens, int TotalItens)> ListarCampanhasFiltroAsync(
            string? busca,
            int? empresaId,
            string? codigo,
            bool? ativa,
            bool? temporaria,
            int? equipeId,
            DateTime? dataCadastro,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? pagina,
            int? tamanhoPagina
        )
        {
            var query = _context.Campanha.Where(e => !e.Excluido).AsQueryable();

            if (empresaId.HasValue && empresaId > 0)
                query = query.Where(c => c.EmpresaId == empresaId.Value);

            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(c => c.Codigo.Contains(codigo));

            if (temporaria.HasValue)
                query = query.Where(c => c.Temporaria == temporaria.Value);

            if (ativa.HasValue)
                query = query.Where(c => c.Ativo == ativa.Value);

            if (equipeId.HasValue)
                query = query.Where(c => c.EquipeId == equipeId.Value);

            if (dataCadastro.HasValue)
                query = query.Where(c => c.DataCriacao.Date == dataCadastro.Value.Date);

            if (dataInicio.HasValue)
                query = query.Where(c => c.DataInicio.HasValue && c.DataInicio.Value.Date >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                query = query.Where(c => c.DataFim.HasValue && c.DataFim.Value.Date <= dataFim.Value.Date);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(c => c.Nome.Contains(busca));

            var totalItens = await query.CountAsync();

            var queryOrdenada = query
                .OrderByDescending(l => l.DataCriacao)
                .Include(l => l.Empresa);

            IQueryable<Campanha> queryFinal = queryOrdenada;

            if (pagina.HasValue && tamanhoPagina.HasValue && pagina > 0 && tamanhoPagina > 0)
            {
                int paginaSeguro = pagina.Value;
                int tamanhoSeguro = tamanhoPagina.Value;

                queryFinal = queryOrdenada
                    .Skip((paginaSeguro - 1) * tamanhoSeguro)
                    .Take(tamanhoSeguro);
            }

            var itens = await queryFinal.ToListAsync();

            return (itens, totalItens);
        }

        public async Task<IEnumerable<Campanha>> ListagemSimplesAsync(int empresaId)
        {
            return await _context.Campanha
                .Where(c => c.IdTransferida == null
                            && c.Temporaria == false
                            && c.Ativo == true
                            && c.EmpresaId == empresaId
                            && c.Excluido == false)
                .ToListAsync();
        }
    }
}
