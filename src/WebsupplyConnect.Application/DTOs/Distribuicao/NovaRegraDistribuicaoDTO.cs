using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para criação de uma nova regra de distribuição
    /// </summary>
    public class NovaRegraDistribuicaoDTO
    {
        /// <summary>
        /// ID da configuração de distribuição à qual esta regra pertence
        /// </summary>
        [Required(ErrorMessage = "O ID da configuração de distribuição é obrigatório")]
        public int ConfiguracaoDistribuicaoId { get; set; }

        /// <summary>
        /// ID do tipo de regra
        /// </summary>
        [Required(ErrorMessage = "O ID do tipo de regra é obrigatório")]
        public int TipoRegraId { get; set; }

        /// <summary>
        /// Nome identificador da regra
        /// </summary>
        [Required(ErrorMessage = "O nome da regra é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do funcionamento desta regra
        /// </summary>
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Ordem de aplicação da regra
        /// </summary>
        [Range(1, 100, ErrorMessage = "A ordem deve estar entre {1} e {2}")]
        public int Ordem { get; set; } = 1;

        /// <summary>
        /// Peso da regra no cálculo final (0-100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "O peso deve estar entre {1} e {2}")]
        public int Peso { get; set; } = 50;

        /// <summary>
        /// Indica se esta regra está ativa
        /// </summary>
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Indica se esta regra é obrigatória
        /// </summary>
        public bool Obrigatoria { get; set; } = false;

        /// <summary>
        /// Pontuação mínima que um vendedor deve ter nesta regra para ser elegível
        /// </summary>
        [Range(0, 100, ErrorMessage = "A pontuação mínima deve estar entre {1} e {2}")]
        public int? PontuacaoMinima { get; set; }

        /// <summary>
        /// Pontuação máxima possível nesta regra
        /// </summary>
        [Range(0, 100, ErrorMessage = "A pontuação máxima deve estar entre {1} e {2}")]
        public int? PontuacaoMaxima { get; set; } = 100;

        /// <summary>
        /// JSON com os parâmetros específicos desta regra
        /// </summary>
        public string ParametrosJson { get; set; } = "{}";
    }

    /// <summary>
    /// DTO para atualização da ordem de uma regra
    /// </summary>
    public class AtualizarOrdemRegraDTO
    {
        /// <summary>
        /// Nova ordem da regra
        /// </summary>
        [Required(ErrorMessage = "A nova ordem é obrigatória")]
        [Range(1, 100, ErrorMessage = "A ordem deve estar entre {1} e {2}")]
        public int NovaOrdem { get; set; }
    }

    /// <summary>
    /// DTO para atualização de status (ativo/inativo) de uma regra
    /// </summary>
    public class AtualizarStatusRegraDTO
    {
        /// <summary>
        /// Indica se a regra deve ser ativada (true) ou desativada (false)
        /// </summary>
        [Required(ErrorMessage = "O status é obrigatório")]
        public bool Ativar { get; set; }
    }

    /// <summary>
    /// DTO para informações detalhadas de uma regra de distribuição
    /// </summary>
    public class RegraDistribuicaoInfoDTO
    {
        /// <summary>
        /// ID da regra
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID da configuração de distribuição à qual esta regra pertence
        /// </summary>
        public int ConfiguracaoDistribuicaoId { get; set; }

        /// <summary>
        /// Nome da configuração de distribuição
        /// </summary>
        public string NomeConfiguracao { get; set; } = string.Empty;

        /// <summary>
        /// ID do tipo de regra
        /// </summary>
        public int TipoRegraId { get; set; }

        /// <summary>
        /// Nome do tipo de regra
        /// </summary>
        public string NomeTipoRegra { get; set; } = string.Empty;

        /// <summary>
        /// Nome identificador da regra
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do funcionamento desta regra
        /// </summary>
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Ordem de aplicação da regra
        /// </summary>
        public int Ordem { get; set; }

        /// <summary>
        /// Peso da regra no cálculo final (0-100)
        /// </summary>
        public int Peso { get; set; }

        /// <summary>
        /// Indica se esta regra está ativa
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Indica se esta regra é obrigatória
        /// </summary>
        public bool Obrigatoria { get; set; }

        /// <summary>
        /// Pontuação mínima que um vendedor deve ter nesta regra para ser elegível
        /// </summary>
        public int? PontuacaoMinima { get; set; }

        /// <summary>
        /// Pontuação máxima possível nesta regra
        /// </summary>
        public int? PontuacaoMaxima { get; set; }

        /// <summary>
        /// JSON com os parâmetros específicos desta regra
        /// </summary>
        public string ParametrosJson { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação da regra
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data da última modificação da regra
        /// </summary>
        public DateTime DataModificacao { get; set; }

        /// <summary>
        /// Lista de parâmetros da regra
        /// </summary>
        public List<ParametroRegraDTO> Parametros { get; set; } = new List<ParametroRegraDTO>();
    }

    /// <summary>
    /// DTO para parâmetro de regra de distribuição
    /// </summary>
    public class ParametroRegraDTO
    {
        /// <summary>
        /// ID do parâmetro
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID da regra de distribuição à qual este parâmetro pertence
        /// </summary>
        public int RegraDistribuicaoId { get; set; }

        /// <summary>
        /// Nome do parâmetro (chave)
        /// </summary>
        [Required(ErrorMessage = "O nome do parâmetro é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome deve ter no máximo {1} caracteres")]
        public string NomeParametro { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do parâmetro (ex: string, int, boolean, date, etc.)
        /// </summary>
        [Required(ErrorMessage = "O tipo do parâmetro é obrigatório")]
        [StringLength(20, ErrorMessage = "O tipo deve ter no máximo {1} caracteres")]
        public string TipoParametro { get; set; } = string.Empty;

        /// <summary>
        /// Valor do parâmetro
        /// </summary>
        public string ValorParametro { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do parâmetro
        /// </summary>
        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo {1} caracteres")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o parâmetro é obrigatório
        /// </summary>
        public bool Obrigatorio { get; set; }

        /// <summary>
        /// Expressão regular para validação do valor do parâmetro
        /// </summary>
        public string ValidacaoRegex { get; set; } = string.Empty;

        /// <summary>
        /// Valor padrão do parâmetro quando não especificado
        /// </summary>
        public string ValorPadrao { get; set; } = string.Empty;
    }
}