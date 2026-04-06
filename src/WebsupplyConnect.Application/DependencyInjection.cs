using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WebsupplyConnect.Application.Interfaces.Comum;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Configuracao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.Equipe;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Application.Interfaces.Notificacao;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Produto;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Application.Services.Comum;
using WebsupplyConnect.Application.Services.Comunicacao;
using WebsupplyConnect.Application.Services.Configuracao;
using WebsupplyConnect.Application.Services.Distribuicao;
using WebsupplyConnect.Application.Services.Empresa;
using WebsupplyConnect.Application.Services.Equipe;
using WebsupplyConnect.Application.Services.Lead;
using WebsupplyConnect.Application.Services.Notificacao;
using WebsupplyConnect.Application.Services.Oportunidade;
using WebsupplyConnect.Application.Services.Produto;
using WebsupplyConnect.Application.Services.Usuario;
using WebsupplyConnect.Application.Validators.Equipe;
using WebsupplyConnect.Application.Validators.Comum;
using WebsupplyConnect.Application.Validators.Comunicacao;
using WebsupplyConnect.Application.Validators.Distribuicao;
using WebsupplyConnect.Application.Validators.Notificacao;
using WebsupplyConnect.Application.Validators.Oportunidade;
using WebsupplyConnect.Application.Validators.Usuario;
using WebsupplyConnect.Application.Interfaces.Perfil;
using WebsupplyConnect.Application.Services.Perfil;
using WebsupplyConnect.Application.Interfaces.Permissao;
using WebsupplyConnect.Application.Services.Permissao;
using WebsupplyConnect.Application.Validators.Lead;
using WebsupplyConnect.Application.Interfaces.ControleSistemasExternos;
using WebsupplyConnect.Application.Interfaces.Dashboard;
using WebsupplyConnect.Application.Services.ControleSistemasExternos;
using WebsupplyConnect.Application.Interfaces.OLAP;
using WebsupplyConnect.Application.Services.Dashboard;
using WebsupplyConnect.Application.Services.OLAP;
using WebsupplyConnect.Domain.Interfaces.ETL;
using WebsupplyConnect.Domain.Interfaces.OLAP.Dimensoes;
using WebsupplyConnect.Application.Services.ETL;
using WebsupplyConnect.Application.Interfaces.VersaoApp;
using WebsupplyConnect.Application.Services.VersaoApp;

namespace WebsupplyConnect.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddHttpClient();
            services.AddScoped<IWebhookWriterService, WebhookWriterService>();
            services.AddScoped<IWebhookReaderService, WebhookReaderService>();
            services.AddScoped<IUsuarioWriterService, UsuarioWriterService>();
            services.AddScoped<IUsuarioReaderService, UsuarioReaderService>();
            services.AddScoped<IUsuarioEmpresaReaderService, UsuarioEmpresaReaderService>();
            services.AddScoped<ICanalReaderService, CanalReaderService>();
            services.AddScoped<ICanalWriterService, CanalWriterService>();
            services.AddScoped<IEmpresaReaderService, EmpresaReaderService>();
            services.AddScoped<IConversaReaderService, ConversaReaderService>();
            services.AddScoped<IChatBotWriterService, ChatBotWriterService>();
            services.AddScoped<ILeadWriterService, LeadWriterService>();
            services.AddScoped<ILeadResponsavelWriterService, LeadReponsavelWriterService>();
            services.AddScoped<ILeadReaderService, LeadReaderService>();
            services.AddScoped<ILeadExportService, LeadExportService>();

            services.AddScoped<ILeadEstatisticasService, LeadEstatisticasService>();
            services.AddScoped<IMensagemReaderService, MensagemReaderService>();
            services.AddScoped<IMensagemWriterService, MensagemWriterService>();
            services.AddScoped<IMidiaReaderService, MidiaReaderService>();
            services.AddScoped<IMidiaWriterService, MidiaWriterService>();
            services.AddScoped<ITemplateReaderService, TemplateReaderService>();
            services.AddScoped<ITemplateWriterService, TemplateWriterService>();
            services.AddScoped<IConversaWriterService, ConversaWriterService>();
            services.AddScoped<IEnderecoWriterService, EnderecoWriterService>();

            services.AddScoped<IOrigemReaderService, OrigemReaderService>();
            services.AddScoped<IOrigemWriterService, OrigemWriterService>();

            services.AddScoped<ICampanhaReaderService, CampanhaReaderService>();
            services.AddScoped<ICampanhaWriterService, CampanhaWriterService>();

            services.AddScoped<IOportunidadeReaderService, OportunidadeReaderService>();
            services.AddScoped<IProdutoWriterService, ProdutoWriterService>();
            services.AddScoped<IProdutoReaderService, ProdutoReaderService>();
            services.AddScoped<IProdutoHistoricoWriterService, ProdutoHistoricoWriterService>();
            services.AddScoped<IRedistribuicaoService, RedistribuicaoService>();
            services.AddScoped<IEtapaReaderService, EtapaReaderService>();
            services.AddScoped<IFunilReaderService, FunilReaderService>();
            services.AddScoped<IOportunidadeWriterService, OportunidadeWriterService>();
            services.AddScoped<IIaWriterService, IaWriterService>();
            services.AddScoped<IPromptEmpresasReaderService, PromptEmpresasReaderService>();
            //Comum
            services.AddScoped<IFeriadoReaderService, FeriadoReaderService>();
            services.AddScoped<IFeriadoWriterService, FeriadoWriterService>();

            //Distribuicao
            services.AddScoped<IHorariosDistribuicaoService, HorariosDistribuicaoService>();

            //Equipe
            services.AddScoped<IEquipeWriterService, EquipeWriterService>();
            services.AddScoped<IEquipeReaderService, EquipeReaderService>();
            services.AddScoped<IMembroEquipeReaderService, MembroEquipeReaderService>();
            services.AddScoped<IMembroEquipeWriterService, MembroEquipeWriterService>();
            services.AddScoped<ITipoEquipeReadService, TipoEquipeReadService>();
            services.AddScoped<IStatusMembroEquipeReadService, StatusMembroEquipeReadService>();

            //Factory
            services.AddScoped<IMensagemEnvioFilaFactory, MensagemEnvioFilaFactory>();

            services.AddScoped<IMidiaReaderService, MidiaReaderService>();
            services.AddScoped<INotificacaoWriterService, NotificacaoWriterService>();
            services.AddScoped<INotificacaoReaderService, NotificacaoReaderService>();
            services.AddScoped<IDispositivosWriterService, DispositivosWriterService>();
            services.AddScoped<IDispositivosReaderService, DispositivoReaderService>();
            services.AddScoped<ISistemaExternoReaderService, SistemaExternoReaderService>();
            services.AddScoped<IEventoIntegracaoWriterService, EventoIntegracaoWriterService>();

            //Permissoes
            services.AddScoped<IPermissaoReaderService, PermissaoReaderService>();
            services.AddScoped<IPermissaoWriterService, PermissaoWriterService>();
            services.AddScoped<IRoleReaderService, RoleReaderService>();
            services.AddScoped<IRoleWriterService, RoleWriterService>();

            //Evento
            services.AddScoped<ILeadEventoWriterService, LeadEventoWriterService>();
            services.AddScoped<ILeadEventoReaderService, LeadEventoReaderService>();

            // Validators
            services.AddValidatorsFromAssemblyContaining<CanalValidator>();
            services.AddValidatorsFromAssemblyContaining<ConversaStatusValidator>();
            services.AddValidatorsFromAssemblyContaining<FeriadoCriarDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<MensagemRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<AdicionarDispositivoValidator>();
            services.AddValidatorsFromAssemblyContaining<AtualizarEmpresaUsuarioValidator>();
            services.AddValidatorsFromAssemblyContaining<AtualizarVinculosRequestValidator>();
            services.AddValidatorsFromAssemblyContaining<NotificarNovoLeadValidator>();
            services.AddValidatorsFromAssemblyContaining<NotificarNovaMensagemValidator>();
            services.AddValidatorsFromAssemblyContaining<NotificarStatusMensagemAtualizadoValidator>();
            services.AddValidatorsFromAssemblyContaining<NotificarNovoLeadVendedorValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateOportunidadeDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<TransferirLeadValidator>();
            services.AddValidatorsFromAssemblyContaining<ConfigurarHorariosDistribuicaoDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<CriarEquipeDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<AtualizarEquipeDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<AdicionarMembroDtoValidator>();
            services.AddValidatorsFromAssemblyContaining<OrigemResquestDTOValidator>();
            services.AddValidatorsFromAssemblyContaining<NotificarEscalonamentoValidator>();

            // Serviços de Configuração
            services.AddScoped<IPromptConfiguracaoService, PromptConfiguracaoService>();

            // Serviços OLAP
            services.AddScoped<IOLAPConsultaService, OLAPConsultaService>();
            services.AddScoped<IDashboardCatalogoService, DashboardCatalogoService>();
            services.AddScoped<IAcompanhamentoDashboardReaderService, AcompanhamentoDashboardReaderService>();
            services.AddScoped<IConversaClassificacaoAiService, ConversaClassificacaoAiService>();

            // Serviços ETL
            services.AddScoped<IETLProcessamentoService, ETLProcessamentoService>();
            services.AddScoped<IETLDimensoesService, ETLDimensoesService>();
            services.AddScoped<IDimensaoOlapReadService>(sp => sp.GetRequiredService<IETLDimensoesService>());
            services.AddScoped<IETLFatosService, ETLFatosService>();
            services.AddScoped<IETLCalculosService, ETLCalculosService>();

            // Serviços de Versão do App
            services.AddScoped<IVersaoAppReaderService, VersaoAppReaderService>();

            return services;
        }
    }
}
