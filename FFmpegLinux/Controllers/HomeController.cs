using FFmpegLinux.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FFmpegLinux.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string GetDataPath(string file) => $"Data\\{file}";

        [HttpPost]
        public void UploadAsync(IFormFile file)
        {
            if (file != null)
            {
                var path = GetDataPath(file.FileName);

                using var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);

                var ffmpegxabe = new FFmpegXabe();
                //await Task.Run(() => ffmpegxabe.convertMP3(path, file.FileName));
                _ = ffmpegxabe.convertMP3(path, file.FileName);        
                
            }
        }
    }
}
