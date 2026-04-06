namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class AzureUserDTO
    {
        public string Id { get; set; }                 
        public string DisplayName { get; set; }        
        public string Email { get; set; }              
        public string Upn {  get; set; }
        public string Cargo { get; set; }              
        public string Departamento { get; set; }
        public bool Cadastrado { get; set; } = false;
        public int? IdUsuario { get; set; }
        public bool? Ativo { get; set; }
        public int CanalPadraoId { get; set; }
        public int? UsuarioSuperiorId { get; set; }
    }

    public class AzureAddUserRequest
    {
        public string AzureUserId { get; set; }
        public int? UsuarioSuperiorId { get; set; }
        public int EmpresaId { get; set; }
        public int CanalPadraoId { get; set; }
        public int EquipePadraoId { get; set; }
        public string Cargo { get; set; }
        public string Departamento { get; set; }
    }
}
