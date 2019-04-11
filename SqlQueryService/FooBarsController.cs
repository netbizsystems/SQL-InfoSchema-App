
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AndersonEnterprise.SqlQueryService
{
    /// <summary>
    /// FooBars - generalized QueryName data resolver
    /// </summary>
    [Route("api/[controller]")] //api/foobars
    public class FooBarsController : InfoQueryController
    {
        public FooBarsController(IConfiguration config) : base(config) { }

        /// <summary>
        /// QueryName - empty tells base-class to use the querystring
        /// </summary>
        public override string QueryName => string.Empty;

        /// <summary>
        /// override as/if desired
        /// </summary>
        /// <param name="queryName"></param>
        /// <returns></returns>
        public override IActionResult Get([FromQuery]string queryName = "")
        {
            var result = base.Get(queryName);

            return result;
        }

    }
}