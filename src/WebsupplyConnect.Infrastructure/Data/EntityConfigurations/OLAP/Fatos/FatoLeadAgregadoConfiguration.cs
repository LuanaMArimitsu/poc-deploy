using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Fatos;

public class FatoLeadAgregadoConfiguration : EntidadeBaseConfiguration<FatoLeadAgregado>
{
    public override void Configure(EntityTypeBuilder<FatoLeadAgregado> builder)
    {
        base.Configure(builder);

        builder.ToTable("FatoLeadAgregado", "OLAP");

        builder.Property(f => f.LeadId).IsRequired();
        builder.Property(f => f.TempoId).IsRequired();
        builder.Property(f => f.EmpresaId).IsRequired();
        builder.Property(f => f.EquipeId).IsRequired(false);
        builder.Property(f => f.VendedorId).IsRequired(false);
        builder.Property(f => f.StatusAtualId).IsRequired(false);
        builder.Property(f => f.OrigemId).IsRequired();
        builder.Property(f => f.CampanhaId).IsRequired(false);

        builder.Property(f => f.ValorTotalOportunidadesGanhas).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TempoMedioRespostaMinutos).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TempoMedioPrimeiroAtendimentoMinutos).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TaxaConversaoLeadParaOportunidade).HasColumnType("decimal(5,2)");
        builder.Property(f => f.TaxaQualificacaoLead).HasColumnType("decimal(5,2)");

        builder.Property(f => f.AguardandoRespostaVendedor).IsRequired().HasDefaultValue(false);
        builder.Property(f => f.AguardandoRespostaAtendimento).IsRequired().HasDefaultValue(false);

        builder.Property(f => f.ProdutoInteresse).IsRequired(false).HasMaxLength(200);
        builder.Property(f => f.DataUltimoEvento).IsRequired(false).HasColumnType("datetime2");

        builder.Property(f => f.DataReferencia).IsRequired().HasColumnType("datetime2");
        builder.Property(f => f.DataConversao).HasColumnType("datetime2");

        builder.HasIndex(f => new { f.LeadId, f.DataReferencia })
            .IsUnique()
            .HasDatabaseName("UX_FatoLeadAgregado_LeadId_DataReferencia");

        builder.HasIndex(f => f.DataReferencia)
            .HasDatabaseName("IX_FatoLeadAgregado_DataReferencia");

        builder.HasIndex(f => new { f.EmpresaId, f.DataReferencia })
            .HasDatabaseName("IX_FatoLeadAgregado_EmpresaId_DataReferencia");

        builder.HasIndex(f => new { f.CampanhaId, f.DataReferencia })
            .HasFilter("[CampanhaId] IS NOT NULL")
            .HasDatabaseName("IX_FatoLeadAgregado_CampanhaId_DataReferencia");

        builder.HasIndex(f => f.DataUltimoEvento)
            .HasFilter("[DataUltimoEvento] IS NOT NULL")
            .HasDatabaseName("IX_FatoLeadAgregado_DataUltimoEvento");
    }
}
