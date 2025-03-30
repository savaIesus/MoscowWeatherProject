namespace NetCore1.Models
{
    public class PaginatedArchieveViewModel
    {
        public List<MoscowWeatherData> Data { get; set; } // Данные для отображения
        public WeatherFilter Filter { get; set; } // Фильтр для года и месяца
        public List<int> Years { get; set; } // Список доступных лет
        public List<string> Months { get; set; } // Список доступных месяцев

        // Свойства для пагинации
        public int CurrentPage { get; set; } // Текущая страница
        public int TotalPages { get; set; } // Общее количество страниц
    }
}
