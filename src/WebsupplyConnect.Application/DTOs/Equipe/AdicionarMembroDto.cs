namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class AdicionarMembroDto
    {
        public int UsuarioId { get; set; }
        public int StatusMembroEquipeId { get; set; }
        public bool IsLider { get; set; }
        public string? Observacoes { get; set; }
    }
}