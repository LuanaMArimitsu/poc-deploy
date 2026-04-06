using System.Globalization;
using WebsupplyConnect.Domain.Entities.Base;

namespace WebsupplyConnect.Domain.Entities.OLAP.Dimensoes;

public class DimensaoTempo : EntidadeBase
{
    public int Ano { get; private set; }
    public int Mes { get; private set; }
    public int Dia { get; private set; }
    public int Hora { get; private set; }
    public int DiaSemana { get; private set; }        // 0-6 (Domingo-Sábado)
    public int Trimestre { get; private set; }       // 1-4
    public int Semana { get; private set; }           // 1-53
    public DateTime DataCompleta { get; private set; }

    protected DimensaoTempo() { } // EF Core

    public DimensaoTempo(DateTime data) : base()
    {
        Ano = data.Year;
        Mes = data.Month;
        Dia = data.Day;
        Hora = data.Hour;
        DiaSemana = (int)data.DayOfWeek;
        Trimestre = (data.Month - 1) / 3 + 1;
        Semana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(data,
            CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        DataCompleta = new DateTime(data.Year, data.Month, data.Day, data.Hour, 0, 0);
    }

    public void Atualizar(DateTime data)
    {
        Ano = data.Year;
        Mes = data.Month;
        Dia = data.Day;
        Hora = data.Hour;
        DiaSemana = (int)data.DayOfWeek;
        Trimestre = (data.Month - 1) / 3 + 1;
        Semana = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(data,
            CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        DataCompleta = new DateTime(data.Year, data.Month, data.Day, data.Hour, 0, 0);
        AtualizarDataModificacao();
    }
}
