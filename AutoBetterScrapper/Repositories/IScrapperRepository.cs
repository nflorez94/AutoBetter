using AutoBetterScrapper.Models.Dto;
using AutoBetterScrapper.Models.Entities;

namespace AutoBetterScrapper.Repositories
{
    public interface IScrapperRepository
    {
        Task Updatebalance(string email, decimal balance);
        Task SaveHistory(HistoryRecordDto history);
        Task<BettingParameter> GetBettingParameters();
    }
}
