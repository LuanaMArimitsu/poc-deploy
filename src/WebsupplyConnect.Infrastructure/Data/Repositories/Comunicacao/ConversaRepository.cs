using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    /// <summary>
    /// Construtor do repositório
    /// </summary>
    /// <param name="dbContext">Contexto do banco de dados</param>
    /// <param name="unitOfWork">Instância do UnitOfWork</param>
    internal class ConversaRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), IConversaRepository
    {
        public async Task<Conversa> GetConversaById(int conversaId, bool includeDeleted = false)
        {
            var resultado = await _context.Conversa
                .Include(l => l.Lead)
                .Where(u => u.Id == conversaId && u.Excluido == includeDeleted)
                .FirstOrDefaultAsync();

            return resultado ?? throw new InfraException($"Conversa com ID '{conversaId}' não encontrada.");
        }

        public async Task<bool> ExisteConversaNoCanalAsync(int usuarioId, int canalId, int statusEncerrado)
        {
            return await _context.Conversa
                .AnyAsync(c => c.UsuarioId == usuarioId && c.CanalId == canalId && !c.Excluido && c.StatusId != statusEncerrado);
        }

        public async Task<int> GetConversaStatusByCodeAsync(string codigo, bool includeDeleted = false)
        {
            var resultado = await GetByPredicateAsync<ConversaStatus>(
                 w => w.Codigo == codigo,
                 includeDeleted
            );

            return resultado?.Id ?? throw new InfraException($"ConversaStatus com código '{codigo}' não encontrado.");
        }

        public async Task<Conversa?> GetConversaByLeadAndCanalAsync(int leadId, int canalId, int statusIdExcluir, bool includeDeleted = false)
        {
            var resultado = await GetByPredicateAsync<Conversa>(
            w => w.LeadId == leadId && w.CanalId == canalId && w.StatusId != statusIdExcluir,
            includeDeleted
            );

            return resultado ?? null;
        }


        public async Task<Conversa?> GetUltimaConversaLead(int leadId, int equipeId)
        {
            //  conversa que já pertence à equipe
            var conversaEquipe = await _context.Conversa
                .Where(c => c.LeadId == leadId)
                .Where(c => c.Status.Codigo != "ATIVA")
                .Where(c => c.EquipeId == equipeId && !c.Usuario.IsBot)
                .Include(c => c.Lead)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.DataCriacao)
                .FirstOrDefaultAsync();

            if (conversaEquipe != null)
                return conversaEquipe;

            // Fallback — última conversa cujo responsável pertence à equipe
            var conversaPorMembro = await _context.Conversa
                .Where(c => c.LeadId == leadId)
                .Where(c => c.Status.Codigo != "ATIVA")
                .Where(c =>
                    c.Usuario.MembrosEquipe.Any(m =>
                        m.EquipeId == equipeId &&
                        !m.Usuario.IsBot))
                .Include(c => c.Lead)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.DataCriacao)
                .FirstOrDefaultAsync();

            return conversaPorMembro;
        }


        public async Task<Conversa?> GetConversaNaoEncerradasByLeadAAsync(int leadId, int statusEncerrado)
        {
            var resultado = await _context.Conversa.Where(c => c.LeadId == leadId && c.StatusId != statusEncerrado)
                .Include(c => c.Status).FirstOrDefaultAsync();

            return resultado ?? null;
        }

        public async Task<List<Conversa>> GetConversasByUsuarioAsync(int usuarioId, int statusIdExcluir)
        {
            //var resultado = await GetListByPredicateAsync<Conversa>(
            //    w => w.UsuarioId == usuarioId && w.StatusId != statusIdExcluir 
            //);
            var resultado = await _context.Conversa.Where(c => c.UsuarioId == usuarioId && c.StatusId != statusIdExcluir).OrderByDescending(c => c.Fixada).ToListAsync();
            return resultado;
        }

        public async Task AtualizarAsync(Conversa conversa)
        {
            Update(conversa);
            await Task.CompletedTask;
        }

        public async Task<bool> IsPrimeiraMensagemClienteAsync(int conversaId)
        {
            // Verifica se já existe alguma mensagem para essa conversa
            int totalMensagens = await _context.Mensagem
              .Where(m => m.ConversaId == conversaId)
              .CountAsync();

            // Se não tiver exatamente 1 mensagem, já retorna false
            if (totalMensagens != 1)
                return false;

            // Verifica se a única mensagem tem Sentido 'R'
            return await _context.Mensagem
                .AnyAsync(m => m.ConversaId == conversaId && m.Sentido == 'R');
        }

        public async Task<Mensagem> GetPrimeiraMensagemClienteAsync(int conversaId)
        {
            var mensagem = await _context.Mensagem
                .Where(m => m.ConversaId == conversaId)
                .OrderBy(m => m.DataEnvio)
                .FirstOrDefaultAsync();
            return mensagem ?? throw new InfraException($"Nenhuma mensagem de cliente encontrada para a conversa ID {conversaId}.");
        }
        public async Task<List<Conversa>> GetConversasEncerradasByUsuarioAsync(int usuarioId, int statusEncerradoId, int? indiceInicial = null, int? indiceFinal = null, int? empresaId = null)
        {
            var query = _context.Conversa
                .Where(c =>
                    c.UsuarioId == usuarioId &&
                    c.StatusId == statusEncerradoId &&
                    !c.Excluido &&
                    (!empresaId.HasValue || c.Lead.EmpresaId == empresaId.Value))
                .Include(c => c.Lead)
                .OrderByDescending(c => c.DataCriacao);

            if (!indiceInicial.HasValue || !indiceFinal.HasValue)
            {
                return await query.ToListAsync();
            }
            else
            {
                var quantidade = indiceFinal.Value - indiceInicial.Value;

                return await query
                    .Skip(indiceInicial.Value)
                    .Take(quantidade)
                    .ToListAsync();
            }
        }

        public async Task<(List<Conversa> conversas, int total)> GetConversasPaginadasByUsuarioAsync(int usuarioId, int statusEncerradoId, int? quantidadeInicial, int? quantidadeFinal, int? empresaId = null, int? equipeId = null)
        {
            try
            {
                var query = _context.Conversa.Where(c => c.UsuarioId == usuarioId && c.StatusId != statusEncerradoId);

                if (empresaId.HasValue)
                {
                    query = query.Where(c => c.Lead.EmpresaId == empresaId.Value);
                }

                if (equipeId.HasValue)
                {
                    query = query.Where(c => c.EquipeId == equipeId.Value);
                }

                var total = await query.CountAsync();

                if ((quantidadeInicial.HasValue && !quantidadeFinal.HasValue) ||
                    (!quantidadeInicial.HasValue && quantidadeFinal.HasValue))
                    throw new AppException("Ambos os campos 'quantidadeInicio' e 'quantidadeFim' devem ser informados juntos.");
                query = query.Include(c => c.Lead).Include(x => x.Status)
                             .OrderByDescending(c => c.DataCriacao);

                // Ambos os valores devem ser fornecidos antes de aplicar Skip e Take
                if (quantidadeInicial.HasValue && quantidadeFinal.HasValue)
                {
                    var skip = quantidadeInicial.Value;
                    var take = quantidadeFinal.Value - quantidadeInicial.Value;
                    query = query.Skip(skip).Take(take);
                }

                var conversas = await query.ToListAsync();

                return (conversas, total);
            }
            catch (Exception ex)
            {
                throw new InfraException("Erro ao obter conversas paginadas por usuário.", ex);
            }
        }

        public async Task<int> GetTotalConversasEncerradasByUsuarioAsync(int usuarioId, int statusEncerradoId)
        {
            return await _context.Conversa
                .Where(c => c.UsuarioId == usuarioId && c.StatusId == statusEncerradoId)
                .CountAsync();
        }

        public async Task<List<Conversa>> GetConversasComInatividade(int responsavelId, int pagina, int tamanhoPagina)
        {
            var tempoLimite = TimeHelper.GetBrasiliaTime().AddMinutes(-4);

            // Lista de IDs de conversas que atendem os critérios
            var conversaIds = await _context.Conversa
                .Where(c => c.UsuarioId == responsavelId
                            && !c.Excluido
                            && c.Status.Codigo != "ENCERRADA"
                            && c.DataUltimaMensagem <= tempoLimite)
                .Select(c => new
                {
                    c.Id,
                    UltimaMensagem = c.Mensagens
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault()
                })
                .Where(x => x.UltimaMensagem != null
                            && x.UltimaMensagem.Sentido == 'E'
                            && x.UltimaMensagem.EhAviso == false)
                .Select(x => x.Id)
                .ToListAsync();

            // Busca as conversas completas apenas dos IDs filtrados
            return await _context.Conversa
                .Where(c => conversaIds.Contains(c.Id))
                .Include(s => s.Status)
                .OrderBy(c => c.Id)
                .Skip(pagina * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();
        }

        // Conversas para ENCERRAR 
        public async Task<List<Conversa>> GetConversasComAviso(int responsavelId, int pagina, int tamanhoPagina)
        {
            var tempoLimite = TimeHelper.GetBrasiliaTime().AddMinutes(-4);

            var conversaIds = await _context.Conversa
                .Where(c => c.UsuarioId == responsavelId
                            && !c.Excluido
                            && c.Status.Codigo != "ENCERRADA")
                .Select(c => new
                {
                    c.Id,
                    UltimaMensagem = c.Mensagens
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault(),
                    UltimoAviso = c.Mensagens
                        .Where(m => m.EhAviso == true)
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault()
                })
                .Where(x => x.UltimoAviso != null
                            && x.UltimoAviso.DataCriacao <= tempoLimite
                            && x.UltimaMensagem.Id == x.UltimoAviso.Id)
                .Select(x => x.Id)
                .ToListAsync();

            return await _context.Conversa
                .Where(c => conversaIds.Contains(c.Id))
                .Include(s => s.Status)
                .OrderBy(c => c.Id)
                .Skip(pagina * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();
        }

        public async Task<List<Conversa>> GetConversasSemAtendimento(int pagina, int tamanhoPagina)
        {
            var agora = TimeHelper.GetBrasiliaTime();

            // Busca conversas que não estão encerradas, têm data da última mensagem e trazem as informações necessárias para a filtragem
            var conversasProjetadas = await _context.Conversa
                .Where(c => c.Status.Codigo != "ENCERRADA"
                            && c.DataUltimaMensagem != null)
                .Include(e => e.Lead)
                    .ThenInclude(r => r.Responsavel)
                    .ThenInclude(u => u.Usuario)
                .Select(c => new
                {
                    Conversa = c,
                    c.Lead.Responsavel,
                    c.Lead.Responsavel.Equipe,
                    LiderUsuarioId = c.Lead.Responsavel.Equipe.Membros
                        .Where(m => m.IsLider)
                        .Select(m => m.UsuarioId)
                        .FirstOrDefault(),
                    c.Lead.Responsavel.Equipe.TempoMaxSemAtendimento,
                    c.Lead.Responsavel.Equipe.TempoMaxDuranteAtendimento,
                    UltimaMensagem = c.Mensagens
                        .OrderByDescending(m => m.DataEnvio)
                        .Select(m => new
                        {
                            m.Sentido,
                            IsBot = m.Usuario != null ? (bool?)m.Usuario.IsBot : null
                        })
                        .FirstOrDefault(),
                    Mensagens = c.Mensagens
                        .Select(m => new
                        {
                            m.Sentido,
                            IsBot = m.Usuario != null ? (bool?)m.Usuario.IsBot : null
                        })
                        .ToList()
                })
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            // Aplica a filtragem complexa em memória, já que envolve várias propriedades e relações
            var conversasFiltradas = conversasProjetadas
                .Where(x =>
                {
                    if (x.Responsavel == null || x.Equipe == null)
                        return false;

                    if (x.UltimaMensagem == null)
                        return false;

                    bool ultimaMensagemDoCliente = x.UltimaMensagem.Sentido == 'R';
                    bool ultimaMensagemDeVendedorHumano = x.UltimaMensagem.Sentido != 'R'
                                                          && x.UltimaMensagem.IsBot == false;

                    // Se a última mensagem foi do vendedor humano, não transfere
                    if (ultimaMensagemDeVendedorHumano)
                        return false;

                    bool houveInteracaoVendedorHumano = x.Mensagens
                        .Any(m => m.Sentido != 'R' && m.IsBot == false);

                    TimeSpan? tempoAplicavel = houveInteracaoVendedorHumano
                       ? x.TempoMaxDuranteAtendimento
                       : x.TempoMaxSemAtendimento;

                    if (!tempoAplicavel.HasValue)
                        return false;

                    var minutosDesdeUltima =
                        (int)(agora - x.Conversa.DataUltimaMensagem!.Value).TotalMinutes;

                    var minutosLimite = tempoAplicavel.Value.TotalMinutes;

                    if (minutosDesdeUltima < minutosLimite)
                        return false;

                    // Não retorna se não houver líder
                    if (x.LiderUsuarioId == 0)
                        return false;

                    // Não retorna se o responsável atual já for o líder
                    if (x.Conversa.UsuarioId == x.LiderUsuarioId)
                        return false;

                    // Não retorna se o próprio responsável já é líder
                    if (x.Responsavel.IsLider)
                        return false;

                    return true;
                })
                .OrderBy(x => x.Conversa.DataUltimaMensagem)
                .Skip(pagina * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(x => x.Conversa)
                .ToList();

            return conversasFiltradas;
        }

        public async Task<(List<Conversa> Conversas, int Total)> GetConversasEncerradasByLeadAsync(
            int leadId,
            int statusEncerradoId,
            int? pagInicial = null,
            int? pagFinal = null)
        {

            var query = _context.Conversa
                .Where(c =>
                    c.LeadId == leadId &&
                    c.StatusId == statusEncerradoId &&
                    !c.Excluido);

            var total = await query.CountAsync();

            query = query
                .Include(c => c.Lead)
                .Include(c => c.Usuario)
                .Include(c => c.Equipe)
                .Include(c => c.Lead.Empresa)
                .OrderByDescending(c => c.DataCriacao);

            List<Conversa> conversas;

            if (!pagInicial.HasValue || !pagFinal.HasValue)
            {
                conversas = await query.ToListAsync();
            }
            else
            {
                var quantidade = pagFinal.Value - pagInicial.Value;

                conversas = await query
                    .Skip(pagInicial.Value)
                    .Take(quantidade)
                    .ToListAsync();
            }

            return (conversas, total);
        }

        public async Task<bool> ExisteConversaEncerradaPorLeadAsync(int leadId)
        {
            return await _context.Conversa
                .AnyAsync(c => c.LeadId == leadId && c.Status.Codigo == "ENCERRADA");
        }

        public async Task<Dictionary<int, (string? Contexto, DateTime? DataAtualizacaoContexto, bool TrocaDeContato, string? ClassificacaoIA)>> GetContextosByIdsAsync(
            IReadOnlyCollection<int> conversaIds)
        {
            if (conversaIds.Count == 0)
                return [];

            var registros = await _context.Conversa
                .AsNoTracking()
                .Where(c => !c.Excluido && conversaIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    Contexto = EF.Property<string?>(c, "Contexto"),
                    DataAtualizacaoContexto = EF.Property<DateTime?>(c, "DataAtualizacaoContexto"),
                    TrocaDeContato = EF.Property<bool>(c, "TrocaDeContato"),
                    ClassificacaoIA = EF.Property<string?>(c, "ClassificacaoIA")
                })
                .ToListAsync();

            return registros.ToDictionary(
                r => r.Id,
                r => (r.Contexto, r.DataAtualizacaoContexto, r.TrocaDeContato, r.ClassificacaoIA));
        }

        public async Task AtualizarContextoAsync(int conversaId, string? contexto, DateTime dataAtualizacaoContexto)
        {
            var conversa = await _context.Conversa
                .FirstOrDefaultAsync(c => c.Id == conversaId && !c.Excluido);

            if (conversa == null)
                return;

            var entry = _context.Entry(conversa);
            entry.Property("Contexto").CurrentValue = contexto;
            entry.Property("DataAtualizacaoContexto").CurrentValue = dataAtualizacaoContexto;
        }

        public async Task AtualizarClassificacaoAsync(int conversaId, bool trocaDeContato, string? classificacaoIA, DateTime dataAtualizacao)
        {
            var conversa = await _context.Conversa
                .FirstOrDefaultAsync(c => c.Id == conversaId && !c.Excluido);

            if (conversa == null)
                return;

            var entry = _context.Entry(conversa);
            entry.Property("TrocaDeContato").CurrentValue = trocaDeContato;
            entry.Property("ClassificacaoIA").CurrentValue = classificacaoIA;
            entry.Property("DataAtualizacaoContexto").CurrentValue = dataAtualizacao;
        }

        public async Task<Conversa?> GetConversaParaClassificacaoPorIdAsync(int conversaId)
        {
            return await _context.Conversa
                .Where(c => c.Id == conversaId && !c.Excluido)
                .Include(c => c.Lead)
                    .ThenInclude(l => l!.Empresa)
                .Include(c => c.Mensagens)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> JanelaAbertaDaConversaAsync(int conversaId, int statusEncerrada)
        {
            var ultimaMensagemRecebida = await _context.Conversa
                .Where(c => c.Id == conversaId
                    && c.StatusId != statusEncerrada
                    && !c.Excluido)
                .Select(c => c.Mensagens
                    .Where(m => m.Sentido == 'R')
                    .OrderByDescending(m => m.DataEnvio)
                    .Select(m => m.DataEnvio)
                    .FirstOrDefault())
                .FirstOrDefaultAsync();

            if (ultimaMensagemRecebida == null)
                return false;

            return (TimeHelper.GetBrasiliaTime() - ultimaMensagemRecebida.Value).TotalHours <= 24;
        }

        public async Task<int> GetQuantidadeConversasFixadasAsync(int conversaId, int usuarioId)
        {
            return await _context.Conversa.CountAsync(c => c.UsuarioId == usuarioId && c.Fixada);
        }
    }
}
