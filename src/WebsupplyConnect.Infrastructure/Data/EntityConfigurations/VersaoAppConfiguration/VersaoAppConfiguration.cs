using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.VersaoApp;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.VersaoAppConfiguration
{
    public class VersaoAppConfiguration : IEntityTypeConfiguration<VersaoApp>
    {
        public void Configure (EntityTypeBuilder<VersaoApp> builder)
        {
            builder.ToTable(nameof(VersaoApp));

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Versao)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(v => v.PlataformaApp)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(v => v.AtualizacaoObrigatoria)
                .IsRequired();

            builder.Property(v => v.DataCriacao)
                .IsRequired();

            builder.Property(v => v.DataModificacao)
                .IsRequired();
        }
    }
}
