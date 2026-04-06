using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface ITemplateReaderService
    {
        Task<Template> GetTemplateByIdAsync(int id);
        Task<Template?> GetTemplateByNameAsync(string nomeTemplate, int canalId);
        Task<List<ListaTemplatesReponseDTO>> GetListTemplates(int usuarioId, int empresaId);
        Task<Template?> GetTemplateByOrigem(int origemId, int canalId);
    }
}
