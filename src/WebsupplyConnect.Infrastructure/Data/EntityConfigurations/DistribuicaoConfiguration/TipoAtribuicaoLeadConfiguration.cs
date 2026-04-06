using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.DistribuicaoConfiguration
{
    public class TipoAtribuicaoLeadConfiguration : EntidadeTipificacaoConfiguration<TipoAtribuicaoLead>
    {
        public override void Configure(EntityTypeBuilder<TipoAtribuicaoLead> builder)
        {
            base.Configure(builder);
            
            // IMPORTANTE: Remover a chamada ToTable para usar a tabela da entidade base
            // builder.ToTable("TipoAtribuicoesLead"); - Esta linha deve ser removida
            
            // Configuração da propriedade de navegação para AtribuicoesLead
            builder.HasMany(t => t.AtribuicoesLead)
                  .WithOne(a => a.TipoAtribuicao)
                  .HasForeignKey(a => a.TipoAtribuicaoId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}