using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade LeadStatusHistorico
    /// </summary>
    public class LeadStatusHistoricoConfiguration : IEntityTypeConfiguration<LeadStatusHistorico>
    {
        public void Configure(EntityTypeBuilder<LeadStatusHistorico> builder)
        {
            // Configuração da tabela
            builder.ToTable("LeadsStatusHistorico");

            // Chave primária
            builder.HasKey(h => h.Id);

            // Configuração de identity para Id
            builder.Property(h => h.Id)
                .IsRequired()
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // Diz ao EF: "deixa o banco gerar"
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            // Configurações de propriedades
            builder.Property(h => h.LeadId)
                .IsRequired();

            builder.Property(h => h.StatusAnteriorId)
                .IsRequired(false);

            builder.Property(h => h.StatusNovoId)
                .IsRequired();

            builder.Property(h => h.DataInicio)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(h => h.DataMudanca)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(h => h.ResponsavelId)
                .IsRequired(false);

            builder.Property(h => h.Observacao)
                .HasMaxLength(1000);

            // Relacionamentos

            // Relacionamento com Lead
            builder.HasOne(h => h.Lead)
                .WithMany(l => l.StatusHistorico)
                .HasForeignKey(h => h.LeadId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com LeadStatus (Status Anterior)
            builder.HasOne(h => h.StatusAnterior)
                .WithMany()
                .HasForeignKey(h => h.StatusAnteriorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com LeadStatus (Status Novo)
            builder.HasOne(h => h.StatusNovo)
                .WithMany()
                .HasForeignKey(h => h.StatusNovoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com Usuario (Responsável pela mudança)
            builder.HasOne(h => h.Responsavel)
                .WithMany()
                .HasForeignKey(h => h.ResponsavelId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Índices

            // Índice para consultas por Lead
            builder.HasIndex(h => h.LeadId)
                .HasDatabaseName("IX_LeadStatusHistorico_LeadId");

            // Índice para consultas por Data da Mudança
            builder.HasIndex(h => h.DataMudanca)
                .HasDatabaseName("IX_LeadStatusHistorico_DataMudanca");

            // Índice para consultas por Status Anterior
            builder.HasIndex(h => h.StatusAnteriorId)
                .HasDatabaseName("IX_LeadStatusHistorico_StatusAnteriorId");

            // Índice para consultas por Status Novo
            builder.HasIndex(h => h.StatusNovoId)
                .HasDatabaseName("IX_LeadStatusHistorico_StatusNovoId");

            // Índice para consultas por Responsável
            builder.HasIndex(h => h.ResponsavelId)
                .HasFilter("[ResponsavelId] IS NOT NULL")
                .HasDatabaseName("IX_LeadStatusHistorico_ResponsavelId");

            // Índice composto para análise temporal de mudanças de status por lead
            builder.HasIndex(h => new { h.LeadId, h.DataMudanca })
                .HasDatabaseName("IX_LeadStatusHistorico_Lead_DataMudanca");

            // Índice composto para análise de transições de status (de -> para)
            builder.HasIndex(h => new { h.StatusAnteriorId, h.StatusNovoId })
                .HasDatabaseName("IX_LeadStatusHistorico_StatusAnterior_StatusNovo");
        }
    }
}
