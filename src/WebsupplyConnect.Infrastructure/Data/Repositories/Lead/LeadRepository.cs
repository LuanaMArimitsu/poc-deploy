using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Lead
{
    internal class LeadRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork, ILogger<LeadRepository> logger) : BaseRepository(dbContext, unitOfWork), ILeadRepository
    {
        private readonly ILogger<LeadRepository> _logger = logger;

        /// <summary>
        /// Filtros compartilhados entre listagem completa e listagem só de IDs (sem Includes).
        /// </summary>
        private async Task<(IQueryable<Domain.Entities.Lead.Lead> Query, int? StatusEncerradaIdQuandoPrimeiro)>
            MontarQueryAcompanhamentoEscopoFiltradaAsync(
                int? usuarioId,
                List<int> empresaIds,
                List<int> equipeIds,
                List<int> origemIds,
                List<int> campanhaIds,
                DateTime? dataInicio,
                DateTime? dataFim,
                bool apenasPrimeiroAtendimentoAguardandoCliente)
        {
            IQueryable<Domain.Entities.Lead.Lead> query = _context.Lead
                .AsNoTracking()
                .Where(l => !l.Excluido);

            int? statusEncerradaIdParaPrimeiro = null;

            if (apenasPrimeiroAtendimentoAguardandoCliente)
            {
                var statusEncerradaId = await _context.Set<ConversaStatus>()
                    .AsNoTracking()
                    .Where(s => s.Codigo == "ENCERRADA")
                    .Select(s => s.Id)
                    .FirstAsync();

                statusEncerradaIdParaPrimeiro = statusEncerradaId;

                query = query
                    .Where(l =>
                        l.ResponsavelId != null &&
                        l.Responsavel != null &&
                        l.Responsavel.Usuario != null &&
                        !l.Responsavel.Usuario.IsBot)
                    .Where(l => l.Conversas.Any(c =>
                        !c.Excluido &&
                        c.StatusId != statusEncerradaId &&
                        !c.Usuario.IsBot &&
                        !l.Conversas.Any(c2 =>
                            !c2.Excluido &&
                            c2.StatusId != statusEncerradaId &&
                            (c2.DataUltimaMensagem ?? c2.DataCriacao) >
                            (c.DataUltimaMensagem ?? c.DataCriacao)) &&
                        c.Mensagens.Any(m => !m.Excluido && m.Sentido == 'E') &&
                        !c.Mensagens.Any(m => !m.Excluido && m.Sentido == 'R')));
            }
            else
            {
                query = query.Where(l =>
                    l.Responsavel == null
                    || l.Responsavel.Usuario == null
                    || !l.Responsavel.Usuario.IsBot);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
                query = query.Where(l => l.ResponsavelId.HasValue && l.Responsavel.UsuarioId == usuarioId.Value);

            if (empresaIds.Count > 0)
                query = query.Where(l => empresaIds.Contains(l.EmpresaId));

            if (equipeIds.Count > 0)
                query = query.Where(l => l.EquipeId.HasValue && equipeIds.Contains(l.EquipeId.Value));

            if (origemIds.Count > 0)
                query = query.Where(l => origemIds.Contains(l.OrigemId));

            if (campanhaIds.Count > 0)
            {
                query = query.Where(l =>
                    l.LeadEventos.Any(le => le.CampanhaId.HasValue && campanhaIds.Contains(le.CampanhaId.Value)));
            }

            if (dataInicio.HasValue && dataFim.HasValue)
            {
                var inicio = dataInicio.Value;
                var fim = dataFim.Value;

                // Inclui leads atribuídos ao usuário sem nenhuma conversa (pendência "primeiro contato"),
                // mesmo quando DataCriacao/eventos ficam fora do período — caso contrário o recorte de datas
                // remove o ID antes da avaliação em memória.
                query = query.Where(l =>
                    (l.DataCriacao >= inicio && l.DataCriacao <= fim) ||
                    l.LeadEventos.Any(e => !e.Excluido && e.DataEvento >= inicio && e.DataEvento <= fim) ||
                    l.Conversas.Any(c => !c.Excluido && (c.DataUltimaMensagem ?? c.DataCriacao) >= inicio && (c.DataUltimaMensagem ?? c.DataCriacao) <= fim) ||
                    (
                        usuarioId.HasValue &&
                        usuarioId.Value > 0 &&
                        l.ResponsavelId.HasValue &&
                        l.Responsavel != null &&
                        l.Responsavel.UsuarioId == usuarioId.Value &&
                        !l.Conversas.Any(c => !c.Excluido)));
            }

            return (query, statusEncerradaIdParaPrimeiro);
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ListarLeadsAcompanhamentoEscopoAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasPrimeiroAtendimentoAguardandoCliente = false)
        {
            var (baseQuery, statusEncerradaIdParaHidratacaoMensagens) =
                await MontarQueryAcompanhamentoEscopoFiltradaAsync(
                    usuarioId,
                    empresaIds,
                    equipeIds,
                    origemIds,
                    campanhaIds,
                    dataInicio,
                    dataFim,
                    apenasPrimeiroAtendimentoAguardandoCliente);

            var query = baseQuery
                .AsSplitQuery()
                .Include(l => l.LeadStatus)
                .Include(l => l.Origem)
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .Include(l => l.LeadEventos.Where(e => !e.Excluido))
                    .ThenInclude(le => le.Campanha)
                .Include(l => l.Oportunidades.Where(o => !o.Excluido))
                    .ThenInclude(o => o.Produto)
                .Include(l => l.Oportunidades.Where(o => !o.Excluido))
                    .ThenInclude(o => o.Etapa)
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Usuario)
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Status);

            var leads = await query.ToListAsync();

            if (leads.Count > 0)
            {
                var statusEncerradaHidratacao = statusEncerradaIdParaHidratacaoMensagens
                    ?? await _context.Set<ConversaStatus>()
                        .AsNoTracking()
                        .Where(s => s.Codigo == "ENCERRADA")
                        .Select(s => s.Id)
                        .FirstAsync();

                await HidratarMensagensSomenteConversasAtivasAsync(leads, statusEncerradaHidratacao);
            }

            return leads;
        }

        public async Task<List<int>> ListarIdsAcompanhamentoEscopoAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasPrimeiroAtendimentoAguardandoCliente = false)
        {
            var (baseQuery, _) = await MontarQueryAcompanhamentoEscopoFiltradaAsync(
                usuarioId,
                empresaIds,
                equipeIds,
                origemIds,
                campanhaIds,
                dataInicio,
                dataFim,
                apenasPrimeiroAtendimentoAguardandoCliente);

            return await baseQuery
                .OrderByDescending(l => l.DataModificacao)
                .ThenByDescending(l => l.Id)
                .Select(l => l.Id)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.Lead.Lead>> CarregarLeadsAvaliacaoAcompanhamentoPorIdsAsync(
            List<int> leadIds,
            int statusConversaEncerradaId)
        {
            if (leadIds.Count == 0)
                return [];

            var leads = await _context.Lead
                .AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.LeadStatus)
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .Include(l => l.LeadEventos.Where(e => !e.Excluido))
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Usuario)
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Status)
                .Where(l => !l.Excluido && leadIds.Contains(l.Id))
                .ToListAsync();

            await HidratarMensagensSomenteConversasAtivasAsync(leads, statusConversaEncerradaId);

            var ordem = new Dictionary<int, int>(leadIds.Count);
            for (var i = 0; i < leadIds.Count; i++)
                ordem[leadIds[i]] = i;

            return leads.OrderBy(l => ordem[l.Id]).ToList();
        }

        /// <summary>
        /// Carrega mensagens só de conversas não encerradas em consulta separada (1+1 queries),
        /// evitando explosão cartesiana na query principal de <see cref="ListarLeadsAcompanhamentoEscopoAsync"/>.
        /// Usado em home-leads-pendentes, home-conversas-ativas, agregado e primeiro atendimento.
        /// </summary>
        private async Task HidratarMensagensSomenteConversasAtivasAsync(
            List<Domain.Entities.Lead.Lead> leads,
            int statusEncerradaId)
        {
            var conversaIdsAtivas = leads
                .SelectMany(l => l.Conversas ?? [])
                .Where(c => !c.Excluido && c.StatusId != statusEncerradaId)
                .Select(c => c.Id)
                .Distinct()
                .ToList();

            if (conversaIdsAtivas.Count == 0)
                return;

            const int tamanhoLoteIds = 1000;
            var todasMensagens = new List<Mensagem>();
            foreach (var lote in conversaIdsAtivas.Chunk(tamanhoLoteIds))
            {
                var idsLote = lote.ToArray();
                var loteMensagens = await _context.Mensagem
                    .AsNoTracking()
                    .Where(m => !m.Excluido && idsLote.Contains(m.ConversaId))
                    .Include(m => m.Usuario)
                    .Include(m => m.Status)
                    .ToListAsync();
                todasMensagens.AddRange(loteMensagens);
            }

            var porConversa = todasMensagens
                .GroupBy(m => m.ConversaId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var lead in leads)
            {
                foreach (var conversa in lead.Conversas ?? [])
                {
                    if (conversa.Excluido || conversa.StatusId == statusEncerradaId)
                        continue;

                    if (porConversa.TryGetValue(conversa.Id, out var lista))
                    {
                        foreach (var m in lista)
                            conversa.Mensagens.Add(m);
                    }
                }
            }
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ListarLeadsAcompanhamentoEscopoParaPendenciaAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            List<int> vendedorUsuarioIds,
            List<int> statusLeadIds)
        {
            IQueryable<Domain.Entities.Lead.Lead> query = _context.Lead
                .AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.LeadStatus)
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .Include(l => l.LeadEventos.Where(e => !e.Excluido))
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Usuario)
                .Include(l => l.Conversas.Where(c => !c.Excluido))
                    .ThenInclude(c => c.Status)
                .Where(l => !l.Excluido)
                .Where(l =>
                    l.Responsavel == null
                    || l.Responsavel.Usuario == null
                    || !l.Responsavel.Usuario.IsBot);

            if (usuarioId.HasValue && usuarioId.Value > 0)
                query = query.Where(l => l.ResponsavelId.HasValue && l.Responsavel.UsuarioId == usuarioId.Value);

            if (empresaIds.Count > 0)
                query = query.Where(l => empresaIds.Contains(l.EmpresaId));

            if (equipeIds.Count > 0)
                query = query.Where(l => l.EquipeId.HasValue && equipeIds.Contains(l.EquipeId.Value));

            if (origemIds.Count > 0)
                query = query.Where(l => origemIds.Contains(l.OrigemId));

            if (campanhaIds.Count > 0)
            {
                query = query.Where(l =>
                    l.LeadEventos.Any(le => le.CampanhaId.HasValue && campanhaIds.Contains(le.CampanhaId.Value)));
            }

            if (vendedorUsuarioIds.Count > 0)
                query = query.Where(l =>
                    l.ResponsavelId.HasValue && vendedorUsuarioIds.Contains(l.Responsavel.UsuarioId));

            if (statusLeadIds.Count > 0)
                query = query.Where(l => statusLeadIds.Contains(l.LeadStatusId));

            var leads = await query.ToListAsync();

            if (leads.Count > 0)
            {
                var statusEncerradaId = await _context.Set<ConversaStatus>()
                    .AsNoTracking()
                    .Where(s => s.Codigo == "ENCERRADA")
                    .Select(s => s.Id)
                    .FirstAsync();

                await HidratarMensagensSomenteConversasAtivasAsync(leads, statusEncerradaId);
            }

            return leads;
        }

        public async Task<List<Domain.Entities.Lead.Lead>> ObterLeadsDetalhesDashboardPorIdsAsync(List<int> leadIds)
        {
            if (leadIds.Count == 0)
                return [];

            return await _context.Lead
                .AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.LeadStatus)
                .Include(l => l.Origem)
                .Include(l => l.Equipe)
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .Include(l => l.LeadEventos.Where(e => !e.Excluido))
                    .ThenInclude(le => le.Campanha)
                .Include(l => l.Oportunidades.Where(o => !o.Excluido))
                    .ThenInclude(o => o.Produto)
                .Where(l => !l.Excluido && leadIds.Contains(l.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Obtém um lead existente a partir do número de WhatsApp informado.
        /// </summary>
        /// <param name="whatsAppNumber">Número de WhatsApp a ser consultado.</param>
        /// <param name="canalId">ID do canal de comunicação.</param>
        /// <returns>Retorna o Lead correspondente ou null se não for encontrado.</returns>
        public async Task<Domain.Entities.Lead.Lead?> GetLeadByWhatsAppNumberAsync(string whatsAppNumber, int empresaId)
        {
            return await _context.Lead
                .Where(l => l.WhatsappNumero == whatsAppNumber && l.EmpresaId == empresaId && l.Excluido == false)
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync();
        }

        public async Task<Domain.Entities.Lead.Lead?> GetLeadByWhatsAppNumberAndGroupAsync(
            string whatsAppNumber,
            List<int> empresaIds)
        {
            return await _context.Lead
                .Where(l =>
                    l.Excluido == false &&
                    l.WhatsappNumero == whatsAppNumber &&
                    empresaIds.Contains(l.EmpresaId))
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync();
        }

        public async Task<Domain.Entities.Lead.Lead?> ObterLeadExistenteNoMesmoGrupo(string? whatsAppNumero, string? email, string? cpf, int grupoEmpresaId)
        {
            return await _context.Lead
                .Include(l => l.Empresa)
                .Include(l => l.Responsavel)
                    .ThenInclude(r => r.Usuario)
                .Where(l =>
                            !l.Excluido &&
                            l.Empresa.GrupoEmpresaId == grupoEmpresaId &&
                            (
                                (whatsAppNumero != null && l.WhatsappNumero == whatsAppNumero) ||
                                (email != null && l.Email == email) ||
                                (cpf != null && l.CPF == cpf)
                            )
                )
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtém o ID do status de lead correspondente ao código informado (case-insensitive),
        /// ignorando registros marcados como excluídos.
        /// </summary>
        /// <param name="status">O código do status a ser buscado.</param>
        /// <returns>O ID do status de lead, ou 0 se não encontrado.</returns>
        public async Task<int> GetLeadStatusId(string status)
        {
            return await _context.LeadStatus
                .Where(ls => !ls.Excluido && ls.Codigo.Equals(status))
                .Select(ls => (int?)ls.Id)
                .FirstOrDefaultAsync() ?? 0;
        }

        public async Task<string?> GetLeadStatusCodigoAsync(int leadStatusId, bool includeDeleted = false)
        {
            var entidade = await GetByPredicateAsync<EntidadeTipificacao>(
                w => w.Id == leadStatusId,
                includeDeleted
            );

            return entidade?.Codigo;
        }

        /// <summary>
        /// Conta o número de leads recebidos por um vendedor em um período específico
        /// </summary>
        public async Task<int> ContarLeadsRecebidosPorVendedorAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            var dataLimite = DateTime.Now.AddDays(-periodoEmDias);

            // Verificar se há um join necessário com a tabela Lead
            return await _context.Set<Domain.Entities.Distribuicao.AtribuicaoLead>()
                .Where(a => a.MembroAtribuidoId == vendedorId &&
                        !a.Excluido &&
                        a.DataAtribuicao >= dataLimite)
                .Join(_context.Set<Domain.Entities.Lead.Lead>(),
                    a => a.LeadId,
                    l => l.Id,
                    (a, l) => new { Atribuicao = a, Lead = l })
                .Where(al => al.Lead.EmpresaId == empresaId && !al.Lead.Excluido)
                .CountAsync();
        }

        /// <summary>
        /// Conta o número de leads convertidos por um vendedor em um período específico
        /// </summary>
        public async Task<int> ContarLeadsConvertidosPorVendedorAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            var dataLimite = DateTime.Now.AddDays(-periodoEmDias);

            // Assumindo que existe um status "CONVERTIDO" ou similar
            var statusConvertido = await GetLeadStatusId("CONVERTIDO");

            return await _context.Set<Domain.Entities.Lead.Lead>()
                .Where(l => l.ResponsavelId == vendedorId &&
                        !l.Excluido &&
                        l.EmpresaId == empresaId &&
                        l.LeadStatusId == statusConvertido &&
                        l.DataModificacao >= dataLimite)
                .CountAsync();
        }

        /// <summary>
        /// Conta o número de leads perdidos por inatividade por um vendedor em um período específico
        /// </summary>
        public async Task<int> ContarLeadsPerdidosPorInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            var dataLimite = DateTime.Now.AddDays(-periodoEmDias);

            // Assumindo que existe um status "PERDIDO_INATIVIDADE" ou similar
            var statusPerdido = await GetLeadStatusId("PERDIDO_INATIVIDADE");

            return await _context.Set<Domain.Entities.Lead.Lead>()
                .Where(l => l.ResponsavelId == vendedorId &&
                        !l.Excluido &&
                        l.EmpresaId == empresaId &&
                        l.LeadStatusId == statusPerdido &&
                        l.DataModificacao >= dataLimite)
                .CountAsync();
        }

        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor em minutos
        /// </summary>
        public async Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = 30)
        {
            try
            {
                var dataLimite = DateTime.Now.AddDays(-periodoEmDias);

                // Buscar dados do banco primeiro
                var dadosAtribuicao = await _context.Set<Domain.Entities.Distribuicao.AtribuicaoLead>()
                    .Join(_context.Set<Domain.Entities.Lead.Lead>(),
                        a => a.LeadId,
                        l => l.Id,
                        (a, l) => new { Atribuicao = a, Lead = l })
                    .Where(al => al.Atribuicao.MembroAtribuidoId == vendedorId &&
                            !al.Atribuicao.Excluido &&
                            al.Atribuicao.DataAtribuicao >= dataLimite &&
                            al.Lead.EmpresaId == empresaId &&
                            al.Lead.DataPrimeiroContato.HasValue)
                    .Select(al => new
                    {
                        DataAtribuicao = al.Atribuicao.DataAtribuicao,
                        DataPrimeiroContato = al.Lead.DataPrimeiroContato.Value
                    })
                    .ToListAsync();

                // Calcular tempos de atendimento na memória
                var temposAtendimento = dadosAtribuicao
                    .Select(d => (decimal)(d.DataPrimeiroContato - d.DataAtribuicao).TotalMinutes)
                    .Where(tempo => tempo > 0)
                    .ToList();

                if (!temposAtendimento.Any())
                    return 0;

                return temposAtendimento.Average();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular velocidade média de atendimento do vendedor {VendedorId}", vendedorId);
                return 0; // Retorna 0 em caso de erro
            }
        }

        public Task<Domain.Entities.Lead.Lead?> GetLeadWithDetailsAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Lead
                .Include(l => l.Empresa)
                .Include(l => l.Origem)
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Include(l => l.LeadStatus)
                .Include(l => l.Equipe)
                    .ThenInclude(e => e.ResponsavelMembro)
                .Include(l => l.LeadEventos
                    .OrderByDescending(e => e.DataEvento)
                    .Take(1))
                    .ThenInclude(e => e.Campanha)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(l => !l.Excluido);

            return query.FirstOrDefaultAsync(l => l.Id == id);
        }


        public Task<Domain.Entities.Lead.Lead?> GetLeadWithUsuarioAsync(int id)
        {
            return _context.Lead
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Include(l => l.Equipe)
                    .ThenInclude(e => e.ResponsavelMembro)
                .Where(l => l.Id == id && !l.Excluido)
                .Include(l => l.LeadEventos
                    .OrderByDescending(e => e.DataEvento)
                    .Take(1))
                    .ThenInclude(e => e.Campanha)
                .FirstOrDefaultAsync();
        }

        public Task<Domain.Entities.Lead.Lead?> GetLeadWithUsuarioIncludingDeletedAsync(int id)
        {
            return _context.Lead
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Where(l => l.Id == id && l.Excluido)
                .FirstOrDefaultAsync();
        }

        public Task<Domain.Entities.Lead.Lead?> GetLeadComResponsavelAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Lead
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Where(l => l.Id == id);
            if (!includeDeleted)
                query = query.Where(l => !l.Excluido);
            return query.FirstOrDefaultAsync();
        }

        public async Task<List<Domain.Entities.Lead.Lead>> GetLeadsComResponsavelUsuarioPorIdsAsync(
            IEnumerable<int> leadIds, bool includeDeleted = false)
        {
            var idList = leadIds.Distinct().ToList();
            if (idList.Count == 0)
                return new List<Domain.Entities.Lead.Lead>();

            var query = _context.Lead
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Where(l => idList.Contains(l.Id));
            if (!includeDeleted)
                query = query.Where(l => !l.Excluido);
            return await query.ToListAsync();
        }

        public async Task<(List<Domain.Entities.Lead.Lead> Itens, int TotalItens)> ListarLeadsFiltradoAsync(
            int? origemId,
            int? statusId,
            int? usuarioId,
            DateTime? dataCadastroInicio,
            DateTime? dataCadastroFim,
            string? nivelInteresse,
            int? pagina,
            int? tamanhoPagina,
            string? busca,
            int? empresaId,
            string? numeroWhatsapp
        )
        {
            var query = _context.Lead.Where(e => !e.Excluido).AsQueryable();

            if (empresaId > 0)
                query = query.Where(l => l.EmpresaId == empresaId.Value);

            if (origemId > 0)
                query = query.Where(l => l.OrigemId == origemId.Value);

            if (statusId > 0)
                query = query.Where(l => l.LeadStatusId == statusId.Value);

            if (usuarioId > 0)
            {
                query = query
                    .Include(x => x.Responsavel)
                        .ThenInclude(x => x.Usuario)
                    .Where(x =>
                        x.Responsavel != null &&
                        x.Responsavel.UsuarioId == usuarioId.Value &&
                        !x.Responsavel.Excluido);
            }

            if (dataCadastroInicio.HasValue)
                query = query.Where(l => l.DataCriacao.Date >= dataCadastroInicio.Value.Date);

            if (dataCadastroFim.HasValue)
                query = query.Where(l => l.DataCriacao.Date <= dataCadastroFim.Value.Date);

            if (!string.IsNullOrWhiteSpace(nivelInteresse))
                query = query.Where(l => l.NivelInteresse == nivelInteresse);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(l => l.Nome.Contains(busca));

            if (!string.IsNullOrWhiteSpace(numeroWhatsapp))
                query = query.Where(l => l.WhatsappNumero != null && l.WhatsappNumero.Contains(numeroWhatsapp));

            var totalItens = await query.CountAsync();

            var queryOrdenada = query
                .OrderByDescending(l => l.DataCriacao)
                .Include(l => l.LeadStatus)
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m!.Usuario)
                .Include(l => l.Origem)
                .Include(x => x.Equipe)
                .Include(l => l.Empresa)
                .Include(l => l.Equipe).ThenInclude(e => e.ResponsavelMembro);

            IQueryable<Domain.Entities.Lead.Lead> queryFinal = queryOrdenada;

            if (pagina.HasValue && tamanhoPagina.HasValue && pagina > 0 && tamanhoPagina > 0)
            {
                int paginaSeguro = pagina.Value;
                int tamanhoSeguro = tamanhoPagina.Value;

                queryFinal = queryOrdenada
                    .Skip((paginaSeguro - 1) * tamanhoSeguro)
                    .Take(tamanhoSeguro);
            }

            var itens = await queryFinal.ToListAsync();

            return (itens, totalItens);
        }

        public async Task<(List<Domain.Entities.Lead.Lead> Itens, int TotalItens)> ListarLeadsFiltradoAsync(
            int? leadId,
            List<int>? empresasId,
            int? equipeId,
            int? usuarioIdLogado,
            bool meusLeads,
            List<int>? responsavelIds,
            bool? comOportunidades,
            List<int>? statusIds,
            List<int>? origemIds,
            DateTime? dataInicio,
            DateTime? dataFim,
            bool? comConversasAtivas,
            bool? comMensagensNaoLidas,
            bool? aguardandoResposta,
            string? whatsApp,
            string? email,
            string? cpf,
            string? textoBusca,
            int? pagina,
            int? tamanhoPagina,
            string orderBy,
            int statusEncerrado)
        {
            try
            {
                // 1. Query base
                var query = _context.Lead
                    .Include(l => l.LeadStatus)
                    .Include(l => l.Origem)
                        .ThenInclude(o => o.OrigemTipo)
                    .Include(l => l.Responsavel)
                        .ThenInclude(l => l.Usuario)
                    .Include(l => l.Oportunidades)
                    .Include(l => l.Conversas)
                        .ThenInclude(c => c.Mensagens)
                    .Include(l => l.Empresa)
                    .Include(l => l.Equipe)
                        .ThenInclude(e => e.ResponsavelMembro)
                    .Where(l => !l.Excluido)
                    .AsQueryable();

                if (leadId is > 0)
                {
                    query = query.Where(l => l.Id == leadId);
                    var itensUnico = await query.ToListAsync();
                    return (itensUnico, itensUnico.Count);
                }

                if (empresasId != null && empresasId.Any())
                {
                    query = query.Where(l => empresasId.Contains(l.EmpresaId));
                }

                if (equipeId is > 0)
                {
                    query = query.Where(l => l.EquipeId == equipeId);
                }

                // 2. Filtro "Meus Leads"
                if (meusLeads && usuarioIdLogado.HasValue)
                {
                    query = query.Where(l => l.Responsavel.UsuarioId == usuarioIdLogado.Value);
                }
                else if (responsavelIds?.Any() == true)
                {
                    query = query.Where(l => responsavelIds.Contains(l.Responsavel.UsuarioId));
                }

                // 3. Filtro de Status
                if (statusIds?.Any() == true)
                {
                    query = query.Where(l => statusIds.Contains(l.LeadStatusId));
                }

                // 4. Filtro de Origem
                if (origemIds?.Any() == true)
                {
                    query = query.Where(l => origemIds.Contains(l.OrigemId));
                }

                // 5. Filtro de Período (Data de Criação)
                if (dataInicio.HasValue)
                {
                    query = query.Where(l => l.DataCriacao >= dataInicio.Value.Date);
                }
                if (dataFim.HasValue)
                {
                    var fim = dataFim.Value.Date.AddDays(1);
                    query = query.Where(l => l.DataCriacao < fim);
                }

                // 6. Filtro de Oportunidades
                if (comOportunidades.HasValue)
                {
                    if (comOportunidades.Value)
                    {
                        query = query.Where(l => l.Oportunidades.Any());
                    }
                    else
                    {
                        query = query.Where(l => !l.Oportunidades.Any());
                    }
                }

                // 7. Filtros de Conversas
                if (comConversasAtivas.HasValue && comConversasAtivas.Value)
                {
                    var statusAtivoId = await _context.Set<ConversaStatus>()
                        .Where(s => s.Codigo == "ATIVA")
                        .Select(s => s.Id)
                        .FirstOrDefaultAsync();

                    query = query.Where(l => l.Conversas.Any(c => c.StatusId == statusAtivoId));
                }

                if (comMensagensNaoLidas.HasValue && comMensagensNaoLidas.Value)
                {
                    query = query.Where(l => l.Conversas.Any(c => c.PossuiMensagensNaoLidas) && l.Conversas.Any(c => c.StatusId != statusEncerrado));
                }

                if (aguardandoResposta.HasValue && aguardandoResposta.Value)
                {
                    query = query.Where(l =>

                        !l.Conversas.Any()
                        ||
                        l.Conversas.Any(c => c.StatusId != statusEncerrado && c.Mensagens.Any() &&
                             (
                                 c.Mensagens
                                     .OrderByDescending(m => m.DataEnvio)
                                     .Select(m => new { m.Sentido, NomeUsuario = m.Usuario.Nome })
                                     .FirstOrDefault().Sentido == 'R'
                                 ||
                                 c.Mensagens
                                     .OrderByDescending(m => m.DataEnvio)
                                     .Select(m => new { m.Sentido, NomeUsuario = m.Usuario.Nome })
                                     .FirstOrDefault().NomeUsuario == "BOT"
                             )
                         )
                     );
                }

                // 8. Filtros de Identificadores
                if (!string.IsNullOrWhiteSpace(whatsApp))
                {
                    var whatsAppLimpo = whatsApp.Trim();
                    query = query.Where(l => l.WhatsappNumero != null && l.WhatsappNumero.Contains(whatsAppLimpo));
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    var emailLimpo = email.Trim().ToLower();
                    query = query.Where(l => l.Email != null && l.Email.ToLower().Contains(emailLimpo));
                }

                if (!string.IsNullOrWhiteSpace(cpf))
                {
                    var cpfLimpo = new string(cpf.Where(char.IsDigit).ToArray());
                    query = query.Where(l => l.CPF != null && l.CPF.Contains(cpfLimpo));
                }

                // 9. Busca Textual Global
                if (!string.IsNullOrWhiteSpace(textoBusca))
                {
                    var termo = textoBusca.Trim().ToLower();
                    query = query.Where(l =>
                        (l.Nome != null && l.Nome.ToLower().Contains(termo)) ||
                        (l.Email != null && l.Email.ToLower().Contains(termo)) ||
                        (l.WhatsappNumero != null && l.WhatsappNumero.Contains(termo)) ||
                        (l.CPF != null && l.CPF.Contains(termo)) ||
                        (l.NomeEmpresa != null && l.NomeEmpresa.ToLower().Contains(termo))
                    );
                }

                // 10. Contar total ANTES da paginação
                var totalItens = await query.CountAsync();

                // 11. Aplicar ordenação
                query = AplicarOrdenacao(query, orderBy);

                IQueryable<Domain.Entities.Lead.Lead> queryFinal = query;

                if (pagina.HasValue && tamanhoPagina.HasValue && pagina > 0 && tamanhoPagina > 0)
                {
                    int paginaSeguro = pagina.Value;
                    int tamanhoSeguro = tamanhoPagina.Value;

                    queryFinal = query
                        .Skip((paginaSeguro - 1) * tamanhoSeguro)
                        .Take(tamanhoSeguro);
                }

                var itens = await queryFinal.ToListAsync();

                return (itens, totalItens);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IQueryable<Domain.Entities.Lead.Lead> AplicarOrdenacao(IQueryable<Domain.Entities.Lead.Lead> query, string orderBy)
        {
            return orderBy?.ToLower() switch
            {
                "datacriacao_asc" => query.OrderBy(l => l.DataCriacao),
                "datacriacao_desc" => query.OrderByDescending(l => l.DataCriacao),
                "nome_asc" => query.OrderBy(l => l.Nome),
                "nome_desc" => query.OrderByDescending(l => l.Nome),
                "status_asc" => query.OrderBy(l => l.LeadStatus.Ordem).ThenBy(l => l.DataCriacao),
                "status_desc" => query.OrderByDescending(l => l.LeadStatus.Ordem).ThenByDescending(l => l.DataCriacao),
                _ => query.OrderByDescending(l => l.DataCriacao) // Padrão
            };
        }

        public async Task<List<LeadStatus>> ListarStatusAsync()
        {
            return await _context.Set<LeadStatus>()
                .Where(s => !s.Excluido)
                .OrderBy(s => s.Ordem)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém leads pendentes de distribuição para uma empresa
        /// </summary>
        public async Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPendentesDistribuicaoAsync(int empresaId, int maxLeads)
        {
            _logger.LogDebug("Obtendo leads pendentes para distribuição. Empresa: {EmpresaId}, Max: {MaxLeads}",
                empresaId, maxLeads);

            return await _context.Set<Domain.Entities.Lead.Lead>()
                .Where(l => l.EmpresaId == empresaId &&
                          !l.Excluido &&
                          l.ResponsavelId == null)
                .OrderBy(l => l.DataCriacao)
                .Take(maxLeads)
                .ToListAsync();
        }

        /// <summary>
        /// Conta o total de leads distribuídos para uma empresa
        /// </summary>
        public async Task<int> CountLeadsDistribuidosAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            _logger.LogDebug("Contando leads distribuídos. Empresa: {EmpresaId}, Período: {DataInicio} a {DataFim}",
                empresaId, dataInicio, dataFim);

            if (empresaId <= 0)
                throw new InfraException("ID da empresa deve ser maior que zero");

            var query = _context.Set<HistoricoDistribuicao>()
                .Include(h => h.ConfiguracaoDistribuicao)
                .Where(h => h.ConfiguracaoDistribuicao.EmpresaId == empresaId && !h.Excluido);

            if (dataInicio.HasValue)
                query = query.Where(h => h.DataExecucao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(h => h.DataExecucao <= dataFim.Value);

            var totalLeads = await query.SumAsync(h => h.TotalLeadsDistribuidos);
            return totalLeads;
        }

        public async Task<bool> ExisteLeadAtribuidoAsync(int equipeId, int? membroId = null)
        {
            var query = _context.Lead
                .AsNoTracking()
                .Where(l => l.EquipeId == equipeId && l.ResponsavelId != null && !l.Excluido);

            if (membroId.HasValue)
                query = query.Where(l => l.ResponsavelId == membroId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<Domain.Entities.Lead.Lead>> GetListLeadExportAsync(int empresaId, int? equipeId, int? usuarioId, int? statusId, DateTime? de, DateTime? ate)
        {
            var query = _context.Lead
                .Include(l => l.LeadStatus)
                .Include(l => l.Responsavel)
                    .ThenInclude(m => m.Usuario)
                .Include(l => l.Empresa)
                .Include(l => l.Origem)
                .Include(l => l.Equipe)
                .Where(l => !l.Excluido && l.EmpresaId == empresaId)
                .AsQueryable();

            if (equipeId is > 0) //quando recebermos equipe o responsavel é um membro da equipe
            {
                query = query.Where(l => l.EquipeId == equipeId);
                if (usuarioId is > 0)
                {
                    query = query.Where(l => l.ResponsavelId == usuarioId);
                }
            }
            else if (usuarioId is > 0) //quando não recebermos equipe o responsavel é o usuario
            {
                query = query.Where(l => l.Responsavel.UsuarioId == usuarioId);
            }

            if (statusId is > 0)
            {
                query = query.Where(l => l.LeadStatusId == statusId);
            }
            if (de.HasValue)
            {
                query = query.Where(l => l.DataCriacao >= de.Value.Date);
            }
            if (ate.HasValue)
            {
                var fim = ate.Value.Date.AddDays(1);
                query = query.Where(l => l.DataCriacao < fim);
            }
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Domain.Entities.Lead.Lead?> GetLeadsComResponsavelAsync(int id)
        {
            return await _context.Lead
                .Include(l => l.Responsavel)
               .FirstOrDefaultAsync(l => l.Id == id && !l.Excluido);
        }
    }
}
