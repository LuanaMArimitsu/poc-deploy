namespace WebsupplyConnect.Application.DTOs.Permissao.Permissao
{
    public class PermissaoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Modulo { get; set; }
        public string Categoria { get; set; }
        public bool IsCritica { get; set; }
        public bool Ativa { get; set; }
    }
}
