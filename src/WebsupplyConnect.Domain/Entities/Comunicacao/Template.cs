using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Comunicacao
{
    /// <summary>
    /// Representa um template para mensagens
    /// </summary>
    public class Template : EntidadeBase
    {
        /// <summary>
        /// Nome do template que é o ID do Template na Meta.
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Conteúdo do template
        /// </summary>
        public string Conteudo { get; private set; }

        /// <summary>
        /// Descrição do template
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// ID da categoria do template
        /// </summary>
        public int CategoriaId { get; private set; }

        /// <summary>
        /// Quantidade de parâmetros no template
        /// </summary>
        public int ParametrosContagem { get; private set; }

        /// <summary>
        /// Exemplo de uso do template
        /// </summary>
        public string Exemplo { get; private set; }

        /// <summary>
        /// ID da empresa à qual o template pertence
        /// </summary>
        public int CanalId { get; private set; }

        // Propriedades de navegação
        public virtual Canal Canal { get; private set; }
        public virtual TemplateCategoria Categoria { get; private set; }
        public virtual ICollection<Mensagem> Mensagens { get; private set; }

        // Construtor protegido para EF
        protected Template() : base()
        {
            Mensagens = new HashSet<Mensagem>();
        }

        /// <summary>
        /// Cria um novo template
        /// </summary>
        public Template(
            string nome,
            string conteudo,
            int categoriaId,
            int canalId,
            string descricao,
            string exemplo,
            int parametrosContagem = 0
            ) : this()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome do template não pode ser vazio", nameof(nome));

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new DomainException("Conteúdo do template não pode ser vazio", nameof(conteudo));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("Descrição do template não pode ser vazio", nameof(descricao));

            if (string.IsNullOrWhiteSpace(exemplo))
                throw new DomainException("Exemplo do template não pode ser vazio", nameof(exemplo));

            if (categoriaId <= 0)
                throw new DomainException("ID da categoria deve ser maior que zero", nameof(categoriaId));

            if (canalId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(canalId));

            Nome = nome;
            Conteudo = conteudo;
            Descricao = descricao;
            CategoriaId = categoriaId;
            ParametrosContagem = parametrosContagem;
            Exemplo = exemplo;
            CanalId = canalId;
        }

        /// <summary>
        /// Atualiza as informações do template
        /// </summary>
        public void Atualizar(
            string nome,
            string conteudo,
            string descricao,
            int categoriaId,
            int parametrosContagem,
            string exemplo)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome do template não pode ser vazio", nameof(nome));

            if (string.IsNullOrWhiteSpace(conteudo))
                throw new DomainException("Conteúdo do template não pode ser vazio", nameof(conteudo));

            if (string.IsNullOrWhiteSpace(exemplo))
                throw new DomainException("Exemplo do template não pode ser vazio", nameof(exemplo));


            if (categoriaId <= 0)
                throw new DomainException("ID da categoria deve ser maior que zero", nameof(categoriaId));

            Nome = nome;
            Conteudo = conteudo;
            Descricao = descricao ?? string.Empty;
            CategoriaId = categoriaId;
            Exemplo = exemplo;
            ParametrosContagem = parametrosContagem;
            Exemplo = exemplo;

            AtualizarDataModificacao();
        }
    }
}