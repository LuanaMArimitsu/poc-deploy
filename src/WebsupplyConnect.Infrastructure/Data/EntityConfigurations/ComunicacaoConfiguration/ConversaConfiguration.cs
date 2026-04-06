using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Graph.Models;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Infrastructure.Data.EntityConfigurations.Base;

namespace WebsupplyConnect.Infrastructure.Data.EntityConfigurations.ComunicacaoConfiguration
{
    public class ConversaConfiguration : EntidadeBaseConfiguration<Conversa>
    {
        public override void Configure(EntityTypeBuilder<Conversa> builder)
        {
            // Chama a configuração base para EntidadeSincronizavel (que já inclui EntidadeBase)
            base.Configure(builder);

            // Tabela
            builder.ToTable("Conversas");

            // Propriedades específicas da Conversa
            builder.Property(c => c.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.LeadId)
                .IsRequired();

            // A propriedade OportunidadeId está comentada na classe original
            //builder.Property(c => c.OportunidadeId);

            builder.Property(c => c.UsuarioId)
                .IsRequired();

            builder.Property(c => c.CanalId)
                .IsRequired();

            builder.Property(c => c.StatusId)
                .IsRequired();

            builder.Property(c => c.DataInicio)
                .IsRequired();

            builder.Property(c => c.DataUltimaMensagem);

            builder.Property(c => c.IdExternoMeta)
                .HasMaxLength(100);

            builder.Property(c => c.PossuiMensagensNaoLidas)
                .IsRequired();

            builder.Property(c => c.Fixada)
                .IsRequired();

            builder.Property<string>("Contexto")
                .HasMaxLength(500);

            builder.Property<DateTime?>("DataAtualizacaoContexto");

            builder.Property<bool>("TrocaDeContato")
                .HasDefaultValue(false);

            builder.Property<string>("ClassificacaoIA");

            //Todo: Equipe Id precisa ser obrigatório no momento, devido o sistema estar produção, após as conversas existentes serem atualizadas com uma equipe, podemos tornar obrigatório
            builder.Property(c => c.EquipeId);
                

            // Relacionamentos
            builder.HasOne(c => c.Canal)
                .WithMany(co => co.Conversas)
                .HasForeignKey(c => c.CanalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Usuario)
                .WithMany(u => u.Conversas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Lead)
                .WithMany(l => l.Conversas)
                .HasForeignKey(c => c.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Status)
                .WithMany()
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Equipe)
                .WithMany(e => e.Conversas)
                .HasForeignKey(c => c.EquipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices específicos (DataUltimaModificacao já é configurado na classe base)
            builder.HasIndex(c => c.LeadId)
                .HasDatabaseName("IX_Conversas_LeadId");

            builder.HasIndex(c => c.UsuarioId)
                .HasDatabaseName("IX_Conversas_UsuarioId");

            builder.HasIndex(c => c.CanalId)
                .HasDatabaseName("IX_Conversas_CanalId");

            builder.HasIndex(c => c.StatusId)
                .HasDatabaseName("IX_Conversas_StatusId");

            builder.HasIndex(c => c.IdExternoMeta)
                .HasDatabaseName("IX_Conversas_IdExternoMeta");

            builder.HasIndex(c => c.PossuiMensagensNaoLidas)
                .HasDatabaseName("IX_Conversas_PossuiMensagensNaoLidas");
        }
    }
}
