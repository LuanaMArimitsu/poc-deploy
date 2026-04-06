namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class RenewJwtRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? ClientType { get; set; } = string.Empty;
    }
}
