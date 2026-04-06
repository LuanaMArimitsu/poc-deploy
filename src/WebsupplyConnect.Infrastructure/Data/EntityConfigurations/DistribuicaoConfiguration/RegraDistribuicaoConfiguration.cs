using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    public class RegraDistribuicaoConfiguration : EntidadeBaseConfiguration<RegraDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<RegraDistribuicao> builder)
        {
            base.Configure(builder);
            
            // Configuração da tabela
            builder.ToTable("RegrasDistribuicao");
            
            // Propriedades
            builder.Property(r => r.ConfiguracaoDistribuicaoId).IsRequired();
            builder.Property(r => r.TipoRegraId).IsRequired();
            builder.Property(r => r.Nome)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(r => r.Descricao)
                .HasMaxLength(500);
            builder.Property(r => r.Ordem).IsRequired();
            builder.Property(r => r.Peso).IsRequired();
            builder.Property(r => r.Ativo).IsRequired();
            builder.Property(r => r.ParametrosJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            builder.Property(r => r.Obrigatoria).IsRequired();
            
            // Configurações de navegação
            builder.HasOne(r => r.ConfiguracaoDistribuicao)
                .WithMany(c => c.Regras)
                .HasForeignKey(r => r.ConfiguracaoDistribuicaoId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(r => r.TipoRegra)
                .WithMany()
                .HasForeignKey(r => r.TipoRegraId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // IMPORTANTE: Não configure a relação com AtribuicaoLead aqui
            // Essa relação já está configurada em AtribuicaoLeadConfiguration
        }
    }
}