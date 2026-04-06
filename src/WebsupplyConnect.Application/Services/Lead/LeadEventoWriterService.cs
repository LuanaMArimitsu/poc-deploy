using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Evento;
using WebsupplyConnect.Application.DTOs.Lead.Historico;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadEventoWriterService : ILeadEventoWriterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeadEventoWriterService> _logger;
        private readonly ILeadEventoRepository _repository;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IMembroEquipeReaderService _membroEquipeReaderService;

        public LeadEventoWriterService(IUnitOfWork unitOfWork, ILogger<LeadEventoWriterService> logger, ILeadEventoRepository repository, IUsuarioReaderService usuarioReaderService, IMembroEquipeReaderService membroEquipeReaderService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _repository = repository;
            _usuarioReaderService = usuarioReaderService;
            _membroEquipeReaderService = membroEquipeReaderService;
        }

        public async Task RegistrarEventoAsync(Domain.Entities.Lead.Lead lead, int? campanhaId = null, string? observacao = null, int? origemId = null)
        {
            try { 

                if (lead == null)
                    throw new DomainException("Lead não pode ser nulo", nameof(LeadEvento));

                var historico = new LeadEvento(
                    leadId: lead.Id,
                    origemId: origemId ?? lead.OrigemId,
                    canalId: null,
                    campanhaId: campanhaId,
                    observacao: observacao);

                await _repository.CreateAsync(historico);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evento de criação via API registrado para o lead {LeadId}", lead.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar evento de criação via API para o lead {LeadId}", lead?.Id);
                throw;
            }

        }

        public async Task RegistrarEventoViaWhatsAsync(Domain.Entities.Lead.Lead lead, int canalId , int? campanhaId = null)
        {
            try
            {
                if (lead == null)
                    throw new DomainException("O lead não pode ser nulo ao registrar evento via WhatsApp.", nameof(LeadEvento));

                var historico = new LeadEvento(
                    leadId: lead.Id,
                    origemId: lead.OrigemId,
                    canalId: canalId,
                    campanhaId: campanhaId,
                    observacao: "Lead criado via WhatsApp *"
                );

                await _repository.CreateAsync(historico);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evento via WhatsApp registrado para o lead {LeadId}", lead.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar evento via WhatsApp para o lead {LeadId}", lead?.Id);
                throw;
            }
        }

        public async Task RegistrarEventoConversaViaWhatsAsync(Domain.Entities.Lead.Lead lead, int canalId, int? campanhaId = null)
        {
            try
            {
                if (lead == null)
                    throw new DomainException("O lead não pode ser nulo ao registrar evento via WhatsApp.", nameof(LeadEvento));

                var historico = new LeadEvento(
                    leadId: lead.Id,
                    origemId: lead.OrigemId,
                    canalId: canalId,
                    campanhaId: campanhaId,
                    observacao: "Lead iniciou conversa via WhatsApp *"
                );

                await _repository.CreateAsync(historico);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar evento conversa via WhatsApp para o lead {LeadId}", lead?.Id);
                throw;
            }
        }

        public async Task RegistrarEventoManualAsync(LeadEventoDTO dto)
        {
            try
            {
                if (dto.LeadId <= 0)
                    throw new AppException("LeadId é obrigatório.");

                if (dto.OrigemId <= 0)
                    throw new AppException("OrigemId é obrigatório.");

                await _unitOfWork.BeginTransactionAsync();

                var historico = new LeadEvento(
                    leadId: dto.LeadId,
                    origemId: dto.OrigemId,
                    canalId: dto.CanalId,
                    campanhaId: dto.CampanhaId,
                    observacao: dto.Observacao
                );

                await _repository.CreateAsync(historico);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao registrar histórico manualmente");
                throw;
            }
        }

        public async Task UpdateEventoAsync(int eventoId, LeadEventoUpdateDTO dto)
        {
            try
            {
                if (eventoId <= 0)
                    throw new AppException("Id do evento inválido.");

                var evento = await _repository.GetByIdAsync<LeadEvento>(eventoId);
                if (evento == null)
                    throw new AppException($"Evento com Id {eventoId} não encontrado.");

                await _unitOfWork.BeginTransactionAsync();

                evento.AtualizarEvento(
                    dto.OrigemId,
                    dto.CanalId,
                    dto.CampanhaId,
                    dto.Observacao
                );

                _repository.Update(evento);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, $"Erro ao atualizar LeadEvento ID {eventoId}");
                throw;
            }
        }

        public async Task TransferirLeadsAsync(int campanhaOrigemId, int campanhaDestinoId)
        {
            try
            {
                var leads = await _repository.GetListByPredicateAsync<LeadEvento>(l => l.CampanhaId == campanhaOrigemId);
                if (leads == null)
                    throw new AppException("Nenhum lead encontrado para a campanha de origem.");

                foreach (var lead in leads)
                {
                    lead.AssociarCampanha(campanhaDestinoId);
                    _repository.Update(lead);
                }
                
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new AppException("Erro ao transferir leads da campanha.", ex);
            }
        }
    }
}
