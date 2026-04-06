using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.DTOs.Lead.Campanha;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.DTOs.Redis;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadReponsavelWriterService(ILogger<LeadReponsavelWriterService> logger, IUsuarioEmpresaReaderService usuarioEmpresaReaderService, IMembroEquipeReaderService membroEquipeReaderService, IDistribuicaoWriterService distribuicaoWriterService, ILeadWriterService leadWriterService, ILeadReaderService leadReaderService, IRedisCacheService redisCacheService, ICampanhaReaderService campanhaReaderService, ICampanhaWriterService campanhaWriterService, IEmpresaReaderService empresaReaderService, IMembroEquipeWriterService membroEquipeWriterService, IEquipeReaderService equipeReaderService, IUsuarioReaderService usuarioReaderService, IOrigemReaderService origemReaderService, ILeadEventoWriterService leadEventoWriterService, INotificacaoClient notificacaoClient, ITemplateReaderService templateReaderService, ICanalReaderService canalReaderService, ITemplateWriterService templateWriterService, IConversaReaderService conversaReaderService, IUnitOfWork unitOfWork, IConversaWriterService conversaWriterService, IMensagemWriterService mensagemWriterService) : ILeadResponsavelWriterService
    {
        private readonly ILogger<LeadReponsavelWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly ILeadWriterService _leadWriterService = leadWriterService ?? throw new ArgumentNullException(nameof(leadWriterService));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IDistribuicaoWriterService _distribuicaoWriterService = distribuicaoWriterService ?? throw new ArgumentNullException(nameof(distribuicaoWriterService));
        private readonly IRedisCacheService _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        private readonly ICampanhaReaderService _campanhaReaderService = campanhaReaderService ?? throw new ArgumentNullException(nameof(campanhaReaderService));
        private readonly ICampanhaWriterService _campanhaWriterService = campanhaWriterService ?? throw new ArgumentNullException(nameof(campanhaWriterService));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IMembroEquipeWriterService _membroEquipeWriterService = membroEquipeWriterService ?? throw new ArgumentNullException(nameof(membroEquipeWriterService));
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService ?? throw new ArgumentNullException(nameof(equipeReaderService));
        private readonly IOrigemReaderService _origemReaderService = origemReaderService ?? throw new ArgumentNullException(nameof(origemReaderService));
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));
        private readonly INotificacaoClient _notificacaoClient = notificacaoClient ?? throw new ArgumentNullException(nameof(notificacaoClient));
        private readonly ITemplateReaderService _templateReaderService = templateReaderService ?? throw new ArgumentNullException(nameof(templateReaderService));
        private readonly ICanalReaderService _canalReaderService = canalReaderService ?? throw new ArgumentNullException(nameof(canalReaderService));
        private readonly ITemplateWriterService _templateWriterService = templateWriterService ?? throw new ArgumentNullException(nameof(templateWriterService));
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService ?? throw new ArgumentNullException(nameof(conversaReaderService));
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService ?? throw new ArgumentNullException(nameof(conversaWriterService));
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService ?? throw new ArgumentNullException(nameof(mensagemWriterService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        public async Task<LeadDTO> VerificarOuCriarLeadComResponsavelAsync(string whatsappNumero, List<CanalDTO> listaCanais, string apelido)
        {
            try
            {
                var lead = await _leadWriterService.VerificarLeadExistente(whatsappNumero, listaCanais, apelido);

                var bot = await _usuarioEmpresaReaderService.GetBotByEmpresa(lead.EmpresaId);
                if (lead.LeadNovo && bot != null)
                {
                    var membroEquipeBot = await _membroEquipeReaderService.GetBotMembroEquipeAsync(bot.UsuarioId, lead.EmpresaId);

                    // Atualiza lead no banco
                    await _leadWriterService.AtualizarResponsavelSemNotificar(lead.LeadId, membroEquipeBot.Id, membroEquipeBot.EquipeId, lead.EmpresaId);

                    // Cria o lead no cache
                    var leadCache = new LeadRedisDTO(
                        lead.LeadId,
                        lead.Nome,
                        lead.WhatsappNumero,
                        bot.UsuarioId,
                        membroEquipeBot.EquipeId,
                        lead.EmpresaId
                    );

                    var finalPrimaryKey = $"lead:{lead.LeadId}";
                    var finalIndiceKey = $"idx:whatsapp:{lead.WhatsappNumero}:canal:{lead.CanalId}";

                    await _redisCacheService.SetAsync(finalPrimaryKey, leadCache);
                    await _redisCacheService.SetStringAsync(finalIndiceKey, finalPrimaryKey);

                    return new LeadDTO(
                        lead.LeadId,
                        bot.UsuarioId,
                        bot.Usuario.Nome,
                        membroEquipeBot.Id,
                        bot.Usuario.IsBot,
                        lead.Nome,
                        lead.LeadNovo,
                        whatsappNumero,
                        lead.EmpresaId,
                        lead.CanalId,
                        membroEquipeBot.EquipeId
                    );
                }
                else if (lead.LeadNovo)
                {
                    var atribuicao = await _distribuicaoWriterService.AtribuirResponsavelPorEquipe(lead.LeadId, lead.EmpresaId) ?? throw new AppException("Não foi possível atribuir responsável");

                    // Atualiza lead no banco
                    await _leadWriterService.AtualizarResponsavelSemNotificar(lead.LeadId, atribuicao.AtribuicaoLead.MembroAtribuidoId, atribuicao.EquipeId, lead.EmpresaId);

                    // Cria o lead no cache
                    var leadCache = new LeadRedisDTO(
                        lead.LeadId,
                        lead.Nome,
                        lead.WhatsappNumero,
                        atribuicao.AtribuicaoLead.MembroAtribuido.Usuario!.Id,
                        atribuicao.EquipeId,
                        lead.EmpresaId
                    );

                    var finalPrimaryKey = $"lead:{lead.LeadId}";
                    var finalIndiceKey = $"idx:whatsapp:{lead.WhatsappNumero}:canal:{lead.CanalId}";

                    await _redisCacheService.SetAsync(finalPrimaryKey, leadCache);
                    await _redisCacheService.SetStringAsync(finalIndiceKey, finalPrimaryKey);

                    return new LeadDTO(
                        lead.LeadId,
                        atribuicao.AtribuicaoLead.MembroAtribuido.Usuario.Id,
                        atribuicao.AtribuicaoLead.MembroAtribuido.Usuario.Nome,
                        atribuicao.AtribuicaoLead.MembroAtribuido.Id,
                        false,
                        lead.Nome,
                        lead.LeadNovo,
                        whatsappNumero,
                        lead.EmpresaId,
                        lead.CanalId,
                        atribuicao.EquipeId
                    );
                }

                return lead;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar ou recuperar lead.");
                throw;
            }
        }

        public async Task CriarLeadViaCrawler(LeadCrawlerDTO dto)
        {
            try
            {
                int? campanhaId = null;
                int membroResponsavelId = 0;
                int equipeId = 0;
                var leadEventoObs = "Evento criado por integração *";
                bool leadPertenceAoBot = false;

                if (!string.IsNullOrWhiteSpace(dto.ObsEvento))
                {
                    leadEventoObs += $"{Environment.NewLine}{dto.ObsEvento}";
                }

                var empresa = await _empresaReaderService.ObterPorCnpjAsync(dto.CNPJEmpresa) ?? throw new ApplicationException($"Não foi possível encontrar a empresa com o CNPJ {dto.CNPJEmpresa}");
                var origem = await _origemReaderService.GetOrigemByName(dto.Origem);

                var lead = await _leadReaderService.ObterLeadPorGrupoAsync(dto.WhatsappNumero, dto.Email, null, empresa.GrupoEmpresaId);

                //verificar se lead existe, se existir criar evento, senao criar lead
                if (lead is not null) //lead existe, nao tem distribuicao, nesse caso o lead permanecera com o mesmo responsavel, basta notificar novo evento
                {
                    leadPertenceAoBot = _leadReaderService.LeadPertenceAoBot(lead);

                    if (!string.Equals(dto.Origem, "MyHonda", StringComparison.OrdinalIgnoreCase) && dto.CampanhaCod == null)
                    {
                        throw new AppException("O código da campanha é obrigatório.");
                    }
                    else if (!string.Equals(dto.Origem, "MyHonda", StringComparison.OrdinalIgnoreCase) && dto.CampanhaCod != null)
                    {
                        var campanhaBanco = await _campanhaReaderService.CampanhaExistsByCodigoAsync(dto.CampanhaCod, empresa.Id);
                        var equipeIntegracao = await _equipeReaderService.GetEquipeIntegracaoPorEmpresaIdAsync(empresa.Id) ?? throw new AppException("Não foi possível encontrar equipe integração"); //campanha sempre deve ser criada pra equipe integracao
                        if (campanhaBanco is null) //criar campanha temporária para garantir que o lead seja criado, depois pode ser migrado para a campanha correta quando ela for criada no sistema
                        {
                            var novaCampanha = new CriarCampanhaApiDTO
                            {
                                Nome = dto.CampanhaNome is not null ? dto.CampanhaNome : $"Campanha Automática - {dto.CampanhaCod}",
                                Codigo = dto.CampanhaCod,
                                EmpresaId = empresa.Id,
                                EquipeId = equipeIntegracao.Id,
                                Temporaria = true
                            };

                            await _campanhaWriterService.CriarCampanhaAsync(novaCampanha, commit: false);

                            // Busca novamente para pegar o ID
                            campanhaBanco = await _campanhaReaderService.CampanhaExistsByCodigoAsync(dto.CampanhaCod, empresa.Id);

                            if (campanhaBanco == null)
                                throw new AppException("Erro inesperado: campanha criada não encontrada.");

                            campanhaId = campanhaBanco.Id;
                        }
                        else
                        {
                            campanhaId = campanhaBanco.IdTransferida ?? campanhaBanco.Id;
                        }
                    }

                    if (leadPertenceAoBot)
                    {
                        var equipeIntegracao = await _equipeReaderService.GetEquipeIntegracaoPorEmpresaIdAsync(empresa.Id)
                        ?? throw new AppException("Não foi possível encontrar equipe integração");
                        equipeId = equipeIntegracao.Id;

                        (bool atribui, string mensagem, int? vendedorId) = await _distribuicaoWriterService.ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeId);
                        membroResponsavelId = vendedorId.Value;

                        await _leadWriterService.AtualizarResponsavelSemNotificar(lead.Id, membroResponsavelId, equipeId, empresa.Id);

                        var statusNovo = await _leadReaderService.GetLeadStatusByCodigo("CONTATO_INICIAL");

                        if (statusNovo != null)
                            await _leadWriterService.UpdateStatusAsync(lead.Id, statusNovo.Id, "Atualização de status via integração.");
                    }

                    //criar apenas evento
                    await _leadEventoWriterService.RegistrarEventoAsync(
                       lead,
                       campanhaId,
                       leadEventoObs,
                       origem.Id
                    );

                    await _unitOfWork.CommitAsync();

                    if (leadPertenceAoBot)
                        await _notificacaoClient.NovoLead(new NotificarNovoLeadDTO
                        {
                            LeadId = lead.Id,
                            UsuarioId = lead.Responsavel.UsuarioId
                        });

                    await _notificacaoClient.NovoLeadEvento(new NotificarNovoLeadDTO
                    {
                        LeadId = lead.Id,
                        UsuarioId = lead.Responsavel.UsuarioId
                    });
                }
                else
                {
                    if (string.Equals(dto.Origem, "MyHonda", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dto.EmailResponsavel != null)
                        {
                            var membroResponsavel = await _membroEquipeWriterService.GetMembroEquipePorEmail(dto.EmailResponsavel, empresa.Id, "ATIVO");
                            membroResponsavelId = membroResponsavel.Id;
                            equipeId = membroResponsavel.EquipeId;
                        }
                        else
                        {
                            var equipeIntegracao = await _equipeReaderService.GetEquipeIntegracaoPorEmpresaIdAsync(empresa.Id)
                            ?? throw new AppException("Não foi possível encontrar equipe integração");
                            equipeId = equipeIntegracao.Id;

                            (bool atribui, string mensagem, int? vendedorId) = await _distribuicaoWriterService.ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeId);
                            membroResponsavelId = vendedorId.Value;
                        }
                    }
                    else if (string.Equals(dto.Origem, "Site Daitan", StringComparison.OrdinalIgnoreCase)) //Site Daitan
                    {
                        var equipeIntegracao = await _equipeReaderService.GetEquipeIntegracaoPorEmpresaIdAsync(empresa.Id)
                            ?? throw new AppException("Não foi possível encontrar equipe integração");

                        equipeId = equipeIntegracao.Id;

                        (bool atribui, string mensagem, int? vendedorId) = await _distribuicaoWriterService.ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeId);

                        if (!atribui || vendedorId == null)
                            throw new AppException("Não foi possível buscar um responsável para distribuir o lead.");

                        membroResponsavelId = vendedorId.Value;
                    }
                    else //RD Station
                    {
                        if (dto.CampanhaCod == null)
                            throw new AppException("O código da campanha é obrigatório.");

                        var campanhaBanco = await _campanhaReaderService.CampanhaExistsByCodigoAsync(dto.CampanhaCod, empresa.Id);
                        var equipeIntegracao = await _equipeReaderService.GetEquipeIntegracaoPorEmpresaIdAsync(empresa.Id) ?? throw new AppException("Não foi possível encontrar equipe integração");

                        if (campanhaBanco is null) //criar campanha temporária para garantir que o lead seja criado, depois pode ser migrado para a campanha correta quando ela for criada no sistema
                        {
                            var novaCampanha = new CriarCampanhaApiDTO
                            {
                                Nome = dto.CampanhaNome is not null ? dto.CampanhaNome : $"Campanha Automática - {dto.CampanhaCod}",
                                Codigo = dto.CampanhaCod,
                                EmpresaId = empresa.Id,
                                EquipeId = equipeIntegracao.Id
                            };

                            await _campanhaWriterService.CriarCampanhaAsync(novaCampanha, commit: false);
                            equipeId = equipeIntegracao.Id;

                            // Busca novamente para pegar o ID
                            campanhaBanco = await _campanhaReaderService.CampanhaExistsByCodigoAsync(dto.CampanhaCod, empresa.Id);

                            if (campanhaBanco == null)
                                throw new AppException("Erro inesperado: campanha criada não encontrada.");

                            campanhaId = campanhaBanco.Id;
                        }
                        else //campanha existe, verificar se tem equipe vinculada, se tiver usar essa equipe, se não tiver usar a equipe de integração
                        {
                            campanhaId = campanhaBanco.IdTransferida ?? campanhaBanco.Id;
                            if (campanhaBanco.EquipeId != null && campanhaBanco.EquipeId != equipeIntegracao.Id)
                            {
                                equipeId = campanhaBanco.EquipeId.Value;
                            }
                            else
                            {
                                equipeId = equipeIntegracao.Id;
                            }
                        }

                        (bool atribui, string mensagem, int? vendedorId) = await _distribuicaoWriterService.ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeId);

                        if (!atribui || vendedorId == null)
                        {
                            throw new AppException("Não foi possível buscar um responsável para distribuir o lead.");
                        }

                        membroResponsavelId = vendedorId.Value;
                    }

                    var observacaoContato = string.Empty;
                    string? whatsappNormalizado = null;

                    if (!string.IsNullOrWhiteSpace(dto.WhatsappNumero))
                    {
                        whatsappNormalizado = _leadReaderService.NormalizarNumeroWhatsApp(dto.WhatsappNumero);

                        bool whatsappInvalido = string.IsNullOrWhiteSpace(whatsappNormalizado)
                                                || (whatsappNormalizado.Length != 12 && whatsappNormalizado.Length != 13);

                        if (whatsappInvalido)
                        {
                            observacaoContato = $"WhatsApp informado: {dto.WhatsappNumero}";
                            whatsappNormalizado = null;
                        }
                    }

                    if (whatsappNormalizado == null && dto.Email == null)
                        throw new AppException("Lead não pode ser criado sem um identificador válido (WhatsApp ou e-mail).");

                    var dtoCompleto = new LeadCompletoDTO
                    {
                        Nome = dto.Nome,
                        Email = dto.Email,
                        WhatsappNumero = whatsappNormalizado,
                        CampanhaId = campanhaId,
                        OrigemId = origem.Id,
                        EmpresaId = empresa.Id,
                        EquipeId = equipeId,
                        ResponsavelId = membroResponsavelId,
                        ObservacoesCadastrais = observacaoContato
                    };

                    lead = await _leadWriterService.CreateAsync(dtoCompleto, true, leadEventoObs);
                    //nao precisa de commit pq ja esta sendo feito no metodo de criar lead
                }

                var conversaAtiva = await _conversaReaderService.GetConversaByLead(lead.Id, "ENCERRADA");
                if ((conversaAtiva == null || leadPertenceAoBot) && lead.WhatsappNumero != null)
                {
                    var canal = await _canalReaderService.GetCanalByEmpresaId(empresa.Id) ?? throw new AppException("Canal relacionado a esse empresa não foi encontrado.");
                    var template = await _templateReaderService.GetTemplateByOrigem(origem.Id, canal.Id);
                    if (template == null)
                    {
                        _logger.LogWarning(
                            "Nenhum template encontrado para OrigemId {OrigemId} e CanalId {CanalId}. LeadId {LeadId}.",
                            origem.Id,
                            canal.Id,
                            lead.Id);

                        return;
                    }

                    await _unitOfWork.BeginTransactionAsync();

                    //if (conversaAtiva != null && leadPertenceAoBot)
                    //{
                    //    await _conversaWriterService.UpdateResponsavelAsync(conversaAtiva, membroResponsavelId, canal.Id, equipeId);
                    //}

                    var conversaId = await _conversaWriterService.GetConversaByLeadAndCanalAsync(lead.Id, lead.Responsavel.UsuarioId, canal.Id, "ATIVA", lead.EquipeId.Value, false, true);
                    var mensagem = await _mensagemWriterService.ProcessarMensagemEnvioTemplateIntegracaoAsync("text", conversaId, lead.Responsavel.UsuarioId, lead, canal, template.Id, null);
                    await _conversaWriterService.UpdateDataUltimaMensagemAsync(conversaId, mensagem.DataCriacao);

                    await _unitOfWork.CommitAsync();
                    NotificarNovaMensagemDTO novaMensagem = new()
                    {
                        MensagemId = mensagem.Id,
                        UsuarioId = lead.Responsavel.UsuarioId,
                        Titulo = lead.Nome,
                        MensagemSincronizacao = new MensagemDTO
                        {
                            MensagemId = mensagem.Id,
                            Conteudo = mensagem.Conteudo,
                            TipoMensagem = mensagem.Tipo.Codigo,
                            DataEnvio = mensagem.DataEnvio!.Value,
                            TipoRemetente = mensagem.Sentido,
                            LeadId = lead.Id,
                            UsuarioId = lead.Responsavel.UsuarioId
                        }
                    };

                    var response = await _notificacaoClient.NovaMensagem(novaMensagem);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
