using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// Extensões para mapeamento de AtribuicaoLead
    /// </summary>
    public static class AtribuicaoLeadExtensions
    {
        /// <summary>
        /// Mapeia uma entidade AtribuicaoLead para AtribuicaoLeadResponseDTO
        /// </summary>
        /// <param name="atribuicao">Entidade AtribuicaoLead</param>
        /// <returns>DTO de resposta</returns>
        public static AtribuicaoLeadResponseDTO ToResponseDTO(this AtribuicaoLead atribuicao)
        {
            if (atribuicao == null)
                return null!;

            return new AtribuicaoLeadResponseDTO
            {
                Id = atribuicao.Id,
                LeadId = atribuicao.LeadId,
                UsuarioAtribuidoId = atribuicao.MembroAtribuidoId,
                NomeVendedor = atribuicao.MembroAtribuido.Usuario?.Nome ?? string.Empty,
                EmailVendedor = atribuicao.MembroAtribuido.Usuario?.Email,
                TipoAtribuicaoId = atribuicao.TipoAtribuicaoId,
                NomeTipoAtribuicao = atribuicao.TipoAtribuicao?.Nome ?? "Atribuição Automática",
                DataAtribuicao = atribuicao.DataAtribuicao,
                MotivoAtribuicao = atribuicao.MotivoAtribuicao,
                AtribuicaoAutomatica = atribuicao.AtribuicaoAutomatica,
                ConfiguracaoDistribuicaoId = atribuicao.ConfiguracaoDistribuicaoId,
                NomeConfiguracaoDistribuicao = atribuicao.ConfiguracaoDistribuicao?.Nome,
                RegraDistribuicaoId = atribuicao.RegraDistribuicaoId,
                NomeRegraDistribuicao = atribuicao.RegraDistribuicao?.Nome,
                ScoreVendedor = atribuicao.ScoreVendedor,
                UsuarioAtribuiuId = atribuicao.MembroAtribuiuId,
                NomeUsuarioAtribuiu = atribuicao.MembroAtribuido.Usuario?.Nome,
                LeadStatusHistoricoId = atribuicao.LeadStatusHistoricoId,
                ParametrosAplicados = atribuicao.ParametrosAplicados,
                VendedoresElegiveis = atribuicao.VendedoresElegiveis,
                ScoresCalculados = atribuicao.ScoresCalculados,
                FallbackHorarioAplicado = atribuicao.FallbackHorarioAplicado,
                DetalhesFallbackHorario = atribuicao.DetalhesFallbackHorario,
                DataFallbackHorario = atribuicao.DataFallbackHorario,
                DataCriacao = atribuicao.DataCriacao,
                DataModificacao = atribuicao.DataModificacao
            };
        }

        /// <summary>
        /// Mapeia uma lista de entidades AtribuicaoLead para lista de AtribuicaoLeadResponseDTO
        /// </summary>
        /// <param name="atribuicoes">Lista de entidades AtribuicaoLead</param>
        /// <returns>Lista de DTOs de resposta</returns>
        public static List<AtribuicaoLeadResponseDTO> ToResponseDTOList(this IEnumerable<AtribuicaoLead> atribuicoes)
        {
            if (atribuicoes == null)
                return new List<AtribuicaoLeadResponseDTO>();

            return atribuicoes.Select(a => a.ToResponseDTO()).ToList();
        }
    }
}
