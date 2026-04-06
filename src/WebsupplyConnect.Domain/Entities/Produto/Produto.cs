using System;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Produto
{
    /// <summary>
    /// Representa um produto ou serviço que pode ser oferecido em oportunidades de venda.
    /// Um produto pode estar disponível para múltiplas empresas e associado a múltiplas oportunidades.
    /// </summary>
    public class Produto : EntidadeBase
    {
        /// <summary>
        /// Nome do produto
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição do produto
        /// </summary>
        public string? Descricao { get; private set; }

        /// <summary>
        /// Valor de referência do produto
        /// </summary>
        public decimal? ValorReferencia { get; private set; }

        /// <summary>
        /// URL do produto (para página externa ou catálogo)
        /// </summary>
        public string? Url { get; private set; }

        /// <summary>
        /// Indica se o produto está ativo no sistema
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// Lista de disponibilidade do produto para empresas
        /// </summary>
        public virtual ICollection<ProdutoEmpresa> ProdutoEmpresas { get; private set; }

        /// <summary>
        /// Lista de histórico de modificações
        /// </summary>
        public virtual ICollection<ProdutoHistorico> Historicos { get; private set; }

        /// <summary>
        /// Construtor protegido para Entity Framework
        /// </summary>
        protected Produto()
        {
            ProdutoEmpresas = new List<ProdutoEmpresa>();
            Historicos = new List<ProdutoHistorico>();
        }

        /// <summary>
        /// Construtor para criação de novo produto
        /// </summary>
        /// <param name="nome">Nome do produto</param>
        /// <param name="descricao">Descrição do produto (opcional)</param>
        /// <param name="valorReferencia">Valor de referência (opcional)</param>
        /// <param name="url">URL do produto (opcional)</param>
        public Produto(
            string nome,
            string? descricao = null,
            decimal? valorReferencia = null,
            string? url = null) : this()
        {
            ValidarParametros(nome);

            Nome = nome.Trim();
            Descricao = descricao?.Trim();
            ValorReferencia = valorReferencia;
            Url = url?.Trim();
            Ativo = true;
            DataCriacao = DateTime.Now;
            DataModificacao = DateTime.Now;
        }

        /// <summary>
        /// Atualiza as informações básicas do produto
        /// </summary>
        /// <param name="nome">Novo nome</param>
        /// <param name="descricao">Nova descrição</param>
        /// <param name="url">Nova URL</param>
        public void atualizarinformacoes(string nome, string? descricao = null, string? url = null)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("o nome do produto é obrigatório");
            
            Nome = nome = nome.Trim();
            Descricao = descricao = descricao?.Trim();
            Url = url = url?.Trim();
            DataModificacao = DateTime.Now;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o valor de referência do produto
        /// </summary>
        /// <param name="novoValor">Novo valor de referência</param>
        public void AtualizarValorReferencia(decimal? novoValor)
        {
            if (novoValor.HasValue && novoValor < 0)
                throw new DomainException("O valor de referência não pode ser negativo");

            ValorReferencia = novoValor;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa o produto
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa o produto
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Exclui o produto
        /// </summary>
        public void Excluir()
        {
            Excluido = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Adiciona uma empresa à lista de empresas que podem usar este produto
        /// </summary>
        /// <param name="empresaId">ID da empresa a adicionar</param>
        /// <param name="valorPersonalizado">Valor personalizado para esta empresa (opcional)</param>
        /// <returns>A entidade de relacionamento criada</returns>
        public ProdutoEmpresa AdicionarEmpresa(int empresaId, decimal? valorPersonalizado = null)
        {
            if (empresaId <= 0)
                throw new DomainException("ID da empresa inválido");

            if (ProdutoEmpresas.Any(pe => pe.EmpresaId == empresaId))
                throw new DomainException("Esta empresa já tem acesso a este produto");

            var produtoEmpresa = new ProdutoEmpresa(Id, empresaId, valorPersonalizado);
            ProdutoEmpresas.Add(produtoEmpresa);

            AtualizarDataModificacao();

            return produtoEmpresa;
        }

        /// <summary>
        /// Remove uma empresa da lista de empresas que podem usar este produto
        /// </summary>
        /// <param name="empresaId">ID da empresa a remover</param>
        public void RemoverEmpresa(int empresaId)
        {
            var produtoEmpresa = ProdutoEmpresas.FirstOrDefault(pe => pe.EmpresaId == empresaId);
            if (produtoEmpresa == null)
                throw new DomainException("Esta empresa não está na lista de empresas do produto");

            ProdutoEmpresas.Remove(produtoEmpresa);

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se uma empresa tem acesso a este produto
        /// </summary>
        /// <param name="empresaId">ID da empresa a verificar</param>
        /// <returns>True se a empresa tem acesso ao produto</returns>
        public bool EmpresaTemAcesso(int empresaId)
        {
            return ProdutoEmpresas.Any(pe => pe.EmpresaId == empresaId);
        }

        /// <summary>
        /// Obtém o valor personalizado para uma empresa específica, ou o valor padrão se não houver personalização
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Valor personalizado ou valor padrão</returns>
        public decimal? ObterValorParaEmpresa(int empresaId)
        {
            var produtoEmpresa = ProdutoEmpresas.FirstOrDefault(pe => pe.EmpresaId == empresaId);
            if (produtoEmpresa == null)
                throw new DomainException("Esta empresa não tem acesso a este produto");

            return produtoEmpresa.ValorPersonalizado ?? ValorReferencia;
        }

        /// <summary>
        /// Valida os parâmetros do construtor
        /// </summary>
        private static void ValidarParametros(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do produto é obrigatório");

            if (nome.Length > 200)
                throw new DomainException("O nome do produto não pode ter mais de 200 caracteres");
        }
    }
}
