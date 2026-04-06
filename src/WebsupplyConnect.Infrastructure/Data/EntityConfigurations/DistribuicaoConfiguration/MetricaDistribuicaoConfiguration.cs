using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade MetricaDistribuicao
    /// </summary>
    public class MetricaDistribuicaoConfiguration : EntidadeBaseConfiguration<MetricaDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<MetricaDistribuicao> builder)
        {
            // Configuração base da entidade
            base.Configure(builder);

            // Configuração de tabela
            builder.ToTable("MetricaDistribuicao");

            // Propriedades
            builder.Property(m => m.DataReferencia)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(m => m.TotalLeadsRecebidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.TotalLeadsDistribuidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.TotalReatribuicoes)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.TempoMedioDistribuicao)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(m => m.TaxaSucessoDistribuicao)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(m => m.DistribuicaoPorVendedor)
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.DistribuicaoPorRegra)
                .HasColumnType("nvarchar(max)");

            // Relacionamentos
            builder.HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices
            builder.HasIndex(m => new { m.EmpresaId, m.DataReferencia })
                .HasDatabaseName("IX_MetricaDistribuicao_Empresa_Data")
                .IsUnique();
        }
    }
}