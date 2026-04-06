namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class EditarEnderecoDTO
    {
        public int EnderecoId { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Pais { get; set; } = "Brasil";
    }
}