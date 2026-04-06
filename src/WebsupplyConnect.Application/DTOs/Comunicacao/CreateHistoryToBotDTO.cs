using System.Text.Json.Serialization;

namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class CreateHistoryToBotDTO
    {
        public int ChatBotId { get; set; }
        public LeadInformationDTO LeadInformation { get; set; }
        public List<MessageRedisDTO> MessagesHistory { get; set; } = [];
        public List<BranchesDTO> Branches { get; set; } = [];
        public string CompanyName { get; set; }
        public bool TransferExecuted { get; set; } = false;
    }

    public class MessageRedisDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Sender { get; set; }
        public DateTime SentOn { get; set; }
        public bool SenderIsBot { get; set; }
    }

    public class LeadInformationDTO
    {
        public int CustomerId { get; set; }

        // ===== NOME =====
        public string? CustomerName { get; set; } = string.Empty;
        public int NameAttempts { get; set; } = 0;

        // ===== LOCALIZAÇÃO (NOVO) =====
        public string? CustomerLocation { get; set; } = string.Empty; // Localização informada pelo cliente
        public int LocationAttempts { get; set; } = 0;

        // ===== FILIAL SUGERIDA (NOVO) =====
        public string? SuggestedBranch { get; set; } = string.Empty;// Filial sugerida pela IA
        public int SuggestedBranchCode { get; set; } = 0;

        // ===== FILIAL CONFIRMADA/SELECIONADA =====
        public string? SelectedBranch { get; set; } = string.Empty; // Filial confirmada pelo cliente
        public int SelectedBranchCode { get; set; } = 0;
        public int BranchAttempts { get; set; } = 0;

        // ===== Outros =====
        public int CompanyId { get; set; }
        public int QualificationScore { get; set; } = 0;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class TeamsDTO
    {
        public string TeamName { get; set; } = string.Empty;
        public int TeamId { get; set; }
    }

    public class BranchesDTO
    {
        public string BranchName { get; set; } = string.Empty;
        public int BranchCode { get; set; }
        public Localizacao? Location { get; set; }
        public List<TeamsDTO> Teams { get; set; }
    }

    public class Localizacao
    {
        public string Regiao { get; set; }
        public string Unidade { get; set; }
        public Contato Contato { get; set; }    
        public EnderecoFilial Endereco { get; set; }
        [JsonPropertyName("horario_funcionamento")]
        public HorarioFuncionamento HorarioFuncionamento { get; set; }
    }

    public class Contato
    {
        public string Telefone { get; set; }
    }

    public class EnderecoFilial
    {
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string Cep { get; set; }
        [JsonPropertyName("endereco_completo")]
        public string EnderecoCompleto { get; set; }
    }

    public class HorarioFuncionamento
    {
        public SetorHorario Vendas { get; set; }
        [JsonPropertyName("recepcao_tecnica_oficina")]
        public SetorHorario RecepcaoTecnicaOficina { get; set; }
        [JsonPropertyName("departamento_pecas")]
        public SetorHorario DepartamentoPecas { get; set; }
    }

    public class SetorHorario
    {
        [JsonPropertyName("segunda_a_sabado")] 
        public Periodo SegundaASabado { get; set; }
        [JsonPropertyName("segunda_a_sexta")] 
        public Periodo SegundaASexta { get; set; }
        public Periodo Sabado { get; set; }
    }

    public class Periodo
    {
        public string Inicio { get; set; }
        public string Fim { get; set; }
    }
}
