using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Empresa;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.EmpresasConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework para a entidade PromptEmpresas
    /// </summary>
    public class PromptEmpresasConfiguration : IEntityTypeConfiguration<PromptEmpresas>
    {
        public void Configure(EntityTypeBuilder<PromptEmpresas> builder)
        {
            builder.ToTable("PromptEmpresas");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Prompt)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(p => p.Excluido)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.Sistema)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.DataCriacao)
                .IsRequired();

            builder.Property(p => p.DataUltimaAtualizacao)
                .IsRequired();

            builder.Property(p => p.EmpresaId)
                .IsRequired();

            builder.HasIndex(p => p.EmpresaId);

            builder.Property(p => p.TipoPromptId)
                .IsRequired(false);

            builder.HasOne(l => l.TipoPrompt)
            .WithMany()
            .HasForeignKey(l => l.TipoPromptId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        }
    }
}