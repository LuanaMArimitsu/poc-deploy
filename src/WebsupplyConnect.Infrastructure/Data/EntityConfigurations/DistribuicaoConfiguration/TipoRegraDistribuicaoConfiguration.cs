using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade TipoRegraDistribuicao
    /// </summary>
    public class TipoRegraDistribuicaoConfiguration : EntidadeTipificacaoConfiguration<TipoRegraDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<TipoRegraDistribuicao> builder)
        {
            // Configuração base da entidade tipificação
            base.Configure(builder);

            // IMPORTANTE: Remover a chamada ToTable para usar a tabela da entidade base
            // builder.ToTable("TipoRegraDistribuicao"); - Esta linha deve ser removida

            // Propriedades adicionais
            builder.Property(t => t.Categoria)
                .HasMaxLength(50);

            // Índices
            builder.HasIndex(t => t.Categoria)
                .HasDatabaseName("IX_TipoRegraDistribuicao_Categoria");
        }
    }
}