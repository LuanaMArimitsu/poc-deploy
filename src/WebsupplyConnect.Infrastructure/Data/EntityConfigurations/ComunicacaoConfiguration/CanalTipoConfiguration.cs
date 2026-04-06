using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class CanalTipoConfiguration : EntidadeTipificacaoConfiguration<CanalTipo>
    {
        public override void Configure(EntityTypeBuilder<CanalTipo> builder)
        {
            // Chama a configuração base para EntidadeTipificacao (que já inclui EntidadeBase)
            base.Configure(builder);
        }
    }
}
