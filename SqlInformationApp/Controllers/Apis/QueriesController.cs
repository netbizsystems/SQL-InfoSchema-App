
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace AndersonEnterprise.SqlInformationApp.Controllers.Apis
{
    using AndersonEnterprise.SqlQueryService;
    using AndersonEnterprise.SqlQueryService.Models;

    /// <summary>
    /// InfoQueryController - RPC for interaction with info meta-data
    /// </summary>
    [Route("api/[controller]")]
    public class QueriesController : Controller
    {
        public IInfoQueryService InfoQueryService { get; }

        public QueriesController( IInfoQueryService iqs)
        {
            InfoQueryService = iqs;
        }

        [HttpGet] [Route("{queryId}")] 
        public IActionResult Get( string queryId, bool isPrimary)
        {
            //todo: this query only wants one single row.. so ask for 0??
            return Ok(InfoQueryService.RunTableQuery(queryOrTableId: queryId, topRows: 0));
        }
        [HttpGet]
        public IActionResult Get()
        {
            var result = InfoQueryService.GetAllQueries();

            return Ok(result);
        }

        [HttpPost("InsertNamedQuery/")]
        public IActionResult InsertNamedQuery([FromBody]NamedQuery namedQuery)
        {
            if (namedQuery == null) return BadRequest("required data not received by api/queryrunner/InsertNamedQuery");

            try
            {
                var result = InfoQueryService.AddNamedQuery(namedQuery);
                return Json(new { result });
            }
            catch (Exception e)
            {
                var errorIdAndText = ErrorFeedback.QueryNameInUse.Split("/");
                return StatusCode((int)HttpStatusCode.BadRequest, new { AppErrorId = errorIdAndText[0], AppErrorText = errorIdAndText[1] });
            }
        }

        [HttpPut("UpdateNamedQuery/")]
        public IActionResult UpdateNamedQuery([FromBody]NamedQuery namedQuery)
        {
            if (namedQuery == null) return BadRequest("required data not received by api/queryrunner/UpdateNamedQuery");

            try
            {
                var result = InfoQueryService.UpdateNamedQuery(namedQuery);
                return Json(new { result });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { AppErrorId = 1000, AppErrorText = "update failed because of foo, bar, baz!" });
            }
        }

        [HttpPut("RunSqlQuery/")]
        public IActionResult RunSqlQuery([FromBody]SqlQuery sqlQuery)
        {
            if (sqlQuery == null) return BadRequest("required data not received by api/queryrunner/RunSqlQuery");

            try
            {
                return Json(new { queryResult = InfoQueryService.RunSqlQuery(sqlQuery.SqlQueryStatement, sqlQuery.RowsExpectedMax) });
            }
            catch (Exception e)
            {
                var errorIdAndText = ErrorFeedback.QueryTimeout.Split("/");
                return StatusCode((int)HttpStatusCode.BadRequest, new { AppErrorId = errorIdAndText[0], AppErrorText = errorIdAndText[1] });
            }
        }

        //[HttpPut("RunNamedQuery/")]
        //public IActionResult RunNamedQuery([FromBody]NamedQuery namedQuery)
        //{
        //    if (namedQuery == null) return BadRequest("required data not received by api/queries/RunNamedQuery");

        //    try
        //    {
        //        return Json(new { queryResult = InfoQueryService.RunNamedQuery(namedQuery.QueryName) } );
        //    }
        //    catch (Exception e)
        //    {
        //        var errorIdAndText = ErrorFeedback.QueryTimeout.Split("/");
        //        return StatusCode((int)HttpStatusCode.BadRequest, new { AppErrorId = errorIdAndText[0], AppErrorText = errorIdAndText[1] });
        //    }
        //}

        [HttpPost("MakeSqlQueryString/")]
        public IActionResult MakeSqlQueryString([FromBody]List<QueryTableDef> queryJoins)
        {
            var sql = InfoQueryService.BuildSqlSelectString( relations: queryJoins );

            return new OkObjectResult(new {fullSql = sql });
        }

        [HttpGet("GetQuerySchema/")] [Route("{namedQuery}")]
        public IActionResult GetQuerySchema( string namedQuery )
        {
            if ( namedQuery == null ) return BadRequest( "required data not received by api/queries/GetQuerySchema" );

            var result = InfoQueryService.GetQuerySchema( namedQuery );

            return new OkObjectResult(new { foo = result  });
        }

        #region private/protected
        static class ErrorFeedback
        {
            public const string QueryTimeout = "1000/query may have time out... try again";
            public const string QueryNameInUse = "1001/this queryname is already being used";
        }
        #endregion
    }
}