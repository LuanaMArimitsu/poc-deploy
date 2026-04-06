using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class EnderecoWriterService(IUnitOfWork unitOfWork, IEnderecoRepository enderecoRepository) : IEnderecoWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IEnderecoRepository _enderecoRepository = enderecoRepository;

        public async Task<int> CriarEnderecoAsync(Endereco endereco)
        {
            var created = await _enderecoRepository.CreateAsync(endereco);
            await _unitOfWork.SaveChangesAsync();
            return created.Id;
        }
        public async Task<bool> ExcluirEnderecoAsync(int enderecoId)
        {
            var endereco = await _enderecoRepository.GetByIdAsync<Endereco>(enderecoId);
            if (endereco == null)
                return false;
            if (endereco.Excluido)
                return true;

            endereco.ExcluirLogicamente();
            _enderecoRepository.Update(endereco);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task EditarEnderecoAsync(EditarEnderecoDTO dto)
        {
            try
            {
                var endereco = await _enderecoRepository.GetByIdAsync<Endereco>(dto.EnderecoId);
                if (endereco == null)
                    throw new AppException($"Endereço com ID {dto.EnderecoId} não encontrado.");

                endereco.Atualizar(
                    dto.Logradouro,
                    dto.Numero,
                    dto.Bairro,
                    dto.Cidade,
                    dto.Estado,
                    dto.Cep,
                    dto.Complemento,
                    dto.Pais
                );

                _enderecoRepository.Update(endereco);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                //_logger.LogError(ex, $"Erro ao editar endereço com ID {dto.EnderecoId}");
                throw new AppException("Erro ao editar endereço", ex);
            }
        }
    }
}