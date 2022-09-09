using OpenQA.Selenium;

namespace AutoBetterScrapper.Models.Dto
{
    public class BetPossibility
    {
        public Guid Id { get; set; }
        public string League { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string HomeActualScore { get; set; }
        public string AwayActualScore { get; set; }
        public string ElapsingTime { get; set; }
        public int HalfTime { get; set; }
        public decimal HomeBet { get; set; }
        public decimal AwayBet { get; set; }
        public IWebElement AwayButton { get; internal set; }
        public IWebElement HomeButton { get; internal set; }
    }
}
