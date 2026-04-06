using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.ControleIntegracoes;
using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Domain.Interfaces.ControleSistemasExternos;

namespace WebsupplyConnect.Application.Services.ControleSistemasExternos
{
    public class SistemaExternoReaderService(ISistemaExternoRepository sistemaExternoRepository) : ISistemaExternoReaderService
    {
        private readonly ISistemaExternoRepository _sistemaExternoRepository = sistemaExternoRepository ?? throw new ArgumentNullException(nameof(sistemaExternoRepository));

        public async Task<SistemaExternoIntegradorDTO> GetSistemaExternoIntegradorData(string nome)
        {
            var sistema = await _sistemaExternoRepository.GetSistemaExterno(nome) ?? throw new AppException($"Sistema externo com nome {nome} não foi encontrado.");
            var extras = sistema.InformacoesExtras ?? "{}";
            var doc = JsonDocument.Parse(extras);

            return new SistemaExternoIntegradorDTO
            {
                Id = sistema.Id,
                URL_API = sistema.URL_API,
                Token = sistema.Token,
            };
        }

        public async Task<SistemaExternoIntegradorDTO> GetSistemaExternoOlxPorCredenciais(string nome, string cnpj)
        {

            var sistemaExterno = await _sistemaExternoRepository.GetSistemaExternoPorCredenciais(nome, cnpj)
                ?? throw new AppException($"Sistema externo '{nome}' não encontrado para o CNPJ {cnpj}.");

            return new SistemaExternoIntegradorDTO
            {
                Id = sistemaExterno.Id,
                URL_API = sistemaExterno.URL_API,
                Token = sistemaExterno.Token,
            };
        }
    }
}
