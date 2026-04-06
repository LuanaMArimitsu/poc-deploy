
namespace WebsupplyConnect.Application.DTOs.Equipe
{
    /// <summary>DTO para criação de equipe.</summary>
    public class CriarEquipeDto
    {
        public string Nome { get; set; } = string.Empty;
        public int TipoEquipeId { get; set; }
        public int EmpresaId { get; set; }
        public int ResponsavelId { get; set; }
        public string? Descricao { get; set; }
        public bool EhPadrao { get; set; } = false;

        //Notificações
        //public bool NotificarAtribuicaoAoDestinatario { get; set; } = false;
        //public bool NotificarAtribuicaoAosLideres { get; set; } = false;
        //public bool NotificarSemAtendimentoLideres { get; set; } = false;

        //public int? TempoSemAtendimentoHoras { get; set; } 
        public int TempoMaxSemAtendimento{ get; set; }
    }
}