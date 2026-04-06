using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Fatos;

internal class FatoLeadAgregadoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IFatoLeadAgregadoRepository
{
    public async Task<List<FatoLeadAgregado>> ObterUnicosPorLeadParaListagemAsync(
        FatoLeadAgregadoConsultaFiltro filtro, CancellationToken cancellationToken = default)
    {
        if (FiltroDimensaoImpossivel(filtro))
            return [];

        var query = CriarQueryPeriodoDataUltimoEvento(filtro);

        query = AplicarFiltrosDimensaoLead(query, filtro);

        var deduped = query
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First());

        return await deduped.ToListAsync(cancellationToken);
    }

    private static bool FiltroDimensaoImpossivel(FatoLeadAgregadoConsultaFiltro filtro)
    {
        return (filtro.FiltrarEmpresa && filtro.EmpresaDimIds.Count == 0) ||
               (filtro.FiltrarEquipe && filtro.EquipeDimIds.Count == 0) ||
               (filtro.FiltrarVendedor && filtro.VendedorDimIds.Count == 0) ||
               (filtro.FiltrarOrigem && filtro.OrigemDimIds.Count == 0) ||
               (filtro.FiltrarCampanha && filtro.CampanhaDimIds.Count == 0) ||
               (filtro.FiltrarStatus && filtro.StatusDimIds.Count == 0) ||
               (filtro.FiltrarEtapaFunil && filtro.EtapaFunilDimIds.Count == 0);
    }

    private IQueryable<FatoLeadAgregado> CriarQueryPeriodoDataUltimoEvento(FatoLeadAgregadoConsultaFiltro filtro)
    {
        var query = _context.FatoLeadAgregado.AsNoTracking()
            .Where(f => !f.Excluido &&
                ((f.DataUltimoEvento != null && f.DataUltimoEvento >= filtro.DataInicio && f.DataUltimoEvento <= filtro.DataFim) ||
                 (f.DataUltimoEvento == null && f.DataReferencia >= filtro.DataInicio && f.DataReferencia <= filtro.DataFim)));

        if (filtro.EmpresaDimensaoId.HasValue)
            query = query.Where(f => f.EmpresaId == filtro.EmpresaDimensaoId.Value);

        return query;
    }

    private static IQueryable<FatoLeadAgregado> AplicarFiltrosDimensaoLead(
        IQueryable<FatoLeadAgregado> query,
        FatoLeadAgregadoConsultaFiltro filtro)
    {
        if (filtro.FiltrarEmpresa)
            query = query.Where(f => filtro.EmpresaDimIds.Contains(f.EmpresaId));

        if (filtro.FiltrarEquipe)
            query = query.Where(f => f.EquipeId.HasValue && filtro.EquipeDimIds.Contains(f.EquipeId!.Value));

        if (filtro.FiltrarVendedor)
            query = query.Where(f => f.VendedorId.HasValue && filtro.VendedorDimIds.Contains(f.VendedorId!.Value));

        if (filtro.FiltrarOrigem)
            query = query.Where(f => filtro.OrigemDimIds.Contains(f.OrigemId));

        if (filtro.FiltrarCampanha)
            query = query.Where(f => f.CampanhaId.HasValue && filtro.CampanhaDimIds.Contains(f.CampanhaId!.Value));

        if (filtro.FiltrarStatus)
            query = query.Where(f => f.StatusAtualId.HasValue && filtro.StatusDimIds.Contains(f.StatusAtualId!.Value));

        return query;
    }

    public async Task<FatoLeadAgregado?> ObterPorLeadIdAsync(
        int leadId, CancellationToken cancellationToken = default)
    {
        return await _context.FatoLeadAgregado
            .Where(f => f.LeadId == leadId && !f.Excluido)
            .OrderByDescending(f => f.DataReferencia)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FatoLeadAgregado?> ObterPorLeadDataReferenciaAsync(
        int leadId, DateTime dataReferencia, CancellationToken cancellationToken = default)
    {
        var dataReferenciaHora = new DateTime(
            dataReferencia.Year, dataReferencia.Month, dataReferencia.Day,
            dataReferencia.Hour, 0, 0);

        return await _context.FatoLeadAgregado
            .FirstOrDefaultAsync(f =>
                f.LeadId == leadId &&
                f.DataReferencia == dataReferenciaHora &&
                !f.Excluido, cancellationToken);
    }

    public async Task<List<FatoLeadAgregado>> ObterPorPeriodoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoLeadAgregado
            .Where(f => f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim && !f.Excluido);

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FatoLeadAgregado>> ObterPorPeriodoDataUltimoEventoAsync(
        DateTime dataInicio, DateTime dataFim, int? empresaId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.FatoLeadAgregado
            .Where(f => !f.Excluido &&
                ((f.DataUltimoEvento != null && f.DataUltimoEvento >= dataInicio && f.DataUltimoEvento <= dataFim) ||
                 (f.DataUltimoEvento == null && f.DataReferencia >= dataInicio && f.DataReferencia <= dataFim)));

        if (empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(FatoLeadAgregado fato, CancellationToken cancellationToken = default)
    {
        var fatoExistente = await ObterPorLeadIdAsync(fato.LeadId, cancellationToken);

        if (fatoExistente != null)
        {
            fatoExistente.AtualizarDimensoesCompletas(
                fato.TempoId, fato.EmpresaId, fato.EquipeId, fato.VendedorId,
                fato.StatusAtualId, fato.OrigemId, fato.CampanhaId, fato.DataReferencia);

            fatoExistente.AtualizarMetricasAgregadas(
                fato.TotalEventos,
                fato.TotalOportunidades,
                fato.OportunidadesGanhas,
                fato.OportunidadesPerdidas,
                fato.ValorTotalOportunidadesGanhas);

            fatoExistente.AtualizarMetricasConversao(
                fato.EhConvertido,
                fato.EhConvertidoPorOportunidade,
                fato.DataConversao);

            fatoExistente.AtualizarMetricasCicloVendas(
                fato.DuracaoCicloCompletoDias,
                fato.TempoAtePrimeiraOportunidadeDias,
                fato.TempoAtePrimeiraConversaoDias);

            fatoExistente.AtualizarMetricasTaxaConversao(
                fato.TaxaConversaoLeadParaOportunidade,
                fato.TaxaQualificacaoLead);

            fatoExistente.AtualizarMetricasAtendimento(
                fato.TempoMedioRespostaMinutos,
                fato.TempoMedioPrimeiroAtendimentoMinutos,
                fato.TotalConversas,
                fato.TotalMensagens,
                fato.ConversasNaoLidas,
                fato.AguardandoRespostaVendedor,
                fato.AguardandoRespostaAtendimento);

            fatoExistente.AtualizarProdutoInteresse(fato.ProdutoInteresse);
            fatoExistente.AtualizarDataUltimoEvento(fato.DataUltimoEvento);

            Update(fatoExistente);
        }
        else
        {
            await CreateAsync(fato);
        }
    }

    public async Task LimparDuplicatasAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var registros = await _context.FatoLeadAgregado
            .Where(f => f.LeadId == leadId && !f.Excluido)
            .OrderByDescending(f => f.DataReferencia)
            .ToListAsync(cancellationToken);

        if (registros.Count <= 1) return;

        foreach (var duplicata in registros.Skip(1))
        {
            duplicata.ExcluirLogicamente();
            Update(duplicata);
        }
    }

    public async Task ExcluirTodosPorLeadIdAsync(int leadId, CancellationToken cancellationToken = default)
    {
        var registros = await _context.FatoLeadAgregado
            .Where(f => f.LeadId == leadId && !f.Excluido)
            .ToListAsync(cancellationToken);

        foreach (var r in registros)
        {
            r.ExcluirLogicamente();
            Update(r);
        }
    }
}
