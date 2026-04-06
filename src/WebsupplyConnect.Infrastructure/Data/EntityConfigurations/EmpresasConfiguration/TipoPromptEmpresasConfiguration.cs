using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.EmpresasConfiguration
{
    public class TipoPromptEmpresasConfiguration : EntidadeTipificacaoConfiguration<TipoPromptEmpresas>
    {
        public override void Configure(EntityTypeBuilder<TipoPromptEmpresas> builder)
        {
            base.Configure(builder);

            builder.HasKey(tp => tp.Id);
            builder.Property(tp => tp.Codigo)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(tp => tp.Nome)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(tp => tp.Descricao)
                .HasMaxLength(1000);
            builder.Property(tp => tp.Ordem)
                .IsRequired();
            builder.Property(tp => tp.DataCriacao)
                .IsRequired();
            builder.Property(tp => tp.DataModificacao)
                .IsRequired();
        }    
    }
}
