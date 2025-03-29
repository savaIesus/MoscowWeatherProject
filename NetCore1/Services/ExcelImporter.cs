using Microsoft.AspNetCore.Mvc;
using NetCore1.Models;
using NetCore1.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

public class ExcelImporter
{
    /// <summary>
    /// Импортирует данные о погоде из Excel-файла.
    /// </summary>
    /// <param name="filePath">Путь к Excel-файлу</param>
    /// <returns>Список объектов MoscowWeatherData</returns>
    public List<MoscowWeatherData> Import(string filePath)
    {
        var data = new List<MoscowWeatherData>();

        // Открываем файл через поток для безопасного управления ресурсами
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            var workbook = new XSSFWorkbook(stream);

            // Обходим все листы в книге
            for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
            {
                var sheet = workbook.GetSheetAt(sheetIndex);

                // Начинаем с 5-й строки (индекс 4) - предположительно заголовки
                for (int rowIdx = 4; rowIdx <= sheet.LastRowNum; rowIdx++)
                {
                    var row = sheet.GetRow(rowIdx);
                    if (row == null) continue; // Пропускаем пустые строки

                    // Создаем объект данных из текущей строки
                    var record = new MoscowWeatherData
                    {
                        Date = ParseDate(row.GetCell(0)),          // Дата
                        MoscowTime = ParseTime(row.GetCell(1)),    // Время
                        Temperature = ParseDouble(row.GetCell(2)), // Температура
                        Humidity = ParseDouble(row.GetCell(3)),    // Влажность
                        DewPoint = ParseDecimal(row.GetCell(4)),  // Точка росы
                        Pressure = ParseInt(row.GetCell(5)),      // Давление
                        WindDirection = ParseString(row.GetCell(6)), // Направление ветра
                        WindSpeed = ParseInt(row.GetCell(7)),     // Скорость ветра
                        Cloudiness = ParseNullableDouble(row.GetCell(8)), // Облачность
                        H = ParseDouble(row.GetCell(9)),         // Осадки
                        VV = ParseNullableDouble(row.GetCell(10)), // Видимость
                        WeatherPhenomena = ParseString(row.GetCell(11)) // Погодные явления
                    };

                    // Добавляем только валидные записи
                    if (IsValid(record))
                        data.Add(record);
                }
            }
        }
        return data;
    }

    /// <summary>
    /// Проверяет соответствие данных реалистичным погодным значениям
    /// </summary>
    private bool IsValid(MoscowWeatherData record)
    {
        return
            record.Temperature >= -50 && record.Temperature <= 50 && 
            record.Humidity >= 0 && record.Humidity <= 100 && 
            record.DewPoint >= -50 && record.DewPoint <= 50 &&    
            record.Pressure >= 600 && record.Pressure <= 900 &&   
            record.WindSpeed >= 0 &&           
            (record.Cloudiness == null || (record.Cloudiness >= 0 && record.Cloudiness <= 100)) &&
            record.H > 0 &&                                     
            !string.IsNullOrWhiteSpace(record.WindDirection);      
    }

    /// <summary>
    /// Преобразует ячейку Excel в DateTime
    /// </summary>
    private DateTime ParseDate(ICell cell)
    {
        if (cell == null) return DateTime.MinValue;

        // Числовые ячейки (даты в Excel)
        if (cell.CellType == CellType.Numeric)
        {
            return (DateTime)cell.DateCellValue;
        }

        // Текстовые ячейки
        var value = cell.StringCellValue?.Trim();
        if (DateTime.TryParseExact(
            value,
            new[] { "MM.dd.yyyy", "dd.MM.yyyy" }, // Допустимые форматы
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime result))
        {
            return result;
        }

        Console.WriteLine($"Ошибка преобразования даты: '{value}'.");
        return DateTime.MinValue;
    }

    /// <summary>
    /// Преобразует ячейку Excel в TimeSpan (время)
    /// </summary>
    private TimeSpan ParseTime(ICell cell)
    {
        if (cell == null) return TimeSpan.Zero;

        // Числовые ячейки (время в Excel)
        if (cell.CellType == CellType.Numeric)
        {
            return TimeSpan.FromHours(cell.NumericCellValue * 24);
        }

        // Текстовые ячейки
        var value = cell.StringCellValue?.Trim();
        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan result))
        {
            return result;
        }

        Console.WriteLine($"Ошибка преобразования времени: '{value}'.");
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Преобразует ячейку Excel в decimal
    /// </summary>
    private decimal ParseDecimal(ICell cell)
    {
        if (cell == null) return 0;

        // Числовые ячейки
        if (cell.CellType == CellType.Numeric)
        {
            return (decimal)cell.NumericCellValue;
        }

        // Текстовые ячейки
        var value = cell.StringCellValue?.Trim();
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        Console.WriteLine($"Ошибка преобразования: '{value}' в decimal.");
        return 0;
    }

    /// <summary>
    /// Преобразует ячейку Excel в double
    /// </summary>
    private double ParseDouble(ICell cell)
    {
        if (cell == null) return 0;

        // Если ячейка числовая
        if (cell.CellType == CellType.Numeric)
        {
            return cell.NumericCellValue;
        }

        // Если ячейка текстовая
        var value = cell.StringCellValue?.Trim();
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        // Логирование ошибки и возврат значения по умолчанию
        Console.WriteLine($"Ошибка преобразования: '{value}' в число.");
        return 0;
    }

    /// <summary>
    /// Преобразует ячейку Excel в nullable double
    /// </summary>
    private double? ParseNullableDouble(ICell cell)
    {
        if (cell == null) return null;

        // Числовые ячейки
        if (cell.CellType == CellType.Numeric)
        {
            return cell.NumericCellValue;
        }

        // Текстовые ячейки
        var value = cell.StringCellValue?.Trim();
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        Console.WriteLine($"Ошибка преобразования: '{value}' в число.");
        return null;
    }

    /// <summary>
    /// Преобразует ячейку Excel в int
    /// </summary>
    private int ParseInt(ICell cell)
    {
        if (cell == null) return 0;

        // Обработка числовых ячеек
        if (cell.CellType == CellType.Numeric)
        {
            return (int)Math.Round(cell.NumericCellValue);
        }

        // Обработка текстовых ячеек
        var value = cell.StringCellValue?.Trim();
        if (int.TryParse(value, out int result))
        {
            return result;
        }

        // Логирование ошибки (или возврат 0)
        Console.WriteLine($"Ошибка преобразования: '{value}' в целое число.");
        return 0;
    }

    /// <summary>
    /// Преобразует ячейку Excel в строку
    /// </summary>
    private string ParseString(ICell cell)
    {
        return cell?.StringCellValue ?? string.Empty;
    }
}
