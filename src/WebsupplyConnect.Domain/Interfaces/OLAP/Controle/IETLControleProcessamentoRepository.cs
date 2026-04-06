using WebsupplyConnect.Domain.Entities.OLAP.Controle;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.OLAP.Controle;

public interface IETLControleProcessamentoRepository : IBaseRepository
{
    Task<ETLControleProcessamento?> ObterPorTipoAsync(string tipoProcessamento, CancellationToken cancellationToken = default);
    Task<ETLControleProcessamento> ObterOuCriarAsync(string tipoProcessamento, CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante linha de controle, bloqueia concorrência (UPDLOCK) e inicia processamento se não houver outra execução ativa não expirada.
    /// </summary>
    Task<(bool ok, ETLControleProcessamento controle)> GarantirControleEAdquirirExecucaoAsync(
        string tipoProcessamento, TimeSpan execucaoMaxima, CancellationToken cancellationToken = default);
}
