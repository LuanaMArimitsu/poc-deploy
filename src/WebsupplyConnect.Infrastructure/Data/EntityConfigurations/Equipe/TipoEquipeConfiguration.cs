using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.Configurations
{
    public class TipoEquipeConfiguration : EntidadeTipificacaoConfiguration<TipoEquipe>
    {
        public override void Configure(EntityTypeBuilder<TipoEquipe> builder)
        {
            base.Configure(builder);
        }
    }
}
