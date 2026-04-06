using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Dashboard;
using WebsupplyConnect.Application.Helpers;
using WebsupplyConnect.Application.Interfaces.Dashboard;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using DomainLead = WebsupplyConnect.Domain.Entities.Lead.Lead;

namespace WebsupplyConnect.Application.Services.Dashboard;

public class AcompanhamentoDashboardReaderService(
    ILogger<AcompanhamentoDashboardReaderService> logger,
    ILeadRepository leadRepository,
    IConversaRepository conversaRepository,
    IConversaClassificacaoAiService conversaClassificacaoAiService,
    IDimensaoOlapReadService dimensoesService) : IAcompanhamentoDashboardReaderService
{
    private const int StatusMensagemNaoLidaId = 25;

    private readonly ILogger<AcompanhamentoDashboardReaderService> _logger = logger;
    private readonly ILeadRepository _leadRepository = leadRepository;
    private readonly IConversaRepository _conversaRepository = conversaRepository;
    private readonly IConversaClassificacaoAiService _conversaClassificacaoAiService = conversaClassificacaoAiService;
    private readonly IDimensaoOlapReadService _dimensoesService = dimensoesService;

    public async Task<AcompanhamentoDashboardAgregadoResponseDTO> ObterAcompanhamentoAgregadoAsync(FiltrosDashboardDTO filtros, int usuarioId)
    {
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var leads = await ObterLeadsEscopoAsync(filtros, usuarioId, dataInicio, dataFim);
        var leadsEscopoPendentes = await ObterLeadsEscopoAsync(filtros, usuarioId, null, null);
        var statusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");

        // No agregado, mantém o total de pendências sem recorte de período.
        var leadsPendentesAtendimento = leadsEscopoPendentes.Count(lead => AvaliarLeadPendente(lead, statusEncerradaId) != null);

        var leadsPeriodo = leads
            .Where(l => l.DataCriacao >= dataInicio && l.DataCriacao <= dataFim)
            .ToList();

        var conversasAtivas = leads
            .SelectMany(l => l.Conversas ?? [])
            .Where(c =>
                !c.Excluido &&
                c.StatusId != statusEncerradaId &&
                (c.DataUltimaMensagem ?? c.DataCriacao) >= dataInicio &&
                (c.DataUltimaMensagem ?? c.DataCriacao) <= dataFim)
            .Select(c => c.Id)
            .Distinct()
            .Count();

        var emNegociacao = leadsPeriodo.Count(l =>
            (l.Oportunidades ?? []).Any(o => !o.Excluido && (o.Etapa?.EhAtiva == true || !o.DataFechamento.HasValue)));

        var convertidos = leadsPeriodo.Count(l =>
            l.DataConversaoCliente.HasValue ||
            l.LeadStatus?.ConsiderarCliente == true ||
            (l.Oportunidades ?? []).Any(o => !o.Excluido && (o.Convertida == true || o.DataConversao.HasValue)));

        var agora = TimeHelper.GetBrasiliaTime();
        var inicioHoje = agora.Date;
        var inicioAmanha = inicioHoje.AddDays(1);
        var deslocamentoParaSegunda = ((int)inicioHoje.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var inicioSemana = inicioHoje.AddDays(-deslocamentoParaSegunda);
        var leadsRecebidosHoje = leadsPeriodo.Count(l => l.DataCriacao >= inicioHoje && l.DataCriacao < inicioAmanha);
        var leadsSemana = leadsPeriodo.Count(l => l.DataCriacao >= inicioSemana && l.DataCriacao < inicioAmanha);

        return new AcompanhamentoDashboardAgregadoResponseDTO
        {
            Kpis = new AcompanhamentoDashboardKpisDTO
            {
                LeadsRecebidosHoje = leadsRecebidosHoje,
                LeadsSemana = leadsSemana,
                ConversasAtivas = conversasAtivas,
                LeadsEmNegociacao = emNegociacao,
                LeadsConvertidos = convertidos,
                LeadsPendentesAtendimento = leadsPendentesAtendimento
            },
            UltimaAtualizacao = ObterUltimaAtualizacao(leadsPeriodo)
        };
    }

    public async Task<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>> ObterLeadsPendentesAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina)
    {
        return await ObterLeadsPendentesInternoAsync(
            filtros,
            usuarioId,
            pagina,
            tamanhoPagina,
            avaliacao => true);
    }

    public async Task<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>> ObterLeadsPrimeiroAtendimentoAguardandoClienteAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina)
    {
        return await ObterLeadsPendentesInternoAsync(
            filtros,
            usuarioId,
            pagina,
            tamanhoPagina,
            EhPrimeiroAtendimentoAguardandoCliente,
            aplicarFiltroPrimeiroAtendimentoNaConsulta: true);
    }

    private async Task<PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>> ObterLeadsPendentesInternoAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina,
        Func<LeadPendenteAvaliacao, bool> filtro,
        bool aplicarFiltroPrimeiroAtendimentoNaConsulta = false)
    {
        var statusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var leadIdsEscopo = await ObterIdsEscopoPendentesAsync(
            filtros,
            usuarioId,
            dataInicio,
            dataFim,
            aplicarFiltroPrimeiroAtendimentoNaConsulta);

        const int tamanhoLoteAvaliacao = 75;
        var passando = new List<(int LeadId, LeadPendenteAvaliacao Avaliacao)>();

        foreach (var lote in leadIdsEscopo.Chunk(tamanhoLoteAvaliacao))
        {
            var leadsLote = await _leadRepository.CarregarLeadsAvaliacaoAcompanhamentoPorIdsAsync(
                lote.ToList(),
                statusEncerradaId);

            foreach (var lead in leadsLote)
            {
                var avaliacao = AvaliarLeadPendente(
                    lead,
                    statusEncerradaId,
                    ignorarCriterioPendenciaGeral: aplicarFiltroPrimeiroAtendimentoNaConsulta);
                if (avaliacao == null || !filtro(avaliacao.Value))
                    continue;

                passando.Add((lead.Id, avaliacao.Value));
            }
        }

        var ordenados = passando
            .OrderByDescending(x => x.Avaliacao.DataUltimoEvento)
            .ToList();

        var totalItens = ordenados.Count;
        var totalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina);
        var paginaOrdenados = ordenados.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();
        var pageIds = paginaOrdenados.Select(x => x.LeadId).ToList();

        var agora = TimeHelper.GetBrasiliaTime();
        var itens = new List<AcompanhamentoDashboardLeadPendenteItemDTO>();

        if (pageIds.Count > 0)
        {
            var detalhes = await _leadRepository.ObterLeadsDetalhesDashboardPorIdsAsync(pageIds);
            var detalhesPorId = detalhes.ToDictionary(l => l.Id);
            foreach (var (leadId, avaliacao) in paginaOrdenados)
            {
                if (!detalhesPorId.TryGetValue(leadId, out var lead))
                    continue;

                itens.Add(MapearLeadPendenteItem(lead, avaliacao, agora));
            }
        }

        return new PagedResultDTO<AcompanhamentoDashboardLeadPendenteItemDTO>
        {
            Itens = itens,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    /// <summary>
    /// IDs do escopo (mesmos filtros que <see cref="ObterLeadsEscopoAsync"/>), sem carregar entidades completas.
    /// </summary>
    private async Task<List<int>> ObterIdsEscopoPendentesAsync(
        FiltrosDashboardDTO filtros,
        int? usuarioId,
        DateTime? dataInicio,
        DateTime? dataFim,
        bool apenasPrimeiroAtendimentoAguardandoCliente = false)
    {
        try
        {
            var campanhaOrigemIds = await ResolverCampanhaOrigemIdsPorFiltroNomesAsync(filtros);
            if (filtros.ObterCampanhaNomes().Count > 0 && campanhaOrigemIds.Count == 0)
                return [];

            return await _leadRepository.ListarIdsAcompanhamentoEscopoAsync(
                usuarioId,
                filtros.ObterEmpresaIds(),
                filtros.ObterEquipeIds(),
                filtros.ObterOrigemIds(),
                campanhaOrigemIds,
                dataInicio,
                dataFim,
                apenasPrimeiroAtendimentoAguardandoCliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter leads em tempo real para acompanhamento. UsuarioId: {UsuarioId}", usuarioId);
            throw;
        }
    }

    private static AcompanhamentoDashboardLeadPendenteItemDTO MapearLeadPendenteItem(
        DomainLead lead,
        LeadPendenteAvaliacao avaliacao,
        DateTime agora)
    {
        var tipoPendencia = avaliacao.EhPrimeiroContato
            ? AcompanhamentoDashboardTipoPendenciaConstantes.PrimeiroContato
            : AcompanhamentoDashboardTipoPendenciaConstantes.AguardandoResposta;

        var tempoSemAcaoMinutos = ObterTempoSemAcaoMinutos(agora, avaliacao);
        var tempoAguardandoRespostaMinutos = avaliacao.DataUltimaMensagemCliente.HasValue
            ? CalcularMinutos(agora, avaliacao.DataUltimaMensagemCliente.Value)
            : 0;

        return new AcompanhamentoDashboardLeadPendenteItemDTO
        {
            LeadId = lead.Id,
            NomeLead = lead.Nome,
            DataUltimoEvento = avaliacao.DataUltimoEvento,
            TipoPendencia = tipoPendencia,
            TipoPendenciaLabel = avaliacao.EhPrimeiroContato ? "Primeiro contato" : "Aguardando resposta",
            TipoPendenciaCor = avaliacao.EhPrimeiroContato ? "#0ea5e9" : "#f97316",
            NomeOrigem = lead.Origem?.Nome ?? string.Empty,
            NomeCampanha = ObterNomeCampanha(lead),
            UltimoEvento = ObterUltimoEventoDTO(lead),
            UltimaMensagemCliente = avaliacao.UltimaMensagemCliente,
            MensagensNaoLidas = avaliacao.MensagensNaoLidas,
            TempoSemAcaoMinutos = tempoSemAcaoMinutos,
            TempoSemAcaoLabel = FormatarTempoMinutosLabel(tempoSemAcaoMinutos),
            TempoAguardandoRespostaMinutos = tempoAguardandoRespostaMinutos,
            TempoAguardandoRespostaLabel = FormatarTempoMinutosLabel(tempoAguardandoRespostaMinutos),
            ConversaAtivaId = avaliacao.ConversaAtivaMaisRecente?.Id,
            PendenteRespostaCliente = ObterPendenteRespostaCliente(avaliacao.ConversaAtivaMaisRecente)
        };
    }

    public async Task<PagedResultDTO<AcompanhamentoDashboardConversaAtivaItemDTO>> ObterConversasAtivasAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina)
    {
        var statusConversaAtivaId = await _conversaRepository.GetConversaStatusByCodeAsync("ATIVA");
        var statusConversaAguardandoRespostaId = await _conversaRepository.GetConversaStatusByCodeAsync("AGUARDANDO_RESPOSTA");
        var (dataInicio, dataFim) = filtros.ResolverPeriodo();
        var leads = await ObterLeadsEscopoAsync(filtros, usuarioId, dataInicio, dataFim);
        var itens = new List<AcompanhamentoDashboardConversaAtivaItemDTO>();

        foreach (var lead in leads)
        {
            if (string.Equals(lead.LeadStatus?.Codigo, "INATIVO", StringComparison.OrdinalIgnoreCase))
                continue;

            var conversaAtiva = (lead.Conversas ?? [])
                .Where(c => !c.Excluido &&
                            (c.StatusId == statusConversaAtivaId || c.StatusId == statusConversaAguardandoRespostaId))
                .OrderByDescending(c => c.DataUltimaMensagem ?? c.DataCriacao)
                .FirstOrDefault();

            if (conversaAtiva == null)
                continue;

            var mensagens = conversaAtiva.Mensagens?.Where(m => !m.Excluido).ToList() ?? [];
            var ultimaMensagem = mensagens
                .OrderByDescending(m => m.DataEnvio ?? m.DataCriacao)
                .ThenByDescending(m => m.Id)
                .FirstOrDefault();
            var dataUltimaMensagem = ultimaMensagem != null
                ? (ultimaMensagem.DataEnvio ?? ultimaMensagem.DataCriacao)
                : conversaAtiva.DataUltimaMensagem;

            if (dataUltimaMensagem.HasValue && (dataUltimaMensagem.Value < dataInicio || dataUltimaMensagem.Value > dataFim))
                continue;

            var ultimaMensagemCliente = mensagens
                .Where(m => m.Sentido == 'R')
                .OrderByDescending(m => m.DataEnvio ?? m.DataCriacao)
                .Select(m => m.Conteudo)
                .FirstOrDefault();
            var mensagensNaoLidas = mensagens
                .Count(m => !m.Excluido &&
                            m.Sentido == 'R' &&
                            (m.StatusId == StatusMensagemNaoLidaId ||
                             string.Equals(m.Status?.Codigo, "DELIVERED", StringComparison.OrdinalIgnoreCase)));
            var tempoMedioAtendimentoMinutos = CalcularTempoMedioAtendimentoMinutos(mensagens);

            itens.Add(new AcompanhamentoDashboardConversaAtivaItemDTO
            {
                ConversaAtivaId = conversaAtiva.Id,
                LeadId = lead.Id,
                NomeLead = lead.Nome,
                ProdutoInteresse = ObterProdutoInteresse(lead),
                StatusNome = lead.LeadStatus?.Nome ?? string.Empty,
                StatusCor = lead.LeadStatus?.Cor,
                UltimaMensagemCliente = ultimaMensagemCliente,
                UltimaMensagemEnviadaPor = ObterUltimaMensagemEnviadaPor(ultimaMensagem),
                DataUltimaMensagem = dataUltimaMensagem,
                DataHoraUltimaMensagem = dataUltimaMensagem,
                TempoMedioAtendimentoMinutos = tempoMedioAtendimentoMinutos,
                TempoMedioAtendimentoLabel = FormatarTempoMinutosLabel(tempoMedioAtendimentoMinutos),
                MensagensNaoLidas = mensagensNaoLidas
            });
        }

        var ordenados = itens
            .OrderByDescending(i => i.DataUltimaMensagem ?? DateTime.MinValue)
            .ToList();

        var totalItens = ordenados.Count;
        var totalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina);
        var paginados = ordenados.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();

        var contextosAtualizados = await _conversaRepository.GetContextosByIdsAsync(
            paginados.Select(p => p.ConversaAtivaId).Distinct().ToList());
        foreach (var paginado in paginados)
        {
            if (contextosAtualizados.TryGetValue(paginado.ConversaAtivaId, out var ctx))
            {
                paginado.TrocaDeContato = ctx.TrocaDeContato;
            }
        }

        return new PagedResultDTO<AcompanhamentoDashboardConversaAtivaItemDTO>
        {
            Itens = paginados,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoAtendimentoAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina)
    {
        return await ObterLeadsAguardandoInternoAsync(
            filtros,
            pagina,
            tamanhoPagina,
            ehPrimeiroContato: true);
    }

    public async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoRespostaAsync(
        FiltrosDashboardDTO filtros,
        int usuarioId,
        int pagina,
        int tamanhoPagina)
    {
        return await ObterLeadsAguardandoInternoAsync(
            filtros,
            pagina,
            tamanhoPagina,
            ehPrimeiroContato: false);
    }

    private async Task<PagedResultDTO<DashboardListagemLeadsDTO>> ObterLeadsAguardandoInternoAsync(
        FiltrosDashboardDTO filtros,
        int pagina,
        int tamanhoPagina,
        bool ehPrimeiroContato)
    {
        var statusEncerradaId = await _conversaRepository.GetConversaStatusByCodeAsync("ENCERRADA");
        var campanhaOrigemIds = await ResolverCampanhaOrigemIdsPorFiltroNomesAsync(filtros);
        if (filtros.ObterCampanhaNomes().Count > 0 && campanhaOrigemIds.Count == 0)
        {
            return new PagedResultDTO<DashboardListagemLeadsDTO>
            {
                Itens = [],
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = 0,
                TotalPaginas = 0
            };
        }

        var leads = await _leadRepository.ListarLeadsAcompanhamentoEscopoParaPendenciaAsync(
            null,
            filtros.ObterEmpresaIds(),
            filtros.ObterEquipeIds(),
            filtros.ObterOrigemIds(),
            campanhaOrigemIds,
            filtros.ObterVendedorIds(),
            filtros.ObterStatusLeadIds());
        var candidatos = new List<(DomainLead Lead, LeadPendenteAvaliacao Avaliacao)>();

        foreach (var lead in leads)
        {
            var avaliacao = AvaliarLeadPendente(lead, statusEncerradaId);
            if (avaliacao == null || avaliacao.Value.EhPrimeiroContato != ehPrimeiroContato)
                continue;
            candidatos.Add((lead, avaliacao.Value));
        }

        var ordenados = candidatos
            .OrderByDescending(i => i.Avaliacao.DataUltimoEvento)
            .ToList();

        var totalItens = ordenados.Count;
        var totalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina);
        var paginados = ordenados
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        var leadIdsPagina = paginados.Select(p => p.Lead.Id).Distinct().ToList();
        var detalhesLeadPagina = await _leadRepository.ObterLeadsDetalhesDashboardPorIdsAsync(leadIdsPagina);
        var detalhesLeadLookup = detalhesLeadPagina.ToDictionary(l => l.Id);
        var itensPaginados = new List<DashboardListagemLeadsDTO>(paginados.Count);

        foreach (var (leadLeve, avaliacao) in paginados)
        {
            if (!detalhesLeadLookup.TryGetValue(leadLeve.Id, out var lead))
                continue;

            var conversaAtivaId = avaliacao.ConversaAtivaMaisRecente?.Id;
            var mensagensConversaAtiva = avaliacao.ConversaAtivaMaisRecente?.Mensagens?
                .Where(m => !m.Excluido)
                .ToList() ?? [];
            var tempoMedioRespostaMinutos = mensagensConversaAtiva.Count > 0
                ? (decimal?)CalcularTempoMedioAtendimentoMinutos(mensagensConversaAtiva)
                : null;

            itensPaginados.Add(new DashboardListagemLeadsDTO
            {
                LeadId = lead.Id,
                EmpresaId = lead.EmpresaId,
                Nome = lead.Nome,
                Email = lead.Email,
                Telefone = lead.Telefone,
                NomeStatus = lead.LeadStatus?.Nome ?? string.Empty,
                NomeOrigem = lead.Origem?.Nome ?? string.Empty,
                NomeEquipe = lead.Equipe?.Nome,
                NomeResponsavel = lead.Responsavel?.Usuario?.Nome,
                NomeResponsavelResumido = lead.Responsavel?.Usuario?.Nome is { Length: > 0 } nomeResponsavel
                    ? NomeVendedorHelper.AbreviarNome(nomeResponsavel)
                    : null,
                NomeCampanha = ObterNomeCampanha(lead),
                ProdutoInteresse = ObterProdutoInteresse(lead),
                TotalOportunidades = (lead.Oportunidades ?? []).Count(o => !o.Excluido),
                MensagensNaoLidas = avaliacao.MensagensNaoLidas,
                TempoMedioRespostaMinutos = tempoMedioRespostaMinutos,
                TempoMedioRespostaLabel = FormatarTempoMinutosLabel(tempoMedioRespostaMinutos),
                DataUltimoEvento = avaliacao.DataUltimoEvento,
                DataCriacao = lead.DataCriacao,
                EhConvertido = lead.DataConversaoCliente.HasValue ||
                              lead.LeadStatus?.ConsiderarCliente == true ||
                              (lead.Oportunidades ?? []).Any(o => !o.Excluido && (o.Convertida == true || o.DataConversao.HasValue)),
                ConversaAtivaId = conversaAtivaId,
                PendenteRespostaCliente = ObterPendenteRespostaCliente(avaliacao.ConversaAtivaMaisRecente)
            });
        }

        var conversaIds = itensPaginados
            .Where(i => i.ConversaAtivaId.HasValue)
            .Select(i => i.ConversaAtivaId!.Value)
            .Distinct()
            .ToList();
        var contextosPorConversa = await _conversaRepository.GetContextosByIdsAsync(conversaIds);

        foreach (var item in itensPaginados)
        {
            if (!item.ConversaAtivaId.HasValue)
                continue;
            if (contextosPorConversa.TryGetValue(item.ConversaAtivaId.Value, out var ctx))
            {
                item.TrocaDeContato = ctx.TrocaDeContato;
            }
        }

        return new PagedResultDTO<DashboardListagemLeadsDTO>
        {
            Itens = itensPaginados,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<AcompanhamentoDashboardConversaClassificacaoResponseDTO?> ObterConversaClassificacaoSobDemandaAsync(int conversaId)
    {
        if (conversaId <= 0)
            return null;

        var conversa = await _conversaRepository.GetConversaParaClassificacaoPorIdAsync(conversaId);
        if (conversa == null)
            return null;

        var contextosPorConversa = await _conversaRepository.GetContextosByIdsAsync([conversaId]);
        contextosPorConversa.TryGetValue(conversaId, out var contextoAtual);

        if (NecessitaAtualizacaoSobDemanda(
            conversa,
            contextoAtual.Contexto,
            contextoAtual.ClassificacaoIA,
            contextoAtual.DataAtualizacaoContexto))
        {
            try
            {
                await _conversaClassificacaoAiService.ProcessarConversaSobDemandaAsync(
                    conversaId,
                    executarExtracaoContexto: true,
                    executarDeteccaoContato: false,
                    executarClassificacaoConversa: true);

                contextosPorConversa = await _conversaRepository.GetContextosByIdsAsync([conversaId]);
                contextosPorConversa.TryGetValue(conversaId, out contextoAtual);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha ao atualizar contexto/classificacao sob demanda da conversa {ConversaId}. Retornando dados persistidos.",
                    conversaId);
            }
        }

        return new AcompanhamentoDashboardConversaClassificacaoResponseDTO
        {
            ConversaId = conversaId,
            Contexto = contextoAtual.Contexto,
            CategoriaIA = contextoAtual.ClassificacaoIA,
            DataAtualizacaoContexto = contextoAtual.DataAtualizacaoContexto
        };
    }

    private async Task<List<int>> ResolverCampanhaOrigemIdsPorFiltroNomesAsync(FiltrosDashboardDTO filtros)
    {
        var nomes = filtros.ObterCampanhaNomes();
        if (nomes.Count == 0)
            return [];

        var empresas = filtros.ObterEmpresaIds();
        var dims = await _dimensoesService.ObterDimensoesCampanhaNaoExcluidasAsync();
        IEnumerable<DimensaoCampanha> q = dims;
        if (empresas.Count > 0)
            q = q.Where(d => empresas.Contains(d.EmpresaId));
        var set = new HashSet<string>(nomes, StringComparer.OrdinalIgnoreCase);
        return q.Where(d => set.Contains(d.Nome.Trim()))
            .Select(d => d.CampanhaOrigemId)
            .Distinct()
            .ToList();
    }

    private async Task<List<DomainLead>> ObterLeadsEscopoAsync(
        FiltrosDashboardDTO filtros,
        int? usuarioId,
        DateTime? dataInicio,
        DateTime? dataFim,
        bool apenasPrimeiroAtendimentoAguardandoCliente = false)
    {
        try
        {
            var campanhaOrigemIds = await ResolverCampanhaOrigemIdsPorFiltroNomesAsync(filtros);
            if (filtros.ObterCampanhaNomes().Count > 0 && campanhaOrigemIds.Count == 0)
                return [];

            return await _leadRepository.ListarLeadsAcompanhamentoEscopoAsync(
                usuarioId,
                filtros.ObterEmpresaIds(),
                filtros.ObterEquipeIds(),
                filtros.ObterOrigemIds(),
                campanhaOrigemIds,
                dataInicio,
                dataFim,
                apenasPrimeiroAtendimentoAguardandoCliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter leads em tempo real para acompanhamento. UsuarioId: {UsuarioId}", usuarioId);
            throw;
        }
    }

    private static DateTime? ObterUltimaAtualizacao(List<DomainLead> leads)
    {
        if (leads.Count == 0)
            return null;

        DateTime? ultima = null;
        foreach (var lead in leads)
        {
            ultima = MaxDate(ultima, lead.DataModificacao);
            ultima = MaxDate(ultima, lead.DataCriacao);

            foreach (var oportunidade in lead.Oportunidades ?? [])
            {
                ultima = MaxDate(ultima, oportunidade.DataModificacao);
                if (oportunidade.DataUltimaInteracao.HasValue)
                    ultima = MaxDate(ultima, oportunidade.DataUltimaInteracao.Value);
            }

            foreach (var conversa in lead.Conversas ?? [])
            {
                ultima = MaxDate(ultima, conversa.DataModificacao);
                if (conversa.DataUltimaMensagem.HasValue)
                    ultima = MaxDate(ultima, conversa.DataUltimaMensagem.Value);

                foreach (var mensagem in conversa.Mensagens ?? [])
                {
                    ultima = MaxDate(ultima, mensagem.DataModificacao);
                    if (mensagem.DataEnvio.HasValue)
                        ultima = MaxDate(ultima, mensagem.DataEnvio.Value);
                }
            }
        }

        return ultima;
    }

    private static DateTime? MaxDate(DateTime? atual, DateTime valor)
    {
        if (!atual.HasValue || valor > atual.Value)
            return valor;

        return atual;
    }

    private static int CalcularMinutos(DateTime referencia, DateTime inicio)
    {
        var minutos = (int)Math.Floor((referencia - inicio).TotalMinutes);
        return minutos < 0 ? 0 : minutos;
    }

    /// <summary>
    /// Para "Aguardando resposta" (quando existe conversa e já houve interação do vendedor), tempo sem ação
    /// é desde a última mensagem do vendedor (não bot) até agora. Caso contrário, desde DataUltimoEvento.
    /// </summary>
    private static int ObterTempoSemAcaoMinutos(DateTime agora, LeadPendenteAvaliacao avaliacao)
    {
        var dataReferencia = !avaliacao.EhPrimeiroContato && avaliacao.DataUltimaMensagemVendedor.HasValue
            ? avaliacao.DataUltimaMensagemVendedor.Value
            : avaliacao.DataUltimoEvento;
        return CalcularMinutos(agora, dataReferencia);
    }

    private static DateTime ObterDataUltimoEventoOnline(DomainLead lead, DateTime? ultimaMsgNaoBotDataAtiva)
    {
        var dataUltimoEventoLead = (lead.LeadEventos ?? [])
            .Where(e => !e.Excluido)
            .OrderByDescending(e => e.DataEvento)
            .Select(e => (DateTime?)e.DataEvento)
            .FirstOrDefault();

        return dataUltimoEventoLead ?? ultimaMsgNaoBotDataAtiva ?? lead.DataCriacao;
    }

    /// <summary>
    /// Considera "aguardando resposta do vendedor" (primeiro contato) enquanto não existir envio por vendedor humano.
    /// Mensagem de vendedor humano (com ou sem template) encerra o estado de primeiro contato.
    /// Cenário sem nenhuma conversa (nunca criada; status irrelevante): pendente só com responsável humano atribuído.
    /// Se já existiu conversa (mesmo só encerrada), este critério não aplica — avaliam conversas ativas abaixo.
    /// </summary>
    private static bool CalcularAguardandoRespostaVendedor(
        bool statusExcluiPendencia,
        bool responsavelEhBot,
        bool leadTemResponsavelHumano,
        bool possuiConversaAssociada,
        bool possuiConversaAtiva,
        bool teveInteracaoVendedorHumanoConversaAtiva,
        bool possuiConversaAtivaComPrimeiraMensagemTemplateBot)
    {
        if (statusExcluiPendencia || responsavelEhBot)
            return false;

        if (!possuiConversaAssociada)
            return leadTemResponsavelHumano;

        if (!possuiConversaAtiva)
            return false;

        if (teveInteracaoVendedorHumanoConversaAtiva)
            return false;

        if (possuiConversaAtivaComPrimeiraMensagemTemplateBot)
            return true;

        return true;
    }

    private static int CalcularTempoMedioAtendimentoMinutos(List<Mensagem> mensagens)
    {
        if (mensagens.Count == 0)
            return 0;

        var ordenadas = mensagens
            .OrderBy(m => m.DataEnvio ?? m.DataCriacao)
            .ThenBy(m => m.Id)
            .ToList();

        var intervalos = new List<decimal>();
        Mensagem? ultimaMensagemCliente = null;
        foreach (var mensagem in ordenadas)
        {
            var ehMsgBot = mensagem.Sentido == 'E' && mensagem.Usuario?.IsBot == true;
            if (ehMsgBot)
                continue;

            if (mensagem.Sentido == 'R')
            {
                ultimaMensagemCliente = mensagem;
                continue;
            }

            if (mensagem.Sentido != 'E' || ultimaMensagemCliente == null)
                continue;

            var dataRecebida = ultimaMensagemCliente.DataEnvio ?? ultimaMensagemCliente.DataCriacao;
            var dataResposta = mensagem.DataEnvio ?? mensagem.DataCriacao;
            var minutos = CalcularMinutosUteisEntreDataHoras(dataRecebida, dataResposta);
            if (minutos >= 0)
                intervalos.Add(minutos);

            ultimaMensagemCliente = null;
        }

        if (intervalos.Count == 0)
            return 0;

        return (int)Math.Round(intervalos.Average(), MidpointRounding.AwayFromZero);
    }

    private static string FormatarTempoMinutosLabel(decimal? minutos)
    {
        if (!minutos.HasValue || minutos.Value <= 0)
            return "0min";

        var totalMinutos = (int)Math.Round(minutos.Value, MidpointRounding.AwayFromZero);
        if (totalMinutos < 60)
            return $"{totalMinutos}min";

        var horas = totalMinutos / 60;
        var minutosRestantes = totalMinutos % 60;
        return $"{horas}h{minutosRestantes:00}min";
    }

    private static decimal CalcularMinutosUteisEntreDataHoras(DateTime inicio, DateTime fim)
    {
        if (fim <= inicio) return 0;

        var totalMinutosUteis = 0m;
        var atual = inicio;
        const int maxIteracoes = 366;
        var iteracoes = 0;

        while (atual < fim && iteracoes++ < maxIteracoes)
        {
            var (ehDiaUtil, horaInicio, horaFim) = ObterExpedienteDia(atual.DayOfWeek);

            if (!ehDiaUtil)
            {
                atual = ProximoDiaUtil(atual.Date.AddDays(1));
                continue;
            }

            if (atual.TimeOfDay < horaInicio)
                atual = atual.Date + horaInicio;

            if (atual.TimeOfDay >= horaFim)
            {
                atual = ProximoDiaUtil(atual.Date.AddDays(1));
                continue;
            }

            var fimDiaUtil = atual.Date + horaFim;
            var fimEfetivo = fim < fimDiaUtil ? fim : fimDiaUtil;

            if (fimEfetivo > atual)
                totalMinutosUteis += (decimal)(fimEfetivo - atual).TotalMinutes;

            atual = ProximoDiaUtil(atual.Date.AddDays(1));
        }

        return totalMinutosUteis;
    }

    private static (bool EhDiaUtil, TimeSpan HoraInicio, TimeSpan HoraFim) ObterExpedienteDia(DayOfWeek dia)
    {
        if (dia == DayOfWeek.Sunday)
            return (false, TimeSpan.Zero, TimeSpan.Zero);

        return (true, new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0));
    }

    private static DateTime ProximoDiaUtil(DateTime data)
    {
        const int maxIteracoes = 8;
        var iteracoes = 0;

        while (iteracoes++ < maxIteracoes)
        {
            var (ehDiaUtil, horaInicio, _) = ObterExpedienteDia(data.DayOfWeek);
            if (ehDiaUtil)
                return data + horaInicio;
            data = data.AddDays(1);
        }

        return data + new TimeSpan(8, 0, 0);
    }

    private static string? ObterNomeCampanha(DomainLead lead)
    {
        return (lead.LeadEventos ?? [])
            .Where(e => !e.Excluido && e.Campanha != null)
            .OrderByDescending(e => e.DataEvento)
            .Select(e => e.Campanha.Nome)
            .FirstOrDefault();
    }

    private static AcompanhamentoDashboardUltimoEventoDTO? ObterUltimoEventoDTO(DomainLead lead)
    {
        var ultimo = (lead.LeadEventos ?? [])
            .Where(e => !e.Excluido)
            .OrderByDescending(e => e.DataEvento)
            .FirstOrDefault();
        if (ultimo == null)
            return null;
        return new AcompanhamentoDashboardUltimoEventoDTO
        {
            Id = ultimo.Id,
            DataEvento = ultimo.DataEvento,
            NomeCampanha = ultimo.Campanha?.Nome
        };
    }

    private static string? ObterProdutoInteresse(DomainLead lead)
    {
        var oportunidade = (lead.Oportunidades ?? [])
            .Where(o => !o.Excluido)
            .OrderByDescending(o => o.DataModificacao)
            .FirstOrDefault();

        return oportunidade?.Produto?.Nome;
    }

    private static string ObterUltimaMensagemEnviadaPor(Mensagem? mensagem)
    {
        if (mensagem == null)
            return string.Empty;

        if (mensagem.Sentido == 'R')
            return "Cliente";

        if (mensagem.Usuario?.IsBot == true)
            return "Bot";

        if (!string.IsNullOrWhiteSpace(mensagem.Usuario?.Nome))
            return mensagem.Usuario.Nome;

        return "Atendimento";
    }

    private static bool NecessitaAtualizacaoSobDemanda(
        Conversa conversa,
        string? contexto,
        string? categoriaIa,
        DateTime? dataAtualizacaoContexto)
    {
        if (string.IsNullOrWhiteSpace(contexto) || string.IsNullOrWhiteSpace(categoriaIa))
            return true;

        if (!dataAtualizacaoContexto.HasValue)
            return true;

        var dataReferencia = conversa.DataUltimaMensagem ?? conversa.DataCriacao;
        return dataReferencia > dataAtualizacaoContexto.Value;
    }

    /// <summary>
    /// Indica se a última mensagem da conversa ativa é de envio por usuário humano (não-bot),
    /// aguardando resposta do cliente.
    /// </summary>
    private static bool ObterPendenteRespostaCliente(Conversa? conversa)
    {
        if (conversa?.Mensagens == null)
            return false;

        var ultimaMensagem = conversa.Mensagens
            .Where(m => !m.Excluido)
            .OrderByDescending(m => m.DataEnvio ?? m.DataCriacao)
            .ThenByDescending(m => m.Id)
            .FirstOrDefault();

        return ultimaMensagem != null &&
               ultimaMensagem.Sentido == 'E' &&
               ultimaMensagem.UsuarioId.HasValue &&
               ultimaMensagem.Usuario?.IsBot == false;
    }

    /// <summary>
    /// Primeiro atendimento aguardando cliente: na conversa ativa mais recente, só existem envios (E),
    /// nenhuma recebida (R); última mensagem é envio — vale texto livre ou template.
    /// </summary>
    private static bool EhPrimeiroAtendimentoAguardandoCliente(LeadPendenteAvaliacao avaliacao)
    {
        var conversa = avaliacao.ConversaAtivaMaisRecente;
        if (conversa?.Mensagens == null)
            return false;

        if (conversa.Usuario?.IsBot == true)
            return false;

        var mensagens = conversa.Mensagens
            .Where(m => !m.Excluido)
            .ToList();
        if (mensagens.Count == 0)
            return false;

        if (mensagens.Any(m => m.Sentido == 'R'))
            return false;

        var ultimaMensagem = mensagens
            .OrderByDescending(m => m.DataEnvio ?? m.DataCriacao)
            .ThenByDescending(m => m.Id)
            .FirstOrDefault();
        if (ultimaMensagem == null || ultimaMensagem.Sentido != 'E')
            return false;

        return true;
    }

    /// <summary>
    /// Avalia se o lead é "pendente de atendimento" no período informado.
    /// Retorna null se não for pendente ou se o último evento estiver fora do período.
    /// Usado pelo totalizador do home-agregado.
    /// </summary>
    private static LeadPendenteAvaliacao? AvaliarLeadPendenteNoPeriodo(
        DomainLead lead,
        int statusConversaEncerradaId,
        DateTime dataInicio,
        DateTime dataFim)
    {
        var avaliacao = AvaliarLeadPendente(lead, statusConversaEncerradaId);
        if (avaliacao == null)
            return null;

        if (avaliacao.Value.DataUltimoEvento < dataInicio || avaliacao.Value.DataUltimoEvento > dataFim)
            return null;

        return avaliacao;
    }

    private static bool LeadStatusExcluiPendenciaAtendimento(string? codigoStatus)
    {
        if (string.IsNullOrEmpty(codigoStatus))
            return false;
        return codigoStatus.Equals("INATIVO", StringComparison.OrdinalIgnoreCase)
            || codigoStatus.Equals("DESQUALIFICADO", StringComparison.OrdinalIgnoreCase)
            || codigoStatus.Equals("NAO_CONVERTIDO", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Avalia se o lead é "pendente de atendimento" sem considerar período.
    /// Usado pela listagem do home-leads-pendentes.
    /// </summary>
    /// <param name="ignorarCriterioPendenciaGeral">
    /// Quando true (listagem primeiro atendimento aguardando cliente), não exige as regras gerais
    /// de pendência (aguardando vendedor / atendimento / cliente com não lidas). A consulta SQL já
    /// restringe o escopo; caso contrário leads com envio humano e só mensagens E seriam descartados
    /// aqui mesmo aparecendo na query.
    /// </param>
    private static LeadPendenteAvaliacao? AvaliarLeadPendente(
        DomainLead lead,
        int statusConversaEncerradaId,
        bool ignorarCriterioPendenciaGeral = false)
    {
        var statusExcluiPendencia = LeadStatusExcluiPendenciaAtendimento(lead.LeadStatus?.Codigo);
        var responsavelEhBot = lead.Responsavel?.Usuario?.IsBot == true;
        var leadTemResponsavelHumano = lead.Responsavel?.Usuario != null && lead.Responsavel.Usuario.IsBot != true;
        var conversasNaoExcluidas = (lead.Conversas ?? [])
            .Where(c => !c.Excluido)
            .ToList();
        var possuiConversaAssociada = conversasNaoExcluidas.Count > 0;
        var conversasAtivas = conversasNaoExcluidas
            .Where(c => c.StatusId != statusConversaEncerradaId)
            .ToList();
        var possuiConversaAtiva = conversasAtivas.Count > 0;
        var conversaAtivaMaisRecente = conversasAtivas
            .OrderByDescending(c => c.DataUltimaMensagem ?? c.DataCriacao)
            .FirstOrDefault();

        var teveInteracaoVendedorHumano = false;
        var teveInteracaoVendedorHumanoConversaAtiva = false;
        var possuiConversaAtivaComPrimeiraMensagemTemplateBot = false;
        var possuiConversaAguardandoRespostaAtendimento = false;
        var possuiConversaComMensagensNaoLidas = conversasAtivas.Any(c => c.PossuiMensagensNaoLidas);
        DateTime? ultimaMsgNaoBotDataAtiva = null;
        DateTime? ultimaMensagemVendedorDataAtiva = null;

        foreach (var conversaAtiva in conversasAtivas)
        {
            var mensagensOrdenadas = (conversaAtiva.Mensagens ?? [])
                .Where(m => !m.Excluido)
                .OrderBy(m => m.DataEnvio ?? m.DataCriacao)
                .ThenBy(m => m.Id)
                .ToList();
            var teveInteracaoVendedorHumanoNaConversa = false;
            DateTime? ultimaMsgNaoBotDataConversa = null;
            char? ultimaMsgNaoBotSentidoConversa = null;

            var primeiraMensagem = mensagensOrdenadas.FirstOrDefault();
            if (primeiraMensagem != null)
            {
                // Só template enviado por bot mantém o cenário explícito de primeiro contato.
                var primeiraMensagemEhTemplateBot = primeiraMensagem.TemplateId.HasValue &&
                                                    primeiraMensagem.Sentido == 'E' &&
                                                    primeiraMensagem.Usuario?.IsBot == true;
                if (primeiraMensagemEhTemplateBot)
                    possuiConversaAtivaComPrimeiraMensagemTemplateBot = true;
            }

            foreach (var msg in mensagensOrdenadas)
            {
                var ehMsgBot = msg.Sentido == 'E' && msg.Usuario?.IsBot == true;
                if (ehMsgBot)
                    continue;

                // Interação do vendedor humano: qualquer envio do vendedor (com ou sem template).
                var ehMensagemVendedorHumano = msg.Sentido == 'E' && msg.UsuarioId.HasValue;
                if (ehMensagemVendedorHumano)
                {
                    teveInteracaoVendedorHumano = true;
                    teveInteracaoVendedorHumanoConversaAtiva = true;
                    teveInteracaoVendedorHumanoNaConversa = true;
                    var dataMensagemVendedor = msg.DataEnvio ?? msg.DataCriacao;
                    if (!ultimaMensagemVendedorDataAtiva.HasValue || dataMensagemVendedor > ultimaMensagemVendedorDataAtiva.Value)
                        ultimaMensagemVendedorDataAtiva = dataMensagemVendedor;
                }

                var dataMensagem = msg.DataEnvio ?? msg.DataCriacao;
                if (!ultimaMsgNaoBotDataAtiva.HasValue || dataMensagem > ultimaMsgNaoBotDataAtiva.Value)
                {
                    ultimaMsgNaoBotDataAtiva = dataMensagem;
                }

                if (!ultimaMsgNaoBotDataConversa.HasValue || dataMensagem > ultimaMsgNaoBotDataConversa.Value)
                {
                    ultimaMsgNaoBotDataConversa = dataMensagem;
                    ultimaMsgNaoBotSentidoConversa = msg.Sentido;
                }
            }

            if (ultimaMsgNaoBotSentidoConversa == 'R' && teveInteracaoVendedorHumanoNaConversa)
                possuiConversaAguardandoRespostaAtendimento = true;
        }

        var aguardandoRespostaVendedor = CalcularAguardandoRespostaVendedor(
            statusExcluiPendencia,
            responsavelEhBot,
            leadTemResponsavelHumano,
            possuiConversaAssociada,
            possuiConversaAtiva,
            teveInteracaoVendedorHumanoConversaAtiva,
            possuiConversaAtivaComPrimeiraMensagemTemplateBot);

        var aguardandoRespostaAtendimento = !statusExcluiPendencia &&
                                           possuiConversaAtiva &&
                                           teveInteracaoVendedorHumano &&
                                           possuiConversaAguardandoRespostaAtendimento;

        var clienteAguardandoEmConversaAberta = !statusExcluiPendencia &&
                                                possuiConversaAtiva &&
                                                possuiConversaComMensagensNaoLidas;

        if (statusExcluiPendencia)
            return null;

        if (!ignorarCriterioPendenciaGeral &&
            !aguardandoRespostaVendedor &&
            !aguardandoRespostaAtendimento &&
            !clienteAguardandoEmConversaAberta)
            return null;

        var dataUltimoEvento = ObterDataUltimoEventoOnline(lead, ultimaMsgNaoBotDataAtiva);

        var mensagensConversaAtiva = conversaAtivaMaisRecente?.Mensagens?
                .Where(m => !m.Excluido)
                .ToList() ?? [];
        var ultimaMensagemClienteMsg = mensagensConversaAtiva
            .Where(m => m.Sentido == 'R')
            .OrderByDescending(m => m.DataEnvio ?? m.DataCriacao)
            .FirstOrDefault();
        var ultimaMensagemCliente = ultimaMensagemClienteMsg?.Conteudo;
        var dataUltimaMensagemCliente = ultimaMensagemClienteMsg != null
            ? (DateTime?)(ultimaMensagemClienteMsg.DataEnvio ?? ultimaMensagemClienteMsg.DataCriacao)
            : null;
        var mensagensNaoLidas = mensagensConversaAtiva
            .Count(m => !m.Excluido &&
                        m.Sentido == 'R' &&
                        (m.StatusId == StatusMensagemNaoLidaId ||
                         string.Equals(m.Status?.Codigo, "DELIVERED", StringComparison.OrdinalIgnoreCase)));

        var ehPrimeiroContato = ignorarCriterioPendenciaGeral || aguardandoRespostaVendedor;

        return new LeadPendenteAvaliacao(
            DataUltimoEvento: dataUltimoEvento,
            DataUltimaMensagemVendedor: ultimaMensagemVendedorDataAtiva,
            DataUltimaMensagemCliente: dataUltimaMensagemCliente,
            EhPrimeiroContato: ehPrimeiroContato,
            ConversaAtivaMaisRecente: conversaAtivaMaisRecente,
            UltimaMensagemCliente: ultimaMensagemCliente,
            MensagensNaoLidas: mensagensNaoLidas);
    }

    private readonly record struct LeadPendenteAvaliacao(
        DateTime DataUltimoEvento,
        DateTime? DataUltimaMensagemVendedor,
        DateTime? DataUltimaMensagemCliente,
        bool EhPrimeiroContato,
        Conversa? ConversaAtivaMaisRecente,
        string? UltimaMensagemCliente,
        int MensagensNaoLidas);

}
