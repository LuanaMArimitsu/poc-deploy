using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Distribuicao
{
    /// <summary>
    /// Implementação do repositório para operações na fila de distribuição
    /// </summary>
    internal class FilaDistribuicaoRepository : BaseRepository, IFilaDistribuicaoRepository
    {
        public FilaDistribuicaoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        /// <summary>
        /// Obtém a posição de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <returns>Objeto com a posição do vendedor ou null se não estiver na fila</returns>
        public async Task<FilaDistribuicao?> GetPosicaoVendedorAsync(int empresaId, int vendedorId)
        {
            return await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId &&
                           f.MembroEquipeId == vendedorId &&
                           !f.Excluido)
                .FirstOrDefaultAsync();
        }

        public async Task<FilaDistribuicao?> GetPosicaoVendedorExcluidoAsync(int empresaId, int vendedorId)
        {
            return await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId &&
                           f.MembroEquipeId == vendedorId &&
                           f.Excluido)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FilaDistribuicao>> GetPosicaoVendedoresAsync(List<MembroEquipe> membrosEquipes)
        {
            var ids = membrosEquipes.Select(m => m.Id).ToList();

            return await _context.Set<FilaDistribuicao>()
                .Where(f => ids.Contains(f.MembroEquipeId) && !f.Excluido)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém o próximo vendedor da fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Indica se deve retornar apenas vendedores com status ativo</param>
        /// <returns>Objeto com a posição do próximo vendedor ou null se não houver</returns>
        public async Task<FilaDistribuicao?> GetProximoVendedorFilaAsync(int empresaId, bool apenasAtivos = true)
        {
            var query = _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId && !f.Excluido);

            if (apenasAtivos)
            {
                // Caso precise filtrar por status ativo - usar ID de status conhecidos
                var statusAtivo = await GetStatusFilaIdPorCodigoAsync("ATIVO");
                query = query.Where(f => f.StatusFilaDistribuicaoId == statusAtivo);
            }

            return await query
                .OrderBy(f => f.PosicaoFila)
                .Include(f => f.MembroEquipe)
                    .ThenInclude(f => f.Usuario)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Registra a atribuição de um lead a um vendedor
        /// </summary>
        /// <param name="posicaoFilaId">ID da posição na fila</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        public async Task<bool> RegistrarAtribuicaoLeadAsync(int posicaoFilaId)
        {
            var posicaoFila = await _context.Set<FilaDistribuicao>()
                .Where(f => f.Id == posicaoFilaId && !f.Excluido)
                .FirstOrDefaultAsync();

            if (posicaoFila == null)
                return false;

            // Registra o recebimento do lead
            posicaoFila.RegistrarRecebimentoLead();

            _context.Update(posicaoFila);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Reorganiza a fila após a distribuição de um lead
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor que recebeu o lead</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        public async Task<bool> ReorganizarFilaAposDistribuicaoAsync(int empresaId, int vendedorId)
        {
            // Obter o vendedor que recebeu o lead
            var vendedorAtual = await GetPosicaoVendedorAsync(empresaId, vendedorId);
            if (vendedorAtual == null)
                return false;

            // Obter a última posição na fila
            var ultimaPosicao = await GetUltimaPosicaoFilaAsync(empresaId);

            // Mover o vendedor para o final da fila
            vendedorAtual.MoverParaFinalDaFila(ultimaPosicao);

            // Atualizar a posição dos outros vendedores
            var vendedoresParaAtualizar = await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId &&
                           f.MembroEquipeId != vendedorId &&
                           f.PosicaoFila > vendedorAtual.PosicaoFila &&
                           !f.Excluido)
                .OrderBy(f => f.PosicaoFila)
                .ToListAsync();

            foreach (var v in vendedoresParaAtualizar)
            {
                v.AtualizarPosicaoFila(v.PosicaoFila - 1);
                _context.Update(v);
            }

            _context.Update(vendedorAtual);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Adiciona um vendedor à fila de distribuição
        /// </summary>
        /// <param name="filaDistribuicao">Objeto com as informações da posição na fila</param>
        /// <returns>Objeto adicionado com ID gerado</returns>
        public async Task<FilaDistribuicao> AdicionarVendedorFilaAsync(FilaDistribuicao filaDistribuicao)
        {
            // Verificar se o vendedor já está na fila
            var existente = await GetPosicaoVendedorAsync(filaDistribuicao.EmpresaId, filaDistribuicao.MembroEquipeId);
            if (existente != null)
            {
                // Se já existir, apenas atualiza o status
                existente.AtualizarStatus(filaDistribuicao.StatusFilaDistribuicaoId);

                _context.Update(existente);
                await _context.SaveChangesAsync();
                return existente;
            }

            // Adicionar novo vendedor à fila
            await _context.Set<FilaDistribuicao>().AddAsync(filaDistribuicao);
            await _context.SaveChangesAsync();
            return filaDistribuicao;
        }

        /// <summary>
        /// Atualiza o status de um vendedor na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="statusId">ID do novo status</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        public async Task<bool> AtualizarStatusVendedorAsync(int empresaId, int vendedorId, int statusId)
        {
            var posicaoFila = await GetPosicaoVendedorAsync(empresaId, vendedorId);
            if (posicaoFila == null)
                return false;

            posicaoFila.AtualizarStatus(statusId);

            _context.Update(posicaoFila);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Remove um vendedor da fila de distribuição
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <returns>True se removido com sucesso, false caso contrário</returns>
        public async Task<bool> RemoverVendedorFilaAsync(int empresaId, int vendedorId)
        {
            var posicaoFila = await GetPosicaoVendedorAsync(empresaId, vendedorId);
            if (posicaoFila == null)
                return false;

            // Marcar como excluído
            posicaoFila.Excluir();

            // Reorganizar as posições dos outros vendedores
            var vendedoresParaAtualizar = await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId &&
                           f.MembroEquipeId != vendedorId &&
                           f.PosicaoFila > posicaoFila.PosicaoFila &&
                           !f.Excluido)
                .OrderBy(f => f.PosicaoFila)
                .ToListAsync();

            foreach (var v in vendedoresParaAtualizar)
            {
                v.AtualizarPosicaoFila(v.PosicaoFila - 1);
                _context.Update(v);
            }

            _context.Update(posicaoFila);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoverTodosVendedoresFilaAsync(List<MembroEquipe> membrosEquipe)
        {
            var posicoesFila = await GetPosicaoVendedoresAsync(membrosEquipe);
            if (posicoesFila == null || posicoesFila.Count == 0)
                return false;

            // Marca como excluído
            foreach (var posicao in posicoesFila)
            {
                posicao.Excluir();
                _context.Update(posicao);
            }

            var empresaId = posicoesFila.First().EmpresaId;

            // Agora pega todos os vendedores não excluídos e reindexa a fila
            var vendedoresParaAtualizar = await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId && !f.Excluido)
                .OrderBy(f => f.PosicaoFila)
                .ToListAsync();

            int novaPosicao = 1;
            foreach (var vendedor in vendedoresParaAtualizar)
            {
                vendedor.AtualizarPosicaoFila(novaPosicao++);
                _context.Update(vendedor);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task RestaurarVendedorNaFilaAsync(int empresaId, int vendedorId)
        {
            var posicaoFila = await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId &&
                           f.MembroEquipeId == vendedorId &&
                           f.Excluido)
                .FirstOrDefaultAsync();
            if (posicaoFila == null)
                return;

            // Obter a última posição na fila
            var ultimaPosicao = await GetUltimaPosicaoFilaAsync(empresaId);

            // Restaurar o vendedor e mover para o final da fila
            posicaoFila.Restaurar();
            posicaoFila.MoverParaFinalDaFila(ultimaPosicao);
            _context.Update(posicaoFila);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtém a última posição atual na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Valor da última posição ou 0 se a fila estiver vazia</returns>
        public async Task<int> GetUltimaPosicaoFilaAsync(int empresaId)
        {
            return await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId && !f.Excluido)
                .Select(f => (int?)f.PosicaoFila)
                .OrderByDescending(p => p)
                .FirstOrDefaultAsync() ?? 0;
        }

        /// <summary>
        /// Obtém o ID do status da fila pelo código
        /// </summary>
        /// <param name="codigo">Código do status (ex: "ATIVO", "PAUSADO")</param>
        /// <returns>ID do status ou 0 se não encontrado</returns>
        public async Task<int> GetStatusFilaIdPorCodigoAsync(string codigo)
        {
            var status = await _context.Set<StatusFilaDistribuicao>()
                .Where(s => s.Codigo.ToUpper() == codigo.ToUpper() && !s.Excluido)
                .Select(s => new { s.Id })
                .FirstOrDefaultAsync();

            return status?.Id ?? 0;
        }

        /// <summary>
        /// Lista todos os vendedores na fila de uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="apenasAtivos">Indica se deve retornar apenas vendedores com status ativo</param>
        /// <returns>Lista de vendedores na fila</returns>
        public async Task<List<FilaDistribuicao>> ListarVendedoresFilaAsync(int empresaId, bool apenasAtivos = false)
        {
            var query = _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId && !f.Excluido);

            if (apenasAtivos)
            {
                // Caso precise filtrar por status ativo
                var statusAtivo = await GetStatusFilaIdPorCodigoAsync("ATIVO");
                query = query.Where(f => f.StatusFilaDistribuicaoId == statusAtivo);
            }

            return await query
                .OrderBy(f => f.PosicaoFila)
                .Include(f => f.MembroEquipeId)
                .ToListAsync();
        }
        
        /// <summary>
        /// Obtém o status da fila pelo ID
        /// </summary>
        /// <param name="statusId">ID do status</param>
        /// <returns>Entidade de status da fila ou null se não encontrado</returns>
        public async Task<StatusFilaDistribuicao?> GetStatusFilaByIdAsync(int statusId)
        {
            return await _context.Set<StatusFilaDistribuicao>()
                .Where(s => s.Id == statusId && !s.Excluido)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Registra a atribuição de um lead a um vendedor
        /// </summary>
        /// <param name="posicaoFilaId">ID da posição na fila</param>
        /// <param name="leadId">ID do lead atribuído</param>
        /// <returns>True se atualizado com sucesso, false caso contrário</returns>
        public async Task<bool> RegistrarAtribuicaoLeadAsync(int posicaoFilaId, int leadId)
        {
            var posicaoFila = await _context.Set<FilaDistribuicao>()
                .Where(f => f.Id == posicaoFilaId && !f.Excluido)
                .FirstOrDefaultAsync();

            if (posicaoFila == null)
                return false;

            // Registra o recebimento do lead
            posicaoFila.RegistrarRecebimentoLead();
            
            _context.Update(posicaoFila);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Adiciona uma nova posição na fila
        /// </summary>
        /// <param name="filaDistribuicao">Objeto com as informações da posição na fila</param>
        /// <returns>Objeto adicionado com ID gerado</returns>
        public async Task<FilaDistribuicao> AddPosicaoFilaAsync(FilaDistribuicao filaDistribuicao)
        {
            await _context.Set<FilaDistribuicao>().AddAsync(filaDistribuicao);
            await _context.SaveChangesAsync();
            return filaDistribuicao;
        }

        /// <summary>
        /// Obtém a próxima posição disponível na fila
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Próxima posição disponível</returns>
        public async Task<int> GetProximaPosicaoFilaAsync(int empresaId)
        {
            var ultimaPosicao = await GetUltimaPosicaoFilaAsync(empresaId);
            return ultimaPosicao + 1;
        }
        
        /// <summary>
        /// Obtém todos os vendedores na fila de distribuição para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Lista de posições dos vendedores na fila</returns>
        public async Task<List<FilaDistribuicao>> GetVendedoresNaFilaAsync(int empresaId)
        {
            return await _context.Set<FilaDistribuicao>()
                .Where(f => f.EmpresaId == empresaId && !f.Excluido)
                .Include(f => f.StatusFilaDistribuicao)
                .OrderBy(f => f.PosicaoFila)
                .ToListAsync();
        }
    }
}