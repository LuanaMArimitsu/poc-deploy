using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.VersaoApp;
using WebsupplyConnect.Application.Interfaces.VersaoApp;
using WebsupplyConnect.Application.Services.Lead;
using WebsupplyConnect.Domain.Interfaces.VersaoApp;

namespace WebsupplyConnect.Application.Services.VersaoApp
{
    public class VersaoAppReaderService(ILogger<VersaoAppReaderService> logger, IVersaoAppRepository versaoAppRepository) : IVersaoAppReaderService
    {
        private readonly ILogger<VersaoAppReaderService> _logger = logger;
        private readonly IVersaoAppRepository _versaoAppRepository = versaoAppRepository;

        public async Task<VersaoAppRetornoDTO> GetUltimaVersaoAppAsync(string? plataformaApp)
        {
            try
            {
                var versaoApp = await _versaoAppRepository.GetUltimaVersaoAppAsync(plataformaApp);

                if (versaoApp == null)
                    throw new AppException("Nenhuma versão foi encontrada.");

                return new VersaoAppRetornoDTO
                {
                    Versao = versaoApp.Versao,
                    PlataformaApp = versaoApp.PlataformaApp,
                    AtualizacaoObrigatoria = versaoApp.AtualizacaoObrigatoria,
                    DataCriacao = versaoApp.DataCriacao,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a última versão do app");
                throw;
            }
        }
    }
}
