using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IAzureAdService
    {
        Task<List<AzureUserDTO>> GetUserAsync(string? startsWith = null);
        Task<AzureUserDTO?> GetUserByIdAsync(string azureUserId);
        Task<AzureUserDTO?> GetUserByAccessTokenAsync(string accessToken);
    }
}
