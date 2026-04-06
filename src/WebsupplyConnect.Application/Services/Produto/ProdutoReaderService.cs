using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Produto;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Produto;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Produto;

namespace WebsupplyConnect.Application.Services.Produto
{
    public class ProdutoReaderService(
        IProdutoRepository produtoRepository
    ) : IProdutoReaderService
    {

        private readonly IProdutoRepository _produtoRepository = produtoRepository;

        public async Task<PagedResponseDTO<ProdutoListagemDTO>> ListarProdutosPaginadoAsync(ProdutoFiltroRequestDTO filtro)
        {
            if (filtro.EmpresaId <= 0)
            {
                throw new AppException("Empresa id não pode ser nulo.");
            }

            var query = _produtoRepository.ObterQueryProdutosComEmpresas(filtro.EmpresaId);

            query = AplicarFiltros(query, filtro);
            query = AplicarOrdenacao(query, filtro);

            var totalItens = await query.CountAsync();

            List<WebsupplyConnect.Domain.Entities.Produto.Produto> produtos;

            if (filtro.Pagina <= 0 || filtro.TamanhoPagina <= 0)
            {
                produtos = await query.ToListAsync();
            }
            else
            {
                produtos = await query
                    .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                    .Take(filtro.TamanhoPagina)
                    .ToListAsync();
            }

            var itens = produtos.Select(p => new ProdutoListagemDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                ValorReferencia = p.ValorReferencia,
                Ativo = p.Ativo
            }).ToList();

            return new PagedResponseDTO<ProdutoListagemDTO>
            {
                Itens = itens,
                PaginaAtual = filtro.Pagina <= 0 ? 1 : filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina <= 0 ? totalItens : filtro.TamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = filtro.TamanhoPagina <= 0
                        ? 1
                        : (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina)
            };
        }

        private IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> AplicarFiltros(
            IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> query, ProdutoFiltroRequestDTO filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var termo = filtro.Busca.ToLower();
                query = query.Where(p =>
                    p.Nome.ToLower().Contains(termo) ||
                    (p.Descricao != null && p.Descricao.ToLower().Contains(termo)));
            }

            if (filtro.Ativo.HasValue)
            {
                query = query.Where(p => p.Ativo == filtro.Ativo.Value);
            }

            return query;
        }

        private IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> AplicarOrdenacao(
            IQueryable<WebsupplyConnect.Domain.Entities.Produto.Produto> query, ProdutoFiltroRequestDTO filtro)
        {
            bool asc = filtro.DirecaoOrdenacao.ToUpper() == "ASC";

            return filtro.OrdenarPor.ToLower() switch
            {
                "valorreferencia" => asc ? query.OrderBy(p => p.ValorReferencia) : query.OrderByDescending(p => p.ValorReferencia),
                "ativo" => asc ? query.OrderBy(p => p.Ativo) : query.OrderByDescending(p => p.Ativo),
                _ => asc ? query.OrderBy(p => p.Nome) : query.OrderByDescending(p => p.Nome),
            };
        }

        public async Task<ProdutoDetalhadoDTO> ObterDetalhadoAsync(int id)
        {
            var produto = await _produtoRepository.ObterDetalhePorIdAsync(id)
                ?? throw new DomainException("Produto não encontrado.");

            return new ProdutoDetalhadoDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                ValorReferencia = produto.ValorReferencia,
                Url = produto.Url,
                Ativo = produto.Ativo,

                Empresas = produto.ProdutoEmpresas.Select(pe => new ProdutoEmpresaDTO
                {
                    EmpresaId = pe.EmpresaId,
                    NomeEmpresa = pe.Empresa?.Nome ?? "",
                    ValorPersonalizado = pe.ValorPersonalizado,
                    DataAssociacao = pe.DataAssociacao
                }).ToList(),

                Historico = produto.Historicos.OrderByDescending(h => h.DataOperacao)
                .Select(h => new ProdutoHistoricoDTO
                {
                    DataOperacao = h.DataOperacao,
                    NomeUsuario = h.Usuario?.Nome ?? "",
                    NomeOperacao = h.TipoOperacao?.Nome ?? "",
                    Descricao = h.Descricao,
                    Json = h.DetalhesJson
                }).ToList()
            };
        }

    }
}
