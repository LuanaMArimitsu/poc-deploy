namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ConversaPagParam
    {
        public int? quantidadeInicial { get; set; }
        public int? quantidadeFinal { get; set; }
        public int? empresaId { get; set; } = null;
        public int? EquipeId { get; set; } = null;
    }
}
