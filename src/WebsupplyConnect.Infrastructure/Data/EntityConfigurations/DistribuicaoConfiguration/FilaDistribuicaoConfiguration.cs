using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    /// <summary>
    /// Configuração de mapeamento EF Core para a entidade FilaDistribuicao
    /// </summary>
    public class FilaDistribuicaoConfiguration : EntidadeBaseConfiguration<FilaDistribuicao>
    {
        public override void Configure(EntityTypeBuilder<FilaDistribuicao> builder)
        {
            // Configuração base da entidade
            base.Configure(builder);

            // Configuração de tabela
            builder.ToTable("FilaDistribuicao");

            // Propriedades
            builder.Property(f => f.PosicaoFila)
                .IsRequired();

            builder.Property(f => f.DataUltimoLeadRecebido)
                .HasColumnType("datetime2");

            builder.Property(f => f.PesoAtual)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(f => f.QuantidadeLeadsRecebidos)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(f => f.DataEntradaFila)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(f => f.DataProximaElegibilidade)
                .HasColumnType("datetime2");

            builder.Property(f => f.MotivoStatusAtual)
                .HasMaxLength(500);

            // Relacionamentos
            builder.HasOne(f => f.MembroEquipe)
                .WithMany()
                .HasForeignKey(f => f.MembroEquipeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(f => f.Empresa)
                .WithMany()
                .HasForeignKey(f => f.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // O relacionamento com StatusFilaDistribuicao já está configurado na outra ponta

            // Índices
            builder.HasIndex(f => new { f.EmpresaId, f.PosicaoFila })
                .HasDatabaseName("IX_FilaDistribuicao_Empresa_Posicao");

            builder.HasIndex(f => new { f.EmpresaId, f.MembroEquipeId })
                .HasDatabaseName("IX_FilaDistribuicao_Empresa_MembroEquipe")
                .IsUnique();

            builder.HasIndex(f => new { f.EmpresaId, f.StatusFilaDistribuicaoId, f.DataProximaElegibilidade })
                .HasDatabaseName("IX_FilaDistribuicao_Elegibilidade");
        }
    }
}