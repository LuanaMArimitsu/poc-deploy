using Microsoft.AspNetCore.Authorization;

namespace WebsupplyConnect.Infrastructure.Authorization.Requirement
{
    /// Requirements herdam da interface IAuthorizationRequirement, são classes "marcadoras", mais como DTOs.
    /// Requirements são dados, não comportamentos.
    public class HorarioDeTrabalhoRequirement : IAuthorizationRequirement
    {
        // Classe vazia - só serve como "marcador" para o sistema de autorização
    }
}
