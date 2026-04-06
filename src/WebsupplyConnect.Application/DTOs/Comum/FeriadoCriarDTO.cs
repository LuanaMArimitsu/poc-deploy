using System;
using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Comum
{
    /// <summary>
    /// DTO para criação de um novo Feriado
    /// </summary>
    public class FeriadoCriarDTO
    {
        /// <summary>
        /// Nome do feriado
        /// </summary>
        [Required(ErrorMessage = "O nome do feriado é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome do feriado deve ter no máximo {1} caracteres")]
        public string Nome { get; set; }

        /// <summary>
        /// Data do feriado
        /// </summary>
        [Required(ErrorMessage = "A data do feriado é obrigatória")]
        public DateTime Data { get; set; }

        /// <summary>
        /// Descrição opcional do feriado
        /// </summary>
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo {1} caracteres")]
        public string Descricao { get; set; }

        /// <summary>
        /// Tipo do feriado (Nacional, Estadual, Municipal, Empresa)
        /// </summary>
        [Required(ErrorMessage = "O tipo do feriado é obrigatório")]
        [StringLength(20, ErrorMessage = "O tipo do feriado deve ter no máximo {1} caracteres")]
        public string Tipo { get; set; }

        /// <summary>
        /// ID da empresa associada ao feriado, se for um feriado específico de empresa.
        /// Nulo para feriados nacionais, estaduais ou municipais.
        /// </summary>
        public int? EmpresaId { get; set; }

        /// <summary>
        /// Indica se o feriado se repete anualmente na mesma data
        /// </summary>
        public bool Recorrente { get; set; }

        /// <summary>
        /// Código UF para feriados estaduais
        /// </summary>
        [StringLength(2, ErrorMessage = "O código UF deve ter {1} caracteres")]
        public string UF { get; set; }

        /// <summary>
        /// Código IBGE do município para feriados municipais
        /// </summary>
        [StringLength(7, ErrorMessage = "O código do município deve ter no máximo {1} caracteres")]
        public string CodigoMunicipio { get; set; }
    }
}