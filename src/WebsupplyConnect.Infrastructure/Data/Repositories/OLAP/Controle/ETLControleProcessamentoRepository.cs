using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.OLAP.Controle;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.OLAP.Controle;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Controle;

internal class ETLControleProcessamentoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IETLControleProcessamentoRepository
{
    public async Task<ETLControleProcessamento?> ObterPorTipoAsync(string tipoProcessamento, CancellationToken cancellationToken = default)
    {
        return await _context.ETLControleProcessamento
            .FirstOrDefaultAsync(c =>
                c.TipoProcessamento == tipoProcessamento && !c.Excluido, cancellationToken);
    }

    public async Task<ETLControleProcessamento> ObterOuCriarAsync(string tipoProcessamento, CancellationToken cancellationToken = default)
    {
        var existente = await ObterPorTipoAsync(tipoProcessamento, cancellationToken);
        if (existente != null)
        {
            return existente;
        }

        var novo = new ETLControleProcessamento(tipoProcessamento);
        await CreateAsync(novo);
        await _unitOfWork.SaveChangesAsync();
        return novo;
    }

    public async Task<(bool ok, ETLControleProcessamento controle)> GarantirControleEAdquirirExecucaoAsync(
        string tipoProcessamento, TimeSpan execucaoMaxima, CancellationToken cancellationToken = default)
    {
        var agora = TimeHelper.GetBrasiliaTime();
        var limiteStale = agora - execucaoMaxima;

        var row = await _context.ETLControleProcessamento
            .FromSqlRaw(
                "SELECT * FROM OLAP.ETLControleProcessamento WITH (UPDLOCK, ROWLOCK) WHERE TipoProcessamento = {0} AND Excluido = 0",
                tipoProcessamento)
            .FirstOrDefaultAsync(cancellationToken);

        if (row == null)
        {
            var novo = new ETLControleProcessamento(tipoProcessamento);
            await CreateAsync(novo);
            await _unitOfWork.SaveChangesAsync();

            row = await _context.ETLControleProcessamento
                .FromSqlRaw(
                    "SELECT * FROM OLAP.ETLControleProcessamento WITH (UPDLOCK, ROWLOCK) WHERE TipoProcessamento = {0} AND Excluido = 0",
                    tipoProcessamento)
                .FirstAsync(cancellationToken);
        }

        if (row.StatusUltimaExecucao == "EmProcessamento" && row.DataUltimaExecucao > limiteStale)
            return (false, row);

        row.IniciarProcessamento();
        Update<ETLControleProcessamento>(row);
        await _unitOfWork.SaveChangesAsync();
        return (true, row);
    }
}
