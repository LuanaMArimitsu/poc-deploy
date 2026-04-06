using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class AlterarStatusDispositivoRequestDTO
    {
        public bool Ativo { get; set; }

        [StringLength(500, ErrorMessage = "Motivo deve ter no máximo 500 caracteres")]
        public string? Motivo { get; set; }

        public bool NotificarUsuario { get; set; } = true;
    }
}
