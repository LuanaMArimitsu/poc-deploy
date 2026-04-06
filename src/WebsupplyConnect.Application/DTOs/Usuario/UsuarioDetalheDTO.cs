using WebsupplyConnect.Application.DTOs.Empresa;

namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioDetalheDTO
    {
        // Informações Básicas
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public bool Ativo { get; set; }
        public bool Cadastrado { get; set; }
        public DateTime? UltimoAcesso { get; set; }

        // Avatar
        public string InicialAvatar { get; set; }
        public string CorAvatar { get; set; }

        // Hierarquia
        public int? UsuarioSuperiorId { get; set; }
        public string UsuarioSuperiorNome { get; set; }

        // Azure AD / Identity
        public string ObjectId { get; set; }
        public string Upn { get; set; }
        public string DisplayName { get; set; }
        public bool IsExternal { get; set; }

        // Empresas
        public EmpresaDetalheDTO EmpresaPrincipal { get; set; }
        public List<UsuarioEmpresaDTO> Empresas { get; set; } = new();

        // Dispositivos
        public List<DispositivoDTO> Dispositivos { get; set; } = new();

        // Horários de Trabalho
        public List<UsuarioHorarioDTO> HorariosTrabalho { get; set; } = new();

        // Estatísticas
        public int TotalLeadsResponsavel { get; set; }
        public int TotalConversasAtivas { get; set; }

        // Auditoria
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }
}
