namespace NetCore1.Models
{
    public class PaginatedArchieveViewModel
    {
        public List<MoscowWeatherData> Data { get; set; }
        public WeatherFilter Filter { get; set; }
        public List<int> Years { get; set; }
        public List<string> Months { get; set; }
    }

}
