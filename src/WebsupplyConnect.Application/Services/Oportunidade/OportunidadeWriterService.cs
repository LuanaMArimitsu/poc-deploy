using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;

namespace WebsupplyConnect.Application.Services.Oportunidade
{
    public class OportunidadeWriterService(
        ILogger<OportunidadeWriterService> logger,
        ILeadRepository leadRepository,
        IOportunidadeRepository oportunidadeRepository,
        IEtapaReaderService etapaReaderService,
        IFunilReaderService funilReaderService,
        IMembroEquipeReaderService membroEquipeReaderService,
        IConnectIntegradorService _connectIntegradorService,
        IUnitOfWork unitOfWork,
        ISistemaExternoReaderService sistemaExternoService,
        ILeadEventoReaderService leadEventoReaderService,
        ILeadReaderService leadReaderService,
        IUsuarioEmpresaReaderService usuarioEmpresaReaderService
    ) : IOportunidadeWriterService
    {
        private readonly ILogger<OportunidadeWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IOportunidadeRepository _oportunidadeRepository = oportunidadeRepository ?? throw new ArgumentNullException(nameof(oportunidadeRepository));
        private readonly IFunilReaderService _funilReaderService = funilReaderService ?? throw new ArgumentNullException(nameof(funilReaderService));
        private readonly ILeadRepository _leadRepository = leadRepository ?? throw new ArgumentNullException(nameof(leadRepository));
        private readonly IEtapaReaderService _etapaReaderService = etapaReaderService ?? throw new ArgumentNullException(nameof(etapaReaderService));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly IConnectIntegradorService _connectIntegradorService = _connectIntegradorService ?? throw new ArgumentNullException(nameof(_connectIntegradorService));
        private readonly ISistemaExternoReaderService _sistemaExternoService = sistemaExternoService ?? throw new ArgumentNullException(nameof(sistemaExternoService));
        private readonly ILeadEventoReaderService _leadEventoReaderService = leadEventoReaderService ?? throw new ArgumentNullException(nameof(leadEventoReaderService));
        private readonly ILeadReaderService _leadReaderService = leadReaderService ?? throw new ArgumentNullException(nameof(leadReaderService));
        private readonly IUsuarioEmpresaReaderService _usuarioEmpresaReaderService = usuarioEmpresaReaderService ?? throw new ArgumentNullException(nameof(usuarioEmpresaReaderService));

        public async Task<Domain.Entities.Oportunidade.Oportunidade> CreateOportunidadeAsync(CreateOportunidadeDTO dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await ValidarEntidadesRelacionadasCreate(dto);

                if (dto.LeadEventoId.HasValue)
                {
                    if (dto.LeadEventoId == 0)
                    {
                        throw new AppException("LeadEventoId inválido.");
                    }

                    var leadEvento = await _leadEventoReaderService.GetLeadEventoByIdAsync(dto.LeadEventoId.Value);
                    if (leadEvento == null)
                        throw new AppException($"LeadEvento com ID igual a {dto.LeadEventoId.Value} não existe.");

                    if (leadEvento.LeadId != dto.LeadId)
                        throw new AppException("O LeadEvento informado não pertence ao mesmo Lead da oportunidade.");

                    if (dto.OrigemId != leadEvento.OrigemId)
                    {
                        dto.OrigemId = leadEvento.OrigemId; //caso recebido leadEventoId e origemId divergentes, prevalece o leadEventoId
                    }
                }

                var etapa = await _etapaReaderService.GetEtapaById(dto.EtapaId)
                           ?? throw new AppException($"Etapa com id {dto.EtapaId} não foi encontrada.");

                if (etapa.EhFinal)
                    throw new AppException("Não é possível criar uma oportunidade na etapa final.");

                var lead = await _leadRepository.GetByIdAsync<Domain.Entities.Lead.Lead>(dto.LeadId)
                           ?? throw new AppException($"Lead com id {dto.LeadId} não foi encontrado.");

                var membro = await _membroEquipeReaderService.GetByIdAsync(lead.ResponsavelId!.Value) ?? throw new AppException($"Membro da equipe com id {lead.ResponsavelId!.Value} não foi encontrado.");

                var oportunidade = new Domain.Entities.Oportunidade.Oportunidade(
                    dto.LeadId,
                    dto.ProdutoId,
                    dto.EtapaId,
                    membro.UsuarioId,
                    dto.OrigemId,
                    dto.EmpresaId,
                    dto.Valor,
                    etapa.ProbabilidadePadrao,
                    dto.DataPrevisaoFechamento,
                    dto.Observacao,
                    dto.TipoInteresseId,
                    dto.LeadEventoId
                );

                var oportunidadeBanco = await _oportunidadeRepository.CreateAsync(oportunidade);
                await _unitOfWork.SaveChangesAsync();

                oportunidade.AdicionarEtapaHistorico(oportunidadeBanco.Id, oportunidadeBanco.EtapaId, oportunidadeBanco.EtapaId, oportunidadeBanco.ResponsavelId);
                await _unitOfWork.CommitAsync();
                return oportunidade;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar uma oportunidade.");
                throw;
            }
        }

        public async Task UpdateOportunidadeAsync(UpdateOportunidadeDTO dto)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(dto.Id) ?? throw new AppException($"Oportunidade com ID igual a {dto.Id} não existe.");

                await ValidarEntidadesRelacionadasUpdate(dto, oportunidade);

                int origemId = oportunidade.OrigemId;
                if (dto.LeadEventoId.HasValue)
                {
                    var leadEvento = await _leadEventoReaderService.GetLeadEventoByIdAsync(dto.LeadEventoId.Value);
                    if (leadEvento == null)
                        throw new AppException($"LeadEvento com ID igual a {dto.LeadEventoId.Value} não existe.");

                    if (origemId != leadEvento.OrigemId)
                    {
                        origemId = leadEvento.OrigemId; //prevalecer o leadEventoId
                    }
                }

                oportunidade.AtualizaOportunidade(
                    dto.ProdutoId,
                    dto.Valor,
                    dto.ValorFinal,
                    dto.Probabilidade,
                    dto.DataPrevisaoFechamento,
                    dto.DataUltimaInteracao,
                    dto.DataFechamento,
                    dto.Observacao,
                    dto.TipoInteresseId,
                    dto.LeadEventoId,
                    origemId
                );
                _oportunidadeRepository.Update(oportunidade);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar uma oportunidade.");
                throw;
            }
        }

        public async Task DeleteOportunidadeAsync(int id)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(id) ?? throw new AppException($"Oportunidade com ID igual a {id} não existe.");
                oportunidade.ExcluirLogicamente();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao excluir uma oportunidade.");
                throw;
            }
        }

        public async Task UpdateResponsavelOportunidade(Domain.Entities.Oportunidade.Oportunidade oportunidade, int novoResponsavelId, int empresaId)
        {
            try
            {
                if (oportunidade == null)
                    throw new AppException("Oportunidade não pode ser nulo.");

                if (novoResponsavelId <= 0 || !await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Usuario.Usuario>(novoResponsavelId))
                    throw new AppException($"Usuário com ID igual a {novoResponsavelId} não existe.");

                if (empresaId <= 0 || !await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Empresa.Empresa>(empresaId))
                    throw new AppException($"Empresa com ID igual a {empresaId} não existe.");

                oportunidade.TransferirResponsabilidade(novoResponsavelId, empresaId);
                _oportunidadeRepository.Update(oportunidade);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                _logger.LogError("Erro ao atualizar o responsável pela oportunidade.");
                throw;
            }

        }

        public async Task UpdateEtapaOpotunidade(int oportunidadeId, ChangeEtapaDTO dto)
        {
            try
            {
                await ValidarUpdateEtapa(oportunidadeId, dto);

                await _unitOfWork.BeginTransactionAsync();


                if (dto is null)
                    throw new AppException("Payload inválido.");

                var oportunidade = await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(oportunidadeId)
                    ?? throw new AppException("Oportunidade não encontrada.");

                var listaEtapas = await _funilReaderService.GetFunilByEmpresa(oportunidade.EmpresaId)
                    ?? throw new AppException("Funil não possui nenhuma etapa");

                var etapas = listaEtapas.OrderBy(e => e.Ordem).ToList();
                var origem = etapas.First(e => e.Id == oportunidade.EtapaId);
                var destino = etapas.FirstOrDefault(e => e.Id == dto.EtapaDestinoId)
                    ?? throw new AppException("Etapa de destino inválida.");

                var usuarioId = oportunidade.ResponsavelId;

                // Regra 1: Etapa atual EhAtiva -> destino EhPerdida
                //          Finaliza e Observação é obrigatória
                if (origem.EhAtiva && destino.EhPerdida)
                {
                    if (string.IsNullOrWhiteSpace(dto.Observacao))
                        throw new AppException("Observação é obrigatória ao mover para a etapa 'Perdida'.");

                    oportunidade.MarcarComoPerdida(destino.Id, usuarioId, dto.Observacao);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Regra 2: Etapa atual EhAtiva -> destino EhVitoria
                //          Finaliza e Valor Final é obrigatório
                if (origem.EhAtiva && destino.EhVitoria)
                {
                    if (!dto.ValorFinalVenda.HasValue || dto.ValorFinalVenda <= 0)
                        throw new AppException("Valor final da venda é obrigatório ao mover para a etapa 'Ganha'.");

                    oportunidade.MarcarComoGanha(destino.Id, dto.ValorFinalVenda.Value, usuarioId, dto.Observacao);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Regra 3: Etapa atual EhFinal -> destino EhAtiva
                //          Reabertura = Observação é obrigatória
                if (origem.EhFinal && destino.EhAtiva)
                {
                    if (string.IsNullOrWhiteSpace(dto.Observacao))
                        throw new AppException("Observação é obrigatória para reabertura (sair de etapa final).");

                    oportunidade.Reabrir(destino.Id, usuarioId, dto.Observacao);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Regra 4: Etapa atual EhFinal -> destino EhFinal
                //          Arquivamento = Observação é obrigatória
                if (origem.EhFinal && destino.EhFinal)
                {
                    if (string.IsNullOrWhiteSpace(dto.Observacao))
                        throw new AppException("Observação é obrigatória para Arquivar oportunidade.");

                    oportunidade.MarcarComoArquivada(destino.Id, usuarioId, dto.Observacao);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Regra 5: EhAtiva -> EhAtiva e ordem destino > ordem origem => Progressão
                if (origem.EhAtiva && destino.EhAtiva && destino.Ordem > origem.Ordem)
                {
                    oportunidade.AvancarPara(destino.Id, usuarioId, dto.Observacao);

                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Regra 6: EhAtiva -> EhAtiva e ordem destino < ordem origem => Regressão (Observação obrigatória)
                if (origem.EhAtiva && destino.EhAtiva && destino.Ordem < origem.Ordem)
                {
                    if (string.IsNullOrWhiteSpace(dto.Observacao))
                        throw new AppException("Observação é obrigatória ao regredir a etapa.");

                    oportunidade.RegredirPara(destino.Id, usuarioId, dto.Observacao);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                // Qualquer outra combinação fora das regras
                throw new AppException("Movimentação não permitida pelas regras definidas.");

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar etapa da oportunidade.");
                throw;
            }
        }
        private async Task ValidarEntidadesRelacionadasUpdate(UpdateOportunidadeDTO dto, Domain.Entities.Oportunidade.Oportunidade oportunidade)
        {
            if (dto.ProdutoId.HasValue && dto.ProdutoId > 0 && dto.ProdutoId.Value != oportunidade.ProdutoId && dto.ProdutoId > 0)
            {
                var produtoExiste = await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Produto.Produto>(dto.ProdutoId.Value);
                if (!produtoExiste)
                    throw new AppException($"Produto com ID igual a {dto.ProdutoId} não existe.");
            }
        }
        private async Task ValidarUpdateEtapa(int oportunidadeId, ChangeEtapaDTO dto)
        {
            if (oportunidadeId < 0 || !await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Oportunidade.Oportunidade>(oportunidadeId))
                throw new AppException($"Oportunidade com ID igual a {oportunidadeId} não existe.");

            if (dto.EtapaDestinoId < 0 || !await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Oportunidade.Etapa>(dto.EtapaDestinoId))
                throw new AppException($"Etapa com ID igual a {dto.EtapaDestinoId} não existe.");
        }
        private async Task ValidarEntidadesRelacionadasCreate(CreateOportunidadeDTO dto)
        {
            if (!await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Lead.Lead>(dto.LeadId))
                throw new AppException($"Lead com ID igual a {dto.LeadId} não existe.");

            if (!await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Produto.Produto>(dto.ProdutoId))
                throw new AppException($"Produto com ID igual a {dto.ProdutoId} não existe.");

            if (!await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Oportunidade.Etapa>(dto.EtapaId))
                throw new AppException($"Etapa com ID igual a {dto.EtapaId} não existe.");

            if (!await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Lead.Origem>(dto.OrigemId))
                throw new AppException($"Origem com ID igual a {dto.OrigemId} não existe.");

            if (!await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Empresa.Empresa>(dto.EmpresaId))
                throw new AppException($"Empresa com ID igual a {dto.EmpresaId} não existe.");
        }

        public async Task EnviarParaIntegrador(int oportunidadeId, int usuarioLogado)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetDetailsById(oportunidadeId)
                    ?? throw new AppException("Oportunidade não encontrada.");

                if (!oportunidade.TipoInteresseId.HasValue)
                    throw new AppException("Não é possível enviar para o Gold oportunidade sem tipo interesse.");

                if (string.IsNullOrWhiteSpace(oportunidade.Lead.WhatsappNumero))
                    throw new AppException("Informe o número de WhatsApp do Lead para enviar para o Gold.");

                ///valida vendedor NBS do usuário logado para realizar a integração
                UsuarioEmpresa? usuarioEmpresa = await _usuarioEmpresaReaderService.GetUsuarioEmpresaByEmpresa(oportunidade.EmpresaId, usuarioLogado);
                if (string.IsNullOrWhiteSpace(usuarioEmpresa?.CodVendedorNBS))
                {
                    throw new AppException("Código de Vendedor NBS desse usuário não encontrado para a empresa da oportunidade.");

                }

                var whatsapp = oportunidade.Lead.WhatsappNumero.Trim();
                if (whatsapp.StartsWith("55"))
                {
                    whatsapp = whatsapp.Substring(2);
                }

                var sistemaExterno = await _sistemaExternoService.GetSistemaExternoIntegradorData("ConnectIntegrador");

                var dto = new OportunidadeRequestDTO
                {
                    OportunidadeId = oportunidade.Id,
                    CnpjEmpresa = oportunidade.Empresa.Cnpj,
                    Interesse = oportunidade.TipoInteresseId.Value.ToString(),
                    NomeCliente = oportunidade.Lead.Nome,
                    Telefone = whatsapp,
                    Email = oportunidade.Lead.Email,
                    Mensagem = $"Oportunidade {oportunidade.Id} - Produto: {oportunidade.Produto?.Nome ?? "N"} - Observações: {oportunidade.Observacoes}",
                    CodVendedor = usuarioEmpresa.CodVendedorNBS!
                };

                GerarEventoResultDTO resultado = await _connectIntegradorService.ConnectIntegradorAsync(
                    dto,
                    sistemaExterno.URL_API,
                    sistemaExterno.Token,
                    sistemaExterno.Id
                );

                if (resultado.Sucesso == true && !string.IsNullOrWhiteSpace(resultado.CodEvento))
                {
                    oportunidade.AtribuirCodEvento(resultado.CodEvento);
                    _oportunidadeRepository.Update(oportunidade);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                    return;
                }

                throw new AppException(resultado.Mensagem);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AtualizarConversaoAsync(ConversaoOportunidadeDTO request)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(request.OportunidadeId)
                    ?? throw new AppException($"Oportunidade {request.OportunidadeId} não encontrada.");

                oportunidade.AtualizarConversao(request.Convertida, request.DataConversao);

                _oportunidadeRepository.Update(oportunidade);

                if (request.Convertida == true)
                {
                    var lead = await _leadReaderService.GetLeadComResponsavelAsync(oportunidade.LeadId)
                        ?? throw new AppException("Lead não encontrado.");

                    var statusCliente = (await _leadReaderService.ListarStatusDoLeadAsync())
                        .FirstOrDefault(s =>
                            s.Codigo.Equals("CLIENTE", StringComparison.OrdinalIgnoreCase))
                        ?? throw new AppException("Status 'CLIENTE' não encontrado.");

                    var usuarioId = lead.Responsavel.UsuarioId;

                    lead.AlterarStatus(statusCliente.Id, usuarioId);
                    lead.DefinirDataConversaoCliente();
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

            }
            catch (AppException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Erro ao atualizar conversão das oportunidades.");
            }
        }
    }
}
