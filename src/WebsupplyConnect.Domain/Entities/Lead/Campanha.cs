using System;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Lead
{
    public class Campanha : EntidadeBase
    {
        /// <summary>
        /// Nome da campanha.
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Código identificador da campanha.
        /// </summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Indica se a campanha está ativa.
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Indica se a campanha é temporária.
        /// </summary>
        public bool Temporaria { get; set; }

        /// <summary>
        /// Id da campanha para a qual esta campanha foi transferida (vinculada).
        /// </summary>
        public int? IdTransferida { get; set; }

        /// <summary>
        /// Data de início da campanha.
        /// </summary>
        public DateTime? DataInicio { get; set; }

        /// <summary>
        /// Data de fim da campanha.
        /// </summary>
        public DateTime? DataFim { get; set; }

        /// <summary>
        /// ID da empresa que a Campanha pertence
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// ID da Equipe que a Campanha pertence
        /// </summary>
        public int? EquipeId { get; set; }

        /// <summary>
        /// Data em que os leads foram transferidos para campanha definitiva
        /// </summary>
        public DateTime? DataTransferencia { get; private set; }

        /// <summary>
        /// Navegação para empresa que a Campanha pertence
        /// </summary>
        public virtual Empresa.Empresa Empresa { get; private set; }

        /// <summary>
        /// Navegação para equipe que a Campanha pertence
        /// </summary>
        public virtual Equipe.Equipe Equipe { get; private set; }

        /// <summary>
        /// Cria uma nova campanha.
        /// </summary>

        public virtual ICollection<LeadEvento> LeadEventos { get; private set; }

        public static Campanha Criar(string nome, string codigo, DateTime? dataInicio, DateTime? dataFim, int empresaId, bool temporaria, int equipeId)
        {
            if (dataInicio.HasValue && dataFim.HasValue && dataFim < dataInicio)
                throw new DomainException("A data de fim não pode ser menor que a data de início.", nameof(Campanha));
            if(equipeId <= 0)
                throw new DomainException("A equipe da campanha deve ser informada.", nameof(Campanha));

            return new Campanha
            {
                Nome = nome,
                Codigo = codigo,
                Temporaria = temporaria,
                Ativo = true,
                DataInicio = dataInicio,
                DataFim = dataFim,
                EmpresaId = empresaId,
                EquipeId = equipeId,
                DataCriacao = TimeHelper.GetBrasiliaTime(),
                DataModificacao = TimeHelper.GetBrasiliaTime()
            };
        }

        /// <summary>
        /// Edita os dados da campanha.
        /// </summary>
        public void Editar(string nome, string codigo, bool temporaria, DateTime? dataInicio, DateTime? dataFim, int empresaId, int equipeId)
        {
            if (dataInicio.HasValue && dataFim.HasValue && dataFim < dataInicio)
                throw new DomainException("A data de fim não pode ser menor que a data de início.", nameof(Campanha));

            Nome = nome;
            Codigo = codigo;
            Temporaria = temporaria;
            DataInicio = dataInicio;
            DataFim = dataFim;
            EmpresaId = empresaId;
            EquipeId = equipeId;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa a campanha.
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa a campanha.
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Exclui logicamente a campanha.
        /// </summary>
        public void Excluir()
        {
            ExcluirLogicamente();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Vincula esta campanha a outra, atualizando o IdTransferida e os leads relacionados.
        /// </summary>
        /// <param name="idCampanhaDestino">Id da campanha destino para vinculação.</param>
        /// <param name="atualizarLeads">Ação para atualizar os leads desta campanha para a campanha destino.</param>
        public void VincularCampanha(int idCampanhaDestino)
        {
            IdTransferida = idCampanhaDestino;
            AtualizarDataModificacao();
            DataTransferencia = DateTime.UtcNow;
        }
    }
}
