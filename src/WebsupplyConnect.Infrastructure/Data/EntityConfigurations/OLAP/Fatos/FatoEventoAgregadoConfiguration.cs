using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Fatos;

public class FatoEventoAgregadoConfiguration : EntidadeBaseConfiguration<FatoEventoAgregado>
{
    public override void Configure(EntityTypeBuilder<FatoEventoAgregado> builder)
    {
        base.Configure(builder);

        builder.ToTable("FatoEventoAgregado", "OLAP");

        builder.Property(f => f.LeadEventoId).IsRequired();
        builder.Property(f => f.LeadId).IsRequired();
        builder.Property(f => f.TempoId).IsRequired();
        builder.Property(f => f.EmpresaId).IsRequired();
        builder.Property(f => f.EquipeId).IsRequired(false);
        builder.Property(f => f.VendedorId).IsRequired(false);
        builder.Property(f => f.StatusAtualId).IsRequired(false);
        builder.Property(f => f.OrigemId).IsRequired();
        builder.Property(f => f.CampanhaId).IsRequired(false);

        builder.Property(f => f.ValorTotalOportunidadesGanhas).HasColumnType("decimal(18,2)");

        builder.Property(f => f.DataConversao).IsRequired(false).HasColumnType("datetime2");
        builder.Property(f => f.TempoMedioRespostaMinutos).IsRequired(false).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TempoMedioPrimeiroAtendimentoMinutos).IsRequired(false).HasColumnType("decimal(18,2)");
        builder.Property(f => f.ProdutoInteresse).IsRequired(false).HasMaxLength(200);

        builder.Property(f => f.DataUltimoEvento).IsRequired(false).HasColumnType("datetime2");
        builder.Property(f => f.DataReferencia).IsRequired().HasColumnType("datetime2");

        builder.HasIndex(f => new { f.LeadEventoId, f.DataReferencia })
            .IsUnique()
            .HasDatabaseName("UX_FatoEventoAgregado_LeadEventoId_DataReferencia");

        builder.HasIndex(f => f.DataReferencia)
            .HasDatabaseName("IX_FatoEventoAgregado_DataReferencia");

        builder.HasIndex(f => new { f.CampanhaId, f.DataReferencia })
            .HasFilter("[CampanhaId] IS NOT NULL")
            .HasDatabaseName("IX_FatoEventoAgregado_CampanhaId_DataReferencia");

        builder.HasIndex(f => f.DataUltimoEvento)
            .HasFilter("[DataUltimoEvento] IS NOT NULL")
            .HasDatabaseName("IX_FatoEventoAgregado_DataUltimoEvento");
    }
}
