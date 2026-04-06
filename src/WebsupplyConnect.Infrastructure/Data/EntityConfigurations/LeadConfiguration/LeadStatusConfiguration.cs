using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade LeadStatus
    /// </summary>
    public class LeadStatusConfiguration : EntidadeTipificacaoConfiguration<LeadStatus>
    {
        public override void Configure(EntityTypeBuilder<LeadStatus> builder)
        {
            // Chama a configuração da classe base primeiro (Id, DataCriacao, DataModificacao, Excluido, Codigo, Nome, Descricao, Ordem, etc.)
            base.Configure(builder);

            builder.Property(ls => ls.PermiteOportunidades)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ls => ls.ConsiderarCliente)
                .IsRequired()
                .HasDefaultValue(false);

            // Índices

            // Índice para ordenação
            builder.HasIndex(ls => ls.Ordem)
                .HasDatabaseName("IX_LeadStatus_Ordem");

            // Índice para determinar quais status permitem oportunidades
            builder.HasIndex(ls => ls.PermiteOportunidades)
                .HasDatabaseName("IX_LeadStatus_PermiteOportunidades");

            // Índice para identificar status que representam clientes
            builder.HasIndex(ls => ls.ConsiderarCliente)
                .HasDatabaseName("IX_LeadStatus_ConsiderarCliente");
        }
    }
}
