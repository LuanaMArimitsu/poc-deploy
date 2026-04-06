using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class DispositivoFiltroRequestDTO
    {
        public string? Busca { get; set; }
        public int? UsuarioId { get; set; }
        public string Status { get; set; } = "Todos";
        public DateTime? SincronizadoApos { get; set; }
        public DateTime? SincronizadoAntes { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
        public string OrdenarPor { get; set; } = "UltimaSincronizacao";
        public string DirecaoOrdenacao { get; set; } = "DESC";
    }
}
