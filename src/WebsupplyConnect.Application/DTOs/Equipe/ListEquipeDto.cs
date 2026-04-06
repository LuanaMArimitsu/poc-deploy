
namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ListEquipeDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool EhPadrao { get; set; }

        public int TipoEquipeId { get; set; }
        public string TipoEquipeNome { get; set; } = string.Empty;

        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;

        public bool Ativa { get; set; }

        public int ResponsavelMembroId { get; set; }
        public string ResponsavelNome { get; set; } = string.Empty;

        public int TotalMembros { get; set; }
        public int MembrosAtivos { get; set; }

        public int TempoMaxSemAtendimento { get; set; }
    }
}
