namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class OportunidadeRequestDTO
    {
        public int OportunidadeId { get; set; }
        public string CnpjEmpresa { get; set; } = string.Empty;
        public string Interesse { get; set; } = string.Empty;
        public string NomeCliente { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string CodVendedor { get; set; } = string.Empty;
    }
}
