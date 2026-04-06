using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade StatusFilaDistribuicao
    /// </summary>
    public class StatusFilaDistribuicaoConfiguration : EntidadeTipificacaoConfiguration<StatusFilaDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<StatusFilaDistribuicao> builder)
        {
            // Configuração base da entidade tipificação
            base.Configure(builder);

            // Propriedades específicas
            builder.Property(s => s.PermiteRecebimento)
                .IsRequired()
                .HasDefaultValue(false);

            // Índices específicos
            builder.HasIndex(s => s.PermiteRecebimento)
                .HasDatabaseName("IX_StatusFilaDistribuicao_PermiteRecebimento");
                
            // Relacionamentos
            builder.HasMany(s => s.FilasDistribuicao)
                .WithOne(f => f.StatusFilaDistribuicao)
                .HasForeignKey(f => f.StatusFilaDistribuicaoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}