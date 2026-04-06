using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Infrastructure.Authorization.Requirement;

namespace WebsupplyConnect.Infrastructure.Authorization.Handlers
{
    public class HorarioDeTrabalhoHandler : AuthorizationHandler<HorarioDeTrabalhoRequirement>
    {
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IUsuarioWriterService _usuarioWriterService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<HorarioDeTrabalhoHandler> _logger;

        public HorarioDeTrabalhoHandler(
            IUsuarioReaderService usuarioReaderService,
            IUsuarioWriterService usuarioWriterService,
            IHttpContextAccessor httpContextAccessor,
            IRedisCacheService redisCacheService,
            ILogger<HorarioDeTrabalhoHandler> logger)
        {
            _usuarioReaderService = usuarioReaderService;
            _usuarioWriterService = usuarioWriterService;
            _httpContextAccessor = httpContextAccessor;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HorarioDeTrabalhoRequirement requirement)
        {
            var userId = ObterUserIdDoContext(context);

            if (userId == 0)
            {
                userId = ObterUserIdDoHttpContext();
                if (userId == 0)
                {
                    DefinirMensagemErro("Usuário não identificado", false);
                    context.Fail();
                    return;
                }
            }

            var agora = TimeHelper.GetBrasiliaTime();
            var diaSemana = agora.ToString("dddd", new CultureInfo("pt-BR"));

            var cacheKey = $"usuario:{userId}:horarios";
            var horarios = await _redisCacheService.GetAsync<List<UsuarioHorarioDTO>>(cacheKey);

            if (horarios == null)
            {
                horarios = await _usuarioReaderService.ObterHorariosUsuarioAsync(userId);
                if (horarios != null)
                {
                    await _redisCacheService.SetAsync(cacheKey, horarios, TimeSpan.FromDays(1));
                }
            }

            //usuário sem expediente = admin
            if (horarios == null || !horarios.Any(h => h.SemExpediente != true)) 
            {
                context.Succeed(requirement);
                return;
            }

            var horarioHoje = horarios?.FirstOrDefault(h =>
                string.Equals(h.DiaSemanaDescricao, diaSemana, StringComparison.OrdinalIgnoreCase));

            if (horarioHoje == null || horarioHoje.SemExpediente == true)
            {
                DefinirMensagemErro("Usuário sem expediente para o dia de hoje", false);
                context.Fail();
                return;
            }

            var agoraT = agora.TimeOfDay;
            var inicio = horarioHoje.HorarioInicio;
            var fim = horarioHoje.HorarioFim;
            var fimComTolerancia = fim.HasValue ? fim.Value.Add(TimeSpan.FromMinutes(5)) : (TimeSpan?)null;

            if (agoraT >= inicio && agoraT <= fim)
            {
                context.Succeed(requirement);
                return;
            }

            bool passouDoFim = agoraT > fim;
            bool passouDaTolerancia = agoraT > fimComTolerancia;

            if (passouDaTolerancia)
            {
                DefinirMensagemErro("Fim da tolerância do usuário", false, fimComTolerancia);
                context.Fail();
                return;
            }

            if (passouDoFim && !horarioHoje.IsTolerancia)
            {
                DefinirMensagemErro("Fim do expediente do usuário", true, fimComTolerancia);
                context.Fail();
                return;
            }

            if (passouDaTolerancia && horarioHoje.IsTolerancia)
            {
                DefinirMensagemErro("Fim da tolerância do usuário", false, fimComTolerancia);
                context.Fail();
                return;
            }


            if (horarioHoje.IsTolerancia && passouDoFim && !passouDaTolerancia)
            {
                context.Succeed(requirement);
                return;
            }

            DefinirMensagemErro("Usuário não autorizado por horário de trabalho", false);
            context.Fail();
        }

        /// <summary>
        /// Define mensagem e flag showModal para ser consumida pelo OnForbidden no Program.cs
        /// </summary>
        private void DefinirMensagemErro(string mensagem, bool showModal, TimeSpan? limite = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["AuthErrorMessage"] = mensagem;
                httpContext.Items["ShowModal"] = showModal;

                if (limite.HasValue)
                {
                    httpContext.Items["Limite"] = DateTime.Today.Add(limite.Value).ToString("HH:mm");
                }
            }
        }

        /// <summary>
        /// MÉTODO 1: Obter User ID do AuthorizationHandlerContext (RECOMENDADO)
        /// </summary>
        private int ObterUserIdDoContext(AuthorizationHandlerContext context)
        {
            // Tentar diferentes claims do JWT
            var claims = new[]
            {
                ClaimTypes.NameIdentifier,  // "sub" padrão
                "user_id",                  // Custom claim
                "userId",                   // Custom claim  
                "id",                       // Custom claim
                "userid"                    // Seu exemplo
            };

            foreach (var claimType in claims)
            {
                var claimValue = context.User.FindFirst(claimType)?.Value;
                if (!string.IsNullOrEmpty(claimValue) && int.TryParse(claimValue, out int userId))
                {
                    return userId;
                }
            }

            return 0;
        }

        /// <summary>
        /// MÉTODO 2: Obter User ID do HttpContextAccessor
        /// </summary>
        private int ObterUserIdDoHttpContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return 0;

            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claimValue) && int.TryParse(claimValue, out var uid))
                {
                    return uid;
                }
            }

            return 0;
        }

    }
}
