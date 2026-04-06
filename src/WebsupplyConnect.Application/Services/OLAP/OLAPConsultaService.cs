using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.DTOs.Dashboard;
using WebsupplyConnect.Application.Helpers;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.OLAP;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.OLAP.Controle;
using WebsupplyConnect.Domain.Interfaces.OLAP.Fatos;
using WebsupplyConnect.Domain.Interfaces.Produto;

namespace WebsupplyConnect.Application.Services.OLAP;

public partial class OLAPConsultaService : IOLAPConsultaService
{
    private readonly IFatoLeadAgregadoRepository _fatoLeadRepository;
    private readonly IFatoOportunidadeMetricaRepository _fatoOportunidadeRepository;
    private readonly IFatoEventoAgregadoRepository _fatoEventoRepository;
    private readonly IDimensaoOlapReadService _dimensoesService;
    private readonly ILeadReaderService _leadReaderService;
    private readonly IConversaReaderService _conversaReaderService;
    private readonly IOportunidadeReaderService _oportunidadeReaderService;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IETLControleProcessamentoRepository _controleRepository;
    private readonly ETLConfig _config;

    public OLAPConsultaService(
        IFatoLeadAgregadoRepository fatoLeadRepository,
        IFatoOportunidadeMetricaRepository fatoOportunidadeRepository,
        IFatoEventoAgregadoRepository fatoEventoRepository,
        IDimensaoOlapReadService dimensoesService,
        ILeadReaderService leadReaderService,
        IConversaReaderService conversaReaderService,
        IOportunidadeReaderService oportunidadeReaderService,
        IProdutoRepository produtoRepository,
        IETLControleProcessamentoRepository controleRepository,
        IOptions<ETLConfig> config)
    {
        _fatoLeadRepository = fatoLeadRepository;
        _fatoOportunidadeRepository = fatoOportunidadeRepository;
        _fatoEventoRepository = fatoEventoRepository;
        _dimensoesService = dimensoesService;
        _leadReaderService = leadReaderService;
        _conversaReaderService = conversaReaderService;
        _oportunidadeReaderService = oportunidadeReaderService;
        _produtoRepository = produtoRepository;
        _controleRepository = controleRepository;
        _config = config.Value;
    }

    public async Task<DashboardKPIDTO> ObterKPIsAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var fatosOportunidade = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);

        var leadsUnicos = fatosLead.GroupBy(f => f.LeadId).Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();
        var totalLeads = leadsUnicos.Count;
        var leadsConvertidos = leadsUnicos.Count(f => f.EhConvertido);
        var oportunidadesAbertas = fatosOportunidade.Count(f => !f.EhGanha && !f.EhPerdida);
        var oportunidadesGanhas = fatosOportunidade.Sum(f => f.EhGanha ? 1 : 0);
        var oportunidadesPerdidas = fatosOportunidade.Sum(f => f.EhPerdida ? 1 : 0);
        var valorPipeline = fatosOportunidade.Where(f => !f.EhGanha && !f.EhPerdida).Sum(f => f.ValorEstimado);
        var valorGanho = fatosOportunidade.Where(f => f.EhGanha).Sum(f => f.ValorFinal ?? f.ValorEstimado);
        var tempoMedio = leadsUnicos.Where(f => f.TempoMedioRespostaMinutos.HasValue && f.TempoMedioRespostaMinutos > 0)
            .Select(f => f.TempoMedioRespostaMinutos!.Value).DefaultIfEmpty(0).Average();

        return new DashboardKPIDTO
        {
            TotalLeads = totalLeads,
            LeadsConvertidos = leadsConvertidos,
            OportunidadesAbertas = oportunidadesAbertas,
            OportunidadesGanhas = oportunidadesGanhas,
            OportunidadesPerdidas = oportunidadesPerdidas,
            ValorTotalPipeline = valorPipeline,
            ValorTotalGanho = valorGanho,
            TaxaConversao = totalLeads > 0 ? (decimal)leadsConvertidos / totalLeads * 100 : 0,
            TempoMedioRespostaMinutos = (decimal)tempoMedio
        };
    }

    public async Task<List<DashboardFunilOportunidadesPorEtapaDTO>> ObterFunilOportunidadesPorEtapaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatos = await ObterFatosOportunidadeFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var oportunidadesUnicas = fatos
            .GroupBy(f => f.OportunidadeId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .Where(f => f.DimensaoEtapaFunilId.HasValue)
            .ToList();

        var dimIds = oportunidadesUnicas.Select(f => f.DimensaoEtapaFunilId!.Value).Distinct().ToList();
        var todasDimensoes = await _dimensoesService.ObterDimensoesEtapaFunilNaoExcluidasAsync();
        var dimLookup = todasDimensoes.Where(d => dimIds.Contains(d.Id)).ToDictionary(d => d.Id, d => d);
        var nomesFunilPorDimId = await _dimensoesService.ObterNomesFunilPorDimensaoFunilIdsAsync(
            dimLookup.Values.Select(d => d.FunilDimensaoId).Distinct(), cancellationToken: default);

        var grupos = oportunidadesUnicas.GroupBy(f => f.DimensaoEtapaFunilId!.Value);
        var resultado = new List<DashboardFunilOportunidadesPorEtapaDTO>();
        foreach (var g in grupos)
        {
            if (!dimLookup.TryGetValue(g.Key, out var dim))
                continue;

            var itens = g.ToList();
            var leadsDistintos = itens.Select(f => f.LeadId).Distinct().Count();
            var valorPipeline = itens.Where(f => !f.EhGanha && !f.EhPerdida).Sum(f => f.ValorEstimado);
            resultado.Add(new DashboardFunilOportunidadesPorEtapaDTO
            {
                EtapaId = dim.EtapaOrigemId,
                FunilId = dim.FunilOrigemId,
                NomeEtapa = dim.Nome,
                NomeFunil = nomesFunilPorDimId.GetValueOrDefault(dim.FunilDimensaoId) ?? string.Empty,
                Ordem = dim.Ordem,
                Cor = dim.Cor,
                QuantidadeOportunidades = itens.Count,
                QuantidadeLeadsDistintos = leadsDistintos,
                ValorPipeline = valorPipeline
            });
        }

        return resultado.OrderBy(x => x.Ordem).ThenBy(x => x.NomeEtapa).ToList();
    }

    public async Task<List<DashboardLeadsPorStatusDTO>> ObterLeadsPorStatusAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsUnicos = fatosLead.GroupBy(f => f.LeadId).Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();

        var grupos = leadsUnicos
            .Where(f => f.StatusAtualId.HasValue)
            .GroupBy(f => f.StatusAtualId!.Value)
            .ToList();

        var statusIds = grupos.Select(g => g.Key).Distinct().ToList();
        var dimensoesStatus = await ObterDimensoesStatusLookupAsync(statusIds);

        var total = leadsUnicos.Count;
        var resultado = grupos.Select(g =>
        {
            var dim = dimensoesStatus.GetValueOrDefault(g.Key);
            return new DashboardLeadsPorStatusDTO
            {
                StatusId = dim?.StatusOrigemId ?? g.Key,
                NomeStatus = dim?.Nome ?? $"Status {g.Key}",
                Quantidade = g.Count(),
                Percentual = total > 0 ? (decimal)g.Count() / total * 100 : 0
            };
        }).OrderByDescending(x => x.Quantidade).ToList();

        return resultado;
    }

    public async Task<List<DashboardLeadsPorOrigemDTO>> ObterLeadsPorOrigemAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsUnicos = fatosLead.GroupBy(f => f.LeadId).Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First()).ToList();

        var grupos = leadsUnicos.GroupBy(f => f.OrigemId).ToList();
        var dimensoesOrigem = await ObterDimensoesOrigemLookupAsync(grupos.Select(g => g.Key).ToList());

        var total = leadsUnicos.Count;
        return grupos.Select(g =>
        {
            var dim = dimensoesOrigem.GetValueOrDefault(g.Key);
            return new DashboardLeadsPorOrigemDTO
            {
                OrigemId = dim?.OrigemOrigemId ?? g.Key,
                NomeOrigem = dim?.Nome ?? $"Origem {g.Key}",
                Quantidade = g.Count(),
                Percentual = total > 0 ? (decimal)g.Count() / total * 100 : 0
            };
        }).OrderByDescending(x => x.Quantidade).ToList();
    }

    public async Task<List<DashboardLeadsPorCampanhaDTO>> ObterLeadsPorCampanhaAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsUnicos = fatosLead.Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.CampanhaId!.Value)
            .ToList();

        if (leadsUnicos.Count == 0) return new List<DashboardLeadsPorCampanhaDTO>();

        var campanhaIds = leadsUnicos.Select(g => g.Key).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaIds);

        var linhas = leadsUnicos.Select(g =>
        {
            var dim = dimensoesCampanha.GetValueOrDefault(g.Key);
            var nome = ResolverNomeCampanhaParaAgrupamento(dim, g.Key);
            return new DashboardLeadsPorCampanhaDTO
            {
                NomeCampanha = nome,
                Quantidade = g.Count(),
                Percentual = 0
            };
        }).ToList();

        linhas = linhas
            .GroupBy(r => r.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new DashboardLeadsPorCampanhaDTO
            {
                NomeCampanha = grupo.Key,
                Quantidade = grupo.Sum(x => x.Quantidade),
                Percentual = 0
            }).ToList();

        var totalGeral = linhas.Sum(x => x.Quantidade);
        foreach (var r in linhas)
            r.Percentual = totalGeral > 0 ? (decimal)r.Quantidade / totalGeral * 100 : 0;

        return linhas.OrderByDescending(x => x.Quantidade).ToList();
    }

    public async Task<List<DashboardCampanhaDisponivelDTO>> ObterCampanhasDisponiveisAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var leadsPorCampanha = fatosLead
            .Where(f => f.CampanhaId.HasValue)
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First())
            .GroupBy(f => f.CampanhaId!.Value)
            .ToList();

        if (leadsPorCampanha.Count == 0)
            return [];

        var campanhaDimensaoIds = leadsPorCampanha.Select(g => g.Key).Distinct().ToList();
        var dimensoesCampanha = await ObterDimensoesCampanhaLookupAsync(campanhaDimensaoIds);

        var linhas = leadsPorCampanha
            .Select(g =>
            {
                var dim = dimensoesCampanha.GetValueOrDefault(g.Key);
                return new DashboardCampanhaDisponivelDTO
                {
                    NomeCampanha = ResolverNomeCampanhaParaAgrupamento(dim, g.Key),
                    QuantidadeLeads = g.Count()
                };
            })
            .ToList();

        linhas = linhas
            .GroupBy(x => x.NomeCampanha, StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new DashboardCampanhaDisponivelDTO
            {
                NomeCampanha = grupo.Key,
                QuantidadeLeads = grupo.Sum(x => x.QuantidadeLeads)
            })
            .ToList();

        return linhas
            .OrderByDescending(x => x.QuantidadeLeads)
            .ThenBy(x => x.NomeCampanha)
            .ToList();
    }

    public async Task<List<DashboardEvolucaoLeadsStatusDTO>> ObterEvolucaoLeadsPorStatusAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatosLead = await ObterFatosLeadFiltradosPorDataUltimoEventoAsync(dataInicio, dataFim, empresaDimensaoId, filtros);
        var dimensoesTempo = fatosLead.Select(f => (f.DataUltimoEvento ?? f.DataReferencia).Date).Distinct().ToList();

        var resultados = new List<DashboardEvolucaoLeadsStatusDTO>();
        var statusIds = fatosLead.Where(f => f.StatusAtualId.HasValue).Select(f => f.StatusAtualId!.Value).Distinct().ToList();
        var dimensoesStatus = await ObterDimensoesStatusLookupAsync(statusIds);

        foreach (var data in dimensoesTempo.OrderBy(d => d))
        {
            var fatosDoDia = fatosLead.Where(f => (f.DataUltimoEvento ?? f.DataReferencia).Date == data)
                .GroupBy(f => f.LeadId)
                .Select(g => g.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).First());

            foreach (var grupo in fatosDoDia.Where(f => f.StatusAtualId.HasValue).GroupBy(f => f.StatusAtualId!.Value))
            {
                var dim = dimensoesStatus.GetValueOrDefault(grupo.Key);
                resultados.Add(new DashboardEvolucaoLeadsStatusDTO
                {
                    Data = data,
                    StatusId = dim?.StatusOrigemId ?? grupo.Key,
                    NomeStatus = dim?.Nome ?? $"Status {grupo.Key}",
                    Quantidade = grupo.Count()
                });
            }
        }

        return resultados.OrderBy(r => r.Data).ThenBy(r => r.NomeStatus).ToList();
    }

    public async Task<List<DashboardLeadsCriadosPorHorarioDTO>> ObterLeadsCriadosPorHorarioAsync(FiltrosDashboardDTO filtros)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var fatos = await _fatoLeadRepository.ObterPorPeriodoAsync(dataInicio, dataFim, empresaDimensaoId);
        var fatosFiltrados = await AplicarFiltrosAdicionaisLeadAsync(fatos, filtros);

        var leadsUnicos = fatosFiltrados
            .GroupBy(f => f.LeadId)
            .Select(g => g.OrderByDescending(f => f.DataReferencia).First())
            .ToList();

        var total = leadsUnicos.Count;
        var porHora = leadsUnicos
            .GroupBy(f => f.DataReferencia.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        var resultado = new List<DashboardLeadsCriadosPorHorarioDTO>();
        for (var h = 0; h < 24; h++)
        {
            var qtd = porHora.GetValueOrDefault(h, 0);
            resultado.Add(new DashboardLeadsCriadosPorHorarioDTO
            {
                Hora = h,
                Quantidade = qtd,
                Percentual = total > 0 ? (decimal)qtd / total * 100 : 0
            });
        }
        return resultado;
    }

    private static readonly HashSet<string> CamposOrdenaveisListagemLeads = new(StringComparer.OrdinalIgnoreCase)
    {
        "dataUltimoEvento", "nome", "nomeOrigem", "nomeCampanha", "nomeStatus", "nomeResponsavel",
        "totalOportunidades", "produtoInteresse", "mensagensNaoLidas", "tempoMedioRespostaMinutos"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingVendedores = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomeresponsavel", "nomeequipe", "nomeempresa", "grupoempresaid",
        "leadsrecebidos", "leadscomoportunidade", "leadsconvertidos", "leadsperdidos",
        "oportunidadesabertas", "oportunidadesganhas", "oportunidadesperdidas",
        "taxaconversaoleads", "tempomediorespostaminutos", "indicadorperformance"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingLeadsEmpresa = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomeempresa", "grupoempresaid", "leadsrecebidos"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingLeadsNomeCampanha = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomecampanha", "leadsrecebidos"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingLeadsNomeCampanhaEmpresa = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomecampanha", "nomeempresa", "grupoempresaid", "empresaid", "leadsrecebidos"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingOportunidadesEmpresa = new(StringComparer.OrdinalIgnoreCase)
    {
        "nomeempresa", "grupoempresaid", "oportunidadestotal", "oportunidadesabertas", "oportunidadesganhas", "oportunidadesperdidas"
    };

    private static readonly HashSet<string> CamposOrdenaveisRankingOportunidadesTipoProduto = new(StringComparer.OrdinalIgnoreCase)
    {
        "nometipointeresse", "nomeproduto", "oportunidadestotal", "oportunidadesabertas", "oportunidadesganhas", "oportunidadesperdidas"
    };

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterListagemLeadsAsync(FiltrosDashboardDTO filtros, int pagina, int tamanhoPagina)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var empresaDimensaoId = await ResolverEmpresaDimensaoIdAsync(filtros.EmpresaId);

        var filtrosDimensao = await ObterFiltrosDimensaoIdsAsync(filtros);
        if (filtrosDimensao.SemResultados)
        {
            return new PagedResultDTO<DashboardListagemLeadsDTO>
            {
                Itens = new List<DashboardListagemLeadsDTO>(),
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = 0,
                TotalPaginas = 0
            };
        }

        var filtroConsulta = filtrosDimensao.ToFatoLeadConsultaFiltro(dataInicio, dataFim, empresaDimensaoId);
        var fatosUnicosPorLead = await _fatoLeadRepository.ObterUnicosPorLeadParaListagemAsync(filtroConsulta);

        var botIds = await ObterLeadIdsAtribuidosABotAsync(fatosUnicosPorLead.Select(f => f.LeadId));
        if (botIds.Count > 0)
            fatosUnicosPorLead = fatosUnicosPorLead.Where(f => !botIds.Contains(f.LeadId)).ToList();

        var criteriosOrdenacao = ObterCriteriosOrdenacaoValidos(
            filtros.OrdenarPor,
            filtros.DirecaoOrdenacao);

        fatosUnicosPorLead = await AplicarOrdenacaoListagemLeadsAsync(fatosUnicosPorLead, criteriosOrdenacao);

        // Paginação sobre os fatos OLAP
        var totalItens = fatosUnicosPorLead.Count;
        var totalPaginas = tamanhoPagina > 0 ? (int)Math.Ceiling((double)totalItens / tamanhoPagina) : 0;
        var fatosPaginados = fatosUnicosPorLead
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        if (fatosPaginados.Count == 0)
        {
            return new PagedResultDTO<DashboardListagemLeadsDTO>
            {
                Itens = new List<DashboardListagemLeadsDTO>(),
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens,
                TotalPaginas = totalPaginas
            };
        }

        // Carregar lookups de dimensões em batch para performance
        var vendedorIds = fatosPaginados.Where(f => f.VendedorId.HasValue).Select(f => f.VendedorId!.Value).Distinct().ToList();
        var campanhaIds = fatosPaginados.Where(f => f.CampanhaId.HasValue).Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var statusIds = fatosPaginados.Where(f => f.StatusAtualId.HasValue).Select(f => f.StatusAtualId!.Value).Distinct().ToList();
        var origemIds = fatosPaginados.Select(f => f.OrigemId).Distinct().ToList();

        var vendedorLookup = await ObterDimensoesVendedorLookupAsync(vendedorIds);
        var campanhaLookup = await ObterDimensoesCampanhaLookupAsync(campanhaIds);
        var statusLookup = await ObterDimensoesStatusLookupAsync(statusIds);
        var origemLookup = await ObterDimensoesOrigemLookupAsync(origemIds);
        var empresaDimIds = fatosPaginados.Select(f => f.EmpresaId).Distinct().ToList();
        var empresaLookup = await ObterDimensoesEmpresaLookupAsync(empresaDimIds);

        // Buscar dados básicos dos leads (nome, email, telefone) em batch
        var leadIds = fatosPaginados.Select(f => f.LeadId).Distinct().ToList();
        var leads = await _leadReaderService.ObterLeadsPorIdsAsync(leadIds, includeDeleted: true);
        var leadLookup = leads.ToDictionary(l => l.Id);

        var conversaAtivaLookup = await _conversaReaderService.ObterConversaAtivaIdsPorLeadIdsAsync(leadIds);
        var conversaIds = fatosPaginados
            .Select(f => conversaAtivaLookup.TryGetValue(f.LeadId, out var cid) ? cid : (int?)null)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .Distinct()
            .ToList();
        var contextosPorConversa = await _conversaReaderService.GetContextosByIdsAsync(conversaIds);

        // Montar DTOs usando exclusivamente dados OLAP + dados básicos do lead
        var dtos = new List<DashboardListagemLeadsDTO>();
        foreach (var fato in fatosPaginados)
        {
            var lead = leadLookup.GetValueOrDefault(fato.LeadId);

            var nomeStatus = fato.StatusAtualId.HasValue && statusLookup.TryGetValue(fato.StatusAtualId.Value, out var statusDim)
                ? statusDim?.Nome ?? "" : "";
            var nomeOrigem = origemLookup.TryGetValue(fato.OrigemId, out var origemDim)
                ? origemDim?.Nome ?? "" : "";
            var nomeResponsavel = fato.VendedorId.HasValue && vendedorLookup.TryGetValue(fato.VendedorId.Value, out var vendedorDim)
                ? vendedorDim?.Nome : null;
            var nomeCampanha = fato.CampanhaId.HasValue && campanhaLookup.TryGetValue(fato.CampanhaId.Value, out var campanhaDim)
                ? ResolverNomeCampanhaParaAgrupamento(campanhaDim, fato.CampanhaId.Value)
                : null;
            var conversaId = conversaAtivaLookup.TryGetValue(fato.LeadId, out var conversaId3) ? conversaId3 : (int?)null;
            var trocaDeContato = conversaId.HasValue && contextosPorConversa.TryGetValue(conversaId.Value, out var ctx)
                ? ctx.TrocaDeContato
                : false;
            var empresaIdListagem = lead?.EmpresaId
                ?? empresaLookup.GetValueOrDefault(fato.EmpresaId)?.EmpresaOrigemId
                ?? filtros.EmpresaId
                ?? 0;

            dtos.Add(new DashboardListagemLeadsDTO
            {
                LeadId = fato.LeadId,
                EmpresaId = empresaIdListagem,
                Nome = lead?.Nome ?? "",
                Email = lead?.Email,
                Telefone = lead?.Telefone,
                NomeStatus = nomeStatus,
                NomeOrigem = nomeOrigem,
                NomeResponsavel = nomeResponsavel,
                NomeResponsavelResumido = nomeResponsavel != null ? NomeVendedorHelper.AbreviarNome(nomeResponsavel) : null,
                NomeCampanha = nomeCampanha,
                ProdutoInteresse = fato.ProdutoInteresse,
                TotalOportunidades = fato.TotalOportunidades,
                MensagensNaoLidas = fato.ConversasNaoLidas,
                TempoMedioRespostaMinutos = fato.TempoMedioRespostaMinutos > 0 ? fato.TempoMedioRespostaMinutos : null,
                DataUltimoEvento = fato.DataUltimoEvento,
                DataCriacao = lead?.DataCriacao ?? fato.DataReferencia,
                EhConvertido = fato.EhConvertido,
                ConversaAtivaId = conversaId,
                TrocaDeContato = trocaDeContato
            });
        }

        return new PagedResultDTO<DashboardListagemLeadsDTO>
        {
            Itens = dtos,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    private static List<(string Campo, bool DirecaoAsc)> ObterCriteriosOrdenacaoValidos(
        string? ordenarPor,
        string? direcaoOrdenacao)
    {
        var campos = (ordenarPor ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var direcoes = (direcaoOrdenacao ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var criterios = new List<(string Campo, bool DirecaoAsc)>();

        for (var i = 0; i < campos.Length; i++)
        {
            var campo = campos[i];
            if (!CamposOrdenaveisListagemLeads.Contains(campo))
                continue;

            var direcaoAsc = i >= direcoes.Length ||
                             !string.Equals(direcoes[i], "desc", StringComparison.OrdinalIgnoreCase);
            criterios.Add((campo, direcaoAsc));
        }

        if (criterios.Count == 0)
        {
            var direcaoPadraoAsc = direcoes.Length == 0 ||
                                   !string.Equals(direcoes[0], "desc", StringComparison.OrdinalIgnoreCase);
            criterios.Add(("dataUltimoEvento", direcaoPadraoAsc));
        }

        return criterios;
    }

    private static List<(string Campo, bool DirecaoAsc)> ObterCriteriosOrdenacaoRankingVendedores(
        string? ordenarPor,
        string? direcaoOrdenacao)
    {
        var campos = (ordenarPor ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var direcoes = (direcaoOrdenacao ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var criterios = new List<(string Campo, bool DirecaoAsc)>();
        for (var i = 0; i < campos.Length; i++)
        {
            var campo = campos[i];
            if (!CamposOrdenaveisRankingVendedores.Contains(campo))
                continue;

            var direcaoAsc = i >= direcoes.Length ||
                             !string.Equals(direcoes[i], "desc", StringComparison.OrdinalIgnoreCase);
            criterios.Add((campo, direcaoAsc));
        }

        if (criterios.Count == 0)
        {
            var direcaoPadraoAsc = direcoes.Length > 0 &&
                                   string.Equals(direcoes[0], "asc", StringComparison.OrdinalIgnoreCase);
            criterios.Add(("leadsRecebidos", direcaoPadraoAsc));
        }

        return criterios;
    }

    private static List<DashboardRankingVendedorDTO> AplicarOrdenacaoRankingVendedores(
        List<DashboardRankingVendedorDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        if (criterios.Count == 0)
            return itens.OrderByDescending(i => i.LeadsRecebidos).ThenBy(i => i.NomeResponsavel).ToList();

        IOrderedEnumerable<DashboardRankingVendedorDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nomeresponsavel" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeResponsavel) : itens.OrderByDescending(i => i.NomeResponsavel))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeResponsavel) : ordenados.ThenByDescending(i => i.NomeResponsavel)),
                "nomeequipe" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeEquipe ?? string.Empty) : itens.OrderByDescending(i => i.NomeEquipe ?? string.Empty))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeEquipe ?? string.Empty) : ordenados.ThenByDescending(i => i.NomeEquipe ?? string.Empty)),
                "nomeempresa" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeEmpresa ?? string.Empty) : itens.OrderByDescending(i => i.NomeEmpresa ?? string.Empty))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeEmpresa ?? string.Empty) : ordenados.ThenByDescending(i => i.NomeEmpresa ?? string.Empty)),
                "grupoempresaid" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.GrupoEmpresaId ?? 0) : itens.OrderByDescending(i => i.GrupoEmpresaId ?? 0))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.GrupoEmpresaId ?? 0) : ordenados.ThenByDescending(i => i.GrupoEmpresaId ?? 0)),
                "leadsrecebidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsRecebidos) : itens.OrderByDescending(i => i.LeadsRecebidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsRecebidos) : ordenados.ThenByDescending(i => i.LeadsRecebidos)),
                "leadscomoportunidade" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsComOportunidade) : itens.OrderByDescending(i => i.LeadsComOportunidade))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsComOportunidade) : ordenados.ThenByDescending(i => i.LeadsComOportunidade)),
                "leadsconvertidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsConvertidos) : itens.OrderByDescending(i => i.LeadsConvertidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsConvertidos) : ordenados.ThenByDescending(i => i.LeadsConvertidos)),
                "leadsperdidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsPerdidos) : itens.OrderByDescending(i => i.LeadsPerdidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsPerdidos) : ordenados.ThenByDescending(i => i.LeadsPerdidos)),
                "oportunidadesabertas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesAbertas) : itens.OrderByDescending(i => i.OportunidadesAbertas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesAbertas) : ordenados.ThenByDescending(i => i.OportunidadesAbertas)),
                "oportunidadesganhas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesGanhas) : itens.OrderByDescending(i => i.OportunidadesGanhas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesGanhas) : ordenados.ThenByDescending(i => i.OportunidadesGanhas)),
                "oportunidadesperdidas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesPerdidas) : itens.OrderByDescending(i => i.OportunidadesPerdidas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesPerdidas) : ordenados.ThenByDescending(i => i.OportunidadesPerdidas)),
                "taxaconversaoleads" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.TaxaConversaoLeads) : itens.OrderByDescending(i => i.TaxaConversaoLeads))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.TaxaConversaoLeads) : ordenados.ThenByDescending(i => i.TaxaConversaoLeads)),
                "tempomediorespostaminutos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.TempoMedioRespostaMinutos) : itens.OrderByDescending(i => i.TempoMedioRespostaMinutos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.TempoMedioRespostaMinutos) : ordenados.ThenByDescending(i => i.TempoMedioRespostaMinutos)),
                "indicadorperformance" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.IndicadorPerformance) : itens.OrderByDescending(i => i.IndicadorPerformance))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.IndicadorPerformance) : ordenados.ThenByDescending(i => i.IndicadorPerformance)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.LeadsRecebidos).ThenBy(i => i.NomeResponsavel)).ToList();
    }

    private static List<(string Campo, bool DirecaoAsc)> ObterCriteriosOrdenacaoSimples(
        string? ordenarPor,
        string? direcaoOrdenacao,
        HashSet<string> camposPermitidos,
        string campoPadrao,
        bool direcaoPadraoAsc)
    {
        var campos = (ordenarPor ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var direcoes = (direcaoOrdenacao ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var criterios = new List<(string Campo, bool DirecaoAsc)>();

        for (var i = 0; i < campos.Length; i++)
        {
            var campo = campos[i];
            if (!camposPermitidos.Contains(campo))
                continue;

            var direcaoAsc = i >= direcoes.Length ||
                             !string.Equals(direcoes[i], "desc", StringComparison.OrdinalIgnoreCase);
            criterios.Add((campo, direcaoAsc));
        }

        if (criterios.Count == 0)
            criterios.Add((campoPadrao, direcaoPadraoAsc));

        return criterios;
    }

    private static List<DashboardRankingLeadsEmpresaDTO> AplicarOrdenacaoRankingLeadsEmpresa(
        List<DashboardRankingLeadsEmpresaDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        IOrderedEnumerable<DashboardRankingLeadsEmpresaDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nomeempresa" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeEmpresa) : itens.OrderByDescending(i => i.NomeEmpresa))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeEmpresa) : ordenados.ThenByDescending(i => i.NomeEmpresa)),
                "grupoempresaid" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.GrupoEmpresaId) : itens.OrderByDescending(i => i.GrupoEmpresaId))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.GrupoEmpresaId) : ordenados.ThenByDescending(i => i.GrupoEmpresaId)),
                "leadsrecebidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsRecebidos) : itens.OrderByDescending(i => i.LeadsRecebidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsRecebidos) : ordenados.ThenByDescending(i => i.LeadsRecebidos)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.LeadsRecebidos).ThenBy(i => i.NomeEmpresa)).ToList();
    }

    private static List<DashboardRankingLeadsNomeCampanhaDTO> AplicarOrdenacaoRankingLeadsNomeCampanha(
        List<DashboardRankingLeadsNomeCampanhaDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        IOrderedEnumerable<DashboardRankingLeadsNomeCampanhaDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nomecampanha" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeCampanha) : itens.OrderByDescending(i => i.NomeCampanha))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeCampanha) : ordenados.ThenByDescending(i => i.NomeCampanha)),
                "leadsrecebidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsRecebidos) : itens.OrderByDescending(i => i.LeadsRecebidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsRecebidos) : ordenados.ThenByDescending(i => i.LeadsRecebidos)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.LeadsRecebidos).ThenBy(i => i.NomeCampanha)).ToList();
    }

    private static List<DashboardRankingLeadsNomeCampanhaEmpresaDTO> AplicarOrdenacaoRankingLeadsNomeCampanhaEmpresa(
        List<DashboardRankingLeadsNomeCampanhaEmpresaDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        IOrderedEnumerable<DashboardRankingLeadsNomeCampanhaEmpresaDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nomecampanha" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeCampanha) : itens.OrderByDescending(i => i.NomeCampanha))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeCampanha) : ordenados.ThenByDescending(i => i.NomeCampanha)),
                "nomeempresa" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeEmpresa) : itens.OrderByDescending(i => i.NomeEmpresa))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeEmpresa) : ordenados.ThenByDescending(i => i.NomeEmpresa)),
                "grupoempresaid" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.GrupoEmpresaId) : itens.OrderByDescending(i => i.GrupoEmpresaId))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.GrupoEmpresaId) : ordenados.ThenByDescending(i => i.GrupoEmpresaId)),
                "empresaid" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.EmpresaId) : itens.OrderByDescending(i => i.EmpresaId))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.EmpresaId) : ordenados.ThenByDescending(i => i.EmpresaId)),
                "leadsrecebidos" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.LeadsRecebidos) : itens.OrderByDescending(i => i.LeadsRecebidos))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.LeadsRecebidos) : ordenados.ThenByDescending(i => i.LeadsRecebidos)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.LeadsRecebidos).ThenBy(i => i.NomeEmpresa).ThenBy(i => i.NomeCampanha)).ToList();
    }

    private static List<DashboardRankingOportunidadesEmpresaDTO> AplicarOrdenacaoRankingOportunidadesEmpresa(
        List<DashboardRankingOportunidadesEmpresaDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        IOrderedEnumerable<DashboardRankingOportunidadesEmpresaDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nomeempresa" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeEmpresa) : itens.OrderByDescending(i => i.NomeEmpresa))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeEmpresa) : ordenados.ThenByDescending(i => i.NomeEmpresa)),
                "grupoempresaid" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.GrupoEmpresaId) : itens.OrderByDescending(i => i.GrupoEmpresaId))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.GrupoEmpresaId) : ordenados.ThenByDescending(i => i.GrupoEmpresaId)),
                "oportunidadestotal" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesTotal) : itens.OrderByDescending(i => i.OportunidadesTotal))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesTotal) : ordenados.ThenByDescending(i => i.OportunidadesTotal)),
                "oportunidadesabertas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesAbertas) : itens.OrderByDescending(i => i.OportunidadesAbertas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesAbertas) : ordenados.ThenByDescending(i => i.OportunidadesAbertas)),
                "oportunidadesganhas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesGanhas) : itens.OrderByDescending(i => i.OportunidadesGanhas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesGanhas) : ordenados.ThenByDescending(i => i.OportunidadesGanhas)),
                "oportunidadesperdidas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesPerdidas) : itens.OrderByDescending(i => i.OportunidadesPerdidas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesPerdidas) : ordenados.ThenByDescending(i => i.OportunidadesPerdidas)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.OportunidadesTotal).ThenBy(i => i.NomeEmpresa)).ToList();
    }

    private static List<DashboardRankingOportunidadesTipoInteresseProdutoDTO> AplicarOrdenacaoRankingTipoInteresseProduto(
        List<DashboardRankingOportunidadesTipoInteresseProdutoDTO> itens,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        IOrderedEnumerable<DashboardRankingOportunidadesTipoInteresseProdutoDTO>? ordenados = null;
        foreach (var criterio in criterios)
        {
            ordenados = criterio.Campo.ToLowerInvariant() switch
            {
                "nometipointeresse" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeTipoInteresse) : itens.OrderByDescending(i => i.NomeTipoInteresse))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeTipoInteresse) : ordenados.ThenByDescending(i => i.NomeTipoInteresse)),
                "nomeproduto" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.NomeProduto) : itens.OrderByDescending(i => i.NomeProduto))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.NomeProduto) : ordenados.ThenByDescending(i => i.NomeProduto)),
                "oportunidadestotal" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesTotal) : itens.OrderByDescending(i => i.OportunidadesTotal))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesTotal) : ordenados.ThenByDescending(i => i.OportunidadesTotal)),
                "oportunidadesabertas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesAbertas) : itens.OrderByDescending(i => i.OportunidadesAbertas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesAbertas) : ordenados.ThenByDescending(i => i.OportunidadesAbertas)),
                "oportunidadesganhas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesGanhas) : itens.OrderByDescending(i => i.OportunidadesGanhas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesGanhas) : ordenados.ThenByDescending(i => i.OportunidadesGanhas)),
                "oportunidadesperdidas" => ordenados == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(i => i.OportunidadesPerdidas) : itens.OrderByDescending(i => i.OportunidadesPerdidas))
                    : (criterio.DirecaoAsc ? ordenados.ThenBy(i => i.OportunidadesPerdidas) : ordenados.ThenByDescending(i => i.OportunidadesPerdidas)),
                _ => ordenados
            };
        }

        return (ordenados ?? itens.OrderByDescending(i => i.OportunidadesTotal).ThenBy(i => i.NomeTipoInteresse).ThenBy(i => i.NomeProduto)).ToList();
    }

    private static PagedResultDTO<T> PaginarResultado<T>(List<T> itens, int pagina, int tamanhoPagina)
    {
        var totalItens = itens.Count;
        var totalPaginas = tamanhoPagina > 0 ? (int)Math.Ceiling((double)totalItens / tamanhoPagina) : 0;
        var itensPagina = itens
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        return new PagedResultDTO<T>
        {
            Itens = itensPagina,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    private async Task<List<FatoLeadAgregado>> AplicarOrdenacaoListagemLeadsAsync(
        List<FatoLeadAgregado> fatos,
        List<(string Campo, bool DirecaoAsc)> criterios)
    {
        if (criterios.Count == 0)
            return fatos.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).ToList();

        var camposQuePrecisamLookup = new[] { "nome", "nomeOrigem", "nomeCampanha", "nomeStatus", "nomeResponsavel" };
        var precisaLookup = criterios.Any(c => camposQuePrecisamLookup.Contains(c.Campo, StringComparer.OrdinalIgnoreCase));

        if (!precisaLookup)
        {
            IOrderedEnumerable<FatoLeadAgregado>? ordenados = null;
            foreach (var criterio in criterios)
            {
                ordenados = criterio.Campo.ToLowerInvariant() switch
                {
                    "dataultimoevento" => ordenados == null
                        ? (criterio.DirecaoAsc
                            ? fatos.OrderBy(f => f.DataUltimoEvento ?? f.DataReferencia)
                            : fatos.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia))
                        : (criterio.DirecaoAsc
                            ? ordenados.ThenBy(f => f.DataUltimoEvento ?? f.DataReferencia)
                            : ordenados.ThenByDescending(f => f.DataUltimoEvento ?? f.DataReferencia)),
                    "totaloportunidades" => ordenados == null
                        ? (criterio.DirecaoAsc
                            ? fatos.OrderBy(f => f.TotalOportunidades)
                            : fatos.OrderByDescending(f => f.TotalOportunidades))
                        : (criterio.DirecaoAsc
                            ? ordenados.ThenBy(f => f.TotalOportunidades)
                            : ordenados.ThenByDescending(f => f.TotalOportunidades)),
                    "produtointeresse" => ordenados == null
                        ? (criterio.DirecaoAsc
                            ? fatos.OrderBy(f => f.ProdutoInteresse ?? "")
                            : fatos.OrderByDescending(f => f.ProdutoInteresse ?? ""))
                        : (criterio.DirecaoAsc
                            ? ordenados.ThenBy(f => f.ProdutoInteresse ?? "")
                            : ordenados.ThenByDescending(f => f.ProdutoInteresse ?? "")),
                    "mensagensnaolidas" => ordenados == null
                        ? (criterio.DirecaoAsc
                            ? fatos.OrderBy(f => f.ConversasNaoLidas)
                            : fatos.OrderByDescending(f => f.ConversasNaoLidas))
                        : (criterio.DirecaoAsc
                            ? ordenados.ThenBy(f => f.ConversasNaoLidas)
                            : ordenados.ThenByDescending(f => f.ConversasNaoLidas)),
                    "tempomediorespostaminutos" => ordenados == null
                        ? (criterio.DirecaoAsc
                            ? fatos.OrderBy(f => f.TempoMedioRespostaMinutos ?? 0)
                            : fatos.OrderByDescending(f => f.TempoMedioRespostaMinutos ?? 0))
                        : (criterio.DirecaoAsc
                            ? ordenados.ThenBy(f => f.TempoMedioRespostaMinutos ?? 0)
                            : ordenados.ThenByDescending(f => f.TempoMedioRespostaMinutos ?? 0)),
                    _ => ordenados
                };
            }

            return ordenados?.ToList() ?? fatos.OrderByDescending(f => f.DataUltimoEvento ?? f.DataReferencia).ToList();
        }

        var leadIds = fatos.Select(f => f.LeadId).Distinct().ToList();
        var vendedorIds = fatos.Where(f => f.VendedorId.HasValue).Select(f => f.VendedorId!.Value).Distinct().ToList();
        var campanhaIds = fatos.Where(f => f.CampanhaId.HasValue).Select(f => f.CampanhaId!.Value).Distinct().ToList();
        var statusIds = fatos.Where(f => f.StatusAtualId.HasValue).Select(f => f.StatusAtualId!.Value).Distinct().ToList();
        var origemIds = fatos.Select(f => f.OrigemId).Distinct().ToList();

        var leads = await _leadReaderService.ObterLeadsPorIdsAsync(leadIds, includeDeleted: true);
        var leadLookup = leads.ToDictionary(l => l.Id);
        var vendedorLookup = await ObterDimensoesVendedorLookupAsync(vendedorIds);
        var campanhaLookup = await ObterDimensoesCampanhaLookupAsync(campanhaIds);
        var statusLookup = await ObterDimensoesStatusLookupAsync(statusIds);
        var origemLookup = await ObterDimensoesOrigemLookupAsync(origemIds);

        var itens = fatos.Select(f => new OrdenacaoListagemLeadsItem
        {
            Fato = f,
            Nome = leadLookup.TryGetValue(f.LeadId, out var l) ? (l.Nome ?? "") : "",
            NomeOrigem = origemLookup.TryGetValue(f.OrigemId, out var o) ? (o?.Nome ?? "") : "",
            NomeCampanha = f.CampanhaId.HasValue && campanhaLookup.TryGetValue(f.CampanhaId.Value, out var c)
                ? ResolverNomeCampanhaParaAgrupamento(c, f.CampanhaId.Value)
                : "",
            NomeStatus = f.StatusAtualId.HasValue && statusLookup.TryGetValue(f.StatusAtualId.Value, out var s) ? (s?.Nome ?? "") : "",
            NomeResponsavel = f.VendedorId.HasValue && vendedorLookup.TryGetValue(f.VendedorId.Value, out var v) ? (v?.Nome ?? "") : ""
        }).ToList();

        IOrderedEnumerable<OrdenacaoListagemLeadsItem>? ordenadosComLookup = null;
        foreach (var criterio in criterios)
        {
            ordenadosComLookup = criterio.Campo.ToLowerInvariant() switch
            {
                "nome" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(x => x.Nome) : itens.OrderByDescending(x => x.Nome))
                    : (criterio.DirecaoAsc ? ordenadosComLookup.ThenBy(x => x.Nome) : ordenadosComLookup.ThenByDescending(x => x.Nome)),
                "nomeorigem" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(x => x.NomeOrigem) : itens.OrderByDescending(x => x.NomeOrigem))
                    : (criterio.DirecaoAsc ? ordenadosComLookup.ThenBy(x => x.NomeOrigem) : ordenadosComLookup.ThenByDescending(x => x.NomeOrigem)),
                "nomecampanha" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(x => x.NomeCampanha) : itens.OrderByDescending(x => x.NomeCampanha))
                    : (criterio.DirecaoAsc ? ordenadosComLookup.ThenBy(x => x.NomeCampanha) : ordenadosComLookup.ThenByDescending(x => x.NomeCampanha)),
                "nomestatus" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(x => x.NomeStatus) : itens.OrderByDescending(x => x.NomeStatus))
                    : (criterio.DirecaoAsc ? ordenadosComLookup.ThenBy(x => x.NomeStatus) : ordenadosComLookup.ThenByDescending(x => x.NomeStatus)),
                "nomeresponsavel" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc ? itens.OrderBy(x => x.NomeResponsavel) : itens.OrderByDescending(x => x.NomeResponsavel))
                    : (criterio.DirecaoAsc ? ordenadosComLookup.ThenBy(x => x.NomeResponsavel) : ordenadosComLookup.ThenByDescending(x => x.NomeResponsavel)),
                "dataultimoevento" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc
                        ? itens.OrderBy(x => x.Fato.DataUltimoEvento ?? x.Fato.DataReferencia)
                        : itens.OrderByDescending(x => x.Fato.DataUltimoEvento ?? x.Fato.DataReferencia))
                    : (criterio.DirecaoAsc
                        ? ordenadosComLookup.ThenBy(x => x.Fato.DataUltimoEvento ?? x.Fato.DataReferencia)
                        : ordenadosComLookup.ThenByDescending(x => x.Fato.DataUltimoEvento ?? x.Fato.DataReferencia)),
                "totaloportunidades" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc
                        ? itens.OrderBy(x => x.Fato.TotalOportunidades)
                        : itens.OrderByDescending(x => x.Fato.TotalOportunidades))
                    : (criterio.DirecaoAsc
                        ? ordenadosComLookup.ThenBy(x => x.Fato.TotalOportunidades)
                        : ordenadosComLookup.ThenByDescending(x => x.Fato.TotalOportunidades)),
                "produtointeresse" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc
                        ? itens.OrderBy(x => x.Fato.ProdutoInteresse ?? "")
                        : itens.OrderByDescending(x => x.Fato.ProdutoInteresse ?? ""))
                    : (criterio.DirecaoAsc
                        ? ordenadosComLookup.ThenBy(x => x.Fato.ProdutoInteresse ?? "")
                        : ordenadosComLookup.ThenByDescending(x => x.Fato.ProdutoInteresse ?? "")),
                "mensagensnaolidas" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc
                        ? itens.OrderBy(x => x.Fato.ConversasNaoLidas)
                        : itens.OrderByDescending(x => x.Fato.ConversasNaoLidas))
                    : (criterio.DirecaoAsc
                        ? ordenadosComLookup.ThenBy(x => x.Fato.ConversasNaoLidas)
                        : ordenadosComLookup.ThenByDescending(x => x.Fato.ConversasNaoLidas)),
                "tempomediorespostaminutos" => ordenadosComLookup == null
                    ? (criterio.DirecaoAsc
                        ? itens.OrderBy(x => x.Fato.TempoMedioRespostaMinutos ?? 0)
                        : itens.OrderByDescending(x => x.Fato.TempoMedioRespostaMinutos ?? 0))
                    : (criterio.DirecaoAsc
                        ? ordenadosComLookup.ThenBy(x => x.Fato.TempoMedioRespostaMinutos ?? 0)
                        : ordenadosComLookup.ThenByDescending(x => x.Fato.TempoMedioRespostaMinutos ?? 0)),
                _ => ordenadosComLookup
            };
        }

        return (ordenadosComLookup ?? itens.OrderByDescending(x => x.Fato.DataUltimoEvento ?? x.Fato.DataReferencia))
            .Select(x => x.Fato)
            .ToList();
    }

    private sealed class OrdenacaoListagemLeadsItem
    {
        public required FatoLeadAgregado Fato { get; init; }
        public required string Nome { get; init; }
        public required string NomeOrigem { get; init; }
        public required string NomeCampanha { get; init; }
        public required string NomeStatus { get; init; }
        public required string NomeResponsavel { get; init; }
    }

}
