using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade Origem
    /// </summary>
    public class OrigemConfiguration : EntidadeBaseConfiguration<Origem>
    {
        public override void Configure(EntityTypeBuilder<Origem> builder)
        {
            // Chama a configuração da classe base primeiro (Id, DataCriacao, DataModificacao, Excluido, etc.)
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("Origens");

            // Configurações de propriedades específicas
            builder.Property(o => o.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.Descricao)
                .HasMaxLength(500);

            builder.Property(o => o.OrigemTipoId)
                .IsRequired();

            // Relacionamentos

            // Relacionamento com OrigemTipo (muitos para um)
            builder.HasOne(o => o.OrigemTipo)
                .WithMany()
                .HasForeignKey(o => o.OrigemTipoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Índices

            // Índice para pesquisa por nome
            builder.HasIndex(o => o.Nome);

            // Índice para pesquisa por tipo de origem
            builder.HasIndex(o => o.OrigemTipoId);
        }
    }
}
