
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
        List<object> GetAllQueries();
        string AddNamedQuery(NamedQuery namedQuery);
        string UpdateNamedQuery(NamedQuery namedQuery);
        List<object> RunNamedQuery(string queryOverride = "");
        List<object> RunTableQuery(string queryOrTableId, int topRows = 1, int startRow = 0);
        List<object> RunSqlQuery(string sqlSelect, int topRows = 1, int startRow = 0);
        string BuildSqlSelectString(List<QueryTableDef> relations, int startRow = 0, int topRows = 1);
        NamedQuery GetQuerySchema(string queryName);
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

        private string GetQuerySql(string queryName)
        {
            var origReader = InfoDataConnection.ExecuteReader(sql: string.Format("SELECT TOP 1 * FROM AES.InfoQuery WHERE QueryName = '{0}'", queryName));
            var typedParser = origReader.GetRowParser<InfoQuery>();
            var queryData = origReader.Parse<InfoQuery>().FirstOrDefault();

            return queryData.QuerySql;
        }
        private string MakeSelectColumn(string col, char colPrefix, bool makeUniqueName)
        {
            // This code is necessary so that serialized json data will have unique property names. This
            // becomes important when joing two tables that have one or more duplicate column names. Good table
            // naming standards would eliminate this need but... (i don't control your standards). So, each
            // column will be suffixed with its table alais.

            var name = string.Empty;
            if ( makeUniqueName )
            {
                // e.g. A.CustomerId AS CustomerId_A
                name = string.Format("{0}.{1} AS {1}_{0}", colPrefix, col);
            }
            else
            {
                // e.g. A.CustomerId
                name = string.Format("{0}.{1}", colPrefix, col);
            }

            return name;
        }
        private string MakeJoinConditions(string joinConditions, char tableAlais)
        {
            var result = string.Empty;
            var i = 0;
            foreach (var joinCondition in joinConditions.Split(new char[] {'|','&'}, StringSplitOptions.None))
            {
                var fixupCondition = joinCondition.Replace("*.", tableAlais + ".");

                if (i == 0)
                {
                    result = string.Format(fixupCondition);
                }
                else
                {
                    var io = joinConditions.IndexOf(joinCondition);
                    var lio1 = joinConditions.LastIndexOf("&", io);
                    var lio2 = joinConditions.LastIndexOf("|", io);
                    var oper = lio1 > lio2 ? "AND" : "OR";
                    result = result + string.Format(" {0} {1}", oper, fixupCondition);
                }
                i++;
            }
            return result;
        }
        #endregion

        public List<object> GetAllQueries()
        {
            var sqlSelect = "SELECT * FROM AES.InfoQuery";

            try
            {
                var origReader = InfoDataConnection.ExecuteReader(sql: sqlSelect);
                var typedParser = origReader.GetRowParser<object>();

                return origReader.Parse<object>().ToList();
            }
            catch (Exception)
            {
                throw; //todo: caller should handle this?
            }
        }
        public string AddNamedQuery(NamedQuery namedQuery)
        {
            var guidPk = Guid.NewGuid().ToString();

            InfoDataConnection.Execute("INSERT INTO AES.InfoQuery (QueryId,QueryName,QueryTableBase,QuerySql,QueryOwner,QueryRowsExpected) VALUES (@QueryId, @QueryName, @QueryTableBase, @QuerySql,@QueryOwner,@QueryRowsExpected)", 
                new {
                    QueryId = guidPk,
                    QueryName = namedQuery.QueryName.Trim(),
                    QueryTableBase = namedQuery.QueryTableBase.Trim(),
                    QuerySql = namedQuery.QuerySql,
                    QueryOwner = "AES",
                    QueryRowsExpected = namedQuery.RowsExpected
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

        public List<object> RunNamedQuery(string queryOverride = "")
        {
            string queryName = queryOverride != string.Empty ? queryOverride : requestedQueryName;

            var querySql = this.GetQuerySql(queryName);
            if (querySql == null)
            {
                throw new ApplicationException(string.Format("requested queryname ({0}) does not exist", requestedQueryName));
            }

            return RunSqlQuery(querySql, -1);
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

            var allTableColumns = new List<string>();
            foreach (var rel in relations)
            {
                allTableColumns.AddRange(rel.IncludeColumns);
            }

            var baseTable = relations.Where(x => x.IsBaseTable).FirstOrDefault();
            var baseTableColumns = new List<string>();
            foreach (var col in baseTable.IncludeColumns)
            {
                baseTableColumns.Add(MakeSelectColumn(col, 'A', false));
            }

            int i = 0;
            var allOtherColumns = new List<string>();
            foreach (var rel in relations.Where(x => !x.IsBaseTable))
            {
                foreach (var col in rel.IncludeColumns)
                {
                    // check for duplicate column name
                    var colCount = allTableColumns.Where(x => x.Equals(col)).Count();
                    allOtherColumns.Add(MakeSelectColumn(col, tableAlais[i], makeUniqueName: colCount > 1 ? true : false));
                }
                i++; // next table alais
            }

            i = 0; //reset 
            sb.Append(string.Format("SELECT {1} {2} {3} FROM {4} A ", 
                topRows, 
                string.Join(", ", baseTableColumns),
                (baseTableColumns.Count > 1 && allOtherColumns.Count > 0 ? ", " : ""),
                string.Join(", ", allOtherColumns),
                baseTable.TableName
            ));

            // included join table(s)
            foreach (var joinTable in relations.Where(x => !x.IsBaseTable))
            {
                var joinType = baseTable.FkTableNames.Contains(joinTable.TableName) ? "INNER JOIN" : "FULL JOIN";
                var joinWhere = string.Empty;

                if (joinType == "INNER JOIN")
                {
                    joinWhere = string.Join(" AND ", joinTable.PkColumnNames.Select(x => tableAlais[i] + "." + x + " = A." + x));
                }
                if (joinType == "FULL JOIN")
                {
                    joinWhere = "1 = 1"; // hopefully you specified JoinOn (below)
                }
                if (joinTable.JoinOn != string.Empty)
                {
                    joinWhere = joinWhere + " AND " +  MakeJoinConditions(joinTable.JoinOn, tableAlais[i]);
                }

                sb.AppendLine( string.Format("{0} {2} {1} on {3}",
                    joinType,
                    tableAlais[i], 
                    joinTable.TableName,
                    joinWhere) 
                );
                i++;
            }

            // optional query condition(s) on the primary table
            if (!string.IsNullOrEmpty(baseTable.JoinOn))
            {
                sb.Append(" WHERE " + MakeJoinConditions( baseTable.JoinOn, 'A') );
            }

            return sb.ToString();
        }

        public NamedQuery GetQuerySchema(string queryName)
        {
            var querySql = this.GetQuerySql(queryName);
            if (querySql == null)
            {
                throw new ApplicationException(string.Format("requested queryname ({0}) does not exist", queryName));
            }

            var result = new NamedQuery();

            var sqlSelectStatement = "SELECT TOP 0 " + querySql.Substring(7); // inject TOP 0
            var dr = AppDataConnection.ExecuteReader(sqlSelectStatement);
            var schemaTable = dr.GetSchemaTable();
            //For each field in the table...
            foreach (System.Data.DataRow dataRow in schemaTable.Rows)
            {
                //For each property of the field...
                foreach (System.Data.DataColumn dataColumn in schemaTable.Columns)
                {
                    if (dataColumn.ColumnName == "ColumnName")
                    {
                        result.QueryColumns.Add(dataRow[dataColumn].ToString());
                    }

                    if (dataColumn.ColumnName == "DataType")
                    {
                        //result.QueryColumns.Add(dataRow[dataColumn].ToString());
                    }
                }
            }

            return result;
        }

        #region nested classes
        private class InfoQuery
        {
            public InfoQuery()
            {

            }
            public string QuerySql { get; set; }
        }
        #endregion
    }
}
