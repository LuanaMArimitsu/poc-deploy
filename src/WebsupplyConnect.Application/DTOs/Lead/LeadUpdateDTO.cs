using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadUpdateDTO
    {
        /// <summary>
        /// Id do Status do Lead
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Nome do lead
        /// </summary>
        public string? Nome { get; set; }

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
        public required string NivelInteresse { get; set; }

        /// <summary>
        /// Observações cadastrais sobre o lead
        /// </summary>
        public string? ObservacoesCadastrais { get; set; }

        /// <summary>
        /// ID da origem do lead (como o lead chegou ao sistema)
        /// </summary>
        public int? OrigemId { get; set; }

        public int ResponsavelId { get; set; }
        public int EmpresaId { get; set; }
        public int EquipeId { get; set; }
    }
}
