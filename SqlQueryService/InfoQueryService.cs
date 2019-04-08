﻿
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;

namespace AndersonEnterprise.SqlQueryService
{
    using AndersonEnterprise.SqlQueryService.Models;

    public interface IInfoQueryService
    {
        string AddNamedQuery(NamedQuery namedQuery);
        string UpdateNamedQuery(NamedQuery namedQuery);
        List<object> RunStoredQuery(string queryOverride = "");
        List<object> RunTableQuery(string queryOrTableId, int topRows = 1, int startRow = 0);
        List<object> RunSqlQuery(string sqlSelect, int topRows = 1, int startRow = 0);
        string BuildSqlSelectString(List<QueryTableDef> relations, int startRow = 0, int topRows = 1);
    }

    public class InfoQueryService : IInfoQueryService
    {
        public InfoQueryService( IConfiguration config, string queryName = null)
        {
            InfoDataConnection = GetOpenConnection(config.GetConnectionString("InfoStoreConnection"));
            AppDataConnection = GetOpenConnection(config.GetConnectionString("DefaultConnection"));

            if (!Int32.TryParse(config["commandTimeout"], result: out _commandTimeout))
            {
                _commandTimeout = 10;
            }

            requestedQueryName = queryName;
        }

        #region private stuff
        private int _commandTimeout = int.MinValue;

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
        #endregion

        public string AddNamedQuery(NamedQuery namedQuery)
        {
            var guidPk = Guid.NewGuid().ToString();

            InfoDataConnection.Execute("INSERT INTO AES.InfoQuery (QueryId,QueryName,QuerySql,QueryOwner) VALUES (@QueryId, @QueryName, @QuerySql,@QueryOwner)", 
                new {
                    QueryId = guidPk,
                    QueryName = namedQuery.QueryName.Trim(),
                    QuerySql = namedQuery.QuerySql,
                    QueryOwner = "XYZ"
                }
            );

            InfoDataConnection.Execute( "INSERT INTO AES.InfoQueryVersion (QueryId,QueryVersion,QuerySql) VALUES (@QueryId, @QueryVersion, @QuerySql )",
                new
                {
                    QueryId = guidPk,
                    QueryVersion = 0,
                    QuerySql = namedQuery.QuerySql
                }
            );

            return guidPk;
        }
        public string UpdateNamedQuery(NamedQuery namedQuery)
        {
            var guidPk = namedQuery.QueryPk;

            InfoDataConnection.Execute( "UPDATE AES.InfoQuery SET QuerySql = @QuerySql WHERE QueryId = @QueryId",
                new
                {
                    QueryId = namedQuery.QueryPk,
                    QuerySql = namedQuery.QuerySql
                }
            );

            InfoDataConnection.Execute("UPDATE AES.InfoQueryVersion SET QuerySql = @QuerySql WHERE QueryId = @QueryId",
                new
                {
                    QueryId = namedQuery.QueryPk,
                    QuerySql = namedQuery.QuerySql
                }
            );

            return guidPk;
        }

        public List<object> RunStoredQuery(string queryOverride = "")
        {
            string queryName = queryOverride != string.Empty ? queryOverride : requestedQueryName;

            var origReader = InfoDataConnection.ExecuteReader(sql: string.Format("SELECT TOP 1 * FROM AES.InfoQuery WHERE QueryName = '{0}'", queryName));
            var typedParser = origReader.GetRowParser<InfoQuery>();
            var queryData = origReader.Parse<InfoQuery>().FirstOrDefault();

            if (queryData == null)
            {
                throw new ApplicationException(string.Format("requested queryname ({0}) does not exist", requestedQueryName));
            }

            return RunSqlQuery(queryData.QuerySql, -1);
        }
        public List<object> RunTableQuery( string queryOrTableId, int topRows = 1, int startRow = 0)
        {
            var columnList = new List<string>() {"*"};
            var queryTables = new List<QueryTableDef>()
            {
                new QueryTableDef() {TableName = queryOrTableId, IsBaseTable = true, IncludeColumns = columnList}
            };
            var dapperSql = this.BuildSqlSelectString(queryTables, 0, topRows);
            return this.RunSqlQuery(dapperSql, topRows, startRow);
        }
        public List<object> RunSqlQuery(string sqlSelect, int topRows = 1, int startRow = 0)
        {
            if (topRows != -1)
            {
                sqlSelect = "SELECT TOP " + (topRows + 1).ToString() + " " + sqlSelect.Substring(7);
            }

            try
            {
                var origReader = AppDataConnection.ExecuteReader(sql: sqlSelect, commandTimeout: _commandTimeout);
                var typedParser = origReader.GetRowParser<object>();

                return origReader.Parse<object>().ToList();
            }
            catch (Exception)
            {
                throw; //todo: caller should handle this?
            }
        }

        public string BuildSqlSelectString( List<QueryTableDef> relations, int startRow = 0, int topRows = 1 )
        {
            var sb = new StringBuilder();
            char[] tableAlais = "BCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(); //todo: deal with limitation of 25
            var baseTable = relations.Where(x => x.IsBaseTable).FirstOrDefault();
            var baseTableColumns = new List<string>();
            foreach (var col in baseTable.IncludeColumns)
            {
                baseTableColumns.Add(string.Format("A." + col));
            }

            int i = 0;
            var allOtherColumns = new List<string>();
            foreach (var rel in relations.Where(x => !x.IsBaseTable))
            {
                foreach (var col in rel.IncludeColumns)
                {
                    allOtherColumns.Add(string.Format("{0}." + col, tableAlais[i]));
                }
                i++; // next table alais
            }

            i = 0; //reset
            //sb.Append(string.Format("SELECT top {0} {1} {2} {3} FROM {4} A ", 
            sb.Append(string.Format("SELECT {1} {2} {3} FROM {4} A ", 
                topRows, 
                string.Join(", ", baseTableColumns),
                (baseTableColumns.Count > 1 && allOtherColumns.Count > 0 ? ", " : ""),
                string.Join(", ", allOtherColumns),
                baseTable.TableName
            ));

            foreach (var joinTable in relations.Where(x => !x.IsBaseTable))
            {
                // a table may be included even if it has no relation to base table
                var joinType = baseTable.FkTableNames.Contains(joinTable.TableName) ? "INNER JOIN" : "FULL JOIN";
                var joinOn = joinType == "INNER JOIN" ? "A." + joinTable.PkColumnName : joinTable.JoinOn;

                sb.AppendLine( string.Format("{0} {2} {1} on {1}.{3} = {4}",
                    joinType,
                    tableAlais[i], 
                    joinTable.TableName, 
                    joinTable.PkColumnName,
                    joinOn) );
                i++;
            }

            if (!string.IsNullOrEmpty( baseTable.JoinOn ))
            {
                i = 0;
                foreach( var foos in baseTable.JoinOn.Split("|") )
                {
                    if ( i == 0 )
                    {
                        sb.AppendLine( "WHERE A." + foos );
                    }
                    else
                    {
                        sb.AppendLine( "AND A." + foos );
                    }

                    i++;
                }
            }

            return sb.ToString();
        }

        #region nested classes
        private class InfoQuery
        {
            public string QuerySql { get; set; }
        }
        #endregion
    }
}