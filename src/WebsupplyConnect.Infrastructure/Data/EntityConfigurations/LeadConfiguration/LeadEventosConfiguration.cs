using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    public class LeadEventosConfiguration : EntidadeBaseConfiguration<LeadEvento>
    {
        public override void Configure(EntityTypeBuilder<LeadEvento> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);

            // Tabela
            builder.ToTable("LeadEventos");

            builder.Property(x => x.LeadId)
                .IsRequired();

            builder.Property(x => x.OrigemId)
                .IsRequired();

            builder.Property(x => x.DataEvento)
                .IsRequired();

            builder.Property(x => x.CanalId)
                .IsRequired(false);

            builder.Property(x => x.CampanhaId)
                .IsRequired(false);

            builder.Property(x => x.Observacao)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.HasOne(x => x.Lead)
                .WithMany(x => x.LeadEventos)
                .HasForeignKey(x => x.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Origem)
                .WithMany(x => x.LeadEventos)
                .HasForeignKey(x => x.OrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Canal)
                .WithMany(x => x.LeadEventos)
                .HasForeignKey(x => x.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Campanha)
                .WithMany(x => x.LeadEventos)
                .HasForeignKey(x => x.CampanhaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
