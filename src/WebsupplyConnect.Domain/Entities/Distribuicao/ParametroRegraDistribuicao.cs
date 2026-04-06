using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Representa um parâmetro específico de uma regra de distribuição
    /// </summary>
    public class ParametroRegraDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID da regra de distribuição à qual este parâmetro pertence
        /// </summary>
        public int RegraDistribuicaoId { get; private set; }

        /// <summary>
        /// Nome do parâmetro (chave)
        /// </summary>
        public string NomeParametro { get; private set; }

        /// <summary>
        /// Tipo do parâmetro (ex: string, int, boolean, date, etc.)
        /// </summary>
        public string TipoParametro { get; private set; }

        /// <summary>
        /// Valor do parâmetro
        /// </summary>
        public string ValorParametro { get; private set; }

        /// <summary>
        /// Descrição do parâmetro
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Indica se o parâmetro é obrigatório
        /// </summary>
        public bool Obrigatorio { get; private set; }

        /// <summary>
        /// Expressão regular para validação do valor do parâmetro
        /// </summary>
        public string ValidacaoRegex { get; private set; }

        /// <summary>
        /// Valor padrão do parâmetro quando não especificado
        /// </summary>
        public string ValorPadrao { get; private set; }

        // Propriedade de navegação
        public virtual RegraDistribuicao RegraDistribuicao { get; private set; }

        // Construtor para EF Core
        protected ParametroRegraDistribuicao()
        {
        }

        /// <summary>
        /// Cria um novo parâmetro para uma regra de distribuição
        /// </summary>
        public ParametroRegraDistribuicao(
            int regraDistribuicaoId,
            string nomeParametro,
            string tipoParametro,
            string valorParametro,
            string descricao = "",
            bool obrigatorio = false,
            string validacaoRegex = null,
            string valorPadrao = null)
        {
            if (string.IsNullOrWhiteSpace(nomeParametro))
                throw new DomainException("Nome do parâmetro é obrigatório", nameof(ParametroRegraDistribuicao));

            if (string.IsNullOrWhiteSpace(tipoParametro))
                throw new DomainException("Tipo do parâmetro é obrigatório", nameof(ParametroRegraDistribuicao));

            if (obrigatorio && string.IsNullOrWhiteSpace(valorParametro) && string.IsNullOrWhiteSpace(valorPadrao))
                throw new DomainException("Parâmetro obrigatório deve ter um valor ou valor padrão", nameof(ParametroRegraDistribuicao));

            RegraDistribuicaoId = regraDistribuicaoId;
            NomeParametro = nomeParametro;
            TipoParametro = tipoParametro;
            ValorParametro = valorParametro;
            Descricao = descricao ?? string.Empty;
            Obrigatorio = obrigatorio;
            ValidacaoRegex = validacaoRegex;
            ValorPadrao = valorPadrao;

            DataCriacao = TimeHelper.GetBrasiliaTime();
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza o valor de um parâmetro existente
        /// </summary>
        public void AtualizarValor(string novoValor)
        {
            if (Obrigatorio && string.IsNullOrWhiteSpace(novoValor) && string.IsNullOrWhiteSpace(ValorPadrao))
                throw new DomainException("Parâmetro obrigatório deve ter um valor ou valor padrão", nameof(ParametroRegraDistribuicao));

            ValorParametro = novoValor;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza a descrição de um parâmetro existente
        /// </summary>
        public void AtualizarDescricao(string novaDescricao)
        {
            Descricao = novaDescricao ?? string.Empty;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }
        
        /// <summary>
        /// Atualiza o tipo do parâmetro
        /// </summary>
        public void AtualizarTipoParametro(string novoTipo)
        {
            if (string.IsNullOrWhiteSpace(novoTipo))
                throw new DomainException("Tipo do parâmetro é obrigatório", nameof(ParametroRegraDistribuicao));

            TipoParametro = novoTipo;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Define se o parâmetro é obrigatório
        /// </summary>
        public void DefinirObrigatoriedade(bool obrigatorio)
        {
            // Se estiver tornando obrigatório, verificar se há valor ou valor padrão
            if (obrigatorio && string.IsNullOrWhiteSpace(ValorParametro) && string.IsNullOrWhiteSpace(ValorPadrao))
                throw new DomainException("Parâmetro obrigatório deve ter um valor ou valor padrão", nameof(ParametroRegraDistribuicao));

            Obrigatorio = obrigatorio;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza a expressão regular de validação
        /// </summary>
        public void AtualizarValidacaoRegex(string novaRegex)
        {
            ValidacaoRegex = novaRegex;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza o valor padrão do parâmetro
        /// </summary>
        public void AtualizarValorPadrao(string novoValorPadrao)
        {
            // Se obrigatório e não tem valor, verifica que o valor padrão não é nulo
            if (Obrigatorio && string.IsNullOrWhiteSpace(ValorParametro) && string.IsNullOrWhiteSpace(novoValorPadrao))
                throw new DomainException("Parâmetro obrigatório deve ter um valor ou valor padrão", nameof(ParametroRegraDistribuicao));

            ValorPadrao = novoValorPadrao;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Marca o parâmetro como excluído logicamente
        /// </summary>
        public void Excluir()
        {
            Excluido = true;
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }
    }
}