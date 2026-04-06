using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class UsuarioFiltroRequestDTO
    {
        public int? EmpresaId { get; set; }
        public string? Nome { get; set; }
        public bool? Ativo { get; set; }

        public int? Pagina { get; set; }
        public int? TamanhoPagina { get; set; }
    }
}
