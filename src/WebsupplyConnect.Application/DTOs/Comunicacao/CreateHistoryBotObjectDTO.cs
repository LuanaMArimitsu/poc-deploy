namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class CreateHistoryBotObjectDTO
    {
        public int ChatBotId { get; set; }
        public int LeadId { get; set; }
        public int ConversaId { get; set; }
        public int EmpresaId { get; set; }
        public required string GrupoEmpresa { get; set; }
        public required string Mensagem { get; set; }
        public List<MensagemDTO>? MensagensAntigas{ get; set; }
        public required List<BranchesDTO> Filiais { get; set; }
    }
}
