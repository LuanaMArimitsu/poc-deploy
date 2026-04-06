namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ListMembroEquipeDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;

        public int StatusMembroEquipeId { get; set; }
        public string StatusNome { get; set; } = string.Empty;

        public bool IsLider { get; set; }
        public DateTime? DataSaida { get; set; }
        public string? Observacoes { get; set; }
    }
}
