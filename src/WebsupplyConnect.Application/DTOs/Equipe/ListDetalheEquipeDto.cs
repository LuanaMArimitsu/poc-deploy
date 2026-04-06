namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ListDetalheEquipeDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool Ativa { get; set; }
        public bool EhPadrao { get; set; }

        // Empresa
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;

        // Tipo de equipe
        public int TipoEquipeId { get; set; }
        public string TipoEquipeNome { get; set; } = string.Empty;

        // Responsável
        public int ResponsavelMembroId { get; set; }
        public string ResponsavelNome { get; set; } = string.Empty;

        public int TempoMaxSemAtendimento { get; set; }
        // Notificações
        //public bool NotificarAtribuicaoAoDestinatario { get; set; }
        //public bool NotificarAtribuicaoAosLideres { get; set; }
        //public bool NotificarSemAtendimentoLideres { get; set; }
        //public int? TempoSemAtendimentoHoras { get; set; }
        //public int? TempoSemAtendimentoMinutos { get; set; }
    }
}
