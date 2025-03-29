using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCore1.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "moscow_weather",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    MoscowTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Temperature = table.Column<double>(type: "numeric(5,1)", nullable: false),
                    Humidity = table.Column<double>(type: "double precision", nullable: false),
                    DewPoint = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    Pressure = table.Column<int>(type: "integer", nullable: false),
                    WindDirection = table.Column<string>(type: "text", nullable: false),
                    WindSpeed = table.Column<int>(type: "integer", nullable: false),
                    Cloudiness = table.Column<double>(type: "double precision", nullable: true),
                    H = table.Column<double>(type: "double precision", nullable: false),
                    VV = table.Column<double>(type: "double precision", nullable: true),
                    WeatherPhenomena = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moscow_weather", x => new { x.Date, x.MoscowTime });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "moscow_weather");
        }
    }
}
