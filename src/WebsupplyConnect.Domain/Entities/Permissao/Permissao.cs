using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Permissao
{
    /// <summary>
    /// Entidade que representa uma permissão auto-suficiente no sistema
    /// </summary>
    public class Permissao : EntidadeBase
    {
        /// <summary>
        /// Código único da permissão (ex: LEAD_VISUALIZAR)
        /// </summary>
        public string Codigo { get; private set; }

        /// <summary>
        /// Nome descritivo da permissão
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada da permissão
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// Módulo ao qual a permissão pertence (LEAD, CONVERSA, VENDAS, etc.)
        /// </summary>
        public string Modulo { get; private set; }

        /// <summary>
        /// Categoria da permissão (LEITURA, ESCRITA, ADMINISTRACAO)
        /// </summary>
        public string Categoria { get; private set; }

        /// <summary>
        /// Ação que a permissão permite (read, write, delete, execute, all)
        /// </summary>
        public string Acao { get; private set; }

        /// <summary>
        /// Recurso/endpoint específico da API que a permissão controla
        /// </summary>
        public string Recurso { get; private set; }

        /// <summary>
        /// Indica se a permissão é crítica para o funcionamento do sistema
        /// </summary>
        public bool IsCritica { get; private set; }

        /// <summary>
        /// Indica se a permissão está ativa no sistema
        /// </summary>
        public bool Ativa { get; private set; }

        /// <summary>
        /// Coleção de associações entre roles e esta permissão
        /// </summary>
        public virtual ICollection<RolePermissao> RolePermissoes { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Permissao()
        {
            RolePermissoes = new HashSet<RolePermissao>();
        }

        /// <summary>
        /// Construtor para criar uma nova permissão
        /// </summary>
        /// <param name="codigo">Código único da permissão</param>
        /// <param name="nome">Nome descritivo da permissão</param>
        /// <param name="descricao">Descrição detalhada da permissão</param>
        /// <param name="modulo">Módulo ao qual pertence</param>
        /// <param name="categoria">Categoria da permissão</param>
        /// <param name="acao">Ação que permite</param>
        /// <param name="recurso">Recurso/endpoint controlado</param>
        /// <param name="isCritica">Se é crítica para o sistema</param>
        public Permissao(
            string codigo,
            string nome,
            string descricao,
            string modulo,
            string categoria,
            string acao,
            string recurso,
            bool isCritica = false)
        {
            ValidarDominio(codigo, nome, descricao, modulo, categoria, acao, recurso);

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Modulo = modulo;
            Categoria = categoria;
            Acao = acao;
            Recurso = recurso;
            IsCritica = isCritica;
            Ativa = true;

            RolePermissoes = new HashSet<RolePermissao>();
        }

        /// <summary>
        /// Atualiza as informações da permissão
        /// </summary>
        /// <param name="nome">Nome descritivo</param>
        /// <param name="descricao">Descrição detalhada</param>
        /// <param name="recurso">Recurso/endpoint controlado</param>
        public void AtualizarInformacoes(string nome, string descricao, string recurso)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da permissão é obrigatório.", nameof(Permissao));

            if (nome.Length > 100)
                throw new DomainException("O nome da permissão não pode ter mais que 100 caracteres.", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição da permissão é obrigatória.", nameof(Permissao));

            if (descricao.Length > 500)
                throw new DomainException("A descrição da permissão não pode ter mais que 500 caracteres.", nameof(Permissao));

            if (!string.IsNullOrWhiteSpace(recurso) && recurso.Length > 200)
                throw new DomainException("O recurso não pode ter mais que 200 caracteres.", nameof(Permissao));

            Nome = nome;
            Descricao = descricao;
            Recurso = recurso ?? string.Empty;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Marca a permissão como crítica
        /// </summary>
        public void MarcarComoCritica()
        {
            IsCritica = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove a marcação de crítica da permissão
        /// </summary>
        public void RemoverMarcacaoCritica()
        {
            IsCritica = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa a permissão no sistema
        /// </summary>
        public void Ativar()
        {
            Ativa = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa a permissão no sistema
        /// </summary>
        public void Desativar()
        {
            if (IsCritica)
                throw new DomainException("Não é possível desativar uma permissão crítica.", nameof(Permissao));

            Ativa = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se a permissão pertence a um módulo específico
        /// </summary>
        /// <param name="modulo">Nome do módulo</param>
        /// <returns>True se pertence ao módulo, false caso contrário</returns>
        public bool PertenceAoModulo(string modulo)
        {
            return string.Equals(Modulo, modulo, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifica se a permissão é de uma categoria específica
        /// </summary>
        /// <param name="categoria">Nome da categoria</param>
        /// <returns>True se é da categoria, false caso contrário</returns>
        public bool EhDaCategoria(string categoria)
        {
            return string.Equals(Categoria, categoria, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifica se a permissão permite uma ação específica
        /// </summary>
        /// <param name="acao">Nome da ação</param>
        /// <returns>True se permite a ação, false caso contrário</returns>
        public bool PermiteAcao(string acao)
        {
            return string.Equals(Acao, acao, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(Acao, "all", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Valida as regras de domínio para a permissão
        /// </summary>
        private void ValidarDominio(
            string codigo,
            string nome,
            string descricao,
            string modulo,
            string categoria,
            string acao,
            string recurso)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new DomainException("O código da permissão é obrigatório.", nameof(Permissao));

            if (codigo.Length > 50)
                throw new DomainException("O código da permissão não pode ter mais que 50 caracteres.", nameof(Permissao));

            if (!ValidarCodigoPermissao(codigo))
                throw new DomainException("O código da permissão deve seguir o padrão MODULO_ACAO (ex: LEAD_VISUALIZAR).", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome da permissão é obrigatório.", nameof(Permissao));

            if (nome.Length > 100)
                throw new DomainException("O nome da permissão não pode ter mais que 100 caracteres.", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição da permissão é obrigatória.", nameof(Permissao));

            if (descricao.Length > 500)
                throw new DomainException("A descrição da permissão não pode ter mais que 500 caracteres.", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(modulo))
                throw new DomainException("O módulo da permissão é obrigatório.", nameof(Permissao));

            if (modulo.Length > 50)
                throw new DomainException("O módulo não pode ter mais que 50 caracteres.", nameof(Permissao));

            if (!ValidarModulo(modulo))
                throw new DomainException("Módulo inválido. Módulos válidos: LEAD, CONVERSA, VENDAS, USUARIO, RELATORIO, CONFIGURACAO.", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(categoria))
                throw new DomainException("A categoria da permissão é obrigatória.", nameof(Permissao));

            if (categoria.Length > 50)
                throw new DomainException("A categoria não pode ter mais que 50 caracteres.", nameof(Permissao));

            if (!ValidarCategoria(categoria))
                throw new DomainException("Categoria inválida. Categorias válidas: LEITURA, ESCRITA, ADMINISTRACAO.", nameof(Permissao));

            if (string.IsNullOrWhiteSpace(acao))
                throw new DomainException("A ação da permissão é obrigatória.", nameof(Permissao));

            if (acao.Length > 20)
                throw new DomainException("A ação não pode ter mais que 20 caracteres.", nameof(Permissao));

            if (!ValidarAcao(acao))
                throw new DomainException("Ação inválida. Ações válidas: read, create, update, delete, execute, all.", nameof(Permissao));

            if (!string.IsNullOrWhiteSpace(recurso) && recurso.Length > 200)
                throw new DomainException("O recurso não pode ter mais que 200 caracteres.", nameof(Permissao));
        }

        /// <summary>
        /// Valida o formato do código da permissão
        /// </summary>
        private bool ValidarCodigoPermissao(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            // Verifica se segue o padrão MODULO_ACAO
            var partes = codigo.Split('_');
            return partes.Length >= 2 && partes.All(p => !string.IsNullOrWhiteSpace(p));
        }

        /// <summary>
        /// Valida se o módulo é válido
        /// </summary>
        private bool ValidarModulo(string modulo)
        {
            var modulosValidos = new[] { "LEAD", "CONVERSA", "VENDAS", "USUARIO", "RELATORIO", "CONFIGURACAO" };
            return modulosValidos.Contains(modulo.ToUpper());
        }

        /// <summary>
        /// Valida se a categoria é válida
        /// </summary>
        private bool ValidarCategoria(string categoria)
        {
            var categoriasValidas = new[] { "LEITURA", "ESCRITA", "ADMINISTRACAO" };
            return categoriasValidas.Contains(categoria.ToUpper());
        }

        /// <summary>
        /// Valida se a ação é válida
        /// </summary>
        private bool ValidarAcao(string acao)
        {
            var acoesValidas = new[] { "read", "create", "update", "delete", "execute", "all" };
            return acoesValidas.Contains(acao.ToLower());
        }
    }
}
