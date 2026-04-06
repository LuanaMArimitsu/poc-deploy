using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class MidiaReaderService(
        ILogger<MidiaReaderService> logger,
        IMidiaRepository midiaRepository) : IMidiaReaderService
    {
        private readonly ILogger<MidiaReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMidiaRepository _midiaRepository = midiaRepository ?? throw new ArgumentNullException(nameof(midiaRepository));

        public async Task<Midia> GetMidiaByIdAsync(int midiaId)
        {
            if (midiaId <= 0)
            {
                throw new AppException("midia Id não pode ser menor ou igual a zero.");
            }
            ;

            return await _midiaRepository.GetByIdAsync<Midia>(midiaId) ?? throw new AppException($"Nenhuma midia encontrada com o id: {midiaId}");
        }

        public async Task<MidiaStatusProcessamento> GetMidiaStatusProcessamentoAsync(string codigo)
        {
            try
            {
                var statusProcessamento = await _midiaRepository.GetMidiaStatusProcessamentoAsync(codigo) ?? throw new AppException($"Erro ao encontrar status com o código {codigo}.");
                return statusProcessamento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encontrar status com o código {codigo}.", codigo);
                throw;
            }
        }

        public async Task<Midia> GetMidiaByMensagemIdAsync(int mensagemId)
        {
            return await _midiaRepository.GetMidiaByMensagemId(mensagemId);
        }

        public ResultadoValidacaoArquivo ValidarArquivo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return new ResultadoValidacaoArquivo
                {
                    Valido = false,
                    Erro = "Arquivo inválido"
                };
            }

            var regra = Regras.FirstOrDefault(r => r.ContentTypes.Contains(arquivo.ContentType));

            if (regra == null)
            {
                return new ResultadoValidacaoArquivo
                {
                    Valido = false,
                    Erro = "Tipo de arquivo não permitido"
                };
            }

            if (arquivo.Length > regra.TamanhoMaximoBytes)
            {
                var limiteMb = regra.TamanhoMaximoBytes / 1024 / 1024;
                return new ResultadoValidacaoArquivo
                {
                    Valido = false,
                    Erro = $"Mídia excede o limite permitido de {limiteMb} MB."
                };
            }

            return new ResultadoValidacaoArquivo
            {
                Valido = true
            };
        }

        private static readonly List<RegraArquivo> Regras =
        [
            new RegraArquivo
            {
                ContentTypes =
                [
                    "image/jpeg",
                    "image/png"
                ],
                TamanhoMaximoBytes = 5 * 1024 * 1024
            },
            new RegraArquivo
            {
                ContentTypes =
                [
                    "audio/amr",
                    "audio/mpeg",
                    "audio/mp4",
                    "audio/ogg",
                    "audio/aac",
                ],
                TamanhoMaximoBytes = 16 * 1024 * 1024
            },
            new RegraArquivo
            {
                ContentTypes =
                [
                    "video/mp4",
                    "video/3gpp"
                ],
                TamanhoMaximoBytes = 16 * 1024 * 1024
            },
            new RegraArquivo
            {
                ContentTypes =
                [
                    "text/plain",
                    "application/pdf",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "application/vnd.ms-powerpoint",
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                ],
                TamanhoMaximoBytes = 100 * 1024 * 1024
            }
        ];

        public async Task<string?> GetTranscricaoByMensagemIdAsync(int mensagemId)
        {
            var midia = await GetMidiaByMensagemIdAsync(mensagemId);
            return string.IsNullOrWhiteSpace(midia?.Transcricao) ? null : midia.Transcricao;
        }
    }
}
