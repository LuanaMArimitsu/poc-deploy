using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    public class CampanhaConfiguration : EntidadeBaseConfiguration<Campanha>
    {
        public override void Configure(EntityTypeBuilder<Campanha> builder)
        {
            // Chama a configuração base para EntidadeBase
            base.Configure(builder);

            // Tabela
            builder.ToTable("Campanhas");

            // Propriedades
            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Codigo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Ativo)
                .IsRequired();

            builder.Property(c => c.Temporaria)
                .IsRequired();

            builder.Property(c => c.IdTransferida)
                .IsRequired(false);

            builder.Property(c => c.DataInicio)
                .IsRequired(false);

            builder.Property(c => c.DataFim)
                .IsRequired(false);

            builder.Property(c => c.DataTransferencia)
                .IsRequired(false);

            builder.Property(c => c.EquipeId);
                //.IsRequired(false);

            // Relacionamento: Campanha pertence a uma Empresa
            builder.HasOne(c => c.Empresa)
                .WithMany(e => e.Campanhas)
                .HasForeignKey(c => c.EmpresaId)
                .IsRequired();

            builder.HasOne(c => c.Equipe)
                .WithMany(e => e.Campanhas)
                .HasForeignKey(c => c.EquipeId)
                .IsRequired(false);

            // Índice único composto: Codigo + EmpresaId
            builder.HasIndex(c => new { c.Codigo, c.EmpresaId }).IsUnique();

        }
    }
}
