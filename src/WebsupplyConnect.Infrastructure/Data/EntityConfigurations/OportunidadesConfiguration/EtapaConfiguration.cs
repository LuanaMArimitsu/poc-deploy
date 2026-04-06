using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.EntityConfiguration.OportunidadesConfiguration
{
    public class EtapaConfiguration : EntidadeBaseConfiguration<Etapa>
    {
        public override void Configure(EntityTypeBuilder<Etapa> builder)
        {
            base.Configure(builder);

            builder.ToTable("Etapas");

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Descricao)
                .HasMaxLength(500);

            builder.Property(e => e.Ordem)
                .IsRequired();

            builder.Property(e => e.Cor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(e => e.ProbabilidadePadrao)
                .IsRequired();

            builder.Property(e => e.EhAtiva)
                .IsRequired();

            builder.Property(e => e.EhFinal)
                .IsRequired();

            builder.Property(e => e.EhVitoria)
                .IsRequired();

            builder.Property(e => e.EhPerdida)
                .IsRequired();

            builder.Property(e => e.FunilId)
                .IsRequired();

            builder.Property(f => f.Ativo)
                .IsRequired();
            // Relationships
            builder.HasOne(e => e.Funil)
                .WithMany(f => f.Etapas)
                .HasForeignKey(e => e.FunilId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}