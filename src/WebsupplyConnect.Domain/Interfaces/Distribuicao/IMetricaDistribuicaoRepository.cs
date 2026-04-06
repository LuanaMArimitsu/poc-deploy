using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsupplyConnect.Domain.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para o resposit�rio de métricas consolidadas de distribuição
    /// </summary>
    public interface IMetricaDistribuicaoRepository : IBaseRepository
    {
        /// <summary>
        /// Obtém a métrica de distribuição para uma data específica
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataReferencia">Data de referência</param>
        /// <param name="includeDeleted">Se deve incluir métricas excluídas</param>
        /// <returns>Métrica da data especificada ou null</returns>
        Task<MetricaDistribuicao?> GetMetricaPorDataAsync(
            int empresaId, 
            DateTime dataReferencia, 
            bool includeDeleted = false);

        /// <summary>
        /// Lista as métricas de distribuição para um período
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início do período</param>
        /// <param name="dataFim">Data de fim do período (opcional, usa DateTime.Now se não informado)</param>
        /// <param name="includeDeleted">Se deve incluir métricas excluídas</param>
        /// <returns>Lista de métricas no período especificado</returns>
        Task<List<MetricaDistribuicao>> ListMetricasPorPeriodoAsync(
            int empresaId, 
            DateTime dataInicio, 
            DateTime? dataFim = null,
            bool includeDeleted = false);

        /// <summary>
        /// Calcula métricas consolidadas para um período
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início do período</param>
        /// <param name="dataFim">Data de fim do período (opcional, usa DateTime.Now se não informado)</param>
        /// <returns>Objeto com métricas consolidadas do período</returns>
        Task<MetricaDistribuicao> CalcularMetricasConsolidadasAsync(
            int empresaId, 
            DateTime dataInicio, 
            DateTime? dataFim = null);

        /// <summary>
        /// Obtém a taxa de sucesso de distribuição para um período
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicio">Data de início do período</param>
        /// <param name="dataFim">Data de fim do período (opcional, usa DateTime.Now se não informado)</param>
        /// <returns>Taxa de sucesso média no período (0-100)</returns>
        Task<decimal> GetTaxaSucessoMedioPeriodoAsync(
            int empresaId, 
            DateTime dataInicio, 
            DateTime? dataFim = null);

        /// <summary>
        /// Compara métricas entre dois períodos
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="dataInicioPeriodo1">Data de início do primeiro período</param>
        /// <param name="dataFimPeriodo1">Data de fim do primeiro período</param>
        /// <param name="dataInicioPeriodo2">Data de início do segundo período</param>
        /// <param name="dataFimPeriodo2">Data de fim do segundo período</param>
        /// <returns>Um dicionário com as diferenças percentuais entre os períodos</returns>
        Task<Dictionary<string, decimal>> CompararPeriodosAsync(
            int empresaId,
            DateTime dataInicioPeriodo1,
            DateTime dataFimPeriodo1,
            DateTime dataInicioPeriodo2,
            DateTime dataFimPeriodo2);
    }
}