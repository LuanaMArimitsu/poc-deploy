using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Comunicacao
{
    public class MensagemRepository(WebsupplyConnectDbContext websupplyConnectDb, IUnitOfWork unitOfWork) : BaseRepository(websupplyConnectDb, unitOfWork), IMensagemRepository
    {

        public async Task<int> GetMensagemStatus(string codigo, bool includeDeleted = false)
        {
            var status = await GetByPredicateAsync<MensagemStatus>(
                             w => w.Codigo == codigo,
                             includeDeleted
            );

            return status?.Id ?? throw new InfraException($"MensagemStatus com código '{codigo}' não encontrado.");
        }

        public async Task<int> GetMensagemTipo(string codigo, bool includeDeleted = false)
        {
            var tipo = await GetByPredicateAsync<MensagemTipo>(
                            w => w.Codigo == codigo,
                            includeDeleted
           );

            return tipo?.Id ?? throw new InfraException($"MensagemTipo com código '{codigo}' não encontrado.");
        }

        //public async Task<List<Mensagem>> GetMessagesFromDateForSync(DateTime? dataUltimaMensagem, int conversaId)
        //{
        //    if (conversaId <= 0)
        //    {
        //        throw new InfraException("Código da conversa deve ser maior que 0");
        //    }

        //    if (dataUltimaMensagem.HasValue && dataUltimaMensagem.Value > DateTime.Now)
        //    {
        //        throw new InfraException("Data da última mensagem não pode ser maior que data e hora atual");
        //    }

        //    try
        //    {
        //        List<Mensagem> mensagens;

        //        if (dataUltimaMensagem == null)
        //        {
        //            mensagens = await GetListByPredicateAsync<Mensagem>(
        //                w => w.ConversaId == conversaId,
        //                q => q.OrderByDescending(m => m.DataEnvio)
        //            );
        //        }
        //        else
        //        {
        //            mensagens = await GetListByPredicateAsync<Mensagem>(
        //                w => w.DataEnvio > dataUltimaMensagem.Value && w.ConversaId == conversaId,
        //                q => q.OrderByDescending(m => m.DataEnvio)
        //            );
        //        }

        //        return mensagens;
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorMessage = dataUltimaMensagem == null
        //            ? $"Erro ao buscar lista completa de mensagens para sincronização da conversa {conversaId}"
        //            : $"Erro ao buscar mensagens para sincronização da conversa {conversaId} desde {dataUltimaMensagem.Value:yyyy-MM-dd HH:mm:ss}";

        //        throw new InfraException(errorMessage, ex);
        //    }
        //}
        public async Task<List<Mensagem>> GetMessagesFromDateForSync(
            int conversaId,
            int? quantidadeInicio = null,
            int? quantidadeFim = null,
            DateTime? dataUltimaMensagem = null,
            bool? includeEhAviso = false)
        {
            if (conversaId <= 0)
                throw new AppException("ID da conversa inválido.");

            if ((quantidadeInicio.HasValue && !quantidadeFim.HasValue) ||
                (!quantidadeInicio.HasValue && quantidadeFim.HasValue))
                throw new AppException("Ambos os campos 'quantidadeInicio' e 'quantidadeFim' devem ser informados juntos.");

            if (quantidadeInicio.HasValue && quantidadeFim.HasValue && quantidadeFim < quantidadeInicio)
                throw new AppException("'quantidadeFim' não pode ser menor que 'quantidadeInicio'.");

            try
            {
                var query = _context.Set<Mensagem>()
                    .Where(m => !m.Excluido && m.ConversaId == conversaId);

                if (dataUltimaMensagem.HasValue)
                {
                    query = query.Where(m => m.DataEnvio > dataUltimaMensagem.Value);
                }

                if (includeEhAviso.HasValue && includeEhAviso.Value == false)
                {
                    query = query.Where(m => m.EhAviso == false);
                }

                // Ordena da mais recente para a mais antiga
                query = query.OrderByDescending(m => m.DataEnvio).ThenByDescending(m => m.Id);

                // Aplica paginação por índice se necessário
                if (quantidadeInicio.HasValue && quantidadeFim.HasValue)
                {
                    var skip = quantidadeInicio.Value;
                    var take = quantidadeFim.Value - quantidadeInicio.Value + 1;

                    query = query.Skip(skip).Take(take);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new AppException("Erro ao buscar mensagens para sincronização.", ex);
            }
        }


        public async Task<List<MensagemTipo>> GetCodigoMessagesSync()
        {
            try
            {
                List<MensagemTipo> entidades = await GetListByPredicateAsync<MensagemTipo>(e => true);

                return entidades;
            }
            catch (Exception ex)
            {
                throw new InfraException($"Erro ao buscar os tipos da mensagem: {ex}");
            }
        }

        public async Task<List<Mensagem>> GetOldMessages(DateTime dataEnvio, int conversaId, int? pageSize = null)
        {
            if (dataEnvio == DateTime.MinValue)
            {
                throw new InfraException("Data de envio não pode ser MinValue");
            }

            if (conversaId <= 0)
            {
                throw new InfraException("Código da conversa deve ser maior que 0");
            }

            int size = pageSize ?? 30;

            if (size <= 0)
            {
                throw new InfraException("Tamanho da página deve ser maior que 0");
            }

            try
            {
                var mensagens = (await GetListByPredicateAsync<Mensagem>(
                    m => m.DataEnvio < dataEnvio && m.ConversaId == conversaId,
                    orderBy: q => q.OrderByDescending(m => m.DataEnvio)
                     )).Take(size).ToList();

                return mensagens;
            }
            catch (Exception ex)
            {
                throw new InfraException($"Erro ao buscar mensagens antigas", ex);
            }
        }

        public async Task<List<Mensagem>> GetMensagensNaoLidasByConversaAsync(int conversaId, int statusId)
        {
            var resultado = await GetListByPredicateAsync<Mensagem>(
                w => w.ConversaId == conversaId && w.StatusId == statusId
            );
            return resultado ?? [];
        }

        public async Task<int?> GetQntdMensagensNaoLidasByConversaAsync(int conversaId, int statusId)
        {
            var mensagensNaoLidas = await _context.Mensagem.Where(m => m.ConversaId == conversaId && m.StatusId == statusId && m.Sentido == 'R').CountAsync();
            return mensagensNaoLidas;
        }

        public async Task<Mensagem?> GetUltimaMensagemByConversaAsync(int conversaId, bool includeDeleted = false)
        {
            var mensagem = await _context.Mensagem.Where(m => m.ConversaId == conversaId)
                .OrderByDescending(m => m.Id).Include(m => m.Tipo)
                .FirstOrDefaultAsync();

            return mensagem;
        }

        public async Task<int> UpdateStatusMensagensClienteAsync(int conversaId, int novoStatusId)
        {
            var linhasAfetadas = await _context.Mensagem
                .Where(m => m.ConversaId == conversaId && m.StatusId != novoStatusId && m.Sentido == 'R')
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.StatusId, novoStatusId));

            return linhasAfetadas;
        }

        public async Task<Mensagem?> GetUltimaMensagemByConversaIdAsync(int conversaId)
        {
            return await _context.Mensagem
                .Where(m => m.ConversaId == conversaId && m.Sentido == 'R')
                .OrderByDescending(m => m.DataCriacao)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, Mensagem?>> GetUltimasMensagensByListConversasAsync(List<int> conversaIds)
        {
            var mensagens = await _context.Mensagem
                .Where(m => conversaIds.Contains(m.ConversaId))
                .GroupBy(m => m.ConversaId)
                .Select(g => g
                    .OrderByDescending(m => m.DataCriacao)
                    .FirstOrDefault())
                .ToListAsync();

            return mensagens
                .Where(m => m != null)
                .ToDictionary(m => m.ConversaId, m => m);
        }
    }
}
