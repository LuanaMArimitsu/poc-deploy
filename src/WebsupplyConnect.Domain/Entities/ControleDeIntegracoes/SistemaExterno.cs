using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.ControleDeIntegracoes
{
    public enum TipoSistemaExterno
    {
        CRM = 0,
        ERP = 1,
        OLX = 2
    }

    public class SistemaExterno : EntidadeBase
    { 
        public string Nome { get; private set; }

        public TipoSistemaExterno Tipo { get; private set; }

        public string URL_API { get; private set; }

        public string? Token { get; private set; }

        public ICollection<EventoIntegracao> EventosIntegracao { get; private set; }

        public string? InformacoesExtras { get; private set; }
    }
}
