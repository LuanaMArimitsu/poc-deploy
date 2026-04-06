namespace WebsupplyConnect.Application.DTOs.Equipe
{
    /// <summary>DTO para transferência de dados de equipe.</summary>
    public class EquipeDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        public int TipoEquipeId { get; set; }
        public string TipoEquipeNome { get; set; } = string.Empty;
        public string TipoEquipeCodigo { get; set; } = string.Empty;

        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;

        public bool Ativa { get; set; }

        public int ResponsavelId { get; set; }
        public string ResponsavelNome { get; set; } = string.Empty;

        public int TotalMembros { get; set; }
        public int MembrosAtivos { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
