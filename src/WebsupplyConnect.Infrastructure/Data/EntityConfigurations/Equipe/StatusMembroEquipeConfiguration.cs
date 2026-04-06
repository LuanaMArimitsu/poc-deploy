using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.Configurations
{
    public class StatusMembroEquipeConfiguration : EntidadeTipificacaoConfiguration<StatusMembroEquipe>
    {
        public override void Configure(EntityTypeBuilder<StatusMembroEquipe> builder)
        {
            base.Configure(builder);
        }
    }
}
