using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Permissao.Permissao;
using WebsupplyConnect.Application.Interfaces.Perfil;
using WebsupplyConnect.Domain.Interfaces.Permissao;

namespace WebsupplyConnect.Application.Services.Perfil
{
    public class PermissaoReaderService(ILogger<PermissaoReaderService> logger, IPermissaoRepository permissaoRepository) : IPermissaoReaderService
    {
        private readonly ILogger<PermissaoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IPermissaoRepository _permissaoRepository = permissaoRepository ?? throw new ArgumentNullException(nameof(permissaoRepository));
        /// <summary>
        /// Busca todas as permissõessem filtro.
        /// </summary>
        public async Task<IReadOnlyList<PermissaoDTO>> GetPermissoes()
        {
            try
            {
                var permissoes = await _permissaoRepository.GetListByPredicateAsync<Domain.Entities.Permissao.Permissao>(x => !x.Excluido);

                var itens = permissoes.Select(x => new PermissaoDTO
                {
                    Id = x.Id,
                    Categoria = x.Categoria,
                    Descricao = x.Descricao,
                    Modulo = x.Modulo,
                    Nome = x.Nome,
                    IsCritica = x.IsCritica,
                    Ativa = x.Ativa
                }).ToList();

                return itens;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Busca todas as permissões com filtro.
        /// </summary>
        public async Task<PermissaoPaginadaDTO> GetPermissoes(PermissaoFiltroDTO filtro)
        {
            try
            {
                var (permissoes, totalItens) = await _permissaoRepository.GetPermissoesAsync(filtro.Nome, filtro.Modulo, filtro.Criticas, filtro.Categoria, filtro.Pagina, filtro.TamanhoPagina);

                var itens = permissoes.Select(x => new PermissaoDTO
                {
                    Id = x.Id,
                    Categoria = x.Categoria,
                    Descricao = x.Descricao,
                    Modulo = x.Modulo,
                    Nome = x.Nome,
                    IsCritica = x.IsCritica,
                    Ativa = x.Ativa
                }).ToList(); 

                var totalPaginas = (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina);

                return new PermissaoPaginadaDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro.Pagina,
                    TotalPaginas = totalPaginas,
                    Itens = itens
                };

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Busca os detalhes de uma permissão pelo ID.
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        /// <returns>Permissão encontrada ou null se não existir</returns>
        public async Task<Domain.Entities.Permissao.Permissao?> GetPermissaoPorIdAsync(int permissaoId)
        {
            return await _permissaoRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId);
        }
    }
}

