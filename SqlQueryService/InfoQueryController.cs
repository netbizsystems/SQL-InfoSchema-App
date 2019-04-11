
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace AndersonEnterprise.SqlQueryService
{
    /// <summary>
    /// InfoQueryController - resource (abstract) for returning rows from an SQL query/select
    /// </summary>
    [Route("api/[controller]")]
    public abstract class InfoQueryController : Controller
    {
        public InfoQueryService InfoQueryService { get; }

        /// <summary>
        /// QueryName - provided by implementation (descendent)
        /// </summary>
        public abstract string QueryName { get; }


        protected InfoQueryController(IConfiguration config)
        {
            InfoQueryService = new InfoQueryService(config, QueryName);
        }

        [HttpGet]
        public virtual IActionResult Get([FromQuery]string queryName = "")
        {
            // descendent class sets QueryName.. except for the FooBar controller
            if (!string.IsNullOrEmpty(QueryName)) queryName = QueryName;
            
            try
            {
                var requestedRows = InfoQueryService.RunNamedQuery(queryName);
                return Ok(requestedRows);
            }
            catch (Exception)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest, new { AppErrorId = "1", AppErrorText = "sorry dude" });
            }
        }

        #region private/protected
        #endregion
    }
}