using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCore1.Models
{
    [Table("moscow_weather")]
    public class MoscowWeatherData
    {
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan MoscowTime { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        [Range(-50, 50)]
        public double T { get; set; }
    }
}
