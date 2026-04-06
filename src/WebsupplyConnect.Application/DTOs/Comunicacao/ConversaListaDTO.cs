namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ConversaListaDTO
    {
        public int ConversaId { get; set; }
        public string NumeroWhatsapp { get; set; }
        public required string ConversaStatus { get; set; }
        public required string LeadName { get; set; }
        public string? Apelido { get; set; }
        public int LeadId { get; set; }
        public required string LeadStatus { get; set; }
        public int LeadEmpresaId { get; set; }
        public bool JanelaAberta { get; set; }
        public string Tipo { get; set; }
        public string? UltimaMensagem { get; set; }
        public DateTime DataUltimaMensagem { get; set; }
        public int QtdMensagensNaoLidas { get; set; }
        public DateTime DataInicioConversa { get; set; }
        public string IniciouContato { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public int? CampanhaId { get; set; }
        public string? CampanhaNome { get; set; }
        public bool Fixada { get; set; }
    }
}
