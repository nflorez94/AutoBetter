using AutoBetterScrapper.Models.Settings;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutoBetterScrapper.Servicios
{
    public class JobsService : IJobsService
    {
        private readonly IOptions<LoginInformation> _logginInformation;

        public JobsService(IOptions<LoginInformation> logginInformation)
        {
            _logginInformation = logginInformation;
        }

        public async Task UpdateBalance(IWebDriver webDriver = null)
        {
            if(webDriver == null)
                webDriver = new ChromeDriver();
            webDriver.Navigate().GoToUrl("https://www.rushbet.co/");
            webDriver = await this.Login(webDriver);
            Thread.Sleep(10000);
            webDriver.Navigate().GoToUrl("https://www.rushbet.co/?page=sportsbook#filter/football/esports_fifa");
            try
            {
                var closeButtons = webDriver.FindElements(By.ClassName("close-modal-button-container"));
                foreach (var closeButton in closeButtons)
                    closeButton.Click();
            }
            catch
            {
                var a = "No buttons";
            }
            var balance=webDriver.FindElement(By.ClassName("bCgcmK")).Text;
        }

        private async Task<IWebDriver> Login(IWebDriver webDriver)
        {
            webDriver.FindElement(By.ClassName("gPURds")).Click();
            webDriver.FindElement(By.CssSelector("#login-form-modal-email")).SendKeys(_logginInformation.Value.Email);
            webDriver.FindElement(By.CssSelector("#login-form-modal-password")).SendKeys(_logginInformation.Value.Password);
            webDriver.FindElement(By.ClassName("w-min-120")).Click();
            return webDriver;
        }
    }
}