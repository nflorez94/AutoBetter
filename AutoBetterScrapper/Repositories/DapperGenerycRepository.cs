using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace AutoBetterScrapper.Repositories
{
    public abstract class DapperGenerycRepository
    {
        /// <summary>
        /// Configuration App
        /// </summary>
        /// <author>Oscar Julian Rojas</author>
        /// <date>18/02/2021</date>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">IConfiguration</param>
        /// <author>Oscar Julian Rojas</author>
        /// <date>18/02/2021</date>
        protected DapperGenerycRepository(IConfiguration configuration) => this._configuration = configuration;

        private SqlConnection GetConnection(string dbConnection) => new SqlConnection(dbConnection);

        public async Task<TOutput> GetAsyncFirst<TOutput>(
            string NameProcedureOrQueryString, DynamicParameters parameters, CommandType typeCommand) where TOutput : new()
        {
            using var connection = GetConnection(this._configuration.GetConnectionString("connectionName"));
            var cmd = new CommandDefinition(NameProcedureOrQueryString, null, null, null, typeCommand);
            await connection.OpenAsync();
            if (parameters != null)
                cmd = new CommandDefinition(NameProcedureOrQueryString, parameters, null, null, typeCommand);
            var retorno = await connection.QueryFirstOrDefaultAsync<TOutput>(cmd);

            return retorno;
        }
        public async Task<IEnumerable<TOutput>> GetAsyncList<TOutput>(string NameProcedureOrQueryString, DynamicParameters parameters, CommandType typeCommand) where TOutput : new()
        {
            using var connection = GetConnection(this._configuration.GetConnectionString("connectionName"));
            var cmd = new CommandDefinition(NameProcedureOrQueryString, parameters, null, null, typeCommand);
            await connection.OpenAsync();
            if (parameters != null)
                cmd = new CommandDefinition(NameProcedureOrQueryString, parameters, null, null, typeCommand);
            return await connection.QueryAsync<TOutput>(cmd);
        }
    }
}