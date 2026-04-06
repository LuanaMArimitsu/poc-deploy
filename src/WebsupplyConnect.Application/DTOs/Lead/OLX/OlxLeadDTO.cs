namespace WebsupplyConnect.Application.DTOs.Lead.OLX
{
    public class OlxLeadDTO
    {
        public string? LinkAd { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public OlxLeadInfoDto? AdsInfo { get; set; }
    }
}
