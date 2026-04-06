using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class TemplateCategoriaConfiguration : EntidadeTipificacaoConfiguration<TemplateCategoria>
    {
        public override void Configure(EntityTypeBuilder<TemplateCategoria> builder)
        {
            base.Configure(builder);
        }
    }
}
