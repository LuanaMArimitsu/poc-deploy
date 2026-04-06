using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade OrigemTipo
    /// </summary>
    public class OrigemTipoConfiguration : EntidadeTipificacaoConfiguration<OrigemTipo>
    {
        public override void Configure(EntityTypeBuilder<OrigemTipo> builder)
        {
            // Chama a configuração da classe base primeiro (Id, DataCriacao, DataModificacao, Excluido, Codigo, Nome, Descricao, Ordem, etc.)
            base.Configure(builder);

            // Índices

            // Índice para ordenação
            builder.HasIndex(ot => ot.Ordem)
                .HasDatabaseName("IX_OrigemTipo_Ordem");
        }
    }
}
