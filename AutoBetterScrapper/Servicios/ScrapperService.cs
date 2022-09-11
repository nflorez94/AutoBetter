using AutoBetterScrapper.Models.Dto;
using AutoBetterScrapper.Models.Settings;
using AutoBetterScrapper.Repositories;
using AutoMapper;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutoBetterScrapper.Servicios
{
    public class ScrapperService : IScrapperService
    {
        private readonly IScrapperRepository _repository;
        private readonly IOptions<LoginInformation> _information;
        private readonly IMapper _mapper;

        public ScrapperService(IScrapperRepository repository, IOptions<LoginInformation> information, IMapper mapper)
        {
            _repository = repository;
            _information = information;
            _mapper = mapper;
        }

        public async Task<IWebDriver> StartScrapping()
        {
        Restart:
            IWebDriver webDriver = await this.StartScrapper();
            var reloads = 0;
            var maxReloads = 30;
            webDriver = await this.PrepareWebSite(webDriver);
        Start:
            reloads++;
            if (reloads == maxReloads)
            {
                webDriver.Quit();
                goto Restart;
            }
            try
            {
                var possibilities = await this.GetPossibilities(webDriver);
                var history = await this.UpdateHistory(webDriver);
                webDriver = await this.UpdateBalance(webDriver);
                while (possibilities.Count() == 0)
                {
                    while (history.Where(b => b.Estado.Equals("Pendiente")).Count() > 0)
                    {
                    getHistory:
                        reloads++;
                        if (reloads == maxReloads)
                        {
                            webDriver.Quit();
                            goto Restart;
                        }
                        try
                        {
                            history = await this.UpdateHistory(webDriver);
                            webDriver = await this.UpdateBalance(webDriver);
                        }
                        catch
                        {
                            goto getHistory;
                        }
                    }
                    webDriver = await this.UpdateBalance(webDriver);

                getPosibilities:
                    try
                    {
                        reloads++;
                        if (reloads == maxReloads)
                        {
                            webDriver.Quit();
                            goto Restart;
                        }
                        possibilities = await this.GetPossibilities(webDriver);
                    }
                    catch
                    {
                        goto getPosibilities;
                    }
                }

                if (possibilities.Count() > 0)
                {
                    var verifiedBets = await this.ValidatePossibilites(possibilities, history);
                    if (verifiedBets.Count() > 0)
                    {
                        var attempts = 0;
                    reAttempt:
                        try
                        {
                            while (attempts <= 10)
                            {
                                var bets = await this.MakeBets(webDriver, verifiedBets, history);
                                if (bets.Count() == 0)
                                    goto Start;
                                else
                                {
                                    await this.UpdateHistory(webDriver);
                                    Thread.Sleep(await this.GetSleeptimeAfterBet(bets));
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            attempts++;
                            goto reAttempt;
                        }
                    }
                }
                else
                {
                    goto Start;
                }
            }
            catch
            {
                goto Start;
            }
            goto Start;
            return webDriver;
        }

        private async Task<int> GetSleeptimeAfterBet(List<BetPossibility> bets)
        {
            var sleepTime = 0;
            foreach (var bet in bets)
            {
                var ruledSecconds = bet.HalfTime * 60 * 2;
                var elapsedMinutesInSecconds = int.Parse(bet.ElapsingTime.Split(":")[0]) * 60;
                var elapsedSecconds = int.Parse(bet.ElapsingTime.Split(":")[1]);
                if (sleepTime == 0)
                {
                    sleepTime = (ruledSecconds - (elapsedMinutesInSecconds + elapsedSecconds)) * 1000;
                }
                else
                {
                    var calculatedNewTime = (ruledSecconds - (elapsedMinutesInSecconds + elapsedSecconds)) * 1000;
                    sleepTime = (Math.Min(sleepTime, calculatedNewTime));
                }
            }
            return sleepTime;
        }

        private async Task<List<HistoryRecordDto>> UpdateHistory(IWebDriver webDriver)
        {
            List<HistoryRecordDto> historycList = new();
            webDriver.Navigate().GoToUrl("https://www.rushbet.co/?page=sportsbook#bethistory");
            webDriver.Navigate().Refresh();
        Start:
            try
            {
                try
                {
                    webDriver.FindElements(By.ClassName("mod-KambiBC-betslip-outcome__close-btn"))[0].Click();
                }
                catch
                {
                    var rawHistory = webDriver.FindElements(By.ClassName("KambiBC-harmonized-my-bets-summary__item"));
                    for (var i = 0; i < 5; i++)
                    {
                        var cuponNumber = rawHistory[i].FindElement(By.ClassName("KambiBC-my-bets-summary__coupon-ref")).FindElement(By.ClassName("KambiBC-my-bets-summary__value")).Text;
                        var state = "";
                        var cuota = decimal.Parse(rawHistory[i].FindElement(By.ClassName("KambiBC-my-bets-summary__value")).FindElement(By.ClassName("KambiBC-my-bets-summary__value")).Text.Replace(".", ","));
                        var fecha = await getDate(rawHistory[i].FindElement(By.ClassName("KambiBC-my-bets-summary__coupon-date")).Text);
                        var apostado = decimal.Parse(rawHistory[i].FindElement(By.ClassName("KambiBC-my-bets-summary__stake-value")).Text.Replace("$", string.Empty).Replace(".", string.Empty));
                        var bottomRigth = rawHistory[i].FindElement(By.ClassName("KambiBC-my-bets-summary__coupon-bottom-right"));
                        var pago = 0M;
                        if (bottomRigth.GetAttribute("innerHTML").Contains("Pago Pot"))
                        {
                            state = "Pendiente";
                        }
                        else if (bottomRigth.GetAttribute("innerHTML").Contains("Pago"))
                        {
                            pago = decimal.Parse(bottomRigth.FindElement(By.ClassName("KambiBC-my-bets-summary-payout__value")).Text.Replace("$", string.Empty).Replace(".", string.Empty));
                            state = "Ganada";
                        }
                        else
                            state = "Perdida";

                        historycList.Add(new HistoryRecordDto
                        {
                            Cupon = cuponNumber,
                            Estado = state,
                            Cuota = cuota,
                            Fecha = fecha,
                            Apostado = apostado,
                            Pago = pago
                        });
                    }
                    foreach (var history in historycList)
                    {
                        await _repository.SaveHistory(history);
                    }
                }
            }
            catch (Exception ex)
            {
                goto Start;
            }
            return historycList;
        }

        private Task<DateTime> getDate(string fecha)
        {
            List<KeyValuePair<string, string>> months = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ene", "01"),
                new KeyValuePair<string, string>("feb", "02"),
                new KeyValuePair<string, string>("mar", "03"),
                new KeyValuePair<string, string>("abr", "04"),
                new KeyValuePair<string, string>("may", "05"),
                new KeyValuePair<string, string>("jun", "06"),
                new KeyValuePair<string, string>("jul", "07"),
                new KeyValuePair<string, string>("ago", "08"),
                new KeyValuePair<string, string>("sept", "09"),
                new KeyValuePair<string, string>("oct", "10"),
                new KeyValuePair<string, string>("nov", "11"),
                new KeyValuePair<string, string>("dic", "12"),
            };

            var dateSplitted = fecha.Split('•');
            var day = dateSplitted[0].Split(' ')[0];
            var month = months.First(d => d.Key.Equals(dateSplitted[0].Split(' ')[1].ToLower())).Value;
            var year = dateSplitted[0].Split(' ')[2];
            var time = dateSplitted[1].Split(' ')[1];
            var date = DateTime.Parse($"{day}/{month}/{year} {time}");
            return Task.FromResult(date);
        }

        private async Task<List<BetPossibility>> MakeBets(IWebDriver webDriver, List<BetPossibility> filteredBets, List<HistoryRecordDto> history)
        {
            List<BetPossibility> Betlist = new();
            //try
            //{
            //    webDriver.FindElements(By.ClassName("mod-KambiBC-betslip-outcome__close-btn"))[0].Click();
            //}
            //catch
            //{
                var parameters = _mapper.Map<BettingParameterDto>(await _repository.GetBettingParameters());
                var straightLostCount = 0;
                var historyOrdered = history.OrderBy(h => h.Fecha).ToList();

                foreach (var bet in history.OrderBy(h => h.Fecha))
                {
                    straightLostCount = bet.Estado.Equals("Perdida") ? bet.Apostado==1000M?1:straightLostCount + 1 : 0;
                }
                var nexBetAmount = parameters.MinimumBetAmount;
                if (straightLostCount > 0 && historyOrdered[history.Count() - 1].Apostado > parameters.MinimumBetAmount)
                {
                    if (straightLostCount > 0)
                    {
                        for (int i = 0; i < straightLostCount; i++)
                        {
                            nexBetAmount = nexBetAmount * parameters.IncreaseFactor;
                        }
                        if (nexBetAmount > parameters.MaximumBetAmount)
                        {
                            nexBetAmount = parameters.MaximumBetAmount;
                        }
                    }
                }
                else if (straightLostCount > 0 && historyOrdered[history.Count() - 1].Apostado == parameters.MinimumBetAmount)
                {
                    nexBetAmount = nexBetAmount * parameters.IncreaseFactor;
                    if (nexBetAmount > parameters.MaximumBetAmount)
                    {
                        nexBetAmount = parameters.MaximumBetAmount;
                    }
                }
                if (straightLostCount < 5)
                {
                    for (int i = 0; i < parameters.SimultaneousBets; i++)
                    {
                        if (filteredBets[i].HomeBet < filteredBets[i].AwayBet)
                        {
                            var containers = webDriver.FindElements(By.ClassName("fkFtEG"));
                            foreach (var container in containers)
                            {

                                var matches = container.FindElements(By.ClassName("KambiBC-event-item--sport-FOOTBALL"));
                                foreach (var match in matches)
                                {
                                    var participants = match.FindElements(By.ClassName("KambiBC-event-participants__name"));
                                    var homeTeam = participants[0].Text;
                                    var awayTeam = participants[1].Text;
                                    var bets = match.FindElements(By.ClassName("hLbRHz"));
                                    var homeBet = decimal.Parse(bets[0].Text.Replace(".", ","));
                                    var awayBet = decimal.Parse(bets[2].Text.Replace(".", ","));
                                    if (filteredBets[i].HomeTeam == homeTeam)
                                    {
                                        bets[0].Click();
                                    }

                                }
                            }
                        }
                        else
                        {
                            var containers = webDriver.FindElements(By.ClassName("fkFtEG"));
                            foreach (var container in containers)
                            {

                                var matches = container.FindElements(By.ClassName("KambiBC-event-item--sport-FOOTBALL"));
                                foreach (var match in matches)
                                {
                                    var participants = match.FindElements(By.ClassName("KambiBC-event-participants__name"));
                                    var homeTeam = participants[0].Text;
                                    var awayTeam = participants[1].Text;
                                    var bets = match.FindElements(By.ClassName("hLbRHz"));
                                    var homeBet = decimal.Parse(bets[0].Text.Replace(".", ","));
                                    var awayBet = decimal.Parse(bets[2].Text.Replace(".", ","));
                                    if (filteredBets[i].HomeTeam == homeTeam)
                                    {
                                        bets[2].Click();
                                    }

                                }
                            }
                        }
                        var betApproved = false;
                        var tryes = 0;
                        while (!betApproved && tryes <= 10)
                        {
                            try
                            {
                                Thread.Sleep(3000);
                                webDriver.FindElement(By.ClassName("mod-KambiBC-js-stake-input")).SendKeys($"{(int)nexBetAmount}");
                                Thread.Sleep(3000);
                                webDriver.FindElement(By.ClassName("mod-KambiBC-betslip__place-bet-btn")).Click();
                                Thread.Sleep(20000);
                                var betValidatedCheck = webDriver.FindElement(By.ClassName("mod-KambiBC-betslip-receipt-header__title")).Text.Equals("¡Tu apuesta se ha realizado!");
                                if (betValidatedCheck)
                                    betApproved = true;
                                Betlist.Add(filteredBets[i]);
                                await this.UpdateBalance(webDriver);
                            }
                            catch (Exception ex)
                            {
                                webDriver.FindElement(By.ClassName("mod-KambiBC-betslip__approve-odds-btn")).Click();
                                Thread.Sleep(3000);
                                webDriver.FindElement(By.ClassName("mod-KambiBC-betslip__place-bet-btn")).Click();
                                Thread.Sleep(20000);
                                var betValidatedCheck = webDriver.FindElement(By.ClassName("mod-KambiBC-betslip-receipt-header__title")).Text.Equals("¡Tu apuesta se ha realizado!");
                                if (betValidatedCheck)
                                    betApproved = true;
                                tryes++;
                            }
                        }

                    }
                //}
            }
            return Betlist;
        }

        private async Task<List<BetPossibility>> ValidatePossibilites(List<BetPossibility> possibilities, List<HistoryRecordDto> history)
        {
            var activeBets = history.Where(h => h.Estado.Equals("Pendiente")).Count() > 0 ? true : false;
            List<BetPossibility> filteredPossibility = new();
            if (!activeBets)
            {
                var parameters = _mapper.Map<BettingParameterDto>(await _repository.GetBettingParameters());
                foreach (var possibility in possibilities)
                {
                    bool percTimeElapsed = await this.GetPercTimeElapsed(possibility);
                    if (percTimeElapsed)
                    {
                        if (possibility.HomeActualScore == possibility.AwayActualScore)
                        {
                            var lowerBetCuota = Math.Min(possibility.HomeBet, possibility.AwayBet);
                            if (lowerBetCuota < parameters.MaximumBetCuota && lowerBetCuota > parameters.MinimumBetCuota)
                                filteredPossibility.Add(possibility);
                        }
                    }
                }
            }
            return filteredPossibility;
        }

        private Task<bool> GetPercTimeElapsed(BetPossibility possibility)
        {
            var ruledSecconds = possibility.HalfTime * 60 * 2;
            var elapsedMinutesInSecconds = int.Parse(possibility.ElapsingTime.Split(":")[0]) * 60;
            var elapsedSecconds = int.Parse(possibility.ElapsingTime.Split(":")[1]);
            decimal test = ((decimal)elapsedMinutesInSecconds + (decimal)elapsedSecconds) / (decimal)ruledSecconds;
            if (test < 0.50M)
                return Task.FromResult(true);
            return Task.FromResult(false);
        }

        private Task<List<BetPossibility>> GetPossibilities(IWebDriver webDriver)
        {
            List<BetPossibility> possibilities = new();
            Thread.Sleep(5000);
        getPosibilities:
            var containers = webDriver.FindElements(By.ClassName("fkFtEG"));
            foreach (var container in containers)
            {
                var leagueNameAndFormat = container.FindElements(By.ClassName("cThihH"))[1].Text;
                var matches = container.FindElements(By.ClassName("KambiBC-event-item--sport-FOOTBALL"));
                foreach (var match in matches)
                {
                    try
                    {
                        var timeElapsed = match.FindElement(By.ClassName("KambiBC-match-clock__inner")).Text;
                        var participants = match.FindElements(By.ClassName("KambiBC-event-participants__name"));
                        var homeTeam = participants[0].Text;
                        var awayTeam = participants[1].Text;
                        var scores = match.FindElements(By.ClassName("KambiBC-event-result__points"));
                        var homeScore = scores[0].Text;
                        var awayScore = scores[1].Text;
                        var bets = match.FindElements(By.ClassName("hLbRHz"));
                        var homeBet = decimal.Parse(bets[0].Text.Replace(".", ","));
                        var awayBet = decimal.Parse(bets[2].Text.Replace(".", ","));
                        var halfTime = leagueNameAndFormat.Split(" ")[leagueNameAndFormat.Split(" ").Length - 2].Replace("(2x", string.Empty).Replace("mins)", string.Empty).Replace("min)", string.Empty);

                        var posibility = new BetPossibility
                        {
                            Id = Guid.NewGuid(),
                            HomeTeam = homeTeam,
                            AwayTeam = awayTeam,
                            HomeActualScore = homeScore,
                            AwayActualScore = awayScore,
                            ElapsingTime = timeElapsed,
                            HalfTime = int.Parse(halfTime),
                            League = leagueNameAndFormat,
                            HomeBet = homeBet,
                            AwayBet = awayBet,
                            HomeButton = bets[0],
                            AwayButton = bets[2]
                        };
                        possibilities.Add(posibility);
                    }
                    catch
                    {

                    }

                }
            }

            return Task.FromResult(possibilities);
        }

        public async Task<IWebDriver> UpdateBalance(IWebDriver webDriver = null)
        {
            if (webDriver == null)
            {
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
                    webDriver.Close();
                }
            }
            var rawbalance = (webDriver.FindElement(By.CssSelector("#rsi-top-navigation > header > div > div.sc-fzXfNf.cnzRUd > div.sc-fzXfNQ.fwPvEA > div > span"))).Text;
            var balance = decimal.Parse(rawbalance.Replace("$", string.Empty).Replace(".", string.Empty));
            webDriver.Navigate().GoToUrl("https://www.rushbet.co/?page=sportsbook#filter/football/esports_fifa");
            await _repository.Updatebalance(_information.Value.Email, balance);
            return webDriver;
        }

        public Task<IEnumerable<object>> FindOportunities(IWebDriver webDriver)
        {
            throw new NotImplementedException();
        }

        public async Task<IWebDriver> Login(IWebDriver webDriver)
        {
            webDriver.FindElement(By.ClassName("gPURds")).Click();
            webDriver.FindElement(By.CssSelector("#login-form-modal-email")).SendKeys(_information.Value.Email);
            webDriver.FindElement(By.CssSelector("#login-form-modal-password")).SendKeys(_information.Value.Password);
            webDriver.FindElement(By.ClassName("w-min-120")).Click();
            return webDriver;
        }

        public async Task<IWebDriver> PrepareWebSite(IWebDriver webDriver)
        {
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
            return webDriver;
        }

        public Task<bool> RevalidateOportunity(IWebDriver webDriver)
        {
            throw new NotImplementedException();
        }

        public async Task<IWebDriver> StartScrapper()
        {
            IWebDriver webDriver = new ChromeDriver();
            return webDriver;
        }

    }
}
