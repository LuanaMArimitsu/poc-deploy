using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comum;
using WebsupplyConnect.Application.Interfaces.Comum;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Interfaces.Comum;

namespace WebsupplyConnect.Application.Services.Comum
{
    public class FeriadoWriterService(ILogger<FeriadoWriterService> logger, IFeriadoRepository feriadoRepository, IValidator<FeriadoCriarDTO> validatorCriar,
            IValidator<FeriadoAtualizarDTO> validatorAtualizar) : IFeriadoWriterService
    {
        private readonly ILogger<FeriadoWriterService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFeriadoRepository _feriadoRepository = feriadoRepository ?? throw new ArgumentNullException(nameof(feriadoRepository));
        private readonly IValidator<FeriadoCriarDTO> _validatorCriar = validatorCriar ?? throw new ArgumentNullException(nameof(validatorCriar));
        private readonly IValidator<FeriadoAtualizarDTO> _validatorAtualizar = validatorAtualizar ?? throw new ArgumentNullException(nameof(validatorAtualizar));

        /// <summary>
        /// Adiciona um novo feriado
        /// </summary>
        public async Task<FeriadoDTO> AdicionarAsync(FeriadoCriarDTO feriadoDTO)
        {
            try
            {
                _logger.LogInformation("Adicionando novo feriado: {Nome}", feriadoDTO.Nome);

                // Validar o DTO
                var validationResult = await _validatorCriar.ValidateAsync(feriadoDTO);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Falha na validação do feriado: {Errors}",
                        string.Join(", ", validationResult.Errors));
                    throw new ValidationAppException(validationResult.Errors);
                }

                // Criar a entidade a partir do DTO
                var feriado = new Feriado(
                    feriadoDTO.Nome,
                    feriadoDTO.Data,
                    feriadoDTO.Tipo,
                    feriadoDTO.Recorrente,
                    feriadoDTO.Descricao,
                    feriadoDTO.EmpresaId,
                    feriadoDTO.UF,
                    feriadoDTO.CodigoMunicipio);

                // Persistir a entidade
                var feriadoAdicionado = await _feriadoRepository.AddAsync(feriado);
                await _feriadoRepository.SaveChangesAsync();

                _logger.LogInformation("Feriado adicionado com sucesso. ID: {Id}", feriadoAdicionado.Id);

                // Mapear manualmente para DTO
                return MapToDTO(feriadoAdicionado);
            }
            catch (ValidationAppException)
            {
                // Já tratada, apenas propaga
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar feriado: {Message}", ex.Message);
                throw new AppException($"Erro ao adicionar feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza um feriado existente
        /// </summary>
        public async Task<FeriadoDTO> AtualizarAsync(FeriadoAtualizarDTO feriadoDTO)
        {
            try
            {
                _logger.LogInformation("Atualizando feriado ID: {Id}", feriadoDTO.Id);

                // Validar o DTO
                var validationResult = await _validatorAtualizar.ValidateAsync(feriadoDTO);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Falha na validação do feriado: {Errors}",
                        string.Join(", ", validationResult.Errors));
                    throw new ValidationAppException(validationResult.Errors);
                }

                // Buscar a entidade existente
                var feriadoExistente = await _feriadoRepository.GetByIdAsync<Feriado>(feriadoDTO.Id);
                if (feriadoExistente == null)
                {
                    _logger.LogWarning("Feriado não encontrado. ID: {Id}", feriadoDTO.Id);
                    throw new NotFoundAppException($"Feriado com ID {feriadoDTO.Id} não encontrado");
                }

                // Atualizar a entidade
                feriadoExistente.Atualizar(
                    feriadoDTO.Nome,
                    feriadoDTO.Data,
                    feriadoDTO.Tipo,
                    feriadoDTO.Recorrente,
                    feriadoDTO.Descricao,
                    feriadoDTO.EmpresaId,
                    feriadoDTO.UF,
                    feriadoDTO.CodigoMunicipio);

                // Persistir as alterações
                _feriadoRepository.Update(feriadoExistente);
                await _feriadoRepository.SaveChangesAsync();

                _logger.LogInformation("Feriado atualizado com sucesso. ID: {Id}", feriadoExistente.Id);

                // Mapear manualmente para DTO
                return MapToDTO(feriadoExistente);
            }
            catch (ValidationAppException)
            {
                // Já tratada, apenas propaga
                throw;
            }
            catch (NotFoundAppException)
            {
                // Já tratada, apenas propaga
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar feriado: {Message}", ex.Message);
                throw new AppException($"Erro ao atualizar feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove um feriado pelo seu ID
        /// </summary>
        public async Task<bool> RemoverAsync(int id)
        {
            try
            {
                _logger.LogInformation("Removendo feriado ID: {Id}", id);

                // Verificar se o feriado existe
                var feriado = await _feriadoRepository.GetByIdAsync<Feriado>(id);
                if (feriado == null)
                {
                    _logger.LogWarning("Feriado não encontrado para remoção. ID: {Id}", id);
                    throw new NotFoundAppException($"Feriado com ID {id} não encontrado");
                }

                // Remover o feriado (exclusão lógica)
                var resultado = await _feriadoRepository.RemoveAsync(id);
                await _feriadoRepository.SaveChangesAsync();

                _logger.LogInformation("Feriado removido com sucesso. ID: {Id}", id);
                return resultado;
            }
            catch (NotFoundAppException)
            {
                // Já tratada, apenas propaga
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover feriado: {Message}", ex.Message);
                throw new AppException($"Erro ao remover feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Método auxiliar para mapear uma entidade Feriado para um DTO
        /// </summary>
        /// <param name="feriado">Entidade Feriado a ser mapeada</param>
        /// <returns>FeriadoDTO correspondente</returns>
        private static FeriadoDTO MapToDTO(Feriado feriado)
        {
            return new FeriadoDTO
            {
                Id = feriado.Id,
                Nome = feriado.Nome,
                Data = feriado.Data,
                Descricao = feriado.Descricao,
                Tipo = feriado.Tipo,
                EmpresaId = feriado.EmpresaId,
                Recorrente = feriado.Recorrente,
                UF = feriado.UF,
                CodigoMunicipio = feriado.CodigoMunicipio,
                DataCriacao = feriado.DataCriacao,
                DataModificacao = feriado.DataModificacao
            };
        }
    }
}
