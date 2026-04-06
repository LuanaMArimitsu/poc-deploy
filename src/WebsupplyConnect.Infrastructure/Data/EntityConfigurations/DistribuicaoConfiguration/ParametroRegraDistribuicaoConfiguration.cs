using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade ParametroRegraDistribuicao
    /// </summary>
    public class ParametroRegraDistribuicaoConfiguration : EntidadeBaseConfiguration<ParametroRegraDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<ParametroRegraDistribuicao> builder)
        {
            // Configuração base da entidade
            base.Configure(builder);

            // Configuração de tabela
            builder.ToTable("ParametroRegraDistribuicao");

            // Propriedades
            builder.Property(p => p.NomeParametro)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.TipoParametro)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.ValorParametro)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.Descricao)
                .HasMaxLength(500);

            builder.Property(p => p.Obrigatorio)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ValidacaoRegex)
                .HasMaxLength(500);

            builder.Property(p => p.ValorPadrao)
                .HasMaxLength(500);

            // Relacionamentos
            builder.HasOne(p => p.RegraDistribuicao)
                .WithMany(r => r.Parametros)
                .HasForeignKey(p => p.RegraDistribuicaoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices
            builder.HasIndex(p => new { p.RegraDistribuicaoId, p.NomeParametro })
                .HasDatabaseName("IX_ParametroRegra_Regra_Nome")
                .IsUnique();
        }
    }
}