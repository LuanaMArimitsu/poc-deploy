using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OportunidadesConfiguration
{
    public class TipoInteresseConfiguration : IEntityTypeConfiguration<TipoInteresse>
    {
        public void Configure(EntityTypeBuilder<TipoInteresse> builder)
        {
            builder.ToTable("TipoInteresse");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            builder.Property(t => t.Titulo)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}