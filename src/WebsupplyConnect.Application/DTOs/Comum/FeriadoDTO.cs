using System;

namespace WebsupplyConnect.Application.DTOs.Comum
{
    /// <summary>
    /// DTO para transferência de dados de Feriado
    /// </summary>
    public class FeriadoDTO
    {
        /// <summary>
        /// ID do feriado
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do feriado
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Data do feriado
        /// </summary>
        public DateTime Data { get; set; }

        /// <summary>
        /// Descrição opcional do feriado
        /// </summary>
        public string Descricao { get; set; }

        /// <summary>
        /// Tipo do feriado (Nacional, Estadual, Municipal, Empresa)
        /// </summary>
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
        public string UF { get; set; }

        /// <summary>
        /// Código IBGE do município para feriados municipais
        /// </summary>
        public string CodigoMunicipio { get; set; }

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data da última modificação do registro
        /// </summary>
        public DateTime DataModificacao { get; set; }
    }
}