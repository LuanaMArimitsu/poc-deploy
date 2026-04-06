using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Configuracao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ConfiguracaoConfiguration;

public class PromptConfiguracaoVersaoConfiguration : EntidadeBaseConfiguration<PromptConfiguracaoVersao>
{
    public override void Configure(EntityTypeBuilder<PromptConfiguracaoVersao> builder)
    {
        base.Configure(builder);

        builder.ToTable("PromptsConfiguracaoVersoes");

        builder.Property(v => v.PromptConfiguracaoId)
            .IsRequired();

        builder.Property(v => v.NumeroVersao)
            .IsRequired();

        builder.Property(v => v.Publicada)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.ConteudoPrompt)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.DataPublicacao)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(v => v.ContadorUso)
            .IsRequired()
            .HasDefaultValue(0);

        // Unique: uma versão por número dentro de cada configuração
        builder.HasIndex(v => new { v.PromptConfiguracaoId, v.NumeroVersao })
            .IsUnique()
            .HasDatabaseName("IX_PromptsConfiguracaoVersoes_Config_Versao");

        // Index para busca da última versão publicada (query otimizada)
        builder.HasIndex(v => new { v.PromptConfiguracaoId, v.Publicada, v.NumeroVersao })
            .HasDatabaseName("IX_PromptsConfiguracaoVersoes_UltimaPublicada");

        // Relacionamento
        builder.HasOne(v => v.PromptConfiguracao)
            .WithMany(p => p.Versoes)
            .HasForeignKey(v => v.PromptConfiguracaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro de soft delete
        builder.HasQueryFilter(v => !v.Excluido);
    }
}
