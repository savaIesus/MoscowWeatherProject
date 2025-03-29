using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCore1.Models
{
    [Table("moscow_weather")]
    public class MoscowWeatherData
    {
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan MoscowTime { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Range(-50, 50)]
        public double Temperature { get; set; }          // Т

        [Range(0, 100)]
        public double Humidity { get; set; }             // Влажность

        [Column(TypeName = "decimal(4,1)")]
        public decimal DewPoint { get; set; }             // Td

        [Range(600, 900)]
        public int Pressure { get; set; }                 // Давление

        public string WindDirection { get; set; }         // Направление ветра

        [Range(0, double.MaxValue)]
        public int WindSpeed { get; set; }               // Скорость ветра

        [Range(0, 100)]
        public double? Cloudiness { get; set; }          // Облачность (может быть null)

        [Range(0, double.MaxValue)]
        public double H { get; set; }                    // h

        public double? VV { get; set; }                  // VV (может быть null)
        public string WeatherPhenomena { get; set; }     // Погодные явления (может быть пусто)
    }
}
