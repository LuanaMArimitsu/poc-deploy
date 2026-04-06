namespace WebsupplyConnect.Domain.Entities.Oportunidade
{
    public class TipoInteresse
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;

        public virtual ICollection<Oportunidade> Oportunidades { get; set; } = new List<Oportunidade>();
    }
}