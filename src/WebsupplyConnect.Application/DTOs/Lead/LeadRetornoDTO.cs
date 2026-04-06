using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadRetornoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Cargo { get; set; }
        public string? WhatsappNumero { get; set; }
        public string? CPF { get; set; }
        public DateTime? DataNascimento { get; set; }
        public DateTime? DataCadastro { get; set; }
        public string? Genero { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? CNPJEmpresa { get; set; }
        public int LeadStatusId { get; set; }
        public string LeadStatus { get; set; }
        public string LeadStatusCor { get; set; }
        public DateTime? DataConversaoCliente { get; set; }
        public bool Cliente { get; set; } = false; 
        public string? NivelInteresse { get; set; }
        public string? ObservacoesCadastrais { get; set; }
        public int ResponsavelId { get; set; }
        public string Responsavel { get; set; }
        public int UsuarioId { get; set; }
        public int EquipeId { get; set; }
        public string Equipe { get; set; }
        public int OrigemId { get; set; }
        public string Origem { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public bool Excluido { get; set; }
        public DateTime? DataPrimeiroContato { get; set; }
        public DateTime? DataUltimaMensagem { get; set; }
        public bool PossuiConversaEncerrada { get; set; }
        public int? CampanhaId { get; set; }
        public string? CampanhaNome { get; set; }
    }
}
