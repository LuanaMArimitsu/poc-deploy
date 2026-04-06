namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioHorarioDTO
    {
        public int Id { get; set; }
        public int DiaSemanaId { get; set; }
        public string DiaSemanaDescricao { get; set; }
        public string DiaSemanaAbreviacao { get; set; }
        public bool SemExpediente { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFim { get; set; }
        //public double? DuracaoHoras { get; set; }
        public string HorarioFormatado { get; set; }
        public bool IsTolerancia { get; set; }
    }
}
