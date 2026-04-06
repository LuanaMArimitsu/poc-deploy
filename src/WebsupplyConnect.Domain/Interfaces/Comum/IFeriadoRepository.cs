using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comum
{
    /// <summary>
    /// Interface do repositório de feriados
    /// </summary>
    public interface IFeriadoRepository : IBaseRepository
    {
        /// <summary>
        /// Obtém todos os feriados para uma empresa específica, incluindo feriados nacionais
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="ano">Ano opcional para filtrar (nulo traz todos os anos)</param>
        /// <returns>Lista de feriados</returns>
        Task<List<Feriado>> ObterFeriadosEmpresaAsync(int empresaId, int? ano = null);
        
        /// <summary>
        /// Verifica se uma data específica é feriado, opcionalmente filtrando por empresa
        /// </summary>
        /// <param name="data">Data a ser verificada</param>
        /// <param name="empresaId">ID da empresa (opcional)</param>
        /// <param name="considerarRecorrentes">Indica se deve considerar feriados recorrentes</param>
        /// <returns>True se for feriado, False caso contrário</returns>
        Task<bool> VerificarDataFeriadoAsync(DateTime data, int? empresaId = null, bool considerarRecorrentes = true);

        /// <summary>
        /// Obtém os próximos feriados a partir da data atual
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="quantidade">Quantidade de feriados a retornar</param>
        /// <returns>Lista de próximos feriados</returns>
        Task<List<Feriado>> ObterProximosFeriadosAsync(int empresaId, int quantidade = 5);
        
        /// <summary>
        /// Obtém feriados por tipo
        /// </summary>
        /// <param name="tipo">Tipo de feriado (Nacional, Estadual, Municipal, Empresa)</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do tipo especificado</returns>
        Task<List<Feriado>> ObterFeriadosPorTipoAsync(string tipo, int? ano = null);
        
        /// <summary>
        /// Obtém feriados por UF
        /// </summary>
        /// <param name="uf">Código da UF</param>
        /// <param name="ano">Ano opcional para filtrar</param>
        /// <returns>Lista de feriados do estado especificado</returns>
        Task<List<Feriado>> ObterFeriadosPorUFAsync(string uf, int? ano = null);
        
        /// <summary>
        /// Obtém todos os feriados
        /// </summary>
        /// <returns>Lista de todos os feriados</returns>
        Task<List<Feriado>> GetAllAsync();
        
        /// <summary>
        /// Adiciona um novo feriado
        /// </summary>
        /// <param name="feriado">Feriado a ser adicionado</param>
        /// <returns>O feriado adicionado</returns>
        Task<Feriado> AddAsync(Feriado feriado);
        
        /// <summary>
        /// Remove um feriado (exclusão lógica)
        /// </summary>
        /// <param name="id">ID do feriado</param>
        /// <returns>True se removido com sucesso, False caso contrário</returns>
        Task<bool> RemoveAsync(int id);
        
        /// <summary>
        /// Salva as alterações no banco de dados
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}