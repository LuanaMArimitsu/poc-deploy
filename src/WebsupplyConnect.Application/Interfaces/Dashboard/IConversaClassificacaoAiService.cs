namespace WebsupplyConnect.Application.Interfaces.Dashboard;

public interface IConversaClassificacaoAiService
{
    Task<ConversaClassificacaoSobDemandaResultado> ProcessarConversaSobDemandaAsync(
        int conversaId,
        bool executarExtracaoContexto,
        bool executarDeteccaoContato,
        bool executarClassificacaoConversa,
        CancellationToken cancellationToken = default);
}

public sealed record ConversaClassificacaoSobDemandaResultado(
    bool ConversaEncontrada,
    int ConversaId,
    bool ExtracaoContextoProcessada,
    bool DeteccaoContatoProcessada,
    bool ClassificacaoConversaProcessada);
