namespace WebsupplyConnect.Application.DTOs.Permissao.Permissao
{
    public class CreatePermissaoDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;
        public string Recurso { get; set; } = string.Empty;
        public bool IsCritica { get; set; } = false;
    }
}
