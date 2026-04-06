using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Fatos;

internal class FatoOportunidadeMetricaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IFatoOportunidadeMetricaRepository
{
    public async Task<FatoOportunidadeMetrica?> ObterPorOportunidadeDataReferenciaAsync(
        int oportunidadeId, DateTime dataReferencia, CancellationToken cancellationToken = default)
    {
        var dataReferenciaHora = new DateTime(
            dataReferencia.Year, dataReferencia.Month, dataReferencia.Day,
            dataReferencia.Hour, 0, 0);

        return await _context.FatoOportunidadeMetrica
            .FirstOrDefaultAsync(f =>
                f.OportunidadeId == oportunidadeId &&
                f.DataReferencia == dataReferenciaHora &&
                !f.Excluido, cancellationToken);
    }

    public async Task<Dictionary<(int OportunidadeId, DateTime DataReferencia), FatoOportunidadeMetrica>>
        ObterPorChavesOportunidadeDataReferenciaAsync(
            IReadOnlyList<(int OportunidadeId, DateTime DataReferencia)> chaves,
            CancellationToken cancellationToken = default)
    {
        if (chaves.Count == 0)
            return new Dictionary<(int, DateTime), FatoOportunidadeMetrica>();

        var norm = chaves
            .Select(c => (
                c.OportunidadeId,
                DataRef: new DateTime(c.DataReferencia.Year, c.DataReferencia.Month, c.DataReferencia.Day,
                    c.DataReferencia.Hour, 0, 0)))
            .Distinct()
            .ToList();

        var oportunidadeIds = norm.Select(x => x.OportunidadeId).Distinct().ToList();
        var candidatos = await _context.FatoOportunidadeMetrica
            .Where(f => !f.Excluido && oportunidadeIds.Contains(f.OportunidadeId))
            .ToListAsync(cancellationToken);

        var set = norm.ToHashSet();
        var dict = new Dictionary<(int, DateTime), FatoOportunidadeMetrica>();
        foreach (var f in candidatos)
        {
            var key = (f.OportunidadeId, f.DataReferencia);
            if (set.Contains(key))
                dict[key] = f;
        }

        return dict;
    }

    public async Task<List<FatoOportunidadeMetrica>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoOportunidadeMetrica
            .Where(f => f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim && !f.Excluido);

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FatoOportunidadeMetrica>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoOportunidadeMetrica
            .Where(f => !f.Excluido &&
                ((f.DataUltimoEvento != null && f.DataUltimoEvento >= dataInicio && f.DataUltimoEvento <= dataFim) ||
                 (f.DataUltimoEvento == null && f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim)));

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FatoOportunidadeMetrica>> ObterOportunidadesEstagnadasAsync(
        int diasEstagnacao = 30, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoOportunidadeMetrica
            .Where(f => f.EhEstagnada && !f.Excluido && !f.EhGanha && !f.EhPerdida);

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query
            .OrderByDescending(f => f.DiasDesdeUltimaInteracao)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(FatoOportunidadeMetrica fato, CancellationToken cancellationToken = default)
    {
        var fatoExistente = await ObterPorOportunidadeDataReferenciaAsync(
            fato.OportunidadeId, fato.DataReferencia, cancellationToken);

        if (fatoExistente != null)
        {
            fatoExistente.AtualizarDimensoes(fato.EquipeId, fato.VendedorId, fato.CampanhaId, fato.DimensaoEtapaFunilId);

            fatoExistente.AtualizarMetricas(
                fato.ValorEstimado,
                fato.ValorFinal,
                fato.Probabilidade,
                fato.EhGanha,
                fato.EhPerdida,
                fato.DataFechamento);

            fatoExistente.AtualizarMetricasCicloVendas(
                fato.DuracaoCicloVendasDias,
                fato.TempoEmEtapaAtualDias,
                fato.DiasDesdeUltimaInteracao,
                fato.EhEstagnada,
                fato.ValorEsperadoPipeline);

            fatoExistente.AtualizarMetricasTaxaConversao(
                fato.TaxaConversaoEtapa,
                fato.WinRateEtapa);

            fatoExistente.AtualizarMetricasLead(
                fato.TempoMedioRespostaMinutos,
                fato.TempoMedioPrimeiroAtendimentoMinutos,
                fato.TotalConversas,
                fato.ConversasNaoLidas);

            fatoExistente.AtualizarDataUltimoEvento(fato.DataUltimoEvento);

            Update(fatoExistente);
        }
        else
        {
            await CreateAsync(fato);
        }
    }
}
