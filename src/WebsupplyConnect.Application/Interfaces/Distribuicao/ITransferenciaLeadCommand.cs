namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    /// <summary>
    /// Interface para comandos de transferência de lead
    /// Responsabilidade: Executar uma transferência específica
    /// </summary>
    public interface ITransferenciaLeadCommand
    {
        /// <summary>
        /// Executa a transferência de um lead
        /// </summary>
        /// <param name="leadId">ID do lead a ser transferido</param>
        /// <param name="novoResponsavelId">ID do novo responsável</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Task representando a operação assíncrona</returns>
        Task ExecutarAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId);
        Task ExecutarSemOportunidadeAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId);
    }
}
