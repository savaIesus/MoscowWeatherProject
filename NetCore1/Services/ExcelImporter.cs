using NetCore1.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

public class ExcelImporter
{
    public List<MoscowWeatherData> Import(string filePath)
    {
        var measurements = new List<MoscowWeatherData>();
        var moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            for (int rowIdx = 4; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                var row = sheet.GetRow(rowIdx);
                if (row == null) continue;

                var dateCell = row.GetCell(0);
                var timeCell = row.GetCell(1);
                var tempCell = row.GetCell(2);

                // Обработка даты
                DateTime date;
                if (dateCell?.CellType == CellType.Numeric)
                {
                    date = dateCell.DateCellValue ?? DateTime.MinValue;
                }
                else if (dateCell?.CellType == CellType.String)
                {
                    date = DateTime.Parse(dateCell.StringCellValue);
                }
                else
                {
                    continue; // Пропустить строку с некорректной датой
                }

                // Обработка времени
                TimeSpan moscowTime;
                if (timeCell?.CellType == CellType.Numeric)
                {
                    moscowTime = TimeSpan.FromTicks((long)(timeCell.NumericCellValue * TimeSpan.TicksPerDay));
                }
                else if (timeCell?.CellType == CellType.String)
                {
                    moscowTime = TimeSpan.Parse(timeCell.StringCellValue);
                }
                else
                {
                    continue; // Пропустить строку с некорректным временем
                }

                // Обработка температуры
                double temperature;
                if (tempCell?.CellType == CellType.Numeric)
                {
                    temperature = tempCell.NumericCellValue;
                }
                else if (tempCell?.CellType == CellType.String)
                {
                    temperature = double.Parse(tempCell.StringCellValue);
                }
                else
                {
                    continue; // Пропустить строку с некорректной температурой
                }

                measurements.Add(new MoscowWeatherData
                {
                    Date = date,
                    MoscowTime = moscowTime,
                    T = temperature
                });
            }
        }
        return measurements;
    }
}
