using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Notificacao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Interfaces.Base;
namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Serviço orquestrador de redistribuição de leads
    /// Responsabilidade: Orquestrar transferências e gerenciar transações
    /// </summary>
    public class RedistribuicaoService(
        ITransferenciaLeadCommand transferenciaCommand,
        IMembroEquipeReaderService membroEquipeReaderService,
        IUnitOfWork unitOfWork,
        IEmpresaReaderService empresaReaderService,
        IEquipeReaderService equipeReaderService,
        IUsuarioReaderService usuarioReaderService,
        IDistribuicaoWriterService distribuicaoWriterService,
        INotificacaoWriterService notificacaoWriterService,
        ILogger<RedistribuicaoService> logger) : IRedistribuicaoService
    {
        private readonly ITransferenciaLeadCommand _transferenciaCommand = transferenciaCommand ?? throw new ArgumentNullException(nameof(transferenciaCommand));
        private readonly IMembroEquipeReaderService _membroEquipeReaderService = membroEquipeReaderService ?? throw new ArgumentNullException(nameof(membroEquipeReaderService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IEmpresaReaderService _empresaReaderService = empresaReaderService ?? throw new ArgumentNullException(nameof(empresaReaderService));
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService ?? throw new ArgumentNullException(nameof(equipeReaderService));
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
        private readonly IDistribuicaoWriterService _distribuicaoWriterService = distribuicaoWriterService ?? throw new ArgumentNullException(nameof(distribuicaoWriterService));
        private readonly INotificacaoWriterService _notificacaoWriterService = notificacaoWriterService ?? throw new ArgumentNullException(nameof(notificacaoWriterService));
        private readonly ILogger<RedistribuicaoService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Transfere um lead com estratégia de persistência configurável
        /// </summary>s
        /// <param name="leadId">ID do lead a ser transferido</param>
        /// <param name="novoResponsavelId">ID do novo responsável</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="usarCommit">Se true, usa CommitAsync; se false, usa SaveChangesAsync</param>
        public async Task TransferirLeadAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId, bool usarCommit = false)
        {
            try
            {
                await _transferenciaCommand.ExecutarAsync(leadId, novoResponsavelId, equipeId, empresaId);

                if (usarCommit)
                    await _unitOfWork.CommitAsync();
                else
                    await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead {LeadId} para usuário {NovoResponsavelId}",
                    leadId, novoResponsavelId);
                throw;
            }
        }

        public async Task RedistribuirLeadAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId)
        {
            try
            {
                await TransferirLeadAsync(leadId, novoResponsavelId, equipeId, empresaId, true);
             
                var usuario = await _usuarioReaderService.ObterVendedorPorMembroId(novoResponsavelId) ?? throw new AppException($"Usuário com id do membro {novoResponsavelId} não foi encontrado.");

                await _notificacaoWriterService.LeadAtualizado(new NotificarNovoLeadDTO
                {
                    LeadId = leadId,
                    UsuarioId = usuario.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead {LeadId} para usuário {NovoResponsavelId}",
                    leadId, novoResponsavelId);
                throw;
            }
        }

        public async Task TransferirLeadSemOportunidadeAsync(int leadId, int novoResponsavelId, int equipeId, int empresaId, bool usarCommit = false)
        {
            try
            {
                await _transferenciaCommand.ExecutarSemOportunidadeAsync(leadId, novoResponsavelId, equipeId, empresaId);

                if (usarCommit)
                    await _unitOfWork.CommitAsync();
                else
                    await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao transferir lead {LeadId} para usuário {NovoResponsavelId}",
                    leadId, novoResponsavelId);
                throw;
            }
        }


        /// <summary>
        /// Transfere todos os leads de um usuário desativado para um novo responsável
        /// </summary>
        //public async Task TransferirTodosLeadsDoUsuarioAsync(Domain.Entities.Usuario.Usuario usuarioDesativado, int novoResponsavelId, int empresaPadraoId)
        //{
        //    var membrosUsuario = await _membroEquipeReaderService.ObterMembrosPorUsuarioAsync(usuarioDesativado.Id);

        //    if (membrosUsuario == null || !membrosUsuario.Any())
        //    {
        //        _logger.LogWarning("Usuário {UsuarioId} não possui vínculos de equipe.", usuarioDesativado.Id);
        //        return;
        //    }

        //    // mapeia os leads com suas respectivas equipes
        //    var leadsDoUsuario = membrosUsuario
        //        .Where(m => m.LeadsSobResponsabilidade != null && m.LeadsSobResponsabilidade.Any())
        //        .SelectMany(m => m.LeadsSobResponsabilidade.Select(l => new
        //        {
        //            LeadItem = l,
        //            EquipeId = m.EquipeId
        //        }))
        //        .ToList();

        //    if (!leadsDoUsuario.Any())
        //    {
        //        _logger.LogInformation("Usuário {UsuarioId} não possui leads sob responsabilidade.", usuarioDesativado.Id);
        //        return;
        //    }

        //    var transferidosComSucesso = 0;
        //    var erros = 0;

        //    foreach (var item in leadsDoUsuario)
        //    {
        //        var lead = item.LeadItem;
        //        var equipeId = item.EquipeId;

        //        try
        //        {
        //            await TransferirLeadAsync(lead.Id, novoResponsavelId, equipeId, empresaPadraoId, usarCommit: false);
        //            transferidosComSucesso++;
        //        }
        //        catch (Exception ex)
        //        {
        //            erros++;
        //            _logger.LogError(ex,
        //                "Erro ao transferir lead {LeadId} (Equipe {EquipeId}) do usuário desativado {UsuarioDesativadoId}",
        //                lead.Id, equipeId, usuarioDesativado.Id);
        //        }
        //    }
        //}

        public async Task TransferirLeadParaEquipePadraoAsync(int leadId, int empresaId)
        {
            try
            {
                var empresa = await _empresaReaderService.ObterPorId(empresaId)
                    ?? throw new AppException("Empresa não encontrada.");

                var equipePadrao = await _equipeReaderService.GetEquipePadraoAsync(empresaId)
                    ?? throw new AppException("Equipe padrão não encontrada para a empresa.");

                (bool sucess, string message, DistribuicaoAutomaticaEquipeResponseDTO? response) = await _distribuicaoWriterService.ExecutarDistribuicaoAutomaticaPorEquipe(leadId, empresaId, equipePadrao.Id);

                var membroId = response?.ResponsavelId;
                if (!membroId.HasValue)
                {
                    var lider = await _membroEquipeReaderService.ObterLiderDaEquipeAsync(equipePadrao.Id);
                    membroId = lider!.Id;
                }

                var usuario = await _usuarioReaderService.ObterVendedorPorMembroId(membroId.Value) ?? throw new AppException($"Usuário com id do membro {membroId} não foi encontrado.");

                await _transferenciaCommand.ExecutarAsync(
                    leadId,
                    usuario.Id,
                    equipePadrao.Id,
                    empresaId
                );

                await _unitOfWork.CommitAsync();

                await _notificacaoWriterService.NovoLead(new NotificarNovoLeadDTO
                {
                    LeadId = leadId,
                    UsuarioId = usuario.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao transferir lead {LeadId} para o líder da equipe",
                    leadId);
                throw;
            }
        }

    }
}
