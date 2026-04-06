using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base
{
    public abstract class EntidadeTipificacaoConfiguration<T> : EntidadeBaseConfiguration<T> where T : EntidadeTipificacao
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);

            // Configuração específica para entidades de tipificação
            builder.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Descricao)
                .HasMaxLength(500);

            builder.Property(e => e.Ordem)
                .IsRequired();

            builder.Property(e => e.Icone)
                .HasMaxLength(50);

            builder.Property(e => e.Cor)
                .HasMaxLength(10);

            // ✅ ÍNDICE ÚNICO COM FILTRO USANDO TipoEntidade
            builder.HasIndex(e => e.Codigo)
                .IsUnique()
                .HasDatabaseName($"IX_{typeof(T).Name}_Codigo_Unique")
                .HasFilter($"[TipoEntidade] = N'{typeof(T).Name}'");
        }
    }
}