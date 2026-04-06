using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsupplyConnect.API.Response;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Produto;
using WebsupplyConnect.Application.DTOs.Usuario;
using WebsupplyConnect.Application.Interfaces.Produto;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.API.Controllers.Produto
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "HorarioTrabalho")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoWriterService _produtoWriterService;
        private readonly IProdutoReaderService _produtoReaderService;
        private readonly IValidator<AdicionarProdutoRequestDTO> _validator;
        private readonly IValidator<VincularEmpresaProdutoRequestDTO> _vinculoValidator;

        public ProdutoController(
        IProdutoWriterService produtoWriterService,
        IProdutoReaderService produtoReaderService,
        IValidator<AdicionarProdutoRequestDTO> validator,
        IValidator<VincularEmpresaProdutoRequestDTO> vinculoValidator)
        {
            _produtoReaderService = produtoReaderService;
            _produtoWriterService = produtoWriterService;
            _validator = validator;
            _vinculoValidator = vinculoValidator;
        }

        [HttpPost("add-produto")]
        public async Task<ActionResult<ApiResponse<object>>> AdicionarProduto([FromBody] AdicionarProdutoRequestDTO dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var erros = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro de validação", erros));
            }

            try
            {
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

                var produto = await _produtoWriterService.AdicionarProdutoAsync(dto, usuarioId);

                var responseData = new
                {
                    produto.Id,
                    produto.Nome,
                    produto.Ativo,
                    produto.ValorReferencia,
                    dto.EmpresaId
                };

                return Ok(ApiResponse<object>.SuccessResponse(responseData, "Produto criado com sucesso"));
            }
            catch (DomainException dex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao criar produto", ex.Message));
            }
        }

        [HttpPost("vincular-empresa")]
        public async Task<ActionResult<ApiResponse<object>>> VincularEmpresa([FromBody] VincularEmpresaProdutoRequestDTO dto)
        {
            var validationResult = await _vinculoValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var erros = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return BadRequest(ApiResponse<object>.ErrorResponse("Erro de validação", erros));
            }

            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

            try
            {
                await _produtoWriterService.VincularEmpresaAsync(dto, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse("Empresa vinculada ao produto com sucesso."));
            }
            catch (DomainException dex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao vincular empresa ao produto.", ex.Message));
            }
        }

        [HttpDelete("delete-produto/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> ExcluirProduto(int id)
        {
            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

            try
            {
                await _produtoWriterService.ExcluirProdutoAsync(id, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse("Produto excluído com sucesso."));
            }
            catch (DomainException dex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao excluir produto", ex.Message));
            }
        }

        [HttpDelete("remove-vinculo/{produtoId:int}/{empresaId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoverEmpresaDoProduto([FromQuery] int produtoId, [FromQuery] int empresaId)
        {
            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

            try
            {
                await _produtoWriterService.RemoverEmpresaDoProdutoAsync(produtoId, empresaId, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse("Empresa removida do produto com sucesso."));
            }
            catch (DomainException dex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao remover empresa do produto", ex.Message));
            }
        }

        [HttpPatch("alter-status/{produtoId}")]
        public async Task<ActionResult<ApiResponse<object>>> AlterarStatusProduto([FromQuery] int produtoId)
        {
            var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

            try
            {
                await _produtoWriterService.AlterarStatusProdutoAsync(produtoId, usuarioId);
                return Ok(ApiResponse<object>.SuccessResponse("Status do produto alterado com sucesso."));
            }
            catch (DomainException dex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(dex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao alterar status do produto.", ex.Message));
            }
        }

        [HttpGet("produtos")]
        public async Task<ActionResult<ApiResponse<PagedResponseDTO<ProdutoListagemDTO>>>> ListarProdutos(
            [FromQuery] ProdutoFiltroRequestDTO filtro)
        {
            try
            {
                var resultado = await _produtoReaderService.ListarProdutosPaginadoAsync(filtro);
                return Ok(ApiResponse<PagedResponseDTO<ProdutoListagemDTO>>.SuccessResponse(resultado, "Produtos listados com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro interno ao listar produtos. Tente novamente mais tarde."));
            }
        }

        [HttpGet("produto/{id}")]
        public async Task<ActionResult<ApiResponse<ProdutoDetalhadoDTO>>> ObterDetalhado([FromQuery] int id)
        {
            try
            {
                var resultado = await _produtoReaderService.ObterDetalhadoAsync(id);
                return Ok(ApiResponse<ProdutoDetalhadoDTO>.SuccessResponse(resultado, "Produto detalhado obtido com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao obter os detalhes do produto."));
            }
        }

        [HttpPut("atualizar-produto/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> AtualizarProduto(int id, [FromBody] AtualizarProdutoRequestDTO dto)
        {
            try
            {
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

                await _produtoWriterService.AtualizarInformacoesAsync(dto, usuarioId, id);

                return Ok(ApiResponse<string>.SuccessResponse("Produto atualizado com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao atualizar o produto."));
            }
        }

        [HttpPatch("alterar-valor/{produtoId:int}/{empresaId:int}")]
        public async Task<ActionResult<ApiResponse<string>>> AlterarValorProdutoEmpresa(int produtoId, int empresaId, 
            [FromBody] AlterarValorProdutoEmpresaRequestDTO dto)
        {
            try
            {
                var usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                if (!int.TryParse(usuarioIdClaim, out var usuarioId))
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não autenticado ou ID inválido."));

                await _produtoWriterService.AlterarValorProdutoEmpresaAsync(produtoId, empresaId, dto, usuarioId);

                return Ok(ApiResponse<string>.SuccessResponse("Valor personalizado alterado com sucesso."));
            }
            catch (AppException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Erro ao alterar o valor personalizado."));
            }
        }
    }
}
