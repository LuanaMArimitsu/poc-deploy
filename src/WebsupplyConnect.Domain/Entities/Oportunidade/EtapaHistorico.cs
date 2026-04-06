using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Oportunidade
{
    public class EtapaHistorico
    {
        /// <summary>
        /// Identificador único da entidade
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// ID da oportunidade relacionada
        /// </summary>
        public int OportunidadeId { get; private set; }

        /// <summary>
        /// ID da etapa anterior
        /// </summary>
        public int? EtapaAnteriorId { get; private set; }

        /// <summary>
        /// ID da nova etapa
        /// </summary>
        public int EtapaNovaId { get; private set; }

        /// <summary>
        /// Data e hora da mudança
        /// </summary>
        public DateTime DataMudanca { get; private set; }

        /// <summary>
        /// ID do usuário responsável pela mudança
        /// </summary>
        public int ResponsavelId { get; private set; }

        /// <summary>
        /// Observação sobre a mudança
        /// </summary>
        public string? Observacao { get; private set; }

        /// <summary>
        /// Tempo (em dias) que a oportunidade permaneceu na etapa anterior
        /// </summary>
        public int DiasNaEtapaAnterior { get; private set; }

        /// <summary>
        /// Oportunidade relacionada
        /// </summary>
        public virtual Oportunidade Oportunidade { get; private set; }

        /// <summary>
        /// Etapa anterior
        /// </summary>
        public virtual Etapa EtapaAnterior { get; private set; }

        /// <summary>
        /// Nova etapa
        /// </summary>
        public virtual Etapa EtapaNova { get; private set; }

        /// <summary>
        /// Usuário responsável pela mudança
        /// </summary>
        public virtual Usuario.Usuario Responsavel { get; private set; }

        /// <summary>
        /// Construtor protegido para EF Core
        /// </summary>
        protected EtapaHistorico()
        {
        }

        /// <summary>
        /// Construtor para criar um novo registro de histórico de etapa
        /// </summary>
        /// <param name="oportunidadeId">ID da oportunidade</param>
        /// <param name="etapaAnteriorId">ID da etapa anterior</param>
        /// <param name="etapaNovaId">ID da nova etapa</param>
        /// <param name="dataMudanca">Data e hora da mudança</param>
        /// <param name="responsavelId">ID do usuário responsável</param>
        /// <param name="observacao">Observação sobre a mudança</param>
        /// <param name="diasNaEtapaAnterior">Dias na etapa anterior (calculado pelo sistema)</param>
        public EtapaHistorico(
            int oportunidadeId,
            int? etapaAnteriorId,
            int etapaNovaId,
            DateTime dataMudanca,
            int responsavelId,
            string? observacao = null,
            int? diasNaEtapaAnterior = null)
        {
            ValidarParametros(oportunidadeId, etapaNovaId, dataMudanca, responsavelId);

            OportunidadeId = oportunidadeId;
            EtapaAnteriorId = etapaAnteriorId ?? 0;
            EtapaNovaId = etapaNovaId;
            DataMudanca = dataMudanca;
            ResponsavelId = responsavelId;
            Observacao = observacao;
            DiasNaEtapaAnterior = diasNaEtapaAnterior ?? 0; // Será calculado pelo Application Service
        }

        /// <summary>
        /// Atualiza a observação sobre a mudança
        /// </summary>
        /// <param name="observacao">Nova observação</param>
        public void AtualizarObservacao(string? observacao)
        {
            Observacao = observacao;
        }

        /// <summary>
        /// Atualiza o número de dias na etapa anterior
        /// </summary>
        /// <param name="dias">Número de dias</param>
        public void AtualizarDiasNaEtapa(int dias)
        {
            if (dias < 0)
                throw new DomainException("O número de dias não pode ser negativo");

            DiasNaEtapaAnterior = dias;
        }

        /// <summary>
        /// Valida os parâmetros do construtor
        /// </summary>
        private static void ValidarParametros(int oportunidadeId, int etapaNovaId,
            DateTime dataMudanca, int responsavelId)
        {
            if (oportunidadeId <= 0)
                throw new DomainException("ID da oportunidade é obrigatório");        

            if (etapaNovaId <= 0)
                throw new DomainException("ID da nova etapa é obrigatório");

            if (dataMudanca > DateTime.UtcNow)
                throw new DomainException("A data da mudança não pode ser futura");

            if (responsavelId <= 0)
                throw new DomainException("ID do responsável é obrigatório");
        }
    }
}
