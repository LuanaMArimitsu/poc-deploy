using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class CanalConfiguration : IEntityTypeConfiguration<Canal>
    {
        public void Configure(EntityTypeBuilder<Canal> builder)
        {
            // Tabela e chave primária
            builder.ToTable("Canais");
            builder.HasKey(c => c.Id);

            builder.Property(d => d.Id)
                .IsRequired()
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // Diz ao EF: "deixa o banco gerar"
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            // Propriedades obrigatórias
            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(90);

            builder.Property(c => c.Descricao)
                .HasMaxLength(550);

            builder.Property(c => c.CanalTipoId)
                .IsRequired();

            builder.Property(c => c.Ativo)
                .IsRequired();

            builder.Property(c => c.EmpresaId)
                .IsRequired();

            builder.Property(c => c.LimiteDiario);

            builder.Property(c => c.WhatsAppNumero)
                .HasMaxLength(20);

            builder.Property(c => c.ConfiguracaoIntegracao)
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.OrigemPadraoId)
                .IsRequired();

            // Relacionamentos
            builder.HasOne(c => c.Empresa)
                .WithMany(e => e.Canais)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CanalTipo)
                .WithMany()
                .HasForeignKey(c => c.CanalTipoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(c => c.WhatsAppNumero);
            builder.HasIndex(c => c.EmpresaId);
        }
    }
}
