
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using AndersonEnterprise.SqlQueryService.Models;

namespace AndersonEnterprise.SqlQueryService
{
    public interface IRunTimeService
    {
        List<object> RunNamedQuery(string queryOverride = "", int skipRows = 0, int takeRows = 0, IList<QueryParam> parameters = null);
    }

    public class RunTimeService : IRunTimeService
    {
        public RunTimeService( IConfiguration config, string queryName = null)
        {
            InfoDataConnection = GetOpenConnection(config.GetConnectionString("InfoStoreConnection"));
            AppDataConnection = GetOpenConnection(config.GetConnectionString("DefaultConnection"));

            if (!Int32.TryParse(config["commandTimeout"], result: out _commandTimeout))
            {
                _commandTimeout = 10;
            }

            requestedQueryName = queryName;
        }

        public List<object> RunNamedQuery(string queryOverride = "", int skipRows = 0, int takeRows = -1, IList<QueryParam> parameters = null)
        {
            string queryName = queryOverride != string.Empty ? queryOverride : requestedQueryName;

            var querySql = this.GetQuerySql(queryName);
            if (querySql == null)
            {
                throw new ApplicationException(string.Format("requested queryname ({0}) does not exist", requestedQueryName));
            }

            return RunSqlQuery(querySql, skipRows, takeRows, parameters);
        }
        protected List<object> RunSqlQuery(string sqlSelect, int skipRows, int takeRows, IList<QueryParam> parameters)
        {
            if (takeRows != -1)
            {
                sqlSelect = "SELECT TOP " + (takeRows + 0).ToString() + " " + sqlSelect.Substring(7);
            }
            else
            {
                //caller wants all rows
            }

            foreach (var param in parameters)
            {
                _parameters.Add( param.ParamName, param.ParamValue, DbType.String, ParameterDirection.Input);

                var x = sqlSelect.LastIndexOf(" WHERE ");
                if (x > -1)
                {
                    sqlSelect = sqlSelect + string.Format(" AND {0} = @{1}", param.DbColumnName, param.ParamName);
                }
                else
                {
                    sqlSelect = sqlSelect + string.Format(" WHERE {0} = @{1}", param.DbColumnName, param.ParamName);
                }
            }

            try
            {
                var resultRows = AppDataConnection.Query<object>(sql: sqlSelect, commandTimeout: _commandTimeout, param: _parameters).ToList();

                return resultRows;
            }
            catch (Exception ex)
            {
                throw; //todo: caller should handle this?
            }
        }

        #region private stuff
        private int _commandTimeout = int.MinValue;
        private DynamicParameters _parameters = new DynamicParameters();

        private readonly string requestedQueryName;
        private SqlConnection AppDataConnection { get; }
        private SqlConnection InfoDataConnection { get; }
        static SqlConnection GetOpenConnection(string cs, bool mars = false)
        {
            if (mars)
            {
                var scsb = new SqlConnectionStringBuilder(cs)
                {
                    MultipleActiveResultSets = true
                };
                cs = scsb.ConnectionString;
            }
            var connection = new SqlConnection(cs);
            connection.Open();
            return connection;
        }
        private string GetQuerySql(string queryName)
        {
            var origReader = InfoDataConnection.Query<InfoQuery>(sql: string.Format("SELECT TOP 1 * FROM AES.InfoQuery WHERE QueryName = '{0}'", queryName)).FirstOrDefault();

            return origReader.QuerySql;
        }
        #endregion

        #region nested classes
        private class InfoQuery
        {
            public string QuerySql { get; set; }
        }
        #endregion
    }
}