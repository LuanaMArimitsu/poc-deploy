namespace WebsupplyConnect.Application.DTOs
{
    public class StatusLeadDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        /// <summary>
        /// Cor hexadecimal (#RRGGBB) para exibição em gráficos do Dashboard
        /// </summary>
        public string Cor { get; set; } = string.Empty;
    }
}
