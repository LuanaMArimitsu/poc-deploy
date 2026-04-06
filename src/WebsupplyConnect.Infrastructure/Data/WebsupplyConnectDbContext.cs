using Microsoft.EntityFrameworkCore;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Configuracao;
using WebsupplyConnect.Domain.Entities.ControleDeIntegracoes;
using WebsupplyConnect.Domain.Entities.Distribuicao;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Entities.Equipe;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Domain.Entities.OLAP.Controle;
using WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;
using WebsupplyConnect.Domain.Entities.OLAP.Fatos;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Entities.Permissao;
using WebsupplyConnect.Domain.Entities.Produto;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Entities.VersaoApp;
using WebsupplyConnect.Infrastructure.Data.Seeds;

namespace WebsupplyConnect.Infrastructure.Data
{
    /// <summary>
    /// Contexto principal do Entity Framework Core para o sistema WebsupplyConnect
    /// </summary>
    /// <remarks>
    /// Construtor com opções de configuração do DbContext
    /// </remarks>
    /// <param name="options">Opções de configuração</param>
    public class WebsupplyConnectDbContext(DbContextOptions<WebsupplyConnectDbContext> options) : DbContext(options)
    {

        #region DbSets - Contexto Empresarial

        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<GrupoEmpresa> GrupoEmpresas { get; set; }
        public DbSet<PromptEmpresas> PromptEmpresas { get; set; }

        #endregion

        #region DbSets - Contexto Configuração

        public DbSet<PromptConfiguracao> PromptsConfiguracao { get; set; }
        public DbSet<PromptConfiguracaoVersao> PromptsConfiguracaoVersoes { get; set; }
        public DbSet<PromptConfiguracaoEmpresa> PromptsConfiguracaoEmpresas { get; set; }

        #endregion

        #region DbSets - Contexto Usuários

        public DbSet<DiaSemana> DiaSemana { get; set; }
        public DbSet<Dispositivo> Dispositivo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<UsuarioEmpresa> UsuarioEmpresa { get; set; }
        public DbSet<UsuarioHorario> UsuarioHorario { get; set; }

        #endregion

        #region DbSets - Contexto Leads/Contatos

        public DbSet<Endereco> Endereco { get; set; }
        public DbSet<Lead> Lead { get; set; }
        public DbSet<LeadStatus> LeadStatus { get; set; }
        public DbSet<LeadStatusHistorico> LeadStatusHistorico { get; set; }
        public DbSet<Origem> Origem { get; set; }
        public DbSet<OrigemTipo> OrigemTipo { get; set; }
        public DbSet<LeadEvento> LeadEvento { get; set; }

        #endregion

        #region DbSets - Contexto Oportunidades/Vendas

        //public DbSet<Oportunidade> Oportunidades { get; set; }
        //public DbSet<OportunidadeStatus> OportunidadesStatus { get; set; }

        #endregion

        #region DbSets - Contexto Comunicação

        public DbSet<Campanha> Campanha { get; set; }
        public DbSet<Canal> Canal { get; set; }
        public DbSet<CanalTipo> CanalTipos { get; set; }
        public DbSet<Conversa> Conversa { get; set; }
        public DbSet<ConversaStatus> ConversasStatus { get; set; }
        public DbSet<Mensagem> Mensagem { get; set; }
        public DbSet<MensagemStatus> MensagemStatus { get; set; }
        public DbSet<MensagemSugestao> MensagemSugestoes { get; set; }
        public DbSet<MensagemSugestaoFeedback> MensagemSugestoesFeedback { get; set; }
        public DbSet<MensagemTipo> MensagemTipos { get; set; }
        public DbSet<Midia> Midia { get; set; }
        public DbSet<MidiaStatusProcessamento> MidiaStatusProcessamento { get; set; }
        public DbSet<Template> Template { get; set; }
        public DbSet<TemplateCategoria> TemplateCategorias { get; set; }
        public DbSet<WebhookMeta> WebhookMeta { get; set; }
        public DbSet<WebhookMetaTipoEvento> WebhookMetaTipoEventos { get; set; }

        #endregion

        #region DbSets - Contexto Notificação
        public DbSet<Notificacao> Notificacao { get; set; }
        public DbSet<NotificacaoStatus> NotificacaoStatus { get; set; }
        public DbSet<NotificacaoTipo> NotificacaoTipo { get; set; }
        public DbSet<UsuarioNotificacaoConfiguracao> UsuarioNotificacaoConfiguracao { get; set; }
        #endregion

        #region DbSets - Contexto Oportunidades
        public DbSet<Etapa> Etapas { get; set; }
        public DbSet<EtapaHistorico> EtapasHistorico { get; set; }
        public DbSet<Funil> Funils { get; set; }
        public DbSet<Oportunidade> Oportunidades { get; set; }
        public DbSet<TipoInteresse> TipoInteresses { get; set; }

        #endregion

        #region  DbSets - Contexto Distribuicao
        public DbSet<ConfiguracaoDistribuicao> ConfiguracaoDistribuicao { get; set; }
        public DbSet<RegraDistribuicao> RegraDistribuicao { get; set; }
        public DbSet<TipoRegraDistribuicao> TipoRegraDistribuicao { get; set; }
        public DbSet<ParametroRegraDistribuicao> ParametroRegraDistribuicao { get; set; }
        public DbSet<AtribuicaoLead> AtribuicaoLead { get; set; }
        public DbSet<TipoAtribuicaoLead> TipoAtribuicaoLead { get; set; }
        public DbSet<FilaDistribuicao> FilaDistribuicao { get; set; }
        public DbSet<StatusFilaDistribuicao> StatusFilaDistribuicao { get; set; }
        public DbSet<MetricaVendedor> MetricaVendedor { get; set; }
        public DbSet<HistoricoDistribuicao> HistoricoDistribuicao { get; set; }
        public DbSet<MetricaDistribuicao> MetricaDistribuicao { get; set; }
        #endregion

        #region DbSets - Contexto Comum
        public DbSet<Feriado> Feriado { get; set; }
        #endregion

        #region DbSets - Contexto Produto
        public DbSet<Produto> Produto { get; set; }
        public DbSet<ProdutoEmpresa> ProdutoEmpresa { get; set; }
        public DbSet<ProdutoHistorico> ProdutoHistorico { get; set; }
        #endregion

        #region DbSets - Contexto Equipe
        public DbSet<Equipe> Equipe { get; set; }
        public DbSet<MembroEquipe> MembrosEquipe { get; set; }
        public DbSet<TipoEquipe> TipoEquipe { get; set; }
        public DbSet<StatusMembroEquipe> StatusMembrosEquipe { get; set; }
        #endregion

        #region DbSets - Contexto Perfil
        public DbSet<Permissao> Permissao { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RolePermissao> RolePermissao { get; set; }
        public DbSet<UsuarioRole> UsuarioRole { get; set; }
        #endregion

        public DbSet<SistemaExterno> SistemaExterno { get; set; }

        #region DbSets - Contexto OLAP

        public DbSet<DimensaoTempo> DimensaoTempo { get; set; }
        public DbSet<DimensaoEmpresa> DimensaoEmpresa { get; set; }
        public DbSet<DimensaoEquipe> DimensaoEquipe { get; set; }
        public DbSet<DimensaoVendedor> DimensaoVendedor { get; set; }
        public DbSet<DimensaoStatusLead> DimensaoStatusLead { get; set; }
        public DbSet<DimensaoOrigem> DimensaoOrigem { get; set; }
        public DbSet<DimensaoCampanha> DimensaoCampanha { get; set; }
        public DbSet<DimensaoCampanhaMapeamento> DimensaoCampanhaMapeamento { get; set; }
        public DbSet<DimensaoFunil> DimensaoFunil { get; set; }
        public DbSet<DimensaoEtapaFunil> DimensaoEtapaFunil { get; set; }
        public DbSet<FatoOportunidadeMetrica> FatoOportunidadeMetrica { get; set; }
        public DbSet<FatoLeadAgregado> FatoLeadAgregado { get; set; }
        public DbSet<FatoEventoAgregado> FatoEventoAgregado { get; set; }
        public DbSet<ETLControleProcessamento> ETLControleProcessamento { get; set; }

        #endregion

        #region DbSets - Contexto Versão App

        public DbSet<VersaoApp> VersaoApp { get; set; }

        #endregion

        /// <summary>
        /// Configura o modelo de dados para o Entity Framework Core
        /// </summary>
        /// <param name="modelBuilder">Builder para configuração do modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar todas as configurações de entidades automaticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebsupplyConnectDbContext).Assembly);

            // Applicação dos dados iniciais (seeds)
            TipificacaoSeeds.ConfigurarTodosTipificacoes(modelBuilder);

            // Aplicação dos dados iniciais de feriados
            FeriadoSeeds.ConfigurarTodosFeriados(modelBuilder);

            //impede exclusões em cascata automáticas
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<EntidadeTipificacao>()
                .HasDiscriminator<string>("TipoEntidade")
                .HasValue<LeadStatus>("LeadStatus")
                .HasValue<OrigemTipo>("OrigemTipo")
                .HasValue<MensagemTipo>("MensagemTipos")
                .HasValue<MensagemStatus>("MensagemStatus")
                .HasValue<ConversaStatus>("ConversaStatus")
                .HasValue<MidiaStatusProcessamento>("MidiaStatusProcessamento")
                .HasValue<CanalTipo>("CanalTipo")
                .HasValue<WebhookMetaTipoEvento>("WebhookMetaTipoEvento")
                .HasValue<NotificacaoStatus>("NotificacaoStatus")
                .HasValue<NotificacaoTipo>("NotificacaoTipo")
                .HasValue<TemplateCategoria>("TemplateCategoria")
                .HasValue<TipoRegraDistribuicao>("TipoRegraDistribuicao")
                .HasValue<StatusFilaDistribuicao>("StatusFilaDistribuicao")
                .HasValue<TipoAtribuicaoLead>("TipoAtribuicaoLead")
                .HasValue<ProdutoOperacaoTipo>("ProdutoOperacao")
                .HasValue<StatusMembroEquipe>("StatusMembroEquipe")
                .HasValue<TipoPromptEmpresas>("TipoPromptEmpresas")
                .HasValue<TipoEquipe>("TipoEquipe");

            base.OnModelCreating(modelBuilder);
        }
    }
}
