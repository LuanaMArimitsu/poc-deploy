using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.ControleSistemasExternos
{
    internal class SistemaExternoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), ISistemaExternoRepository
    {
        public async Task<SistemaExterno> GetSistemaExterno(string nome)
        {
            try
            {
                var sistemaExterno = await _context.SistemaExterno.Where(s => s.Nome == nome).FirstOrDefaultAsync();
                return sistemaExterno;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SistemaExterno?> GetSistemaExternoPorCredenciais(string nome, string cnpj)
        {
            try
            {
                var sistemaExterno = await _context.SistemaExterno
                    .Where(s => s.Nome == nome && s.URL_API.Contains(cnpj))
                    .FirstOrDefaultAsync();
                return sistemaExterno;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
