namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class AtualizarHorarioTrabalhoDTO
    {
        public bool SemExpediente { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFim { get; set; }
    }
}
