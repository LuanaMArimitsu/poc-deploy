using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.OportunidadesConfiguration
{
    public class OportunidadeConfiguration : EntidadeBaseConfiguration<Oportunidade>
    {
        public override void Configure(EntityTypeBuilder<Oportunidade> builder)
        {
            base.Configure(builder);

            builder.ToTable("Oportunidades");

            builder.Property(o => o.LeadId)
                .IsRequired();

            builder.Property(o => o.ProdutoId)
                .IsRequired();

            builder.Property(o => o.EtapaId)
                .IsRequired();

            builder.Property(o => o.Valor)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Probabilidade);

            builder.Property(o => o.DataPrevisaoFechamento)
                .HasColumnType("datetime2");

            builder.Property(o => o.ResponsavelId)
                .IsRequired();

            builder.Property(o => o.OrigemId)
                .IsRequired();  

            builder.Property(o => o.EmpresaId)
                .IsRequired();

            builder.Property(o => o.TipoInteresseId)
               .IsRequired(false);

            builder.Property(o => o.Convertida)
               .IsRequired(false);

            builder.Property(o => o.Observacoes)
                .HasMaxLength(1000);

            builder.Property(o => o.DataFechamento)
                .HasColumnType("datetime2");

            builder.Property(o => o.ValorFinal)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.DataUltimaInteracao)
                .HasColumnType("datetime2");

            builder.Property(o => o.CodEvento)
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(o => o.LeadEventoId)
                .IsRequired(false);

            // Relationships
            builder.HasOne(o => o.Lead)
                .WithMany()
                .HasForeignKey(o => o.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Produto)
                .WithMany()
                .HasForeignKey(o => o.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Etapa)
                .WithMany(e => e.Oportunidades)
                .HasForeignKey(o => o.EtapaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Responsavel)
                .WithMany()
                .HasForeignKey(o => o.ResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Origem)
                .WithMany()
                .HasForeignKey(o => o.OrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Lead)
               .WithMany(o => o.Oportunidades)
               .HasForeignKey(o => o.LeadId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Empresa)
                .WithMany()
                .HasForeignKey(o => o.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.TipoInteresse)
               .WithMany(o => o.Oportunidades)
               .HasForeignKey(e => e.TipoInteresseId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.LeadEvento)
                .WithMany(o => o.Oportunidades)
                .HasForeignKey(o => o.LeadEventoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}