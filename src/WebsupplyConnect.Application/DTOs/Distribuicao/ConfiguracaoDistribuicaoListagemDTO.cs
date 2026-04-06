using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para listagem de configurações de distribuição (sem loop infinito)
    /// </summary>
    public class ConfiguracaoDistribuicaoListagemDTO
    {
        /// <summary>
        /// ID da configuração
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID da empresa
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>
        /// Nome da configuração
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição da configuração
        /// </summary>
        public string? Descricao { get; set; }

        /// <summary>
        /// Indica se a configuração está ativa
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Data de início da vigência
        /// </summary>
        public DateTime? DataInicioVigencia { get; set; }

        /// <summary>
        /// Data de fim da vigência
        /// </summary>
        public DateTime? DataFimVigencia { get; set; }

        /// <summary>
        /// Indica se permite atribuição manual
        /// </summary>
        public bool PermiteAtribuicaoManual { get; set; }

        /// <summary>
        /// Máximo de leads ativos por vendedor
        /// </summary>
        public int? MaxLeadsAtivosVendedor { get; set; }

        /// <summary>
        /// Indica se considera horário de trabalho
        /// </summary>
        public bool ConsiderarHorarioTrabalho { get; set; }

        /// <summary>
        /// Indica se considera feriados
        /// </summary>
        public bool ConsiderarFeriados { get; set; }

        /// <summary>
        /// Parâmetros gerais em JSON
        /// </summary>
        public string ParametrosGerais { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data de modificação
        /// </summary>
        public DateTime DataModificacao { get; set; }

        /// <summary>
        /// Total de regras associadas
        /// </summary>
        public int TotalRegras { get; set; }

        /// <summary>
        /// Total de regras ativas
        /// </summary>
        public int RegrasAtivas { get; set; }
    }
}
