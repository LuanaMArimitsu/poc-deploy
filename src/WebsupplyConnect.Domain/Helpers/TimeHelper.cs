using System.Runtime.InteropServices;

namespace WebsupplyConnect.Domain.Helpers
{
    public class TimeHelper
    {
        public static DateTime GetBrasiliaTime()
        {
            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "E. South America Standard Time"
                : "America/Sao_Paulo";

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }

        public static DateTime GetTimestampParaHorarioBrasilia(long timestamp)
        {
            var utcDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

            var timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "E. South America Standard Time"
                : "America/Sao_Paulo";

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }

        /// <summary>
        /// Verifica se uma data está dentro do horário comercial
        /// </summary>
        /// <param name="date">Data para verificação</param>
        /// <returns>True se estiver dentro do horário comercial (seg-sex, 8h-18h), false caso contrário</returns>
        public static bool IsBusinessHours(DateTime date)
        {
            // Verificar se é final de semana (sábado = 6, domingo = 0)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Verificar se está dentro do horário comercial (8h-18h)
            if (date.Hour < 8 || date.Hour >= 18)
                return false;

            return true;
        }

        /// <summary>
        /// Calcula a próxima data útil a partir de uma data
        /// </summary>
        /// <param name="date">Data base</param>
        /// <param name="businessDays">Número de dias úteis a adicionar</param>
        /// <returns>Data útil calculada</returns>
        public static DateTime GetNextBusinessDay(DateTime date, int businessDays = 1)
        {
            var result = date;

            while (businessDays > 0)
            {
                result = result.AddDays(1);

                // Pular finais de semana
                if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
                {
                    // TODO: Adicionar lógica para verificar feriados se necessário
                    businessDays--;
                }
            }

            return result;
        }

        /// <summary>
        /// Obtém o início do dia (00:00:00) para uma data específica
        /// </summary>
        /// <param name="date">Data de referência</param>
        /// <returns>Data com horário ajustado para início do dia</returns>
        public static DateTime GetStartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        /// <summary>
        /// Obtém o fim do dia (23:59:59) para uma data específica
        /// </summary>
        /// <param name="date">Data de referência</param>
        /// <returns>Data com horário ajustado para fim do dia</returns>
        public static DateTime GetEndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        /// <summary>
        /// Obtém o início do mês para uma data específica
        /// </summary>
        /// <param name="date">Data de referência</param>
        /// <returns>Data com dia ajustado para primeiro dia do mês</returns>
        public static DateTime GetStartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
        }

        /// <summary>
        /// Obtém o fim do mês para uma data específica
        /// </summary>
        /// <param name="date">Data de referência</param>
        /// <returns>Data com dia ajustado para último dia do mês</returns>
        public static DateTime GetEndOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59);
        }
    }
}
