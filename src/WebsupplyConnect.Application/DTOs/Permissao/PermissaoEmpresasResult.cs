namespace WebsupplyConnect.Application.DTOs.Permissao
{
    public class PermissaoEmpresasResult
    {
        public bool AcessoGlobal { get; set; }
        public List<int>? EmpresasIds { get; set; }
    }
}
