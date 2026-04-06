namespace WebsupplyConnect.Application.DTOs.Equipe
{
    /// <summary>Campos parciais para atualização de equipe.</summary>
    public class AtualizarEquipeDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }

        // Status
        public bool? Ativa { get; set; }
        public bool? EhPadrao { get; set; }

        // Notificações (tri-state: null = não alterar)
        //public bool? NotificarAtribuicaoAoDestinatario { get; set; }
        //public bool? NotificarAtribuicaoAosLideres { get; set; }
        //public bool? NotificarSemAtendimentoLideres { get; set; }

        // Tempo (opcional; usado quando a flag de SLA estiver verdadeira)
        //public int? TempoSemAtendimentoHoras { get; set; }
        //public int? TempoSemAtendimentoMinutos { get; set; }

        public int TempoMaxSemAtendimento { get; set; }

        public int EmpresaId { get; set; }
    }
}
