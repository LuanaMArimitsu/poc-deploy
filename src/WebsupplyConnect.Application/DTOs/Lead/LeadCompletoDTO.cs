using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadCompletoDTO
    {
        /// <summary>
        /// Nome do lead
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Email do lead
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Telefone do lead
        /// </summary>
        public string? Telefone { get; set; }

        /// <summary>
        /// Cargo do lead na empresa se for empresa
        /// </summary>
        public string? Cargo { get; set; }

        /// <summary>
        /// Número de WhatsApp do lead
        /// </summary>
        public string? WhatsappNumero { get; set; }

        /// <summary>
        /// CPF do lead
        /// </summary>
        public string? CPF { get; set; }

        /// <summary>
        /// Data de nascimento do lead
        /// </summary>
        public DateTime? DataNascimento { get; set; }

        /// <summary>
        /// Gênero do lead
        /// </summary>
        public string? Genero { get; set; }

        /// <summary>
        /// Nome da empresa lead 
        /// </summary>
        public string? NomeEmpresa { get; set; }

        /// <summary>
        /// CNPJ da empresa lead 
        /// </summary>
        public string? CNPJEmpresa { get; set; }

        /// <summary>
        /// Nível de interesse do lead (baixo, médio, alto)
        /// </summary>
        public string? NivelInteresse { get; set; }

        /// <summary>
        /// Observações cadastrais sobre o lead
        /// </summary>
        public string? ObservacoesCadastrais { get; set; }

        /// <summary>
        /// ID do usuário responsável pelo lead
        /// </summary>
        public int? ResponsavelId { get; set; } = 0;

        /// <summary>
        /// ID da origem do lead (como o lead chegou ao sistema)
        /// </summary>
        public int OrigemId { get; set; }

        /// <summary>
        /// Código da campanha associada ao lead, se houver
        /// </summary>
        public int? CampanhaId { get; set; }

        /// <summary>
        /// ID da empresa que o Lead pertence
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>
        /// ID da equipe que o Lead pertence
        /// </summary>
        public int EquipeId { get; set; }
    }
}
