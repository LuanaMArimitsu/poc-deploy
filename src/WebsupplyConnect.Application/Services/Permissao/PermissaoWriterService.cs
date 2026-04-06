using WebsupplyConnect.Application.DTOs.Permissao.Permissao;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Permissao;

namespace WebsupplyConnect.Application.Services.Permissao
{
    public class PermissaoWriterService(
        IPermissaoRepository permissaoRepository, IUnitOfWork unitOfWork) : IPermissaoWriterService
    {
        private readonly IPermissaoRepository _permissaoRepository = permissaoRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        /// <summary>
        /// Cria uma nova permissão no sistema.
        /// </summary>
        /// <param name="codigo">Código único da permissão</param>
        /// <param name="nome">Nome descritivo da permissão</param>
        /// <param name="descricao">Descrição detalhada da permissão</param>
        /// <param name="modulo">Módulo ao qual pertence</param>
        /// <param name="categoria">Categoria da permissão</param>
        /// <param name="acao">Ação que permite</param>
        /// <param name="recurso">Recurso/endpoint controlado</param>
        /// <param name="isCritica">Se é crítica para o sistema</param>
        public async Task CriarPermissaoAsync(CreatePermissaoDTO dto)
        {
            try
            {
                // Validação de unicidade do código
                var permissaoExistente = await _permissaoRepository.GetByPredicateAsync<Domain.Entities.Permissao.Permissao>(p => p.Codigo == dto.Codigo);
                if (permissaoExistente != null)
                    throw new InvalidOperationException("Já existe uma permissão com este código.");

                var permissao = new Domain.Entities.Permissao.Permissao(
                    dto.Codigo,
                    dto.Nome,
                    dto.Descricao,
                    dto.Modulo,
                    dto.Categoria,
                    dto.Acao,
                    dto.Recurso,
                    dto.IsCritica
                );

                var permissaoCriada = await _permissaoRepository.CreateAsync(permissao);
                await _unitOfWork.CommitAsync();
            } catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao criar permissão: " + ex.Message);
            }
            
        }

        /// <summary>
        /// Exclui logicamente uma permissão pelo ID.
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        public async Task ExcluirPermissaoAsync(int permissaoId)
        {
            try
            {
                var permissao = await _permissaoRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId);
                if (permissao == null)
                    throw new InvalidOperationException("Permissão não encontrada.");

                permissao.ExcluirLogicamente();
                _permissaoRepository.Update(permissao);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao excluir permissão: " + ex.Message);
            }
        }

        /// <summary>
        /// Atualiza os dados de uma permissão existente.
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        /// <param name="nome">Novo nome</param>
        /// <param name="descricao">Nova descrição</param>
        /// <param name="isCritica">Se é crítica para o sistema</param>
        public async Task AtualizarPermissaoAsync(int permissaoId, string nome, string descricao, bool isCritica)
        {
            try
            {
                var permissao = await _permissaoRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId);
                if (permissao == null)
                    throw new InvalidOperationException("Permissão não encontrada.");

                permissao.AtualizarInformacoes(nome, descricao, permissao.Recurso);

                if (isCritica && !permissao.IsCritica)
                    permissao.MarcarComoCritica();
                else if (!isCritica && permissao.IsCritica)
                    permissao.RemoverMarcacaoCritica();

                _permissaoRepository.Update(permissao);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao atualizar permissão: " + ex.Message);
            }
        }

        /// <summary>
        /// Desativa uma permissão pelo ID.
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        public async Task DesativarPermissaoAsync(int permissaoId)
        {
            try
            {
                var permissao = await _permissaoRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId);
                if (permissao == null)
                    throw new InvalidOperationException("Permissão não encontrada.");

                permissao.Desativar();
                _permissaoRepository.Update(permissao);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao desativar permissão: " + ex.Message);
            }
        }

        /// <summary>
        /// Reativa uma permissão logicamente inativa pelo ID.
        /// </summary>
        /// <param name="permissaoId">ID da permissão</param>
        public async Task ReativarPermissaoAsync(int permissaoId)
        {
            try
            {
                // Busca incluindo logicamente excluídos
                var permissao = await _permissaoRepository.GetByIdAsync<Domain.Entities.Permissao.Permissao>(permissaoId, includeDeleted: false);
                if (permissao == null)
                    throw new InvalidOperationException("Permissão não encontrada.");

                permissao.Ativar();
                _permissaoRepository.Update(permissao);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Erro ao reativar permissão: " + ex.Message);
            }
        }
    }
}
