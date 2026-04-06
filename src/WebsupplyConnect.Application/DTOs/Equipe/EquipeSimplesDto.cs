namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public record EquipeSimplesDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<MembroSimplesDTO> Membros { get; set; }
    }
}
