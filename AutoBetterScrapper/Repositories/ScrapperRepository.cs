using AutoBetterScrapper.Models.Dto;
using AutoBetterScrapper.Models.Entities;
using AutoBetterScrapper.Repositories.Data;
using AutoBetterScrapper.Transversals.Commons;
using Dapper;
using System.Data;

namespace AutoBetterScrapper.Repositories
{
    public class ScrapperRepository : DapperGenerycRepository, IScrapperRepository
    {
        public ScrapperRepository(IConfiguration configuration) : base(configuration)
        {

        }

        public async Task<BettingParameter> GetBettingParameters()
        {
            return await GetAsyncFirst<BettingParameter>(DBHelperParameters.BuilderFunction(
                DBHelperParameters.EnumSchemas.DBO), null, CommandType.StoredProcedure);
        }

        public async Task SaveHistory(HistoryRecordDto history)
        {
            DynamicParameters parameters = new();

            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Cupon), history.Cupon);
            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Apostado), history.Apostado);
            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Pago), history.Pago);
            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Fecha), history.Fecha);
            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Cuota), history.Cuota);
            parameters.Add(EnumHelper.GetEnumDescription(EnumBetHistoryParams.Estado), history.Estado);

            await GetAsyncFirst<object>(DBHelperParameters.BuilderFunction(
                DBHelperParameters.EnumSchemas.DBO), parameters, CommandType.StoredProcedure);
        }

        public async Task Updatebalance(string email, decimal balance)
        {
            DynamicParameters parameters = new();

            parameters.Add(EnumHelper.GetEnumDescription(EnumAccountInformationParams.Email), email);
            parameters.Add(EnumHelper.GetEnumDescription(EnumAccountInformationParams.Balance), balance);

            await GetAsyncFirst<object>(DBHelperParameters.BuilderFunction(
                DBHelperParameters.EnumSchemas.DBO), parameters, CommandType.StoredProcedure);
        }
    }
}
