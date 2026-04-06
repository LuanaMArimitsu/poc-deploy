using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// Extensões para mapeamento de FilaDistribuicao
    /// </summary>
    //public static class FilaDistribuicaoExtensions
    //{
    //    /// <summary>
    //    /// Mapeia uma entidade FilaDistribuicao para FilaDistribuicaoResponseDTO
    //    /// </summary>
    //    /// <param name="fila">Entidade FilaDistribuicao</param>
    //    /// <returns>DTO de resposta</returns>
    //    public static FilaDistribuicaoResponseDTO ToResponseDTO(this FilaDistribuicao fila)
    //    {
    //        if (fila == null)
    //            return null!;

    //        return new FilaDistribuicaoResponseDTO
    //        {
    //            Id = fila.Id,
    //            MembroEquipeId = fila.MembroEquipeId,
    //            EmpresaId = fila.EmpresaId,
    //            PosicaoFila = fila.PosicaoFila,
    //            DataUltimoLeadRecebido = fila.DataUltimoLeadRecebido,
    //            StatusFilaDistribuicaoId = fila.StatusFilaDistribuicaoId,
    //            DataEntradaFila = fila.DataEntradaFila,
    //            PesoAtual = fila.PesoAtual,
    //            QuantidadeLeadsRecebidos = fila.QuantidadeLeadsRecebidos,
    //            DataProximaElegibilidade = fila.DataProximaElegibilidade,
    //            MotivoStatusAtual = fila.MotivoStatusAtual ?? string.Empty,
    //            Usuario = fila.Usuario != null ? new UsuarioFilaDTO
    //            {
    //                Id = fila.Usuario.Id,
    //                Nome = fila.Usuario.Nome ?? string.Empty,
    //                Email = fila.Usuario.Email,
    //                Cargo = fila.Usuario.Cargo,
    //                Departamento = fila.Usuario.Departamento,
    //                Ativo = fila.Usuario.Ativo,
    //                UsuarioSuperiorId = fila.Usuario.UsuarioSuperiorId,
    //                UsuarioSuperiorNome = fila.Usuario.UsuarioSuperior?.Nome,
    //                ObjectId = fila.Usuario.ObjectId,
    //                Upn = fila.Usuario.Upn,
    //                DisplayName = fila.Usuario.DisplayName,
    //                IsExternal = fila.Usuario.IsExternal
    //            } : null,
    //            DataCriacao = fila.DataCriacao,
    //            DataModificacao = fila.DataModificacao,
    //            Excluido = fila.Excluido,
    //            FallbackHorarioAplicado = false, // Será definido pelo serviço
    //            DetalhesFallbackHorario = null, // Será definido pelo serviço
    //            DataFallbackHorario = null // Será definido pelo serviço
    //        };
    //    }

    //    /// <summary>
    //    /// Mapeia uma entidade FilaDistribuicao para FilaDistribuicaoResponseDTO com informações de fallback
    //    /// </summary>
    //    /// <param name="fila">Entidade FilaDistribuicao</param>
    //    /// <param name="fallbackAplicado">Indica se o fallback foi aplicado</param>
    //    /// <param name="detalhesFallback">Detalhes do fallback</param>
    //    /// <returns>DTO de resposta</returns>
    //    public static FilaDistribuicaoResponseDTO ToResponseDTO(this FilaDistribuicao fila, bool fallbackAplicado, string? detalhesFallback = null)
    //    {
    //        if (fila == null)
    //            return null!;

    //        var dto = fila.ToResponseDTO();
    //        dto.FallbackHorarioAplicado = fallbackAplicado;
    //        dto.DetalhesFallbackHorario = detalhesFallback;
    //        dto.DataFallbackHorario = fallbackAplicado ? DateTime.UtcNow : null;
            
    //        return dto;
    //    }
    //}
}
