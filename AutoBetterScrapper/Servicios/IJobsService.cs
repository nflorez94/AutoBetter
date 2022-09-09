using OpenQA.Selenium;

namespace AutoBetterScrapper.Servicios
{
    public interface IJobsService
    {
        public Task UpdateBalance(IWebDriver webDriver=null);
    }
}
