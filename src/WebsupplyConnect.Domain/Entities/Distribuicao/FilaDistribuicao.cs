using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Representa a posição de um vendedor na fila de distribuição Round-Robin.
    /// Controla a ordem e elegibilidade dos vendedores para receber leads.
    /// </summary>
    public class FilaDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID do membro (vendedor) na fila
        /// </summary>
        public int MembroEquipeId { get; private set; }

        /// <summary>
        /// ID da empresa à qual a fila pertence
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Posição atual na fila (1 = próximo a receber)
        /// </summary>
        public int PosicaoFila { get; private set; }

        /// <summary>
        /// Data e hora do último lead recebido
        /// </summary>
        public DateTime? DataUltimoLeadRecebido { get; private set; }

        /// <summary>
        /// ID do status atual na fila
        /// </summary>
        public int StatusFilaDistribuicaoId { get; private set; }

        /// <summary>
        /// Data em que o vendedor entrou na fila
        /// </summary>
        public DateTime DataEntradaFila { get; private set; }

        /// <summary>
        /// Peso atual do vendedor na distribuição (para distribuição ponderada)
        /// </summary>
        public int PesoAtual { get; private set; }

        /// <summary>
        /// Quantidade total de leads recebidos pelo vendedor
        /// </summary>
        public int QuantidadeLeadsRecebidos { get; private set; }

        /// <summary>
        /// Data e hora em que o vendedor estará elegível para receber novos leads
        /// </summary>
        public DateTime? DataProximaElegibilidade { get; private set; }

        /// <summary>
        /// Motivo descritivo do status atual (ex: "Férias até 10/07")
        /// </summary>
        public string MotivoStatusAtual { get; private set; }

        // Propriedades de navegação
        public virtual MembroEquipe MembroEquipe { get; private set; }
        public virtual Empresa.Empresa Empresa { get; private set; }
        public virtual StatusFilaDistribuicao StatusFilaDistribuicao { get; private set; }

        // Construtor para EF Core
        protected FilaDistribuicao() { }

        /// <summary>
        /// Cria uma nova entrada na fila de distribuição
        /// </summary>
        public FilaDistribuicao(
            int membroEquipeId,
            int empresaId,
            int posicaoFila,
            int statusFilaDistribuicaoId,
            DateTime? dataUltimoLeadRecebido = null,
            int pesoAtual = 1,
            string motivoStatusAtual = "")
        {
            if (membroEquipeId <= 0)
                throw new DomainException("ID do membro deve ser maior que zero", nameof(FilaDistribuicao));

            if (empresaId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(FilaDistribuicao));

            if (posicaoFila <= 0)
                throw new DomainException("Posição na fila deve ser maior que zero", nameof(FilaDistribuicao));

            if (statusFilaDistribuicaoId <= 0)
                throw new DomainException("ID do status deve ser maior que zero", nameof(FilaDistribuicao));

            if (pesoAtual <= 0)
                throw new DomainException("Peso deve ser maior que zero", nameof(FilaDistribuicao));

            MembroEquipeId = membroEquipeId;
            EmpresaId = empresaId;
            PosicaoFila = posicaoFila;
            StatusFilaDistribuicaoId = statusFilaDistribuicaoId;
            DataEntradaFila = TimeHelper.GetBrasiliaTime();
            DataUltimoLeadRecebido = dataUltimoLeadRecebido;
            PesoAtual = pesoAtual;
            QuantidadeLeadsRecebidos = 0;
            DataProximaElegibilidade = null;
            MotivoStatusAtual = motivoStatusAtual ?? string.Empty;
            
            DataCriacao = TimeHelper.GetBrasiliaTime();
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza o status do vendedor na fila
        /// </summary>
        /// <param name="statusId">ID do novo status</param>
        /// <param name="motivo">Motivo da alteração de status</param>
        public void AtualizarStatus(int statusId, string motivo = "")
        {
            if (statusId <= 0)
                throw new DomainException("ID do status deve ser maior que zero", nameof(FilaDistribuicao));

            StatusFilaDistribuicaoId = statusId;
            MotivoStatusAtual = motivo ?? string.Empty;
            AtualizarDataModificacao();
        }

        public void Restaurar()
        {
            Excluido = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra o recebimento de um novo lead pelo vendedor
        /// </summary>
        public void RegistrarRecebimentoLead()
        {
            DataUltimoLeadRecebido = TimeHelper.GetBrasiliaTime();
            QuantidadeLeadsRecebidos++;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza a posição do vendedor na fila
        /// </summary>
        /// <param name="novaPosicao">Nova posição na fila</param>
        public void AtualizarPosicaoFila(int novaPosicao)
        {
            if (novaPosicao <= 0)
                throw new DomainException("Posição na fila deve ser maior que zero", nameof(FilaDistribuicao));

            PosicaoFila = novaPosicao;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Coloca o vendedor no final da fila após receber um lead
        /// </summary>
        /// <param name="ultimaPosicaoFila">Última posição na fila atual</param>
        public void MoverParaFinalDaFila(int ultimaPosicaoFila)
        {
            PosicaoFila = ultimaPosicaoFila + 1;
            RegistrarRecebimentoLead();
        }

        /// <summary>
        /// Define o peso do vendedor na distribuição ponderada
        /// </summary>
        /// <param name="novoPeso">Novo peso para o vendedor</param>
        public void AtualizarPeso(int novoPeso)
        {
            if (novoPeso <= 0)
                throw new DomainException("Peso deve ser maior que zero", nameof(FilaDistribuicao));

            PesoAtual = novoPeso;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Define quando o vendedor estará elegível novamente para receber leads
        /// </summary>
        /// <param name="dataElegibilidade">Data e hora de elegibilidade</param>
        public void DefinirProximaElegibilidade(DateTime? dataElegibilidade)
        {
            // Se a data for fornecida, garantir que é futura
            if (dataElegibilidade.HasValue && dataElegibilidade.Value <= TimeHelper.GetBrasiliaTime())
                throw new DomainException("Data de elegibilidade deve ser futura", nameof(FilaDistribuicao));

            DataProximaElegibilidade = dataElegibilidade;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se o vendedor está elegível para receber leads neste momento
        /// </summary>
        /// <returns>True se elegível, false caso contrário</returns>
        public bool EstaElegivel()
        {
            // Se não tem data de elegibilidade definida, está elegível
            if (!DataProximaElegibilidade.HasValue)
                return true;

            // Caso contrário, verifica se a data já passou
            return DataProximaElegibilidade.Value <= TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Marca a posição como excluída (exclusão lógica)
        /// </summary>
        public new void Excluir()
        {
            Excluido = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Método auxiliar para atualizar a data de modificação
        /// </summary>
        private void AtualizarDataModificacao()
        {
            DataModificacao = TimeHelper.GetBrasiliaTime();
        }
    }
}