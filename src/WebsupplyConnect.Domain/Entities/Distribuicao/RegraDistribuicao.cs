using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Representa uma regra específica dentro de uma configuração de distribuição.
    /// Cada configuração pode ter múltiplas regras que são aplicadas em conjunto.
    /// </summary>
    public class RegraDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID da configuração de distribuição à qual esta regra pertence
        /// </summary>
        public int ConfiguracaoDistribuicaoId { get; private set; }

        /// <summary>
        /// ID do tipo de regra (MERITO, FILA, TEMPO, etc)
        /// </summary>
        public int TipoRegraId { get; private set; }

        /// <summary>
        /// Nome identificador da regra
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada do funcionamento desta regra
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Ordem de aplicação da regra (regras com menor ordem são aplicadas primeiro)
        /// </summary>
        public int Ordem { get; private set; }

        /// <summary>
        /// Peso da regra no cálculo final (0-100). Usado em estratégias compostas
        /// </summary>
        public int Peso { get; private set; }

        /// <summary>
        /// Indica se esta regra está ativa
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// JSON com os parâmetros específicos desta regra
        /// </summary>
        public string ParametrosJson { get; private set; }

        /// <summary>
        /// Indica se esta regra é obrigatória (não pode ser desativada)
        /// </summary>
        public bool Obrigatoria { get; private set; }

        /// <summary>
        /// Pontuação mínima que um vendedor deve ter nesta regra para ser elegível
        /// </summary>
        public int? PontuacaoMinima { get; private set; }

        /// <summary>
        /// Pontuação máxima possível nesta regra
        /// </summary>
        public int? PontuacaoMaxima { get; private set; }

        // Propriedades de navegação
        public virtual ConfiguracaoDistribuicao ConfiguracaoDistribuicao { get; private set; }
        public virtual TipoRegraDistribuicao TipoRegra { get; private set; }
        public virtual ICollection<ParametroRegraDistribuicao> Parametros { get; private set; }
        public virtual ICollection<AtribuicaoLead> Atribuicoes { get; private set; } = new HashSet<AtribuicaoLead>();

        // Construtor para EF Core
        protected RegraDistribuicao()
        {
            Parametros = new HashSet<ParametroRegraDistribuicao>();
            Atribuicoes = new HashSet<AtribuicaoLead>();
        }

        /// <summary>
        /// Cria uma nova regra de distribuição
        /// </summary>
        public RegraDistribuicao(
            int configuracaoDistribuicaoId,
            int tipoRegraId,
            string nome,
            string? descricao,
            int ordem,
            int peso,
            bool ativo = true,
            bool obrigatoria = false,
            int? pontuacaoMinima = null,
            int? pontuacaoMaxima = null,
            string? parametrosJson = null)
        {
            if (configuracaoDistribuicaoId <= 0)
                throw new DomainException("ID da configuração deve ser maior que zero", nameof(RegraDistribuicao));

            if (tipoRegraId <= 0)
                throw new DomainException("ID do tipo de regra deve ser maior que zero", nameof(RegraDistribuicao));

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da regra é obrigatório", nameof(RegraDistribuicao));

            if (peso < 0 || peso > 100)
                throw new DomainException("Peso deve estar entre 0 e 100", nameof(RegraDistribuicao));

            ConfiguracaoDistribuicaoId = configuracaoDistribuicaoId;
            TipoRegraId = tipoRegraId;
            Nome = nome;
            Descricao = descricao ?? string.Empty;
            Ordem = ordem;
            Peso = peso;
            Ativo = ativo;
            ParametrosJson = parametrosJson ?? "{}";
            Obrigatoria = obrigatoria;
            PontuacaoMinima = pontuacaoMinima;
            PontuacaoMaxima = pontuacaoMaxima ?? 100;

            Parametros = new HashSet<ParametroRegraDistribuicao>();
            Atribuicoes = new HashSet<AtribuicaoLead>();
        }

               /// <summary>
        /// Marca a regra como excluída logicamente
        /// </summary>
        public void Excluir()
        {
            if (Obrigatoria)
                throw new DomainException("Não é possível excluir uma regra obrigatória", nameof(RegraDistribuicao));

            Excluido = true;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }
    }
}