using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadResponsavelWriterService
    {
        Task<LeadDTO> VerificarOuCriarLeadComResponsavelAsync(string whatsappNumero, List<CanalDTO> listaCanais, string apelido);
        Task CriarLeadViaCrawler(LeadCrawlerDTO dto);
    }
}
