using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComumConfiguration
{
    /// <summary>
    /// Configuração da entidade Feriado para o Entity Framework Core
    /// </summary>
    public class FeriadoConfiguration : EntidadeBaseConfiguration<Feriado>
    {
        public override void Configure(EntityTypeBuilder<Feriado> builder)
        {
            base.Configure(builder);

            // Configurações específicas do Feriado
            builder.ToTable("Feriados");

            // Configurações de propriedades específicas da entidade Feriado
            builder.Property(f => f.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Data)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(f => f.Descricao)
                .HasMaxLength(500);

            builder.Property(f => f.Tipo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.UF)
                .HasMaxLength(2);

            builder.Property(f => f.CodigoMunicipio)
                .HasMaxLength(7);

            builder.Property(f => f.Recorrente)
                .IsRequired();

            builder.Property(f => f.DataCriacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(f => f.DataModificacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(f => f.Excluido)
                .IsRequired()
                .HasDefaultValue(false);

            // Relacionamento com Empresa (opcional)
            builder.HasOne<Empresa>()
                .WithMany()
                .HasForeignKey(f => f.EmpresaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para otimização de consultas
            builder.HasIndex(f => new { f.Data, f.Tipo });
            builder.HasIndex(f => f.EmpresaId);
            builder.HasIndex(f => f.UF);
            builder.HasIndex(f => f.Tipo);
            
            // Filtro de consulta para soft delete
            builder.HasQueryFilter(f => !f.Excluido);
        }
    }
}