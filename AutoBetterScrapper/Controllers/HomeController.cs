using AutoBetterScrapper.Models;
using AutoBetterScrapper.Servicios;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

namespace AutoBetterScrapper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IScrapperService _scrapperService;

        public HomeController(ILogger<HomeController> logger, IScrapperService scrapperService)
        {
            _logger = logger;
            _scrapperService = scrapperService;
        }

        public async Task<IActionResult> Index()
        {
            var bandera = false;
            while (!bandera)
            {
                try
                {
                    var finished = await _scrapperService.StartScrapping();
                    if (finished != null)
                        bandera = true;
                }
                catch
                {
                    bandera = false;
                }
            }
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
    }
}