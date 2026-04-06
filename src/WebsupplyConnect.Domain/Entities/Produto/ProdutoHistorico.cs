using System.Text.Json;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Produto
{
    /// <summary>
    /// Entidade que armazena o histórico de modificações em produtos com detalhes estruturados
    /// </summary>
    public class ProdutoHistorico
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID do produto relacionado
        /// </summary>
        public int ProdutoId { get; private set; }

        /// <summary>
        /// Produto relacionado
        /// </summary>
        public virtual Produto Produto { get; private set; }

        /// <summary>
        /// ID do usuário que realizou a operação
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// Referência para o usuário que realizou a operação
        /// </summary>
        public virtual Usuario.Usuario Usuario { get; private set; }

        /// <summary>
        /// ID do tipo de operação realizada
        /// </summary>
        public int TipoOperacaoId { get; private set; }

        /// <summary>
        /// Referência para o tipo de operação
        /// </summary>
        public virtual ProdutoOperacaoTipo TipoOperacao { get; private set; }

        /// <summary>
        /// Descrição principal da operação
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Detalhes estruturados da operação em formato JSON
        /// Armazena as alterações de forma estruturada para facilitar a renderização
        /// </summary>
        public string DetalhesJson { get; private set; }

        /// <summary>
        /// Data e hora da operação
        /// </summary>
        public DateTime DataOperacao { get; private set; }

        /// <summary>
        /// Construtor protegido para Entity Framework
        /// </summary>
        protected ProdutoHistorico()
        {
        }

        /// <summary>
        /// Construtor para criar um novo registro de histórico com detalhes estruturados
        /// </summary>
        /// <param name="produtoId">ID do produto</param>
        /// <param name="usuarioId">ID do usuário que realizou a operação</param>
        /// <param name="tipoOperacaoId">ID do tipo de operação</param>
        /// <param name="descricao">Descrição principal da operação</param>
        /// <param name="detalhes">Objeto contendo os detalhes estruturados</param>
        public ProdutoHistorico(
            int produtoId,
            int usuarioId,
            int tipoOperacaoId,
            string descricao,
            object detalhes)
        {
            if (produtoId <= 0)
                throw new DomainException("ID do produto inválido");

            if (usuarioId <= 0)
                throw new DomainException("ID do usuário inválido");

            if (tipoOperacaoId <= 0)
                throw new DomainException("ID do tipo de operação inválido");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("Descrição da operação é obrigatória");

            ProdutoId = produtoId;
            UsuarioId = usuarioId;
            TipoOperacaoId = tipoOperacaoId;
            Descricao = descricao.Trim();
            DetalhesJson = detalhes != null ? JsonSerializer.Serialize(detalhes) : null;
            DataOperacao = DateTime.Now;
        }

        /// <summary>
        /// Obtém os detalhes estruturados da operação
        /// </summary>
        /// <typeparam name="T">Tipo do objeto de detalhes</typeparam>
        /// <returns>Objeto de detalhes tipado</returns>
        public T ObterDetalhes<T>() where T : class
        {
            if (string.IsNullOrWhiteSpace(DetalhesJson))
                return null;

            return JsonSerializer.Deserialize<T>(DetalhesJson);
        }
    }

    /// <summary>
    /// Classes para armazenar detalhes estruturados das operações
    /// </summary>
    public class DetalhesCampo
    {
        public string Campo { get; set; }
        public string ValorAntigo { get; set; }
        public string ValorNovo { get; set; }
    }

    public class DetalhesAtualizacao
    {
        public System.Collections.Generic.List<DetalhesCampo> Campos { get; set; } = new System.Collections.Generic.List<DetalhesCampo>();
    }

    public class DetalhesEmpresa
    {
        public int EmpresaId { get; set; }
        public string NomeEmpresa { get; set; }
        public string ValorPersonalizado { get; set; }
        public bool UsandoValorReferencia { get; set; }
    }

    /// <summary>
    /// Constantes para tipos de operações do histórico de produtos
    /// </summary>
    public static class TipoOperacaoProdutoEnum
    {
        public const int Criacao = 88;
        public const int Atualizacao = 89;
        public const int AlteracaoValor = 90;
        public const int AlteracaoUrl = 91;
        public const int Ativacao = 92;
        public const int Desativacao = 93;
        public const int EmpresaAdicionada = 94;
        public const int EmpresaRemovida = 95;
        public const int ValorEmpresaAlterado = 96;
    }
}
