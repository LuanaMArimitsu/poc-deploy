using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    public class AtribuicaoPorEquipeDTO
    {
        public required AtribuicaoLead AtribuicaoLead { get; set; }
        public required int EquipeId { get; set; }

    }
}
