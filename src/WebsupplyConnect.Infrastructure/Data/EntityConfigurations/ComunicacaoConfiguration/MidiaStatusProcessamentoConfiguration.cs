using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class MidiaStatusProcessamentoConfiguration : EntidadeTipificacaoConfiguration<MidiaStatusProcessamento>
    {
        public override void Configure(EntityTypeBuilder<MidiaStatusProcessamento> builder)
        {
            // Chama a configuração base para EntidadeTipificacao (que já inclui EntidadeBase)
            base.Configure(builder);


            builder.Property(s => s.Finalizado)
                .IsRequired();

            // Índices específicos
            builder.HasIndex(s => s.Finalizado)
                .HasDatabaseName("IX_MidiaStatusProcessamento_Finalizado");
        }
    }
}
