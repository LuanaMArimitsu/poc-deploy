using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Infrastructure.ExternalServices.ConnectIntegrador
{
    public class ConnectIntegradorService : IConnectIntegradorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConnectIntegradorService> _logger;
        private readonly IEventoIntegracaoWriterService _eventoIntegracaoWriter;

        public ConnectIntegradorService(HttpClient httpClient, ILogger<ConnectIntegradorService> logger, IEventoIntegracaoWriterService eventoIntegracaoWriterService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _eventoIntegracaoWriter = eventoIntegracaoWriterService;
        }

        public async Task<GerarEventoResultDTO> ConnectIntegradorAsync(OportunidadeRequestDTO dto, string url, string token, int sistemaId)
        {
            string payloadEnviado = JsonSerializer.Serialize(dto);
            string payloadRecebido = null;
            int statusCode = 0;
            EventoIntegracao eventoIntegracao = null;
            GerarEventoResultDTO? retorno = null;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("x-api-key", token);
                request.Content = new StringContent(payloadEnviado, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                statusCode = (int)response.StatusCode;

                payloadRecebido = await response.Content.ReadAsStringAsync();
                retorno = JsonSerializer.Deserialize<GerarEventoResultDTO>(payloadRecebido);

                if (retorno == null)
                {
                    retorno.Mensagem = "Não foi possível interpretar o JSON retornado pelo Connect Integrador.";
                }

                eventoIntegracao = new EventoIntegracao(
                    sistemaExternoId: sistemaId,
                    direcao: DirecaoIntegracao.Enviado,
                    tipoEvento: TipoEventoIntegracao.LEAD_CREATED,
                    sucesso: response.IsSuccessStatusCode,
                    payloadEnviado: payloadEnviado,
                    payloadRecebido: payloadRecebido,
                    codigoResposta: statusCode.ToString(),
                    mensagemErro: retorno.Mensagem,
                    tipoEntidadeOrigem: TipoEntidadeIntegracao.Oportunidade,
                    entidadeOrigemId: dto.OportunidadeId
                );

                await _eventoIntegracaoWriter.RegistrarAsync(eventoIntegracao);

                return new GerarEventoResultDTO
                {
                    Sucesso = response.IsSuccessStatusCode,
                    CodEvento = retorno.CodEvento,
                    Mensagem = retorno.Mensagem
                };
            }
            catch (Exception ex)
            {
                eventoIntegracao = new EventoIntegracao(
                sistemaExternoId: sistemaId,
                direcao: DirecaoIntegracao.Enviado,
                tipoEvento: TipoEventoIntegracao.LEAD_CREATED,
                sucesso: false,
                payloadEnviado: payloadEnviado,
                payloadRecebido: payloadRecebido,
                codigoResposta: statusCode.ToString(),
                mensagemErro: ex.Message,
                tipoEntidadeOrigem: TipoEntidadeIntegracao.Oportunidade,
                entidadeOrigemId: dto.OportunidadeId
                );

                await _eventoIntegracaoWriter.RegistrarAsync(eventoIntegracao);

                _logger.LogError(ex, "Erro ao comunicar com Connect Integrador para oportunidade {OportunidadeId}", dto.OportunidadeId);

                return new GerarEventoResultDTO
                {
                    Sucesso = false,
                    CodEvento = null,
                    Mensagem = ex.Message
                };
            }
        }
    }
}
