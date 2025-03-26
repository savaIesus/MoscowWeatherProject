using Microsoft.AspNetCore.Mvc;
using NetCore1.Services;

namespace NetCore1.Controllers
{
    public class MoscowWeatherDataController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ExcelImporter _importer;

        public MoscowWeatherDataController(AppDbContext context, ExcelImporter importer)
        {
            _context = context;
            _importer = importer;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file?.Length == 0) return BadRequest();

            var tempFile = Path.GetTempFileName();
            using (var stream = new FileStream(tempFile, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var data = _importer.Import(tempFile);
            await _context.MoscowWeatherData.AddRangeAsync(data);
            await _context.SaveChangesAsync();

            System.IO.File.Delete(tempFile);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            return View(_context.MoscowWeatherData.ToList());
        }
    }
}
