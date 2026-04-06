using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Infrastructure.Data;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.OLAP.Dimensoes;

internal class DimensaoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
    : BaseRepository(dbContext, unitOfWork), IDimensaoRepository
{
    public async Task<DimensaoTempo?> ObterDimensaoTempoPorDataAsync(DateTime data, CancellationToken cancellationToken = default)
    {
        var dataTruncada = new DateTime(data.Year, data.Month, data.Day, data.Hour, 0, 0);
        return await _context.DimensaoTempo
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.Ano == dataTruncada.Year &&
                d.Mes == dataTruncada.Month &&
                d.Dia == dataTruncada.Day &&
                d.Hora == dataTruncada.Hour &&
                !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoEmpresa?> ObterDimensaoEmpresaPorOrigemIdAsync(int empresaOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoEmpresa
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EmpresaOrigemId == empresaOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoEquipe?> ObterDimensaoEquipePorOrigemIdAsync(int equipeOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoEquipe
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EquipeOrigemId == equipeOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoVendedor?> ObterDimensaoVendedorPorOrigemIdAsync(int usuarioOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoVendedor
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UsuarioOrigemId == usuarioOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoStatusLead?> ObterDimensaoStatusLeadPorOrigemIdAsync(int statusOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoStatusLead
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.StatusOrigemId == statusOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdAsync(int origemOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoOrigem
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OrigemOrigemId == origemOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoOrigem?> ObterDimensaoOrigemPorOrigemIdIncluindoExcluidaAsync(int origemOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoOrigem
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OrigemOrigemId == origemOrigemId, cancellationToken);
    }

    public async Task<DimensaoCampanha?> ObterDimensaoCampanhaPorOrigemIdAsync(int campanhaOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoCampanha
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.CampanhaOrigemId == campanhaOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoFunil?> ObterDimensaoFunilPorOrigemIdAsync(int funilOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoFunil
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.FunilOrigemId == funilOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<DimensaoEtapaFunil?> ObterDimensaoEtapaFunilPorOrigemIdAsync(int etapaOrigemId, CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoEtapaFunil
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EtapaOrigemId == etapaOrigemId && !d.Excluido, cancellationToken);
    }

    public async Task<List<DimensaoEtapaFunil>> ObterDimensoesEtapaFunilNaoExcluidasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DimensaoEtapaFunil
            .AsNoTracking()
            .Where(d => !d.Excluido)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> ObterNomesFunilPorDimensaoFunilIdsAsync(
        IEnumerable<int> dimensaoFunilIds, CancellationToken cancellationToken = default)
    {
        var ids = dimensaoFunilIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        return await _context.DimensaoFunil
            .AsNoTracking()
            .Where(f => ids.Contains(f.Id) && !f.Excluido)
            .ToDictionaryAsync(f => f.Id, f => f.Nome, cancellationToken);
    }

    public async Task<HashSet<int>> ObterIdsDimensaoOrigemReferenciadosEmFatosAsync(CancellationToken cancellationToken = default)
    {
        var q1 = _context.FatoOportunidadeMetrica.AsNoTracking()
            .Where(f => !f.Excluido)
            .Select(f => f.OrigemId);
        var q2 = _context.FatoLeadAgregado.AsNoTracking()
            .Where(f => !f.Excluido)
            .Select(f => f.OrigemId);
        var q3 = _context.FatoEventoAgregado.AsNoTracking()
            .Where(f => !f.Excluido)
            .Select(f => f.OrigemId);
        var ids = await q1.Union(q2).Union(q3).Distinct().ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public async Task<HashSet<int>> ObterIdsDimensaoStatusLeadReferenciadosEmFatosAsync(CancellationToken cancellationToken = default)
    {
        var q1 = _context.FatoOportunidadeMetrica.AsNoTracking()
            .Where(f => !f.Excluido && f.StatusLeadId != null)
            .Select(f => f.StatusLeadId!.Value);
        var q2 = _context.FatoLeadAgregado.AsNoTracking()
            .Where(f => !f.Excluido && f.StatusAtualId != null)
            .Select(f => f.StatusAtualId!.Value);
        var q3 = _context.FatoEventoAgregado.AsNoTracking()
            .Where(f => !f.Excluido && f.StatusAtualId != null)
            .Select(f => f.StatusAtualId!.Value);
        var ids = await q1.Union(q2).Union(q3).Distinct().ToListAsync(cancellationToken);
        return ids.ToHashSet();
    }

    public async Task<IReadOnlyDictionary<DateTime, DimensaoTempo>> ObterDimensoesTempoPorDatasAsync(
        IReadOnlyCollection<DateTime> datas, CancellationToken cancellationToken = default)
    {
        var distinct = datas
            .Select(d => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0))
            .Distinct()
            .ToList();
        if (distinct.Count == 0)
            return new Dictionary<DateTime, DimensaoTempo>();

        var rows = await _context.DimensaoTempo
            .AsNoTracking()
            .Where(d => !d.Excluido && distinct.Any(x =>
                x.Year == d.Ano && x.Month == d.Mes && x.Day == d.Dia && x.Hour == d.Hora))
            .ToListAsync(cancellationToken);

        var dict = new Dictionary<DateTime, DimensaoTempo>();
        foreach (var r in rows)
        {
            var key = new DateTime(r.Ano, r.Mes, r.Dia, r.Hora, 0, 0);
            dict[key] = r;
        }

        return dict;
    }

    public async Task<IReadOnlyDictionary<int, DimensaoEmpresa>> ObterDimensoesEmpresaPorOrigemIdsAsync(
        IReadOnlyCollection<int> empresaOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = empresaOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoEmpresa>();

        var rows = await _context.DimensaoEmpresa
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.EmpresaOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.EmpresaOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = origemOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoOrigem>();

        var rows = await _context.DimensaoOrigem
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.OrigemOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.OrigemOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoOrigem>> ObterDimensoesOrigemPorOrigemIdsIncluindoExcluidasAsync(
        IReadOnlyCollection<int> origemOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = origemOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoOrigem>();

        var rows = await _context.DimensaoOrigem
            .AsNoTracking()
            .Where(d => ids.Contains(d.OrigemOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.OrigemOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoEquipe>> ObterDimensoesEquipePorOrigemIdsAsync(
        IReadOnlyCollection<int> equipeOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = equipeOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoEquipe>();

        var rows = await _context.DimensaoEquipe
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.EquipeOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.EquipeOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoVendedor>> ObterDimensoesVendedorPorOrigemIdsAsync(
        IReadOnlyCollection<int> usuarioOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = usuarioOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoVendedor>();

        var rows = await _context.DimensaoVendedor
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.UsuarioOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.UsuarioOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoStatusLead>> ObterDimensoesStatusLeadPorOrigemIdsAsync(
        IReadOnlyCollection<int> statusOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = statusOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoStatusLead>();

        var rows = await _context.DimensaoStatusLead
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.StatusOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.StatusOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoCampanha>> ObterDimensoesCampanhaPorOrigemIdsAsync(
        IReadOnlyCollection<int> campanhaOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = campanhaOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoCampanha>();

        var rows = await _context.DimensaoCampanha
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.CampanhaOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.CampanhaOrigemId, x => x);
    }

    public async Task<IReadOnlyDictionary<int, DimensaoEtapaFunil>> ObterDimensoesEtapaFunilPorOrigemIdsAsync(
        IReadOnlyCollection<int> etapaOrigemIds, CancellationToken cancellationToken = default)
    {
        var ids = etapaOrigemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DimensaoEtapaFunil>();

        var rows = await _context.DimensaoEtapaFunil
            .AsNoTracking()
            .Where(d => !d.Excluido && ids.Contains(d.EtapaOrigemId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.EtapaOrigemId, x => x);
    }
}
