using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Entities.Usuario;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Equipe
{
    /// <summary>Representa uma equipe de trabalho.</summary>
    public class Equipe : EntidadeBase
    {
        // Propriedades
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public int TipoEquipeId { get; private set; }
        public int EmpresaId { get; private set; }
        public bool Ativa { get; private set; }
        public int? ResponsavelMembroId { get; private set; }
        public bool EhPadrao { get; private set; }

        // Navegações
        public virtual TipoEquipe? TipoEquipe { get; private set; }
        public virtual Empresa.Empresa? Empresa { get; private set; }
        public virtual MembroEquipe? ResponsavelMembro { get; private set; }
        public virtual ICollection<MembroEquipe> Membros { get; private set; }

        //Notificações
        public bool NotificarAtribuicaoAoDestinatario { get; private set; }
        public bool NotificarAtribuicaoAosLideres { get; private set; }
        public bool NotificarSemAtendimentoLideres { get; private set; }
        public TimeSpan? TempoMaxSemAtendimento { get; private set; }
        public TimeSpan? TempoMaxDuranteAtendimento { get; private set; }
        public ICollection<Campanha> Campanhas { get; private set; }
        public ICollection<Conversa> Conversas { get; private set; }
        public virtual ICollection<UsuarioEmpresa> UsuarioEmpresas { get; private set; }
        // EF
        protected Equipe()
        {
            Membros = new HashSet<MembroEquipe>();
        }

        public Equipe(
            string nome,
            int tipoEquipeId,
            int empresaId,
            int? responsavelMembroId,
            string? descricao = null,
            bool ativa = true,
            bool notificarDestinatario = false,
            bool notificarLideres = false,
            TimeSpan? tempoMaxSemAtendimento = null,
            TimeSpan? tempoMaxDuranteAtendimento = null)
        {
            ValidarDominio(nome, tipoEquipeId, empresaId);
            ValidarTempo(tempoMaxSemAtendimento, nameof(tempoMaxSemAtendimento));
            ValidarTempo(tempoMaxDuranteAtendimento, nameof(tempoMaxDuranteAtendimento));

            Nome = nome;
            Descricao = descricao;
            TipoEquipeId = tipoEquipeId;
            EmpresaId = empresaId;
            ResponsavelMembroId = responsavelMembroId;
            Ativa = ativa;
            NotificarAtribuicaoAoDestinatario = notificarDestinatario;
            NotificarAtribuicaoAosLideres = notificarLideres;
            TempoMaxSemAtendimento = tempoMaxSemAtendimento;
            TempoMaxDuranteAtendimento = tempoMaxDuranteAtendimento;
            NotificarSemAtendimentoLideres = tempoMaxSemAtendimento.HasValue;

            Membros = new HashSet<MembroEquipe>();
        }

        // Regras de negócio
        public void AtualizarInformacoes(string nome, string? descricao)
        {
            ValidarNome(nome);

            Nome = nome;
            Descricao = descricao;

            AtualizarDataModificacao();
        }

        public void DefinirResponsavel(int responsavelMembroId)
        {
            if (responsavelMembroId <= 0)
                throw new DomainException("O membro responsável deve ser informado.", nameof(Equipe));

            ResponsavelMembroId = responsavelMembroId;
            AtualizarDataModificacao();
        }

        public void Ativar()
        {
            Ativa = true;
            AtualizarDataModificacao();
        }

        public void Desativar()
        {
            Ativa = false;
            AtualizarDataModificacao();
        }

        public void DefinirComoPadrao()
        {
            EhPadrao = true;
            AtualizarDataModificacao();
        }

        public void RemoverPadrao()
        {
            EhPadrao = false;
            AtualizarDataModificacao();
        }

        public void AtualizarTempoSemAtendimento(TimeSpan tempo)
        {
            TempoMaxSemAtendimento = tempo;
            AtualizarDataModificacao();
        }

        public void AtualizarNotificacoes(
            bool notificarDestinatario,
            bool notificarLideres,
            bool notificarSemAtendimentoLideres,
            TimeSpan? tempoMaxSemAtendimento,
            TimeSpan? tempoMaxDuranteAtendimento)
        {
            NotificarAtribuicaoAoDestinatario = notificarDestinatario;
            NotificarAtribuicaoAosLideres = notificarLideres;

            if (notificarSemAtendimentoLideres)
            {
                if (!tempoMaxSemAtendimento.HasValue || !tempoMaxDuranteAtendimento.HasValue)
                    throw new DomainException("Defina o tempo máximo sem atendimento quando a notificação por SLA estiver ativa.", nameof(Equipe));

                ValidarTempo(tempoMaxSemAtendimento, nameof(tempoMaxSemAtendimento));
                ValidarTempo(tempoMaxDuranteAtendimento, nameof(tempoMaxDuranteAtendimento));

                NotificarAtribuicaoAoDestinatario = notificarDestinatario;
                NotificarAtribuicaoAosLideres = notificarLideres;
                NotificarSemAtendimentoLideres = true;
            }
            else
            {
                NotificarSemAtendimentoLideres = false;
                TempoMaxSemAtendimento = null;
            }

            AtualizarDataModificacao();
        }

        // Validações internas
        private static void ValidarDominio(string nome, int tipoEquipeId, int empresaId)
        {
            ValidarNome(nome);

            if (tipoEquipeId <= 0)
                throw new DomainException("O tipo da equipe deve ser informado.", nameof(Equipe));

            if (empresaId <= 0)
                throw new DomainException("A empresa deve ser informada.", nameof(Equipe));
        }

        private static void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome da equipe é obrigatório.", nameof(Equipe));

            if (nome.Length > 100)
                throw new DomainException("O nome da equipe não pode ter mais que 100 caracteres.", nameof(Equipe));
        }

        private static void ValidarTempo(TimeSpan? tempo, string nomeParametro)
        {
            if (tempo.HasValue && tempo.Value <= TimeSpan.Zero)
                throw new DomainException(
                    $"O parâmetro {nomeParametro} deve ser maior que zero.",
                    nameof(Equipe));
        }
    }
}
