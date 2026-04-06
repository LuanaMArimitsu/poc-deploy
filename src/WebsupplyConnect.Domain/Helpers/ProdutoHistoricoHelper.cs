using WebsupplyConnect.Domain.Entities.Produto;

namespace WebsupplyConnect.Domain.Helpers
{
    /// <summary>
    /// Helper para facilitar a criação de registros históricos com detalhes estruturados
    /// </summary>
    public static class ProdutoHistoricoHelper
    {
        /// <summary>
        /// Cria um registro para criação de produto
        /// </summary>
        public static ProdutoHistorico CriacaoProduto(int produtoId, int usuarioId, string nomeProduto, decimal? valorReferencia, string url)
        {
            var detalhes = new DetalhesAtualizacao
            {
                Campos = new System.Collections.Generic.List<DetalhesCampo>
                {
                    new DetalhesCampo { Campo = "Nome", ValorNovo = nomeProduto },
                    new DetalhesCampo { Campo = "Valor Referência", ValorNovo = valorReferencia?.ToString("C") ?? "N/A" },
                    new DetalhesCampo { Campo = "URL", ValorNovo = url ?? "N/A" },
                    new DetalhesCampo { Campo = "Status", ValorNovo = "Ativo" }
                }
            };

            return new ProdutoHistorico(
                produtoId,
                usuarioId,
                TipoOperacaoProdutoEnum.Criacao,
                $"Produto {nomeProduto} foi criado.",
                detalhes
            );
        }

        /// <summary>
        /// Cria um registro para atualização de produto
        /// </summary>
        public static ProdutoHistorico AtualizacaoProduto(int produtoId, int usuarioId,
            string nomeAntigo = null, string nomeNovo = null,
            string descricaoAntiga = null, string descricaoNova = null,
            decimal? valorAntigo = null, decimal? valorNovo = null,
            string urlAntiga = null, string urlNova = null)
        {
            var detalhes = new DetalhesAtualizacao { Campos = new System.Collections.Generic.List<DetalhesCampo>() };

            if (nomeAntigo != nomeNovo && (nomeAntigo != null || nomeNovo != null))
            {
                detalhes.Campos.Add(new DetalhesCampo
                {
                    Campo = "Nome",
                    ValorAntigo = nomeAntigo,
                    ValorNovo = nomeNovo
                });
            }

            if (descricaoAntiga != descricaoNova && (descricaoAntiga != null || descricaoNova != null))
            {
                detalhes.Campos.Add(new DetalhesCampo
                {
                    Campo = "Descrição",
                    ValorAntigo = "Descrição anterior",
                    ValorNovo = "Nova descrição"
                });
            }

            if (valorAntigo != valorNovo && (valorAntigo.HasValue || valorNovo.HasValue))
            {
                detalhes.Campos.Add(new DetalhesCampo
                {
                    Campo = "Valor Referência",
                    ValorAntigo = valorAntigo?.ToString("C") ?? "N/A",
                    ValorNovo = valorNovo?.ToString("C") ?? "N/A"
                });
            }

            if (urlAntiga != urlNova && (urlAntiga != null || urlNova != null))
            {
                detalhes.Campos.Add(new DetalhesCampo
                {
                    Campo = "URL",
                    ValorAntigo = urlAntiga ?? "N/A",
                    ValorNovo = urlNova ?? "N/A"
                });
            }

            return new ProdutoHistorico(
                produtoId,
                usuarioId,
                TipoOperacaoProdutoEnum.Atualizacao,
                "Informações do produto foram atualizadas.",
                detalhes
            );
        }

        /// <summary>
        /// Cria um registro para empresa adicionada
        /// </summary>
        public static ProdutoHistorico EmpresaAdicionada(int produtoId, int usuarioId, int empresaId, string nomeEmpresa, decimal? valorPersonalizado)
        {
            var detalhes = new DetalhesEmpresa
            {
                EmpresaId = empresaId,
                NomeEmpresa = nomeEmpresa,
                UsandoValorReferencia = !valorPersonalizado.HasValue,
                ValorPersonalizado = valorPersonalizado?.ToString("C") ?? "Usando valor de referência"
            };

            return new ProdutoHistorico(
                produtoId,
                usuarioId,
                TipoOperacaoProdutoEnum.EmpresaAdicionada,
                $"Empresa {nomeEmpresa} foi associada ao produto.",
                detalhes
            );
        }

        /// <summary>
        /// Cria um registro para alteração de valor de empresa
        /// </summary>
        public static ProdutoHistorico ValorEmpresaAlterado(int produtoId, int usuarioId, int empresaId, string nomeEmpresa, decimal? valorAntigo, decimal? valorNovo)
        {
            var detalhes = new DetalhesAtualizacao
            {
                Campos = new System.Collections.Generic.List<DetalhesCampo>
                {
                    new DetalhesCampo
                    {
                        Campo = "Valor Personalizado",
                        ValorAntigo = valorAntigo?.ToString("C") ?? "Usando valor de referência",
                        ValorNovo = valorNovo?.ToString("C") ?? "Usando valor de referência"
                    }
                }
            };

            return new ProdutoHistorico(
                produtoId,
                usuarioId,
                TipoOperacaoProdutoEnum.ValorEmpresaAlterado,
                $"Valor personalizado para empresa {nomeEmpresa} atualizado.",
                detalhes
            );
        }
    }
}

