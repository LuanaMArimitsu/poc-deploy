using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OLAP.Fatos;

public class FatoOportunidadeMetricaConfiguration : EntidadeBaseConfiguration<FatoOportunidadeMetrica>
{
    public override void Configure(EntityTypeBuilder<FatoOportunidadeMetrica> builder)
    {
        base.Configure(builder);

        builder.ToTable("FatoOportunidadeMetrica", "OLAP");

        builder.Property(f => f.OportunidadeId).IsRequired();
        builder.Property(f => f.LeadId).IsRequired();
        builder.Property(f => f.LeadEventoId).IsRequired(false);
        builder.Property(f => f.TempoId).IsRequired();
        builder.Property(f => f.EmpresaId).IsRequired();
        builder.Property(f => f.EquipeId).IsRequired(false);
        builder.Property(f => f.VendedorId).IsRequired(false);
        builder.Property(f => f.StatusLeadId).IsRequired(false);
        builder.Property(f => f.OrigemId).IsRequired();
        builder.Property(f => f.CampanhaId).IsRequired(false);
        builder.Property(f => f.DimensaoEtapaFunilId).IsRequired(false);

        builder.Property(f => f.ValorEstimado).HasColumnType("decimal(18,2)");
        builder.Property(f => f.ValorFinal).HasColumnType("decimal(18,2)");
        builder.Property(f => f.ValorEsperadoPipeline).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TempoMedioRespostaMinutos).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TempoMedioPrimeiroAtendimentoMinutos).HasColumnType("decimal(18,2)");
        builder.Property(f => f.TaxaConversaoEtapa).HasColumnType("decimal(5,2)");
        builder.Property(f => f.WinRateEtapa).HasColumnType("decimal(5,2)");

        builder.Property(f => f.DataUltimoEvento).IsRequired(false).HasColumnType("datetime2");
        builder.Property(f => f.DataReferencia).IsRequired().HasColumnType("datetime2");
        builder.Property(f => f.DataFechamento).HasColumnType("datetime2");

        builder.HasIndex(f => new { f.OportunidadeId, f.DataReferencia })
            .IsUnique()
            .HasDatabaseName("UX_FatoOportunidadeMetrica_OportunidadeId_DataReferencia");

        builder.HasIndex(f => f.DataReferencia)
            .HasDatabaseName("IX_FatoOportunidadeMetrica_DataReferencia");

        builder.HasIndex(f => new { f.EmpresaId, f.DataReferencia })
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EmpresaId_DataReferencia");

        builder.HasIndex(f => new { f.EmpresaId, f.DimensaoEtapaFunilId, f.DataReferencia })
            .HasFilter("[DimensaoEtapaFunilId] IS NOT NULL")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EmpresaId_Etapa_DataRef");

        builder.HasIndex(f => new { f.VendedorId, f.DataReferencia })
            .HasDatabaseName("IX_FatoOportunidadeMetrica_VendedorId_DataReferencia");

        builder.HasIndex(f => new { f.EquipeId, f.DataReferencia })
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EquipeId_DataReferencia");

        builder.HasIndex(f => new { f.CampanhaId, f.DataReferencia })
            .HasFilter("[CampanhaId] IS NOT NULL")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_CampanhaId_DataReferencia");

        builder.HasIndex(f => f.LeadEventoId)
            .HasFilter("[LeadEventoId] IS NOT NULL")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_LeadEventoId");

        builder.HasIndex(f => f.EhGanha)
            .HasFilter("[EhGanha] = 1")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EhGanha");

        builder.HasIndex(f => f.EhPerdida)
            .HasFilter("[EhPerdida] = 1")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EhPerdida");

        builder.HasIndex(f => f.EhEstagnada)
            .HasFilter("[EhEstagnada] = 1")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_EhEstagnada");

        builder.HasOne(f => f.Tempo)
            .WithMany()
            .HasForeignKey(f => f.TempoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Empresa)
            .WithMany()
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Equipe)
            .WithMany()
            .HasForeignKey(f => f.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Vendedor)
            .WithMany()
            .HasForeignKey(f => f.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.StatusLead)
            .WithMany()
            .HasForeignKey(f => f.StatusLeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Origem)
            .WithMany()
            .HasForeignKey(f => f.OrigemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Campanha)
            .WithMany()
            .HasForeignKey(f => f.CampanhaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.EtapaFunil)
            .WithMany()
            .HasForeignKey(f => f.DimensaoEtapaFunilId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.DataUltimoEvento)
            .HasFilter("[DataUltimoEvento] IS NOT NULL")
            .HasDatabaseName("IX_FatoOportunidadeMetrica_DataUltimoEvento");
    }
}
