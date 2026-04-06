using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade MetricaVendedor
    /// </summary>
    public class MetricaVendedorConfiguration : EntidadeBaseConfiguration<MetricaVendedor>
    {
        public override void Configure(EntityTypeBuilder<MetricaVendedor> builder)
        {
            // Configuração base da entidade
            base.Configure(builder);

            // Configuração de tabela
            builder.ToTable("MetricaVendedor");

            // Propriedades
            builder.Property(m => m.UsuarioId)
                .IsRequired();

            builder.Property(m => m.EmpresaId)
                .IsRequired();

            builder.Property(m => m.DataInicioMedicao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(m => m.DataUltimaAtualizacao)
                .IsRequired()
                .HasColumnType("datetime2");

            // Propriedades decimais - configuração explícita para evitar truncamento
            builder.Property(m => m.TaxaConversao)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(m => m.VelocidadeMediaAtendimento)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(m => m.TaxaPerdaInatividade)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);
                
            // CORREÇÃO: Adicionando a configuração explícita para ScoreGeral
            builder.Property(m => m.ScoreGeral)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            // Propriedades inteiras
            builder.Property(m => m.TotalLeadsRecebidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.TotalLeadsConvertidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.TotalLeadsPerdidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.LeadsAtivosAtual)
                .IsRequired()
                .HasDefaultValue(0);
                
            // Configuração para o campo JSON de métricas detalhadas
            builder.Property(m => m.MetricasDetalhadas)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Relacionamentos
            builder.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices
            builder.HasIndex(m => new { m.UsuarioId, m.EmpresaId })
                .HasDatabaseName("IX_MetricaVendedor_Usuario_Empresa")
                .IsUnique();

            builder.HasIndex(m => m.EmpresaId)
                .HasDatabaseName("IX_MetricaVendedor_Empresa");

            builder.HasIndex(m => m.TaxaConversao)
                .HasDatabaseName("IX_MetricaVendedor_TaxaConversao");
                
            // ADICIONAL: Índice para ScoreGeral para melhorar performance de queries de ranking
            builder.HasIndex(m => m.ScoreGeral)
                .HasDatabaseName("IX_MetricaVendedor_ScoreGeral");
        }
    }
}