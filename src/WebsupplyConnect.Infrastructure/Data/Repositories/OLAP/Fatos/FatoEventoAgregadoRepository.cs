using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Fatos;

internal class FatoEventoAgregadoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IFatoEventoAgregadoRepository
{
    public async Task<FatoEventoAgregado?> ObterPorLeadEventoDataReferenciaAsync(
        int leadEventoId, DateTime dataReferencia, CancellationToken cancellationToken = default)
    {
        var dataReferenciaHora = new DateTime(
            dataReferencia.Year, dataReferencia.Month, dataReferencia.Day,
            dataReferencia.Hour, 0, 0);

        return await _context.FatoEventoAgregado
            .FirstOrDefaultAsync(f =>
                f.LeadEventoId == leadEventoId &&
                f.DataReferencia == dataReferenciaHora &&
                !f.Excluido, cancellationToken);
    }

    public async Task<List<FatoEventoAgregado>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoEventoAgregado
            .Where(f => f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim && !f.Excluido);

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FatoEventoAgregado>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoEventoAgregado
            .Where(f => !f.Excluido &&
                ((f.DataUltimoEvento != null && f.DataUltimoEvento >= dataInicio && f.DataUltimoEvento <= dataFim) ||
                 (f.DataUltimoEvento == null && f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim)));

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(FatoEventoAgregado fato, CancellationToken cancellationToken = default)
    {
        var fatoExistente = await ObterPorLeadEventoDataReferenciaAsync(
            fato.LeadEventoId, fato.DataReferencia, cancellationToken);

        if (fatoExistente != null)
        {
            fatoExistente.AtualizarDimensoes(fato.EquipeId, fato.VendedorId, fato.CampanhaId);

            fatoExistente.AtualizarMetricas(
                fato.TotalOportunidadesGeradas,
                fato.OportunidadesGanhas,
                fato.OportunidadesPerdidas,
                fato.ValorTotalOportunidadesGanhas);
            fatoExistente.AtualizarMetricasConversao(fato.EhConvertido, fato.DataConversao);
            fatoExistente.AtualizarMetricasCicloVendas(fato.DuracaoCicloCompletoDias, fato.TempoAtePrimeiraOportunidadeDias);
            fatoExistente.AtualizarMetricasAtendimento(
                fato.TempoMedioRespostaMinutos, fato.TempoMedioPrimeiroAtendimentoMinutos,
                fato.TotalConversas, fato.TotalMensagens, fato.ConversasNaoLidas);
            fatoExistente.AtualizarProdutoInteresse(fato.ProdutoInteresse);
            fatoExistente.AtualizarDataUltimoEvento(fato.DataUltimoEvento);

            Update(fatoExistente);
        }
        else
        {
            await CreateAsync(fato);
        }
    }

    public async Task ExcluirTodosPorLeadIdAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var registros = await _context.FatoEventoAgregado
            .Where(f => f.LeadId == leadId && !f.Excluido)
            .ToListAsync(cancellationToken);

        foreach (var r in registros)
        {
            r.ExcluirLogicamente();
            Update(r);
        }
    }
}
