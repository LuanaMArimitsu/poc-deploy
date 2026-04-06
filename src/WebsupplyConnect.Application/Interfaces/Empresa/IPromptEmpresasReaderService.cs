using WebsupplyConnect.Domain.Entities.Empresa;

namespace WebsupplyConnect.Application.Interfaces.Empresa
{
    public interface IPromptEmpresasReaderService
    {
        Task<PromptEmpresas?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<string?> GetPromptAsync(int empresaId, bool sistema, string tipoPrompt, bool includeDeleted = false);  
    }
}
