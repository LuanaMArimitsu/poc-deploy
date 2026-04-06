namespace WebsupplyConnect.Domain.Entities.ControleDeIntegracoes
{
    public enum DirecaoIntegracao
    {
        Enviado = 0,
        Recebido = 1
    }
    public enum TipoEntidadeIntegracao
    {
        Oportunidade = 0,
        Lead = 1
    }

    public class EventoIntegracao
    {
        public int Id { get; protected set; }

        public int SistemaExternoId { get; private set; }

        public DirecaoIntegracao Direcao { get; private set; }

        public TipoEventoIntegracao TipoEvento { get; private set; }

        public bool Sucesso { get; private set; }

        public string? PayloadEnviado { get; private set; }

        public string PayloadRecebido { get; private set; }

        public string CodigoResposta { get; private set; }

        public string? MensagemErro { get; private set; }

        public TipoEntidadeIntegracao? TipoEntidadeOrigem { get; private set; }
        public int? EntidadeOrigemId { get; private set; }

        public virtual SistemaExterno SistemaExterno { get; private set; }

        public DateTime DataEvento { get; private set; }

        public EventoIntegracao(
            int sistemaExternoId,
            DirecaoIntegracao direcao,
            TipoEventoIntegracao tipoEvento,
            bool sucesso,
            string payloadRecebido,
            string codigoResposta,
            string? mensagemErro,
            TipoEntidadeIntegracao? tipoEntidadeOrigem = null,
            string? payloadEnviado = null,
            int? entidadeOrigemId = null) 
        {
            SistemaExternoId = sistemaExternoId;
            Direcao = direcao;
            TipoEvento = tipoEvento;
            Sucesso = sucesso;
            PayloadEnviado = payloadEnviado;
            PayloadRecebido = payloadRecebido;
            CodigoResposta = codigoResposta;
            MensagemErro = mensagemErro;
            TipoEntidadeOrigem = tipoEntidadeOrigem;
            EntidadeOrigemId = entidadeOrigemId;
            DataEvento = Helpers.TimeHelper.GetBrasiliaTime(); 
        }
    }
}
