namespace WebsupplyConnect.Application.DTOs.Permissao.Permissao
{
    public class UpdatePermissaoDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool IsCritica { get; set; }
    }
}
