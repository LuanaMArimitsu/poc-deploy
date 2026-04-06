using System.Security.Claims;
using WebsupplyConnect.Application.DTOs.Usuario;

namespace WebsupplyConnect.Application.Interfaces.Usuario
{
    public interface IJwtTokenService
    {
        Task<GenerateJwtResponseDTO> GerarJwtAsync(GenerateJwtRequestDTO request);
        Task<GenerateJwtResponseDTO> RenovarJwtAsync(string refreshToken, ClaimsPrincipal userClaims, string clientType);
    }
}
