using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.DTOs.Lead.OLX;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Infrastructure.ExternalServices.OLX
{
    public class OlxIntegracaoService : IOlxIntegracaoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILeadWriterService _leadWriterService;
        private readonly ILeadReaderService _leadReaderService;
        private readonly ILeadEventoWriterService _leadEventoWriterService;
        private readonly ISistemaExternoReaderService _sistemaExternoReaderService;
        private readonly IEventoIntegracaoWriterService _eventoIntegracaoWriterService;
        private readonly IEmpresaReaderService _empresaReaderService;
        private readonly IOrigemReaderService _origemReaderService;
        private readonly IEquipeReaderService _equipeReaderService;
        private readonly IDistribuicaoWriterService _distribuicaoWriterService;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService;
        private readonly INotificacaoClient _notificacaoClient;
        private readonly ICanalReaderService _canalReaderService;
        private readonly ITemplateReaderService _templateReaderService;
        private readonly ITemplateWriterService _templateWriterService;
        private readonly IConversaReaderService _conversaReaderService;
        private readonly IConversaWriterService _conversaWriterService;
        private readonly IMensagemWriterService _mensagemWriterService;
        private readonly IRedistribuicaoService _redistribuicaoService;
        private readonly ILogger<OlxIntegracaoService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public OlxIntegracaoService(IHttpContextAccessor httpContextAccessor,
            ILeadWriterService leadWriterService,
            ILeadReaderService leadReaderService,
            ILeadEventoWriterService leadEventoWriterService,
            ISistemaExternoReaderService sistemaExternoReaderService,
            IEventoIntegracaoWriterService eventoIntegracaoWriterService,
            IEmpresaReaderService empresaReaderService,
            IOrigemReaderService origemReaderService,
            IEquipeReaderService equipeReaderService,
            IDistribuicaoWriterService distribuicaoWriterService,
            IMembroEquipeReaderService membroEquipeReaderService,
            INotificacaoClient notificacaoClient,
            ICanalReaderService canalReaderService,
            ITemplateReaderService templateReaderService,
            ITemplateWriterService templateWriterService,
            IConversaReaderService conversaReaderService,
            IConversaWriterService conversaWriterService,
            IMensagemWriterService mensagemWriterService,
            IRedistribuicaoService redistribuicaoService,
            ILogger<OlxIntegracaoService> logger,
            IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _leadWriterService = leadWriterService;
            _leadReaderService = leadReaderService;
            _leadEventoWriterService = leadEventoWriterService;
            _sistemaExternoReaderService = sistemaExternoReaderService;
            _eventoIntegracaoWriterService = eventoIntegracaoWriterService;
            _empresaReaderService = empresaReaderService;
            _origemReaderService = origemReaderService;
            _equipeReaderService = equipeReaderService;
            _distribuicaoWriterService = distribuicaoWriterService;
            _membroEquipeReaderService = membroEquipeReaderService;
            _notificacaoClient = notificacaoClient;
            _canalReaderService = canalReaderService;
            _templateReaderService = templateReaderService;
            _templateWriterService = templateWriterService;
            _conversaWriterService = conversaWriterService;
            _conversaReaderService = conversaReaderService;
            _mensagemWriterService = mensagemWriterService;
            _redistribuicaoService = redistribuicaoService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> ReceberLeadOlxAsync(string cnpjEmpresa, string rawJson)
        {
            int? leadCriadoId = null;
            int? sistemaOlxId = null;
            EventoIntegracao eventoIntegracao = null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var leadOlxDTO = JsonSerializer.Deserialize<OlxLeadDTO>(rawJson, options);

                //validações do lead
                if (leadOlxDTO == null)
                    throw new AppException("Payload inválido recebido da OLX.");

                if (string.IsNullOrWhiteSpace(leadOlxDTO.Name))
                    throw new AppException("Nome não informado pela OLX.");

                if (string.IsNullOrWhiteSpace(leadOlxDTO.Email))
                    throw new AppException("Email não informado pela OLX.");

                //validações do token
                //var authHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();

                //if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                //throw new AppException("Token não fornecido");

                //var tokenRecebido = authHeader.Substring("Bearer ".Length).Trim();

                var empresa = await _empresaReaderService.GetEmpresaPorCnpjAsync(cnpjEmpresa);
                if (empresa == null)
                    throw new AppException($"Empresa com nome {cnpjEmpresa} não encontrada.");

                var sistemaOlx = await _sistemaExternoReaderService.GetSistemaExternoOlxPorCredenciais("OLX", cnpjEmpresa);
                if (sistemaOlx == null)
                    throw new AppException($"Sistema externo OLX não cadastrado para a empresa com CNPJ {cnpjEmpresa}.");

                sistemaOlxId = sistemaOlx.Id;

                //if (string.IsNullOrWhiteSpace(sistemaOlx.Token) || tokenRecebido != sistemaOlx.Token)
                //throw new AppException("Token inválido.");

                var origem = await _origemReaderService.GetOrigemByName("OLX");
                if (origem == null)
                    throw new AppException("Origem 'OLX' não encontrada.");

                var equipeOlx = await _equipeReaderService.ObterEquipeOlxIdAsync(empresa.Id);
                if (equipeOlx == null)
                    throw new AppException($"Equipe OLX para empresa {empresa.Cnpj} não encontrada.");

                //lógica de lead
                var leadExistente = await _leadReaderService.ObterLeadPorGrupoAsync(leadOlxDTO.Phone, leadOlxDTO.Email, null, empresa.GrupoEmpresaId);

                Domain.Entities.Lead.Lead lead = leadExistente;

                var leadEventoObs =
                    $"<strong>Assunto:</strong> {leadOlxDTO.AdsInfo?.Subject}<br/>" +
                    $"<strong>Link:</strong> <a href='{leadOlxDTO.LinkAd}' target='_blank'>{leadOlxDTO.LinkAd}</a><br/>" +
                    $"<strong>Placa:</strong> {leadOlxDTO.AdsInfo?.VehicleTag}";

                var responsavelIsBot = false;

                if (leadExistente == null)
                {
                    var (atribui, mensagemDistribuicao, vendedorId) = await _distribuicaoWriterService.ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeOlx.Id);
                    if (vendedorId == null)
                        throw new AppException($"Nenhum vendedor disponível para distribuição na equipe ID {equipeOlx.Id}.");

                    var nome = leadOlxDTO.Name;
                    var email = leadOlxDTO.Email;
                    var telefone = leadOlxDTO.Phone;
                    var mensagem = leadOlxDTO.Message;

                    var leadCreateDto = new LeadCompletoDTO
                    {
                        Nome = leadOlxDTO.Name,
                        Email = leadOlxDTO.Email,
                        WhatsappNumero = leadOlxDTO.Phone,
                        Telefone = null,
                        EmpresaId = empresa.Id,
                        OrigemId = origem.Id,
                        ResponsavelId = vendedorId.Value,
                        EquipeId = equipeOlx.Id,
                        ObservacoesCadastrais = mensagem,
                        CampanhaId = null,
                        NivelInteresse = null,
                        Cargo = null,
                        CPF = null,
                        CNPJEmpresa = null,
                        NomeEmpresa = null,
                        Genero = null,
                        DataNascimento = null
                    };

                    lead = await _leadWriterService
                        .CreateAsync(leadCreateDto, false, leadEventoObs);

                    leadCriadoId = lead.Id;
                }
                else
                {
                    // Verifica se o responsável atual é BOT
                    responsavelIsBot = _leadReaderService.LeadPertenceAoBot(leadExistente);

                    //Verifica o ID do status para comparar se o lead está inativo
                    var statusInativoId = await _leadReaderService.GetLeadStatusByCodigo("INATIVO");
                    var leadInativo = leadExistente.LeadStatusId == statusInativoId?.Id;

                    if (responsavelIsBot)
                    {
                        var (atribui, mensagemDistribuicao, vendedorId) = await _distribuicaoWriterService
                            .ObterVendedorParaDistribuicaoPorEquipe(empresa.Id, equipeOlx.Id);

                        if (vendedorId == null)
                            throw new AppException($"Nenhum vendedor disponível para redistribuição na equipe ID {equipeOlx.Id}.");

                        await _redistribuicaoService.RedistribuirLeadAsync(
                            leadExistente.Id,
                            vendedorId.Value,
                            equipeOlx.Id,
                            empresa.Id
                        );
                    }

                    if (leadInativo)
                    {
                        var statusReativado = await _leadReaderService.GetLeadStatusByCodigo("REATIVADO")
                            ?? throw new AppException("Status 'REATIVADO' não encontrado.");

                        await _leadWriterService.UpdateStatusAsync(leadExistente.Id, statusReativado.Id, "LEAD REATIVADO PELA INTEGRAÇÃO OLX");
                    }

                    lead = await _leadReaderService.GetLeadByIdAsync(leadExistente.Id)
                        ?? throw new AppException($"Lead {leadExistente.Id} não encontrado após redistribuição.");

                    await _leadEventoWriterService.RegistrarEventoAsync(
                        leadExistente,
                        null,
                        leadEventoObs
                    );

                    leadCriadoId = leadExistente.Id;
                }

                eventoIntegracao = new EventoIntegracao(
                    sistemaExternoId: sistemaOlx.Id,
                    direcao: DirecaoIntegracao.Recebido,
                    tipoEvento: TipoEventoIntegracao.LEAD_CREATED,
                    sucesso: true,
                    payloadRecebido: rawJson,
                    codigoResposta: StatusCodes.Status200OK.ToString(),
                    mensagemErro: null,
                    tipoEntidadeOrigem: TipoEntidadeIntegracao.Lead,
                    entidadeOrigemId: leadCriadoId.Value
                );

                await _eventoIntegracaoWriterService.RegistrarAsync(eventoIntegracao, false);

                var conversaAtiva = await _conversaReaderService.GetConversaByLead(lead.Id, "ENCERRADA");
                if ((conversaAtiva == null || responsavelIsBot) && lead.WhatsappNumero != null)
                {
                    var canal = await _canalReaderService.GetCanalByEmpresaId(empresa.Id) ?? throw new AppException("Canal relacionado a esse empresa não foi encontrado.");
                    var template = await _templateReaderService.GetTemplateByOrigem(origem.Id, canal.Id);
                    if (template != null)
                    {
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
                    else
                    {
                        _logger.LogWarning(
                            "Nenhum template encontrado para OrigemId {OrigemId} e CanalId {CanalId}. LeadId {LeadId}.",
                            origem.Id,
                            canal.Id,
                            lead.Id);
                    }
                }

                await _unitOfWork.CommitAsync();

                if (leadExistente == null)
                {
                    await _notificacaoClient.NovoLead(new NotificarNovoLeadDTO
                    {
                        LeadId = lead.Id,
                        UsuarioId = lead.Responsavel.UsuarioId
                    });
                }
                else
                {
                    await _notificacaoClient.NovoLeadEvento(new NotificarNovoLeadDTO
                    {
                        LeadId = lead.Id,
                        UsuarioId = lead.Responsavel.UsuarioId
                    });
                }

                return eventoIntegracao.Id.ToString();
            }
            catch (AppException ex)
            {
                eventoIntegracao = new EventoIntegracao(
                    sistemaExternoId: sistemaOlxId.Value,
                    direcao: DirecaoIntegracao.Recebido,
                    tipoEvento: TipoEventoIntegracao.LEAD_CREATED,
                    sucesso: false,
                    payloadRecebido: rawJson,
                    codigoResposta: StatusCodes.Status400BadRequest.ToString(),
                    mensagemErro: ex.Message,
                    tipoEntidadeOrigem: TipoEntidadeIntegracao.Lead,
                    entidadeOrigemId: leadCriadoId
                );

                await _eventoIntegracaoWriterService.RegistrarAsync(eventoIntegracao);

                throw;
            }
            catch (Exception ex)
            {
                eventoIntegracao = new EventoIntegracao(
                    sistemaExternoId: sistemaOlxId.Value,
                    direcao: DirecaoIntegracao.Recebido,
                    tipoEvento: TipoEventoIntegracao.LEAD_CREATED,
                    sucesso: false,
                    payloadRecebido: rawJson,
                    codigoResposta: StatusCodes.Status500InternalServerError.ToString(),
                    mensagemErro: ex.Message,
                    tipoEntidadeOrigem: TipoEntidadeIntegracao.Lead,
                    entidadeOrigemId: leadCriadoId
                );

                await _eventoIntegracaoWriterService.RegistrarAsync(eventoIntegracao);

                throw;
            }
        }
    }
}
