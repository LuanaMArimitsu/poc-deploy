using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Produto;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Produto;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Produto;

namespace WebsupplyConnect.Application.Services.Produto
{
    public class ProdutoWriterService(
        IUnitOfWork unitOfWork,
        IProdutoRepository produtoRepository,
        IEmpresaReaderService empresaReaderService,
        IProdutoHistoricoWriterService produtoHistoricoWriterService
    ) : IProdutoWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService;
        private readonly IProdutoRepository _produtoRepository = produtoRepository;
        private readonly IProdutoHistoricoWriterService _produtoHistoricoWriterService = produtoHistoricoWriterService;

        public async Task<WebsupplyConnect.Domain.Entities.Produto.Produto> AdicionarProdutoAsync(AdicionarProdutoRequestDTO dto, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var empresaExiste = await _empresaReaderService.EmpresaExistsAsync(dto.EmpresaId);
                if (!empresaExiste)
                    throw new DomainException("Empresa informada não foi encontrada.");

                var produto = new WebsupplyConnect.Domain.Entities.Produto.Produto(
                    dto.Nome, dto.Descricao, dto.ValorReferencia, dto.Url
                );

                await _produtoRepository.AdicionarAsync(produto);
                await _unitOfWork.SaveChangesAsync();

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.Criacao,
                    "Produto criado.",
                    dto
                );
                await _unitOfWork.CommitAsync();

                produto.AdicionarEmpresa(dto.EmpresaId);
                await _unitOfWork.SaveChangesAsync();

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.EmpresaAdicionada,
                    $"Produto vinculado à empresa ID {dto.EmpresaId}.",
                    new { dto.EmpresaId }
                );

                await _unitOfWork.CommitAsync();

                return produto;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task VincularEmpresaAsync(VincularEmpresaProdutoRequestDTO dto, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterComEmpresasAsync(dto.ProdutoId)
                      ?? throw new DomainException("Produto não encontrado.");

                var empresa = await _empresaReaderService.ObterPorId(dto.EmpresaId)
                              ?? throw new DomainException("Empresa informada não foi encontrada.");

                var produtoEmpresa = produto.AdicionarEmpresa(dto.EmpresaId, dto.ValorPersonalizado);

                await _unitOfWork.SaveChangesAsync();

                var detalhes = new DetalhesEmpresa
                {
                    EmpresaId = dto.EmpresaId,
                    NomeEmpresa = empresa.Nome,
                    ValorPersonalizado = dto.ValorPersonalizado?.ToString("F2") ?? "null",
                    UsandoValorReferencia = !dto.ValorPersonalizado.HasValue
                };

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.EmpresaAdicionada,
                    $"Produto vinculado à empresa {empresa.Nome} (ID {dto.EmpresaId}).",
                    detalhes
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task ExcluirProdutoAsync(int produtoId, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterComEmpresasAsync(produtoId)
                    ?? throw new DomainException("Produto não encontado");

                if (produto.Excluido) throw new DomainException("Produto já foi excluído");

                produto.Desativar();
                produto.Excluir();

                var empresasRemovidas = produto.ProdutoEmpresas.Select(pe => new DetalhesEmpresa
                {
                    EmpresaId = pe.EmpresaId,
                    NomeEmpresa = pe.Empresa.Nome,
                    ValorPersonalizado = pe.ValorPersonalizado?.ToString("F2") ?? "null",
                    UsandoValorReferencia = !pe.ValorPersonalizado.HasValue
                }).ToList();

                await _produtoRepository.RemoverVinculosEmpresasAsync(produto.Id);

                await _produtoRepository.AtualizarAsync(produto);
                await _unitOfWork.SaveChangesAsync();

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.Desativacao,
                    $"Produto {produto.Id} excluido, desativado empresas desvinculadas.",
                    new
                    {
                        produto.Id,
                        produto.Nome,
                        produto.ValorReferencia,
                        produto.Url,
                        EmpresasRemovidas = empresasRemovidas
                    }
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RemoverEmpresaDoProdutoAsync(int produtoId, int empresaId, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterComEmpresasAsync(produtoId)
                    ?? throw new DomainException("Produto não encontrado");

                var empresa = produto.ProdutoEmpresas.FirstOrDefault(pe => pe.EmpresaId == empresaId)
                    ?? throw new DomainException("A empresa não está associada a este produto.");

                var detalhes = new DetalhesEmpresa
                {
                    EmpresaId = empresa.EmpresaId,
                    NomeEmpresa = empresa.Empresa.Nome,
                    ValorPersonalizado = empresa.ValorPersonalizado?.ToString("F2") ?? "null",
                    UsandoValorReferencia = !empresa.ValorPersonalizado.HasValue
                };

                produto.RemoverEmpresa(empresaId);

                await _produtoRepository.RemoverVinculoEmpresaAsync(produtoId, empresaId);

                await _produtoRepository.AtualizarAsync(produto);
                await _unitOfWork.SaveChangesAsync();

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.EmpresaRemovida,
                    $"Produto desvinculado à empresa {detalhes.NomeEmpresa} (ID {detalhes.EmpresaId}).",
                    detalhes
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task AlterarStatusProdutoAsync(int produtoId, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterPorIdAsync(produtoId)
                    ?? throw new DomainException("Produto não encontrado.");

                bool statusAnterior = produto.Ativo;
                int tipoOperacaoId;
                string descricao;

                if (statusAnterior)
                {
                    produto.Desativar();
                    tipoOperacaoId = TipoOperacaoProdutoEnum.Desativacao;
                    descricao = "Produto desativado.";
                }
                else
                {
                    produto.Ativar();
                    tipoOperacaoId = TipoOperacaoProdutoEnum.Ativacao;
                    descricao = "Produto ativado.";
                }

                await _produtoRepository.AtualizarAsync(produto);
                await _unitOfWork.SaveChangesAsync();

                var detalhes = new DetalhesAtualizacao
                {
                    Campos = new List<DetalhesCampo>
                    {
                        new DetalhesCampo
                        {
                            Campo = "Ativo",
                            ValorAntigo = statusAnterior.ToString(),
                            ValorNovo = produto.Ativo.ToString()
                        }
                    }
                };

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    tipoOperacaoId,
                    descricao,
                    detalhes
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task AtualizarInformacoesAsync(AtualizarProdutoRequestDTO dto, int usuarioId, int produtoId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterPorIdAsync(produtoId)
                    ?? throw new DomainException("Produto não encontrado.");

                var detalhes = new DetalhesAtualizacao();

                if (!string.Equals(produto.Nome, dto.Nome, StringComparison.Ordinal))
                {
                    detalhes.Campos.Add(new DetalhesCampo
                    {
                        Campo = "Nome",
                        ValorAntigo = produto.Nome,
                        ValorNovo = dto.Nome
                    });
                }

                var descricaoAntiga = produto.Descricao ?? string.Empty;
                var descricaoNova = dto.Descricao ?? string.Empty;
                if (!string.Equals(descricaoAntiga, descricaoNova, StringComparison.Ordinal))
                {
                    detalhes.Campos.Add(new DetalhesCampo
                    {
                        Campo = "Descrição",
                        ValorAntigo = descricaoAntiga,
                        ValorNovo = descricaoNova
                    });
                }

                var urlAntiga = produto.Url ?? string.Empty;
                var urlNova = dto.Url ?? string.Empty;
                if (!string.Equals(urlAntiga, urlNova, StringComparison.Ordinal))
                {
                    detalhes.Campos.Add(new DetalhesCampo
                    {
                        Campo = "URL",
                        ValorAntigo = urlAntiga,
                        ValorNovo = urlNova
                    });
                }

                if (produto.ValorReferencia != dto.ValorReferencia)
                {
                    detalhes.Campos.Add(new DetalhesCampo
                    {
                        Campo = "ValorReferencia",
                        ValorAntigo = produto.ValorReferencia?.ToString() ?? string.Empty,
                        ValorNovo = dto.ValorReferencia?.ToString() ?? string.Empty
                    });
                }

                if (!detalhes.Campos.Any())
                    throw new AppException("Nenhuma alteração foi detectada.");

                produto.atualizarinformacoes(dto.Nome, dto.Descricao, dto.Url);
                produto.AtualizarValorReferencia(dto.ValorReferencia);

                await _produtoRepository.AtualizarAsync(produto);
                await _unitOfWork.SaveChangesAsync();

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.Atualizacao,
                    "Informações do produto atualizadas.",
                    detalhes
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task AlterarValorProdutoEmpresaAsync(int produtoId, int empresaId, AlterarValorProdutoEmpresaRequestDTO dto, int usuarioId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var produto = await _produtoRepository.ObterPorIdAsync(produtoId)
                    ?? throw new AppException("Produto não encontrado.");

                var vinculo = await _produtoRepository.ObterVinculoProdutoEmpresaAsync(produtoId, empresaId)
                    ?? throw new AppException("Vínculo entre produto e empresa não encontrado.");

                var valorAntigo = vinculo.ValorPersonalizado;
                vinculo.AtualizarValorPersonalizado(dto.NovoValor);

                await _produtoRepository.AtualizarVinculoProdutoEmpresaAsync(vinculo);
                await _unitOfWork.SaveChangesAsync();

                var detalhes = new DetalhesAtualizacao();
                detalhes.Campos.Add(new DetalhesCampo
                {
                    Campo = "ValorPersonalizado",
                    ValorAntigo = valorAntigo?.ToString() ?? string.Empty,
                    ValorNovo = dto.NovoValor?.ToString() ?? string.Empty
                });

                await _produtoHistoricoWriterService.RegistrarAsync(
                    produto.Id,
                    usuarioId,
                    TipoOperacaoProdutoEnum.ValorEmpresaAlterado,
                    $"Valor personalizado para a empresa {empresaId} alterado.",
                    detalhes
                );

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
