namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class HorarioTrabalhoDTO
    {
        public int DiaSemanaId { get; set; }
        public bool SemExpediente { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFim { get; set; }
    }
}
