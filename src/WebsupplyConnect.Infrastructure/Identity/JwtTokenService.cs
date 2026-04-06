using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Interfaces.Usuario;

namespace WebsupplyConnect.Infrastructure.Identity
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAzureAdService _azureAdService;
        private readonly IConfiguration _configuration;
        private readonly IDispositivosRepository _dispositivosRepository;
        private readonly IDispositivosWriterService _dispositivosWriterService;

        public JwtTokenService(IUsuarioRepository usuarioRepository, IAzureAdService azureAdService, IConfiguration configuration, IDispositivosRepository dispositivosRepository, IDispositivosWriterService dispositivosWriterService)
        {
            _usuarioRepository = usuarioRepository;
            _azureAdService = azureAdService;
            _configuration = configuration;
            _dispositivosRepository = dispositivosRepository;
            _dispositivosWriterService = dispositivosWriterService;
        }

        public async Task<GenerateJwtResponseDTO> GerarJwtAsync(GenerateJwtRequestDTO request)
        {
            var azureUser = await _azureAdService.GetUserByAccessTokenAsync(request.AccessToken);
            if (azureUser == null || string.IsNullOrEmpty(azureUser.Id))
                throw new UnauthorizedAccessException("Token inválido ou expirado.");

            var usuario = await _usuarioRepository.BuscarUsuarioPorObjectIdAsync(azureUser.Id);
            if (usuario == null)
                throw new UnauthorizedAccessException("Usuário não encontrado no sistema.");

            if (usuario.UsuarioEmpresas == null || !usuario.UsuarioEmpresas.Any())
                throw new UnauthorizedAccessException("Usuário sem empresa vinculada.");

            var empresaPrincipal = usuario.UsuarioEmpresas.FirstOrDefault(ue => ue.IsPrincipal) ?? usuario.UsuarioEmpresas.First();

            string? deviceId = null;
            int dispositivoID = 0;

            if (request.DeviceInfo is not null && !string.IsNullOrWhiteSpace(request.DeviceInfo.DeviceId) && 
                request.DeviceInfo.DeviceId != "string" && request.DeviceInfo.Modelo != "string")
            {
                try
                {
                    var dispositivoExistente = await _dispositivosRepository.ObterPorDeviceIdAsync(usuario.Id, request.DeviceInfo.DeviceId);

                    if (dispositivoExistente == null)
                    {
                        var adicionarDispositivoDto = new AdicionarDispositivoDTO
                        {
                            UsuarioId = usuario.Id,
                            DeviceId = request.DeviceInfo.DeviceId,
                            Modelo = request.DeviceInfo.Modelo
                        };

                        await _dispositivosWriterService.Create(adicionarDispositivoDto);

                        dispositivoExistente = await _dispositivosRepository.ObterPorDeviceIdAsync(usuario.Id, request.DeviceInfo.DeviceId);
                    }

                    deviceId = request.DeviceInfo.DeviceId;
                    dispositivoID = dispositivoExistente.Id;
                }
                catch
                {
                    // falha silenciosa
                }
            }

            var expirationDays = int.TryParse(_configuration["Jwt:AccessTokenExpirationDays"], out var days) ? days : 30;
            var expiration = DateTime.UtcNow.AddDays(expirationDays);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim("email", usuario.Email),
                new Claim("objectId", usuario.ObjectId),
                new Claim("EmpresaId", empresaPrincipal.EmpresaId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new GenerateJwtResponseDTO
            {
                JwtToken = jwt,
                RefreshToken = Guid.NewGuid().ToString(),
                ExpirationDate = expiration,
                UserInfo = new AzureUserDTO
                {
                    IdUsuario = usuario.Id,
                    DisplayName = usuario.Nome,
                    Email = usuario.Email,
                    Upn = usuario.Upn,
                    Cargo = usuario.Cargo ?? azureUser.Cargo,
                    Departamento = usuario.Departamento ?? azureUser.Departamento,
                    Id = usuario.ObjectId,
                    Cadastrado = true,
                    Ativo = usuario.Ativo
                },
                DeviceId = dispositivoID.ToString()
            };
        }

        public async Task<GenerateJwtResponseDTO> RenovarJwtAsync(string refreshToken, ClaimsPrincipal userClaims, string clientType)
        {
            if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length < 10)
                throw new UnauthorizedAccessException("Refresh token inválido.");

            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Usuário inválido.");

            var usuario = await _usuarioRepository.GetEmpresaByUsuarioId(int.Parse(userIdClaim.Value));
            if (usuario == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            if (usuario.UsuarioEmpresas == null || !usuario.UsuarioEmpresas.Any())
                throw new UnauthorizedAccessException("Usuário sem empresa vinculada.");

            // Lógica para gerar novo token
            var expirationDays = int.TryParse(_configuration["Jwt:RenewedTokenExpirationDays"], out var days) ? days : 2;
            var expiration = DateTime.UtcNow.AddDays(expirationDays);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim("email", usuario.Email),
                new Claim("objectId", usuario.ObjectId),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new GenerateJwtResponseDTO
            {
                JwtToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = Guid.NewGuid().ToString(),
                ExpirationDate = expiration,
                UserInfo = new AzureUserDTO
                {
                    IdUsuario = usuario.Id,
                    DisplayName = usuario.Nome,
                    Email = usuario.Email,
                    Upn = usuario.Upn,
                    Cargo = usuario.Cargo,
                    Departamento = usuario.Departamento,
                    Id = usuario.ObjectId,
                    Cadastrado = true,
                    Ativo = usuario.Ativo
                }
            };
        }
    }
}
