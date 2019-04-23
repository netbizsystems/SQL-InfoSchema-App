
using AndersonEnterprise.SqlQueryService.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Data;

namespace AndersonEnterprise.SqlQueryService
{
    /// <summary>
    /// InfoQueryController - resource (abstract) for returning rows from an SQL query/select
    /// </summary>
    [Route("api/[controller]")]
    public abstract class InfoQueryController : Controller
    {
        public IRunTimeService RunTimeService { get; }

        /// <summary>
        /// QueryName - provided by implementation (descendent)
        /// </summary>
        public abstract string QueryName { get; }

        protected InfoQueryController(IRunTimeService runtimeService)
        {
            RunTimeService = runtimeService;
            Parameters = new List<QueryParam>();
        }

        /// <summary>
        /// Get - return row(s) for requested QueryName
        /// </summary>
        /// <param name="queryName"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual IActionResult Get([FromQuery]string queryName = "", int skip = 0, int take = -1)
        {
            // descendent class sets QueryName.. except for the FooBar controller
            if (!string.IsNullOrEmpty(QueryName)) queryName = QueryName;
            
            try
            {
                var requestedRows = RunTimeService.RunNamedQuery(queryName, skip, take, Parameters);
                return Ok(requestedRows);
            }
            catch (Exception)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, new { AppErrorId = "1", AppErrorText = "sorry dude" });
            }
        }

        public void AddSelectParameter(string queryStringItem, string defaultValue, string dbColumnName)
        {
            StringValues paramValue;
            if (!HttpContext.Request.Query.TryGetValue(queryStringItem, out paramValue))
            {
                paramValue = defaultValue;
            }

            if (!string.IsNullOrEmpty(paramValue))
            {
                var paramName = dbColumnName.Replace(".", "");
                Parameters.Add(new QueryParam() { ParamName = paramName, ParamValue = paramValue.ToString(), DbColumnName = dbColumnName });
            }
        }

        #region private/protected
        private IList<QueryParam> Parameters { get; set; }
        #endregion
    }
}