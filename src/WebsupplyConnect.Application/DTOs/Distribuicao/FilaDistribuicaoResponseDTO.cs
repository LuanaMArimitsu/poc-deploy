using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para resposta da fila de distribuição
    /// </summary>
    public class FilaDistribuicaoResponseDTO
    {
        /// <summary>
        /// ID do registro da fila
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do membro na fila
        /// </summary>
        public int MembroEquipeId { get; set; }

        /// <summary>
        /// ID da empresa
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>
        /// Posição na fila
        /// </summary>
        public int PosicaoFila { get; set; }

        /// <summary>
        /// Data do último lead recebido
        /// </summary>
        public DateTime? DataUltimoLeadRecebido { get; set; }

        /// <summary>
        /// ID do status da fila de distribuição
        /// </summary>
        public int StatusFilaDistribuicaoId { get; set; }

        /// <summary>
        /// Data de entrada na fila
        /// </summary>
        public DateTime DataEntradaFila { get; set; }

        /// <summary>
        /// Peso atual do vendedor
        /// </summary>
        public int PesoAtual { get; set; }

        /// <summary>
        /// Quantidade de leads recebidos
        /// </summary>
        public int QuantidadeLeadsRecebidos { get; set; }

        /// <summary>
        /// Data da próxima elegibilidade
        /// </summary>
        public DateTime? DataProximaElegibilidade { get; set; }

        /// <summary>
        /// Motivo do status atual
        /// </summary>
        public string MotivoStatusAtual { get; set; } = string.Empty;

        ///// <summary>
        ///// Informações básicas do usuário
        ///// </summary>
        //public UsuarioFilaDTO? Usuario { get; set; }

        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data da última modificação
        /// </summary>
        public DateTime DataModificacao { get; set; }

        /// <summary>
        /// Indica se o registro foi excluído
        /// </summary>
        public bool Excluido { get; set; }

        /// <summary>
        /// Indica se o fallback de horário foi aplicado
        /// </summary>
        public bool FallbackHorarioAplicado { get; set; }

        /// <summary>
        /// Detalhes sobre o fallback de horário aplicado
        /// </summary>
        public string? DetalhesFallbackHorario { get; set; }

        /// <summary>
        /// Data e hora quando o fallback foi aplicado
        /// </summary>
        public DateTime? DataFallbackHorario { get; set; }
    }

    /// <summary>
    /// DTO para informações básicas do usuário na fila
    /// </summary>
    public class MembroFilaDTO
    {
        /// <summary>
        /// ID do usuário
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Cargo do usuário
        /// </summary>
        public string? Cargo { get; set; }

        /// <summary>
        /// Departamento do usuário
        /// </summary>
        public string? Departamento { get; set; }

        /// <summary>
        /// Indica se o usuário está ativo
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// ID do usuário superior
        /// </summary>
        public int? UsuarioSuperiorId { get; set; }

        /// <summary>
        /// Nome do usuário superior
        /// </summary>
        public string? UsuarioSuperiorNome { get; set; }

        /// <summary>
        /// Object ID do Azure AD
        /// </summary>
        public string? ObjectId { get; set; }

        /// <summary>
        /// UPN do usuário
        /// </summary>
        public string? Upn { get; set; }

        /// <summary>
        /// Display name do usuário
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Indica se é um usuário externo
        /// </summary>
        public bool IsExternal { get; set; }
    }
}
