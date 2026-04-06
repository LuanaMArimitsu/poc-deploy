using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface ITemplateWriterService
    {
        object MontarJsonTemplateMeta(string nomeTemplateMeta, string numeroRemetente);
        Task<string> EnviarTemplateAsync(string nomeTemplate, string numeroRemetente, string token, string telefoneId);
        object MontarJsonTemplateIntegracao(string nomeTemplateMeta, string numeroRemetente, List<TemplateParamIntegracao> templateParamIntegracaos);
        Task<string> EnviarTemplateAsync(string nomeTemplate, string numeroRemetente, string token, string telefoneId, object corpoTemplate);
        string MontarPreviewTemplate(string conteudoTemplate, List<TemplateParamIntegracao> parametros);
    }
}
