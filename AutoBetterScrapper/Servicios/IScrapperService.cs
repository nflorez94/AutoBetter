using OpenQA.Selenium;

namespace AutoBetterScrapper.Servicios
{
    public interface IScrapperService
    {
        public Task<IWebDriver> StartScrapper();
        public Task<IWebDriver> PrepareWebSite(IWebDriver webDriver);
        public Task<IWebDriver> Login(IWebDriver webDriver);
        public Task<IWebDriver> StartScrapping();
        public Task<IEnumerable<object>> FindOportunities(IWebDriver webDriver);
        public Task<bool> RevalidateOportunity(IWebDriver webDriver);
    }
}
