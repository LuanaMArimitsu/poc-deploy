namespace WebsupplyConnect.Application.DTOs.Equipe
{
    /// <summary>DTO para membro de equipe.</summary>
    public class MembroEquipeDto
    {
        public int Id { get; set; }

        public int EquipeId { get; set; }
        public string EquipeNome { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;

        public int StatusMembroId { get; set; }
        public string StatusNome { get; set; } = string.Empty;
        public string StatusCodigo { get; set; } = string.Empty;

        public bool IsLider { get; set; }

        public DateTime? DataSaida { get; set; }

        public string? Observacoes { get; set; }
    }
}
