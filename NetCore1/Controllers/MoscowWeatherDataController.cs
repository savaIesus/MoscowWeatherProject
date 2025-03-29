using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore1.Models;
using Npgsql;
using NetCore1.Services;



namespace NetCore1.Controllers
{
    // Контроллер для обработки данных о погоде в Москве
    public class MoscowWeatherDataController : Controller
    {
        private readonly AppDbContext _context; // Контекст базы данных для работы с данными
        private readonly ExcelImporter _importer; // Сервис для импорта данных из Excel
        private readonly ILogger<MoscowWeatherDataController> _logger; // Логгер для записи информации о работе контроллера

        // Конструктор контроллера, принимает зависимости через DI (Dependency Injection)
        public MoscowWeatherDataController(AppDbContext context, ExcelImporter importer, ILogger<MoscowWeatherDataController> logger)
        {
            _context = context; // Инициализация контекста базы данных
            _importer = importer; // Инициализация сервиса импорта данных
            _logger = logger; // Инициализация логгера
        }

        /// <summary>
        /// Метод для загрузки файлов Excel и сохранения данных в базу данных.
        /// </summary>
        /// <param name="files">Массив загруженных файлов</param>
        /// <returns>Результат HTTP-ответа</returns>
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile[] files)
        {
            // Проверяем, что файлы были выбраны
            if (files == null || files.Length == 0)
            {
                return BadRequest("Файлы не выбраны.");
            }

            var errorMessages = new List<string>(); // Список для хранения сообщений об ошибках

            foreach (var file in files)
            {
                if (file.Length == 0) continue; // Пропускаем пустые файлы

                var tempFile = Path.GetTempFileName(); // Создаем временный файл
                try
                {
                    // Открываем поток для записи во временный файл
                    using (var stream = new FileStream(tempFile, FileMode.Create))
                    {
                        await file.CopyToAsync(stream); // Копируем содержимое загруженного файла во временный файл
                    }

                    // Импортируем данные из временного файла с помощью сервиса ExcelImporter
                    var data = _importer.Import(tempFile);

                    // Перебираем все записи, полученные из Excel-файла
                    foreach (var record in data)
                    {
                        try
                        {
                            _context.MoscowWeatherData.Add(record); // Добавляем запись в контекст базы данных
                            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                        }
                        catch (DbUpdateException ex)
                        {
                            // Обрабатываем исключение, связанное с ошибками при обновлении базы данных
                            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                            {
                                // Обрабатываем исключение, связанное с нарушением уникальности ключа
                                _logger.LogWarning($"Попытка добавить дублирующуюся запись из файла {file.FileName}: {pgEx.Message}");
                                errorMessages.Add($"Попытка добавить дублирующуюся запись из файла {file.FileName}: {pgEx.Message}");
                            }
                            else
                            {
                                // Обрабатываем другие исключения, связанные с базой данных
                                _logger.LogError($"Ошибка при добавлении записи из файла {file.FileName}: {ex.Message}");
                                errorMessages.Add($"Ошибка при добавлении записи из файла {file.FileName}: {ex.Message}");
                            }

                            // Отменяем изменения для данной записи, чтобы избежать дальнейших проблем
                            _context.Entry(record).State = EntityState.Detached;
                        }
                        catch (Exception ex)
                        {
                            // Обрабатываем другие исключения
                            _logger.LogError($"Ошибка при добавлении записи из файла {file.FileName}: {ex.Message}");
                            errorMessages.Add($"Ошибка при добавлении записи из файла {file.FileName}: {ex.Message}");

                            // Отменяем изменения для данной записи, чтобы избежать дальнейших проблем
                            _context.Entry(record).State = EntityState.Detached;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Обрабатываем исключения, возникшие при обработке файла
                    _logger.LogError($"Ошибка при обработке файла {file.FileName}: {ex.Message}");
                    errorMessages.Add($"Ошибка при обработке файла {file.FileName}: {ex.Message}");
                }
                finally
                {
                    System.IO.File.Delete(tempFile); // Удаляем временный файл
                }
            }

            // Если есть сообщения об ошибках, возвращаем BadRequest с перечислением ошибок
            if (errorMessages.Any())
            {
                //return BadRequest(string.Join("\n", errorMessages));
            }

            // Перенаправляем пользователя на метод Index после успешной загрузки
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Метод для фильтрации данных по году и месяцу.
        /// </summary>
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

        /// <summary>
        /// Метод для отображения всех данных о погоде.
        /// </summary>
        /// <returns>Представление с данными</returns>
        public IActionResult Index()
        {
            return View(_context.MoscowWeatherData.ToList()); // Возвращаем все записи из базы данных для отображения
        }
    }
}

