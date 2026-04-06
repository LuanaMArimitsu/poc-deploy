using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base
{
    public abstract class EntidadeBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : EntidadeBase
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            // Configuração comum para todas as entidades base
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd() // <- reforça que o banco deve gerar o valor
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            builder.Property(e => e.DataCriacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(e => e.DataModificacao)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(e => e.Excluido)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}

