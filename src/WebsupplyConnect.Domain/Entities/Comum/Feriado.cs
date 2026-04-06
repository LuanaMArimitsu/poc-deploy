using System;
using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.Comum
{
    /// <summary>
    /// Representa um feriado no sistema, que pode ser nacional, estadual, municipal ou específico de uma empresa
    /// </summary>
    public class Feriado : EntidadeBase
    {
        /// <summary>
        /// Nome do feriado
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Data do feriado
        /// </summary>
        public DateTime Data { get; private set; }

        /// <summary>
        /// Descrição opcional do feriado
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Tipo do feriado (Nacional, Estadual, Municipal, Empresa)
        /// </summary>
        public string Tipo { get; private set; }

        /// <summary>
        /// ID da empresa associada ao feriado, se for um feriado específico de empresa.
        /// Nulo para feriados nacionais, estaduais ou municipais.
        /// </summary>
        public int? EmpresaId { get; private set; }

        /// <summary>
        /// Indica se o feriado se repete anualmente na mesma data
        /// </summary>
        public bool Recorrente { get; private set; }

        /// <summary>
        /// Código UF para feriados estaduais
        /// </summary>
        public string UF { get; private set; }

        /// <summary>
        /// Código IBGE do município para feriados municipais
        /// </summary>
        public string CodigoMunicipio { get; private set; }

        /// <summary>
        /// Construtor protegido para uso pelo EF Core
        /// </summary>
        protected Feriado()
        {
            Nome = string.Empty;
            Descricao = string.Empty;
            Tipo = string.Empty;
            UF = string.Empty;
            CodigoMunicipio = string.Empty;
        }

        /// <summary>
        /// Construtor para criar um novo feriado
        /// </summary>
        /// <param name="nome">Nome do feriado</param>
        /// <param name="data">Data do feriado</param>
        /// <param name="tipo">Tipo do feriado (Nacional, Estadual, Municipal, Empresa)</param>
        /// <param name="recorrente">Indica se o feriado se repete anualmente</param>
        /// <param name="descricao">Descrição opcional do feriado</param>
        /// <param name="empresaId">ID da empresa associada ao feriado (opcional)</param>
        /// <param name="uf">UF para feriados estaduais (opcional)</param>
        /// <param name="codigoMunicipio">Código IBGE do município para feriados municipais (opcional)</param>
        public Feriado(
            string nome,
            DateTime data,
            string tipo,
            bool recorrente,
            string descricao = "",
            int? empresaId = null,
            string uf = "",
            string codigoMunicipio = "")
        {
            ValidarCamposObrigatorios(nome, data, tipo);

            Nome = nome;
            Data = data;
            Tipo = tipo;
            Recorrente = recorrente;
            Descricao = descricao ?? string.Empty;
            EmpresaId = empresaId;
            UF = uf ?? string.Empty;
            CodigoMunicipio = codigoMunicipio ?? string.Empty;

            ValidarTipoFeriado();
        }

        /// <summary>
        /// Atualiza as informações do feriado
        /// </summary>
        /// <param name="nome">Nome do feriado</param>
        /// <param name="data">Data do feriado</param>
        /// <param name="tipo">Tipo do feriado (Nacional, Estadual, Municipal, Empresa)</param>
        /// <param name="recorrente">Indica se o feriado se repete anualmente</param>
        /// <param name="descricao">Descrição opcional do feriado</param>
        /// <param name="empresaId">ID da empresa associada ao feriado (opcional)</param>
        /// <param name="uf">UF para feriados estaduais (opcional)</param>
        /// <param name="codigoMunicipio">Código IBGE do município para feriados municipais (opcional)</param>
        public void Atualizar(
            string nome,
            DateTime data,
            string tipo,
            bool recorrente,
            string descricao = "",
            int? empresaId = null,
            string uf = "",
            string codigoMunicipio = "")
        {
            ValidarCamposObrigatorios(nome, data, tipo);

            Nome = nome;
            Data = data;
            Tipo = tipo;
            Recorrente = recorrente;
            Descricao = descricao ?? string.Empty;
            EmpresaId = empresaId;
            UF = uf ?? string.Empty;
            CodigoMunicipio = codigoMunicipio ?? string.Empty;

            DataModificacao = DateTime.Now;
            ValidarTipoFeriado();
        }

        /// <summary>
        /// Verifica se a data atual está dentro do período do feriado
        /// </summary>
        /// <returns>True se for hoje o feriado, False caso contrário</returns>
        public bool EhHoje()
        {
            var hoje = DateTime.Today;
            
            if (Recorrente)
            {
                // Para feriados recorrentes, comparamos apenas dia e mês
                return hoje.Day == Data.Day && hoje.Month == Data.Month;
            }
            
            // Para feriados não recorrentes, comparamos a data completa
            return hoje.Date == Data.Date;
        }

        /// <summary>
        /// Verifica se uma data específica é este feriado
        /// </summary>
        /// <param name="data">Data a ser verificada</param>
        /// <returns>True se a data for o feriado, False caso contrário</returns>
        public bool EhFeriado(DateTime data)
        {
            if (Recorrente)
            {
                // Para feriados recorrentes, comparamos apenas dia e mês
                return data.Day == Data.Day && data.Month == Data.Month;
            }
            
            // Para feriados não recorrentes, comparamos a data completa
            return data.Date == Data.Date;
        }

        #region Validações

        private void ValidarCamposObrigatorios(string nome, DateTime data, string tipo)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome do feriado não pode ser vazio", nameof(nome));

            if (data == default)
                throw new ArgumentException("Data do feriado não pode ser vazia", nameof(data));

            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo do feriado não pode ser vazio", nameof(tipo));
        }

        private void ValidarTipoFeriado()
        {
            // Valida que tipo está entre os valores permitidos
            if (!string.Equals(Tipo, "Nacional", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Tipo, "Estadual", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Tipo, "Municipal", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Tipo, "Empresa", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Tipo de feriado '{Tipo}' inválido. Os tipos válidos são: Nacional, Estadual, Municipal ou Empresa.");
            }

            // Valida campos específicos para cada tipo
            switch (Tipo.ToUpperInvariant())
            {
                case "ESTADUAL":
                    if (string.IsNullOrWhiteSpace(UF))
                        throw new ArgumentException("Para feriados estaduais, o campo UF é obrigatório.");
                    break;

                case "MUNICIPAL":
                    if (string.IsNullOrWhiteSpace(CodigoMunicipio))
                        throw new ArgumentException("Para feriados municipais, o campo Código do Município é obrigatório.");
                    break;

                case "EMPRESA":
                    if (!EmpresaId.HasValue)
                        throw new ArgumentException("Para feriados de empresa, o campo EmpresaId é obrigatório.");
                    break;
            }
        }

        #endregion
    }
}