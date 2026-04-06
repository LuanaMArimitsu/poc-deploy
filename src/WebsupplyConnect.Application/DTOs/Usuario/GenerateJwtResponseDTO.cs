namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class GenerateJwtResponseDTO
    {
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public AzureUserDTO UserInfo { get; set; } = new();
        public string? DeviceId { get; set; }
    }
}
