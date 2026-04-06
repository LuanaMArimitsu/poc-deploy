using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Produto
{
    /// <summary>
    /// Entidade de relacionamento entre Produto e Empresa.
    /// Permite que um produto esteja disponível para múltiplas empresas,
    /// com possibilidade de personalização de valores.
    /// </summary>
    public class ProdutoEmpresa
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID do produto
        /// </summary>
        public int ProdutoId { get; private set; }

        /// <summary>
        /// Produto relacionado
        /// </summary>
        public virtual Produto Produto { get; private set; }

        /// <summary>
        /// ID da empresa
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Empresa relacionada
        /// </summary>
        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Valor personalizado para esta empresa (opcional)
        /// </summary>
        public decimal? ValorPersonalizado { get; private set; }

        /// <summary>
        /// Data em que a associação foi criada
        /// </summary>
        public DateTime DataAssociacao { get; private set; }


        /// <summary>
        /// Construtor protegido para Entity Framework
        /// </summary>
        protected ProdutoEmpresa()
        {
        }

        /// <summary>
        /// Construtor para criar novo relacionamento
        /// </summary>
        /// <param name="produtoId">ID do produto</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="valorPersonalizado">Valor personalizado (opcional)</param>
        public ProdutoEmpresa(int produtoId, int empresaId, decimal? valorPersonalizado = null)
        {
            if (produtoId <= 0)
                throw new DomainException("ID do produto inválido");

            if (empresaId <= 0)
                throw new DomainException("ID da empresa inválido");

            if (valorPersonalizado.HasValue && valorPersonalizado.Value < 0)
                throw new DomainException("O valor personalizado não pode ser negativo");

            ProdutoId = produtoId;
            EmpresaId = empresaId;
            ValorPersonalizado = valorPersonalizado;
            DataAssociacao = DateTime.Now;
        }

        /// <summary>
        /// Atualiza o valor personalizado
        /// </summary>
        /// <param name="novoValor">Novo valor personalizado</param>
        public void AtualizarValorPersonalizado(decimal? novoValor)
        {
            if (novoValor.HasValue && novoValor.Value < 0)
                throw new DomainException("O valor personalizado não pode ser negativo");

            ValorPersonalizado = novoValor;
        }
    }
}
