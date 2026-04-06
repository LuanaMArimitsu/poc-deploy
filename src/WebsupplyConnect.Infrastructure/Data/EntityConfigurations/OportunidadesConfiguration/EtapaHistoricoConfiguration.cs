using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OportunidadesConfiguration
{
    public class EtapaHistoricoConfiguration : IEntityTypeConfiguration<EtapaHistorico>
    {
        public void Configure(EntityTypeBuilder<EtapaHistorico> builder)
        {
            builder.ToTable("EtapaHistorico");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.OportunidadeId)
                .IsRequired();

            builder.Property(e => e.EtapaAnteriorId);                

            builder.Property(e => e.EtapaNovaId)
                .IsRequired();

            builder.Property(e => e.DataMudanca)
                .IsRequired();

            builder.Property(e => e.ResponsavelId)
                .IsRequired();

            builder.Property(e => e.Observacao)
                .HasMaxLength(1000);

            builder.Property(e => e.DiasNaEtapaAnterior)
                .IsRequired();

            builder.HasOne(e => e.Oportunidade)
                .WithMany(o => o.HistoricoEtapas)
                .HasForeignKey(e => e.OportunidadeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EtapaAnterior)
                .WithMany(et => et.HistoricosComoEtapaAnterior)
                .HasForeignKey(e => e.EtapaAnteriorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EtapaNova)
                .WithMany(et => et.HistoricosComoEtapaNova)
                .HasForeignKey(e => e.EtapaNovaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Responsavel)
                .WithMany()
                .HasForeignKey(e => e.ResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}