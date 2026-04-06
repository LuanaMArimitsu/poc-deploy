using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OportunidadesConfiguration
{
    public class FunilConfiguration : EntidadeBaseConfiguration<Funil>
    {
        public override void Configure(EntityTypeBuilder<Funil> builder)
        {
            base.Configure(builder);

            builder.ToTable("Funil");

            builder.Property(f => f.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.EmpresaId)
                .IsRequired();

            builder.Property(f => f.EhPadrao)
                .IsRequired();

            builder.Property(f => f.Cor)
                .HasMaxLength(7);

            builder.Property(f => f.Ativo)
                .IsRequired();

            builder.HasOne(f => f.Empresa)
                .WithMany()
                .HasForeignKey(f => f.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}