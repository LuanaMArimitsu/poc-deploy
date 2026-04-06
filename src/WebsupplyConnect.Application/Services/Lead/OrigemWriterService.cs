using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Validators.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class OrigemWriterService : IOrigemWriterService
    {
        private readonly IOrigemRepository _origemRepository;
        private readonly ITipoOrigemRepository _tipoOrigemRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly OrigemResquestDTOValidator _validator;
        private readonly ILogger<OrigemWriterService> _logger;

        public OrigemWriterService(
            IOrigemRepository origemRepository,
            ITipoOrigemRepository tipoOrigemRepository,
            IUnitOfWork unitOfWork,
            OrigemResquestDTOValidator origemRequestDTOValidator,
            ILogger<OrigemWriterService> logger)
        {
            _origemRepository = origemRepository ?? throw new ArgumentNullException(nameof(origemRepository));
            _tipoOrigemRepository = tipoOrigemRepository ?? throw new ArgumentNullException(nameof(tipoOrigemRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _validator = origemRequestDTOValidator ?? throw new ArgumentNullException(nameof(origemRequestDTOValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CreateAsync(OrigemRequest request)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new AppException($"Dados inválidos para cadastro de origem: {errors}");
                }

                var origemTipoExists = await _origemRepository.ExistsInDatabaseAsync<OrigemTipo>(request.OrigemTipoId);
                if (!origemTipoExists)
                {
                    throw new AppException($"Tipo de origem com ID {request.OrigemTipoId} não existe.");
                }

                var novaOrigem = new Origem(
                    nome: request.Nome,
                    origemTipoId: request.OrigemTipoId,
                    descricao: request.Descricao);

                await _unitOfWork.BeginTransactionAsync();
                await _origemRepository.CreateAsync(novaOrigem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar dados para criação de origem.");
                throw;
            }
        }

        public async Task UpdateOrigemAsync(int id, UpdateOrigemDTO updateOrigemDTO)
        {
            try
            {
                var origem = await _origemRepository.GetByIdAsync<Origem>(id) ?? throw new ApplicationException($"Origem com id {id} não encontrada.");

                var nome = updateOrigemDTO.Nome ?? origem.Nome;
                var descricao = updateOrigemDTO.Descricao ?? origem.Descricao;
                var origemTipoId = updateOrigemDTO.OrigemTipoId ?? origem.OrigemTipoId;

                if (updateOrigemDTO.OrigemTipoId.HasValue)
                {
                    var origemTipo = await _tipoOrigemRepository.GetByIdAsync<OrigemTipo>(updateOrigemDTO.OrigemTipoId.Value)
                        ?? throw new ApplicationException($"Tipo de origem com id {updateOrigemDTO.OrigemTipoId.Value} não encontrado.");
                }

                await _unitOfWork.BeginTransactionAsync();
                origem.Atualizar(nome, origemTipoId, descricao);
                _origemRepository.Update(origem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Erro ao atualizar origem");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var origem = await _origemRepository.GetByIdAsync<Origem>(id);
                if (origem == null)
                {
                    throw new AppException($"Origem com ID {id} não encontrada.");
                }
                await _unitOfWork.BeginTransactionAsync();
                origem.ExcluirLogicamente();

                _origemRepository.Update(origem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir origem com ID {OrigemId}.", id);
                throw;
            }
        }
    }
}
