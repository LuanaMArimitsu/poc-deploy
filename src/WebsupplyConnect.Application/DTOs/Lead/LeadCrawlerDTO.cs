namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadCrawlerDTO
    {
        /// <summary>
        /// Nome do lead
        /// </summary>
        public required string  Nome { get; set; }

        /// <summary>
        /// Email do lead
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Número de WhatsApp do lead
        /// </summary>
        public string? WhatsappNumero { get; set; }

        /// <summary>
        /// Parâmetro necessário em integrações como o  MyHonda. A distribuição de responsável já é feita pela plataforma.
        /// </summary>
        public string? EmailResponsavel { get; set; } = null;

        /// <summary>
        /// Origem do lead (como o lead chegou ao sistema: MyHonda, RD Station, Instagram, Facebook)
        /// </summary>
        public required string Origem { get; set; }

        /// <summary>
        /// Nome da campanha
        /// </summary>
        public string? CampanhaNome { get; set; }

        /// <summary>
        /// Código da campanha
        /// </summary>
        public string? CampanhaCod { get; set; }

        /// <summary>
        /// Cnpj da empresa que o lead pertence
        public required string CNPJEmpresa { get; set; }

        /// <summary>
        /// Observações sobre o evento
        /// </summary>
        public string? ObsEvento { get; set; }
    }
}
