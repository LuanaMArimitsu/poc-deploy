using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Configuracao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ConfiguracaoConfiguration;

public class PromptConfiguracaoConfiguration : EntidadeBaseConfiguration<PromptConfiguracao>
{
    public override void Configure(EntityTypeBuilder<PromptConfiguracao> builder)
    {
        base.Configure(builder);

        builder.ToTable("PromptsConfiguracao");

        builder.Property(p => p.Codigo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descricao)
            .HasMaxLength(500);

        // Índice único no Codigo (com filtro de soft delete)
        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("IX_PromptsConfiguracao_Codigo");

        // Relacionamentos
        builder.HasMany(p => p.Versoes)
            .WithOne(v => v.PromptConfiguracao)
            .HasForeignKey(v => v.PromptConfiguracaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Empresas)
            .WithOne(e => e.PromptConfiguracao)
            .HasForeignKey(e => e.PromptConfiguracaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro de soft delete
        builder.HasQueryFilter(p => !p.Excluido);
    }
}
