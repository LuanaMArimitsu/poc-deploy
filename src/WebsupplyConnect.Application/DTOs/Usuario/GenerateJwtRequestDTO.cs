namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class GenerateJwtRequestDTO
    {
        public string AccessToken { get; set; } = string.Empty;

        public DeviceInfoDTO? DeviceInfo { get; set; }
    }

    public class DeviceInfoDTO
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
    }
}
