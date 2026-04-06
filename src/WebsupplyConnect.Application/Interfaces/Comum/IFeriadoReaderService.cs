using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsupplyConnect.Application.DTOs.Comum;

namespace WebsupplyConnect.Application.Interfaces.Comum
{
    /// <summary>
    /// Interface do serviço de feriados
    /// </summary>
    public interface IFeriadoReaderService
    {
        /// <summary>
        /// Obtém todos os feriados
        /// </summary>
        /// <returns>Lista de feriados</returns>
        Task<List<FeriadoDTO>> ObterTodosAsync();

        /// <summary>
        /// Obtém um feriado pelo seu ID
        /// </summary>
        /// <param name="id">ID do feriado</param>
        /// <returns>O feriado se encontrado, ou null</returns>
        Task<FeriadoDTO> ObterPorIdAsync(int id);

        /// <summary>
        /// Obtém todos os feriados para uma empresa específica, incluindo feriados nacionais
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="ano">Ano opcional para filtrar (nulo traz todos os anos)</param>
        /// <returns>Lista de feriados</returns>
        Task<List<FeriadoDTO>> ObterFeriadosPorEmpresaAsync(int empresaId, int? ano = null);

        /// <summary>
        /// Verifica se uma data específica é feriado para uma empresa
        /// </summary>
        /// <param name="data">Data a ser verificada</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="considerarRecorrentes">Indica se deve considerar feriados recorrentes</param>
        /// <returns>True se for feriado, False caso contrário</returns>
        Task<bool> VerificarDataFeriadoAsync(DateTime data, int? empresaId = null, bool considerarRecorrentes = true);
        /// <summary>
        /// Obtém os próximos feriados a partir da data atual
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="quantidade">Quantidade de feriados a retornar</param>
        /// <returns>Lista de próximos feriados</returns>
        Task<List<FeriadoDTO>> ObterProximosFeriadosAsync(int empresaId, int quantidade = 5);

        /// <summary>
        /// Obtém feriados por tipo
        /// </summary>
        /// <param name="tipo">Tipo de feriado (Nacional, Estadual, Municipal, Empresa)</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do tipo especificado</returns>
        Task<List<FeriadoDTO>> ObterFeriadosPorTipoAsync(string tipo, int? ano = null);

        /// <summary>
        /// Obtém feriados por UF
        /// </summary>
        /// <param name="uf">Código da UF</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do estado especificado</returns>
        Task<List<FeriadoDTO>> ObterFeriadosPorUFAsync(string uf, int? ano = null);
    }
}