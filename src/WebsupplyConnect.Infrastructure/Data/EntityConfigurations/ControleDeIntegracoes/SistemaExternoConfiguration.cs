using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ControleDeIntegracoes
{
    public class SistemaExternoConfiguration : EntidadeBaseConfiguration<SistemaExterno>
    {
        public override void Configure(EntityTypeBuilder<SistemaExterno> builder)
        {
            base.Configure(builder);

            builder.ToTable("SistemasExternos");

            builder.Property(s => s.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Tipo)
                .IsRequired();

            builder.Property(s => s.URL_API)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(s => s.Token)
                .HasMaxLength(500);

            builder.Property(s => s.InformacoesExtras)
                .HasMaxLength(500);

            builder.HasMany(s => s.EventosIntegracao)
                .WithOne(e => e.SistemaExterno)
                 .HasForeignKey(e => e.SistemaExternoId)
                 .OnDelete(DeleteBehavior.Restrict);
        }
    }
}