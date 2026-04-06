using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    public class HistoricoDistribuicaoConfiguration : EntidadeBaseConfiguration<HistoricoDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<HistoricoDistribuicao> builder)
        {
            base.Configure(builder);
            
            // Configuração da tabela
            builder.ToTable("HistoricoDistribuicao");
            
            // Propriedades
            builder.Property(h => h.DataExecucao)
                .IsRequired()
                .HasColumnType("datetime2");
            
            builder.Property(h => h.TotalLeadsDistribuidos)
                .IsRequired();
                
            builder.Property(h => h.TotalVendedoresAtivos)
                .IsRequired();
                
            builder.Property(h => h.ResultadoDistribuicao)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
                
            builder.Property(h => h.ErrosOcorridos)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
                
            builder.Property(h => h.TempoExecucaoSegundos)
                .IsRequired();
            
            // IMPORTANTE: Configuração explícita da relação com ConfiguracaoDistribuicao
            builder.HasOne(h => h.ConfiguracaoDistribuicao)
                .WithMany(c => c.Historicos) // Usar a coleção existente na classe ConfiguracaoDistribuicao
                .HasForeignKey(h => h.ConfiguracaoDistribuicaoId)
                .IsRequired(true) // Este campo não é nullable na entidade
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(h => h.UsuarioExecutou)
                .WithMany()
                .HasForeignKey(h => h.UsuarioExecutouId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices
            builder.HasIndex(h => h.DataExecucao);
            builder.HasIndex(h => h.ConfiguracaoDistribuicaoId);
        }
    }
}