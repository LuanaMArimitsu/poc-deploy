using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq.Expressions;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.Interfaces.Empresa;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Helpers;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadExportService : ILeadExportService
    {
        private readonly ILeadRepository _leadRepository;
        private readonly IMailSenderService _mailSenderService;
        private readonly IEmpresaReaderService _empresaReaderService;
        private readonly ILogger<LeadExportService> _logger;

        public LeadExportService(ILeadRepository leadRepository, IMailSenderService mailSenderService, IEmpresaReaderService empresaReaderService, ILogger<LeadExportService> logger)
        {
            _leadRepository = leadRepository;
            _mailSenderService = mailSenderService;
            _empresaReaderService = empresaReaderService;
            _logger = logger;
        }

        /// <summary>
        /// Gera o arquivo Excel com os leads e retorna os bytes.
        /// </summary>
        private async Task<byte[]> GerarExcelLeadsAsync(
            int empresaId,
            int? equipeId = null,
            int? usuarioId = null,
            int? statusId = null,
            DateTime? de = null,
            DateTime? ate = null)
        {

            var leads = await _leadRepository.GetListLeadExportAsync(empresaId, equipeId, usuarioId, statusId, de, ate);

            if (leads == null || !leads.Any())
                throw new AppException("Nenhum lead encontrado para os filtros informados.");

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Leads");

            var header = sheet.CreateRow(0);
            var headers = new[]
            {
                "Nome", "E-mail", "WhatsApp", "Status", "Responsável", "Equipe", "Empresa", "Origem", "Data Criação"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = header.CreateCell(i);
                cell.SetCellValue(headers[i]);
            }

            var headerStyle = workbook.CreateCellStyle();
            var font = workbook.CreateFont();
            font.IsBold = true;
            headerStyle.SetFont(font);
            foreach (var cell in header.Cells)
                cell.CellStyle = headerStyle;

            int rowIndex = 1;
            foreach (var lead in leads)
            {
                var row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(lead.Nome ?? "");
                row.CreateCell(1).SetCellValue(lead.Email ?? "");
                row.CreateCell(2).SetCellValue(lead.WhatsappNumero ?? "");
                row.CreateCell(3).SetCellValue(lead.LeadStatus?.Descricao ?? "");
                row.CreateCell(4).SetCellValue(lead.Responsavel?.Usuario?.Nome ?? "");
                row.CreateCell(5).SetCellValue(lead.Equipe?.Nome ?? "");
                row.CreateCell(6).SetCellValue(lead.Empresa?.Nome ?? "");
                row.CreateCell(7).SetCellValue(lead.Origem?.Descricao ?? "");
                row.CreateCell(8).SetCellValue(lead.DataCriacao.ToString("dd/MM/yyyy HH:mm"));
            }

            for (int i = 0; i < headers.Length; i++)
                sheet.AutoSizeColumn(i);

            using var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream.ToArray();
        }

        /// <summary>
        /// Gera o Excel e envia por e-mail para o solicitante.
        /// </summary>
        public async Task ExportarLeadsEEnviarPorEmailAsync(
            int empresaId,
            string destinatarioEmail,
            string destinatarioNome,
            int? equipeId = null,
            int? usuarioId = null,
            int? statusId = null,
            DateTime? de = null,
            DateTime? ate = null)
        {
            try 
            { 
                var empresa = await _empresaReaderService.ObterPorId(empresaId);
                if (empresa == null)
                    throw new AppException($"Empresa com ID {empresaId} não encontrada.");

                var arquivoExcel = await GerarExcelLeadsAsync(empresaId, equipeId, usuarioId, statusId, de, ate);
                if (arquivoExcel == null || arquivoExcel.Length == 0)
                    throw new AppException("Erro ao gerar o arquivo Excel. Nenhum dado encontrado.");

                var nomeArquivo = $"Leads_{empresa.Nome}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                var assunto = $"Relatório de Leads - {empresa.Nome}";
                var mensagem = $"Olá, {destinatarioNome}!\n\nSegue em anexo o relatório de leads da empresa {empresa.Nome}.\n\nGerado em: {TimeHelper.GetBrasiliaTime():dd/MM/yyyy HH:mm}.";

                await _mailSenderService.EnviarAsync(
                    destinatarioEmail,
                    destinatarioNome,
                    assunto,
                    mensagem,
                    mensagem,
                    arquivoExcel,
                    nomeArquivo);

                _logger.LogInformation("Relatório de leads enviado por e-mail para {Email}", destinatarioEmail);
            }
            catch (AppException ex)
            {
                _logger.LogWarning("Falha na exportação de leads: {Mensagem}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao exportar leads para empresa {EmpresaId}", empresaId);
                throw;
            }
        }
    }
}
