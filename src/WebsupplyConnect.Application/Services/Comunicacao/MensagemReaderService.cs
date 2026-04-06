using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class MensagemReaderService(IMensagemRepository mensagemRepository, ILogger<MensagemReaderService> logger, ITemplateRepository templateRepository, IMidiaReaderService midiaReaderService, IConversaReaderService conversaReaderService) : IMensagemReaderService
    {

        private readonly IMensagemRepository _mensagemRepository = mensagemRepository ?? throw new ArgumentNullException(nameof(mensagemRepository));
        private readonly ITemplateRepository _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(_templateRepository));
        private readonly IMidiaReaderService _midiaReaderService = midiaReaderService ?? throw new ArgumentNullException(nameof(midiaReaderService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly ILogger<MensagemReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> MensagemExistsAsync(int id)
        {
            try
            {
                return await _mensagemRepository.ExistsInDatabaseAsync<Mensagem>(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se mensagem {id} existe.", id);
                throw;
            }
            ;
        }

        public async Task<List<MensagemStatus>> GetListMensagemStatusAsync()
        {
            try
            {
                return await _mensagemRepository.GetListByPredicateAsync<MensagemStatus>(e => true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lista de status da mensage.");
                throw;
            }
            ;
        }

        public async Task<List<MensagemTipo>> GetListMensagemTiposAsync()
        {
            try
            {
                return await _mensagemRepository.GetListByPredicateAsync<MensagemTipo>(e => true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retornar lista de tipos de mensagems.");
                throw;
            }
            ;
        }
        public async Task<MensagemStatus> GetMensagensStatusAsync(int? id, string? codigo)
        {
            if (id == null && string.IsNullOrWhiteSpace(codigo))
                throw new AppException("É necessário informar pelo menos o ID ou o código do status da mensagem.");

            try
            {
                var resultado = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(
                    e => e.Id == id || e.Codigo == codigo);

                return resultado ?? throw new AppException("Status da mensagem não encontrado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status da mensagem pelo id {id} ou código {codigo}", id, codigo);
                throw;
            }
        }
        public async Task<MensagemTipo> GetMensagemTipoAsync(int id)
        {
            try
            {
                return await _mensagemRepository.GetByIdAsync<MensagemTipo>(id, false) ?? throw new AppException($"Erro ao encontrar o tipo de mensagem pelo id: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipo da mensagem com id {id}", id);
                throw;
            }
        }

        public async Task<MensagemTipo> GetMensagemTipoByCodigoAsync(string codigo)
        {
            try
            {
                return await _mensagemRepository.GetByPredicateAsync<MensagemTipo>(e => e.Codigo == codigo) ?? throw new AppException($"Erro ao encontrar o tipo de mensagem pelo codigo: {codigo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipo da mensagem pelo código {codigo}", codigo);
                throw;
            }
        }

        public async Task<List<Mensagem>> GetMensagensNaoLidasByConversaAsync(int conversaId, int statusId)
        {
            try
            {
                return await _mensagemRepository.GetMensagensNaoLidasByConversaAsync(conversaId, statusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens não lidas");
                throw;
            }
        }

        public async Task<int?> GetQntdMensagensNaoLidasByConversaAsync(int conversaId, string status)
        {
            try
            {
                var statusEntregue = await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(e => e.Codigo == status);
                return await _mensagemRepository.GetQntdMensagensNaoLidasByConversaAsync(conversaId, statusEntregue.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens não lidas");
                throw;
            }
        }

        public async Task<Mensagem?> GetUltimaMensagemByConversaAsync(int conversaId, bool includeDeleted = false)
        {
            try
            {
                return await _mensagemRepository.GetUltimaMensagemByConversaAsync(conversaId, includeDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar última mensagem da conversa {id}.", conversaId);
                throw;
            }
        }

        public async Task<Mensagem> GetMensagemByIdAsync(int id)
        {
            try
            {
                return await _mensagemRepository.GetByIdAsync<Mensagem>(id) ?? throw new AppException($"Erro ao encontrar a mensagem pelo id: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagem por id {id}.", id);
                throw;
            }
        }
        public async Task<Mensagem> GetMensagemByIdMeta(string idMeta)
        {
            try
            {
                return await _mensagemRepository.GetByPredicateAsync<Mensagem>(
                                 w => w.IdExternoMeta == idMeta) ?? throw new AppException($"Erro em buscar mensagem pelo id externo meta: {idMeta}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagem por id meta {id}", idMeta);
                throw;
            }
        }

        public async Task<MensagemStatus> GetMensagemStatusByCodigo(string codigo)
        {
            try
            {
                return await _mensagemRepository.GetByPredicateAsync<MensagemStatus>(
                                 w => w.Codigo == codigo) ?? throw new AppException($"Erro em buscar status da mensagem pelo código: {codigo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagem status pelo código {codigo}.", codigo);
                throw;
            }
        }

        public async Task<Mensagem?> GetUltimaMensagemByConversaIdAsync(int conversaId)
        {
            try
            {
                return await _mensagemRepository.GetUltimaMensagemByConversaIdAsync(conversaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar última mensagen pela conversa id. ConversaId: {ConversaId}", conversaId);
                throw;
            }
        }

        public async Task<List<MensagemDTO>> GetMensagensRecentesSemAviso(int conversaId, int? quantidadeInicio, int? quantidadeFim)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                if ((quantidadeInicio.HasValue && !quantidadeFim.HasValue) ||
                   (!quantidadeInicio.HasValue && quantidadeFim.HasValue))
                {
                    throw new AppException("Ambos os campos 'quantidadeInicial' e 'quantidadeFinal' devem ser informados juntos.");
                }

                var mensagens = await _mensagemRepository.GetMessagesFromDateForSync(conversaId, quantidadeInicio, quantidadeFim, includeEhAviso: false);

                var conversa = await _conversaReaderService.GetConversaByIdAsync(conversaId);

                if (mensagens == null || mensagens.Count == 0)
                {
                    return [];
                }

                var mensagensDTO = new List<MensagemDTO>();

                foreach (var mensagem in mensagens)
                {
                    try
                    {
                        MensagemStatus? status = null;

                        if ((mensagem.StatusId.HasValue && mensagem.StatusId.Value != 0))
                        {
                            status = await _mensagemRepository.GetByIdAsync<MensagemStatus>(mensagem.StatusId.Value);
                        }

                        var tipo = await _mensagemRepository.GetByIdAsync<MensagemTipo>(mensagem.TipoId);

                        Midia? midia = null;

                        if (tipo != null && tipo.Codigo?.ToLowerInvariant() != "text")
                        {
                            midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(mensagem.Id);
                        }

                        var mensagemDTO = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Midia = (tipo?.Codigo?.ToLowerInvariant() ?? "") != "text",
                            File = midia?.UrlStorage,
                            MidiaId = midia?.Id,
                            Template = mensagem.Template != null,
                            TemplateId = mensagem.TemplateId,
                            Conteudo = mensagem.Conteudo,
                            TipoMensagem = tipo?.Codigo ?? throw new AppException($"Erro em buscar o tipo da mensagem. Conversa: {conversaId}"),
                            MensagemStatus = status?.Nome ?? throw new AppException($"Erro ao obter status da mensagem. Mensagem: {mensagem.Id}"),
                            DataEnvio = mensagem.DataEnvio!.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = conversa.LeadId,
                            UsuarioId = conversa.UsuarioId
                        };

                        mensagensDTO.Add(mensagemDTO);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar mensagem ID {MensagemId}", mensagem.Id);
                    }
                }

                return mensagensDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar mensagens recentes. ConversaId: {ConversaId}.",
                    conversaId);
                throw;
            }
        }


        public async Task<List<MensagemDTO>> GetMensagensRecentesAsync(int conversaId, int? quantidadeInicio, int? quantidadeFim, DateTime? dataInicio = null)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                if ((quantidadeInicio.HasValue && !quantidadeFim.HasValue) ||
                   (!quantidadeInicio.HasValue && quantidadeFim.HasValue))
                {
                    throw new AppException("Ambos os campos 'quantidadeInicial' e 'quantidadeFinal' devem ser informados juntos.");
                }

                if (dataInicio.HasValue && dataInicio.Value > DateTime.Now)
                    throw new AppException("Data da última mensagem não pode ser maior que data e hora atual");

                var mensagens = await _mensagemRepository.GetMessagesFromDateForSync(conversaId, quantidadeInicio, quantidadeFim, dataInicio, true);

                var conversa = await _conversaReaderService.GetConversaByIdAsync(conversaId);

                if (mensagens == null || !mensagens.Any())
                {
                    return [];
                }

                var mensagensDTO = new List<MensagemDTO>();

                foreach (var mensagem in mensagens)
                {
                    try
                    {
                        MensagemStatus? status = null;

                        if ((mensagem.StatusId.HasValue && mensagem.StatusId.Value != 0))
                        {
                            status = await _mensagemRepository.GetByIdAsync<MensagemStatus>(mensagem.StatusId.Value);
                        }

                        var tipo = await _mensagemRepository.GetByIdAsync<MensagemTipo>(mensagem.TipoId);

                        Midia? midia = null;

                        if (tipo != null && tipo.Codigo?.ToLowerInvariant() != "text" && tipo.Codigo?.ToLowerInvariant() != "unknown")
                        {
                            midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(mensagem.Id);
                        }

                        var mensagemDTO = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Midia = (tipo?.Codigo?.ToLowerInvariant() ?? "") != "text",
                            File = midia?.UrlStorage,
                            MidiaId = midia?.Id,
                            Template = mensagem.Template != null,
                            TemplateId = mensagem.TemplateId,
                            Conteudo = mensagem.Conteudo,
                            TipoMensagem = tipo?.Codigo ?? throw new AppException($"Erro em buscar o tipo da mensagem. Conversa: {conversaId}"),
                            MensagemStatus = status?.Nome ?? throw new AppException($"Erro ao obter status da mensagem. Mensagem: {mensagem.Id}"),
                            DataEnvio = mensagem.DataEnvio!.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = conversa.LeadId,
                            UsuarioId = conversa.UsuarioId
                        };

                        mensagensDTO.Add(mensagemDTO);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar mensagem ID {MensagemId}", mensagem.Id);
                    }
                }
                return mensagensDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar mensagens recentes. ConversaId: {ConversaId}, DataInicio: {DataInicio}",
                    conversaId, dataInicio);
                throw;
            }
        }
        public async Task<List<MensagemDTO>> GetMensagensAntigasAsync(DateTime dataLimite, int conversaId, int? pageSize)
        {
            try
            {
                if (conversaId <= 0)
                    throw new AppException("ID da conversa deve ser maior que zero.");

                if (dataLimite > DateTime.Now)
                    throw new AppException("Data limite não pode ser futura.");

                if (pageSize <= 0)
                    pageSize = 30;

                var mensagens = await _mensagemRepository.GetOldMessages(dataLimite, conversaId, pageSize);

                var conversa = await _conversaReaderService.GetConversaByIdAsync(conversaId);
                if (mensagens == null || !mensagens.Any())
                {
                    return [];
                }

                var mensagensDTO = new List<MensagemDTO>();
                foreach (var mensagem in mensagens)
                {

                    MensagemStatus? status = null;

                    if ((mensagem.StatusId.HasValue && mensagem.StatusId.Value != 0))
                    {
                        status = await _mensagemRepository.GetByIdAsync<MensagemStatus>(mensagem.StatusId.Value);
                    }

                    var tipo = await _mensagemRepository.GetByIdAsync<MensagemTipo>(mensagem.TipoId);

                    Midia? midia = null;

                    if (tipo != null && tipo.Codigo?.ToLowerInvariant() != "text")
                    {
                        midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(mensagem.Id);
                    }

                    var mensagemDTO = new MensagemDTO
                    {
                        MensagemId = mensagem.Id,
                        Midia = (tipo?.Codigo?.ToLowerInvariant() ?? "") != "text",
                        File = null,
                        MidiaId = midia?.Id,
                        Template = mensagem.Template != null,
                        TemplateId = mensagem.TemplateId,
                        Conteudo = mensagem.Conteudo,
                        TipoMensagem = tipo?.Codigo ?? throw new AppException($"Erro em buscar o tipo da mensagem. Conversa: {conversaId}"),
                        DataEnvio = mensagem.DataEnvio!.Value,
                        TipoRemetente = mensagem.Sentido,
                        LeadId = conversa.LeadId,
                        UsuarioId = conversa.UsuarioId
                    };


                    mensagensDTO.Add(mensagemDTO);
                }
                return mensagensDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao buscar mensagens antigas. ConversaId: {ConversaId}, DataLimite: {DataLimite}",
                    conversaId, dataLimite);
                throw;
            }
        }

        public async Task<List<MensagemDTO>> GetTodasMensagens(int conversaId)
        {
            try
            {
                var mensagens = await _mensagemRepository.GetListByPredicateAsync<Mensagem>(c => c.Conversa.Id == conversaId);
                var conversa = await _conversaReaderService.GetConversaByIdAsync(conversaId);

                var mensagensDTO = new List<MensagemDTO>();

                foreach (var mensagem in mensagens)
                {

                    MensagemStatus? status = null;

                    if ((mensagem.StatusId.HasValue && mensagem.StatusId.Value != 0))
                    {
                        status = await _mensagemRepository.GetByIdAsync<MensagemStatus>(mensagem.StatusId.Value);
                    }

                    var tipo = await _mensagemRepository.GetByIdAsync<MensagemTipo>(mensagem.TipoId);

                    Midia? midia = null;

                    if (tipo != null && tipo.Codigo?.ToLowerInvariant() != "text")
                    {
                        midia = await _midiaReaderService.GetMidiaByMensagemIdAsync(mensagem.Id);
                    }

                    var mensagemDTO = new MensagemDTO
                    {
                        MensagemId = mensagem.Id,
                        Midia = (tipo?.Codigo?.ToLowerInvariant() ?? "") != "text",
                        File = midia?.UrlStorage,
                        MidiaId = midia?.Id,
                        Template = mensagem.Template != null,
                        TemplateId = mensagem.TemplateId,
                        Conteudo = mensagem.Conteudo,
                        TipoMensagem = tipo?.Codigo ?? throw new AppException($"Erro em buscar o tipo da mensagem. Conversa: {conversaId}"),
                        MensagemStatus = status?.Nome ?? throw new AppException($"Erro ao obter status da mensagem. Mensagem: {mensagem.Id}"),
                        DataEnvio = mensagem.DataEnvio!.Value,
                        TipoRemetente = mensagem.Sentido,
                        LeadId = conversa.LeadId,
                        UsuarioId = conversa.UsuarioId
                    };

                    mensagensDTO.Add(mensagemDTO);

                }
                return mensagensDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todas as mensagens da conversa {ConversaId}", conversaId);
                throw;
            }
        }

        public async Task<List<Mensagem>> ObterMensagensPorConversaIdParaETLAsync(int conversaId, char? sentido = null)
        {
            if (!sentido.HasValue)
            {
                return await _mensagemRepository.GetListByPredicateAsync<Mensagem>(m => m.ConversaId == conversaId, includeDeleted: true);
            }
            else
            {
                return await _mensagemRepository.GetListByPredicateAsync<Mensagem>(m => m.ConversaId == conversaId && m.Sentido == sentido, includeDeleted: true);
            }
        }

        public async Task<List<int>> ObterLeadIdsComMensagensNoPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim)
        {
            var mensagens = await _mensagemRepository.GetListByPredicateAsync<Mensagem>(
                m => m.DataCriacao >= dataInicio && m.DataCriacao <= dataFim, includeDeleted: true);

            if (mensagens.Count == 0) return new List<int>();

            var conversaIds = mensagens.Select(m => m.ConversaId).Distinct().ToList();
            var conversas = await _mensagemRepository.GetListByPredicateAsync<Conversa>(
                c => conversaIds.Contains(c.Id), includeDeleted: true);

            return conversas.Select(c => c.LeadId).Distinct().ToList();
        }

        public async Task<Dictionary<int, Mensagem?>> GetUltimasMensagensByListConversasAsync(List<int> conversaIds)
        {
            try
            {
                return await _mensagemRepository.GetUltimasMensagensByListConversasAsync(conversaIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar últimas mensagens por lista de conversas. ConversaIds: {ConversaIds}", conversaIds);
                throw;
            }
        }
    }
}
