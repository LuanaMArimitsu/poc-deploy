using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.VersaoApp
{
    public class VersaoApp : EntidadeBase
    {
        public string Versao { get; set; }

        public string? PlataformaApp { get; set; }

        public bool AtualizacaoObrigatoria { get; set; }

        protected VersaoApp () { }

        public VersaoApp(string versao)
        {
            if (string.IsNullOrWhiteSpace(versao))
                throw new DomainException("Versão deve ser informada.", nameof(VersaoApp));

            Versao = versao;
        }
    }
}
