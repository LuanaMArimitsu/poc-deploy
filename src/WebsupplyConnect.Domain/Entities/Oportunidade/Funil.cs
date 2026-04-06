using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Oportunidade
{
    public class Funil : EntidadeBase
    {
        /// <summary>
        /// Nome do funil de vendas
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// Descrição detalhada do funil
        /// </summary>
        public string Descricao { get; private set; }

        /// <summary>
        /// ID da empresa proprietária deste funil
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Empresa proprietária deste funil
        /// </summary>
        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Indica se este é o funil padrão da empresa
        /// </summary>
        public bool EhPadrao { get; private set; }

        /// <summary>
        /// Cor hexadecimal para representação visual do funil
        /// </summary>
        public string? Cor { get; private set; }

        /// <summary>
        /// Lista de etapas que compõem este funil
        /// </summary>
        public virtual ICollection<Etapa> Etapas { get; private set; }


        /// <summary>
        /// Funil está ativo ou inativo
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// Construtor protegido para Entity Framework
        /// </summary>
        protected Funil()
        {
            Etapas = [];
        }

        /// <summary>
        /// Construtor para criação de novo funil
        /// </summary>
        /// <param name="nome">Nome do funil</param>
        /// <param name="descricao">Descrição do funil</param>
        /// <param name="empresaId">ID da empresa proprietária</param>
        /// <param name="ehPadrao">Se é o funil padrão</param>
        /// <param name="cor">Cor hexadecimal (opcional)</param>
        public Funil(
            string nome,
            string descricao,
            int empresaId,
            bool ativo = true,
            bool ehPadrao = false,
            string? cor = null) : this()
        {
            ValidarParametros(nome, descricao, empresaId);

            Nome = nome.Trim();
            Descricao = descricao.Trim();
            EmpresaId = empresaId;
            EhPadrao = ehPadrao;
            Cor = cor?.Trim();
            DataCriacao = TimeHelper.GetBrasiliaTime();
            DataModificacao = TimeHelper.GetBrasiliaTime();
            Ativo = ativo;
        }

        /// <summary>
        /// Atualiza as informações básicas do funil
        /// </summary>
        /// <param name="nome">Novo nome</param>
        /// <param name="descricao">Nova descrição</param>
        /// <param name="cor">Nova cor</param>
        public void AtualizarInformacoes(string nome, string descricao, string? cor = null)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do funil é obrigatório");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição do funil é obrigatória");

            if (!string.IsNullOrWhiteSpace(cor) && !ValidarCorHexadecimal(cor))
                throw new DomainException("A cor deve estar no formato hexadecimal válido (ex: #FF5722)");

            Nome = nome.Trim();
            Descricao = descricao.Trim();
            Cor = cor?.Trim();

            AtualizarDataModificacao();
        }


        /// <summary>
        /// Atualiza a propriedade Ativo do etapa
        /// </summary>
        public void AtualizarAtivo(bool ativo)
        {
            Ativo = ativo;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Define este funil como padrão da empresa
        /// </summary>
        public void DefinirComoPadrao()
        {
            if (!Excluido || !Ativo)
                throw new DomainException("Não é possível definir um funil inativo como padrão");

            EhPadrao = true;

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove a definição de funil padrão
        /// </summary>
        public void RemoverComoPadrao()
        {
            EhPadrao = false;

            AtualizarDataModificacao();
        }


        /// <summary>
        /// Adiciona uma nova etapa ao funil
        /// </summary>
        /// <param name="etapa">Etapa a ser adicionada</param>
        public void AdicionarEtapa(Etapa etapa)
        {
            if (etapa == null)
                throw new DomainException("A etapa não pode ser nula");

            if (etapa.FunilId != Id)
                throw new DomainException("A etapa deve pertencer a este funil");

            if (Etapas.Any(e => e.Nome == etapa.Nome))
                throw new DomainException($"Já existe uma etapa com o nome '{etapa.Nome}' neste funil");

            if (Etapas.Any(e => e.Ordem == etapa.Ordem))
                throw new DomainException($"Já existe uma etapa com a ordem {etapa.Ordem} neste funil");

            Etapas.Add(etapa);

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Remove uma etapa do funil
        /// </summary>
        /// <param name="etapaId">ID da etapa a ser removida</param>
        public void RemoverEtapa(int etapaId)
        {
            var etapa = Etapas.FirstOrDefault(e => e.Id == etapaId);
            if (etapa == null)
                throw new DomainException("Etapa não encontrada neste funil");

            if (etapa.Oportunidades.Any())
                throw new DomainException("Esta etapa não pode ser excluída pois possui oportunidades associadas");

            Etapas.Remove(etapa);

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Reordena as etapas do funil
        /// </summary>
        /// <param name="novaOrdenacao">Dicionário com ID da etapa e nova ordem</param>
        public void ReordenarEtapas(Dictionary<int, int> novaOrdenacao)
        {
            if (novaOrdenacao == null || !novaOrdenacao.Any())
                throw new DomainException("A nova ordenação deve ser fornecida");

            foreach (var item in novaOrdenacao)
            {
                var etapa = Etapas.FirstOrDefault(e => e.Id == item.Key);
                if (etapa != null)
                {
                    etapa.AtualizarOrdem(item.Value);
                }
            }

            AtualizarDataModificacao();
        }

        /// <summary>
        /// Obtém as etapas do funil ordenadas sequencialmente
        /// </summary>
        /// <returns>Lista de etapas ordenadas</returns>
        public IEnumerable<Etapa> ObterEtapasOrdenadas()
        {
            return Etapas
                .Where(e => e.EhAtiva)
                .OrderBy(e => e.Ordem);
        }

        /// <summary>
        /// Obtém a primeira etapa do funil
        /// </summary>
        /// <returns>Primeira etapa ou null</returns>
        public Etapa? ObterEtapaInicial()
        {
            return ObterEtapasOrdenadas().FirstOrDefault();
        }

        /// <summary>
        /// Obtém a etapa de vitória (Ganha)
        /// </summary>
        /// <returns>Etapa de vitória ou null</returns>
        public Etapa? ObterEtapaVitoria()
        {
            return Etapas.FirstOrDefault(e => e.EhVitoria && !e.Excluido);
        }

        /// <summary>
        /// Obtém a etapa de perda (Perdida)
        /// </summary>
        /// <returns>Etapa de perda ou null</returns>
        public Etapa? ObterEtapaPerdida()
        {
            return Etapas.FirstOrDefault(e => e.EhPerdida && !e.Excluido);
        }

        /// <summary>
        /// Verifica se o funil está completo (possui todas as etapas necessárias)
        /// </summary>
        /// <returns>True se estiver completo</returns>
        public bool EstaCompleto()
        {
            var temEtapaInicial = ObterEtapaInicial() != null;
            var temEtapaVitoria = ObterEtapaVitoria() != null;
            var temEtapaPerdida = ObterEtapaPerdida() != null;

            return temEtapaInicial && temEtapaVitoria && temEtapaPerdida;
        }

        /// <summary>
        /// Cria o funil padrão para uma empresa
        /// </summary>
        /// <param name="empresaId">ID da empresa</param>
        /// <returns>Funil padrão configurado</returns>
        public static Funil CriarFunilPadrao(int empresaId)
        {
            var funil = new Funil(
                nome: "Funil Padrão",
                descricao: "Funil de vendas padrão do sistema",
                empresaId: empresaId,
                ehPadrao: true,
                cor: "#2563EB"
            );

            // As etapas serão criadas pelo Application Service
            return funil;
        }

        /// <summary>
        /// Valida os parâmetros do construtor
        /// </summary>
        private static void ValidarParametros(string nome, string descricao, int empresaId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("O nome do funil é obrigatório");

            if (nome.Length > 100)
                throw new DomainException("O nome do funil não pode ter mais de 100 caracteres");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new DomainException("A descrição do funil é obrigatória");

            if (descricao.Length > 500)
                throw new DomainException("A descrição do funil não pode ter mais de 500 caracteres");

            if (empresaId <= 0)
                throw new DomainException("ID da empresa é obrigatório");
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

            if (!cor.StartsWith("#"))
                return false;

            if (cor.Length != 7 && cor.Length != 4)
                return false;

            var hexChars = cor.Substring(1);
            return hexChars.All(c => "0123456789ABCDEFabcdef".Contains(c));
        }
    }
}
