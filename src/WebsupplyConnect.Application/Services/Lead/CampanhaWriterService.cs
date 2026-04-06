using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Campanha;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class CampanhaWriterService(IUnitOfWork unitOfWork, ICampanhaRepository campanhaRepository, ILeadEventoWriterService leadEventoWriterService) : ICampanhaWriterService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ICampanhaRepository _campanhaRepository = campanhaRepository ?? throw new ArgumentNullException(nameof(campanhaRepository));
        private readonly ILeadEventoWriterService _leadEventoWriterService = leadEventoWriterService ?? throw new ArgumentNullException(nameof(leadEventoWriterService));

        /// <summary>
        /// Cria uma nova campanha e persiste no banco de dados.
        /// </summary>
        public async Task CriarCampanhaAsync(CriarCampanhaApiDTO dto, bool commit)
        {
            try
            {
                var campanhaExistente = await _campanhaRepository.GetByPredicateAsync<Campanha>(
                    c => c.Codigo == dto.Codigo && c.EmpresaId == dto.EmpresaId, false);

                if (campanhaExistente != null)
                {
                    throw new AppException($"Já existe uma campanha com o código '{dto.Codigo}' para esta empresa.");
                }

                var campanha = Campanha.Criar(dto.Nome, dto.Codigo, dto.DataInicio, dto.DataFim, dto.EmpresaId, dto.Temporaria, dto.EquipeId);

                await _campanhaRepository.CreateAsync(campanha);

                await _unitOfWork.SaveChangesAsync();
                if (commit)
                {
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception)
            {
                if (commit)
                    await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Edita uma campanha e persiste no banco de dados.
        /// </summary>
        public async Task EditarCampanhaAsync(int id, EditarCampanhaDTO dto)
        {
            try
            {
                var campanha = await _campanhaRepository.GetByIdAsync<Campanha>(id, true);
                if (campanha == null)
                    throw new AppException("Campanha não encontrada.");

                if (string.IsNullOrWhiteSpace(dto.Nome))
                    throw new AppException("O nome da campanha não pode ser vazio.");

                if (string.IsNullOrWhiteSpace(dto.Codigo))
                    throw new AppException("O código da campanha não pode ser vazio.");

                if(dto.EquipeId <= 0)
                    throw new AppException("A equipe da campanha não pode ser vazia.");

                bool alterado = false;

                if (campanha.Nome != dto.Nome)
                {
                    campanha.Nome = dto.Nome;
                    alterado = true;
                }
                if (campanha.Codigo != dto.Codigo)
                {
                    var campanhaExistente = await _campanhaRepository.GetByPredicateAsync<Campanha>(
                        c => c.Codigo == dto.Codigo && c.EmpresaId == campanha.EmpresaId && c.Id != campanha.Id, true);

                    if (campanhaExistente != null)
                        throw new AppException($"Já existe uma campanha com o código '{dto.Codigo}' para esta empresa.");

                    campanha.Codigo = dto.Codigo;
                    alterado = true;
                }
                if (campanha.Ativo != dto.Ativo)
                {
                    campanha.Ativo = dto.Ativo;
                    alterado = true;
                }

                if(dto.Temporaria == true && campanha.Temporaria == false)
                {
                    throw new AppException("Não é possível alterar uma campanha definitiva para temporária.");
                }

                if (campanha.Temporaria != dto.Temporaria)
                {
                    campanha.Temporaria = dto.Temporaria;
                    alterado = true;
                }
                if (campanha.DataInicio != dto.DataInicio)
                {
                    campanha.DataInicio = dto.DataInicio;
                    alterado = true;
                }
                if (campanha.DataFim != dto.DataFim)
                {
                    campanha.DataFim = dto.DataFim;
                    alterado = true;
                }

                
                if (alterado)
                {
                    await _unitOfWork.BeginTransactionAsync();
                    _campanhaRepository.Update(campanha);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Soft delete de uma campanha no banco de dados.
        /// </summary>
        public async Task DeleteCampanhaAsync(int id)
        {
            try
            {
                var campanha = await _campanhaRepository.GetByIdAsync<Campanha>(id, true);
                if (campanha == null || campanha.Excluido)
                    throw new AppException("Campanha não encontrada.");

                await _unitOfWork.BeginTransactionAsync();

                campanha.ExcluirLogicamente();
                _campanhaRepository.Update(campanha);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        //Transferir campanhas temporárias para campanhas definitivas
        public async Task TransferirCampanhasAsync (int campanhaOrigemId, int campanhaDestinoId)
        {
            try
            {
                var campanhaOrigem = await _campanhaRepository.GetByIdAsync<Campanha>(campanhaOrigemId, true);
                if (campanhaOrigem == null || campanhaOrigem.Excluido)
                    throw new AppException("Campanha de origem não encontrada.");

                var campanhaDestino = await _campanhaRepository.GetByIdAsync<Campanha>(campanhaDestinoId, true);
                if (campanhaDestino == null || campanhaDestino.Excluido)
                    throw new AppException("Campanha de destino não encontrada.");

                if (!campanhaOrigem.Temporaria)
                    throw new AppException("Apenas campanhas temporárias podem ser transferidas.");

                await _unitOfWork.BeginTransactionAsync();

                await _leadEventoWriterService.TransferirLeadsAsync(campanhaOrigemId, campanhaDestinoId);

                campanhaOrigem.VincularCampanha(campanhaDestinoId);

                _campanhaRepository.Update(campanhaOrigem);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
