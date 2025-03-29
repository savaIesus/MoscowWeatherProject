using Microsoft.AspNetCore.Mvc;
using NetCore1.Models;
using NetCore1.Services;
using System.Collections.Generic;

namespace NetCore1.Controllers
{
    public class MoscowWeatherDataController : Controller
    {
        private readonly AppDbContext _context; // Контекст базы данных для работы с данными
        private readonly ExcelImporter _importer; // Сервис для импорта данных из Excel

        // Конструктор контроллера, принимает зависимости через DI (Dependency Injection)
        public MoscowWeatherDataController(AppDbContext context, ExcelImporter importer)
        {
            _context = context; // Инициализация контекста базы данных
            _importer = importer; // Инициализация сервиса импорта данных
        }

        /// Метод для загрузки файлов Excel и сохранения данных в базу данных.
        /// <param name="files">Массив загруженных файлов</param>
        /// <returns>Результат HTTP-ответа</returns>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile[] files)
        {
            // Проверяем, что файлы были выбраны
            if (files == null || files.Length == 0)
                return BadRequest("Файлы не выбраны.");

            var allData = new List<MoscowWeatherData>(); // Список для хранения всех импортированных данных

            foreach (var file in files)
            {
                if (file.Length == 0) continue; // Пропускаем пустые файлы

                // Создаем временный файл для обработки
                var tempFile = Path.GetTempFileName();
                using (var stream = new FileStream(tempFile, FileMode.Create))
                {
                    await file.CopyToAsync(stream); // Копируем содержимое загруженного файла в поток
                }

                // Импортируем данные из временного файла
                var data = _importer.Import(tempFile);
                allData.AddRange(data); // Добавляем данные в общий список

                // Удаляем временный файл после обработки
                System.IO.File.Delete(tempFile);
            }

            // Сохраняем все данные в базу данных
            await _context.MoscowWeatherData.AddRangeAsync(allData);
            await _context.SaveChangesAsync();

            // Перенаправляем пользователя на метод Index после успешной загрузки
            return RedirectToAction(nameof(Index));
        }

        /// Метод для фильтрации данных по году и месяцу.
        /// <param name="filter">Объект фильтра с параметрами</param>
        /// <returns>Отфильтрованные данные и модель представления</returns>
        [HttpGet]
        public IActionResult Filter(WeatherFilter filter)
        {
            // Фильтруем данные по году и месяцу, если они указаны в фильтре
            var data = _context.MoscowWeatherData
                .Where(w =>
                    (filter.Year == 0 || w.Date.Year == filter.Year) && // Фильтрация по году
                    (filter.Month == 0 || w.Date.Month == filter.Month)) // Фильтрация по месяцу
                .OrderBy(w => w.Date) // Сортировка по дате
                .ToList();

            // Возвращаем модель представления с данными и параметрами фильтрации
            return View(new PaginatedArchieveViewModel
            {
                Data = data, // Отфильтрованные данные
                Filter = filter, // Параметры фильтрации
                Years = _context.MoscowWeatherData.Select(w => w.Date.Year).Distinct().ToList(), // Список доступных лет
                Months = new List<string> { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                  "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" } // Список месяцев
            });
        }

        /// Метод для отображения всех данных о погоде.
        /// <returns>Представление с данными</returns>
        public IActionResult Index()
        {
            return View(_context.MoscowWeatherData.ToList()); // Возвращаем все записи из базы данных для отображения
        }
    }
}
