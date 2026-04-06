namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ListMembroEEquipeDTO
    {
            public int UsuarioId { get; set; }
            public string UsuarioNome { get; set; } = string.Empty;
            public List<ListEquipeEConversasDTO> Equipes { get; set; } = new();
    }
}
