namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ListConversasEncerradasLeadDTO
    {
        public int ConversaId { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }

        public int LeadId { get; set; }
        public string LeadNome { get; set; }

        public string Status { get; set; }

        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataEncerramento { get; set; }

        public int? EquipeId { get; set; }
        public string EquipeNome { get; set; }

        public string UltimaMensagem { get; set; }
    }
}
