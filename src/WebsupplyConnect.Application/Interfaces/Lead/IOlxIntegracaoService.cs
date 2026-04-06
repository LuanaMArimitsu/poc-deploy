using WebsupplyConnect.Application.DTOs.Lead.OLX;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface IOlxIntegracaoService
    {
        Task<string> ReceberLeadOlxAsync(string cnpjEmpresa, string rawJson);
    }
}
