namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class LeadGold
    {
        //idLead a Chave da lead no Sistema CRM externo
        public string idLead { get; set; }

        //idCRM a Chave do Sistema CRM externo(para identificar de onde estamos recebendo a lead)
        public string idCRM { get; set; }

        //TipoInteresse(receber apenas Novos ou Seminovos, validar no webservice do NBS)
        public string TipoInteresse { get; set; }
        public string Origem { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Observacao { get; set; }
        public string CNPJ_Unidade { get; set; }
    }
}
