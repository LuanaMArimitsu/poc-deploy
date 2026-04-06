using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Oportunidade;
public class Etapa : EntidadeBase
{
    /// <summary>
    /// Nome da etapa
    /// </summary>
    public string Nome { get; private set; }

    /// <summary>
    /// Descrição detalhada da etapa
    /// </summary>
    public string Descricao { get; private set; }

    /// <summary>
    /// Ordem sequencial da etapa no funil (1, 2, 3...)
    /// </summary>
    public int Ordem { get; private set; }

    /// <summary>
    /// Cor hexadecimal para representação visual da etapa
    /// </summary>
    public string Cor { get; private set; }

    /// <summary>
    /// Probabilidade padrão de fechamento para oportunidades nesta etapa (0-100%)
    /// </summary>
    public int ProbabilidadePadrao { get; private set; }

    /// <summary>
    /// Indica se esta etapa representa uma oportunidade ativa (pode receber ações)
    /// </summary>
    public bool EhAtiva { get; private set; }

    /// <summary>
    /// Indica se esta etapa representa um estado final do processo
    /// </summary>
    public bool EhFinal { get; private set; }

    /// <summary>
    /// Indica se esta etapa representa uma vitória (venda fechada)
    /// </summary>
    public bool EhVitoria { get; private set; }

    /// <summary>
    /// Indica se esta etapa representa uma oportunidade perdida
    /// </summary>
    public bool EhPerdida { get; private set; }

    /// <summary>
    /// Indica se esta etapa pode ser exibida
    /// </summary>
    public bool EhExibida { get; private set; }

    /// <summary>
    /// ID do funil ao qual esta etapa pertence
    /// </summary>
    public int FunilId { get; private set; }

    /// <summary>
    /// Funil ao qual esta etapa pertence
    /// </summary>
    public virtual Funil Funil { get; private set; }

    /// <summary>
    /// Lista de oportunidades que estão nesta etapa
    /// </summary>
    public virtual ICollection<Oportunidade> Oportunidades { get; private set; }

    /// <summary>
    /// Lista de históricos de etapa que referenciam esta etapa como nova
    /// </summary>
    public virtual ICollection<EtapaHistorico> HistoricosComoEtapaNova { get; private set; }

    /// <summary>
    /// Lista de históricos de etapa que referenciam esta etapa como anterior
    /// </summary>
    public virtual ICollection<EtapaHistorico> HistoricosComoEtapaAnterior { get; private set; }

    /// <summary>
    /// Etapa está ativa ou inativa
    /// </summary>
    public bool Ativo { get; private set; }


    /// <summary>
    /// Construtor protegido para Entity Framework
    /// </summary>
    protected Etapa()
    {
        Oportunidades = [];
        HistoricosComoEtapaNova = [];
        HistoricosComoEtapaAnterior = [];
    }

    /// <summary>
    /// Construtor para criação de nova etapa
    /// </summary>
    /// <param name="nome">Nome da etapa</param>
    /// <param name="descricao">Descrição da etapa</param>
    /// <param name="ordem">Ordem sequencial</param>
    /// <param name="cor">Cor hexadecimal</param>
    /// <param name="probabilidadePadrao">Probabilidade padrão (%)</param>
    /// <param name="funilId">ID do funil</param>
    /// <param name="ehAtiva">Se é etapa ativa</param>
    /// <param name="ehFinal">Se é etapa final</param>
    /// <param name="ehVitoria">Se representa vitória</param>
    /// <param name="ehPerdida">Se representa oportunidade perdida</param>
    /// <param name="ehExibida">Se a etapa pode ser exibida</param>
    public Etapa(
        string nome,
        string descricao,
        int ordem,
        string cor,
        int probabilidadePadrao,
        int funilId,
        bool ehAtiva,
        bool ehFinal,
        bool ehVitoria,
        bool ehPerdida,
        bool ehExibida,
        bool ativo = true) : this()
    {
        ValidarParametros(nome, descricao, ordem, cor, probabilidadePadrao, funilId);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Ordem = ordem;
        Cor = cor.Trim();
        ProbabilidadePadrao = probabilidadePadrao;
        FunilId = funilId;
        EhAtiva = ehAtiva;
        EhFinal = ehFinal;
        EhVitoria = ehVitoria;
        EhPerdida = ehPerdida;
        EhExibida = ehExibida;
        DataCriacao = TimeHelper.GetBrasiliaTime();
        DataModificacao = TimeHelper.GetBrasiliaTime();
        Ativo = ativo;
        // Validação de regras de negócio
        ValidarRegrasDeNegocio();
    }

    /// <summary>
    /// Atualiza as informações básicas da etapa
    /// </summary>
    /// <param name="nome">Novo nome</param>
    /// <param name="descricao">Nova descrição</param>
    /// <param name="cor">Nova cor</param>
    public void AtualizarInformacoes(string nome, string descricao, string cor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da etapa é obrigatório");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição da etapa é obrigatória");

        if (string.IsNullOrWhiteSpace(cor))
            throw new DomainException("A cor da etapa é obrigatória");

        if (!ValidarCorHexadecimal(cor))
            throw new DomainException("A cor deve estar no formato hexadecimal válido (ex: #FF5722)");

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Cor = cor.Trim();   

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a propriedade Ativo da etapa
    /// </summary>

    public void AtualizarAtivo(bool ativo)
    {
        Ativo = ativo;
        AtualizarDataModificacao();
    }
    /// <summary>
    /// Atualiza a probabilidade padrão da etapa
    /// </summary>
    /// <param name="novaProbabilidade">Nova probabilidade padrão (0-100)</param>
    public void AtualizarProbabilidadePadrao(int novaProbabilidade)
    {
        if (novaProbabilidade < 0 || novaProbabilidade > 100)
            throw new DomainException("A probabilidade deve estar entre 0 e 100");

        ProbabilidadePadrao = novaProbabilidade;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza a ordem da etapa no funil
    /// </summary>
    /// <param name="novaOrdem">Nova ordem</param>
    public void AtualizarOrdem(int novaOrdem)
    {
        if (novaOrdem < 0)
            throw new DomainException("A ordem deve ser um número positivo");

        Ordem = novaOrdem;

        AtualizarDataModificacao();
    }

    /// <summary>
    /// Atualiza as características comportamentais da etapa
    /// </summary>
    /// <param name="ehAtiva">Se representa estado ativo</param>
    /// <param name="ehFinal">Se representa estado final</param>
    /// <param name="ehVitoria">Se representa vitória</param>
    /// <param name="ehPerdida">Se representa oportunidade perdida</param>
    public void AtualizarCaracteristicas(bool ehAtiva, bool ehFinal, bool ehVitoria, bool ehPerdida)
    {
        EhAtiva = ehAtiva;
        EhFinal = ehFinal;
        EhVitoria = ehVitoria;
        EhPerdida = ehPerdida;

        AtualizarDataModificacao();

        // Revalidar regras após mudança
        ValidarRegrasDeNegocio();
    }

    /// <summary>
    /// Obtém a próxima etapa sequencial (se existir)
    /// </summary>
    /// <returns>Próxima etapa ou null</returns>
    public Etapa? ObterProximaEtapa()
    {
        return Funil?.Etapas?
            .Where(e => e.Ordem > Ordem && !e.Excluido)
            .OrderBy(e => e.Ordem)
            .FirstOrDefault();
    }

    /// <summary>
    /// Obtém a etapa anterior sequencial (se existir)
    /// </summary>
    /// <returns>Etapa anterior ou null</returns>
    public Etapa? ObterEtapaAnterior()
    {
        return Funil?.Etapas?
            .Where(e => e.Ordem < Ordem && !e.Excluido)
            .OrderByDescending(e => e.Ordem)
            .FirstOrDefault();
    }

    /// <summary>
    /// Valida os parâmetros do construtor
    /// </summary>
    private static void ValidarParametros(string nome, string descricao,
        int ordem, string cor, int probabilidadePadrao, int funilId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da etapa é obrigatório");

        if (nome.Length > 100)
            throw new DomainException("O nome da etapa não pode ter mais de 100 caracteres");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição da etapa é obrigatória");

        if (descricao.Length > 500)
            throw new DomainException("A descrição da etapa não pode ter mais de 500 caracteres");

        if (ordem < 0)
            throw new DomainException("A ordem deve ser um número positivo");

        if (string.IsNullOrWhiteSpace(cor))
            throw new DomainException("A cor da etapa é obrigatória");

        if (!ValidarCorHexadecimal(cor))
            throw new DomainException("A cor deve estar no formato hexadecimal válido (ex: #FF5722)");

        if (probabilidadePadrao < 0 || probabilidadePadrao > 100)
            throw new DomainException("A probabilidade padrão deve estar entre 0 e 100");

        if (funilId <= 0)
            throw new DomainException("ID do funil é obrigatório");
    }

    /// <summary>
    /// Valida regras de negócio específicas
    /// </summary>
    private void ValidarRegrasDeNegocio()
    {
        // Etapa final não pode ser ativa
        if (EhFinal && EhAtiva)
            throw new DomainException("Uma etapa final não pode ser ativa");

        // Etapa de vitória deve ser final
        if (EhVitoria && !EhFinal)
            throw new DomainException("Uma etapa de vitória deve ser final");

        // Etapa de perda deve ser final
        if (EhPerdida && !EhFinal)
            throw new DomainException("Uma etapa de perda deve ser final");

        // Etapa ativa não pode ser de vitória nem de perda
        if (EhAtiva && (EhVitoria || EhPerdida))
            throw new DomainException("Uma etapa ativa não pode ser de vitória ou perda");

        // Etapa não pode ser de vitória e perda ao mesmo tempo
        if (EhVitoria && EhPerdida)
            throw new DomainException("Uma etapa não pode ser de vitória e perda ao mesmo tempo");

        // Etapa de vitória deve ter probabilidade 100%
        if (EhVitoria && ProbabilidadePadrao != 100)
            throw new DomainException("Uma etapa de vitória deve ter probabilidade padrão de 100%");

        // Etapa de perda deve ter probabilidade 0%
        if (EhPerdida && ProbabilidadePadrao != 0)
            throw new DomainException("Uma etapa de perda deve ter probabilidade padrão de 0%");
    }

    /// <summary>
    /// Valida se a cor está no formato hexadecimal correto
    /// </summary>
    /// <param name="cor">Cor a ser validada</param>
    /// <returns>True se válida</returns>
    private static bool ValidarCorHexadecimal(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            return false;

        cor = cor.Trim();

        // Deve começar com # e ter 7 caracteres (#RRGGBB) ou 4 caracteres (#RGB)
        if (!cor.StartsWith("#"))
            return false;

        if (cor.Length != 7 && cor.Length != 4)
            return false;

        // Verifica se todos os caracteres após # são hexadecimais
        var hexChars = cor.Substring(1);
        return hexChars.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}