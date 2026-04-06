using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade Endereco
    /// </summary>
    public class EnderecoConfiguration : EntidadeBaseConfiguration<Endereco>
    {
        public override void Configure(EntityTypeBuilder<Endereco> builder)
        {
            // Chama a configuração da classe base primeiro (Id, DataCriacao, DataModificacao, Excluido, etc.)
            base.Configure(builder);

            // Configuração da tabela
            builder.ToTable("Enderecos");

            // Configurações de propriedades específicas
            builder.Property(e => e.Logradouro)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Numero)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Complemento)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(e => e.Bairro)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Cidade)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Pais)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("Brasil");

            builder.Property(e => e.CEP)
                .IsRequired()
                .HasMaxLength(8);

            // Índices

            // Índice para busca por CEP
            builder.HasIndex(e => e.CEP);

            // Índice para busca por cidade e estado
            builder.HasIndex(e => new { e.Cidade, e.Estado });

            // Relacionamentos
            // Nota: Os relacionamentos com Lead (endereço residencial e comercial)
            // são configurados na entidade Lead, pois é ela que possui as chaves estrangeiras

            // Conversões

            // Garantindo que o CEP seja armazenado sem formatação (apenas dígitos)
            // Isso já é tratado no domínio através do método LimparCep, mas é uma boa prática
            // reforçar isso na configuração
        }
    }
}
