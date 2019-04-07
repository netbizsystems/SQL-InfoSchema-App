
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AndersonEnterprise.SqlInformationApp.Controllers.Apis
{
    /// <summary>
    /// FooBars - resource (restful) for interaction against a new query in the SqlInformationApp UI
    /// </summary>
    [Route("api/[controller]")] //api/foobars
    public class FooBarsController : AndersonEnterprise.SqlQueryService.InfoQueryController
    {
        public FooBarsController(IConfiguration config) : base(config) { }

        /// <summary>
        /// QueryName - empty tells base-class to use the querystring
        /// </summary>
        public override string QueryName => string.Empty; // controller get name from GET querystring
    }
}