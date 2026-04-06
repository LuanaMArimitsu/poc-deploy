using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.LeadConfiguration
{
    /// <summary>
    /// Configuração do Entity Framework Core para a entidade Lead
    /// </summary>
    public class LeadsConfiguration : EntidadeBaseConfiguration<Lead>
    {
        public override void Configure(EntityTypeBuilder<Lead> builder)
        {
            // Configuração da tabela TPT (Table Per Type)
            builder.ToTable("Leads");

            // Configurações de propriedades específicas
            builder.Property(l => l.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Apelido)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(l => l.Email)
                .HasMaxLength(100);

            builder.Property(l => l.Telefone)
                .HasMaxLength(20);

            builder.Property(l => l.Cargo)
                .HasMaxLength(100);

            builder.Property(l => l.WhatsappNumero)
                .HasMaxLength(20);

            builder.Property(l => l.CPF)
                .HasMaxLength(11); // Apenas dígitos

            builder.Property(l => l.DataNascimento)
                .HasColumnType("date"); // Só precisamos da data, sem hora

            builder.Property(l => l.Genero)
                .HasMaxLength(20);

            builder.Property(l => l.NomeEmpresa)
                .HasMaxLength(200);

            builder.Property(l => l.CNPJEmpresa)
                .HasMaxLength(14); // Apenas dígitos

            builder.Property(l => l.LeadStatusId)
                .IsRequired();

            builder.Property(l => l.DataConversaoCliente)
                .HasColumnType("datetime2");

            builder.Property(l => l.NivelInteresse)
                .HasMaxLength(10); // "Baixo", "Médio", "Alto"

            builder.Property(l => l.ObservacoesCadastrais)
                .HasMaxLength(4000); // Texto longo para observações

            builder.Property(l => l.ResponsavelId);
            //.IsRequired();

            builder.Property(l => l.OrigemId)
                .IsRequired();

            builder.Property(x => x.EmpresaId)
                .IsRequired();

            builder.Property(x => x.EquipeId)
                .IsRequired(false);


            // Relacionamentos

            // Relacionamento com Equipe
            builder.HasOne(x => x.Equipe)
                .WithMany()
                .HasForeignKey(x => x.EquipeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Relacionamento com LeadStatus
            builder.HasOne(l => l.LeadStatus)
                .WithMany()
                .HasForeignKey(l => l.LeadStatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com MembroEquipe (Responsável)
            builder.HasOne(l => l.Responsavel)
                .WithMany(m => m.LeadsSobResponsabilidade)
                .HasForeignKey(l => l.ResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com Origem
            builder.HasOne(l => l.Origem)
                .WithMany()
                .HasForeignKey(l => l.OrigemId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com Endereco (Residencial)
            builder.HasOne(l => l.EnderecoResidencial)
                .WithMany()
                .HasForeignKey(l => l.EnderecoResidencialId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Relacionamento com Endereco (Comercial)
            builder.HasOne(l => l.EnderecoComercial)
                .WithMany()
                .HasForeignKey(l => l.EnderecoComercialId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Relacionamento com Empresa
            builder.HasOne(l => l.Empresa)
                .WithMany(e => e.Leads)
                .HasForeignKey(l => l.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Índices

            // Índice para busca por nome
            builder.HasIndex(l => l.Nome);

            builder.HasIndex(l => l.Apelido);

            // Índice para busca por email
            builder.HasIndex(l => l.Email)
                .HasFilter("[Email] IS NOT NULL");


            builder.HasIndex(l => new { l.WhatsappNumero, l.EmpresaId })
                .IsUnique()
                .HasFilter("[WhatsappNumero] IS NOT NULL AND [Excluido] = 0");

            builder.HasIndex(l => new { l.Email, l.EmpresaId })
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL AND [Excluido] = 0");


            // Índice para busca por CPF
            builder.HasIndex(l => l.CPF)
                .HasFilter("[CPF] IS NOT NULL");

            // Índice para busca por status
            builder.HasIndex(l => l.LeadStatusId);

            // Índice para busca por responsável
            builder.HasIndex(l => l.ResponsavelId);

            // Índice para busca por origem
            builder.HasIndex(l => l.OrigemId);

            // Índice para busca por nível de interesse
            builder.HasIndex(l => l.NivelInteresse)
                .HasFilter("[NivelInteresse] IS NOT NULL");

            // Índice para busca de clientes convertidos
            builder.HasIndex(l => l.DataConversaoCliente)
                .HasFilter("[DataConversaoCliente] IS NOT NULL");

            // Índice composto para busca por CNPJ da empresa e nome
            builder.HasIndex(l => new { l.CNPJEmpresa, l.NomeEmpresa })
                .HasFilter("[CNPJEmpresa] IS NOT NULL AND [NomeEmpresa] IS NOT NULL");
        }
    }
}
