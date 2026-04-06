using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Distribuicao;
using WebsupplyConnect.Application.Interfaces.Usuario;
using WebsupplyConnect.Domain.Interfaces.Comum;
using WebsupplyConnect.Domain.Interfaces.Distribuicao;

namespace WebsupplyConnect.Application.Services.Distribuicao
{
    /// <summary>
    /// Serviço para gerenciamento de horários de distribuição
    /// </summary>
    public class HorariosDistribuicaoService : IHorariosDistribuicaoService
    {
        private readonly IConfiguracaoDistribuicaoRepository _configuracaoRepository;
        private readonly IUsuarioReaderService _usuarioReaderService;
        private readonly IFeriadoRepository _feriadoRepository;
        private readonly ILogger<HorariosDistribuicaoService> _logger;

        public HorariosDistribuicaoService(
            IConfiguracaoDistribuicaoRepository configuracaoRepository,
            IUsuarioReaderService usuarioReaderService,
            IFeriadoRepository feriadoRepository,
            ILogger<HorariosDistribuicaoService> logger)
        {
            _configuracaoRepository = configuracaoRepository ?? throw new ArgumentNullException(nameof(configuracaoRepository));
            _usuarioReaderService = usuarioReaderService ?? throw new ArgumentNullException(nameof(usuarioReaderService));
            _feriadoRepository = feriadoRepository ?? throw new ArgumentNullException(nameof(feriadoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Verifica se um vendedor está disponível para receber leads baseado nos horários configurados
        /// </summary>
        public async Task<bool> VerificarDisponibilidadeVendedorAsync(int configuracaoId, int vendedorId, DateTime? dataHora = null)
        {
            try
            {
                var dataVerificacao = dataHora ?? DateTime.Now;
                
                // Obter configuração
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    _logger.LogWarning("Configuração de distribuição {ConfiguracaoId} não encontrada", configuracaoId);
                    return false;
                }

                // Verificar se a distribuição está ativa
                if (!await VerificarDistribuicaoAtivaAsync(configuracaoId, dataVerificacao))
                {
                    return false;
                }

                // Verificar horários do vendedor
                var horariosVendedor = await _usuarioReaderService.ObterHorariosUsuarioAsync(vendedorId);
                if (horariosVendedor == null || !horariosVendedor.Any())
                {
                    _logger.LogDebug("Vendedor {VendedorId} não possui horários configurados", vendedorId);
                    return false;
                }

                var diaSemana = (int)dataVerificacao.DayOfWeek;
                var horarioAtual = dataVerificacao.TimeOfDay;
                
                var horarioDia = horariosVendedor.FirstOrDefault(h => h.DiaSemanaId == diaSemana);
                if (horarioDia == null || horarioDia.SemExpediente)
                {
                    _logger.LogDebug("Vendedor {VendedorId} não trabalha no dia {DiaSemana}", vendedorId, diaSemana);
                    return false;
                }

                // Verificar se está dentro do horário de trabalho
                if (!horarioDia.HorarioInicio.HasValue || !horarioDia.HorarioFim.HasValue)
                {
                    _logger.LogWarning("Horários inválidos para vendedor {VendedorId} no dia {DiaSemana}", vendedorId, diaSemana);
                    return false;
                }

                var inicio = horarioDia.HorarioInicio.Value;
                var fim = horarioDia.HorarioFim.Value;

                // Considerar intervalo de almoço (assumindo 1 hora de almoço se não especificado)
                var intervaloAlmoco = 60; // minutos
                var meioDia = new TimeSpan(12, 0, 0);
                var inicioAlmoco = meioDia;
                var fimAlmoco = meioDia.Add(TimeSpan.FromMinutes(intervaloAlmoco));
                
                // Verificar se está no horário de almoço
                if (horarioAtual >= inicioAlmoco && horarioAtual <= fimAlmoco)
                {
                    _logger.LogDebug("Vendedor {VendedorId} está no horário de almoço", vendedorId);
                    return false;
                }

                var disponivel = horarioAtual >= inicio && horarioAtual <= fim;
                
                _logger.LogDebug("Vendedor {VendedorId} disponibilidade: {Disponivel} (horário atual: {HorarioAtual}, início: {Inicio}, fim: {Fim})", 
                    vendedorId, disponivel, horarioAtual, inicio, fim);
                
                return disponivel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar disponibilidade do vendedor {VendedorId} para configuração {ConfiguracaoId}", 
                    vendedorId, configuracaoId);
                return false;
            }
        }

        /// <summary>
        /// Verifica se a distribuição está ativa baseado nos horários da configuração
        /// </summary>
        public async Task<bool> VerificarDistribuicaoAtivaAsync(int configuracaoId, DateTime? dataHora = null)
        {
            try
            {
                var dataVerificacao = dataHora ?? DateTime.Now;
                
                // Obter configuração
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null || !configuracao.Ativo)
                {
                    return false;
                }

                // Verificar vigência
                if (configuracao.DataInicioVigencia.HasValue && dataVerificacao < configuracao.DataInicioVigencia.Value)
                {
                    return false;
                }
                
                if (configuracao.DataFimVigencia.HasValue && dataVerificacao > configuracao.DataFimVigencia.Value)
                {
                    return false;
                }

                // Se não considerar horário de trabalho, está sempre ativa
                if (!configuracao.ConsiderarHorarioTrabalho)
                {
                    return true;
                }

                // Verificar feriados se configurado
                if (configuracao.ConsiderarFeriados)
                {
                    var ehFeriado = await _feriadoRepository.VerificarDataFeriadoAsync(dataVerificacao.Date, configuracao.EmpresaId);
                    if (ehFeriado)
                    {
                        _logger.LogDebug("Distribuição inativa devido ao feriado na data {Data}", dataVerificacao.Date);
                        return false;
                    }
                }

                // Verificar fim de semana se configurado
                if (configuracao.ConsiderarFeriados)
                {
                    var diaSemana = dataVerificacao.DayOfWeek;
                    if (diaSemana == DayOfWeek.Saturday || diaSemana == DayOfWeek.Sunday)
                    {
                        _logger.LogDebug("Distribuição inativa devido ao fim de semana");
                        return false;
                    }
                }

                // Verificar horários específicos da configuração
                if (!string.IsNullOrEmpty(configuracao.ParametrosGerais))
                {
                    try
                    {
                        var parametros = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(
                            configuracao.ParametrosGerais);
                        
                        if (parametros != null)
                        {
                            // Verificar horário de expediente geral
                            if (parametros.TryGetValue("HorarioInicioExpediente", out var inicioExpediente) &&
                                parametros.TryGetValue("HorarioFimExpediente", out var fimExpediente))
                            {
                                if (TimeSpan.TryParse(inicioExpediente?.ToString(), out var inicio) &&
                                    TimeSpan.TryParse(fimExpediente?.ToString(), out var fim))
                                {
                                    var horarioAtual = dataVerificacao.TimeOfDay;
                                    if (horarioAtual < inicio || horarioAtual > fim)
                                    {
                                        _logger.LogDebug("Distribuição inativa fora do horário de expediente");
                                        return false;
                                    }
                                }
                            }

                            // Verificar horários específicos por dia
                            if (parametros.TryGetValue("HorariosPorDia", out var horariosPorDia))
                            {
                                if (horariosPorDia is System.Text.Json.JsonElement horariosElement)
                                {
                                    var horariosDia = System.Text.Json.JsonSerializer.Deserialize<List<HorarioDiaSemanaDTO>>(
                                        horariosElement.GetRawText());
                                    
                                    if (horariosDia != null)
                                    {
                                        var diaSemana = (int)dataVerificacao.DayOfWeek;
                                        var horarioDia = horariosDia.FirstOrDefault(h => h.DiaSemanaId == diaSemana);
                                        
                                        if (horarioDia != null && horarioDia.TrabalhaNesteDia)
                                        {
                                            if (TimeSpan.TryParse(horarioDia.HorarioInicio, out var inicioDia) &&
                                                TimeSpan.TryParse(horarioDia.HorarioFim, out var fimDia))
                                            {
                                                var horarioAtual = dataVerificacao.TimeOfDay;
                                                if (horarioAtual < inicioDia || horarioAtual > fimDia)
                                                {
                                                    _logger.LogDebug("Distribuição inativa fora do horário específico do dia");
                                                    return false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _logger.LogDebug("Distribuição inativa - não trabalha neste dia da semana");
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao processar parâmetros de horário da configuração {ConfiguracaoId}", configuracaoId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se distribuição está ativa para configuração {ConfiguracaoId}", configuracaoId);
                return false;
            }
        }

        /// <summary>
        /// Obtém os próximos horários de disponibilidade para uma configuração
        /// </summary>
        public async Task<List<HorarioDisponibilidadeDTO>> ObterProximosHorariosDisponibilidadeAsync(int configuracaoId, int dias = 7)
        {
            try
            {
                var horarios = new List<HorarioDisponibilidadeDTO>();
                var dataInicio = DateTime.Today;
                
                // Obter configuração
                var configuracao = await _configuracaoRepository.GetByIdAsync(configuracaoId);
                if (configuracao == null)
                {
                    return horarios;
                }

                // Obter feriados para o período
                var feriados = new List<DateTime>();
                if (configuracao.ConsiderarFeriados)
                {
                    var dataFim = dataInicio.AddDays(dias);
                    // Verificar cada dia do período para feriados
                    for (int i = 0; i < dias; i++)
                    {
                        var data = dataInicio.AddDays(i);
                        var ehFeriado = await _feriadoRepository.VerificarDataFeriadoAsync(data, configuracao.EmpresaId);
                        if (ehFeriado)
                        {
                            feriados.Add(data);
                        }
                    }
                }

                // Processar cada dia
                for (int i = 0; i < dias; i++)
                {
                    var data = dataInicio.AddDays(i);
                    var diaSemana = (int)data.DayOfWeek;
                    var nomeDia = data.DayOfWeek.ToString();
                    
                    var horario = new HorarioDisponibilidadeDTO
                    {
                        Data = data,
                        DiaSemanaId = diaSemana,
                        NomeDia = nomeDia,
                        DistribuicaoAtiva = true
                    };

                    // Verificar se é feriado
                    var ehFeriado = feriados.Any(f => f.Date == data.Date);
                    if (ehFeriado)
                    {
                        horario.EhFeriado = true;
                        horario.DistribuicaoAtiva = false;
                        horario.MotivoIndisponibilidade = "Feriado";
                    }
                    // Verificar fim de semana
                    else if (configuracao.ConsiderarFeriados && (diaSemana == 0 || diaSemana == 6))
                    {
                        horario.DistribuicaoAtiva = false;
                        horario.MotivoIndisponibilidade = "Fim de semana";
                    }
                    // Verificar horários específicos
                    else if (!string.IsNullOrEmpty(configuracao.ParametrosGerais))
                    {
                        try
                        {
                            var parametros = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(
                                configuracao.ParametrosGerais);
                            
                            if (parametros != null && parametros.TryGetValue("HorariosPorDia", out var horariosPorDia))
                            {
                                if (horariosPorDia is System.Text.Json.JsonElement horariosElement)
                                {
                                    var horariosDia = System.Text.Json.JsonSerializer.Deserialize<List<HorarioDiaSemanaDTO>>(
                                        horariosElement.GetRawText());
                                    
                                    var horarioDia = horariosDia?.FirstOrDefault(h => h.DiaSemanaId == diaSemana);
                                    if (horarioDia != null)
                                    {
                                        horario.DistribuicaoAtiva = horarioDia.TrabalhaNesteDia;
                                        horario.HorarioInicio = horarioDia.HorarioInicio;
                                        horario.HorarioFim = horarioDia.HorarioFim;
                                        
                                        if (!horarioDia.TrabalhaNesteDia)
                                        {
                                            horario.MotivoIndisponibilidade = "Não trabalha neste dia";
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Erro ao processar horários específicos para configuração {ConfiguracaoId}", configuracaoId);
                        }
                    }

                    horarios.Add(horario);
                }

                return horarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximos horários de disponibilidade para configuração {ConfiguracaoId}", configuracaoId);
                return new List<HorarioDisponibilidadeDTO>();
            }
        }
    }
}
