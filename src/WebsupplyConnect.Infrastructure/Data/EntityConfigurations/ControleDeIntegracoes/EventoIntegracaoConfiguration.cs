using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ControleDeIntegracoes
{
    public class EventoIntegracaoConfiguration : IEntityTypeConfiguration<EventoIntegracao>
    {
        public void Configure(EntityTypeBuilder<EventoIntegracao> builder)
        {
            builder.ToTable("EventosIntegracao");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.SistemaExternoId)
                .IsRequired();

            builder.Property(e => e.Direcao)
                .IsRequired();

            builder.Property(e => e.TipoEvento)
                .IsRequired();

            builder.Property(e => e.Sucesso)
                .IsRequired();

            builder.Property(e => e.PayloadEnviado)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.PayloadRecebido)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.CodigoResposta)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MensagemErro)
                .HasMaxLength(1000);

            builder.Property(e => e.DataEvento)
                .IsRequired();

            builder.Property(e => e.EntidadeOrigemId)
               .IsRequired(false);

            builder.Property(e => e.TipoEntidadeOrigem)
               .HasConversion<int>()
               .IsRequired(false);
        }
    }
}