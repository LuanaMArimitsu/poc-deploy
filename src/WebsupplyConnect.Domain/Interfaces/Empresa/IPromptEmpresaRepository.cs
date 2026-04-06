using WebsupplyConnect.Domain.Entities.Empresa;

namespace WebsupplyConnect.Domain.Interfaces.Empresa
{
    public interface IPromptEmpresaRepository
    {
        Task<PromptEmpresas?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<string?> GetPromptAsync(int empresaId, bool sistema, string tipoPrompt, bool includeDeleted = false);
    }
}
