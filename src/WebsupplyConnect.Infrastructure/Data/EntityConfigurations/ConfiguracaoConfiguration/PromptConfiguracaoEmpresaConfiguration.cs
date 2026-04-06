using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Configuracao;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ConfiguracaoConfiguration;

public class PromptConfiguracaoEmpresaConfiguration : IEntityTypeConfiguration<PromptConfiguracaoEmpresa>
{
    public void Configure(EntityTypeBuilder<PromptConfiguracaoEmpresa> builder)
    {
        builder.ToTable("PromptsConfiguracaoEmpresas");

        // Chave composta
        builder.HasKey(e => new { e.PromptConfiguracaoId, e.EmpresaId });

        builder.Property(e => e.PromptConfiguracaoId)
            .IsRequired();

        builder.Property(e => e.EmpresaId)
            .IsRequired();

        // Relacionamentos
        builder.HasOne(e => e.PromptConfiguracao)
            .WithMany(p => p.Empresas)
            .HasForeignKey(e => e.PromptConfiguracaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice para consultas por empresa
        builder.HasIndex(e => e.EmpresaId)
            .HasDatabaseName("IX_PromptsConfiguracaoEmpresas_EmpresaId");
    }
}
