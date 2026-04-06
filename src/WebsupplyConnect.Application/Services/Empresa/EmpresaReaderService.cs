using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.Empresa;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Domain.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Empresa;

namespace WebsupplyConnect.Application.Services.Empresa
{
    public class EmpresaReaderService(
        ILogger<EmpresaReaderService> logger,
        IEmpresaRepository empresaRepository,
        ICanalRepository canalRepository,
        IEquipeReaderService equipeReaderService
        ) : IEmpresaReaderService
    {
        private readonly ILogger<EmpresaReaderService> _logger = logger;
        private readonly IEmpresaRepository _empresaRepository = empresaRepository;
        private readonly ICanalRepository _canalRepository = canalRepository;
        private readonly IEquipeReaderService _equipeReaderService = equipeReaderService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<Domain.Entities.Empresa.Empresa> ObterPorId(int empresaId)
        {
            var empresa = await _empresaRepository.GetByIdAsync<Domain.Entities.Empresa.Empresa>(empresaId);
            return empresa ?? throw new Exception("Empresa não encontrada.");
        }

        public async Task<Domain.Entities.Empresa.Empresa?> ObterPorCnpjAsync(string cnpj)
        {
            return await _empresaRepository.GetByPredicateAsync<Domain.Entities.Empresa.Empresa>(e => e.Cnpj == cnpj && e.Ativo && !e.Excluido);
        }

        public async Task<EmpresaComCanaisResponseDTO?> ObterEmpresaComCanaisAsync(int empresaId)
        {
            var empresa = await _empresaRepository
                .Query()
                .Include(e => e.GrupoEmpresa)
                .FirstOrDefaultAsync(e => e.Id == empresaId && e.Ativo && !e.Excluido);

            if (empresa == null)
                return null;

            var canais = await _canalRepository.ListCanaisAsync(empresaId, ativo: true);

            return new EmpresaComCanaisResponseDTO
            {
                EmpresaId = empresa.Id,
                EmpresaNome = empresa.Nome,
                EmpresaCnpj = empresa.Cnpj,
                GrupoEmpresaNome = empresa.GrupoEmpresa.Nome,
                PossuiIntegracaoNBS = empresa.PossuiIntegracaoNBS,
                Canais = canais.Select(c => new CanalItemDTO
                {
                    CanalId = c.Id,
                    CanalNome = c.Nome
                }).ToList()
            };
        }

        public async Task<List<EmpresaComCanaisResponseDTO>> ObterEmpresasComCanaisAsync()
        {
            var empresas = await _empresaRepository
                .Query()
                .Include(e => e.GrupoEmpresa)
                .Where(e => e.Ativo && !e.Excluido)
                .ToListAsync();

            var empresaIds = empresas.Select(e => e.Id).ToList();
            var canaisPorEmpresa = await _canalRepository.ListarCanaisPorEmpresasAsync(empresaIds);

            var resultado = empresas.Select(empresa =>
            {
                var canais = canaisPorEmpresa
                    .Where(c => c.EmpresaId == empresa.Id)
                    .Select(c => new CanalItemDTO
                    {
                        CanalId = c.Id,
                        CanalNome = c.Nome
                    }).ToList();

                return new EmpresaComCanaisResponseDTO
                {
                    EmpresaId = empresa.Id,
                    EmpresaNome = empresa.Nome,
                    EmpresaCnpj = empresa.Cnpj,
                    PossuiIntegracaoNBS = empresa.PossuiIntegracaoNBS,
                    GrupoEmpresaNome = empresa.GrupoEmpresa.Nome,
                    Canais = canais
                };
            }).ToList();

            return resultado;
        }

        /// <summary>
        /// Método para verificar se empresa existe a partir do ID.
        /// </summary>
        public async Task<bool> EmpresaExistsAsync(int id)
        {
            return await _empresaRepository.ExistsInDatabaseAsync<Domain.Entities.Empresa.Empresa>(id);
        }

        public async Task<string?> GetConfiguracaoIntegracao(int empresaId)
        {
            var empresa = await _empresaRepository.GetByIdAsync<Domain.Entities.Empresa.Empresa>(empresaId);
            if (empresa == null)
            {
                return null;
            }             
            return empresa.ConfiguracaoIntegracao;
        }

        public async Task<List<EmpresaListagemDTO>> ObterTodasEmpresasAsync()
        {
            var empresas = await _empresaRepository.ListarEmpresasAtivasAsync();

            return empresas.Select(e => new EmpresaListagemDTO
            {
                Id = e.Id,
                Nome = e.Nome
            }).ToList();
        }

        public async Task<bool> ExistemEmpresasAtivasAsync(List<int> empresaIds)
        {
            return await _empresaRepository.ExistemEmpresasAtivasAsync(empresaIds);
        }

        public async Task<GrupoEmpresaDTO> GetGrupoEmpresaByEmpresaId(int empresaId)
        {
            var empresa = await _empresaRepository.GetGrupoEmpresaByEmpresaId(empresaId);

            return new GrupoEmpresaDTO
            {
                Id = empresa.GrupoEmpresa.Id,
                Nome = empresa.GrupoEmpresa.Nome,
                Ativo = empresa.GrupoEmpresa.Ativo,
                CnpjHolding = empresa.GrupoEmpresa.CnpjHolding
            };
        }

        public async Task<List<BranchesDTO>> GetFiliasAsync(int grupoEmpresaId)
        {
            var empresas = await _empresaRepository.GetFiliais(grupoEmpresaId);
            
            List<BranchesDTO> filiais = [];
            foreach (var empresa in empresas)
            {
                Localizacao? localizacao = null;
                if (!string.IsNullOrWhiteSpace(empresa.ConfiguracaoIntegracao))
                {
                    var config = JsonSerializer.Deserialize<EmpresaConfigIntegracaoDTO>(
                    empresa.ConfiguracaoIntegracao, _jsonOptions);

                    localizacao = config?.Localizacao;
                }
                var equipes = await _equipeReaderService.GetEquipesByEmpresaId(empresa.Id);
                BranchesDTO branch = new ()
                {
                    BranchCode = empresa.Id,
                    BranchName = empresa.Nome,
                    Location = localizacao,
                    Teams = equipes.Select(e => new TeamsDTO
                    {
                        TeamId = e.Id,
                        TeamName = e.Nome
                    }).ToList()
                };
                filiais.Add(branch);
            }
            _logger.LogInformation("Filiais montadas:\n{Filiais}",
      JsonSerializer.Serialize(filiais, new JsonSerializerOptions { WriteIndented = true }));
            return filiais;
        }

        public async Task<Domain.Entities.Empresa.Empresa?> GetEmpresaPorCnpjAsync(string cnpjEmpresa)
        {
            var empresa = await _empresaRepository.GetEmpresaPorCnpjAsync(cnpjEmpresa);
            return empresa;
        }

        public async Task<List<Domain.Entities.Empresa.Empresa>> ObterTodasNaoExcluidasParaETLAsync()
        {
            return await _empresaRepository.GetListByPredicateAsync<Domain.Entities.Empresa.Empresa>(e => !e.Excluido, includeDeleted: true);
        }
    }
}
